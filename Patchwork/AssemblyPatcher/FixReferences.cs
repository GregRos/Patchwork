using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Patchwork.Attributes;
using Patchwork.Utility;

namespace Patchwork
{
	public partial class AssemblyPatcher
	{
		/// <summary>
		/// Fixes a parameter reference 
		/// </summary>
		/// <param name="targetMethod"></param>
		/// <param name="yourParamRef"></param>
		/// <returns></returns>
		private ParameterDefinition FixParamReference(MethodDefinition targetMethod,
			ParameterReference yourParamRef) {
			var targetParam = targetMethod.Parameters[yourParamRef.Index];
			return targetParam;
		}

		public static OpCode SimplifyOpCode(OpCode toSimplify) {
			switch (toSimplify.Code) {
				case Code.Br_S:
					return OpCodes.Br;
				case Code.Brfalse_S:
					return OpCodes.Brfalse;
				case Code.Brtrue_S:
					return OpCodes.Brtrue;
				case Code.Beq_S:
					return OpCodes.Beq;
				case Code.Bge_S:
					return OpCodes.Bge;
				case Code.Bgt_S:
					return OpCodes.Bgt;
				case Code.Ble_S:
					return OpCodes.Ble;
				case Code.Blt_S:
					return OpCodes.Blt;
				case Code.Bne_Un_S:
					return OpCodes.Bne_Un;
				case Code.Bge_Un_S:
					return OpCodes.Bge_Un;
				case Code.Bgt_Un_S:
					return OpCodes.Bgt_Un;
				case Code.Ble_Un_S:
					return OpCodes.Ble_Un;
				case Code.Blt_Un_S:
					return OpCodes.Blt_Un;
				case Code.Leave_S:
					return OpCodes.Leave;
			}
			return toSimplify;
		}

		/*
		 * As a rule, all the Fix methods will call FixType to fix their types, while there is also one case where FixType
		 * will call FixMethod, which is when MVars are involved. However, to avoid infinite recursion, FixType⇒FixMethod calls
		 * only return partially fixed methods (Some of the types may not be fixed).
		 * 
		 * Also, most Fix methods are recursive, especially FixType.
		 */

