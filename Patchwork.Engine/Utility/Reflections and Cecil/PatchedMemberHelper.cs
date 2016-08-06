using System;
using System.Runtime.CompilerServices;
using Mono.Cecil;

namespace Patchwork.Engine.Utility {
	internal static class PatchedMemberHelper {
		public static string GetPatchedMemberName(this IMemberDefinition yourMember) {
			return yourMember.GetCustomAttribute<ModifiesMemberAttribute>()?.MemberName ?? yourMember.Name;
		}

		/// <summary>
		///     If given a ref to a patching type, returns the type that it patches. Otherwise, returns null.
		/// </summary>
		/// <param name="typeRef">The type reference.</param>
		/// <returns></returns>
		internal static string GetPatchedTypeFullName(this TypeReference typeRef) {
			var typeActionAttr = typeRef.Resolve().GetCustomAttribute<TypeActionAttribute>();
			var newTypeAttr = typeActionAttr as NewTypeAttribute;
			var modTypeAttr = typeActionAttr as ModifiesTypeAttribute;
			if (typeActionAttr is NewTypeAttribute || typeActionAttr == null) {
				if (!typeRef.IsNested) {
					return typeRef.FullName;
				}
				var declaringType = typeRef.DeclaringType;
				declaringType.AssertUnequal(null);
				var decPatchedName = declaringType.GetPatchedTypeFullName();
				return decPatchedName + "/" + typeRef.Name;
			}
			var typeAttr = typeActionAttr as ModifiesTypeAttribute;
			if (typeAttr == null) {
				throw new NotSupportedException("Encountered an unknown TypeActionAttribute.");
			}
			return typeAttr.FullTypeName == "base" ? typeRef.Resolve().BaseType.FullName : typeAttr.FullTypeName;
		}

		internal static bool IsDisablePatching(this IMemberDefinition member) {
			if (member == null) { return false; }
			return member.HasCustomAttribute<DisablePatchingAttribute>()
				|| (member.DeclaringType != null && member.HasCustomAttribute<DisablePatchingAttribute>());
		}

		/// <summary>
		///     Determines whether the member was compiler generated.
		/// </summary>
		/// <param name="attrProvider">The attribute provider.</param>
		/// <returns></returns>
		internal static bool IsCompilerGenerated(this IMemberDefinition attrProvider) {

			return attrProvider.HasCustomAttribute<CompilerGeneratedAttribute>()
				|| (attrProvider.DeclaringType?.IsCompilerGenerated() == true);
		}
	}
}