using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchworkLauncher {
	internal class DebugOptions {
		public bool AlwaysPatch {
			get;
			set;
		} = false;

		public bool DontRunProgram {
			get;
			set;
		} = false;

		public bool DontCopyFiles {
			get;
			set;
		}

		public bool OpenLogAfterPatch {
			get;
			set;
		}


		public static DebugOptions Default = new DebugOptions() {
			AlwaysPatch = true
		};

		

		private static DebugOptions _debugEnable = new DebugOptions() {
			AlwaysPatch = true,
			DontCopyFiles = true,
			DontRunProgram = false
		};
	}
}
