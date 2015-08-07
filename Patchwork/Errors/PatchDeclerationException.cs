using System;

namespace Patchwork {
	/// <summary>
	/// One of the declerations in the patching assembly is invalid in the context of the target assembly.
	/// </summary>
	public class PatchDeclerationException : PatchException {

		public PatchDeclerationException() {
		}

		public PatchDeclerationException(string message, Exception innerException) : base(message, innerException) {
		}

		public PatchDeclerationException(string message) : base(message) {
		}
	}
}