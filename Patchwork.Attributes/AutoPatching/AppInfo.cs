using System;
using System.IO;

namespace Patchwork.AutoPatching {

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
		} = new long[0];

		/// <summary>
		/// The base or primary folder of the application.
		/// </summary>
		public DirectoryInfo BaseDirectory {
			get;
			set;
		}

		/// <summary>
		/// Returns the location of a file from which the icon can be retrieved. Different kinds of files are supported, including executables.
		/// </summary>
		public FileInfo IconLocation {
			get;
			set;
		} 
	}

}
