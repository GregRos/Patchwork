using System;
using System.Reflection;

namespace Patchwork {

	/// <summary>
	/// This toggles (or XORs) all the specified decleration attributes in the field. Lets you add/remove things like 'readonly'.
	/// You must still use a <see cref="MemberActionAttribute"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ToggleFieldAttributes : PatchingAttribute {
		/// <summary>
		/// The attributes to toggle. Defaults to 0 (no attributes).
		/// </summary>
		public FieldAttributes Attributes {
			get;
		}

		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		/// <param name="attributes">The attributes to toggle. Defaults to 0 (no attributes).</param>
		public ToggleFieldAttributes(FieldAttributes attributes = 0) {
			Attributes = attributes;
		}
	}

}
