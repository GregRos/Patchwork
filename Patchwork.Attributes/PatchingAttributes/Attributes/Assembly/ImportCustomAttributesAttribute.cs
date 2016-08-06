using System;
using System.Collections.Generic;

namespace Patchwork {
	/// <summary>
	/// Specifies that Patchwork should import non-patching attributes on this module or assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Module | AttributeTargets.Assembly)]
	public class ImportCustomAttributesAttribute : AssemblyActionAttribute {
		/// <summary>
		/// The types of the attributes to import. In cecil, it will contain an array of TypeReference objects, but it is initialized with Type objects.
		/// </summary>
		public IEnumerable<object> AttributeTypes {
			get;
			private set;
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="attributeTypes">The types of the attributes to import. Should be an array of Type objects.</param>
		public ImportCustomAttributesAttribute(params object[] attributeTypes) {
			AttributeTypes = attributeTypes;

		}
	}
}
