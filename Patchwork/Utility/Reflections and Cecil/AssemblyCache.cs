using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;

namespace Patchwork.Utility {
	/// <summary>
	/// An AssemblyDefinition loader that doesn't load an assembly if it's in the cache.
	/// </summary>
	public class AssemblyCache {

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

		public AssemblyDefinition ReadAssembly(string path, bool readSymbols = false) {
			var fileInfo = new FileInfo(path);
			fileInfo.Refresh();
			if (_cache.ContainsKey(path)) {
				if (DoesCacheMatch(fileInfo, _cache[path].Metadata)) {
					return _cache[path].Assembly;
				}
			}
			var read = AssemblyDefinition.ReadAssembly(path, new ReaderParameters() {
				ReadSymbols = readSymbols
			});
			var assemblyResolver = read.MainModule.AssemblyResolver as BaseAssemblyResolver;
			//Cecil doesn't add the assembly's original directory as a search path by default.
			var dir = Path.GetDirectoryName(path);
			assemblyResolver.RegisterSpecialResolveFailureHandler ((sender, name) => {
				var localDir = Path.GetDirectoryName (path);

				var localPath = Path.Combine (dir, $"{name.Name}.dll");
				return File.Exists (localPath) ? AssemblyCache.Default.ReadAssembly (localPath) : null;
			});

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

		public void Clear() {
			_cache.Clear();
		}
	}
}