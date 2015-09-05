using System;

namespace Patchwork.Attributes {
	/// <summary>
	/// Indicates that this member has been patched by another member in a patching assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Constructor, AllowMultiple = true)]
	[NewType(true)]
	public class PatchedByMemberAttribute : PatchingHistoryAttribute {
		public string MemberName {
			get;
			private set;
		}

		public object ActionAttributeType {
			get;
			private set;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="memberName">The member name. The declaring type is inferred.</param>
		/// <param name="actionAttributeType">The type of action attribute according to which the type was patched.</param>
		public PatchedByMemberAttribute(string memberName, object actionAttributeType) {
			MemberName = memberName;
			ActionAttributeType = actionAttributeType;
		}
	}
}