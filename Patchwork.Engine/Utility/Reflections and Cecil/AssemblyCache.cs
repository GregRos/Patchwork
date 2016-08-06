using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace Patchwork.Engine.Utility {

	internal class ExpandedAssemblyResolver : DefaultAssemblyResolver {
		public new void RegisterAssembly(AssemblyDefinition assemblyDef) {
			base.RegisterAssembly(assemblyDef);
		}
	}

	/// <summary>
	/// An AssemblyDefinition loader that doesn't load an assembly if it's in the cache.
	/// </summary>
	public class AssemblyCache {

		/// <summary>
		/// Gets the default AssemblyCache.
		/// </summary>
		public static readonly AssemblyCache Default = new AssemblyCache();

		

		private class FileMetadata {

			public long Length {
				get;
				set;
			}

			public DateTime LastWriteTime {
				get;
				set;
			}

			public DateTime CreationTime {
				get;
				set;
			}
		}

		private class AssemblyCacheEntry {
			public string Path {
				get;
				set;
			}

			public FileMetadata Metadata {
				get;
				set;
			}

			public AssemblyDefinition Assembly {
				get;
				set;
			}
		}

		private readonly IDictionary<string, AssemblyCacheEntry> _cache = new Dictionary<string, AssemblyCacheEntry>();

		private static bool DoesCacheMatch(FileInfo currentFile, FileMetadata storedMetadata) {
			currentFile.Refresh();
			return currentFile.Length == storedMetadata.Length
				&& currentFile.LastWriteTimeUtc == storedMetadata.LastWriteTime
				&& currentFile.CreationTimeUtc == storedMetadata.CreationTime
				;

		}

		/// <summary>
		/// Reads the assembly from the given path, or else loads it from cache.
		/// </summary>
		/// <param name="path">The patch to read the assembly from.</param>
		/// <param name="readSymbols">Whether or not to read symbols.</param>
		/// <returns></returns>
		public AssemblyDefinition ReadAssembly(string path, bool readSymbols = false) {
			var fileInfo = new FileInfo(path);
			fileInfo.Refresh();
			if (_cache.ContainsKey(path)) {
				if (DoesCacheMatch(fileInfo, _cache[path].Metadata)) {
					return _cache[path].Assembly;
				}
			}
			var defAssemblyResolver = new ExpandedAssemblyResolver();
			defAssemblyResolver.AddSearchDirectory(Path.GetDirectoryName(path));
			var rdrParams = new ReaderParameters() {
				AssemblyResolver = defAssemblyResolver,
				ReadSymbols = readSymbols
			};
			var read = AssemblyDefinition.ReadAssembly(path, rdrParams);
			var assemblyResolver = read.MainModule.AssemblyResolver as BaseAssemblyResolver;
			//Cecil doesn't add the assembly's original directory as a search path by default.
			var dir = Path.GetDirectoryName(path);
			

			var entry = new AssemblyCacheEntry() {
				Assembly = read,
				Metadata = new FileMetadata() {
					Length = fileInfo.Length,
					LastWriteTime = fileInfo.LastWriteTimeUtc,
					CreationTime = fileInfo.CreationTimeUtc
				},
				Path = path
			};
			_cache[path] = entry;
			return read;
		}

		/// <summary>
		/// Clears the assembly cache.
		/// </summary>
		public void ClearCache() {
			_cache.Clear();
		}
	}
}