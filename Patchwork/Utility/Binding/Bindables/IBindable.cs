using System;

namespace Patchwork.Utility.Binding {

	public interface IBindable : IDisposable {

		event Action<IBindable> HasChanged;

		/// <summary>
		/// The Name of a Bindable is used for debugging and informational purposes.
		/// </summary>
		string Name {
			get;
		}

		bool IsDisposed {
			get;
		}

		void NotifyChange();

		object Value {
			get;
			set;
		}

		bool CanGet {
			get;
		}

		bool CanSet {
			get;
		}
	}

	public interface IBindable<T> : IBindable  {
		new event Action<IBindable<T>> HasChanged;

		new T Value {
			get;
			set;
		}

		Binding<T> Binding {
			get;
			set;
		}

		void SetRule(string name, Func<T, bool> rule);

		IBindable<bool> IsValid {
			get;
		}
	}
}