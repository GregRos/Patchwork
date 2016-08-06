using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Patchwork;
using Patchwork.AutoPatching;
using Patchwork.Engine.Utility;
using Patchwork.History;
using Patchwork.Utility;
using Serilog;

namespace PatchworkLauncher {

	internal class PatchingHelper {

		public static string GetBackupForOriginal(string path) {
			return PathHelper.ChangeExtension(path, ext => $"{ext}.pw.original");
		}

		public static string GetBackupForModified(string path) {
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



		public static bool SwitchFilesSafely(string takeFileFromPath, string putItHere, string putExistingIn) {
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

		public static AppInfoFactory LoadAppInfoFactory(string assembly) {
			Assembly gameInfoAssembly = null;
			var absolutePath = PathHelper.GetAbsolutePathFromAssembly(assembly);
			if (!File.Exists(absolutePath)) {
				throw new FileNotFoundException($"The AppInfo assembly file was not found.", assembly);
			}
			gameInfoAssembly = Assembly.LoadFile(absolutePath);

			var gameInfoFactories =
				from type in gameInfoAssembly.GetTypes()
				where type.GetCustomAttribute<AppInfoFactoryAttribute>() != null
				select type;

			var factories = gameInfoFactories.ToList();

			if (factories.Count == 0) {
				throw new ArgumentException(
					$"The AppInfo assembly did not have a class decorated with {nameof(AppInfoFactoryAttribute)}");
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
			var gameInfoFactory = (AppInfoFactory) ctor.Invoke(null);
			return gameInfoFactory;
		}
	}

}
 