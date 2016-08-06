using Mono.Cecil;

namespace Patchwork.Engine {
	/// <summary>
	/// Specifies an action to be taken on a type.
	/// </summary>
	public class TypeAction {
		/// <summary>
		/// The type in the patching assembly on behalf of which patching is performed.
		/// </summary>
		public TypeDefinition YourType {
			get;
			set;
		}
		
		/// <summary>
		/// The type action attribute that specifies the operation.
		/// </summary>
		public TypeActionAttribute ActionAttribute {
			get;
			set;
		}

		/// <summary>
		/// The type in the target assembly on which to perform the operation, if any.
		/// </summary>
		public TypeDefinition TargetType {
			get;
			set;
		}


	}
}