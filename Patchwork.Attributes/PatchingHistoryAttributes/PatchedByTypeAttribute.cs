using System;

namespace Patchwork.Attributes {
	/// <summary>
	/// Indicates that this type has been patched by another type in a patching assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = true)]
	[NewType(true)]
	public class PatchedByTypeAttribute : PatchingHistoryAttribute {
		public object PatchingType {
			get;
			private set;
		}

		public object ActionAttributeType {
			get;
			private set;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="patchingType">The type according to which this type was patched.</param>
		/// <param name="actionAttributeType">The type of action attribute, denoting the patching action taken.</param>
		public PatchedByTypeAttribute(object patchingType, object actionAttributeType) {
			PatchingType = patchingType;
			ActionAttributeType = actionAttributeType;
		}
	}
}