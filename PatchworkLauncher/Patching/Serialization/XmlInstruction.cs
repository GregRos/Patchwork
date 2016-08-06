using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Mono.Cecil;
using Patchwork;

namespace PatchworkLauncher {
	public class XmlInstruction {
		public string Name {
			get;
			set;
		}

		public string PatchPath {
			get;
			set;
		}

		[XmlAttribute]
		public bool IsEnabled {
			get;
			set;
		}

		public static XmlInstruction FromInstruction(PatchInstruction instr) {
			return new XmlInstruction() {
				IsEnabled = instr.IsEnabled,
				Name = instr.Patch.PatchInfo.PatchName,
				PatchPath = instr.PatchLocation
			};
		}
	}


}
