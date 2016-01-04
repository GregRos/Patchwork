namespace Patchwork.Utility.Binding {
	public class VariableBindable<T> : BindableBase<T> {
		private T _inner;

		public VariableBindable(T inner) {
			_inner = inner;
		}

		public override T Value {
			get {
				return _inner;
			}
			set {
				_inner = value;
				NotifyChange();
			}
		}
	}
}