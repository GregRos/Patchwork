using System;

namespace Patchwork.Attributes {
	/// <summary>
	///     Designates that the assembly contains patching types. Assemblies without this attribute cannot be used as patches.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	[NeverEmbed]
	public class PatchAssemblyAttribute : PatchingAttribute {
		public object PatchExecutionType {
			get;
		}
		/// <summary>
		/// Constructs a new instance of the attribute.
		/// </summary>
		/// <param name="patchExecutionType">A type that inherits from PatchExecution and implements auto-execution related operations. Must define a default constructor.</param>
		public PatchAssemblyAttribute(object patchExecutionType = null) {
			PatchExecutionType = patchExecutionType;
		}
	}
}