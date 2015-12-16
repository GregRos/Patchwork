using System;
using AttrT = System.AttributeTargets;
namespace Patchwork.Attributes {
	/// <summary>
	///     Removes the member from the modified type.
	/// </summary>
	[AttributeUsage(AttrT.Field | AttrT.Property | AttrT.Method | AttrT.Constructor, Inherited = false)]
	public class RemoveThisMemberAttribute : MemberActionAttribute {
		public RemoveThisMemberAttribute()
			: base(ModificationScope.All) {
		}
	}
}