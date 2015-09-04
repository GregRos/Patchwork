using Mono.Cecil;
using Patchwork.Attributes;

namespace Patchwork {
	public class TypeAction {
		public TypeDefinition YourType {
			get;
			set;
		}

		public TypeActionAttribute ActionAttribute {
			get;
			set;
		}

		public TypeDefinition TargetType {
			get;
			set;
		}
	}
}