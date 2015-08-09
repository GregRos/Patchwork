using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Mono.Cecil;
using Patchwork.Attributes;
using Patchwork.Utility;

namespace Patchwork
{		

	public partial class AssemblyPatcher
	{
		private CustomAttributeArgument CopyCustomAttributeArg(CustomAttributeArgument yourArgument) {
			return new CustomAttributeArgument(FixTypeReference(yourArgument.Type), yourArgument.Value);
		}

		/// <summary>
		/// Copies the custom attributes, excluding any attributes from a patching assembly that haven't been declared
		/// and Patchwork attributes. Note that this can only be used after all types and methods have been declared.
		/// </summary>
		/// <param name="targetMember">The target member.</param>
		/// <param name="yourMember">Your member, the source of the attributes.</param>
		private void CopyCustomAttributes(ICustomAttributeProvider targetMember, ICustomAttributeProvider yourMember) {
			var filteredAttrs =
				from attr in yourMember.CustomAttributes
				let attrType = attr.AttributeType.Resolve()
				let attrAssembly = attrType.Module.Assembly
				where !attrAssembly.Name.Name.Contains("Patchwork")
				&& (!attrAssembly.IsPatchingAssembly() || attrType.Resolve().HasCustomAttribute<NewTypeAttribute>())
				select attr;

			foreach (var yourAttr in filteredAttrs) {
				var targetAttr = new CustomAttribute(FixMethodReference(yourAttr.Constructor));
				targetAttr.ConstructorArguments.AddRange(yourAttr.ConstructorArguments.Select(CopyCustomAttributeArg));
				var targetFieldArgs =
					from nArg in targetAttr.Fields
					let arg = CopyCustomAttributeArg(nArg.Argument)
					select new CustomAttributeNamedArgument(nArg.Name, arg);

				var targetPropArgs =
					from nArg in targetAttr.Properties
					let arg = CopyCustomAttributeArg(nArg.Argument)
					select new CustomAttributeNamedArgument(nArg.Name, arg);

				targetAttr.Fields.AddRange(targetFieldArgs);
				targetAttr.Properties.AddRange(targetPropArgs);
				targetMember.CustomAttributes.Add(targetAttr);
			}
		}

		private void AutoModifyProperty(TypeDefinition targetType, MemberActionAttribute propActionAttr,
			PropertyDefinition yourProp) {
			Log_modifying_member("property", yourProp);
			var modifiesMemberAttr = propActionAttr as ModifiesMemberAttribute;
			var newMemberAttr = propActionAttr as NewMemberAttribute;
			string targetPropName;
			ModificationScope scope;

			if (modifiesMemberAttr != null) {
				targetPropName = modifiesMemberAttr.MemberName ?? yourProp.Name;
				scope = modifiesMemberAttr.Scope;
			} else if (newMemberAttr != null) {
				targetPropName = yourProp.Name;
				scope = ModificationScope.All;
			} else {
				throw Errors.Unknown_action_attribute(propActionAttr);
			}
			var targetProp = targetType.GetProperty(targetPropName,
				yourProp.Parameters.Select(x => x.ParameterType));
			if (targetProp == null) {
				throw Errors.Missing_member("property", yourProp, targetPropName);
			}
			
			if ((scope & ModificationScope.CustomAttributes) != 0) {
				CopyCustomAttributes(targetProp, yourProp);
				for (int i = 0; i < yourProp.Parameters.Count; i++) {
					CopyCustomAttributes(targetProp.Parameters[i], yourProp.Parameters[i]);
				}
			}
			if ((scope & ModificationScope.Body) != 0) {
				targetProp.GetMethod = yourProp.GetMethod != null ? FixMethodReference(yourProp.GetMethod).Resolve() : null;
				targetProp.SetMethod = yourProp.SetMethod != null ? FixMethodReference(yourProp.SetMethod).Resolve() : null;
				targetProp.OtherMethods.Clear();
				if (yourProp.HasOtherMethods) {
					//I have absolutely NO idea what this is used for
					foreach (var otherMethod in yourProp.OtherMethods) {
						targetProp.OtherMethods.Add(FixMethodReference(otherMethod).Resolve());
					}
				}
			}
		}

