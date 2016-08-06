using Mono.Cecil;

namespace Patchwork.Engine {

	/// <summary>
	/// An action to perform on a member.
	/// </summary>
	/// <typeparam name="T">The type of member definition on which to perform the action.</typeparam>
	public class MemberAction<T>
		where T : IMemberDefinition {

		/// <summary>
		/// The type action in the context of which this member action is being performed.
		/// </summary>
		public TypeAction TypeAction {
			get;
			set;
		}

		/// <summary>
		/// The member definition in the patching assembly on behalf of which patching is being performed.
		/// </summary>
		public T YourMember {
			get;
			set;
		}

		/// <summary>
		/// The member definition in the target assembly which is to be patched, if any.
		/// </summary>
		public T TargetMember {
			get;
			set;
		}

		/// <summary>
		/// The member action that is to be performed.
		/// </summary>
		public MemberActionAttribute ActionAttribute {
			get;
			set;
		}
	}

}