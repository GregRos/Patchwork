using System;

namespace Patchwork.Attributes {

	/// <summary>
	///     Denotes that this member is a new member, which will be injected into the modified type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class NewMemberAttribute : MemberActionAttribute {
		public NewMemberAttribute() {
		}

		internal NewMemberAttribute(bool isImplicit) {
			IsImplicit = isImplicit;
		}

		public bool IsImplicit { get; private set; }
	}

}