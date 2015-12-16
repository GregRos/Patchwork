using System;
using Patchwork.Utility.Binding;

namespace Patchwork.Utility {
	public interface IProgressObject {
		IBindable<string> TaskTitle { get; }
		IBindable<string> TaskText { get; }
		IBindable<int> Total { get; }
		IBindable<int> Current { get; }
		IBindable<ProgressObject> Child { get; }
		event EventHandler Finished;
	}
}