using System.ComponentModel;

namespace Patchwork.Utility.Binding {
	public class NotifyPropertyChanged : NotificationMode {
		public NotifyPropertyChanged(string propertyName) {
			PropertyName = propertyName;
		}

		public string PropertyName {
			get;
		}

		public override void Subscribe(object target, IBindable bindable) {
			var notifier = (INotifyPropertyChanged) target;
			PropertyChangedEventHandler notifierOnPropertyChanged = null;
			notifierOnPropertyChanged = (sender, e) => {
				if (e.PropertyName != PropertyName) return;
				if (bindable.IsDisposed) {
					notifier.PropertyChanged -= notifierOnPropertyChanged;
					return;
				}
				bindable.NotifyChange();
			};
			notifier.PropertyChanged += notifierOnPropertyChanged;
		}
	}
}