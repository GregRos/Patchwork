using System.Collections.Generic;
using Patchwork.Engine.Utility;

namespace Patchwork.Utility.Binding {
	public class AggregateMode : NotificationMode {
		private readonly IList<NotificationMode> _modes = new List<NotificationMode>();

		public AggregateMode(IEnumerable<NotificationMode> modes) {
			foreach (var mode in modes) {
				var asAggregate = mode as AggregateMode;
				if (asAggregate != null) {
					_modes.AddRange(asAggregate._modes);
				} else {
					_modes.Add(mode);
				}
			}
		}

		public override void Subscribe(object target, IBindable bindable) {
			_modes.ForEach(x => x.Subscribe(target, bindable));
		}
	}
}
