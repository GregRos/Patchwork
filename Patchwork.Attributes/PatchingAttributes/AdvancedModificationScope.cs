namespace Patchwork.Attributes {
	/// <summary>
	/// Contains advanced modification scopes for internal use.
	/// </summary>
	internal static class AdvancedModificationScope {

		

		/// <summary>
		/// Specifies that a method's explicitly declared overrides section should be changed. 
		/// </summary>
		public const ModificationScope ExplicitOverrides = (ModificationScope) (1 << 16);

		/// <summary>
		/// Specifies that a modification scope is not applicable for this element.
		/// </summary>
		public const ModificationScope NotApplicable = (ModificationScope) (1 << 17);

		/// <summary>
		/// ALL the things! For real this time!
		/// </summary>
		public const ModificationScope NewlyCreated = ModificationScope.All | ExplicitOverrides;
	}
}