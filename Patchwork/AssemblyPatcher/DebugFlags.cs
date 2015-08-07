using System;
using System.ComponentModel;

namespace Patchwork {
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Flags]
	public enum DebugFlags {
		None = 0 << 0,
		CreationOverwrites = 1 << 0,
	}
}