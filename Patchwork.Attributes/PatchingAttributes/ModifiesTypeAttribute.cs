using System;

namespace Patchwork.Attributes {
	/// <summary>
	///     Use this attribute on a type to denote that it patches an existing type in the game.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
	public class ModifiesTypeAttribute : TypeActionAttribute {
		/// <summary>
		///     Initializes a new instance of the <see cref="ModifiesTypeAttribute" /> class.
		/// </summary>
		/// <param name="fullTypeName">
		///     Optionally, the full name of the type to be modified. "base" means the base type is used instead. null means a type
		///     with the same name is used.
		///     For nested classes, use the syntax <c>Namespace.Container/Nested/...</c>
		/// </param>
		public ModifiesTypeAttribute(string fullTypeName = "base") {
			FullTypeName = fullTypeName;
		}

		public string FullTypeName { get; private set; }
	}


}