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
}