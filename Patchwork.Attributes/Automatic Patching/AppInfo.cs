using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Patchwork.Attributes {

	/// <summary>
	/// A simple class that contains information about an application. Should be constructed through the <see cref="AppInfoFactory"/> 
	/// </summary>
	[Serializable]
	public sealed class AppInfo {
		/// <summary>
		/// The executable file of the application.
		/// </summary>
		public FileInfo Executable {
			get;
			set;
		}

		/// <summary>
		/// A string that describes the version of the application.
		/// </summary>
		public string AppVersion {
			get;
			set;
		}

		/// <summary>
		/// The display name of the application.
		/// </summary>
		public string AppName {
			get;
			set;
		}

		/// <summary>
		/// An array of error codes to ignore when/if running PEVerify on a patched file.
		/// </summary>
		public long[] IgnorePEVerifyErrors {
			get;
			set;
		}

		/// <summary>
		/// The base or primary folder of the application.
		/// </summary>
		public DirectoryInfo BaseDirectory {
			get;
			set;
		}
	}

}
