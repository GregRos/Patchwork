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
			@"Patchwork is a framework that allows you to easily and decleratively modify other assemblies.

To do this, you write a special patch assembly, decorating members with attributes that serve as patching instruction. Patchwork then inserts the code you wrote into the target assembly.

It's very easy to use. It has fairly robust error reporting and logging support, so you know when something goes wrong.

It's written for game modding in mind, but can work on any assembly. See the project website for more details.";

		public const string PatchworkAttributesDescription =
			@"This library contains the patching attributes for the Patchwork assembly modification framework. You must reference this library from your patch assembly.";
	}
}