using System;
using System.Xml.Serialization;

namespace PatchworkLauncher {
	internal class PatchSequenceHistory {
		public DateTime StartDate {
			get;
			set;
		}

		public DateTime EndDate {
			get;
			set;
		}

		[XmlArray("PatchInstructions")]
		[XmlArrayItem("PatchInstruction")]
		public PatchInstructionHistory[] PatchInstructionsHistory {
			get;
			set;
		} 
	}
}