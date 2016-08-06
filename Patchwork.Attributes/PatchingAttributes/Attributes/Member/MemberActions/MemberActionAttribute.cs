namespace Patchwork {
	/// <summary>
	///     This kind of attribute denotes the action to perform regarding this member. There should be only one such attribute
	///     on an element.
	/// </summary>
	public abstract class MemberActionAttribute : PatchingAttribute {
		/// <summary>
		/// Constructs a new instance of the attribute.
		/// </summary>
		/// <param name="scope">The scope of the modification applied by this attribute.</param>
		protected MemberActionAttribute(ModificationScope scope) {
			Scope = scope;
		}

		/// <summary>
		/// The scope of the modification applied by this attribute.
		/// </summary>
		public ModificationScope Scope {
			get;
		}
	}
}