using System;
using System.Collections.Generic;
using System.Text;

namespace Patchwork.Attributes {

	/// <summary>
	/// Parent class of patching history attributes. These are used to indicate that a member, type, or assembly has been patched.
	/// </summary>
	[NewType(true)]
	public abstract class PatchingHistoryAttribute : Attribute {

	}

}
