using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Patchwork.Utility {
	/// <summary>
	/// Methods for resolving members within types and modules using their name and/or signature.
	/// </summary>
	public static class CecilOverloadResolver {
		/// <summary>
		///     Gets the field.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static FieldDefinition GetField(this TypeDefinition typeDef, string name) {
			return typeDef.Fields.SingleOrDefault(x => x.Name == name);
		}

		/// <summary>
		/// Returns the event with the specified name, or null.
		/// </summary>
		/// <param name="typeDef"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static EventDefinition GetEvent(this TypeDefinition typeDef, string name) {
			var vent = typeDef.Events.SingleOrDefault(e => e.Name == name);
			return vent;
		}

		/// <summary>
		///     Gets the property.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <param name="name">The name.</param>
		/// <param name="types"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyDefinition> GetProperties(this TypeDefinition typeDef, string name, IEnumerable<TypeReference> types) {
			var typesList = types.ToList();
			var props = 
				from prop in typeDef.Properties
				where prop.Name == name
					&& prop.Parameters.Count == typesList.Count()
					&& prop.Parameters.Select(x => x.ParameterType).Zip<TypeReference, TypeReference, bool>(typesList, IsOverloadEquiv).All(x => x)
				select prop;

			return props;
		}

		public static PropertyDefinition GetPropertyLike(this TypeDefinition typeDef, PropertyDefinition likeWhat, string altName = null) {
			return
				typeDef.GetProperties(altName ?? likeWhat.Name, likeWhat.Parameters.Select(x => x.ParameterType)).SingleOrDefault();
		}

		/// <summary>
		///     Gets the property.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <param name="name">The name.</param>
		/// <param name="types"></param>
		/// <returns></returns>
		public static PropertyDefinition GetProperty(this TypeDefinition typeDef, string name, IEnumerable<TypeReference> types) {
			return typeDef.GetProperties(name, types).SingleOrDefault();
		}

		public static MethodReference GetMethod<T>(this ModuleDefinition module, Expression<Func<T>> expr) {
			return module.Import(ExprHelper.GetMethod(expr));
		}

		public static MethodReference GetMethod(this ModuleDefinition module, Expression<Action> expr) {
			return module.Import(ExprHelper.GetMethod(expr));
		}

		internal static MethodDefinition GetMethodLike(this TypeDefinition type, MethodReference methodRef,
			string altName = null) {
			return
				type.GetMethods(altName ?? methodRef.Name, methodRef.Parameters.Select(x => x.ParameterType),
					methodRef.GenericParameters.Count, methodRef.ReturnType)
					.SingleOrDefault();
		}

		/// <summary>
		/// This method only considers the return type of the method if its name is op_Explicit or op_Implicit.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="methodName"></param>
		/// <param name="similarParams"></param>
		/// <param name="genericArity"></param>
		/// <param name="returnType"></param>
		/// <returns></returns>
		internal static IEnumerable<MethodDefinition> GetMethods(this TypeDefinition type, string methodName,
			IEnumerable<TypeReference> similarParams, int genericArity, TypeReference returnType) {
			var sameName =
				from method in type.Methods
				where method.Name == methodName
				select method;

			similarParams = similarParams.ToList();
			var ignoreReturnType = !methodName.EqualsAny("op_Implicit", "op_Explicit");
			var sameSig =
				from method in sameName
				where method.Parameters.Count == similarParams.Count()
				where similarParams == null
					|| method.Parameters.Select(x => x.ParameterType).Zip<TypeReference, TypeReference, bool>(similarParams, IsOverloadEquiv).All(x => x)
				where ignoreReturnType || returnType == null || method.ReturnType.IsOverloadEquiv(returnType)
				where method.GenericParameters.Count == genericArity
				select method;

			return sameSig.ToList();
		}

		/// <summary>
		///     Determines if the types are equivalent for the purpose of choosing overloads.
		/// </summary>
		/// <param name="a">Type a.</param>
		/// <param name="b">Type b.</param>
		/// <returns></returns>
		internal static bool IsOverloadEquiv(this TypeReference a, TypeReference b) {
			if (a.IsGenericParameter || b.IsGenericParameter) {
				//there are two kinds of generic parameters, Var and MVar (class/method).
				//they are different. otherwise, all gen params are the same.
				return a.MetadataType == b.MetadataType;
			}

			//this isn't really a comprehensive identity test. At some undetermined point in the future it will be fixed.
			//however, the problem cases are pretty rare.
			return a.FullName == b.FullName; // [a.MetadataType == b.MetadataType] this was deleted because apparently the types can match even though the MT is different.
		}

		internal static bool IsOfType(this TypeDefinition typeDef, string parentType) {
			var isActualType = typeDef.FullName == parentType;
			var hasInterface = typeDef.Interfaces.Any(intf => intf.FullName == parentType);
			var parentIsType = typeDef.BaseType?.Resolve().IsOfType(parentType) == true;
			return isActualType || hasInterface || parentIsType;
		}

		/// <summary>
		///     Finds a nested type with the specified local name in the given type. It only works for immediate descendants.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		/// <exception cref="System.Reflection.AmbiguousMatchException">More than one nested type matched the search criteria.</exception>
		/// <exception cref="System.MissingMemberException">Could not find a nested type with that name.</exception>
		public static TypeReference GetNestedType(this TypeReference typeDef, string name) {
			var ret = typeDef.Resolve().NestedTypes.Where(x => x.Name == name).Select(x => x.Resolve()).ToList();

			if (ret.Count > 1) {
				throw new AmbiguousMatchException("More than one nested type matched the search criteria.");
			}
			if (ret.Count == 0) {
				throw new MissingMemberException("Could not find a nested type with that name.");
			}
			return ret[0];
		}

		public static bool IsVarOrMVar(this TypeReference typeRef) {
			return typeRef.MetadataType == MetadataType.MVar || typeRef.MetadataType == MetadataType.Var;

		}
	}
}