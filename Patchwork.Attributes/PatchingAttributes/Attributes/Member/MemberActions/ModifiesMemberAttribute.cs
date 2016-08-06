using System;
using AttrT = System.AttributeTargets;
namespace Patchwork {
	/// <summary>
	///     Marks this member as being a modification of a member in the target assembly.
	/// </summary>
	[AttributeUsage(AttrT.Field | AttrT.Method | AttrT.Property | AttrT.Constructor, Inherited = false)]
	public class ModifiesMemberAttribute : MemberActionAttribute {
		/// <summary>
		///     Initializes a new instance of the <see cref="ModifiesMemberAttribute" /> class.
		/// </summary>
		/// <param name="memberName">Optionally, name of the member to be modified. If null, a member with the same name is used.</param>
		/// <param name="scope">
		///     Can limit the scope of the modification. Everything is modified by default. Usually used to make
		///     things public, etc.
		/// </param>
		public ModifiesMemberAttribute(string memberName = null, ModificationScope scope = ModificationScope.All)
		: base(scope) {
			MemberName = memberName;
		}

		/// <summary>
		/// Optionally, name of the member to be modified. If null, a member with the same name is used.
		/// </summary>
		public string MemberName { get; private set; }

	}
}