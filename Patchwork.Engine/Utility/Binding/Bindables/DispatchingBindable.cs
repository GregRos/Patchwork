using System;

namespace Patchwork.Utility.Binding {
	internal class DispatchingBindable<T> : BindableBase<T> {
		public LinkingBindable<T> Link {
			get;
		}

		private Action<Action> Dispatcher {
			get;
		}

		public DispatchingBindable(IBindable<T> inner, Action<Action> dispatcher) {
			Link = new LinkingBindable<T>(inner);
			Dispatcher = dispatcher;
			Link.HasChanged += delegate {
				NotifyChange();
			};
		}

		public override T Value {
			get {
				T v = default(T);
				Dispatcher(() => v = Link.Value);
				return v;
			}
			set {
				Dispatcher(() => Link.Value = value);
			}
		}

		public override bool CanGet => Link.CanGet;

		public override bool CanSet => Link.CanSet;
	}
}