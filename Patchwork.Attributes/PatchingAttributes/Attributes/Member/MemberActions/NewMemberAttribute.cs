using System;
using AttrT = System.AttributeTargets;
namespace Patchwork.Attributes {

	/// <summary>
	///     Denotes that this member is a new member, which will be injected into the modified type.
	/// </summary>
	[AttributeUsage(AttrT.Field | AttrT.Property | AttrT.Method, Inherited = false)]
	public class NewMemberAttribute : MemberActionAttribute {

		public string NewMemberName {
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newMemberName">Optionally, the name of the new member. Otherwise, defaults to its compiled name. Use this option to prevent collisions.</param>
		public NewMemberAttribute(string newMemberName = null)
			: this(false, newMemberName) {

		}

		internal NewMemberAttribute(bool isImplicit, string newMemberName = null)
			: base(AdvancedModificationScope.NewlyCreated) {
			IsImplicit = isImplicit;
			NewMemberName = newMemberName;
		}

		public bool IsImplicit {
			get;
			private set;
		}
	}

}