using System;

namespace Patchwork {
	/// <summary>
	///     The scope of a member modification. Use this to modify only the body, accessibility, value, etc.
	/// </summary>
	[Flags]
	public enum ModificationScope {
		/// <summary>
		///     Don't modify element.
		/// </summary>
		Nothing = 0,

		/// <summary>
		///     Modifies the body of the element. For properties, this can change the getter/setter methods, if new ones were
		///     defined. For fields, this changes the constant value.
		/// </summary>
		Body = 1 << 0,

		/// <summary>
		///     Modifies accessibility only.
		/// </summary>
		Accessibility = 1 << 1,

		/// <summary>
		/// Adds any custom attributes on the member (doesn't erase existing attributes).
		/// </summary>
		CustomAttributes = 1 << 2,

		/// <summary>
		///     All the things!
		/// </summary>
		All = Body | Accessibility | CustomAttributes,
	}

}