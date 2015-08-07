using System;

namespace Patchwork.Attributes {
	/// <summary>
	///     Removes the member from the modified type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public class RemoveThisMemberAttribute : MemberActionAttribute {
	}
}