namespace Patchwork.History {

	/// <summary>
	/// Contains metadata that can be used to uniquely identify a patch operation.
	/// </summary>
	public class PatchApplicationMetadata {
		/// <summary>
		/// A metadata string identifying the patch assembly.
		/// </summary>
		public string PatchAssemblyMetadata {
			get;
			set;
		}

		/// <summary>
		/// A metadata string identifying the original (target) assembly.
		/// </summary>
		public string OriginalAssemblyMetadata {
			get;
			set;
		}

		/// <summary>
		/// A metadata string identifying the Patchwork.Engine assembly.
		/// </summary>
		public string PatchworkAssemblyMetadata {
			get;
			set;
		}
	}
}