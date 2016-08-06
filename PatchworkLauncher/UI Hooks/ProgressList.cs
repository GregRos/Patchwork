using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Patchwork.Utility {
	public class ProgressList : ObservableCollection<ProgressObject> {
		private ProgressObject _root;

		public ProgressList(ProgressObject root) {
			Root = root;
		}

		public ProgressObject Root {
			get {
				return _root;
			}
			private set {

				_root = value;
				Add(_root);
			}
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
			if (e.OldItems != null) {
				foreach (var item in e.OldItems.Cast<ProgressObject>()) {

				}
			}
			if (e.NewItems != null) {
				foreach (var item in e.NewItems.Cast<ProgressObject>()) {
					var index = IndexOf(item);
					item.Child.HasChanged += x => OnProgressChanged(index, item);
				}
			}

			base.OnCollectionChanged(e);
		}

		private void OnProgressChanged(int nesting, object sender) {
			var obj = (ProgressObject) sender;
			var child = obj.Child.Value;
			while (child != null) {
				if (this.Count > nesting + 1) {
					this[nesting + 1] = child;
				} else {
					this.Add(child);
				}
				child = child.Child.Value;
				nesting++;
			}
			var n = this.Count;
			for (int i = nesting + 1; i < n; i++) {
				RemoveAt(this.Count - 1);
			}
		}
	}
}