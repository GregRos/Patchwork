using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Patchwork;
using Patchwork.Attributes;
using Patchwork.Utility;
using Serilog;

namespace PatchworkLauncher {

	public class PatchingHelper {

		private static string GetBackupForOriginal(string path) {
			return PathHelper.ChangeExtension(path, ext => $"{ext}.pw.original");
		}

		private static string GetBackupForModified(string path) {
			return PathHelper.ChangeExtension(path, ext => $"{ext}.pw.modified");
		}

		private static string GetNonCollidingBackupName(string path, int timesToTry = 10) {
			var rnd = new Random();
			
			for (int i = 0; i < timesToTry; i++) {
				var nextIndex = rnd.Next(0, timesToTry * 100).ToString();
				var tryName = PathHelper.ChangeExtension(path, ext => $"{ext}.pw.bak{nextIndex}");
				if (!File.Exists(tryName)) {
					return tryName;
				}
			}
			throw new Exception($"Something is really really wrong here. Tried {timesToTry} file names and all of them are taken...");
		}
		
		private static bool SwitchFilesSafely(string takeFileFromPath, string putItHere, string putExistingIn) {
			if (!File.Exists(takeFileFromPath)) {
				return false;
			}
			var bakNameForExistingTargetFile = GetNonCollidingBackupName(putExistingIn);
			var putExistingIn_exists = File.Exists(putExistingIn);
			int stage = 0;
			var putItHere_exists = File.Exists(putItHere);
			try {
				//STAGE 0
				if (putItHere_exists) {
					if (putExistingIn_exists) {
						File.Move(putExistingIn, bakNameForExistingTargetFile);
					}
					stage++;
					//STAGE 1
					File.Move(putItHere, putExistingIn);
				} else {
					stage++;
				}
				stage++; //STAGE 2
				File.Move(takeFileFromPath, putItHere);
			}
			catch (Exception ex) {
				try {
					//roll back all the changes to the file system.
					//recovery is in the reverse order...
					if (stage > 1) {
						if (putItHere_exists) {
							File.Move(putExistingIn, putItHere);
						}
					}

					if (stage > 0) {
						if (putItHere_exists && putExistingIn_exists) {
							File.Move(bakNameForExistingTargetFile, putExistingIn);
						}
					}
				}
				catch (Exception innerEx) {
					throw new AggregateException("Encountered exception [1] while trying to recover from exception [0]. Disk in unknown state.",ex, innerEx);
				}
				throw;
			}
			try {
				if (File.Exists(bakNameForExistingTargetFile)) {
					File.Delete(bakNameForExistingTargetFile);
				}
			}
			catch (Exception ex) {
				throw new Exception("Caught exception while trying to delete temporary backup file. Weird.", ex);
			}
			return true;
		}

		public static void RestorePatchedFiles(AppInfo appInfo, IEnumerable<XmlFileHistory> seq) {
			foreach (var file in seq) {
				RestorePatchedFile(file.TargetPath);
			}
		}

		public static XmlHistory ApplyInstructions(
			AppInfo appInfo, IEnumerable<PatchInstruction> seq, ILogger logger, ProgressObject po, bool alwaysPatch) {
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
				var localAssemblyName = Path.Combine(patchesForFile.Key, "..", attributesAssemblyName);
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
				
				var backupModified = GetBackupForModified(patchesForFile.Key);
				var backupOrig = GetBackupForOriginal(patchesForFile.Key);
				fileProgress.TaskText.Value = "Applying Patch";
				
				if (!DoesFileMatchPatchList(backupModified, patchesForFile.Key, patchesForFile) || alwaysPatch) {
					var patcher = new AssemblyPatcher(patchesForFile.Key, logger) {
						EmbedHistory = true
					};
					
					foreach (var patch in patchesForFile) {
						patcher.PatchManifest(patch.Patch, patchProgress);
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
				
				SwitchFilesSafely(backupModified, patchesForFile.Key, backupOrig);
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

		public static void RestorePatchedFile(string file) {
			var backupModified = GetBackupForModified(file);
			var backupOrig = GetBackupForOriginal(file);
			if (!IsFilePatched(file)) {
				return;
			}
			if (!File.Exists(backupOrig)) {
				throw new Exception(
					"The existing file was found to be modified, but a backup of the original could not be found. This means the disk is in an unknown state.");
			}
			SwitchFilesSafely(backupOrig, file, backupModified);
		}

		public static bool IsFilePatched(string file) {
			var assembly = AssemblyCache.Default.ReadAssembly(file);
			return assembly.HasCustomAttribute<PatchedByAssemblyAttribute>();
		}

		private static IEnumerable<PatchApplicationMetadata> ReadPatchedFileMetadata(string file) {
			var assembly = AssemblyCache.Default.ReadAssembly(file);
			var history =
				from attr in assembly.GetCustomAttributes<PatchedByAssemblyAttribute>()
				orderby attr.Index
				select attr.ToPatchApplicationMetadata();

			return history;
		}

		public static bool DoesFileMatchPatchList(string file, string targetFile, IEnumerable<PatchInstruction> patches) {
			List<PatchApplicationMetadata> patchedMetadata;
			try {
				patchedMetadata = ReadPatchedFileMetadata(file).ToList();
			}
			catch {
				return false;
			}

			var pwMeta = CecilHelper.PatchworkMetadataString;
			var patchMetas =
				from patch in patches
				select patch.Patch.PatchAssembly.GetAssemblyMetadataString();

			var origFileMeta = AssemblyCache.Default.ReadAssembly(targetFile).GetAssemblyMetadataString();
			//check that the backup assembly was patched using exactly the same version of PW
			var pwCheck = patchedMetadata.All(x => x.PatchworkAssemblyMetadata == pwMeta);
			//check that the original assembly was exactly the same as the current original assembly (e.g. in case it was updated or something).
			var origCheck = patchedMetadata.All(x => x.OriginalAssemblyMetadata == origFileMeta);
			//check that the patch configuration is the same as the current configuration (in case the user changed the configuration since the last time).
			var configPatchMetadata = patchedMetadata.Select(x => x.PatchAssemblyMetadata);
			var patchCheck = configPatchMetadata.SequenceEqual(patchMetas);
			return patchCheck && pwCheck && origCheck;
		}
		
	}

}
 