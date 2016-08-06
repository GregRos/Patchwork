using System;
using AttrT = System.AttributeTargets;
namespace Patchwork {

	/// <summary>
	///     Denotes that this member is a new member, which will be injected into the modified type.
	/// </summary>
	[AttributeUsage(AttrT.Field | AttrT.Property | AttrT.Method, Inherited = false)]
	public class NewMemberAttribute : MemberActionAttribute {

		/// <summary>
		/// The name of the member.
		/// </summary>
		public string NewMemberName {
			get;
		}

		/// <summary>
		/// Constructs a new instance of this attribute.
		/// </summary>
		/// <param name="newMemberName">Optionally, the name the new member is introduced with. Otherwise, defaults to its compiled name. Use this option to prevent collisions.</param>
		public NewMemberAttribute(string newMemberName = null)
			: this(false, newMemberName) {

		}

		/// <summary>
		/// Constructs a new instance of this attribute.
		/// </summary>
		/// <param name="isImplicit">Whether or not the member counts as an implicit new member. Changes the behavior of the engine in some cases.</param>
		/// <param name="newMemberName">Optionally, the name of the new member. Otherwise, defaults to its compiled name. Use this option to prevent collisions.</param>
		protected internal NewMemberAttribute(bool isImplicit, string newMemberName = null)
			: base(AdvancedModificationScope.NewlyCreated) {
			IsImplicit = isImplicit;
			NewMemberName = newMemberName;
		}

		/// <summary>
		/// Whether or not 
		/// </summary>
		public bool IsImplicit {
			get;
			private set;
		}
	}

}