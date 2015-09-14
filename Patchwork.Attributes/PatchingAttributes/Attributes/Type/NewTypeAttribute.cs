namespace Patchwork.Attributes {
	public class NewTypeAttribute : TypeActionAttribute {
		public string NewTypeName {
			get;
		}

		public string NewNamespace {
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newTypeName">The short name of the new type. If null, the name is unchanged. Dots should not be used here.</param>
		/// <param name="newNamespace">The namespace the type will be moved to. If null, the namespace is unchanged.</param>
		public NewTypeAttribute(string newTypeName = null, string newNamespace = null) : this(false, newTypeName, newNamespace) {
			
		}

		/// <summary>
		///     Creates a new NewTypeAttribute, possibly marking the type as implicit.
		/// </summary>
		/// <param name="isImplicit">If set to true, the type is considered implicitly new, which generates various warnings.</param>
		/// <param name="newTypeName"></param>
		/// <param name="newNamespace"></param>
		internal NewTypeAttribute(bool isImplicit, string newTypeName = null, string newNamespace = null)  {
			IsImplicit = isImplicit;
			NewTypeName = newTypeName;
			NewNamespace = newNamespace;
		}

		public bool IsImplicit { get; private set; }

	}
}