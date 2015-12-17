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

		private const bool _copyToModFolder = false;

		private readonly OpenFileDialog _openModDialog = new OpenFileDialog() {
			Filter = "Patchwork Mod Files (*.pw.dll)|*.pw.dll|DLL files (*.dll)|*.dll|All files (*.*)|*.*",
			CheckFileExists = true,
			CheckPathExists = true,
			Title = "Select Patchwork mod file",
			Multiselect = false,
			FilterIndex = 0,
			SupportMultiDottedExtensions = true,
			AutoUpgradeEnabled = true,
			InitialDirectory = PathHelper.GetAbsolutePath(""),
			RestoreDirectory = false,
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
		private const string _pathHistoryXml = "./history.pw.xml";
		private const string _pathSettings = "./instructions.pw.xml";
		private const string _pathGameInfoAssembly = "./app.dll";
		private const string _pathLogFile = "./log.txt";
		private static readonly XmlSerializer _historySerializer = new XmlSerializer(typeof(XmlHistory));
		private static readonly XmlSerializer _instructionSerializer = new XmlSerializer(typeof (XmlSettings));
	

		public async Task<XmlHistory> Command_Patch() {
			var progObj = new ProgressObject();
			try {
				using (var logForm = new LogForm(progObj)) {
					logForm.Show();
					State.Value = LaunchManagerState.IsPatching;
					XmlHistory history = null;
					try {
						history = await Task.Run(() => ApplyInstructions(progObj));
					}
					catch (Exception ex) {
						Command_Display_Error("Patching the game", ex: ex);
						State.Value = LaunchManagerState.Idle;
						return new XmlHistory();
					}
					finally {
						if (DebugOptions.Default.OpenLogAfterPatch) {
							Process.Start(_pathLogFile);
						}
					}
					_historySerializer.Serialize(history, _pathHistoryXml);
					logForm.Close();
					return history;
				}
			}
			finally {
				State.Value = LaunchManagerState.Idle;
			}
		}

		public async void Command_Launch_Modded() {
			Action<IBindable<LaunchManagerState>> p = null;
			var history = await Command_Patch();
			p = v => {
				if (v.Value == LaunchManagerState.Idle) {
					PatchingHelper.RestorePatchedFiles(AppInfo, history.Files);
					State.HasChanged -= p;
				}
			};
			State.HasChanged += p;
			Command_Launch();
		}

		public void Command_ChangeFolder() {
			if (Command_SetGameFolder_Dialog()) {
				Command_ExitApplication();
			}
		}

		private bool Command_SetGameFolder_Dialog() {
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
			if (DebugOptions.Default.DontRunProgram) {
				State.Value = LaunchManagerState.Idle;
				return;
			}
			var process = new Process() {
				StartInfo = {
					FileName = AppInfo.Executable.FullName,
					WorkingDirectory = Path.GetDirectoryName(AppInfo.Executable.FullName)
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
			
			bool hadToCopy = false;
			PatchingManifest manifest = null;
			try {
				Directory.CreateDirectory(_modFolder);
				var folder = Path.GetDirectoryName(path);
				var absoluteFolder = PathHelper.GetAbsolutePath(folder);
				var modsPath = PathHelper.GetAbsolutePath(_modFolder);
				
				if (!DebugOptions.Default.DontCopyFiles && !modsPath.Equals(absoluteFolder, StringComparison.InvariantCultureIgnoreCase)) {
					targetPath = Path.Combine(_modFolder, fileName);
					File.Copy(path, targetPath, true);
					hadToCopy = true;
				}
				manifest = ManifestMaker.CreateManifest(PathHelper.GetAbsolutePath(targetPath));
				if (manifest.PatchInfo == null) {
					throw new PatchDeclerationException("The patch did not have a PatchInfo class.");
				}
				var patchInstruction = new PatchInstruction() {
					IsEnabled = isEnabled,
					Patch = manifest,
					PatchLocation = PathHelper.GetRelativePath(targetPath),
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
			//TODO: Better error dialog
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
			if (ex != null) {
				throw new Exception("", ex);
			}
			return MessageBox.Show(errorString, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private static AppInfoFactory LoadAppInfoFactory() {
			Assembly gameInfoAssembly = null;
			if (!File.Exists(PathHelper.GetAbsolutePath(_pathGameInfoAssembly))) {
				throw new FileNotFoundException($"The AppInfo assembly file was not found.",_pathGameInfoAssembly);
			}
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
			//TODO: Refactor this into a constructor?
			try {
				if (File.Exists(_pathLogFile)) {
					File.Delete(_pathLogFile);
				}
				Logger =
					new LoggerConfiguration().WriteTo.File(_pathLogFile, LogEventLevel.Debug).MinimumLevel.Debug().CreateLogger();
				var gameInfoFactory = LoadAppInfoFactory();
			
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

		public async void Command_TestRun() {
			DebugOptions.Default.OpenLogAfterPatch = true;
			var history = await Command_Patch();
			PatchingHelper.RestorePatchedFiles(AppInfo, history.Files);

		}

		private XmlHistory ApplyInstructions(ProgressObject po) {
			//TODO: Use a different progress tracking system and make the entire patching operation more recoverable and fault-tolerant.
			//TODO: Refactor this method.

			var seq = Instructions;
			var appInfo = AppInfo;
			var logger = Logger;
			var byFile =
				from file in seq
				where file.IsEnabled
				let target = file.Patch.PatchInfo.GetTargetFile(appInfo)
				group file by target.FullName;
			byFile = byFile.ToList();
			var fileProgress = new ProgressObject();
			po.Child.Value = fileProgress;
			var patchProgress = new ProgressObject();
			fileProgress.Child.Value = patchProgress;
			var myAttributesAssembly = typeof (Patchwork.Attributes.AppInfo).Assembly;
			var attributesAssemblyName = Path.GetFileName(myAttributesAssembly.Location);
			var history = new List<XmlFileHistory>();
			po.TaskTitle.Value = "Patching Game";
			po.TaskText.Value = appInfo.AppName;
			po.Total.Value = byFile.Count();
			
			
			
			foreach (var patchesForFile in byFile) {
				var patchCount = patchesForFile.Count();
				po.TaskTitle.Value = $"Patching {appInfo.AppName}";
				po.TaskText.Value = Path.GetFileName(patchesForFile.Key);
				var dir = Path.GetDirectoryName (patchesForFile.Key);

				var localAssemblyName = Path.Combine (dir, attributesAssemblyName);
				var copy = true;
				fileProgress.TaskTitle.Value = "Patching File";
				fileProgress.TaskText.Value = "Copying Attributes Assembly";
				fileProgress.Total.Value = 2 + patchCount;
				if (File.Exists(localAssemblyName)) {
					try {
						var localAssembly = AssemblyCache.Default.ReadAssembly(localAssemblyName);
						if (localAssembly.GetAssemblyMetadataString() == myAttributesAssembly.GetAssemblyMetadataString()) {
							copy = false;
						}
					}
					catch {
						//if reading the assembly failed for any reason, just ignore...
					}
				}
				if (copy) {
					File.Copy(myAttributesAssembly.Location, localAssemblyName, true);
				}
				fileProgress.Current.Value++;
				
				var backupModified = PatchingHelper.GetBackupForModified(patchesForFile.Key);
				var backupOrig = PatchingHelper.GetBackupForOriginal(patchesForFile.Key);
				fileProgress.TaskText.Value = "Applying Patch";
				
				if (!PatchingHelper.DoesFileMatchPatchList(backupModified, patchesForFile.Key, patchesForFile) || DebugOptions.Default.AlwaysPatch) {
					var patcher = new AssemblyPatcher(patchesForFile.Key, logger) {
						EmbedHistory = true
					};
					
					foreach (var patch in patchesForFile) {
						try {
							patcher.PatchManifest(patch.Patch, patchProgress);
						}
						catch (PatchException ex) {
							Command_Display_Error("Patch a game file", patch.Name, ex);
							throw;
						}
						fileProgress.Current.Value++;
					}
					patchProgress.TaskText.Value = "";
					patchProgress.TaskTitle.Value = "";
					
					if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
						fileProgress.TaskText.Value = "Running PEVerify...";
						try {
							logger.Information(patcher.RunPeVerify(Path.GetDirectoryName(patchesForFile.Key),appInfo.IgnorePEVerifyErrors));
						}
						catch (Exception ex) {
							logger.Error(ex, "Failed to run PEVerify on the assembly.");
						}
					}
					fileProgress.Current.Value++;
					fileProgress.TaskText.Value = "Writing Assembly";
					patcher.WriteTo(backupModified);
				} else {
					fileProgress.Current.Value += patchCount;
				}
				
				PatchingHelper.SwitchFilesSafely(backupModified, patchesForFile.Key, backupOrig);
				history.Add(new XmlFileHistory() {
					TargetPath = patchesForFile.Key,
					PatchHistory = patchesForFile.Select(patch => new XmlPatchHistory(patch.PatchLocation)).ToList(),
				});
				AssemblyCache.Default.Clear();
				po.Current.Value++;
			}
			return new XmlHistory() {
				Files = history
			};
		}
	}
}