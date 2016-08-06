using System;

namespace Patchwork.History {
	/// <summary>
	/// Indicates that this member has been patched by another member in a patching assembly.
	/// </summary>
	[AttributeUsage(CommonTargets.Members, AllowMultiple = true, Inherited = false)]
	public class PatchedByMemberAttribute : PatchingHistoryAttribute {

		/// <summary>
		/// The member name that did the patching.
		/// </summary>
		public string YourMemberName {
			get;
			private set;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="yourMemberName">The member name. The declaring type is inferred.</param>
		public PatchedByMemberAttribute(string yourMemberName) {
			YourMemberName = yourMemberName;
		}
	}
}