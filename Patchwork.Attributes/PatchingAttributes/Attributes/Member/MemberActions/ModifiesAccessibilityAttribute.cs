using System;
using AttrT = System.AttributeTargets;
namespace Patchwork {
	/// <summary>
	///     A special case of <see cref="ModifiesMemberAttribute"/>. Modifies only the accessibility (public/private/etc).
	/// </summary>
	[AttributeUsage(AttrT.Field | AttrT.Method | AttrT.Property, Inherited = false)]
	public class ModifiesAccessibilityAttribute : ModifiesMemberAttribute {
		/// <summary>
		/// Constructs a new instance of the attribute.
		/// </summary>
		/// <param name="memberName">Optionally, the name of the member to be modified. If not specified, the name of the current member is used.</param>
		public ModifiesAccessibilityAttribute(string memberName = null) : base(memberName, ModificationScope.Accessibility) {
		}
	}
}