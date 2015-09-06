using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patchwork {
	/// <summary>
	/// Specifies how much history to embed. History attributes can be used it identify assemblies, types, and members that have been patched, but cause a dependency on Patchwork.Attributes.
	/// </summary>
	[Flags]
	public enum HistorySetting {
		/// <summary>
		/// Embed no history. 
		/// </summary>
		NoHistory = 0,
		/// <summary>
		/// Embed patching attributes.
		/// </summary>
		PatchingAttributes = 1 << 0,
		/// <summary>
		/// Embed history attributes. 
		/// </summary>
		HistoryAttributes = 1 << 1,
		/// <summary>
		/// Embed both history and patching attributes.
		/// </summary>
		FullHistory = PatchingAttributes | HistoryAttributes

	}
}
