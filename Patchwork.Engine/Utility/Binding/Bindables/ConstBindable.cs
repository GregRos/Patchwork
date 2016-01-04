namespace Patchwork.Utility.Binding {
	internal class ConstBindable<T> : BindableBase<T> {

		private readonly T _value;

		public ConstBindable(T value) {
			_value = value;
		}

		public override T Value {
			get {
				return _value;
			}
			set {
				throw Errors.Bindable_is_readonly();
			}
		}

		public override bool CanSet => false;
	}
}