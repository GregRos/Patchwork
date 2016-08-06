using System;

namespace Patchwork {
	/// <summary>
	/// Indicates that this type is a new type that must be injected into the assembly.
	/// </summary>
	[AttributeUsage(CommonTargets.Types, Inherited = false)]
	public class NewTypeAttribute : TypeActionAttribute {
		/// <summary>
		/// The short name under which the type is introduced. If <c>null</c>, the name is unchanged. Dots should not be used here.
		/// </summary>
		public string NewTypeName {
			get;
		}

		/// <summary>
		/// The namespace under which the type will be introduced. May contain dots. If <c>null</c>, the type is unchanged.
		/// </summary>
		public string NewNamespace {
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newTypeName">The short name under which the type is introduced. If <c>null</c>, the name is unchanged. Dots should not be used here.</param>
		/// <param name="newNamespace">The namespace under which the type will be introduced. May contain dots. If <c>null</c>, the type is unchanged.</param>
		public NewTypeAttribute(string newTypeName = null, string newNamespace = null) : this(false, newTypeName, newNamespace) {
			
		}

		/// <summary>
		///     Creates a new NewTypeAttribute, possibly marking the type as implicit.
		/// </summary>
		/// <param name="isImplicit">If set to true, the type is considered implicitly new, which generates various warnings.</param>
		/// <param name="newTypeName">The short name under which the type is introduced. If <c>null</c>, the name is unchanged. Dots should not be used here.</param>
		/// <param name="newNamespace">The namespace under which the type will be introduced. May contain dots. If <c>null</c>, the type is unchanged.</param>
		internal NewTypeAttribute(bool isImplicit, string newTypeName = null, string newNamespace = null)  {
			IsImplicit = isImplicit;
			NewTypeName = newTypeName;
			NewNamespace = newNamespace;
		}
		/// <summary>
		/// Whether or not this attribute signifies an implicitly new member.
		/// </summary>
		public bool IsImplicit { get; private set; }

	}
}