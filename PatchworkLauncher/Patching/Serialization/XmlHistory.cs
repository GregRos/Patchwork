using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PatchworkLauncher {

	[XmlInclude(typeof(XmlFileHistory))]
	public class XmlHistory {
		[XmlArray("Files")]
		[XmlArrayItem("File")]
		public List<XmlFileHistory> Files {
			get;
			set;
		} = new List<XmlFileHistory>();

		public bool Success {
			get;
			set;
		}
	}
}
