using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Mono.Cecil;
using Patchwork;
using Patchwork.Attributes;
using Patchwork.Utility;
using Patchwork.Utility.Binding;
using Serilog;
using Serilog.Events;

namespace PatchworkLauncher {

	public enum LaunchManagerState {
		GameRunning,
		IsPatching,
		Idle
	}

	public  class LaunchManager {

		private  guiHome _home;

		private  guiMods _mods;

		private NotifyIcon _icon;

		public ILogger Logger {
			get;
			set;
		}

		public DisposingBindingList<PatchInstruction> Instructions {
			get;
			private set;
		}

	    public static readonly LaunchManager Instance = new LaunchManager();

		public  guiMods Command_OpenMods() {
			if (_mods == null) {
				_mods = new guiMods(this);
				_mods.Closing += (sender, args) => {
					if (_home?.Visible == true) {
						args.Cancel = true;
						_mods.Hide();
					}
				};
			}
			_mods.ShowOrFocus();
			return _mods;
		}

	    public  AppInfo AppInfo { get; set; }

	    private ManifestCreator ManifestMaker {
			get;
		} = new ManifestCreator();

		public string BaseFolder {
			get;
			set;
		}

		private const string _modFolder = "Mods";

		private readonly OpenFileDialog _openModDialog = new OpenFileDialog() {
			Filter = "Patchwork Mod Files (*.pw.dll)|*.pw.dll|DLL files (*.dll)|*.dll|All files (*.*)|*.*",
			CheckFileExists = true,
			CheckPathExists = true,
			Title = "Select Patchwork mod file",
			Multiselect = false,
			FilterIndex = 0,
			SupportMultiDottedExtensions = true,
			AutoUpgradeEnabled = true,
			InitialDirectory = PathHelper.GetAbsolutePath("")
		};

		public int Command_MovePatch(int index, int offset) {
			
			if (index < 0 || index >= Instructions.Count) {
				throw new ArgumentException("Specified instruction was not in the sequence.");
			}
			var instruction = Instructions[index];
			var newIndex = index + offset;
			if (newIndex < 0 || newIndex >= Instructions.Count) {
				return index;
			}
			var oldOccupant = Instructions[newIndex];
			Instructions[index] = oldOccupant;
			Instructions[newIndex] = instruction;
			return newIndex;
		}

		public  void Command_Dialog_AddPatch(IWin32Window owner) {
			var result = _openModDialog.ShowDialog(owner);
			if (result == DialogResult.Cancel) {
				return;
			}
			var fileName = _openModDialog.FileName;
			var fileNameOnly = Path.GetFileName(fileName);
			var collision =
				Instructions.SingleOrDefault(instr => Path.GetFileName(instr.PatchLocation).EqualsIgnoreCase(fileNameOnly));

			if (collision != null) {
				Command_Display_Error("Load a patch", fileNameOnly, message: "You already have a patch with this filename.");
				return;
			}
			try {
				Command_Direct_AddPatch(fileName, true);
			}
			catch (Exception ex) {
				Command_Display_Error("Load a patch", PathHelper.GetUserFriendlyPath(fileName), ex, "");
			}
		}

		public IBindable<LaunchManagerState> State { get; } = Bindable.Variable(LaunchManagerState.Idle);
		private const string _pathHistoryXml = "history.pw.xml";
		private const string _pathSettings = "instructions.pw.xml";
		private const string _pathGameInfoAssembly = "app.dll";
		private const string _pathLogFile = "log.txt";
		private static readonly XmlSerializer _historySerializer = new XmlSerializer(typeof(XmlHistory));
		private static readonly XmlSerializer _instructionSerializer = new XmlSerializer(typeof (XmlSettings));

		public async void Command_Launch_Modded() {
			XmlHistory history;
			var progObj = new ProgressObject();
			using (var logForm = new LogForm(progObj) {
				
			}) {
				logForm.Show();
				State.Value = LaunchManagerState.IsPatching;
				history = await Task.Run(() => PatchingHelper.ApplyInstructions(AppInfo, Instructions, Logger, progObj, false));
				_historySerializer.Serialize(history, _pathHistoryXml);
				logForm.Close();
			}
			Action<IBindable<LaunchManagerState>> p = null;
			p = v => {
				if (v.Value == LaunchManagerState.Idle) {
					PatchingHelper.RestorePatchedFiles(AppInfo, history.Files);
					State.HasChanged -= p;
				}
			};
			State.HasChanged += p;
			if (false) {
				State.Value = LaunchManagerState.Idle;
			} else {
				Command_Launch();
			}
		}

