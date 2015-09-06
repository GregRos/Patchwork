using System;

namespace Patchwork.Attributes {
	/// <summary>
	/// Indicates that this type has been patched by another type in a patching assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = true)]
	public class PatchedByTypeAttribute : PatchingHistoryAttribute {

		public string YourType {
			get;
			private set;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="yourType">The type according to which this type was patched.</param>
		public PatchedByTypeAttribute(string yourType) : base() {
			YourType = yourType;
		}
	}
}