using System;

namespace Patchwork.Attributes {
	/// <summary>
	///     Designates that the assembly contains patching types. Assemblies without this attribute cannot be used as patches.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public class PatchAssemblyAttribute : PatchingAttribute {
	}
}