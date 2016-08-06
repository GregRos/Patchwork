using System;
using AttrT = System.AttributeTargets;
namespace Patchwork {
	/// <summary>
	///     Removes the member from the modified type.
	/// </summary>
	[Obsolete("This attribute is obsolete. There appears little reason to remove existing members.")]
	[AttributeUsage(AttrT.Field | AttrT.Property | AttrT.Method | AttrT.Constructor, Inherited = false)]
	public class RemoveThisMemberAttribute : MemberActionAttribute {
		/// <summary>
		/// Constructs a new instance of this attribute.
		/// </summary>
		public RemoveThisMemberAttribute()
			: base(ModificationScope.All) {
		}
	}
}