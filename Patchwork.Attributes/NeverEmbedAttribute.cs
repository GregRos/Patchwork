using System;

namespace Patchwork {
	/// <summary>
	/// A meta-attribute, placed on other attributes. Specifies that this attribute should never be embedded into patched assemblies.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class NeverEmbedAttribute : Attribute {
		internal NeverEmbedAttribute() {
			
		}
	}
}
