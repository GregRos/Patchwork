using System;

namespace Patchwork {
	/// <summary>
	///     Like <see cref="ModifiesTypeAttribute"/>, but removes all  members and introduces yours instead. Only implemented for enums.
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum)]
	public class ReplaceTypeAttribute : ModifiesTypeAttribute {
		/// <summary>
		/// Constructs a new instance of this attribute.
		/// </summary>
		/// <param name="fullTypeName">The full name of the type to apply this attribute to.</param>
		public ReplaceTypeAttribute(string fullTypeName = null) : base(fullTypeName) {
	
		}
	}
}