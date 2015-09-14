using System;

namespace Patchwork.Attributes {
	/// <summary>
	///     A special case of ModifiesMemberAttribute. Modifies only the accessibility (public/private/etc).
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
	public class ModifiesAccessibilityAttribute : ModifiesMemberAttribute {
		public ModifiesAccessibilityAttribute(string memberName = null) : base(memberName, ModificationScope.Accessibility) {
		}
	}
}