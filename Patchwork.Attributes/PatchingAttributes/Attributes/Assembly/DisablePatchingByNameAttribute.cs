using System;

namespace Patchwork {

	/// <summary>
	/// This attribute disables patching types and/or methods in the assembly based on a regex you supply, which is matched against their full name (including namespace and declaring type).
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[NeverEmbed]
	public class DisablePatchingByNameAttribute : Attribute {
		/// <summary>
		/// The regular expression used to match the name of the element to ignore.
		/// </summary>
		public string Regex {
			get;
		}
		/// <summary>
		/// Restricts the attribute to affect only elements of any of these types 
		/// </summary>
		public PatchingTarget Target {
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="regex">A regular expression to match the name of the element to ignore.</param>
		/// <param name="target">Restricts the attribute to affect only elements of any of these types.</param>
		public DisablePatchingByNameAttribute(string regex, PatchingTarget target = PatchingTarget.Member) {
			Regex = regex;
			Target = target;

		}
	}
}
