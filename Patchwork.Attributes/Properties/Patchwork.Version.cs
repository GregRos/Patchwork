using System.ComponentModel;

// ReSharper disable CheckNamespace

namespace Patchwork.Attributes {
	/// <summary>
	/// Provides version information for Patchwork and its related assemblies.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[NewType(true)] //although this is an explicit member, we want to make it behave like an implicit one.
	public static class PatchworkVersion {
		/// <summary>
		/// Patchwork version string.
		/// </summary>
		public const string Version = "0.5.9.0";
	}
}