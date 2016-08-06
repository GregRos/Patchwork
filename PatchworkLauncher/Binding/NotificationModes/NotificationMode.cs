using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Patchwork.Utility.Binding {

	public abstract class NotificationMode {
		public abstract void Subscribe(object target, IBindable bindable);

		public static NotificationMode Combine(params NotificationMode[] others) {
			return new AggregateMode(others);
		}

		public static NotificationMode operator +(NotificationMode a, NotificationMode b) {
			return Combine(a, b);
		}

	}


}

