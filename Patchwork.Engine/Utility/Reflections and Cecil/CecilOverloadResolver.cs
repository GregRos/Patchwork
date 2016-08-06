using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil;

namespace Patchwork.Engine.Utility {
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
		/// <param name="signature"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyDefinition> GetProperties(this TypeDefinition typeDef, string name, IEnumerable<TypeReference> signature) {
			var typesList = signature.ToList();
			var props = 
				from prop in typeDef.Properties
				where prop.Name == name
					&& prop.Parameters.Count == typesList.Count()
					&& prop.Parameters.Select(x => x.ParameterType).Zip<TypeReference, TypeReference, bool>(typesList, IsOverloadMatch).All(x => x)
				select prop;

			return props;
		}

		/// <summary>
		/// Returns a property similar to another one.
		/// </summary>
		/// <param name="containingDef"></param>
		/// <param name="likeWhat"></param>
		/// <param name="altName"></param>
		/// <returns></returns>
		public static PropertyDefinition GetPropertyLike(this TypeDefinition containingDef, PropertyDefinition likeWhat, string altName = null) {
			return
				containingDef.GetProperties(altName ?? likeWhat.Name, likeWhat.Parameters.Select(x => x.ParameterType)).SingleOrDefault();
		}

		/// <summary>
		///    Returns a property on the type with the given name and signature.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <param name="name">The name.</param>
		/// <param name="signature">A sequence of type references taken to be the parameters of an indexer property.</param>
		/// <returns></returns>
		public static PropertyDefinition GetProperty(this TypeDefinition typeDef, string name, IEnumerable<TypeReference> signature) {
			return typeDef.GetProperties(name, signature).SingleOrDefault();
		}

		/// <summary>
		/// Imports the method referenced in an expression in the context of <paramref name="module"/>.
		/// </summary>
		/// <typeparam name="T">The return type of the method.</typeparam>
		/// <param name="module">The module into which to import the method as a reference.</param>
		/// <param name="expr">A method invocation expression meant to be supplied in lambda form, e.g. <c>() => 1.ToString()</c>. Otherwise, throws an exception.</param>
		/// <returns></returns>
		public static MethodReference GetMethodLike<T>(this ModuleDefinition module, Expression<Func<T>> expr) {
			return module.Import(ExprHelper.GetMethod(expr));
		}

		/// <summary>
		/// Imports the method referenced in an expression in the context of <paramref name="module"/>.
		/// </summary>
		/// <param name="module">The module on which to find the method.</param>
		/// <param name="expr">A method invocation expression meant to be supplied in lambda form. Otherwise, throws an exception.</param>
		/// <returns></returns>
		public static MethodReference GetMethodLike(this ModuleDefinition module, Expression<Action> expr) {
			return module.Import(ExprHelper.GetMethod(expr));
		}

		/// <summary>
		/// Returns a method on the type similar to the specified method.
		/// </summary>
		/// <param name="containingType">The type on which to resolve the method.</param>
		/// <param name="similarMethod">A reference to the similar method.</param>
		/// <param name="altName">Optionally, find a method similar to <paramref name="similarMethod"/>, but with this name instead.</param>
		/// <returns></returns>
		public static MethodDefinition GetMethodLike(this TypeDefinition containingType, MethodReference similarMethod,
			string altName = null) {
			return
				containingType.GetMethods(altName ?? similarMethod.Name, similarMethod.Parameters.Select(x => x.ParameterType),
					similarMethod.GenericParameters.Count, similarMethod.ReturnType)
					.SingleOrDefault();
		}

		/// <summary>
		/// This method only considers the return type of the method if its name is op_Explicit or op_Implicit.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <param name="genericArity"></param>
		/// <param name="returnType"></param>
		/// <returns></returns>
		public static IEnumerable<MethodDefinition> GetMethods(this TypeDefinition type, string methodName,
			IEnumerable<TypeReference> parameters, int genericArity, TypeReference returnType) {
			var sameName =
				from method in type.Methods
				where method.Name == methodName
				select method;

			parameters = parameters.ToList();
			var ignoreReturnType = !methodName.EqualsAny("op_Implicit", "op_Explicit");
			var sameSig =
				from method in sameName
				where method.Parameters.Count == parameters.Count()
				where parameters == null
					|| method.Parameters.Select(x => x.ParameterType).Zip<TypeReference, TypeReference, bool>(parameters, IsOverloadMatch).All(x => x)
				where ignoreReturnType || returnType == null || method.ReturnType.IsOverloadMatch(returnType)
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
		public static bool IsOverloadMatch(this TypeReference a, TypeReference b) {
			if (a.IsGenericParameter || b.IsGenericParameter) {
				//there are two kinds of generic parameters, Var and MVar (class/method).
				//they are different. otherwise, all gen params are the same.
				return a.MetadataType == b.MetadataType;
			}

			//this isn't really a comprehensive identity test. At some undetermined point in the future it will be fixed.
			//however, the problem cases are pretty rare.
			return a.FullName == b.FullName; // [a.MetadataType == b.MetadataType] this was deleted because apparently the types can match even though the MT is different.
		}

		/// <summary>
		/// Returns true if the type definition has a parent type with the full name of <paramref name="parentType"/>.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <param name="parentType">The full name of the parent type.</param>
		/// <returns></returns>
		public static bool IsOfType(this TypeDefinition typeDef, string parentType) {
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

		/// <summary>
		/// Returns true if the type reference is a generic type parameter (either of a type or of a method)
		/// </summary>
		/// <param name="typeRef"></param>
		/// <returns></returns>
		public static bool IsVarOrMVar(this TypeReference typeRef) {
			return typeRef.MetadataType == MetadataType.MVar || typeRef.MetadataType == MetadataType.Var;

		}
	}
}