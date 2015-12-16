using System;

namespace Patchwork.Attributes {

	/// <summary>
	/// This attribute disables patching types and/or methods in the assembly based on a regex you supply, which is matched against their full name (including namespace and declaring type).
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[NeverEmbed]
	public class DisablePatchingByNameAttribute : Attribute {
		public string Regex {
			get;
		}

		public PatchingTarget Target {
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="regex">A regular expression to match the name of the element to ignore.</param>
		/// <param name="target">The PatchingTarget this attribute applies to.</param>
		public DisablePatchingByNameAttribute(string regex, PatchingTarget target = PatchingTarget.Member) {
			Regex = regex;
			Target = target;

		}
	}
}
