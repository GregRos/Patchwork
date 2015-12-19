using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Patchwork.Utility {
	public static class ReflectHelper {
		public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider provider) {
			return provider.GetCustomAttributes(typeof (T), true).OfType<T>();
		}

		public static T GetCustomAttribute<T>(this ICustomAttributeProvider provider) {
			return provider.GetCustomAttributes<T>().SingleOrDefault();
		}

		public static T New<T>(this Type type) {
			return (T) Activator.CreateInstance(type);
		}

		public static ConstructorInfo GetConstructorEx(this Type type, BindingFlags flags, Type[] args) {
			return type.GetConstructor(flags, null, args, null);
		}

		private static IDictionary<object, string> _enumDescriptions = new Dictionary<object, string>(); 

		public static string GetEnumValueText<T>(this T value)
			where T : struct, IConvertible, IComparable, IFormattable  {
			if (!typeof (T).IsEnum) {
				throw new ArgumentException($"The type {nameof(T)} must be an Enum.", nameof(value));
			}
			var boxed = (object) value;
			if (_enumDescriptions.ContainsKey(boxed)) {
				return _enumDescriptions[boxed];
			}
			var statFields = typeof (T).GetFields(CommonBindingFlags.Everything);
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
