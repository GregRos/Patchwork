using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mono.Cecil;
using Patchwork.Attributes;
using Patchwork.Shared;
using Patchwork.Utility;
using Serilog;

namespace Patchwork {
	/// <summary>
	///     A class that patches a specific assembly (a target assembly) with your assemblies.
	/// </summary>
	public partial class AssemblyPatcher {
		/// <summary>
		///     Initializes a new instance of the <see cref="AssemblyPatcher" /> class.
		/// </summary>
		/// <param name="targetAssembly">The target assembly being patched by this instance.</param>
		/// <param name="implicitImport">The implicit import setting.</param>
		/// <param name="log"></param>
		public AssemblyPatcher(AssemblyDefinition targetAssembly,
			ImplicitImportSetting implicitImport = ImplicitImportSetting.OnlyCompilerGenerated,
			ILogger log = null) {
			TargetAssembly = targetAssembly;
			ImplicitImports = implicitImport;
			Log = log ?? Serilog.Log.Logger;
			Log.Information("Created patcher for assembly: {0:l}", targetAssembly.Name);
			Filter = x => true;
			var assemblyLocation = typeof (PwVersion).Assembly.Location;
			Log.Information("Patching the assembly using Patchwork.Attributes to embed some common information.");
			PatchAssembly(assemblyLocation); //we add the Shared members of the Patchwork.Attributes assembly
		}

		public AssemblyPatcher(string targetAssemblyPath,
			ImplicitImportSetting implicitImport = ImplicitImportSetting.OnlyCompilerGenerated,
			ILogger log = null)
			: this(AssemblyDefinition.ReadAssembly(targetAssemblyPath), implicitImport, log) {
		}

		/// <summary>
		///     Special options for debugging purposes. Should not be set from user code.
		/// </summary>
		/// <value>
		///     The debug options.
		/// </value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DebugFlags DebugOptions {
			get;
			set;
		}

