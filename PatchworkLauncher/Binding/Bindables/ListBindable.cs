using System.Collections.Generic;
using System.Collections.Specialized;
using Patchwork.Engine.Utility;

namespace Patchwork.Utility.Binding {
	internal class ListBindable<T> : BindableBase<IList<T>> {

		private readonly IList<T> _inner;

		public ListBindable(IList<T> inner) {
			_inner = inner;
			var asNotify = inner as INotifyCollectionChanged;
			if (asNotify != null) {
				asNotify.CollectionChanged += delegate {
					NotifyChange();
				};
			}
		}

		public override IList<T> Value {
			get {
				return _inner;
			}
			set {
				_inner.Clear();
				_inner.AddRange(value);
			}
		}
	}
}