		public bool Command_SetGameFolder_Dialog() {
			bool wasHomeDisabled = false;
			try {
				using (var input = new guiInputGameFolder()) {
					if (_home?.Visible == true) {
						_home.Enabled = false;
						wasHomeDisabled = true;
					}
					var result = input.ShowDialog();
					if (result == DialogResult.OK) {
						BaseFolder = input.Folder.Value;
						return true;
					}
					return false;
				}
			}
			finally {
				if (wasHomeDisabled) {
					_home.Enabled = true;
				}
			}
		}

		public void Command_Launch() {
			var process = new Process() {
				StartInfo = {
					FileName = AppInfo.Executable.FullName
				},
				EnableRaisingEvents = true
			};
			process.Exited += delegate {
				State.Value = LaunchManagerState.Idle;
			};
			
			State.HasChanged += delegate {
				_home.Invoke((Action)(() => {
					if (State.Value == LaunchManagerState.GameRunning) {
						_home.Hide();
						_icon.Visible = true;
						_icon.ShowBalloonTip(1000, "Launching", "Launching the application. The launcher will remain in the tray for cleanup.",
							ToolTipIcon.Info);
					} else {
						Command_ExitApplication();
					}
				}));
			};
			State.Value = process.Start() ? LaunchManagerState.GameRunning : LaunchManagerState.Idle;
		}

		public Bitmap ProgramIcon {
			get;
			private set;
		}

		public PatchInstruction Command_Direct_AddPatch(string path, bool isEnabled) {
			var targetPath = path;
			var fileName = Path.GetFileName(path);
			targetPath = Path.Combine(_modFolder, fileName);
			bool hadToCopy = false;
			PatchingManifest manifest = null;
			try {
				Directory.CreateDirectory(_modFolder);
				var folder = Path.GetDirectoryName(path);
				var absoluteFolder = PathHelper.GetAbsolutePath(folder);
				var modsPath = PathHelper.GetAbsolutePath(_modFolder);
				
				if (!modsPath.Equals(absoluteFolder, StringComparison.InvariantCultureIgnoreCase)) {
					File.Copy(path, targetPath, true);
					hadToCopy = true;
				}
				manifest = ManifestMaker.CreateManifest(targetPath);
				if (manifest.PatchInfo == null) {
					throw new PatchDeclerationException("The patch did not have a PatchInfo class.");
				}
				var patchInstruction = new PatchInstruction() {
					IsEnabled = isEnabled,
					Patch = manifest,
					PatchLocation = targetPath,
					AppInfo = AppInfo,
					PatchOriginalLocation = path
				};
				Instructions.Add(patchInstruction);
				return patchInstruction;
			}
			catch (Exception ex) {
				Logger.Error(ex, $"The patch located in {path} could not be loaded.");
				manifest?.Dispose();
				if (hadToCopy) {
					File.Delete(targetPath);
				}
				throw;
			}
		}

		public void Command_Restore() {

			_home.Visible = true;
			_icon.Visible = false;
		}

