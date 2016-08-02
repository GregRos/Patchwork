using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Patchwork.Attributes {

	/// <summary>
	/// This toggles (or XORs) all the specified decleration attributes in the field. Lets you add/remove things like 'readonly'.
	/// You must still use a <see cref="MemberActionAttribute"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ToggleFieldAttributes : PatchingAttribute {
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
