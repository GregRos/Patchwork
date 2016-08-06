using System;

namespace Patchwork.History {

	/// <summary>
	/// Indicates that this assembly has been patched by another assembly and specifies metadata about the original assembly, the patch assembly, and the Patchwork engine assembly. Added automatically during patching.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class PatchedByAssemblyAttribute : PatchingHistoryAttribute {
		/// <param name="index">The order of the patch in a given patching session.</param>
		/// <param name="patchAssemblyMetadata">A string that contains identifiying metadata of the patch.</param>
		/// <param name="originalAssemblyMetadata">A string that contains the identifying metadata of the original file.</param>
		/// <param name="patchworkAssemblyMetadata">A string that contains the identifying metadata of the Patchwork engine used to perform the patching.</param>
		public PatchedByAssemblyAttribute(int index, string patchAssemblyMetadata, string originalAssemblyMetadata, string patchworkAssemblyMetadata) {
			PatchAssemblyMetadata = patchAssemblyMetadata;
			OriginalAssemblyMetadata = originalAssemblyMetadata;
			PatchworkAssemblyMetadata = patchworkAssemblyMetadata;
			Index = index;
		}

		/// <summary>
		/// A string that contains identifiying metadata of the patch.
		/// </summary>
		public string PatchAssemblyMetadata {
			get;
		}

		/// <summary>
		/// A string that contains the identifying metadata of the original file.
		/// </summary>
		public string OriginalAssemblyMetadata {
			get;
		}

		/// <summary>
		/// A string that contains the identifying metadata of the Patchwork engine used to perform the patching.
		/// </summary>
		public string PatchworkAssemblyMetadata {
			get;
		}

		/// <summary>
		/// The order of the patch in a given patching session.
		/// </summary>
		public int Index {
			get;
		}

		/// <summary>
		/// Converts the info in this attribute into a <see cref="PatchApplicationMetadata"/> instance.
		/// </summary>
		/// <returns></returns>
		public PatchApplicationMetadata ToPatchApplicationMetadata() {
			return new PatchApplicationMetadata() {
				OriginalAssemblyMetadata = OriginalAssemblyMetadata,
				PatchAssemblyMetadata = PatchAssemblyMetadata,
				PatchworkAssemblyMetadata = PatchworkAssemblyMetadata
			};
		}

	}
}