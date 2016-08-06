using System;
using System.Runtime.Serialization;
#pragma warning disable 1591

namespace Patchwork.Engine {
	/// <summary>
	/// Base class for all Patchwork-specific exceptions the library throws.
	/// </summary>
	[Serializable]
	public abstract class PatchException : ApplicationException {

		protected PatchException() {
			
		}

		protected PatchException(SerializationInfo info, StreamingContext c) : base(info, c) {
			
		}

		protected PatchException(string message, Exception innerException) : base(message, innerException) {
		}

		protected PatchException(string message) : base(message) {
		}

	}
}