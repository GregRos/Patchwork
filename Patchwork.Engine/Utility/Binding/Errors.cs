using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patchwork.Utility.Binding {
	public static class Errors {

		public static InvalidOperationException Bindable_is_readonly() {
			return new InvalidOperationException("This Bindable is read-only.");
		}

		public static ArgumentException Convertion_bindable_must_have_conversion() {
			return new ArgumentException("A conversion bindable must supply at least one conversion.");
		}

		public static ArgumentException Underlying_bindable_changed() {
			return new ArgumentException("The underlying Bindable has changed, but this conversion bindable doesn't have an out converter.");
		}
	}
}
