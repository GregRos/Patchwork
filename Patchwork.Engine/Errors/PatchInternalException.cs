using System;

namespace Patchwork {
	/// <summary>
	/// Indicates an internal error in the program.
	/// </summary>
	public class PatchInternalException : PatchException {

		public PatchInternalException() {
		}

		public PatchInternalException(string message, Exception innerException) : base(message, innerException) {
		}

		public PatchInternalException(string message) : base(message) {
		}
	}
}