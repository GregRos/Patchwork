using System;

namespace Patchwork.Attributes {
	/// <summary>
	///     Like <see cref="ModifiesTypeAttribute"/>, but removes all  members and introduces yours instead. Only implemented for enums.
	/// </summary>
	[AttributeUsage(AttributeTargets.Enum)]
	public class ReplaceTypeAttribute : ModifiesTypeAttribute {
		public ReplaceTypeAttribute(string fullTypeName = null) : base(fullTypeName) {
	
		}
	}
}