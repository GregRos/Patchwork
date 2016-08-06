using System.Linq;
using Mono.Cecil;
using Patchwork.Engine.Utility;

namespace Patchwork.Engine
{
	public partial class AssemblyPatcher
	{
		/// <summary>
		/// Creates a new property like the specified property, but doesn't add it anywhere.
		/// </summary>
		/// <param name="yourProperty"></param>
		/// <param name="newName"></param>
		/// <returns></returns>
		private PropertyDefinition CopyProperty(PropertyDefinition yourProperty, string newName) {
			var targetPropertyType = FixTypeReference(yourProperty.PropertyType);
			var targetProperty = new PropertyDefinition(newName,
				yourProperty.Attributes,
				targetPropertyType) {
					HasThis = yourProperty.HasThis,
					Constant = yourProperty.Constant,
					HasDefault = yourProperty.HasDefault,
					HasConstant = yourProperty.HasConstant,
				};
			foreach (var yourParam in yourProperty.Parameters) {
				targetProperty.Parameters.Add(new ParameterDefinition(yourParam.Name, yourParam.Attributes,
					FixTypeReference(yourParam.ParameterType)) {
						Constant = yourParam.Constant,
						MarshalInfo = CopyMarshalInfo(yourParam.MarshalInfo),
						//we need to set this because Cecil behaves weirdly when you set Constant (even though Constant returns null if there is no Constant, if you set Constant to null it thinks there is a constant, and its value is null)
						HasConstant = yourParam.HasConstant
					});
			}

			return targetProperty;
		}

		/// <summary>
		///     Creates a new property in the target assembly, but doesn't set its accessors.
		/// </summary>
		/// <param name="targetType">Type of the target.</param>
		/// <param name="yourProperty">Your property.</param>
		/// <param name="newPropAttr">The new property attribute.</param>
		/// <exception cref="PatchDeclerationException">Thrown if this member collides with another member, and the error cannot be resolved.</exception>
		private PropertyDefinition CreateNewProperty(TypeDefinition targetType, PropertyDefinition yourProperty,
			NewMemberAttribute newPropAttr) {
			if (newPropAttr.IsImplicit) {
				Log_implicitly_creating_member("property", yourProperty);
			} else {
				Log_creating_member("property", yourProperty);
			}
			var newName = newPropAttr.NewMemberName ?? yourProperty.Name;
			var maybeDuplicate = targetType.GetProperty(newName, yourProperty.Parameters.Select(x => x.ParameterType));
			
			if (maybeDuplicate != null) {
				Log_duplicate_member("property", yourProperty, maybeDuplicate);
				newName = GetNameAfterCollision(newName);
				var prevName = newName;
				Log_name_changed("property", yourProperty, prevName, newName);
			}
			var targetProperty = CopyProperty(yourProperty, newName);
			targetType.Properties.Add(targetProperty);
			return targetProperty;
		}

		/// <summary>
		/// Creates an event like the specified event, but doesn't add it anywhere.
		/// </summary>
		/// <param name="yourEvent"></param>
		/// <param name="newName"></param>
		/// <returns></returns>
		private EventDefinition CopyEvent(EventDefinition yourEvent, string newName) {
			var targetEventType = FixTypeReference(yourEvent.EventType);
			var targetEvent = new EventDefinition(newName,
				yourEvent.Attributes,
				targetEventType);
			return targetEvent;
		}

		private EventDefinition CreateNewEvent(TypeDefinition targetType, EventDefinition yourEvent,
			NewMemberAttribute newEventAttr) {
			if (newEventAttr.IsImplicit) {
				Log_implicitly_creating_member("event", yourEvent);
			} else {
				Log_creating_member("event", yourEvent);
			}
			var newName = newEventAttr.NewMemberName ?? yourEvent.Name;
			var maybeDuplicate = targetType.GetEvent(newName);
			
			if (maybeDuplicate != null) {
				var prevName = newName;
				Log_duplicate_member("event", yourEvent, maybeDuplicate);
				newName = GetNameAfterCollision(newName);
				Log_name_changed("event", yourEvent, prevName, newName);
			}
			
			var targetEvent = CopyEvent(yourEvent, newName);
			targetType.Events.Add(targetEvent);
			return targetEvent;
		}

