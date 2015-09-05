using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Patchwork.Attributes;
using Patchwork.Utility;

namespace Patchwork
{		

	public partial class AssemblyPatcher
	{
		private CustomAttributeArgument CopyCustomAttributeArg(CustomAttributeArgument yourArgument) {
			return new CustomAttributeArgument(FixTypeReference(yourArgument.Type), yourArgument.Value);
		}

		private void CopyCustomAttributesByImportAttribute(ICustomAttributeProvider targetMember,
			ICustomAttributeProvider yourMember, ImportCustomAttributesAttribute importAttribute) {
			if (importAttribute == null) return;
			var legalAttributes = importAttribute.AttributeTypes.Cast<TypeReference>().Select(x => x.Name);
			Func<CustomAttribute, bool> importOnly = attr => legalAttributes.Contains(attr.AttributeType.Name);
			CopyCustomAttributes(targetMember, yourMember, importOnly);	
		}

		/// <summary>
		/// Copies the custom attributes, excluding any attributes from a patching assembly that haven't been declared
		/// and Patchwork attributes. Note that this can only be used after all types and methods have been declared.
		/// </summary>
		/// <param name="targetMember">The target member.</param>
		/// <param name="yourMember">Your member, the source of the attributes.</param>
		private void CopyCustomAttributes(ICustomAttributeProvider targetMember, ICustomAttributeProvider yourMember, Func<CustomAttribute, bool> filter = null) {
			filter = filter ?? (x => true);
			var filteredAttrs =
				from attr in yourMember.CustomAttributes
				let attrType = attr.AttributeType.Resolve()
				let attrAssembly = attrType.Module.Assembly
				where !attrAssembly.Name.Name.Contains("Patchwork")
				&& (!attrAssembly.IsPatchingAssembly() || attrType.Resolve().HasCustomAttribute<NewTypeAttribute>())
				&& filter(attr)
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
				throw Errors.Missing_member_in_attribute("property", yourProp, targetPropName);
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

		private void AutoModifyEvent(TypeDefinition targetType, MemberActionAttribute eventActionAttr,
			EventDefinition yourEvent) {
			Log_modifying_member("property", yourEvent);
			var modifiesMemberAttr = eventActionAttr as ModifiesMemberAttribute;
			var newMemberAttr = eventActionAttr as NewMemberAttribute;
			string targetEventName;
			ModificationScope scope;

			if (modifiesMemberAttr != null) {
				targetEventName = modifiesMemberAttr.MemberName ?? yourEvent.Name;
				scope = modifiesMemberAttr.Scope;
			} else if (newMemberAttr != null) {
				targetEventName = yourEvent.Name;
				scope = ModificationScope.All;
			} else {
				throw Errors.Unknown_action_attribute(eventActionAttr);
			}
			var targetEvent = targetType.GetEvent(targetEventName);
			if (targetEvent == null) {
				throw Errors.Missing_member_in_attribute("property", yourEvent, targetEventName);
			}
			
			if ((scope & ModificationScope.CustomAttributes) != 0) {
				CopyCustomAttributes(targetEvent, yourEvent);
			}
			if ((scope & ModificationScope.Body) != 0) {
				targetEvent.AddMethod = yourEvent.AddMethod != null ? FixMethodReference(yourEvent.AddMethod).Resolve() : null;
				targetEvent.RemoveMethod = yourEvent.RemoveMethod != null ? FixMethodReference(yourEvent.RemoveMethod).Resolve() : null;
				targetEvent.InvokeMethod = yourEvent.InvokeMethod != null ? FixMethodReference(yourEvent.InvokeMethod).Resolve() : null;
				targetEvent.OtherMethods.Clear();
				if (yourEvent.HasOtherMethods) {
					//I have absolutely NO idea what this is used for
					foreach (var otherMethod in yourEvent.OtherMethods) {
						targetEvent.OtherMethods.Add(FixMethodReference(otherMethod).Resolve());
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
				throw Errors.Missing_member_in_attribute("field", yourField, targetFieldName);
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
					yourMethod.Parameters.Select(x => x.ParameterType), yourMethod.ReturnType).SingleOrDefault();

				var others =
					importSourceType.Resolve().Methods.Where(x => x.Name == insertAttribute.MethodName).ToArray();

				if (importMethod == null) {
					throw Errors.Missing_member_in_attribute("method", yourMethod, insertAttribute.MethodName);
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
				targetType.GetMethods(targetMethodName, yourMethod.Parameters.Select(x => x.ParameterType), yourMethod.ReturnType).FirstOrDefault();

			if (targetMethod == null) {
				throw Errors.Missing_member_in_attribute("method", yourMethod, targetMethodName);
			}
			if (modifiesMemberAttr != null && targetMethod.IsAbstract && (scope & ModificationScope.Body) != 0) {
				throw Errors.Invalid_member("method", yourMethod, targetMethod.FullName,
					"You cannot modify the body of an abstract method.");
			}

			ModifyMethod(targetMethod, yourMethod, scope & ~ModificationScope.Body, newMemberAttr != null); 
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
				TransferMethodBody(targetMethod, yourMethod);
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
				var targetParam = targetType.GenericParameters[i];
				var yourParam = yourType.GenericParameters[i];
				CopyCustomAttributes(targetParam, yourParam);
				targetParam.Constraints.Clear();
				targetParam.Constraints.AddRange(yourParam.Constraints.Select(FixTypeReference));
			}
		}

		private static bool IsBreakingOpCode(OpCode code) {
			return code.EqualsAny(OpCodes.Leave, OpCodes.Leave_S, OpCodes.Ret, OpCodes.Rethrow, OpCodes.Throw, OpCodes.Endfinally,
				OpCodes.Endfilter);
		}

		/// <summary>
		/// Fixes the cil instruction. Currently mutates yourInstruction rather than returning a new instruction, because creating a new instruction creates a bugg that I don't understand.
		/// </summary>
		/// <param name="targetMethod">The target method.</param>
		/// <param name="yourMethod">Your instructions.</param>
		/// <returns>The return type is a sequence because instructions can sometimes be fixed to multiple instructions.</returns>
		private void TransferMethodBody(MethodDefinition targetMethod, MethodDefinition yourMethod) {
			targetMethod.Body.Instructions.Clear();
			var injectManual = yourMethod.GetCustomAttribute<PatchworkDebugRegisterAttribute>();
			FieldReference debugFieldRef = null;

			var concat = targetMethod.Module.GetMethod(() => String.Concat("", ""));
			var instructionEquiv = new Dictionary<Instruction, Instruction>();
			targetMethod.Body.InitLocals = yourMethod.Body.Variables.Count > 0;

			targetMethod.Body.Variables.Clear();
			
			foreach (var yourVar in yourMethod.Body.Variables) {
				
				var targetVarType = FixTypeReference(yourVar.VariableType);
				var targetVar = new VariableDefinition(yourVar.Name, targetVarType);
				targetMethod.Body.Variables.Add(targetVar);
			}
			var ilProcesser = targetMethod.Body.GetILProcessor();
			if (injectManual != null) {
				var debugDeclType = (TypeReference)injectManual.DeclaringType ?? targetMethod.DeclaringType;
				var debugMember = injectManual.DebugFieldName;
				debugFieldRef = debugDeclType.Resolve().GetField(debugMember);
				if (debugFieldRef == null) {
					throw Errors.Missing_member_in_attribute("field", yourMethod, debugMember);
				}
				ilProcesser.Emit(OpCodes.Ldstr, "");
				ilProcesser.Emit(OpCodes.Stsfld, debugFieldRef);
			}
			//branch instructions reference other instructions in the method body as branch targets.
			//in order to fix them, I will first have to reconstruct the other instructions in the body
			for (int i = 0; i < yourMethod.Body.Instructions.Count; i++) {
				var yourInstruction = yourMethod.Body.Instructions[i];
				var yourOperand = yourInstruction.Operand;
				//Note that properties or events are pure non-functional metadata, kind of like attributes.
				//They will never be directly referenced in a CIL instruction, though reflection is a different story of course.
				object targetOperand;
				OpCode targetOpcode = yourInstruction.OpCode;
				if (yourOperand is MethodReference) {
					var yourMethodOperand = (MethodReference) yourOperand;
					var memberAliasAttr = yourMethodOperand.Resolve().GetCustomAttribute<MemberAliasAttribute>();
					if (memberAliasAttr != null && targetOpcode.EqualsAny(OpCodes.Call, OpCodes.Callvirt)) {
						switch (memberAliasAttr.CallMode) {
							case AliasCallMode.NonVirtual:
								targetOpcode = OpCodes.Call;
								break;
							case AliasCallMode.Virtual:
								targetOpcode = OpCodes.Callvirt;
								break;
						}
					}
					//FixMethodReference also resolves the aliased method, so processing MemberAliasAttribute isn't done yet.
					var targetMethodRef = FixMethodReference(yourMethodOperand);
					targetOperand = targetMethodRef;
				} else if (yourOperand is TypeReference) {
					//includes references to type parameters
					var yourTypeRef = (TypeReference) yourOperand;
					targetOperand = FixTypeReference(yourTypeRef);
				} else if (yourOperand is FieldReference) {
					var yourFieldRef = (FieldReference) yourOperand;
					targetOperand = FixFieldReference(yourFieldRef);
				} else if (yourOperand is ParameterReference) {
					var yourParamRef = (ParameterReference) yourOperand;
					targetOperand = FixParamReference(targetMethod, yourParamRef);
				} else {
					targetOperand = yourOperand;
				}

				var targetInstruction = CecilHelper.CreateInstruction(yourInstruction.OpCode, targetOperand);
				targetInstruction.OpCode = targetOpcode;
				targetInstruction.Operand = targetOperand;
				targetInstruction.SequencePoint = yourInstruction.SequencePoint;
				
				if (injectManual != null) {
					targetInstruction.OpCode = SimplifyOpCode(targetInstruction.OpCode);	
				}
				var lastInstr = ilProcesser.Body.Instructions.LastOrDefault();
				if (yourInstruction.SequencePoint != null && injectManual != null && (lastInstr == null ||!IsBreakingOpCode(lastInstr.OpCode))) {
					var str = i == 0 ? "" : " ⇒ ";
					str += yourInstruction.SequencePoint.StartLine;
					ilProcesser.Emit(OpCodes.Ldsfld, debugFieldRef);
					ilProcesser.Emit(OpCodes.Ldstr, str);	
					ilProcesser.Emit(OpCodes.Call, concat);
					ilProcesser.Emit(OpCodes.Stsfld, debugFieldRef);
				}
				instructionEquiv[yourInstruction] = targetInstruction;
				ilProcesser.Append(targetInstruction);
			}
			targetMethod.Body.ExceptionHandlers.Clear();
			if (yourMethod.Body.HasExceptionHandlers) {
				var handlers =
					from exhandler in yourMethod.Body.ExceptionHandlers
					select new ExceptionHandler(exhandler.HandlerType) {
						CatchType = exhandler.CatchType == null ? null : FixTypeReference(exhandler.CatchType),
						HandlerStart = instructionEquiv[exhandler.HandlerStart],
						HandlerEnd = instructionEquiv[exhandler.HandlerEnd],
						TryStart = instructionEquiv[exhandler.TryStart],
						TryEnd = instructionEquiv[exhandler.TryEnd],
						FilterStart = exhandler.FilterStart == null ? null : instructionEquiv[exhandler.FilterStart]
					};
				
				foreach (var exhandlr in handlers) {
					targetMethod.Body.ExceptionHandlers.Add(exhandlr);
				}
			}
			
			
			foreach (var targetInstruction in ilProcesser.Body.Instructions) {
				var targetOperand = targetInstruction.Operand;
				if (targetOperand is Instruction) { //conditional branch instructions
					var asInstr = (Instruction) targetOperand;
					targetOperand = instructionEquiv[asInstr];
				}
				else if (targetOperand is Instruction[]) { //Switch instruction (jump table)
					var asInstrs = ((Instruction[]) targetOperand);
					var equivTargetInstrs = asInstrs.Select(instr => instructionEquiv[instr]).ToArray();
					targetOperand = equivTargetInstrs;
				}
				targetInstruction.Operand = targetOperand;
			}
		}
	}
}