		private void AutoModifyField(TypeDefinition targetType, MemberActionAttribute fieldActionAttr,
			FieldDefinition yourField) {
			Log_modifying_member("field", yourField);
			(fieldActionAttr != null).AssertTrue();
			var asModifies = fieldActionAttr as ModifiesMemberAttribute;
			string targetFieldName = null;
			ModificationScope scope;
			if (asModifies != null) {
				targetFieldName = asModifies.MemberName ?? yourField.Name;
				scope = asModifies.Scope;
			} else if (fieldActionAttr is NewMemberAttribute) {
				targetFieldName = yourField.Name;
				scope = ModificationScope.All;
			} else {
				throw Errors.Unknown_action_attribute(fieldActionAttr);
			}
			var targetField = targetType.GetField(targetFieldName);
			if (targetField == null) {
				throw Errors.Missing_member("field", yourField, targetFieldName);
			}
			if ((scope & ModificationScope.Accessibility) != 0) {
				targetField.SetAccessibility(yourField.GetAccessbility());
			}
			if ((scope & ModificationScope.CustomAttributes) != 0) {
				CopyCustomAttributes(targetField, yourField);
			}
			if ((scope & ModificationScope.Body) != 0) {
				targetField.InitialValue = yourField.InitialValue; //dunno what this is used for
				targetField.Constant = yourField.Constant;
			}
		}

		private void AutoModifyMethod(TypeDefinition targetType, MethodDefinition yourMethod,
			MemberActionAttribute memberAction) {
			Log_modifying_member("method", yourMethod);
			var bodySource = yourMethod;
			var insertAttribute = yourMethod.GetCustomAttribute<DuplicatesBodyAttribute>();
			if (insertAttribute != null) {
				//Note that the source type is resolved using yourMethod's module, which uses a different IMetadataResolver, 
				//and thus will resolve the method from the target, unmodified assembly.

				var importSourceType = insertAttribute.SourceType != null
					? yourMethod.Module.Import((TypeReference) insertAttribute.SourceType)
					: yourMethod.Module.Import(targetType);

				var importMethod = importSourceType.Resolve().GetMethods(insertAttribute.MethodName,
					yourMethod.Parameters.Select(x => x.ParameterType)).SingleOrDefault();

				var others =
					importSourceType.Resolve().Methods.Where(x => x.Name == insertAttribute.MethodName).ToArray();

				if (importMethod == null) {
					throw Errors.Missing_member("method", yourMethod, insertAttribute.MethodName);
				}

				bodySource = importMethod;
			}
			var modifiesMemberAttr = memberAction as ModifiesMemberAttribute;
			var newMemberAttr = memberAction as NewMemberAttribute;
			ModificationScope scope;
			string targetMethodName;
			
			if (modifiesMemberAttr != null) {
				targetMethodName = modifiesMemberAttr.MemberName ?? yourMethod.Name;
				scope = modifiesMemberAttr.Scope;
				
			} else if (newMemberAttr != null) {
				targetMethodName = yourMethod.Name;
				scope = ModificationScope.All;
			} else {
				throw Errors.Unknown_action_attribute(memberAction);
			}
			var targetMethod =
				targetType.GetMethods(targetMethodName, yourMethod.Parameters.Select(x => x.ParameterType)).FirstOrDefault();

			if (targetMethod == null) {
				throw Errors.Missing_member("method", yourMethod, targetMethodName);
			}
			if (modifiesMemberAttr != null && targetMethod.IsAbstract && (scope & ModificationScope.Body) != 0) {
				throw Errors.Invalid_member("method", yourMethod, targetMethod.FullName,
					"You cannot modify the body of an abstract method.");
			}

			// in general, this hsould be scope & ~ModificationScope.Body,
			// but I've found that overwriting it twice like this lets you catch errors that would otherwise be hidden...
			// once there is a more reliable method of checking that the IL is valid, it should be corrected, as it can also be a source for errors.
			
			//note that after these calls, yourMethod's Instructions will have mutated somewhat, so it will no longer work if written to disk as-is,
			//but the mutation doesn't affect using it for patching.
			ModifyMethod(targetMethod, yourMethod, scope, newMemberAttr != null); 
			ModifyMethod(targetMethod, bodySource, ModificationScope.Body & scope, false);
		}

