using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Patchwork.Utility {
	/// <summary>
	/// Utility methods for dealing with paths.
	/// </summary>
	public static class PathHelper {
		private static readonly string _executingAssemblyPath  = Assembly.GetExecutingAssembly().Location;

		public static string GetAbsolutePath(string relativePath) {
			if (Path.IsPathRooted(relativePath)) {
				return relativePath;
			}
			var result = Path.Combine(_executingAssemblyPath, "..", relativePath);
			var full = Path.GetFullPath(result);
			return full;
		}

		public static string ChangeExtension(string path, Func<string, string> selector) {
			var ext = Path.GetExtension(path);
			return Path.ChangeExtension(path, selector(ext));
		}

		public static IEnumerable<string> Components(string path) {
			return path.Split(Path.DirectorySeparatorChar);
		} 

		public static string GetUserFriendlyPath(string path) {
			var components = Components(path).ToList();
			var total = components.Count;
			if (total <= 5) {
				return path;
			}
			components.RemoveRange(2, components.Count - 4);
			components.Insert(2, "...");
			return Path.Combine(components.ToArray());
		}
	}
}