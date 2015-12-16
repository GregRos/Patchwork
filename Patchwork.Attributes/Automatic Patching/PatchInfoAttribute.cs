using System;
using System.Collections.Generic;
using System.Text;

namespace Patchwork.Attributes {
	/// <summary>
	/// You must decorate your <see cref="IPatchInfo"/>-implementing class with this attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class PatchInfoAttribute : Attribute {

	}


}
