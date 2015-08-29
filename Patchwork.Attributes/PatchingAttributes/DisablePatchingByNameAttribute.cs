using System;

namespace Patchwork.Attributes {

	[Flags]
	public enum PatchingTarget {
		Method,
		Property,
		Field,
		Event,
		Member = Method | Property | Field | Event,
		Type,
		All = Member | Type
	}

	/// <summary>
	/// This attribute disables patching types and/or methods in the assembly based on a regex you supply, which is matched against their full name (including namespace and declaring type).
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class DisablePatchingByNameAttribute : Attribute {
		public string Regex {
			get;
		}

		public PatchingTarget Target {
			get;
		}

		public DisablePatchingByNameAttribute(string regex, PatchingTarget target = ~PatchingTarget.Type) {
			Regex = regex;
			Target = target;

		}
	}
}
