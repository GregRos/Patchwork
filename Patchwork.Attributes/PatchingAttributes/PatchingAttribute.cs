using System;

namespace Patchwork.Attributes {

	/// <summary>
	///     Parent class of all attributes related to patching (except for DisablePatching).
	/// </summary>
	public abstract class PatchingAttribute : Attribute {
		internal PatchingAttribute() {

		}
	}
}