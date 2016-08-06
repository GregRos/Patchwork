using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Patchwork.Engine.Utility;
using CustomAttributeNamedArgument = Mono.Cecil.CustomAttributeNamedArgument;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Patchwork.Engine {
	public partial class AssemblyPatcher {
		private MethodReference _concat;

		private CustomAttributeArgument CopyCustomAttributeArg(CustomAttributeArgument yourArgument) {

			var type = FixTypeReference(yourArgument.Type);
			var value = yourArgument.Value;
			if (value is CustomAttributeArgument) {
				value = CopyCustomAttributeArg((CustomAttributeArgument) value);
			} else if (value is TypeReference) {
				value = FixTypeReference((TypeReference) value);
			}
			return new CustomAttributeArgument(type, value);
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
		/// <param name="filter"></param>
		private void CopyCustomAttributes(ICustomAttributeProvider targetMember, ICustomAttributeProvider yourMember,
			Func<CustomAttribute, bool> filter = null) {
			filter = filter ?? (x => true);
			var filteredAttrs =
				from attr in yourMember.CustomAttributes
				let attrType = attr.AttributeType.Resolve()
				let attrAssembly = attrType.Module.Assembly
				let isFromPatchworkAttributes = attrType.Module.Assembly.FullName == typeof (PatchworkInfo).Assembly.FullName
				let isFromPatchworkOther = attrType.Module.Assembly.FullName == typeof (AssemblyPatcher).Assembly.FullName
				//attributes that are in Patchwork but not Patchwork.Attributes are never included
				where !isFromPatchworkOther
				//attributes from Patchwork.Attributes may be included depending on whether History is enabled.
				where EmbedHistory || !isFromPatchworkAttributes
				//attributes specifically designated NeverEmbed should not be embedded.
				where
					attrType.CustomAttributes.All(
						attrOnAttr => attrOnAttr.AttributeType.FullName != typeof (NeverEmbedAttribute).FullName)
				//attributes from a patching assembly are only included if they have the NewType attribute
				where !attrAssembly.IsPatchingAssembly() || attrType.Resolve().HasCustomAttribute<NewTypeAttribute>()
				//also, apply this custom filter:
				where filter(attr)
				select attr;

			foreach (var yourAttr in filteredAttrs) {
				var targetAttr = new CustomAttribute(FixMethodReference(yourAttr.Constructor));
				targetAttr.ConstructorArguments.AddRange(yourAttr.ConstructorArguments.Select(CopyCustomAttributeArg));
				var targetFieldArgs =
					from nArg in yourAttr.Fields
					let arg = CopyCustomAttributeArg(nArg.Argument)
					
					select new CustomAttributeNamedArgument(nArg.Name, arg);

				var targetPropArgs =
					from nArg in yourAttr.Properties
					let arg = CopyCustomAttributeArg(nArg.Argument)
					select new CustomAttributeNamedArgument(nArg.Name, arg);

				targetAttr.Fields.AddRange(targetFieldArgs);
				targetAttr.Properties.AddRange(targetPropArgs);
				targetMember.CustomAttributes.Add(targetAttr);
			}
		}

		private void ModifyProperty(MemberActionAttribute propActionAttr,
			PropertyDefinition yourProp, PropertyDefinition targetProp) {
			Log_modifying_member("property", yourProp);
			ModificationScope scope = propActionAttr.Scope;
			var attrFilter = AttrFilter(scope);
			CopyCustomAttributes(targetProp, yourProp, attrFilter);
			for (int i = 0; i < yourProp.Parameters.Count; i++) {
				CopyCustomAttributes(targetProp.Parameters[i], yourProp.Parameters[i], attrFilter);
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

		private void ModifyEvent(MemberActionAttribute eventActionAttr,
			EventDefinition yourEvent, EventDefinition targetEvent) {
			Log_modifying_member("property", yourEvent);
			ModificationScope scope = eventActionAttr.Scope;
			var attrFilter = AttrFilter(scope);
			CopyCustomAttributes(targetEvent, yourEvent, attrFilter);
			if ((scope & ModificationScope.Body) != 0) {
				targetEvent.AddMethod = yourEvent.AddMethod != null ? FixMethodReference(yourEvent.AddMethod).Resolve() : null;
				targetEvent.RemoveMethod = yourEvent.RemoveMethod != null
					? FixMethodReference(yourEvent.RemoveMethod).Resolve() : null;
				targetEvent.InvokeMethod = yourEvent.InvokeMethod != null
					? FixMethodReference(yourEvent.InvokeMethod).Resolve() : null;
				targetEvent.OtherMethods.Clear();
				if (yourEvent.HasOtherMethods) {
					//I have absolutely NO idea what this is used for
					foreach (var otherMethod in yourEvent.OtherMethods) {
						targetEvent.OtherMethods.Add(FixMethodReference(otherMethod).Resolve());
					}
				}
			}

		}

		private void ModifyField(MemberActionAttribute fieldActionAttr,
			FieldDefinition yourField, FieldDefinition targetField) {
			Log_modifying_member("field", yourField);
			(fieldActionAttr != null).AssertTrue();
			ModificationScope scope = fieldActionAttr.Scope;
			if ((scope & ModificationScope.Accessibility) != 0) {
				targetField.SetAccessibility(yourField.GetAccessbility());
			}
			var attrFilter = AttrFilter(scope);
			CopyCustomAttributes(targetField, yourField, attrFilter);
			if ((scope & ModificationScope.Body) != 0) {
				targetField.InitialValue = yourField.InitialValue; //dunno what this is used for
				targetField.Constant = yourField.Constant;
				targetField.HasConstant = yourField.HasConstant;
			}
			var toggleAttributesAttr = yourField.GetCustomAttribute<ToggleFieldAttributes>();
			var toggleValue = toggleAttributesAttr?.Attributes ?? 0;
			targetField.Attributes ^= (FieldAttributes) toggleValue;
		}

		private MethodDefinition GetBodySource(TypeDefinition targetType, MethodDefinition yourMethod,
			DuplicatesBodyAttribute insertAttribute) {
			//Note that the source type is resolved using yourMethod's module, which uses a different IMetadataResolver, 
			//and thus will resolve the method from the target, unmodified assembly.

			var importSourceType = insertAttribute.SourceType != null
				? yourMethod.Module.Import((TypeReference) insertAttribute.SourceType)
				: yourMethod.Module.Import(targetType);

			var importMethod = importSourceType.Resolve().GetMethodLike(yourMethod, insertAttribute.MethodName);

			var others =
				importSourceType.Resolve().Methods.Where(x => x.Name == insertAttribute.MethodName).ToArray();

			if (importMethod == null) {
				throw Errors.Missing_member_in_attribute("method", yourMethod, insertAttribute.MethodName);
			}
			return importMethod;
		}

		private void ModifyMethod(TypeDefinition targetType, MethodDefinition yourMethod,
			MemberActionAttribute memberAction, MethodDefinition targetMethod) {
			Log_modifying_member("method", yourMethod);
			var insertAttribute = yourMethod.GetCustomAttribute<DuplicatesBodyAttribute>();
			var bodySource = insertAttribute == null ? yourMethod : GetBodySource(targetType, yourMethod, insertAttribute);
			ModificationScope scope = memberAction.Scope;
			if (scope.HasFlag(AdvancedModificationScope.ExplicitOverrides)) {
				targetMethod.Overrides.Clear();
				foreach (var explicitOverride in yourMethod.Overrides) {
					targetMethod.Overrides.Add(FixMethodReference(explicitOverride));
				}
			}

			var attrFilter = AttrFilter(scope);
			CopyCustomAttributes(targetMethod, yourMethod, attrFilter);
			CopyCustomAttributes(targetMethod.MethodReturnType, yourMethod.MethodReturnType, attrFilter);
			for (int i = 0; i < yourMethod.Parameters.Count; i++) {
				CopyCustomAttributes(targetMethod.Parameters[i], yourMethod.Parameters[i], attrFilter);
			}
			for (int i = 0; i < yourMethod.GenericParameters.Count; i++) {
				CopyCustomAttributes(targetMethod.GenericParameters[i], yourMethod.GenericParameters[i], attrFilter);
			}

			if ((scope & ModificationScope.Accessibility) != 0) {
				targetMethod.SetAccessibility(yourMethod.GetAccessbility());
			}

			if (scope.HasFlag(ModificationScope.Body)) {
				if (yourMethod.Body != null) {
					TransferMethodBody(targetMethod, bodySource);
				} else {
					//this happens in abstract methods and some others.
					targetMethod.Body = null;
				}
			}
			if (EmbedHistory) {
				targetMethod.AddPatchedByMemberAttribute(yourMethod);
			}

			var toggleAttributesAttr = yourMethod.GetCustomAttribute<ToggleMethodAttributes>();
			var toggleValue = toggleAttributesAttr?.Attributes ?? 0;
			targetMethod.Attributes ^= (MethodAttributes) toggleValue;
		}

		private static Func<CustomAttribute, bool> AttrFilter(ModificationScope scope) {
			Func<CustomAttribute, bool> onlyPwAttrs = x => x.AttributeType.Namespace == nameof(Patchwork);
			Func<CustomAttribute, bool> anyAttr = x => true;
			return scope.HasFlag(ModificationScope.CustomAttributes) ? anyAttr : onlyPwAttrs;
		}

		private void Log_modifying_member(string kind, MemberReference forMember) {
			Log.Debug("Modifying {0:l} for: {1:l}", kind, forMember.UserFriendlyName());
		}

		private void ModifyTypeDecleration(TypeDefinition yourType) {
			Log_modifying_member("type", yourType);
			var targetType = CurrentMemberCache.Types[yourType].TargetType;
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
			if (EmbedHistory) {
				targetType.AddPatchedByTypeAttribute(yourType);
			}
		}

		private static bool IsBreakingOpCode(OpCode code) {
			return code.EqualsAny(OpCodes.Leave, OpCodes.Leave_S, OpCodes.Ret, OpCodes.Rethrow, OpCodes.Throw, OpCodes.Endfinally,
				OpCodes.Endfilter);
		}


		/// <summary>
		/// Transfers the method body of yourMethod into the targetMethod, keeping everything neat and tidy, creating new copies of yourMethod's instructions.
		/// </summary>
		/// <param name="targetMethod">The target method.</param>
		/// <param name="yourMethod">Your instructions.</param>
		private void TransferMethodBody(MethodDefinition targetMethod, MethodDefinition yourMethod) {
			//TODO: This method needs to be refactored somehow. In particular, the IL transformation in PatchworkDebugRegisterAttribute needs to be applied separately.

			targetMethod.Body.Instructions.Clear();
			var injectManual = yourMethod.GetCustomAttribute<PatchworkDebugRegisterAttribute>();
			FieldReference debugFieldRef = null;

			_concat = _concat ?? targetMethod.Module.GetMethodLike(() => String.Concat("", ""));
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
				var debugDeclType = (TypeReference) injectManual.DeclaringType ?? targetMethod.DeclaringType;
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

				var targetInstruction = Hacks.CreateInstruction(yourInstruction.OpCode, targetOperand);
				targetInstruction.OpCode = targetOpcode;
				targetInstruction.Operand = targetOperand;
				targetInstruction.SequencePoint = yourInstruction.SequencePoint;

				if (injectManual != null) {
					targetInstruction.OpCode = SimplifyOpCode(targetInstruction.OpCode);
				}
				var lastInstr = ilProcesser.Body.Instructions.LastOrDefault();
				if (yourInstruction.SequencePoint != null && injectManual != null
					&& (lastInstr == null || !IsBreakingOpCode(lastInstr.OpCode))) {
					var str = i == 0 ? "" : " ⇒ ";
					str += yourInstruction.SequencePoint.StartLine;
					ilProcesser.Emit(OpCodes.Ldsfld, debugFieldRef);
					ilProcesser.Emit(OpCodes.Ldstr, str);
					ilProcesser.Emit(OpCodes.Call, _concat);
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
				if (targetOperand is Instruction) {
					//conditional branch instructions
					var asInstr = (Instruction) targetOperand;
					targetOperand = instructionEquiv[asInstr];
				} else if (targetOperand is Instruction[]) {
					//Switch instruction (jump table)
					var asInstrs = ((Instruction[]) targetOperand);
					var equivTargetInstrs = asInstrs.Select(instr => instructionEquiv[instr]).ToArray();
					targetOperand = equivTargetInstrs;
				}
				targetInstruction.Operand = targetOperand;
			}
		}
	}
}
