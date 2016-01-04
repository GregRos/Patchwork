using System.Runtime.Hosting;
using Mono.Cecil;
using Patchwork.Attributes;

namespace Patchwork {

	/// <summary>
	/// An action to perform on a member.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MemberAction<T>
		where T : IMemberDefinition {

		public TypeAction TypeAction {
			get;
			set;
		}

		public T YourMember {
			get;
			set;
		}

		public T TargetMember {
			get;
			set;
		}

		public MemberActionAttribute ActionAttribute {
			get;
			set;
		}
	}

}