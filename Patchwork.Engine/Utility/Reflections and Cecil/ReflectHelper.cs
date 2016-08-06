using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Patchwork.Engine.Utility {
	/// <summary>
	/// A helper class that provides extension methods for working with objects from the System.Reflection namespace.
	/// </summary>
	public static class ReflectHelper {
		/// <summary>
		/// Returns a sequence of custom attributes with the specified type
		/// </summary>
		/// <typeparam name="T">The type of attributes to return.</typeparam>
		/// <param name="provider">The attribute provider to inspect.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider provider) {
			return provider.GetCustomAttributes(typeof (T), true).OfType<T>();
		}

		/// <summary>
		/// Returns a single custom attribute of the specified type. Returns the first one.
		/// </summary>
		/// <typeparam name="T">The type of the attribute to return.</typeparam>
		/// <param name="provider"></param>
		/// <returns></returns>
		public static T GetCustomAttribute<T>(this ICustomAttributeProvider provider) {
			return provider.GetCustomAttributes<T>().SingleOrDefault();
		}

		/// <summary>
		/// Returns the constructor for the type matching the specified signature, using the specified binding flags.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="flags">The binding flags.</param>
		/// <param name="args">A set of Type objects treated as the signature of the constructor to find.</param>
		/// <returns></returns>
		public static ConstructorInfo GetConstructorEx(this Type type, BindingFlags flags, Type[] args) {
			return type.GetConstructor(flags, null, args, null);
		}

		private static readonly IDictionary<object, string> _enumDescriptions = new Dictionary<object, string>(); 

		/// <summary>
		/// Returns the text title of an Enum value, which is actually the content of its <see cref="DescriptionAttribute"/>, if any.
		/// </summary>
		/// <typeparam name="T">the type of the enum.</typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetEnumValueText<T>(this T value)
			where T : struct, IConvertible, IComparable, IFormattable  {
			if (!typeof (T).IsEnum) {
				throw new ArgumentException($"The type {nameof(T)} must be an Enum.", nameof(value));
			}
			var boxed = (object) value;
			if (_enumDescriptions.ContainsKey(boxed)) {
				return _enumDescriptions[boxed];
			}
			var statFields = typeof (T).GetFields(CommonBindingFlags.Everything & BindingFlags.Static);
			var attrAndValues =
				from field in statFields
				let descAttr = field.GetCustomAttribute<DescriptionAttribute>()
				let text = descAttr?.Description
				let fieldValue = field.GetValue(null)
				select new {
					field,
					text,
					fieldValue
				};

			foreach (var item in attrAndValues) {
				_enumDescriptions[item.fieldValue] = item.text;
			}
			return _enumDescriptions[boxed];
		}

	}
}
