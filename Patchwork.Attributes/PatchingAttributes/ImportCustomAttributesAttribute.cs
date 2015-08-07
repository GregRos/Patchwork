using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Patchwork.Attributes {
	/// <summary>
	/// Specifies that Patchwork should import non-patching attributes on this module or assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Module | AttributeTargets.Assembly)]
	public class ImportCustomAttributesAttribute : PatchingAttribute {
		public ImportCustomAttributesAttribute() {
			
		}
	}
}