		/// <summary>
		///     Fixes a type reference, possibly replacing a reference to a patching type from your assembly with a reference to
		///     the type being patched.
		/// </summary>
		/// <param name="yourTypeRef">The type reference.</param>
		/// <param name="methodContext"></param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">
		///     This method can only fix a reference to a patching type, or a reference to a
		///     nested type in a patching type.
		/// </exception>
		private TypeReference FixTypeReference(TypeReference yourTypeRef) {
			//Fixes references to YourAssembly::PatchingType to TargetAssembly::PatchedType
			//or YourAssembly::PatchingType::YourType to TargetAssembly::PatchedType::OrigType
			//also imports the type to the module.

			/*
			 * A type reference may be to the following "special" forms of a type:
			 * 0. A regular type.
			 * 1. A generic instantiation of the type, e.g. List<int> is an instantiation of List<_> 
			 * 2. A reference (basically, 'ref int', which is displayed int&)
			 * 3. An array of the type (in the IL, an array type T[] is a special form of T, rather than an instantiation of Array<_>)
			 * x. Pinned, pointer, and a few other uncommon things I don't want to mess with right now
			 * 
			 * Each is a kind of modified type that modifies a base type, exposed via ElementType. The ElementType could also be one of the above. For example we might have:
			 *		List<int>[], List<List<int>>, List<int>&, etc
			 *	
			 * We need to go down the "stack" and fix every modification, using recursion and calling ElementType. This is because we might have something like:
			 *		List<UserIntroducedType>[] ⇒ we need to recurse twice to fix it
			 *		UserIntroducedType[][][] ⇒ we need to recurse 3 times to fix it
			 *		
			 * Note that GetElementType() should NOT be called because it returns the bottom "pure" type, not 1 level below the current type.
			 * 
			 * Plus, we have references to type parameters ofc.
			 */
			
			if (yourTypeRef == null) {
				Log_called_to_fix_null("type");
				return null;
			}
			Log_fixing_reference("type", yourTypeRef);

			TypeReference targetTypeRef;
			
			var yourTypeDef = yourTypeRef.Resolve();
			if (yourTypeDef != null && yourTypeDef.IsDisablePatching()) {
				Log_trying_to_fix_disabled_reference("type", yourTypeRef);
			}
			TypeReference targetInnerTypeRef;
			switch (yourTypeRef.MetadataType) {
				case MetadataType.Array:
					//array of the type, e.g. int[]
					//kind of amusing arrays aren't generic instantiations of Array<T>, but rather
					//a special form of T itself (though this is implified by the syntax T[]).
					var yourArrayType = (ArrayType) yourTypeRef;
					targetInnerTypeRef = FixTypeReference(yourArrayType.ElementType);
					targetTypeRef = targetInnerTypeRef.MakeArrayType(yourArrayType.Rank);
					break;
				case MetadataType.ByReference:
					//ByRef type, e.g. int& or in C# "ref int". 
					//Note that IL allows for variables and fields to have a reference type (C# does not)
					var yourByRefType = (ByReferenceType) yourTypeRef;
					targetInnerTypeRef = FixTypeReference(yourByRefType.ElementType);
					targetTypeRef = targetInnerTypeRef.MakeByReferenceType();
					break;
				case MetadataType.GenericInstance:
					//fully instantiated generic type, like List<int>
					var asGenericInstanceType = (GenericInstanceType) yourTypeRef;
					var targetGenArguments = asGenericInstanceType.GenericArguments.Select<TypeReference, TypeReference>(FixTypeReference);
					targetInnerTypeRef = FixTypeReference(asGenericInstanceType.ElementType);
					targetTypeRef = targetInnerTypeRef.MakeGenericInstanceType(targetGenArguments.ToArray());
					break;
				case MetadataType.MVar:
					//method's generic type parameter. We find the DeclaringMethod, and find its version in the target assembly.
					var asGenParam = (GenericParameter) yourTypeRef;
					//the following is dangeorus because it brings about mutual recursion FixMethod ⇔ FixType... 
					//the 'false' argument makes sure the recursion doesn't become infinite, as it allows for FixMethod
					//not to fix all the types in the signature. After all, we just need the generic parameters.
					var targetDeclaringMethod = FixMethodReference(asGenParam.DeclaringMethod, false); 
					targetTypeRef = targetDeclaringMethod.GenericParameters.Single(x => x.Name == asGenParam.Name);
					break;
				case MetadataType.Var:
					//type's generic type parameter
					var declaringType = yourTypeRef.DeclaringType;
					var index = declaringType.GenericParameters.IndexOf(x => x.Name == yourTypeRef.Name);
					var targetDeclaringType = FixTypeReference(declaringType);
					targetTypeRef = targetDeclaringType.GenericParameters[index];
					break;
				case MetadataType.Sentinel:  //interop: related to the varargs calling convention, "__arglist" in C#.
				case MetadataType.RequiredModifier: //interop: related to marshaling
				case MetadataType.OptionalModifier: //interop: related to marshaling	
				case MetadataType.Pinned: //interop: created using the 'fixed' keyword
				case MetadataType.FunctionPointer: //interop: naked function pointer
				case MetadataType.TypedByReference: //interop: related to the varargs calling convention, __typeref
				case MetadataType.Pointer: //interop: pointer
					throw Errors.Feature_not_supported("MetadataType not supported: {0}", yourTypeRef.MetadataType);
				default:
					//this is the stopping condition for the recursion, which is dealing with a normal type.
					if (!yourTypeDef.Module.Assembly.IsPatchingAssembly()) {
						//we assume any types that aren't from the patching assembly are safe to import directly.
						targetTypeRef = TargetAssembly.MainModule.Import(yourTypeDef);
					} else {
						//If the type is from a patching assembly.
						//note that even if this is a nested type inside another patching type, the patched type name must be the full name always.
						targetTypeRef = GetPatchedTypeByName(yourTypeDef);
					}
					if (targetTypeRef == null) {
						throw Errors.Could_not_resolve_reference("type", yourTypeRef);
					}
					
					break;
			}
			Log_fixed_reference("type", yourTypeRef, targetTypeRef);
			targetTypeRef.Module.Assembly.AssertEqual(TargetAssembly);
			
			return targetTypeRef;
		}

