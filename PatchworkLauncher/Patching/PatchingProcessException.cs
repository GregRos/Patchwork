using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchworkLauncher {

	public enum PatchProcessingStep {
		[Description("Unknown")]
		Unknown,
		[Description("Group patches by target file")]
		Grouping,
		[Description("Apply a specific patch")]
		ApplyingSpecificPatch,
		[Description("Write resulting assembly")]
		WritingToFile,
		[Description("Switch file to modded version")]
		PerformingSwitch,
	}

	public class PatchingProcessException : Exception {
		private string _targetFile;

		public PatchInstruction AssociatedInstruction {
			get;
			set;
		}

		public PatchGroup AssociatedPatchGroup {
			get;
			set;
		}

		public string TargetFile {
			get {
				return _targetFile ?? AssociatedPatchGroup?.TargetPath;
			}
			set {
				_targetFile = value;
			}
		}

		public PatchProcessingStep Step {
			get;
			set;
		}

		public PatchingProcessException(Exception innerException) : base(null, innerException) {
		}

		public PatchingProcessException(string message)
			: base(message) {
		}

		public PatchingProcessException(string message, Exception innerException)
			: base(message, innerException) {
		}
	}
}
