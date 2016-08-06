using System;

namespace Patchwork {
	/// <summary>
	///     Designates that the assembly contains patching types. Assemblies without this attribute cannot be used as patches.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	[NeverEmbed]
	public class PatchAssemblyAttribute : PatchingAttribute {
		
	}
}