using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Patchwork;
using Patchwork.Attributes;

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
