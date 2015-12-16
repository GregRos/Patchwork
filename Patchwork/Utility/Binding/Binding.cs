using System;
using System.Diagnostics;

namespace Patchwork.Utility.Binding
{

	/// <summary>
	/// Represents a Binding for some Bindable, consisting of a binding source and a binding mode.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	
	public class Binding<T> {

		public Binding(IBindable<T> source, BindingMode mode = BindingMode.TwoWay) {
			Source = source;
			Mode = mode;
		}

		private bool _isUpdating;
		public IBindable<T> Source {
			get;
			set;
		}

		public BindingMode Mode {
			get;
		}

		public override string ToString() {
			return BindingString;
		}

		public string BindingString {
			get {
				string symbol = "";
				switch (Mode) {
					case BindingMode.IntoTarget:
						symbol = "<=";
						break;
					case BindingMode.FromTarget:
						symbol = "=>";
						break;
					case BindingMode.TwoWay:
						symbol = "<<=>";
						break;
					case BindingMode.TwoWayPrioritizeTarget:
						symbol = "<=>>";
						break;
					case BindingMode.Disabled:
						symbol = "<≠>";
						break;
				}

				var type = typeof(T).Name;
				return $"[{type}] '{Target.Name}' {symbol} '{Source.Name}'";
			}
		}

		protected virtual void OnChanged(IBindable<T> changedBinding) {
			if (_isUpdating) return;
			var wasSourceDisposed = (bool?) null;
			
			if (changedBinding == Target && (Mode & BindingMode.FromTarget) == 0) {
				return;
			}
			if (changedBinding == Source && (Mode & BindingMode.IntoTarget) == 0) {
				return;
			}
			_isUpdating = true;
			var isTarget = changedBinding.Equals(Target);
			var gettingFrom = isTarget ? "target" : "source";
			var puttingIn = !isTarget ? "target" : "source";
			var valueTarget = isTarget ? Source : Target;
			T value;
			try {
				value = changedBinding.Value;
			}
			catch (Exception ex) {
				if (ex is ObjectDisposedException || ex.InnerException is ObjectDisposedException) {
					Debug.WriteLine(
						$"In binding ({BindingString}), when the {gettingFrom} changed, tried to get the value but the object was disposed.");
					wasSourceDisposed = changedBinding != Target;
					//!!!! WARNING WARNING WARNING !!!! CODER DISCRETION IS ADVISED: 
					goto skipUpdate;
				} else throw;
			}
			try {
				valueTarget.Value = value;
			}
			catch (Exception ex) {
				if (ex is ObjectDisposedException || ex.InnerException is ObjectDisposedException) {
					Debug.WriteLine(
						$"In binding ({BindingString}), when the {gettingFrom} changed, tried to update the {puttingIn}'s value, but the object was disposed.");
					wasSourceDisposed = changedBinding != Target;
				} else throw;
			}
			skipUpdate:
			_isUpdating = false;

			switch (wasSourceDisposed) {
				case true:
                    Debug.WriteLine(
						$"In binding ({BindingString}), the source (rightmost) was disposed, so the binding will be scrapped.");
					Dispose();
					break;
				case false:
                    Debug.WriteLine(
						$"In binding ({BindingString}), the target (leftmost) was disposed, so the binding will be scrapped.");
					Dispose();
					break;
				case null:
					//provided for emphasis
					break;
			}
		}

		public bool IsDisposed {
			get;
			private set;
		}

		public void Dispose() {
			if (IsDisposed) return;
			IsDisposed = true;
			Source.HasChanged -= OnChanged;
			Target.HasChanged -= OnChanged;
		}

		public IBindable<T> Target {
			get;
			private set;
		}

		internal void Initialize(IBindable<T> target) {
			Target = target;
			Source.HasChanged += OnChanged;
			Target.HasChanged += OnChanged;
			switch (Mode) {
				case BindingMode.TwoWayPrioritizeTarget:
				case BindingMode.FromTarget:
					target.NotifyChange();
					break;
				case BindingMode.IntoTarget:
				case BindingMode.TwoWay:
					Source.NotifyChange();
					break;
				case BindingMode.Disabled:
					break;
			}
		}
	}


}
