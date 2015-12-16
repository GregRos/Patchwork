using System;
namespace Patchwork.Attributes {
	/// <summary>
	/// An enum consisting of all the valid code elements that can be decorated with patching attributes.
	/// </summary>
	[Flags]
	public enum PatchingTarget {
		/// <summary>
		/// Includes constructors.
		/// </summary>
		Method = 1 << 0,
		/// <summary>
		/// Includes indexers.
		/// </summary>
		Property = 1 << 1,
		Field = 1 << 2,
		Event = 1 << 3,
		/// <summary>
		/// Method, Property, Field, or Event
		/// </summary>
		Member = Method | Property | Field | Event,
		/// <summary>
		/// Interface, Class, Struct, Enum, and Delegate
		/// </summary>
		Type = 1 << 4,
		/// <summary>
		/// All patching targets.
		/// </summary>
		All = Member | Type
	}
}