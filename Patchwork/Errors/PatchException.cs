using System;

namespace Patchwork {
	/// <summary>
	/// Base class for all Patchwork-specific exceptions the library throws.
	/// </summary>
	public abstract class PatchException : ApplicationException{

		protected PatchException() {
		}

		protected PatchException(string message, Exception innerException) : base(message, innerException) {
		}

		protected PatchException(string message) : base(message) {
		}
	}
}