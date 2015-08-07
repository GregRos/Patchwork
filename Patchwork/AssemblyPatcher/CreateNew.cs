using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
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
			if (yourProperty.Name.Contains("GetEnumerator")) {
				int sadfsg = 5;
			}
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
					HasThis = yourMethod.HasThis,
					ExplicitThis = yourMethod.ExplicitThis,
					NoInlining = yourMethod.NoInlining,
					NoOptimization = yourMethod.NoOptimization,
					ReturnType = yourMethod.ReturnType //<---- this is temporary (setting it again to emphasize)
				};

			targetMethod.SecurityDeclarations.AddRange(yourMethod.SecurityDeclarations.Select(x => new SecurityDeclaration(x.Action, x.GetBlob())));

			targetDeclaringType.Methods.Add(targetMethod);

			//however, for FixType calls to work, the signature has to be correct... 
			foreach (var yourParam in yourMethod.Parameters) {
				targetMethod.Parameters.Add(new ParameterDefinition(yourParam.ParameterType));
			}

			var genList = new List<GenericParameter>();
			foreach (var yourTypeParam in yourMethod.GenericParameters) {
				var targetTypeParam = new GenericParameter(yourTypeParam.Name, targetMethod) {
					Attributes = yourTypeParam.Attributes //includes non-type constraints
				};
				genList.Add(targetTypeParam);
			}
			//clearing params can change the method signature, which can make FixType calls fail 
			//so we try to do things like that as atomically as possible.

			targetMethod.GenericParameters.AddRange(genList);
			//the T : S constraint means that we have to first declare all type parameters, and only then
			//fix their constraints... otherwise FixType will find the method we just modified above 
			//but may not find the type parameter.



			for (int i = 0; i < targetMethod.GenericParameters.Count; i++) {
				foreach (var constraint in yourMethod.GenericParameters[i].Constraints) {
					targetMethod.GenericParameters[i].Constraints.Add(FixTypeReference(constraint));
				}
			}

			var parList = new List<ParameterDefinition>();
			foreach (var yourParam in yourMethod.Parameters) {
				var targetParamType = FixTypeReference(yourParam.ParameterType);
				var targetParam = new ParameterDefinition(yourParam.Name, yourParam.Attributes, targetParamType) {
					Constant = yourParam.Constant,
				};
				parList.Add(targetParam);
			}
			//AFTER fixing the type params because it might reference one
			targetMethod.Parameters.Clear();
			targetMethod.Parameters.AddRange(parList);
			targetMethod.ReturnType = FixTypeReference(yourMethod.ReturnType);

			//note that YOU DO NOT do targetMethod.Parameters.AddRangE(yourMethod.Parameters)
			//ParameterDefinitions are mutable! 
			//What's worse, adding a parameter to a member modifies that parameter!

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
			Log.Warning("Implicitly creating {0:l} for: {1:l}.", kind,forMember.UserFriendlyName());
		}

		private void Log_creating_member(string kind, MemberReference forMember) {
			Log.Debug("Creating {0:l} for: {1:l}", kind, forMember.UserFriendlyName());
		}

		private void Log_duplicate_member(string kind, MemberReference newMember, MemberReference oldMember) {
			Log.Error("Conflict between {0:l}s: {1:l}, and {2:l}. This can screw things up, so make sure this member isn't important.", kind, newMember.UserFriendlyName(), oldMember.UserFriendlyName());
		}
	}
}
