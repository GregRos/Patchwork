using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Patchwork;

namespace PatchworkLauncher {
	public class PatchInstruction {
		public PatchingManifest Patch {
			get;
			set;
		}

		public string Path {
			get;
			set;
		}

		public bool IsEnabled {
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
