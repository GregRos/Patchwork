namespace Patchwork.Engine {
	/// <summary>
	///     Changes how members without explicit patch attributes are treated.
	/// </summary>
	public enum ImplicitImportSetting {
		/// <summary>
		///     All new elements must be explicitly declared or an error is thrown.
		/// </summary>
		NoImplicit,

		/// <summary>
		///     Only compiler-generated elements will be implicitly imported/created.
		/// </summary>
		OnlyCompilerGenerated,

		/// <summary>
		///     (Not recommended) All elements that don't have a Patchwork ActionAttributes will be imported by default.
		/// </summary>
		ImplicitByDefault
	}
}