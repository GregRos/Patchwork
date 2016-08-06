using System;
using System.Runtime.Serialization;
#pragma warning disable 1591

namespace Patchwork.Engine {
	/// <summary>
	/// A reference to a member or other element could not be imported (corrected and injected into the target assembly).
	/// </summary>
	[Serializable]
	public class PatchImportException : PatchException {

		public PatchImportException() {
		}

		protected PatchImportException(SerializationInfo info, StreamingContext c)
			: base(info, c) {
		}

		public PatchImportException(string message, Exception innerException) : base(message, innerException) {
		}

		public PatchImportException(string message) : base(message) {
		}
	}
}