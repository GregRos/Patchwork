using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Patchwork.History;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;

namespace Patchwork.Engine.Utility {
	/// <summary>
	/// Helper and extension methods for working with attributes, including instantiating attribute instances from their Cecil metadata.
	/// </summary>
	public static class CustomAttributeHelper {
		private static readonly Dictionary<ICustomAttributeProvider, List<object>> _attributeCache =
			new Dictionary<ICustomAttributeProvider, List<object>>();

		/// <summary>
		///     Determines whether the custom attribute provider has the right custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="memberDef">The member definition.</param>
		/// <returns></returns>
		public static bool HasCustomAttribute<T>(this ICustomAttributeProvider memberDef) {
			return memberDef.GetCustomAttributes<T>().Any();
		}

		/// <summary>
		///     Determines whether this is a patching assembly. Normally, if it has PatchingAssemblyAttribute.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns></returns>
		internal static bool IsPatchingAssembly(this AssemblyDefinition assembly) {
			return assembly.HasCustomAttribute<PatchAssemblyAttribute>();
		}

		private static IEnumerable<object> GetAllCustomAttributes(ICustomAttributeProvider provider) {
			if (_attributeCache.ContainsKey(provider)) {
				return _attributeCache[provider];
			}
			var attributes =
				from attr in provider.CustomAttributes
				where attr.AttributeType.FullName.ContainsAny("Patchwork", "System")
				select attr.TryConstructAttribute();
			_attributeCache[provider] = attributes.ToList();
			return _attributeCache[provider];
		}

		/// <summary>
		///     Gets the custom attributes. However, it can fail in some cases, so use it with care.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="provider">The provider.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider provider) {
			//this is a bit more complicated than you'd think. If we try to load the type itself, we'll get an error about dependencies.
			//loading in a ReflectionOnly context will create problems of their own.
			//so we're passively reading the attributes using Cecil, without loading anything, until we find the one we want
			//then we just instantiate another copy, using the same constructor parameters.
			return GetAllCustomAttributes(provider).OfType<T>();
		}

		/// <summary>
		///     Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="provider">The provider.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T>(this ICustomAttributeProvider provider) {
			return provider.GetCustomAttributes<T>().FirstOrDefault();
		}

		/// <summary>
		/// Adds a proper, loaded custom attribute to a Cecil custom attribute provider.
		/// </summary>
		/// <param name="provider">The Cecil custom attribute provider to which the attribute is to be attached.</param>
		/// <param name="module">The module in the context of which types and constructors will be imported.</param>
		/// <param name="attribType">The proper type of the attribute to add.</param>
		/// <param name="constructorArgs">The arguments to the attribute constructor. These attributes will be used to resolve the constructor in question.</param>
		public static void AddCustomAttribute(this ICustomAttributeProvider provider, ModuleDefinition module,
			Type attribType,
			params object[] constructorArgs) {
			var ctor =
				attribType.GetConstructor(CommonBindingFlags.Everything, null, constructorArgs.Select(x => x.GetType()).ToArray(),
					null);
			var imported = module.Import(ctor);
			var ctorArgs =
				from ctorParamArg in imported.Parameters.Zip(constructorArgs, Tuple.Create)
				let paramType = ctorParamArg.Item1.ParameterType
				let argValueAsType = ctorParamArg.Item2 as Type
				let argValue = argValueAsType == null ? ctorParamArg.Item2 : module.Import(argValueAsType)
				let argType = module.Import(ctorParamArg.Item2 is Type ? typeof(Type) : ctorParamArg.Item2.GetType())
				let ctorArg = paramType.MetadataType != MetadataType.Object ? ctorParamArg.Item2 : 
					new CustomAttributeArgument(argType, argValue)
				select new CustomAttributeArgument(paramType, ctorArg);

			var customAttr = new CustomAttribute(imported);
			customAttr.ConstructorArguments.AddRange(ctorArgs);
			provider.CustomAttributes.Add(customAttr);
		}

		internal static void AddPatchedByMemberAttribute(this IMemberDefinition targetMember, IMemberDefinition yourMember) {
			dynamic dynMemberDef = targetMember;
			//calling PatchedByMemberAttribute(string patchMemberName, object actionAttributeType);
			targetMember.AddCustomAttribute((ModuleDefinition)dynMemberDef.Module, typeof(PatchedByMemberAttribute), yourMember.FullName);
		}

		internal static void AddPatchedByTypeAttribute(this TypeDefinition targetType, TypeDefinition yourType) {
			
			//calling PatchedByTypeAttribute(object patchType, object actionAttributeType);
			targetType.AddCustomAttribute(targetType.Module, typeof(PatchedByTypeAttribute), yourType.FullName);
		}

		internal static void AddPatchedByAssemblyAttribute(this AssemblyDefinition targetAssembly,
			AssemblyDefinition yourAssembly, int index, string originalAssemblyMetadata) {
			targetAssembly.AddCustomAttribute(targetAssembly.MainModule, typeof(PatchedByAssemblyAttribute),index, yourAssembly.GetAssemblyMetadataString(), originalAssemblyMetadata, CecilHelper.PatchworkMetadataString);
		}

		private static object UnpackArgument(CustomAttributeArgument arg) {
			if (arg.Value is CustomAttributeArgument) {
				return CustomAttributeHelper.UnpackArgument((CustomAttributeArgument) arg.Value);
			}
			else if (arg.Value is CustomAttributeArgument[]) {
				var asArray = (CustomAttributeArgument[]) arg.Value;
				var unpacked = asArray.Select(CustomAttributeHelper.UnpackArgument).ToArray();
				return unpacked;
			}
			return arg.Value;
		}

		/// <summary>
		///     Constructs an attribute instance from its metadata.
		///     The method will fail if the attribute constructor has a System.Type parameter.
		/// </summary>
		/// <param name="customAttrData">The custom attribute data.</param>
		/// <returns></returns>
		private static object TryConstructAttribute(this CustomAttribute customAttrData) {
			var constructor = (ConstructorInfo) customAttrData.Constructor.Resolve().LoadMethod();
			var args = customAttrData.ConstructorArguments.Select(UnpackArgument).ToArray();
			if (constructor.GetParameters().Any(p => p.ParameterType == typeof (Type))) {
				//we cannot invoke this constructor.
				return null;
			}
			var ret = constructor.Invoke(args);
			return ret;
		}
	}
}