using System;

namespace Patchwork.Attributes {
	/// <summary>
	/// Indicates that this assembly has been patched by another assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[NewType(true)]
	public class PatchedByAssemblyAttribute : PatchingHistoryAttribute {
		/// <param name="assemblyFullName">The full name of the assembly according to which this assembly was patched.</param>
		public PatchedByAssemblyAttribute(string assemblyFullName) {
			AssemblyFullName = assemblyFullName;
		}

		public string AssemblyFullName {
			get;
			private set;
		}
	}
}