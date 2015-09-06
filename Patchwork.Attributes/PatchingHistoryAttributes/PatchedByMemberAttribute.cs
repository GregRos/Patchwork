using System;

namespace Patchwork.Attributes {
	/// <summary>
	/// Indicates that this member has been patched by another member in a patching assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Constructor, AllowMultiple = true)]
	public class PatchedByMemberAttribute : PatchingHistoryAttribute {
		public string YourMemberName {
			get;
			private set;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="yourMemberName">The member name. The declaring type is inferred.</param>
		/// <param name="actionAttributeType">The type of action attribute according to which the type was patched.</param>
		public PatchedByMemberAttribute(string yourMemberName) {
			YourMemberName = yourMemberName;
		}
	}
}