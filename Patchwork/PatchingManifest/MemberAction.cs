using Mono.Cecil;
using Patchwork.Attributes;

namespace Patchwork {

	/// <summary>
	/// This used to be an anonymous type, but later I realized I use it often enough that it needs a name.
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