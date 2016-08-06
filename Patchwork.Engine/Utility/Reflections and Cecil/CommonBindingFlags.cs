using System.Reflection;

namespace Patchwork.Engine.Utility {
	/// <summary>
	///     Commonly used combinations of the BindingFlags enum.
	/// </summary>
	public static class CommonBindingFlags {
		/// <summary>
		///     Instance, Static, Public, NonPublic
		/// </summary>
		public static BindingFlags Everything = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
			| BindingFlags.Instance;
	}
}