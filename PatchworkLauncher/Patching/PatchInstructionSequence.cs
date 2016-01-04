using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Patchwork;

namespace PatchworkLauncher {

	public class PatchGroup {
		public string TargetPath {
			get;
			set;
		}

		public IList<PatchInstruction> Instructions {
			get;
			set;
		}
	}

	public class PatchInstructionSequence {
		public BindingList<PatchInstruction> Instructions {
			get;
		} = new BindingList<PatchInstruction>();
	}
}