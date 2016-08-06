using System.IO;

namespace Patchwork.AutoPatching {
	/// <summary>
	/// A factory for constructing instances of the <see cref="AppInfo"/> class. The inheriting class must have a default constructor and must be decorated with <see cref="AppInfoFactoryAttribute"/>.
	/// </summary>
	public abstract class AppInfoFactory {
		/// <summary>
		/// Constructs a new instance of <see cref="AppInfo"/>.
		/// </summary>
		/// <param name="folderInfo">The primary folder of the application from which other information is deduced.</param>
		/// <returns></returns>
		public abstract AppInfo CreateInfo(DirectoryInfo folderInfo);
	}
}