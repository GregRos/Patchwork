using System;

namespace Patchwork.History {
	/// <summary>
	/// Indicates that this type has been patched by another type in a patching assembly.
	/// </summary>
	[AttributeUsage(CommonTargets.Types, AllowMultiple = true, Inherited = false)]
	public class PatchedByTypeAttribute : PatchingHistoryAttribute {
		/// <summary>
		/// The type according to which this type was patched.
		/// </summary>
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