		/// <summary>
		/// This performs a more diligent Import-like operation. The standard Import method can sometimes fail unpredictably when generics are involved.
		/// Note that it's possible yourMethodRef will be mutated, so don't use it.
		/// </summary>
		/// <param name="yourMethodRef">A reference to your method.</param>
		/// <returns></returns>
		private MethodReference ManualImportMethod(MethodReference yourMethodRef) {
			//the following is required due to a workaround. 
			var newRef = yourMethodRef.IsGenericInstance ? yourMethodRef : yourMethodRef.MakeReference();
			
			foreach (var param in newRef.Parameters) {
				if (param.ParameterType.IsVarOrMVar()) continue; //also workaround, though I'm not sure if we need this anymore.
				param.ParameterType = FixTypeReference(param.ParameterType);
			}
			if (!newRef.ReturnType.IsVarOrMVar()) {
				newRef.ReturnType = FixTypeReference(yourMethodRef.ReturnType);		
			}
			
			return newRef;
		}

		/// <summary>
		///     Fixes the method reference.
		/// </summary>
		/// <param name="yourMethodRef">The method reference.</param>
		/// <param name="isntFixTypeCall">This parameter is sort of a hack that lets FixType call FixMethod to fix MVars, without infinite recursion. If set to false, it avoids fixing some types.</param>
		/// <returns></returns>
		/// <exception cref="Exception">Method isn't part of a patching type in this assembly...</exception>
		private MethodReference FixMethodReference(MethodReference yourMethodRef, bool isntFixTypeCall = true) {
			//Fixes reference like YourAssembly::PatchingClass::Method to TargetAssembly::PatchedClass::Method
			if (yourMethodRef == null) {
				Log_called_to_fix_null("method");
				return null;
			}
			Log_fixing_reference("method", yourMethodRef);
			Asserts.AssertTrue(yourMethodRef.DeclaringType != null);
			var yourMethodDef = yourMethodRef.Resolve();
			if (yourMethodDef.IsDisablePatching()) {
				Log_trying_to_fix_disabled_reference("method", yourMethodDef);
			}
			/*
			 * In comparison to types, method references to methods are pretty simple. There are only two kinds:
			 * 1. A regular method reference
			 * 2. A reference to an instantiated generic method, e.g. Create<int>
			 * 
			 * 3. As a special case, any method reference (assume non-generic) that has a generic DeclaringType.
			 */
			int hadGenericParams = yourMethodRef.GenericParameters.Count;
			MethodReference targetMethodRef;
			var memberAlias = yourMethodDef.GetCustomAttribute<MemberAliasAttribute>();
			if (yourMethodRef.IsGenericInstance) {
				var yourGeneric = (GenericInstanceMethod) yourMethodRef;
				var targetBaseMethod = FixMethodReference(yourGeneric.ElementMethod);
				var targetGenericInstMethod = new GenericInstanceMethod(targetBaseMethod);
				
				foreach (var arg in yourGeneric.GenericArguments) {
					var targetGenericArg = FixTypeReference(arg);
					targetGenericInstMethod.GenericArguments.Add(targetGenericArg);
				}
				targetMethodRef = targetGenericInstMethod;
				
			} else {	
				TypeReference typeToFix = yourMethodRef.DeclaringType;
				if (memberAlias != null && memberAlias.AliasedMemberDeclaringType != null) {
					typeToFix = (TypeReference)memberAlias.AliasedMemberDeclaringType;
				}
				var targetType = FixTypeReference(typeToFix);
				var targetBaseMethodDef = yourMethodRef;
				if (yourMethodDef.Module.Assembly.IsPatchingAssembly()) {
					//additional checking
					var methodName = memberAlias == null || memberAlias.AliasedMemberName == null ? yourMethodRef.Name : memberAlias.AliasedMemberName;
					var targetMethodDef =
						targetType.Resolve().GetMethods(methodName, yourMethodRef.Parameters.Select(x => x.ParameterType), yourMethodRef.ReturnType).SingleOrDefault
							();
					var debugOverloads =
						targetType.Resolve().Methods.Where(x => x.Name == yourMethodRef.Name).ToArray();
					if (targetMethodDef == null) {
						throw Errors.Could_not_resolve_reference("method", yourMethodRef);
					}
					targetBaseMethodDef = targetMethodDef;
				} else {
					targetBaseMethodDef = yourMethodRef;
				}
				var newMethodRef = targetBaseMethodDef.MakeReference();
				newMethodRef.DeclaringType = targetType;
				targetMethodRef = newMethodRef;
			}
			
			if (isntFixTypeCall) {
				targetMethodRef = ManualImportMethod(targetMethodRef);
			} 
			targetMethodRef = TargetAssembly.MainModule.Import(targetMethodRef); //for good measure...
			targetMethodRef.Module.Assembly.AssertEqual(TargetAssembly);
			Log_fixed_reference("method", yourMethodRef, targetMethodRef);
			return targetMethodRef;
		}

