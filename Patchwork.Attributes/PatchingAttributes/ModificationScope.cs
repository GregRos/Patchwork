using System;
using System.ComponentModel;

namespace Patchwork.Attributes {
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

	/// <summary>
	/// Contains advanced modification scopes for internal use.
	/// </summary>
	internal static class AdvancedModificationScope {
		/// <summary>
		/// Specifies that a method's explicitly declared overrides section should be changed. 
		/// </summary>
		public const ModificationScope ExplicitOverrides = (ModificationScope) (1 << 16);

		/// <summary>
		/// ALL the things! For real this time!
		/// </summary>
		public const ModificationScope NewlyCreated = ModificationScope.All | ExplicitOverrides;
	}
}