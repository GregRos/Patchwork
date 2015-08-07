using System;

namespace Patchwork {
	/// <summary>
	/// A reference to a member or other element could not be imported (corrected and injected into the target assembly).
	/// </summary>
	public class PatchImportException : PatchException {

		public PatchImportException() {
		}

		public PatchImportException(string message, Exception innerException) : base(message, innerException) {
		}

		public PatchImportException(string message) : base(message) {
		}
	}
}