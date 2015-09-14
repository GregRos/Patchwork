using System.Reflection;

namespace Patchwork.Attributes {
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
}