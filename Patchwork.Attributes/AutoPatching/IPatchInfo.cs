using System.IO;

namespace Patchwork.AutoPatching {
	/// <summary>
	/// Represents information about a patch and the manner in which patching should be performed. An inheriting class must have a default constructor and be decorated with <see cref="PatchInfoAttribute"/>.
	/// </summary>
	public interface IPatchInfo {
		/// <summary>
		/// Returns the file this patch is meant to patch, when supplied information about the app. This method is supposed to locate the file, etc.
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		FileInfo GetTargetFile(AppInfo app);

		/// <summary>
		/// Determines if this patch can be applied to the specified application. If so, returns 'null'. Otherwise, returns a string that describes the problem.
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		string CanPatch(AppInfo app);

		/// <summary>
		/// Returns the version of the patch.
		/// </summary>
		string PatchVersion { get; }

		/// <summary>
		/// Returns a display of the requirements of the patch.
		/// </summary>
		string Requirements { get; }

		/// <summary>
		/// Returns the display name of the patch.
		/// </summary>
		string PatchName { get; }
	}
}