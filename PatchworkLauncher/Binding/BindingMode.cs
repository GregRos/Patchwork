using System;

namespace Patchwork.Utility.Binding {


	/// <summary>
	/// Represents the direction in which changes propogate in a binding.
	/// </summary>
	[Flags]
	
	public enum BindingMode {
		/// <summary>
		/// The binding is inactive. Changes do not propogate.
		/// </summary>
		Disabled = 0x0,
		/// <summary>
		/// Changes propogate to the binding target (the Bindable whose Binding you set) from the source.
		/// </summary>
		IntoTarget = 0x1,
		/// <summary>
		/// Changes propogate to the binding source (the Bindable referenced in the Binding) from the target.
		/// </summary>
		FromTarget = 0x2,
		/// <summary>
		/// Changes propogate in both directions.
		/// </summary>
		TwoWay = IntoTarget | FromTarget,

		TwoWayPrioritizeTarget = IntoTarget | FromTarget | 0x4
	}
}