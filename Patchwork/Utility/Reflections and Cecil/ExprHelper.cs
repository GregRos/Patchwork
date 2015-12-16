using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Patchwork.Utility {
	public static class ExprHelper {
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