		private DialogResult Command_Display_Error(string tryingToDoWhat, string objectsThatFailed = null, Exception ex = null, string message = null) {
			var errorType = "";
			if (ex is PatchException) {
				errorType = "A patch was invalid, incompatible, or caused an error.";
			}  else if (ex is IOException) {
				errorType = "Related to reading/writing files.";
			} else if (ex is ApplicationException) {
				errorType = "An application error.";
			} else if (ex != null) {
				errorType = "A system error or some sort of bug.";
			}
			string errorString = "An error has occurred,\r\n";
			errorString += tryingToDoWhat.IsNullOrWhitespace() ? "" : $"While trying to: {tryingToDoWhat}\r\n";
			errorString += errorType.IsNullOrWhitespace() ? "" : $"Error type: {errorType} ({ex?.GetType().Name})\r\n";
			errorString += ex == null ? "" : $"Internal message: {ex.Message}\r\n";
			errorString += objectsThatFailed.IsNullOrWhitespace() ? "" : $"Object(s) that failed: {objectsThatFailed}\r\n";
			errorString += message.IsNullOrWhitespace() ? "" : $"{message}\r\n";
			Logger.Error(ex, errorString);
			return MessageBox.Show(errorString, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private static AppInfoFactory LoadAppInfoFactory() {
			Assembly gameInfoAssembly = null;
			gameInfoAssembly = Assembly.LoadFile(PathHelper.GetAbsolutePath(_pathGameInfoAssembly));

			var gameInfoFactories =
				from type in gameInfoAssembly.GetTypes()
				where type.GetCustomAttribute<AppInfoFactoryAttribute>() != null
				select type;

			var factories = gameInfoFactories.ToList();

			if (factories.Count == 0) {
				throw new ArgumentException($"The AppInfo assembly did not have a class decorated with {nameof(AppInfoFactoryAttribute)}");
			}
			if (factories.Count > 1) {
				throw new ArgumentException(
					$"The AppInfo assembly had more than one class decorated with {nameof(AppInfoFactoryAttribute)}");
			}

			var gameInfoFactoryType = factories[0];
			if (!typeof (AppInfoFactory).IsAssignableFrom(gameInfoFactoryType)) {
				throw new ArgumentException($"The AppInfoFactory class does not inherit from {nameof(AppInfoFactory)}");
			}
			var ctor = gameInfoFactoryType.GetConstructorEx(CommonBindingFlags.Everything, new Type[] {
			});
			if (ctor == null) {
				throw new ArgumentException($"The AppInfo class did not have a default constructor.");
			}
			var gameInfoFactory = (AppInfoFactory)ctor.Invoke(null);
			return gameInfoFactory;
		}

		public guiHome Command_Start() {
			try {
				var gameInfoFactory = LoadAppInfoFactory();
				File.Delete(_pathLogFile);
				Logger =
					new LoggerConfiguration().WriteTo.File(_pathLogFile, LogEventLevel.Debug).MinimumLevel.Debug().CreateLogger();
				XmlSettings settings = new XmlSettings();
				XmlHistory history = new XmlHistory();;
				try {
					history = _historySerializer.Deserialize(_pathHistoryXml, new XmlHistory());
				}
				catch (Exception ex) {
					Command_Display_Error("Load patching history", _pathHistoryXml, ex,
						"If the launcher was terminated unexpectedly last time, it may not be able to recover.");
				}

				try {
					settings = _instructionSerializer.Deserialize(_pathSettings, new XmlSettings());
				}
				catch (Exception ex) {
					Command_Display_Error("Read settings file", _pathSettings, ex, "Patch list and other settings will be reset.");
				}
				
				if (settings.BaseFolder.IsNullOrWhitespace()) {
					if (!Command_SetGameFolder_Dialog()) {
						Command_ExitApplication();
					}
				} else {
					BaseFolder = settings.BaseFolder;
				}
				this.AppInfo = gameInfoFactory.CreateInfo(new DirectoryInfo(BaseFolder));
				var icon = Icon.ExtractAssociatedIcon(AppInfo.Executable.FullName) ?? _home.Icon;
				ProgramIcon = icon.ToBitmap();
				Instructions = new DisposingBindingList<PatchInstruction>();
				var instructions = new List<XmlInstruction>();
				foreach (var xmlPatch in settings.Instructions) {
					try {
						Command_Direct_AddPatch(xmlPatch.PatchPath, xmlPatch.IsEnabled);
					}
					catch {
						instructions.Add(xmlPatch);
					}
				}
				var patchList = instructions.Select(x => x.PatchPath).Join(Environment.NewLine);
				if (patchList.Length > 0) {
					Command_Display_Error("Load patches on startup.", patchList);
				}

				PatchingHelper.RestorePatchedFiles(AppInfo, history.Files);
				//File.Delete(_pathHistoryXml);
				_home = new guiHome(this);
				_home.Closed += (sender, args) => Command_ExitApplication();
				_home.ShowOrFocus();
				_icon = new NotifyIcon() {
					Icon = _home.Icon,
					Visible = false,
					Text = "Patchwork Launcher",
					ContextMenu = new ContextMenu() {
						MenuItems = {
							new MenuItem("Quit", (o, e) => Command_ExitApplication())
						}
					}
				};
				return _home;
			}
			catch (Exception ex) {
				Command_Display_Error("Launch the application", ex: ex, message: "The application will now exit.");
				Command_ExitApplication();
				return null;
			}
		}

		public void Command_ExitApplication() {
			_icon?.Dispose();
			var xmlInstructions = XmlSettings.FromInstructionSeq(Instructions);
			xmlInstructions.BaseFolder = BaseFolder;
			_instructionSerializer?.Serialize(xmlInstructions, _pathSettings);
			if (Application.MessageLoop) {
				Application.Exit();
			} else {
				Environment.Exit(0);
			}
		}

	}
}