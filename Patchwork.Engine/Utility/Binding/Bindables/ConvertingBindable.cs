using System;

namespace Patchwork.Utility.Binding {
	internal class ConvertingBindable<TIn, TOut> : BindableBase<TOut> {

		private readonly Func<TIn, TOut> _convertOut;
		private readonly Func<TOut, TIn> _convertIn;

		private TOut _outValue;

		public ConvertingBindable(IBindable<TIn> inner, Func<TOut, TIn> convertIn = null, Func<TIn, TOut> convertOut = null) {
			Link = new LinkingBindable<TIn>(inner);
			if (convertIn == null && convertOut == null) {
				throw Errors.Convertion_bindable_must_have_conversion();
			}
			_convertIn = convertIn;
			_convertOut = convertOut;
			if (_convertOut != null) {
				inner.HasChanged += InnerHasChanged;
			}
		}

		public IBindable<TIn> Link {
			get;
		}

		public override TOut Value {
			get {
				return _convertOut == null ? _outValue : _convertOut(Link.Value);
			}
			set {
				if (_convertIn == null) {
					throw Errors.Bindable_is_readonly();
				}
				_outValue = value;
				Link.Value = _convertIn(value);
			}
		}

		public override void Dispose() {
			Link.Dispose();
			base.Dispose();
		}

		public override bool CanSet => _convertIn != null;
		
		private void InnerHasChanged(IBindable<TIn> x) {
			_outValue = _convertOut(x.Value);
			NotifyChange();
		}

			
	}
}