		/// <summary>
		/// Creates a tpye like the specified type, but doesn't add it anywhere. However, its DeclaringType is set correctly.
		/// </summary>
		/// <param name="yourType"></param>
		/// <param name="targetNamespace"></param>
		/// <param name="targetName"></param>
		/// <returns></returns>
		private TypeDefinition CopyType(TypeDefinition yourType, string targetNamespace, string targetName) {
			var targetTypeDef = new TypeDefinition(targetNamespace, targetName, yourType.Attributes) {
				DeclaringType = yourType.DeclaringType == null ? null : FixTypeReference(yourType.DeclaringType).Resolve(),
				PackingSize = yourType.PackingSize,
				ClassSize = yourType.ClassSize,
			};

			targetTypeDef.SecurityDeclarations.AddRange(yourType.SecurityDeclarations);
			foreach (var yourTypeParam in yourType.GenericParameters) {
				var targetTypeParam = new GenericParameter(yourTypeParam.Name, targetTypeDef);
				targetTypeDef.GenericParameters.Add(targetTypeParam);
			}
			return targetTypeDef;
		}

		private static string GetNameAfterCollision(string original) {
			return $"{original}_$pw$_{StringHelper.RandomWordString(5)}";
		}
		
		private static void SplitTypeName(string fullName, out string ns, out string typeName) {
			var lastDot = fullName.LastIndexOf('.');
			if (lastDot == -1) {
				ns = "";
				typeName = fullName;
			} else {
				ns = fullName.Substring(0, lastDot);
				typeName = fullName.Substring(lastDot + 1);
			}
		}

