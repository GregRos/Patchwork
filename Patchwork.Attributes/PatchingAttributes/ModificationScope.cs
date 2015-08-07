using System;

namespace Patchwork.Attributes {
	/// <summary>
	///     The scope of a member modification. Use this to modify only the body, accessibility, value, etc.
	/// </summary>
	[Flags]
	public enum ModificationScope {
		/// <summary>
		///     Don't modify element.
		/// </summary>
		Nothing = 0x0,

		/// <summary>
		///     Modifies the body of the element. For properties, this can change the getter/setter methods, if new ones were
		///     defined. For fields, this changes the constant value.
		/// </summary>
		Body = 0x1,

		/// <summary>
		///     Modifies accessibility only.
		/// </summary>
		Accessibility = 0x2,

		/// <summary>
		/// Adds any custom attributes on the member (doesn't erase existing attributes).
		/// </summary>
		CustomAttributes = 0x4,


		/// <summary>
		///     All the things!
		/// </summary>
		All = Body | Accessibility | CustomAttributes
	}
}