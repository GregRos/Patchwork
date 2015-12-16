using System;

namespace Patchwork.Attributes {

	/// <summary>
	/// Indicates that this assembly has been patched by another assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class PatchedByAssemblyAttribute : PatchingHistoryAttribute {
		/// <param name="index">The order of the patch in a given patching session.</param>
		/// <param name="patchAssemblyMetadata"></param>
		/// <param name="originalAssemblyMetadata"></param>
		/// <param name="patchworkAssemblyMetadata"></param>
		public PatchedByAssemblyAttribute(int index, string patchAssemblyMetadata, string originalAssemblyMetadata, string patchworkAssemblyMetadata) {
			PatchAssemblyMetadata = patchAssemblyMetadata;
			OriginalAssemblyMetadata = originalAssemblyMetadata;
			PatchworkAssemblyMetadata = patchworkAssemblyMetadata;
			Index = index;
		}

		public string PatchAssemblyMetadata {
			get;
		}

		public string OriginalAssemblyMetadata {
			get;
		}

		public string PatchworkAssemblyMetadata {
			get;
		}

		public int Index {
			get;
		}

		public PatchApplicationMetadata ToPatchApplicationMetadata() {
			return new PatchApplicationMetadata() {
				OriginalAssemblyMetadata = OriginalAssemblyMetadata,
				PatchAssemblyMetadata = PatchAssemblyMetadata,
				PatchworkAssemblyMetadata = PatchworkAssemblyMetadata
			};
		}

	}
}