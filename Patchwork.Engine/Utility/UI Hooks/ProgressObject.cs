using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Patchwork.Utility.Binding;

namespace Patchwork.Utility {

	public class ProgressObject : IProgressObject {

		public ProgressObject() {
			Current.HasChanged += x => {
				if (Current.Value == Total.Value) OnFinished();
			};
			Total.HasChanged += x => {
				if (Current.Value == Total.Value) OnFinished();
			};

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