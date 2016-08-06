using System.Linq;
using Mono.Cecil;

namespace Patchwork.Engine.Utility {
	/// <summary>
	/// Extension and helper methods for resolving user-friendly names of various code elements.
	/// </summary>
	public static class DisplayNameHelper {
		/// <summary>
		/// Returns the common name used for the member definition of the specified type.
		/// </summary>
		/// <typeparam name="T">The type of the member definition, e.g. <see cref="FieldDefinition"/>, <see cref="MethodDefinition"/>, etc. </typeparam>
		/// <returns></returns>
		public static string CommonNameForMemberDef<T>()
			where T : IMemberDefinition {
			var t = typeof (T);
			if (t == typeof (FieldDefinition)) {
				return "field";
			}
			if (t == typeof (MethodDefinition)) {
				return "method";
			}
			if (t == typeof (TypeDefinition)) {
				return "type";
			}
			if (t == typeof (PropertyDefinition)) {
				return "property";
			}
			if (t == typeof (EventDefinition)) {
				return "event";
			}
			return "unknown";
		}

		internal static string UserFriendlyNameDef(this IMemberDefinition memberDef) {
			return ((MemberReference) memberDef).UserFriendlyName();
		}

		/// <summary>
		///     Returns a user-friendly name for the reference.
		///     It's not as short as Name, but not as long as FullName.
		/// </summary>
		/// <param name="memberRef">The member reference.</param>
		/// <returns></returns>
		internal static string UserFriendlyName(this MemberReference memberRef) {
			if (memberRef is FieldReference) {
				return UserFriendlyName((FieldReference) memberRef);
			}
			if (memberRef is PropertyReference) {
				return UserFriendlyName((PropertyReference) memberRef);
			}
			if (memberRef is TypeReference) {
				return UserFriendlyName((TypeReference) memberRef, true);
			}
			if (memberRef is MethodReference) {
				return UserFriendlyName((MethodReference) memberRef);
			}
			return memberRef.FullName;
		}

		private static string UserFriendlyName(this FieldReference fRef) {
			var declaringType = fRef.DeclaringType.UserFriendlyName();
			var fieldType = fRef.FieldType.UserFriendlyName();
			return $"{fieldType} {declaringType}::{fRef.Name}";
		}

		private static string UserFriendlyName(this PropertyReference fRef) {
			var declaringType = fRef.DeclaringType.UserFriendlyName();
			var fieldType = fRef.PropertyType.UserFriendlyName();
			return $"{fieldType} {declaringType}::{fRef.Name}";
		}

		/// <summary>
		///     Returns a user-friendly name.
		/// </summary>
		/// <param name="typeRef">The type reference.</param>
		/// <param name="longForm">If true, returns a longer form of the name.</param>
		/// <returns></returns>
		private static string UserFriendlyName(this TypeReference typeRef, bool longForm = false) {
			string baseType;
			switch (typeRef.MetadataType) {
				case MetadataType.Array:
					var asArray = (ArrayType) typeRef;
					return UserFriendlyName(asArray.ElementType) + $"[{",".Replicate(asArray.Rank - 1)}]";
				case MetadataType.GenericInstance:
					var asInst = (GenericInstanceType) typeRef;
					baseType = UserFriendlyName(asInst.ElementType);
					var genericArgs = $"<{asInst.GenericArguments.Select(UserFriendlyName).Join(", ")}>";
					return baseType + genericArgs;
				case MetadataType.ByReference:
					var asRef = (ByReferenceType) typeRef;
					baseType = UserFriendlyName(asRef.ElementType);
					return "ref " + baseType;
				default:
					var name = !longForm ? typeRef.Name : typeRef.FullName;
					if (!longForm && typeRef.DeclaringType != null) {
						name = $"{typeRef.DeclaringType.Name}/{name}";
					}
					return name;
			}
		}

		private static string UserFriendlyName(this MethodReference mRef) {
			var returnType = mRef.ReturnType.UserFriendlyName();
			var paramTypes = mRef.Parameters.Select(x => x.ParameterType.UserFriendlyName()).Join(", ");
			var declaringType = mRef.DeclaringType.UserFriendlyName();
			var displayName = $"{returnType} {declaringType}::{mRef.Name}";
			if (mRef.IsGenericInstance) {
				var asGeneric = (GenericInstanceMethod) mRef;
				var sig = asGeneric.GenericArguments.Select(UserFriendlyName).Join(", ");
				displayName = $"{displayName}<{sig}>";
			}
			displayName = $"{displayName}({paramTypes})";
			return displayName;
		}
	}
}