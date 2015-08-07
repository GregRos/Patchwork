using System;

namespace Patchwork.Attributes {
	/// <summary>
	///     Disables patching this element and any child elements. Can be applied to anything that can be patched. Overrides
	///     other attributes.
	/// </summary>
	/// <remarks>
	///     Note that this class does not descend from <see cref="PatchingAttribute" />
	/// </remarks>
	[AttributeUsage(
		AttributeTargets.Class
			| AttributeTargets.Enum
			| AttributeTargets.Method
			| AttributeTargets.Property
			| AttributeTargets.Field
			| AttributeTargets.Assembly
			| AttributeTargets.Struct)]
	public class DisablePatchingAttribute : Attribute {

	}
}