using System;

namespace Patchwork.Utility.Binding {
	public class EventRaised : NotificationMode {

		public EventRaised(string eventName) {
			EventName = eventName;
		}

		public string EventName {
			get;
		}

		public override void Subscribe(object target, IBindable bindable) {
			var type = target.GetType();
			var eventInfo = type.GetEvent(EventName);
			EventHandler eventHandler = null;
			eventHandler = new EventHandler((o, e) => {
				if (bindable.IsDisposed) {
					eventInfo.RemoveEventHandler(target, eventHandler);
					return;
				}
				bindable.NotifyChange();
			});
			eventInfo.AddEventHandler(target, eventHandler);
		}
	}
}