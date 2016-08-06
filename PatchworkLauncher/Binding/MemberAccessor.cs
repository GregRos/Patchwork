using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Patchwork.Engine.Utility;

namespace Patchwork.Utility {

	public class MemberAccessor {

		public object RootObject {
			get;
			set;
		}

		public string RootObjectName {
			get;
			set;
		}

		public IList<MemberInfo> MemberChain {
			get;
		}


		public MemberAccessor(object rootObject, string rootObjectName, IEnumerable<MemberInfo> memberChain) {
			RootObject = rootObject;
			RootObjectName = rootObjectName;
			this.MemberChain = memberChain.ToList().AsReadOnly();
		}

		private static string GetStringForm(IEnumerable<MemberInfo> memberChain) {
			var names = new List<string>();
			foreach (var member in memberChain) {
				var asFieldInfo = member as FieldInfo;
				var asPropInfo = member as PropertyInfo;
				if (asFieldInfo?.IsStatic == true) {
					names.Add(asFieldInfo.DeclaringType.Name);
				} else if (asPropInfo?.GetMethod?.IsStatic == true || asPropInfo?.SetMethod?.IsStatic == true) {
					names.Add(asPropInfo.DeclaringType.Name);
				}
				if (asFieldInfo != null) {
					names.Add(asFieldInfo.Name);
				} else if (asPropInfo != null) {
					names.Add(asPropInfo.Name);
				}
			}
			return names.Join(".");
		}

		private static object InvokeGetter(object instance, IEnumerable<MemberInfo> memberChain) {
			object lastResult = instance;
			foreach (var member in memberChain) {
				var asPropertyInfo = member as PropertyInfo;
				if (asPropertyInfo != null) {
					lastResult = asPropertyInfo.GetValue(lastResult);
				}
				var asFieldInfo = member as FieldInfo;
				if (asFieldInfo != null) {
					lastResult = asFieldInfo.GetValue(lastResult);
				}
			}
			return lastResult;
		}

		private static void InvokeSetter(object instance, IEnumerable<MemberInfo> memberChain, object newValue) {
			var asList = memberChain.ToList();
			if (asList.Count == 0) {
				throw new ArgumentException("Cannot invoke setter with empty member chain.");
			}
			var targetInstance = InvokeGetter(instance, asList.SkipLast(1));
			var lastMember = asList.Last();
			var asPropertyInfo = lastMember as PropertyInfo;
			var asFieldInfo = lastMember as FieldInfo;
			asPropertyInfo?.SetValue(targetInstance, newValue);
			asFieldInfo?.SetValue(targetInstance, newValue);
		}

		public override string ToString() {
			var stringForm = GetStringForm(MemberChain);
			return RootObjectName == null ? stringForm : $"{RootObjectName}.{stringForm}";
		}

		public object InvokeGetter(object instance) {
			return InvokeGetter(instance, MemberChain);
		}

		public void InvokeSetter(object instance, object newValue) {
			InvokeSetter(instance, MemberChain, newValue);
		}

		/// <summary>
		/// Converts a chained member expression to a MemberAccessor with a getter and a setter.
		/// </summary>
		/// <param name="expr">A chained member access expression, of the form <c>X.Property.Field.Property</c>, where <c>X</c> can be a constant, parameter, or null (in the case of a static member).</param>
		/// <returns></returns>
		public static MemberAccessor FromChainedMemberExpression(Expression expr) {
			if (expr == null) {
				return null;
			}
			object baseInstance = null;
			string baseInstanceName = null;
			Expression curExpr = expr;
			var stack = new Stack<MemberInfo>();
			while (curExpr != null) {
				switch (curExpr.NodeType) {
					case ExpressionType.Constant:
						var asConstant = (ConstantExpression) curExpr;
						baseInstance = asConstant.Value;
						baseInstanceName = asConstant.Value?.ToString() ?? "(null)";
						goto breakLoop;
					case ExpressionType.Parameter:
						var asParameter = (ParameterExpression) curExpr;
						baseInstanceName = asParameter.Name;
						baseInstance = null;
						goto breakLoop;
					case ExpressionType.MemberAccess:
						var asMemberExpr = (MemberExpression) curExpr;
						if (!(asMemberExpr.Member is PropertyInfo || asMemberExpr.Member is FieldInfo)) {
							goto default;
						}
						stack.Push(asMemberExpr.Member);
						curExpr = asMemberExpr.Expression;
						break;
					default:
						throw new ArgumentException("Invalid ExpressionType");
				}
			}
			breakLoop:
			return new MemberAccessor(baseInstance, baseInstanceName, stack);
		}
	}
}