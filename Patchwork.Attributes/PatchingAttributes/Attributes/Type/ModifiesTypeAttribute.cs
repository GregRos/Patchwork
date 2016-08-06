using System;

namespace Patchwork {
	/// <summary>
	///     Use this attribute on a type to denote that it patches an existing type in the application.
	/// </summary>
	[AttributeUsage(CommonTargets.Types, Inherited = false)]
	public class ModifiesTypeAttribute : TypeActionAttribute {
		/// <summary>
		///     Initializes a new instance of the <see cref="ModifiesTypeAttribute" /> class.
		/// </summary>
		/// <param name="fullTypeName">
		///     Optionally, the full name of the type to be modified. <c>"base"</c> means the base type is used instead. <c>null</c> means a type
		///     with the same full name (in the target assembly) is modified.
		///     For nested classes, use the syntax <c>Namespace.Container/Nested/...</c>
		/// </param>
		public ModifiesTypeAttribute(string fullTypeName = "base") {
			FullTypeName = fullTypeName;
		}
		/// <summary>
		/// Optionally, the full name of the type to be modified. <c>"base"</c> means the base type is used instead. <c>null</c> means a type
		/// with the same full name (in the target assembly) is modified.
		/// For nested classes, use the syntax <c>Namespace.Container/Nested/...</c>
		/// </summary>
		public string FullTypeName { get; private set; }
	}


}