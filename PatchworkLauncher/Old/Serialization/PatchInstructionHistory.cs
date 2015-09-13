using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Patchwork;

namespace PatchworkLauncher {

	internal class PatchInstructionHistory {
		public string PatchLocation {
			get;
			set;
		}

		public string TargetLocation {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		[XmlAttribute]
		public bool IsEnabled {
			get;
			set;
		}
	}

}
