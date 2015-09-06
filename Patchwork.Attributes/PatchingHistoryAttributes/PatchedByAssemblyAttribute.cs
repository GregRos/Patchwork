using System;

namespace Patchwork.Attributes {
	/// <summary>
	/// Indicates that this assembly has been patched by another assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class PatchedByAssemblyAttribute : PatchingHistoryAttribute {
		/// <param name="yourAssemblyFullName">The full name of the assembly according to which this assembly was patched.</param>
		public PatchedByAssemblyAttribute(string yourAssemblyFullName) {
			YourAssemblyFullName = yourAssemblyFullName;
		}

		public string YourAssemblyFullName {
			get;
			private set;
		}
	}
}