		/// <summary>
		/// Creates a new type in the target assembly, based on yourType.
		/// </summary>
		/// <param name="yourType">Your type, which describes what kind of type to create.</param>
		/// <param name="actionAttribute">The action attribute ordering the creation.</param>
		/// <returns></returns>
		/// <exception cref="PatchDeclerationException">Thrown if this member collides with another member, and the error cannot be resolved.</exception>
		private TypeDefinition CreateNewType(TypeDefinition yourType, NewTypeAttribute actionAttribute) {
			if (actionAttribute.IsImplicit) {
				Log_implicitly_creating_member("type", yourType);
			} else {
				Log_creating_member("type", yourType);
			}

			string targetNamespace = yourType.Namespace, targetName = yourType.Name;
			if (actionAttribute.NewTypeName != null) {
				targetName = actionAttribute.NewTypeName;
			}
			if (actionAttribute.NewNamespace != null) {
				targetNamespace = actionAttribute.NewNamespace;
			}

			//sometimes, compilers generate short names with dots in them. This terribly confuses Cecil.
			if (targetName.Contains(".")) {
				var prevName = targetName;
				targetName = targetName.Replace('.', '_');
				Log_name_changed("type", yourType, prevName, targetName);
			}
			var maybeDuplicate = TargetAssembly.MainModule.GetType(targetNamespace, targetName);
			if (maybeDuplicate != null) {
				var prevName = targetName;
				Log_duplicate_member("type", yourType, maybeDuplicate);
				targetName = GetNameAfterCollision(targetName);
				Log_name_changed("type", yourType, prevName, targetName);
			}
			
			var targetTypeDef = CopyType(yourType, targetNamespace, targetName);
			if (yourType.DeclaringType != null) {
				targetTypeDef.DeclaringType.NestedTypes.Add(targetTypeDef);
			} else {
				TargetAssembly.MainModule.Types.Add(targetTypeDef);
			}
			return targetTypeDef;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="yourMethod"></param>
		/// <param name="targetName"></param>
		/// <returns></returns>
		private MethodDefinition CopyMethod(MethodDefinition yourMethod, string targetName) {
			var targetMethod = new MethodDefinition(targetName, yourMethod.Attributes,
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

			foreach (var yourTypeParam in yourMethod.GenericParameters) {
				var targetTypeParam = new GenericParameter(yourTypeParam.Name, targetMethod) {
					Attributes = yourTypeParam.Attributes //includes non-type constraints
				};
				targetMethod.GenericParameters.Add(targetTypeParam);
			}
			
			//we do this so we can perform collision detection later on.
			foreach (var yourParam in yourMethod.Parameters) {
				targetMethod.Parameters.Add(new ParameterDefinition(yourParam.ParameterType));
			}

			return targetMethod;
		}

		private void ModifyMethodDecleration(MethodDefinition yourMethod, MethodDefinition targetMethod) {
			for (int i = 0; i < targetMethod.GenericParameters.Count; i++) {
				foreach (var constraint in yourMethod.GenericParameters[i].Constraints) {
					targetMethod.GenericParameters[i].Constraints.Add(FixTypeReference(constraint));
				}
			}
			targetMethod.Parameters.Clear();
			foreach (var yourParam in yourMethod.Parameters) {
				var targetParamType = FixTypeReference(yourParam.ParameterType);
				//we need to set this because Cecil behaves weirdly when you set Constant to `null`
				var targetParam = new ParameterDefinition(yourParam.Name, yourParam.Attributes, targetParamType) {
					Constant = yourParam.Constant,
					MarshalInfo = CopyMarshalInfo(yourParam.MarshalInfo),
					HasConstant = yourParam.HasConstant
				};
				targetMethod.Parameters.Add(targetParam);
			}

			targetMethod.ReturnType = FixTypeReference(yourMethod.ReturnType);
		}

		/// <summary>
		/// Creates a new method in the target assembly, for the specified type.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="PatchDeclerationException">Thrown if this member collides with another member, and the error cannot be resolved.</exception>
		private MethodDefinition CreateNewMethod(MemberAction<MethodDefinition> memberAction) {
			var actionAttribute = (NewMemberAttribute)memberAction.ActionAttribute;
			var yourMethod = memberAction.YourMember;
			var targetDeclaringType = memberAction.TypeAction.TargetType;
			var newName = actionAttribute.NewMemberName ?? yourMethod.Name;
			if (actionAttribute.IsImplicit) {
				Log_implicitly_creating_member("method", yourMethod);
			} else {
				Log_creating_member("method", yourMethod);
			}

			var maybeDuplicate = targetDeclaringType.GetMethodLike(yourMethod, newName);
			if (maybeDuplicate != null) {
				var prevName = newName;
				Log_duplicate_member("method", yourMethod, maybeDuplicate);
				newName = GetNameAfterCollision(newName);
				Log_name_changed("method", yourMethod, prevName, newName);
			}

			var targetMethod = CopyMethod(yourMethod, newName);
			memberAction.TargetMember = targetMethod;
			targetDeclaringType.Methods.Add(targetMethod);
			return targetMethod;
		}

		private void Log_overwriting() {
			Log.Error("Going to overwrite that member.");
		}

		private void Log_failed_to_create() {
			Log.Error("Failed to create member.");
		}

		private MarshalInfo CopyMarshalInfo(MarshalInfo info) {
			return info == null ? null : new MarshalInfo(info.NativeType);
		}

		private FieldDefinition CopyField(FieldDefinition yourField, string newName) {
			var targetField =
				new FieldDefinition(newName, yourField.Resolve().Attributes, FixTypeReference(yourField.FieldType)) {
					InitialValue = yourField.InitialValue, //for field RVA
					Constant = yourField   ,
					MarshalInfo = CopyMarshalInfo(yourField.MarshalInfo),
					HasConstant = yourField.HasConstant
				};
			return targetField;
		}

		/// <summary>
		/// Creates a new field in the target assembly, for the specified type.
		/// </summary>
		/// <param name="targetDeclaringType">The target declaring type.</param>
		/// <param name="yourField">Your field.</param>
		/// <param name="attr">The action attribute.</param>
		/// <exception cref="PatchDeclerationException">Thrown if this member collides with another member, and the error cannot be resolved.</exception>
		/// <returns></returns>
		private FieldDefinition CreateNewField(TypeDefinition targetDeclaringType, FieldDefinition yourField,
			NewMemberAttribute attr) {
			if (attr.IsImplicit) {
				Log_implicitly_creating_member("field", yourField);
			} else {
				Log_creating_member("field", yourField);
			}
			var newName = attr.NewMemberName ?? yourField.Name;
			var maybeDuplicate = targetDeclaringType.GetField(newName);
			if (maybeDuplicate != null) {
				var prevName = newName;
				Log_duplicate_member("field", yourField, maybeDuplicate);
				newName = GetNameAfterCollision(newName);
				Log_name_changed("field", yourField, prevName, newName);
			}
			var targetField = CopyField(yourField, newName);
			targetDeclaringType.Fields.Add(targetField);
			return targetField;
		}

		private void Log_implicitly_creating_member(string kind, MemberReference forMember) {
			Log.Information("Implicitly creating {0:l} for: {1:l}.", kind,forMember.UserFriendlyName());
		}

		private void Log_creating_member(string kind, MemberReference forMember) {
			Log.Debug("Creating {0:l} for: {1:l}", kind, forMember.UserFriendlyName());
		}

		private void Log_duplicate_member(string kind, MemberReference newMember, MemberReference oldMember) {
			Log.Warning("Conflict between {0:l}s: {1:l}, and {2:l}.", kind, newMember.UserFriendlyName(), oldMember.UserFriendlyName());
		}

		private void Log_name_changed(string kind, MemberReference newMember, string oldName, string newName) {
			Log.Warning("The {kind:l} called {type:l} was to be introduced under the name {oldName:l} but will be introduced under the name {newName:l}", kind, newMember, oldName, newName);
		}
	}
}
