using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Patchwork.Attributes {
	/// <summary>
	/// Put this on a field to encode it as a const with its literal value calculated at the time of patching. Generally not useful outside the library.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class EncodeAsLiteralAttribute : PatchingAttribute {

	}
}
