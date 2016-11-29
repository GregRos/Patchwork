using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Patchwork.Engine;
using Patchwork.Utility.Binding;

namespace Patchwork.Utility {

	public class ProgressObject : IProgressObject {
		private class Wrapper : IProgressMonitor {
			private ProgressObject _inner;

			public Wrapper(ProgressObject inner) {
				_inner = inner;
			}

			public string TaskTitle {
				get {
					return _inner.TaskTitle.Value;
				}
				set {
					_inner.TaskTitle.Value = value;
				}
			}

			public string TaskText {
				get {
					return _inner.TaskText.Value;
				}
				set {
					_inner.TaskText.Value = value;
				}
			}

			public int Current {
				get {
					return _inner.Current.Value;
				}
				set {
					_inner.Current.Value = value;
				}
			}

			public int Total {
				get {
					return _inner.Total.Value;
				}
				set {
					_inner.Total.Value = value;
				}
			}
		}
		public ProgressObject() {
			Current.HasChanged += x => {
				if (Current.Value == Total.Value) OnFinished();
			};
			Total.HasChanged += x => {
				if (Current.Value == Total.Value) OnFinished();
			};

		}

		public IProgressMonitor ToMonitor() {
			return new Wrapper(this);
		}

		public IBindable<string> TaskTitle {
			get;
		} = Bindable.Variable("");

		public IBindable<string> TaskText {
			get;
		} = Bindable.Variable("");

		public IBindable<int> Total {
			get;
		} = Bindable.Variable(0);

		public IBindable<int> Current {
			get;
		} = Bindable.Variable(0);

		public IBindable<ProgressObject> Child {
			get;
		} = Bindable.Variable<ProgressObject>(null);

		public event EventHandler Finished;

		protected virtual void OnFinished() {
			var handler = Finished;
			handler?.Invoke(this, EventArgs.Empty);
		}



	}
}
