using System;

namespace Patchwork.Attributes {
	/// <summary>
	/// Indicates that this assembly has been patched by another assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class PatchedByAssemblyAttribute : PatchingHistoryAttribute {
		/// <param name="patchAssembly">The full name of the assembly according to which this assembly was patched.</param>
		/// <param name="index">The order of the patch in a given patching session.</param>
		public PatchedByAssemblyAttribute(string patchAssembly, int index) {
			PatchAssembly = patchAssembly;
			Index = index;
		}

		public string PatchAssembly {
			get;
		}

		public int Index {
			get;
		}
	}
}