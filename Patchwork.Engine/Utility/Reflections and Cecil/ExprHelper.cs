using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Patchwork.Engine.Utility {
	internal static class ExprHelper {
		private static MethodInfo GetMethod(Expression expr) {
			if (!(expr is MethodCallExpression)) {
				throw new ArgumentException("Expected MethodCallExpression.");
			}
			var methodCallExpr = (MethodCallExpression) expr;
			return methodCallExpr.Method;
		}
		public static MethodInfo GetMethod<T>(Expression<Func<T>> methodCall) {
			return GetMethod(methodCall.Body);
		}

		public static MethodInfo GetMethod(Expression<Action> methodCall) {
			return GetMethod(methodCall.Body);
		} 
	}
}
