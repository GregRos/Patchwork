namespace PatchworkLauncher {
	public class XmlPatchHistory {

		public XmlPatchHistory() {
			
		}
		public XmlPatchHistory(string patchLocation) {
			PatchLocation = patchLocation;
		}

		public string PatchLocation {
			get;
			set;
		} = "";

		public static XmlPatchHistory FromInstruction(PatchInstruction instr) {
			return new XmlPatchHistory(instr.PatchLocation);
		}
	}
}