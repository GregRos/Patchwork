using System;
using System.Collections.Generic;

namespace Patchwork.Utility.Binding {

	public static class Bindable {
		public static IBindable<IList<T>> List<T>(IList<T> inner) {
			return new ListBindable<T>(inner);
		}

		/// <summary>
		/// Creates a special storage locaiton wrapped in a Bindable, which supports change notification.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="initialValue">The initial value to which the storage location is initialized.</param>
		/// <returns></returns>
		public static IBindable<T> Variable<T>(T initialValue) {
			return new VariableBindable<T>(initialValue);
		}

		public static IBindable<T> Computed<T>(Func<T> function) {
			return new ComputedBindable<T>(function);
		}

		public static IBindable<T> Const<T>(T constant) {
			return new ConstBindable<T>(constant);
		}
	}
}