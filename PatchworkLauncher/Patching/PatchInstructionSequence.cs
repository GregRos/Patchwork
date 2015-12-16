using System.ComponentModel;
using Patchwork;

namespace PatchworkLauncher {
	public class PatchInstructionSequence {
		public BindingList<PatchInstruction> Instructions {
			get;
		} = new BindingList<PatchInstruction>();
	}
}