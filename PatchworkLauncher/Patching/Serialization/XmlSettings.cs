using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Patchwork;
using Patchwork.Utility;

namespace PatchworkLauncher {


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