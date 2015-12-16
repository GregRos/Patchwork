using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patchwork.Utility.Binding {
	internal class LinkingBindable<T> : VariableBindable<T> {
		private readonly IBindable<T> _inner;

		public LinkingBindable(IBindable<T> inner) : base(inner.Value) {
			_inner = inner;
			this.Binding = inner.ToBinding(BindingMode.TwoWay);
		}

		public override bool CanGet => _inner.CanGet;

		public override bool CanSet => _inner.CanSet;

	}
}
