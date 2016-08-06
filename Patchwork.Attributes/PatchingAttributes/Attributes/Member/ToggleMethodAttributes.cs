using System;
using System.Reflection;

namespace Patchwork {
	/// <summary>
	/// This toggles (or XORs) all the specified decleration attributes in the method. Lets you add/remove things like 'sealed and 'virtual''.
	/// You must still use a <see cref="MemberActionAttribute"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class ToggleMethodAttributes : PatchingAttribute {
		/// <summary>
		/// The attributes to toggle. Default to 0 (no attributes).
		/// </summary>
		public MethodAttributes Attributes {
			get;
		}

		/// <summary>
		/// Constructs a new instance of the attribute.
		/// </summary>
		/// <param name="attributes">The attributes to toggle. Default to 0 (no attributes).</param>
		public ToggleMethodAttributes(MethodAttributes attributes = 0) {
			Attributes = attributes;
		}
	}
}