using AttrT = System.AttributeTargets;
namespace Patchwork {
	/// <summary>
	/// Common attribute targets.
	/// </summary>
	internal static class CommonTargets {
		/// <summary>
		/// Class, Enum, Struct, Interface, and Delegate.
		/// </summary>
		public const AttrT Types =
			AttrT.Class
				| AttrT.Enum
				| AttrT.Struct
				| AttrT.Interface
				| AttrT.Delegate;

		/// <summary>
		/// Event, Property, Field, Method, and Constructor.
		/// </summary>
		public const AttrT Members =
			AttrT.Method
				| AttrT.Event
				| AttrT.Property
				| AttrT.Field
				| AttrT.Constructor;
	}

}
