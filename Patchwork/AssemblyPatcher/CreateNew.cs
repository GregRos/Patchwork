using System;
using System.Collections.Generic;
using System.Linq;
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
		///     Creates a new property in the target assembly, but doesn't set its accessors.
		/// </summary>
		/// <param name="targetType">Type of the target.</param>
		/// <param name="yourProperty">Your property.</param>
		/// <param name="newPropAttr">The new property attribute.</param>
		/// <exception cref="PatchDeclerationException">Thrown if this member collides with another member, and the error cannot be resolved.</exception>
		private NewMemberStatus CreateNewProperty(TypeDefinition targetType, PropertyDefinition yourProperty,
			NewMemberAttribute newPropAttr) {
			if (newPropAttr.IsImplicit) {
				Log_implicitly_creating_member("property", yourProperty);
			} else {
				Log_creating_member("property", yourProperty);
			}
			var maybeDuplicate = targetType.GetProperty(yourProperty.Name,
				yourProperty.Parameters.Select(x => x.ParameterType));
			if (maybeDuplicate != null) {
				Log_duplicate_member("property", yourProperty, maybeDuplicate);
				if ((DebugOptions & DebugFlags.CreationOverwrites) != 0) {
					Log_overwriting();
					return NewMemberStatus.Continue;
				}
				if (newPropAttr.IsImplicit) {
					return NewMemberStatus.InvalidItem;
				}
				throw Errors.Duplicate_member("property", yourProperty.FullName, maybeDuplicate.FullName);
			}
			var targetPropertyType = FixTypeReference(yourProperty.PropertyType);
			var targetProperty = new PropertyDefinition(yourProperty.Name,
				yourProperty.Attributes,
				targetPropertyType) {
					HasThis = yourProperty.HasThis,
					Constant =  yourProperty.Constant,
					HasDefault = yourProperty.HasDefault
				};
			targetType.Properties.Add(targetProperty);
			foreach (var param in yourProperty.Parameters) {
				targetProperty.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes,
					FixTypeReference(param.ParameterType)));
			}
			return NewMemberStatus.Continue;
		}

		private NewMemberStatus CreateNewEvent(TypeDefinition targetType, EventDefinition yourEvent, NewMemberAttribute newEventAttr) {
			if (newEventAttr.IsImplicit) {
				Log_implicitly_creating_member("property", yourEvent);

			} else {
				Log_creating_member("property", yourEvent);
			}
			var maybeDuplicate = targetType.GetEvent(yourEvent.Name);
			if (maybeDuplicate != null) {
				Log_duplicate_member("property", yourEvent, maybeDuplicate);
				if ((DebugOptions & DebugFlags.CreationOverwrites) != 0) {
					Log_overwriting();
					return NewMemberStatus.Continue;
				}
				if (newEventAttr.IsImplicit) {
					return NewMemberStatus.InvalidItem;
				}
				throw Errors.Duplicate_member("property", yourEvent.FullName, maybeDuplicate.FullName);
			}
			var targetEventType = FixTypeReference(yourEvent.EventType);
			var targetEvent = new EventDefinition(yourEvent.Name,
				yourEvent.Attributes,
				targetEventType);

			targetType.Events.Add(targetEvent);
			return NewMemberStatus.Continue;
		}

		/// <summary>
		/// Creates a new type in the target assembly, based on yourType.
		/// </summary>
		/// <param name="yourType">Your type, which describes what kind of type to create.</param>
		/// <param name="actionAttribute">The action attribute ordering the creation.</param>
		/// <returns></returns>
		/// <exception cref="PatchDeclerationException">Thrown if this member collides with another member, and the error cannot be resolved.</exception>
		private NewMemberStatus CreateNewType(TypeDefinition yourType, NewTypeAttribute actionAttribute) {
			
			if (actionAttribute.IsImplicit) {
				Log_implicitly_creating_member("type", yourType);
			} else {
				Log_creating_member("type", yourType);
			}
			var maybeDuplicate = TargetAssembly.MainModule.GetAllTypes().FirstOrDefault(x => x.FullName == yourType.FullName);
			if (maybeDuplicate != null) {
				Log_duplicate_member("type", yourType, maybeDuplicate);
				if ((DebugOptions & DebugFlags.CreationOverwrites) != 0) {
					Log_overwriting();
					return NewMemberStatus.Continue;
				}
				if (actionAttribute.IsImplicit) {
					return NewMemberStatus.InvalidItem;
				}
				throw Errors.Duplicate_member("type", yourType.FullName, maybeDuplicate.FullName);
			}
			
			var targetTypeDef = new TypeDefinition(yourType.Namespace, yourType.Name, yourType.Attributes) {
				DeclaringType = yourType.DeclaringType == null ? null : FixTypeReference(yourType.DeclaringType).Resolve(),
				PackingSize = yourType.PackingSize,
				ClassSize = yourType.ClassSize,
			};
			
			targetTypeDef.SecurityDeclarations.AddRange(yourType.SecurityDeclarations);
			foreach (var yourTypeParam in yourType.GenericParameters) {
				var targetTypeParam = new GenericParameter(yourTypeParam.Name, targetTypeDef);
				targetTypeDef.GenericParameters.Add(targetTypeParam);
			}
			if (yourType.DeclaringType != null) {
				targetTypeDef.DeclaringType.NestedTypes.Add(targetTypeDef);
			} else {
				TargetAssembly.MainModule.Types.Add(targetTypeDef);
			}
			Log.Verbose("Created: {0}", targetTypeDef.FullName);
			//Note that 
			return NewMemberStatus.Continue;
		}

		/// <summary>
		/// Creates a new method in the target assembly, for the specified type.
		/// </summary>
		/// <param name="targetDeclaringType">The type declaring the method in the target assembly.</param>
		/// <param name="yourMethod">Your method, which describes the method to create..</param>
		/// <param name="actionAttribute">The action attribute ordering creation.</param>
		/// <returns></returns>
		/// <exception cref="PatchDeclerationException">Thrown if this member collides with another member, and the error cannot be resolved.</exception>
		private NewMemberStatus CreateNewMethod(TypeDefinition targetDeclaringType, MethodDefinition yourMethod,
			NewMemberAttribute actionAttribute) {
			if (actionAttribute.IsImplicit) {
				Log_implicitly_creating_member("method", yourMethod);
			} else {
				Log_creating_member("method", yourMethod);
			}
			
			var maybeDuplicate = targetDeclaringType.GetMethodsLike(yourMethod).FirstOrDefault();
			if (maybeDuplicate != null) {
				Log_duplicate_member("method", yourMethod, maybeDuplicate);
				if ((DebugOptions & DebugFlags.CreationOverwrites) != 0) {
					Log_overwriting();
					//this only okay for controlled testing code.
					return NewMemberStatus.Continue;
				}
				if (actionAttribute.IsImplicit) {
					return NewMemberStatus.InvalidItem;
				}
				throw Errors.Duplicate_member("type", yourMethod.FullName, maybeDuplicate.FullName);
			}
			
			var targetMethod = new MethodDefinition(yourMethod.Name, yourMethod.Attributes, 
				yourMethod.ReturnType) {
					ImplAttributes = yourMethod.ImplAttributes,
					SemanticsAttributes = yourMethod.SemanticsAttributes,
					CallingConvention = yourMethod.CallingConvention,
					//all the Attributes and Conventions take care of most of the IsX/HasY properties, except for a few.
					//this can be seen by looking at the Cecil code, where you can see which fields MethodDefinition has.
					HasThis = yourMethod.HasThis,
					ExplicitThis = yourMethod.ExplicitThis,
					NoInlining = yourMethod.NoInlining,
					NoOptimization = yourMethod.NoOptimization,
					ReturnType = yourMethod.ReturnType //<---- this is temporary (setting it again to emphasize)
				};

			targetMethod.SecurityDeclarations.AddRange(yourMethod.SecurityDeclarations.Select(x => new SecurityDeclaration(x.Action, x.GetBlob())));

			targetDeclaringType.Methods.Add(targetMethod);

			//First we add the parameters to the method so its signature is correct.
			//Note that we do not fix the types at this stage. 
			//This is because FixType calls can end up looking for the method we're adding right now
			//if any of the parameter types are MVars. The signature has to be similar to yourMethod's signature 
			//if FixType is to find this definition.
			//this may seem needlessly complicated and rather risky, and I thought so too, but there really isn't any way around it
			//or at least, I'm not clever enough to come up with one that doesn't introduce more problems 
			//(believe me, I've tried all sorts of things)
			foreach (var yourParam in yourMethod.Parameters) {
				targetMethod.Parameters.Add(new ParameterDefinition(yourParam.ParameterType));
			}
			
			//Note we're not adding any constraints yet because doing that involves FixType calls
			//also, the T : S constraint involves 2 MVars, so unless we add all the MVars first, FixType calls could never be resolved.

			var genList = new List<GenericParameter>();
			foreach (var yourTypeParam in yourMethod.GenericParameters) {
				var targetTypeParam = new GenericParameter(yourTypeParam.Name, targetMethod) {
					Attributes = yourTypeParam.Attributes //includes non-type constraints
				};
				genList.Add(targetTypeParam);
			}

			targetMethod.GenericParameters.AddRange(genList);
			//NOW we're adding constraints, when the signature is stable and when all MVars have been declared.
			for (int i = 0; i < targetMethod.GenericParameters.Count; i++) {
				foreach (var constraint in yourMethod.GenericParameters[i].Constraints) {
					targetMethod.GenericParameters[i].Constraints.Add(FixTypeReference(constraint));
				}
			}

			// note that we don't add the parameters to the method directly, at least not at first.
			//this is because we're making FixType calls, and again, they can end up looking up targetMethod
			//changing the signature will just end up with FixType not finding it.
			var parList = new List<ParameterDefinition>();
			foreach (var yourParam in yourMethod.Parameters) {
				var targetParamType = FixTypeReference(yourParam.ParameterType);
				var targetParam = new ParameterDefinition(yourParam.Name, yourParam.Attributes, targetParamType) {
					Constant = yourParam.Constant,
				};
				parList.Add(targetParam);
			}
			//we reset the parameters after we're done with that in as near an atomic operation as possible, and without any FixType calls.
			targetMethod.Parameters.Clear();
			targetMethod.Parameters.AddRange(parList);

			//we also fix this guy:
			targetMethod.ReturnType = FixTypeReference(yourMethod.ReturnType);

			//note that YOU DO NOT do targetMethod.Parameters.AddRangE(yourMethod.Parameters)
			//ParameterDefinitions are mutable!  and doing this can mutate them...
			return NewMemberStatus.Continue;
		}

		private void Log_overwriting() {
			Log.Error("Going to overwrite that member.");
		}

		private void Log_failed_to_create() {
			Log.Error("Failed to create member.");
		}

		/// <summary>
		/// Creates a new field in the target assembly, for the specified type.
		/// </summary>
		/// <param name="targetDeclaringType">The target declaring type.</param>
		/// <param name="yourField">Your field.</param>
		/// <param name="attr">The action attribute.</param>
		/// <exception cref="PatchDeclerationException">Thrown if this member collides with another member, and the error cannot be resolved.</exception>
		/// <returns></returns>
		private NewMemberStatus CreateNewField(TypeDefinition targetDeclaringType, FieldDefinition yourField,
			NewMemberAttribute attr) {
			if (attr.IsImplicit) {
				Log_implicitly_creating_member("field", yourField);
			} else {
				Log_creating_member("field", yourField);
			}
			var maybeDuplicate = targetDeclaringType.GetField(yourField.Name);
			if (maybeDuplicate != null) {
				Log_duplicate_member("field", yourField, maybeDuplicate);
				if ((DebugOptions & DebugFlags.CreationOverwrites) != 0) {
					Log_overwriting();
					return NewMemberStatus.Continue;
				}
				if (attr.IsImplicit) {
					return NewMemberStatus.InvalidItem;
				}
				throw Errors.Duplicate_member("type", yourField.FullName, maybeDuplicate.FullName);
			}
			var targetField =
				new FieldDefinition(yourField.Name, yourField.Resolve().Attributes, FixTypeReference(yourField.FieldType)) {
					InitialValue = yourField.InitialValue, //probably for string consts	
					Constant = yourField.Constant
				};
			targetDeclaringType.Fields.Add(targetField);
			return NewMemberStatus.Continue;
		}

		private void Log_implicitly_creating_member(string kind, MemberReference forMember) {
			Log.Information("Implicitly creating {0:l} for: {1:l}.", kind,forMember.UserFriendlyName());
		}

		private void Log_creating_member(string kind, MemberReference forMember) {
			Log.Debug("Creating {0:l} for: {1:l}", kind, forMember.UserFriendlyName());
		}

		private void Log_duplicate_member(string kind, MemberReference newMember, MemberReference oldMember) {
			Log.Error("Conflict between {0:l}s: {1:l}, and {2:l}.", kind, newMember.UserFriendlyName(), oldMember.UserFriendlyName());
		}
	}
}
