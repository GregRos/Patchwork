using System.ComponentModel;

// ReSharper disable CheckNamespace

namespace Patchwork.Attributes {
	/// <summary>
	/// Provides version information for Patchwork and its related assemblies.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class PatchworkVersion {
		/// <summary>
		/// Patchwork version string.
		/// </summary>
		public const string Version = "1.8.3";
	}
}