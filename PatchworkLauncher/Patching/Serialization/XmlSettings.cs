using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Patchwork;
using Patchwork.Attributes;
using Patchwork.Utility;

namespace PatchworkLauncher {


	[XmlRoot("Preferences")]
	public class XmlPreferences {
		public bool AlwaysPatch {
			get;
			set;
		} = false;

		public bool DontCopyFiles {
			get;
			set;
		}

		public bool OpenLogAfterPatch {
			get;
			set;
		}
	}


	[XmlInclude(typeof(XmlInstruction))]
	[XmlRoot("Settings")]
	public class XmlSettings {
		[XmlArrayItem("Instruction")]
		public List<XmlInstruction> Instructions {
			get;
			set;
		} = new List<XmlInstruction>();

		public string BaseFolder {
			get;
			set;
		}

		public static XmlSettings FromInstructionSeq(IEnumerable<PatchInstruction> instrSeq) {
			return new XmlSettings() {
				Instructions = instrSeq?.Select(XmlInstruction.FromInstruction).ToList() ?? new List<XmlInstruction>()
			};
		}
	}
}