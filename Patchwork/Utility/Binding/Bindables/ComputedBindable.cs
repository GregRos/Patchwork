using System;

namespace Patchwork.Utility.Binding {
	internal class ComputedBindable<T> : BindableBase<T> {
		private readonly Func<T> _compute;

		public ComputedBindable(Func<T> getter) {
			_compute = getter;
		}

		public override T Value {
			get {
				return _compute();
			}
			set {
				throw Errors.Bindable_is_readonly();
			}
		}

		public override bool CanSet => false;
	}
}