namespace Patchwork {
	/// <summary>
	/// This used as a return value from methods that create new stuff. It is used in AssemblyPatcher.cs
	/// in order to remove items from further processing if the need arises.
	/// </summary>
	internal enum NewMemberStatus {
		/// <summary>
		/// Everything is A-okay.
		/// </summary>
		Continue,
		/// <summary>
		/// The last item wasn't created correctly. Remove it from further processing.
		/// </summary>
		InvalidItem
	}
}