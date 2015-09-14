using System;
using System.Collections.Generic;
using System.Text;

namespace Patchwork.Attributes {
	/// <summary>
	/// Specifies that the specified class is the PatchExecutionInfo class for this patch assembly. May only appear once per assembly, on a non-nested type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	internal class PatchExecutionInfoAttribute : PatchingAttribute {

	}
}