		private FieldReference FixFieldReference(FieldReference yourFieldRef) {
			if (yourFieldRef == null) {
				Log_called_to_fix_null("field");
				return null; 
			}
			Log_fixing_reference("field", yourFieldRef);
			Asserts.AssertTrue(yourFieldRef.DeclaringType != null);
			
			var yourFieldDef = yourFieldRef.Resolve();
			if (yourFieldDef.IsDisablePatching()) {
				Log_trying_to_fix_disabled_reference("field", yourFieldRef);
			}
			var targetType = FixTypeReference(yourFieldRef.DeclaringType);
			var targetBaseFieldDef = yourFieldRef;
			if (yourFieldDef.Module.Assembly.IsPatchingAssembly()) {
				//additional checking
				var targetFieldDef = targetType.Resolve().GetField(yourFieldDef.Name);
				if (targetFieldDef == null) {
					throw Errors.Could_not_resolve_reference("field", yourFieldRef);
				}
				targetBaseFieldDef = targetFieldDef;
			} else {
				//we assume that types that aren't in a patching assembly will never reference types in a patching assembly
				targetBaseFieldDef = yourFieldRef;
			}
			var newMethodRef = targetBaseFieldDef.MakeReference();
			newMethodRef.DeclaringType = targetType;
			newMethodRef.FieldType = FixTypeReference(newMethodRef.FieldType);
			var targetFieldRef = TargetAssembly.MainModule.Import(newMethodRef);

			Log_fixed_reference("field", yourFieldRef, targetFieldRef);
			targetFieldRef.Module.Assembly.AssertEqual(TargetAssembly);

			return targetFieldRef;
		}

		private void Log_trying_to_fix_disabled_reference(string kind, MemberReference badMemberRef) {
			Log.Warning("Trying to fix {0} reference to {1}, but it has the DisablePatching attribute.", kind, badMemberRef.UserFriendlyName());
		}

		private void Log_called_to_fix_null(string kind) {
			Log.Warning("Trying to fix {0} reference, but the reference was null. Fixing to null.", kind);
		}

		private void Log_fixing_reference(string kind, MemberReference badMemberRef) {
			Log.Verbose("Trying to fix {0} reference for: {1}", kind, badMemberRef.UserFriendlyName());
		}

		private void Log_fixed_reference(string kind, MemberReference oldMemberRef, MemberReference fixedMemberRef) {
			Log.Verbose("Fixed {0} reference: {1} ⇒ {2}", kind, oldMemberRef.UserFriendlyName(),  fixedMemberRef.UserFriendlyName());
		}

	}
}