		/// <summary>
		///     Patches the target method by overwriting some aspects with your method, such as: its body and accessibility. Also allows adding custom attributes.
		/// </summary>
		/// <param name="targetMethod">The target method.</param>
		/// <param name="yourMethod">Your method.</param>
		/// <param name="scope">The extent to which to modify the method..</param>
		
		private void ModifyMethod(MethodDefinition targetMethod, MethodDefinition yourMethod, ModificationScope scope, bool isNew) {
			if ((scope & ModificationScope.Accessibility) != 0) {
				targetMethod.SetAccessibility(yourMethod.GetAccessbility());
			}

			if (isNew) {
				//There is absolutely no point allowing you to modify the explicit overrides of existing methods.
				//I thought of adding an enum value, but it will just cause confusion.
				targetMethod.Overrides.Clear();
				foreach (var explicitOverride in yourMethod.Overrides) {
					targetMethod.Overrides.Add(FixMethodReference(explicitOverride));
				}
			}

			if ((scope & ModificationScope.CustomAttributes) != 0) {
				CopyCustomAttributes(targetMethod, yourMethod);
				CopyCustomAttributes(targetMethod.MethodReturnType, yourMethod.MethodReturnType);
				for (int i = 0; i < yourMethod.Parameters.Count; i++) {
					CopyCustomAttributes(targetMethod.Parameters[i], yourMethod.Parameters[i]);
				}
				for (int i = 0; i < yourMethod.GenericParameters.Count; i++) {
					CopyCustomAttributes(targetMethod.GenericParameters[i], yourMethod.GenericParameters[i]);
				}
			}

			if ((scope & ModificationScope.Body) == 0) {
				return;
			}

			if (yourMethod.Body != null) {
				targetMethod.Body.ExceptionHandlers.Clear();

				if (yourMethod.Body.HasExceptionHandlers) {
					foreach (var exhandlr in yourMethod.Body.ExceptionHandlers) {
						targetMethod.Body.ExceptionHandlers.Add(exhandlr);
					}

					foreach (var exhandlr in targetMethod.Body.ExceptionHandlers) {
						if (exhandlr.CatchType != null) {
							exhandlr.CatchType = FixTypeReference(exhandlr.CatchType);
						}
					}
				}
				targetMethod.Body.Variables.Clear();
				
				foreach (var def in yourMethod.Body.Variables) {
					def.VariableType = FixTypeReference(def.VariableType);
					targetMethod.Body.Variables.Add(def);
				}

				targetMethod.Body.Instructions.Clear();
				foreach (var yourInstruction in yourMethod.Body.Instructions) {
					var targetInstructions = FixCilInstruction(targetMethod, yourInstruction);
					targetMethod.Body.Instructions.Add(targetInstructions);
				}

			} else {
				//this happens in abstract methods and some others.
				targetMethod.Body = null;
			}

		}

		private void Log_modifying_member(string kind, MemberReference forMember) {
			Log.Debug("Modifying {0:l} for: {1:l}", kind, forMember.UserFriendlyName());
		}

		private void AutoModifyTypeDecleration(TypeDefinition yourType) {
			Log_modifying_member("type", yourType);
			var targetType = TargetAssembly.MainModule.GetType(yourType.GetPatchedTypeFullName());
			targetType.BaseType = yourType.BaseType == null ? null : FixTypeReference(yourType.BaseType);
			targetType.Interfaces.AddRange(yourType.Interfaces.Select(FixTypeReference));

			CopyCustomAttributes(targetType, yourType);
			for (int i = 0; i < yourType.GenericParameters.Count; i++) {
				CopyCustomAttributes(targetType.GenericParameters[i], yourType.GenericParameters[i]);
			}

		}
	}
}
