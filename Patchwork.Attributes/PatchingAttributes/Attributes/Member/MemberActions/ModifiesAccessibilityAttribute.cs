using System;
using AttrT = System.AttributeTargets;
namespace Patchwork.Attributes {
	/// <summary>
	///     A special case of <see cref="ModifiesMemberAttribute"/>. Modifies only the accessibility (public/private/etc).
	/// </summary>
	[AttributeUsage(AttrT.Field | AttrT.Method | AttrT.Property, Inherited = false)]
	public class ModifiesAccessibilityAttribute : ModifiesMemberAttribute {
		public ModifiesAccessibilityAttribute(string memberName = null) : base(memberName, ModificationScope.Accessibility) {
		}
	}
}