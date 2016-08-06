using System;

namespace Patchwork {
	/// <summary>
	///     Disables patching this element and any child elements. Can be applied to anything that can be patched. Overrides
	///     other attributes.
	/// </summary>
	/// <remarks>
	///     Note that this class does not descend from <see cref="PatchingAttribute" />
	/// </remarks>
	[AttributeUsage(AttributeTargets.All,Inherited = false)]
	[NeverEmbed]
	public class DisablePatchingAttribute : Attribute {

	}
}