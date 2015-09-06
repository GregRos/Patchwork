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
		/// ALL the things! For real this time!
		/// </summary>
		public const ModificationScope NewlyCreated = ModificationScope.All | ExplicitOverrides;
	}
}