using System;
using System.Collections.Generic;
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

	}
}
