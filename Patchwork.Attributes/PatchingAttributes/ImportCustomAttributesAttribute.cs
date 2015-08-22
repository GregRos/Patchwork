using System;
using System.Collections.Generic;

namespace Patchwork.Attributes {
	/// <summary>
	/// Specifies that Patchwork should import non-patching attributes on this module or assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Module | AttributeTargets.Assembly)]
	public class ImportCustomAttributesAttribute : PatchingAttribute {
		public IEnumerable<object> AttributeTypes {
			get;
			private set;
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="attributeTypes">The types of the attributes to import.</param>
		public ImportCustomAttributesAttribute(params object[] attributeTypes) {
			AttributeTypes = attributeTypes;

		}
	}
}
