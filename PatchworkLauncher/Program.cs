using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mono.Cecil;
using Patchwork.Attributes;
using Patchwork.Utility;

namespace PatchworkLauncher
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new guiHome());
		}

		public static LauncherState State {
			get;
			set;
		}

		static string GetBackupFileName(string original, bool forOriginal) {
			var ext = Path.GetExtension(original);
			return Path.ChangeExtension(original, !forOriginal ? $"{ext}.mod" : $"{ext}.orig");
		}

		static bool StartupGameFilesCheck(out List<string> messages) {
			var lastConfig = State.SavedSettings.LastExecution;
			var filesToCheck =
				from entry in lastConfig.Entries
				where entry.Enabled
				group entry by entry.Target;
			messages = new List<string>();
			var replaceFiles = State.SavedSettings.ReplaceFiles;
			filesToCheck = filesToCheck.Distinct();
			foreach (var fileMods in filesToCheck) {
				var assembly = AssemblyDefinition.ReadAssembly(fileMods.Key);
				State.Targets[fileMods.Key] = assembly;
				var executionListSeq =
					from attr in assembly.GetCustomAttributes<PatchedByAssemblyAttribute>()
					orderby attr.Index
					select attr.PatchAssembly;
				var exList = executionListSeq.ToList();
				if (!replaceFiles) {
					if (exList.Count > 0) {
						messages.Add($"File {fileMods.Key} was supposed to be unpatched, but it was patched.");
					}
				} else {
					bool failed = fileMods.Select(x => x.Path).SequenceEqual(exList);
					if (!failed) {
						messages.Add($"File {fileMods.Key}'s mod list didn't match the expected list.");	
					}
				}
			}
			return messages.Count == 0;
		}

		private static void RestoreFile(string path, bool toOriginal) {
			var originalFileName = GetBackupFileName(path, toOriginal);
			File.Copy(path, GetBackupFileName(path, !toOriginal));
			File.Move(originalFileName, path);
		}

		static void RestoreFiles() {
			foreach (var file in State.Targets.ToList()) {
				var isPatched = file.Value.HasCustomAttribute<PatchedByAssemblyAttribute>();
				if (!isPatched && !State.SavedSettings.ReplaceFiles) {
					continue;
				}
				if (isPatched) {
					RestoreFile(file.Key, true);
					State.Targets[file.Key] = AssemblyDefinition.ReadAssembly(file.Key);
				}
				//add code to patch original files if ReplaceFiles is true.
			}
		}
	}
}
