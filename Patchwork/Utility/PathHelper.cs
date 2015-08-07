using System.IO;
using System.Linq;
using System.Reflection;

namespace Patchwork.Utility {
	/// <summary>
	/// Just a few helper methods for strings. Nothing to see here.
	/// </summary>
	public static class PathHelper {
		public static string GetAbsolutePath(string relativePath) {
			var myPath = Assembly.GetExecutingAssembly().Location;
			var result = Path.Combine(myPath, "..", relativePath);
			var full = Path.GetFullPath(result);
			return full;
		}

		public static string GetUserFriendlyPath(string path) {
			var components = path.Split('\\').ToList();
			var total = components.Count;
			if (total <= 5) {
				return path;
			}
			components.RemoveRange(2, components.Count - 4);
			components.Insert(2, "...");
			return components.Join("\\");
		}
	}
}