		/// <summary>
		///     If set (default null), a filter that says which types to include. This is a debug option.
		/// </summary>
		/// <value>
		///     The filter.
		/// </value>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Func<TypeDefinition, bool> Filter {
			get;
			set;
		}

		/// <summary>
		///     Exposes the target assembly being patched by this instance.
		/// </summary>
		/// <value>
		///     The target assembly.
		/// </value>
		public AssemblyDefinition TargetAssembly {
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the implicit imports setting. This influences how members that don't have any Patch attributes are
		///     treated.
		/// </summary>
		/// <value>
		///     The implicit import setting.
		/// </value>
		public ImplicitImportSetting ImplicitImports {
			get;
			set;
		}

		/// <summary>
		///     Gets the log.
		/// </summary>
		/// <value>
		///     The log.
		/// </value>
		public ILogger Log {
			get;
			private set;
		}

		public void PatchAssembly(string path) {
			PatchAssembly(
				AssemblyDefinition.ReadAssembly(path, new ReaderParameters() {ReadSymbols = true}));
		}

		private void LogFailedToRemove(string memberType, MemberReference memberRef) {
			Log.Warning("Tried to remove a {type:l} for {fullName:l}, but couldn't find it. This can be expected if you're both removing and adding the member at the same time.", memberType, memberRef.UserFriendlyName());
		}

		/// <summary>
		///     Patches the current assembly with your patching assembly.
		/// </summary>
		/// <param name="yourAssembly">Your patching assembly.</param>
		/// <exception cref="System.ArgumentException">The assembly MUST have the PatchAssemblyAttribute attribute. Sorry.</exception>
		public void PatchAssembly(AssemblyDefinition yourAssembly) {
			try {
				/*	The point of this method is to introduce all the patched elements in the correct order,
			 * 	so the user doesn't have to worry about dependencies between code elements, and can have circular  dependencies.
			 * 	
			 * 	The order is the following:
			 *  1. Declare all the new types, in the order of their nesting level (.e.g topmost types, nested types, types nested in those, etc),
				 *  But don't set anything that references other things, like BaseType, interfaces, custom attributes, etc.
				 *  Type parameters *are* added at this stage, but no constraints are specified.
			 *  2. Remove any methods that need removing.
			 *	3. Declare all the new methods, including signatures and type parameters + constraints, but without setting their bodies.
				 *	This is performed now because types can reference attribute constructors that don't exist until we create them,
				 *	but method definitions alone just reference types, and don't need to know about inheritance or constraints.
				 *	Don't set custom attributes in this stage.
			 *  4. Add any custom attributes for things that aren't part of the method/type hierarchy, such as assemblies.
			 *  5.  Set the type information we didn't set in (1), according to the same order, including custom attributes.
				 *  When/if modifying type inheritance is allowed, this process will occur at this stage.
			 * 	6.	remove/define/modify fields. This is where most enum processing happens. Custom attributes are set at this stage.
			 * 	7.	Remove/define/modify properties. This doesn't change anything about methods. Custom attributes are set at this stage.
			 * 	8.	Fill/modify the bodies of all remaining methods, and also modify accessibility/attributes of existing ones.
			 * 			Note that methods that are the source of DuplicatesBody will be resolved in their unmodified version,
			 * 			so the modifications won't influence this functionality.
				   		Custom attributes are set at this stage.
				 *		Also, explicit overrides (what in C# is explicit interface implementation) are specified here.
				
			Standard order of business for order between modify/remove/update:
			1. Remove Xs
			2. Create new Xs
			3. Update existing Xs (+ those we just created)
			 */
				Log.Information(
					"Patching assembly {PatchName:l} [{PatchPath:l}] => {OrigName:l} [{OrigPath:l}]",
					yourAssembly.Name.Name,
					PathHelper.GetUserFriendlyPath(yourAssembly.MainModule.FullyQualifiedName),
					TargetAssembly.Name.Name,
					PathHelper.GetUserFriendlyPath(TargetAssembly.MainModule.FullyQualifiedName)
					);

				if (!yourAssembly.IsPatchingAssembly()) {
					throw new PatchDeclerationException(
						"The assembly MUST have the PatchAssemblyAttribute attribute. Sorry.");
				}


				var allTypesInOrder = GetAllTypesInNestingOrder(yourAssembly.MainModule.Types).ToList();
				//+ Organizing types by action attribute
				var typesByActionSeq =
					from type in allTypesInOrder
					let typeActionAttr = GetTypeActionAttribute(type)
					where typeActionAttr != null && Filter(type)
					orderby type.UserFriendlyName()
					group new {
						yourType = type,
						typeActionAttr
					} by typeActionAttr.GetType();

				var typesByAction = typesByActionSeq.ToSimpleTypeLookup();

				Log.Information("Type Tasks: {@Tasks}",
					new {
						ByTask = new {
							Replace = typesByAction[typeof (ReplaceTypeAttribute)].Count(),
							Modify = typesByAction[typeof (ModifiesTypeAttribute)].Count(),
							Create = typesByAction[typeof (NewTypeAttribute)].Count()
						},
						Total = typesByAction.Count
					});

				//++ 1. Creating new types
				Log.Information("Creating new types");
				foreach (var newTypeAction in typesByAction[typeof (NewTypeAttribute)]) {
					var status = CreateNewType(newTypeAction.yourType,
						(NewTypeAttribute) newTypeAction.typeActionAttr);
					if (status == NewMemberStatus.InvalidItem) {
						Log_failed_to_create();
						typesByAction.Remove(newTypeAction);
					}
				}

				//+ Pairing types, organizing for modification
				//we pair off each yourType with the targetType it modifies (note that a targetType is also one we created just now)
				//we'll use these pairings throughout the rest of the method.
				var typePairingsSeq =
					from typeGroup in typesByAction
					from typeAction in typeGroup
					group new {
						typeAction.yourType,
						typeAction.typeActionAttr,
						targetType = GetPatchedTypeByName(typeAction.yourType)
					} by typeAction.typeActionAttr.GetType();

				var typePairings = typePairingsSeq.ToSimpleTypeLookup(); //cache them
				foreach (var pairing in typePairings.SelectMany(x => x)) {
					if (pairing.targetType == null) {
						throw Errors.Missing_member("type",
							pairing.yourType,
							pairing.yourType.GetPatchedTypeFullName());
					}
					if (pairing.targetType.IsInterface && !(pairing.typeActionAttr is NewTypeAttribute)) {
						throw Errors.Invalid_member("type",
							pairing.yourType,
							pairing.yourType.GetPatchedTypeFullName(),
							"You cannot modify existing interfaces.");
					}
				}
				//+ Organizing methods
				Log.Information("Organizing methods.");
				var methodActionsSeq =
					from pair in typePairings.SelectMany(x => x)
					from yourMethod in pair.yourType.Methods
					where !yourMethod.HasCustomAttribute<DisablePatchingAttribute>()
					let actionAttr = GetMemberActionAttribute(yourMethod, pair.typeActionAttr)
					where actionAttr != null
					group new {
						pair.targetType,
						pair.yourType,
						yourMethod,
						methodActionAttr = actionAttr
					} by actionAttr.GetType();

				var methodActions = methodActionsSeq.ToSimpleTypeLookup();

				Log.Information("Method Tasks: {@Tasks}",
					new {
						ByTask = new {
							CreateAndModify = methodActions[typeof (NewMemberAttribute)].Count(),
							Remove = methodActions[typeof (RemoveThisMemberAttribute)].Count(),
							Modify = methodActions[typeof (ModifiesMemberAttribute)].Count()
						},
						Total = methodActions.Count
					});

				//+ Organizing fields
				var fieldActionsSeq =
					//we create a list of all fields, as well as the action to perform with each field
					from typeAction in typePairings.SelectMany(x => x)
					from yourField in typeAction.yourType.Fields
					where !yourField.HasCustomAttribute<DisablePatchingAttribute>()
					let fieldActionAttr = GetMemberActionAttribute(yourField, typeAction.typeActionAttr)
					where fieldActionAttr != null
					group //grouping the fields is mostly for debugging purposes
						new {
							typeAction.yourType,
							typeAction.targetType,
							fieldActionAttr,
							yourField
						} by fieldActionAttr.GetType();

				var fieldActions = fieldActionsSeq.ToSimpleTypeLookup();
				Log.Information("Field Tasks: {@Tasks}",
					new {
						ByTask = new {
							CreateAndModify = fieldActions[typeof (NewMemberAttribute)].Count(),
							Remove = fieldActions[typeof (RemoveThisMemberAttribute)].Count(),
							Modify = fieldActions[typeof (ModifiesMemberAttribute)].Count()
						},
						Total = fieldActions.Count
					});
				
				//+ Organizing properties
				var propActionsSeq =
					from pair in typePairings.SelectMany(x => x)
					from yourProp in pair.yourType.Properties
					where !yourProp.HasCustomAttribute<DisablePatchingAttribute>()
					let propActionAttr = GetMemberActionAttribute(yourProp, pair.typeActionAttr)
					where propActionAttr != null
					group new {
						pair.targetType,
						pair.yourType,
						yourProp,
						pair.typeActionAttr,
						propActionAttr
					} by propActionAttr.GetType();

				var propActions = propActionsSeq.ToSimpleTypeLookup();

				Log.Information("Property Tasks: {@Tasks}",
					new {
						ByTask = new {
							CreateAndModify = propActions[typeof (NewMemberAttribute)].Count(),
							Remove = propActions[typeof (RemoveThisMemberAttribute)].Count(),
							Modify = propActions[typeof (ModifiesTypeAttribute)].Count()
						},
						Total = propActions.Count
					});

				//++ 2. Removing methods
				//remove all the methods
				Log.Header("Removing methods");
				foreach (var actionParams in methodActions[typeof (RemoveThisMemberAttribute)]) {
					var result = actionParams.targetType.GetMethodsLike(actionParams.yourMethod).ToList().Any(x => actionParams.targetType.Methods.Remove(x));
					if (!result) {
						LogFailedToRemove("method", actionParams.yourMethod);
					}
				}

				//++ 3. Creating new methods
				Log.Header("Creating new methods");
				//create all the new methods
				foreach (var actionParams in methodActions[typeof (NewMemberAttribute)]) {
					var status = CreateNewMethod(actionParams.targetType,
						actionParams.yourMethod,
						(NewMemberAttribute) actionParams.methodActionAttr);
					if (status == NewMemberStatus.InvalidItem) {
						Log_failed_to_create();
						methodActions.Remove(actionParams);
					}
				}



				//++ 4. Adding custom attributes to module/assembly.
				var assemblyImportAttribute = yourAssembly.GetCustomAttribute<ImportCustomAttributesAttribute>();
				var moduleImportAttribute = yourAssembly.MainModule.GetCustomAttribute<ImportCustomAttributesAttribute>();
				CopyCustomAttributesByImportAttribute(TargetAssembly, yourAssembly, assemblyImportAttribute);
				CopyCustomAttributesByImportAttribute(TargetAssembly.MainModule, yourAssembly.MainModule, moduleImportAttribute);
				
				

				//++ 5. Modifying type declerations
				Log.Information("Updating Type Ceclerations");
				foreach (var modType in typesByAction[typeof (NewTypeAttribute)]) {
					AutoModifyTypeDecleration(modType.yourType);
				}

				//++ 6. Updating Fields
				Log.Header("Clearing fields in replaced types");
				//+ Removing fields in replaced types
				foreach (var pairing in typePairings[typeof (ReplaceTypeAttribute)]) {
					Log.Information("Clearing fields in {0:l}", pairing.targetType.UserFriendlyName());
					pairing.targetType.Fields.Clear();
				}


				//+ Removing fields
				Log.Header("Removing fields");
				foreach (var fieldAction in fieldActions[typeof (RemoveThisMemberAttribute)]) {
					var removed =
						fieldAction.targetType.Fields.RemoveWhere(x => x.Name == fieldAction.yourField.Name);
					if (!removed) {
						LogFailedToRemove("field", fieldAction.yourField);
					}
				}

				//+ Creating new fields
				Log.Header("Creating new fields");
				foreach (var fieldAction in fieldActions[typeof (NewMemberAttribute)]) {
					var status = CreateNewField(fieldAction.targetType,
						fieldAction.yourField,
						(NewMemberAttribute) fieldAction.fieldActionAttr);
					if (status == NewMemberStatus.InvalidItem) {
						Log_failed_to_create();
						fieldActions.Remove(fieldAction);
					}
				}


				//+ Modifying existing fields
				Log.Header("Modifying fields");
				foreach (
					var fieldAction in fieldActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
					AutoModifyField(fieldAction.targetType, fieldAction.fieldActionAttr, fieldAction.yourField);
				}


				//++ 7. Updating properties
				Log.Header("Removing properties");
				//+ Removing properties
				foreach (var propAction in propActions[typeof (RemoveThisMemberAttribute)]) {
					var removed =
						propAction.targetType.Properties.RemoveWhere(x => x.Name == propAction.yourProp.Name);
					if (!removed) {
						LogFailedToRemove("field", propAction.yourProp);
					}
				}

				Log.Header("Creating properties");
				//+ Creating properties
				foreach (var propAction in propActions[typeof (NewMemberAttribute)]) {
					var status = CreateNewProperty(propAction.targetType,
						propAction.yourProp,
						(NewMemberAttribute) propAction.propActionAttr);
					if (status == NewMemberStatus.InvalidItem) {
						Log_failed_to_create();
						propActions.Remove(propAction);
					}
				}

				Log.Header("Modifying properties");
				//+ Modifying properties
				foreach (
					var propAction in propActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
					AutoModifyProperty(propAction.targetType, propAction.propActionAttr, propAction.yourProp);
				}
				//++ 8. Finalizing methods, generating method bodies.
				Log.Header("Modifying/generating method bodies");
				foreach (var methodAction in methodActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
					AutoModifyMethod(methodAction.targetType,
						methodAction.yourMethod,
						methodAction.methodActionAttr);
				}

				Log.Information("Patching {@PatchName:l} => {@OrigName:l} completed",
					yourAssembly.Name.Name,
					TargetAssembly.Name.Name);
			}
			catch (Exception ex) {
				Log.Fatal(ex,
					"An exception was unhandled. Since execution was halted mid-way, this means that this AssemblyPatcher has been corrupted.");
				throw;
			}
		}

		public void WriteTo(string path) {
			
			Log.Information("Writing assembly {@OrigName:l} [{@OrigPath:l}] to location {@DestPath:l}",
				TargetAssembly.Name.Name,
				PathHelper.GetUserFriendlyPath(TargetAssembly.MainModule.FullyQualifiedName),
				PathHelper.GetUserFriendlyPath(path));
			TargetAssembly.Write(path, new WriterParameters() {
			});
			Log.Information("Write completed successfuly.");
		}

		public TypeDefinition GetPatchedTypeByName(TypeReference typeRef) {
			var patchedTypeName = typeRef.GetPatchedTypeFullName();
			if (patchedTypeName == null) {
				return null;
			}
			return TargetAssembly.MainModule.GetType(patchedTypeName);
		}

		private IEnumerable<TypeDefinition> GetAllTypesInNestingOrder(
			ICollection<TypeDefinition> currentNestingLevelTypes) {
			if (currentNestingLevelTypes.Count == 0) {
				yield break;
			}
			var stack = new List<TypeDefinition>();
			foreach (var type in currentNestingLevelTypes) {
				if (type.HasCustomAttribute<DisablePatchingAttribute>()) {
					continue;
				}
				stack.AddRange(type.NestedTypes);
				yield return type;
			}
			foreach (var nestedType in GetAllTypesInNestingOrder(stack)) {
				yield return nestedType;
			}
		}

		private TypeActionAttribute GetTypeActionAttribute(TypeDefinition provider) {
			var attr = provider.GetCustomAttribute<TypeActionAttribute>();
			if (attr != null) {
				return attr;
			}
			switch (ImplicitImports) {
				case ImplicitImportSetting.OnlyCompilerGenerated:
					if (provider.IsCompilerGenerated()) {
						goto case ImplicitImportSetting.ImplicitByDefault;
					}
					goto case ImplicitImportSetting.NoImplicit;
				case ImplicitImportSetting.ImplicitByDefault:
					return new NewTypeAttribute(true);
				default:
				case ImplicitImportSetting.NoImplicit:
					return null;
			}
		}

		private MemberActionAttribute GetMemberActionAttribute(IMemberDefinition provider,
			TypeActionAttribute typeAttr) {
			var attr = provider.GetCustomAttribute<MemberActionAttribute>();
			if (attr != null) {
				if (attr is ModifiesMemberAttribute && !(typeAttr is ModifiesTypeAttribute)) {
					throw Errors.Invalid_decleration("ModifiesMember is only legal inside ModifiesType.");
				}
				return attr;
			}
			if (typeAttr is NewTypeAttribute || typeAttr is ReplaceTypeAttribute) {
				return new NewMemberAttribute();
			}
			switch (ImplicitImports) {
				case ImplicitImportSetting.OnlyCompilerGenerated:
					if (provider.IsCompilerGenerated()) {
						goto case ImplicitImportSetting.ImplicitByDefault;
					}
					goto case ImplicitImportSetting.NoImplicit;
				case ImplicitImportSetting.ImplicitByDefault:
					return new NewMemberAttribute(true);
				default:
				case ImplicitImportSetting.NoImplicit:
					return null;
			}
		}
	}
}