using System;
using System.Runtime.Serialization;
using Patchwork.AutoPatching;
#pragma warning disable 1591

namespace Patchwork.Engine {
	/// <summary>
	/// One of the declerations in the patching assembly is invalid in the context of the target assembly.
	/// </summary>
	[Serializable]
	public class PatchDeclerationException : PatchException {
		public PatchDeclerationException(SerializationInfo info, StreamingContext c)
			: base(info, c) {
		}

		public PatchDeclerationException() {
		}

		public PatchDeclerationException(string message, Exception innerException) : base(message, innerException) {
		}

		public PatchDeclerationException(string message) : base(message) {
		}
	}

	/// <summary>
	/// Executing code contained within a patch (such as the <see cref="IPatchInfo"/> class) threw an exception or otherwise failed.
	/// </summary>
	[Serializable]
	public class PatchExecutionException : PatchException {
		public PatchExecutionException() {
		}

		public PatchExecutionException(SerializationInfo info, StreamingContext c)
			: base(info, c) {
		}

		public PatchExecutionException(string message, Exception innerException)
			: base(message, innerException) {
		}

		public PatchExecutionException(string message)
			: base(message) {
		}
	}
}