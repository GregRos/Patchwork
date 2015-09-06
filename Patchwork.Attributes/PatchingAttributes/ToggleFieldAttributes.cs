using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Patchwork.Attributes {

	/// <summary>
	/// This toggles (or XORs) all the specified decleration attributes in the field. Lets you add/remove things like 'readonly'.
	/// Must be used with ModifiesMemberAttribute.
	/// </summary>
	public class ToggleFieldAttributes : PatchingAttribute {
		public FieldAttributes Attributes {
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="attributes">The attributes to toggle. Defaults to 0 (no attributes).</param>
		public ToggleFieldAttributes(FieldAttributes attributes = 0) {
			Attributes = attributes;
		}
	}

	/// <summary>
	/// This toggles (or XORs) all the specified decleration attributes in the method. Lets you add/remove things like 'sealed and 'virtual''.
	/// Must be used with ModifiesMemberAttribute.
	/// </summary>
	public class ToggleMethodAttributes : PatchingAttribute {
		public MethodAttributes Attributes {
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="attributes">The attributes to toggle. Default to 0 (no attributes).</param>
		public ToggleMethodAttributes(MethodAttributes attributes = 0) {
			Attributes = attributes;
		}
	}

	/// <summary>
	/// This toggles (or XORs) all the specified decleration attributes in the type. Lets you add/remove things like 'sealed'.
	/// Must be used with ModifiesMemberAttribute.
	/// Not implemented.
	/// </summary>
	internal class ToggleTypeAttributes : PatchingAttribute {
		public TypeAttributes Attributes {
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="attributes">The attributes to toggle. Default to 0 (no attributes).</param>
		private ToggleTypeAttributes(TypeAttributes attributes = 0) {
			Attributes = attributes;
		}
	}
}
