using System;

namespace Patchwork.Attributes.AutoPatching {
	/// <summary>
	/// You must decorate your <see cref="IPatchInfo"/>-implementing class with this attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class PatchInfoAttribute : Attribute {

	}


}
