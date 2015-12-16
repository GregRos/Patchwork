using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchworkLauncher {
	public class DisposingBindingList<T> : BindingList<T>
	where T : IDisposable{
		protected override void RemoveItem(int index) {
			this[index].Dispose();
			base.RemoveItem(index);
		}

	}
}
