using System;
using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Utility.Binding {

	public abstract class BindableBase<T> : IBindable<T> {


		private readonly IDictionary<string, ValidationRule<T>> _validation = new Dictionary<string, ValidationRule<T>>();
		private Binding<T> _binding;

		private event Action<string> RuleChanged;

		private event Action<IBindable> _ifaceHasChanged;

		event Action<IBindable> IBindable.HasChanged {
			add {
				_ifaceHasChanged += value;
			}
			remove {
				_ifaceHasChanged -= value;
			}
		}

		public event Action<IBindable<T>> HasChanged;

		static BindableBase() {
		}

		protected BindableBase()
			: this(false) {

		}

		private BindableBase(bool alwaysValid) {
			if (alwaysValid) {
				IsValid = AlwaysTrueBindable.Instance;
			} else {
				IsValid = new ValidationBindable(this);

				RuleChanged += delegate {
					IsValid.NotifyChange();
				};
				HasChanged += delegate {
					IsValid.NotifyChange();
				};
				HasChanged += delegate {
					_ifaceHasChanged?.Invoke(this);
				};
			}
		}

		public string Name {
			get;
		}

		public bool IsDisposed {
			get;
			private set;
		}

		public virtual void NotifyChange() {
			HasChanged?.Invoke(this);
		}

		public Binding<T> Binding {
			get {
				return _binding;
			}
			set {
				_binding?.Dispose();
				_binding = value;
				_binding.Initialize(this);
			}
		}

		public abstract T Value {
			get;
			set;
		}

		public IEnumerable<ValidationRule<T>> Rules {
			get {
				return _validation.Values;
			}
		}

		public void SetRule(string name, Func<T, bool> isValid) {
			_validation[name] = new ValidationRule<T>(isValid, name);
			OnRuleChanged(name);
		}
		
		public virtual void Dispose() {
			IsDisposed = true;
			HasChanged = null;
			_ifaceHasChanged = null;
			_validation.Clear();
			Binding.Dispose();
		}

		protected virtual void OnRuleChanged(string obj) {
			RuleChanged?.Invoke(obj);
		}

		public virtual bool CanGet => true;

		public virtual bool CanSet => true;

		public IBindable<bool> IsValid {
			get;
			private set;
		}

		object IBindable.Value {
			get {
				return Value;
			}
			set {
				Value = (T) value;
			}
		}

		private class AlwaysTrueBindable : BindableBase<bool> {

			public static AlwaysTrueBindable Instance = new AlwaysTrueBindable();

			private AlwaysTrueBindable()
				: base(true) {
				IsValid = Instance;
			}

			public override bool Value {
				get {
					return true;
				}
				set {
					throw Errors.Bindable_is_readonly();
				}
			}

			public override bool CanSet => false;
		}

		private class ValidationBindable : BindableBase<bool> {
			private readonly BindableBase<T> _bindable;

			public ValidationBindable(BindableBase<T> bindable) : base(true) {
				_bindable = bindable;
			}

			public override bool Value {
				get {
					return _bindable.Rules.All(rule => rule.Validate(_bindable.Value));
				}
				set {
					throw Errors.Bindable_is_readonly();
				}
			}

			public override bool CanSet => false;
		}


	}

}