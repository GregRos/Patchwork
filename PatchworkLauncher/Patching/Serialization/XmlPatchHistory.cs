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
	}
}