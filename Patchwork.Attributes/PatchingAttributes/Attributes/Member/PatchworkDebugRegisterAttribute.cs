using System;

namespace Patchwork {
	/// <summary>
	/// This attribute enables limited debugging capability. You can specify a string field that contains the lines executed, from the beginning of the method.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class PatchworkDebugRegisterAttribute : PatchingAttribute {
		/// <summary>
		/// The name of the string field that will store the debug information.
		/// </summary>
		public string DebugFieldName {
			get;
			private set;
		}

		/// <summary>
		/// The type that declares the field. If not specified, the modified type is assumed.
		/// </summary>
		public object DeclaringType {
			get;
			private set;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="debugFieldName">The name of the string field that will store the debug information.</param>
		/// <param name="declaringType">The type that declares the field. If not specified, the modified type is assumed.</param>
		public PatchworkDebugRegisterAttribute(string debugFieldName, object declaringType = null) {
			DebugFieldName = debugFieldName;
			DeclaringType = declaringType;

		}
	}
}
