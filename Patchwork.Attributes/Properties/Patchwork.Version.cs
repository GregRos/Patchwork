using System.ComponentModel;
using Patchwork.Attributes;

namespace Patchwork.Shared {
	/// <summary>
	/// Provides version information for Patchwork and its related assemblies.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	
	[NewType(true)] //although this is an explicit member, we want to make it behave like an explicit one.
	public static class PwVersion {
		public const string Version = "0.5.0.2";
	}
}