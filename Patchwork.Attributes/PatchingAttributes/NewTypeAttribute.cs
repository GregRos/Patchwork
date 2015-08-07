namespace Patchwork.Attributes {
	public class NewTypeAttribute : TypeActionAttribute {
		public NewTypeAttribute() {
		}

		/// <summary>
		///     Creates a new NewTypeAttribute, possibly marking the type as implicit.
		/// </summary>
		/// <param name="isImplicit">If set to true, the type is considered implicitly new, which generates various warnings.</param>
		internal NewTypeAttribute(bool isImplicit) {
			IsImplicit = isImplicit;
		}

		public bool IsImplicit { get; private set; }
	}
}