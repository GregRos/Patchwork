namespace Patchwork.Attributes {
	public class NewTypeAttribute : TypeActionAttribute {
		public string NewTypeName {
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newTypeName">The name of the new type. Otherwise, the name is unchanged. You can use this option to prevent collisions.</param>
		public NewTypeAttribute(string newTypeName = null) : this(false, newTypeName) {
			
		}

		/// <summary>
		///     Creates a new NewTypeAttribute, possibly marking the type as implicit.
		/// </summary>
		/// <param name="isImplicit">If set to true, the type is considered implicitly new, which generates various warnings.</param>
		/// <param name="newTypeName"></param>
		internal NewTypeAttribute(bool isImplicit, string newTypeName = null)  {
			IsImplicit = isImplicit;
			NewTypeName = newTypeName;
		}

		public bool IsImplicit { get; private set; }

	}
}