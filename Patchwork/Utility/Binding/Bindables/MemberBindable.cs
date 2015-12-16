using System.ComponentModel;

namespace Patchwork.Utility.Binding {
	public class MemberBindable<T> : BindableBase<T> {
		private readonly MemberAccessor _accessor;

		public MemberBindable(MemberAccessor access, NotificationMode mode = null) {
			_accessor = access;
			var instance = _accessor.RootObject;
			var changed = instance as INotifyPropertyChanged;
			if (changed != null) {
				changed.PropertyChanged += (sender, args) => {
					if (args.PropertyName == access.MemberChain[0].Name) {
						NotifyChange();
					}
				};
			}
			mode?.Subscribe(instance, this);
		}

		public override T Value {
			get {
				return (T)_accessor.InvokeGetter(_accessor.RootObject);
			}
			set {
				_accessor.InvokeSetter(_accessor.RootObject, value);
			}
		}
	}
}