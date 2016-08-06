namespace Patchwork {
	/// <summary>
	/// Changes how calls to this alias are translated in the target assembly.
	/// </summary>
	public enum AliasCallMode {
		/// <summary>
		/// The call to the aliased member will use the same calling convention as the original call.
		/// </summary>
		NoChange,
		/// <summary>
		/// The call to the aliased member will always be a virtual call, even if the original was non-virtual.
		/// </summary>
		Virtual,
		/// <summary>
		/// The call to the aliased member will always be non-virtual, even if the original was virtual. This allows you to call overridden members.
		/// </summary>
		NonVirtual
	}
}