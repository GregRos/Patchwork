using System.Collections.Generic;
using System.Xml.Serialization;

namespace PatchworkLauncher {
	[XmlInclude(typeof(XmlPatchHistory))]
	public class XmlFileHistory {
		[XmlArray("PatchHistory")]
		[XmlArrayItem("PatchHistoryInstruction")]
		public List<XmlPatchHistory> PatchHistory {
			get;
			set;
		} = new List<XmlPatchHistory>();

		public string TargetPath {
			get;
			set;
		}

	}
}