using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PatchworkLauncher {
	internal class XmlInstruction {
		public string Name {
			get;
			set;
		}

		public string PatchLocation {
			get;
			set;
		}

		public bool IsEnabled {
			get;
			set;
		}
	}

	internal class XmlPreferences {
		
	}

	[XmlInclude(typeof(XmlInstruction))]
	[XmlInclude(typeof(XmlPatchHistory))]
	internal class XmlSettings {
		[XmlArray(nameof(Instructions))]
		[XmlArrayItem("Instruction")]
		public XmlInstruction[] Instructions {
			get;
			set;
		}

		public XmlFileHistory[] LastExecution {
			get;
			set;
		}

		public bool ReplaceFiles {
			get;
			set;
		}

		public static XmlSettings Default {
			get {
				return new XmlSettings() {
					Instructions = new XmlInstruction[0],
					LastExecution = null
				};
			}
		}

	}

	internal class XmlFileHistory {
		public XmlPatchHistory[] PatchHistory {
			get;
			set;
		}

		public string TargetPath {
			get;
			set;
		}

		public string BackupPath {
			get;
			set;
		}
	}

	internal class XmlPatchHistory {
		public string PatchLocation {
			get;
			set;
		}
	}
}
