using System;

namespace Patchwork.Attributes {
	/// <summary>
	///     Like ModifiesTypeAttribute, but indicates that your type should completely replace the modified type.
	///     Currently only implemented for enums.
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum)]
	public class ReplaceTypeAttribute : ModifiesTypeAttribute {
		public ReplaceTypeAttribute(string fullTypeName = null) : base(fullTypeName) {

		}
	}
}