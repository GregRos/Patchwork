using System.Reflection;

namespace Patchwork {
	/// <summary>
	/// This toggles (or XORs) all the specified decleration attributes in the type. Lets you add/remove things like 'sealed'.
	/// You must still use a <see cref="TypeActionAttribute"/>.
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