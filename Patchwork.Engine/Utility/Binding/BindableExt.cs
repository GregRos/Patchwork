using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Patchwork.Utility.Binding {
	public static class BindableExt {

		public static Binding<T> ToBinding<T>(this IBindable<T> bv, BindingMode mode = BindingMode.TwoWay) {
			return new Binding<T>(bv, mode);
		}
		public static IBindable<IList<T>> ToBindable<T>(this IList<T> list) {
			return Bindable.List(list);
		}

		public static IBindable<TOut> Convert<TIn, TOut>(this IBindable<TIn> original, Func<TIn, TOut> convertOut = null,
			Func<TOut, TIn> convertIn = null) {
			return new ConvertingBindable<TIn,TOut>(original, convertIn, convertOut);
		}

		public static IBindable<T> WithDispatcher<T>(this IBindable<T> bindable, Action<Action> dispatcher) {
			return new DispatchingBindable<T>(bindable, dispatcher);
		}

		public static IBindable<TValue> Bind<TTarget, TValue>(this TTarget obj, Expression<Func<TTarget, TValue>> memberExpr,
			NotificationMode mode = null) {
			var accessor = MemberAccessor.FromChainedMemberExpression(memberExpr.Body);
			accessor.RootObject = obj;
			return new MemberBindable<TValue>(accessor, mode);
		}

	}
}