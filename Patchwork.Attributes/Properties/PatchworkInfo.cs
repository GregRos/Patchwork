using System.ComponentModel;

// ReSharper disable CheckNamespace

namespace Patchwork.Attributes {
	/// <summary>
	/// Provides version information for Patchwork and its related assemblies.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class PatchworkInfo {
		/// <summary>
		/// Patchwork version string.
		/// </summary>
		public const string Version = "0.8.5";

		public const string PatchworkDescription =
			@"This is the patching engine of the Patchwork framework. 

Its method of distribution is currently being rethought while a new version is tested.";

		public const string PatchworkAttributesDescription =
			@"This library contains the patching attributes for the Patchwork assembly modification framework. You must reference this library from your patch assembly.";
	}
}