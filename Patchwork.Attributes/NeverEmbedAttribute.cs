using System;
using System.Collections.Generic;
using System.Text;

namespace Patchwork.Attributes {
	/// <summary>
	/// Specifies that this attribute should never be embedded into patched assemblies.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class NeverEmbedAttribute : Attribute {
		internal NeverEmbedAttribute() {
			
		}
	}
}
