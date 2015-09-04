using System.IO;

namespace Patchwork.Attributes {
	public abstract class PatchInfo {
		/// <summary>
		/// Returns the file to be patched with the current patch.
		/// </summary>
		/// <param name="gameFolder">The game folder, as determined by Engage or supplied by the user.</param>
		/// <returns></returns>
		public abstract FileInfo GetPatchTarget(DirectoryInfo gameFolder);

		/// <summary>
		/// This method is executed to check if the patch can be applied to the specified file. It can contain version checks, etc. If not, an exception should be thrown.
		/// </summary>
		public virtual void CanPatch(FileInfo target) {
			
		}

		/// <summary>
		/// This method is executed right after applying the patch. Can contain checks, etc. If this method throws an exception, the action is reverted.
		/// </summary>
		public virtual void AfterPatching() {
			
		}
	}
}