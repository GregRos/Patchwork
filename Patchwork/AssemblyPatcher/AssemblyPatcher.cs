using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Patchwork.Attributes;
using Patchwork.Collections;
using Patchwork.Utility;
using Serilog;

namespace Patchwork {

	public class BodyFileChange {
		public SequencePoint Start;
		public SequencePoint End;
		public MethodDefinition ModifiedMember;
		public string Name;
	}

	/// <summary>
	///     A class that patches a specific assembly (a target assembly) with your assemblies.
	/// </summary>
	public partial class AssemblyPatcher {
		private List<BodyFileChange> _bodyChanges = new List<BodyFileChange>();

		public IEnumerable<BodyFileChange> BodyChanges {
			get {
				return _bodyChanges;
			}
		}

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
			var assemblyLocation = typeof (PatchworkVersion).Assembly.Location;
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
		/// Whether or not to backup the TargetAssembly before applying a patch. Set to false for faster execution.
		/// </summary>
		public bool UseBackup {
			get;
			private set;
		} = true;

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

		/// <summary>
		/// Patches the current assembly with the assembly in the specified path.
		/// </summary>
		/// <param name="path"></param>
		public void PatchAssembly(string path) {
			bool readSymbols = File.Exists(Path.ChangeExtension(path, "pdb")) || File.Exists(path + ".mdb");
			var yourAssembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters() {
				ReadSymbols = readSymbols
			});
			var manifest = CreateManifest(yourAssembly);
			PatchManifest(manifest);
		}

		private void LogFailedToRemove(string memberType, MemberReference memberRef) {
			Log.Warning("Tried to remove a {type:l} for {fullName:l}, but couldn't find it. This can be expected if you're both removing and adding the member at the same time.", memberType, memberRef.UserFriendlyName());
		}

		private void LogTasks<T>(SimpleTypeLookup<T> memberActions, string format) {
			Log.Information(format + " {@Tasks}",
				new {
					ByTask = new {
						CreateAndModify = memberActions[typeof (NewMemberAttribute)].Count(),
						Remove = memberActions[typeof (RemoveThisMemberAttribute)].Count(),
						Modify = memberActions[typeof (ModifiesMemberAttribute)].Count()
					},
					Total = memberActions.Count
				});
		}

		private void IntroduceTypes(SimpleTypeLookup<TypeAction> typeActions) {
			foreach (var newTypeAction in typeActions[typeof (NewTypeAttribute)]) {
				bool needsInitialization = true;
				var newType = CreateNewType(newTypeAction.YourType, (NewTypeAttribute) newTypeAction.ActionAttribute, out needsInitialization);
				if (newType == null) {
					typeActions.Remove(newTypeAction);
					continue;
				}
				newTypeAction.TargetType = newType;
				if (!needsInitialization) {
					typeActions.Remove(newTypeAction);
					newTypeAction.ActionAttribute = new ModifiesTypeAttribute();
					typeActions.Add(typeof (ModifiesTypeAttribute), newTypeAction);
				}
			}
		}

		private void IntroduceMethods(SimpleTypeLookup<MemberAction<MethodDefinition>> methodActions) {
			foreach (var actionParams in methodActions[typeof (NewMemberAttribute)]) {
				var newMethod = CreateNewMethod(actionParams.TypeAction.TargetType, actionParams.YourMember, (NewMemberAttribute) actionParams.ActionAttribute);
				if (newMethod == null) {
					methodActions.Remove(actionParams);
				}
			}
		}

		private void IntroduceFields(SimpleTypeLookup<MemberAction<FieldDefinition>> fieldActions) {
			Log.Header("Creating new fields");
			foreach (var fieldAction in fieldActions[typeof (NewMemberAttribute)]) {
				var status = CreateNewField(fieldAction.TypeAction.TargetType,
					fieldAction.YourMember,
					(NewMemberAttribute) fieldAction.ActionAttribute);
				if (status == null) {
					fieldActions.Remove(fieldAction);
				}
			}
		}

		private void IntroduceProperties(SimpleTypeLookup<MemberAction<PropertyDefinition>> propActions) {
			foreach (var propAction in propActions[typeof (NewMemberAttribute)]) {
				var newProperty = CreateNewProperty(propAction.TypeAction.TargetType, propAction.YourMember, (NewMemberAttribute) propAction.ActionAttribute);
				if (newProperty == null) {
					propActions.Remove(propAction);
				}
			}
		}

		private void IntroduceEvents(SimpleTypeLookup<MemberAction<EventDefinition>> eventActions) {
			foreach (var eventAction in eventActions[typeof (NewMemberAttribute)]) {
				var newProperty = CreateNewEvent(eventAction.TypeAction.TargetType, eventAction.YourMember, (NewMemberAttribute) eventAction.ActionAttribute);
				if (newProperty == null) {
					eventActions.Remove(eventAction);
				}
			}
		}

		private void UpdateMethods(SimpleTypeLookup<MemberAction<MethodDefinition>> methodActions) {
			foreach (var methodAction in methodActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
				AutoModifyMethod(methodAction.TypeAction.TargetType,
					methodAction.YourMember,
					methodAction.ActionAttribute);
			}
		}

		private void UpdateProperties(SimpleTypeLookup<MemberAction<PropertyDefinition>> propActions) {
			foreach (var propAction in propActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
				AutoModifyProperty(propAction.TypeAction.TargetType, propAction.ActionAttribute, propAction.YourMember);
			}
		}

		private void UpdateFields(SimpleTypeLookup<MemberAction<FieldDefinition>> fieldActions) {
			foreach (var fieldAction in fieldActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
				AutoModifyField(fieldAction.TypeAction.TargetType, fieldAction.ActionAttribute, fieldAction.YourMember);
			}
		}

		private void UpdateEvents(SimpleTypeLookup<MemberAction<EventDefinition>> eventActions) {
			foreach (var eventAction in eventActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
				AutoModifyEvent(eventAction.TypeAction.TargetType, eventAction.ActionAttribute, eventAction.YourMember);
			}
		}

		private void UpdateTypes(SimpleTypeLookup<TypeAction> typeActions) {
			foreach (var modType in typeActions[typeof (NewTypeAttribute)]) {
				AutoModifyTypeDecleration(modType.YourType);
			}
		}

		private void RemoveMethods(SimpleTypeLookup<MemberAction<MethodDefinition>> methodActions) {
			foreach (var actionParams in methodActions[typeof (RemoveThisMemberAttribute)]) {
				var result =
					actionParams.TypeAction.TargetType.GetMethodsLike(actionParams.YourMember).ToList().Any(
						x => actionParams.TypeAction.TargetType.Methods.Remove(x));
				if (!result) {
					LogFailedToRemove("method", actionParams.YourMember);
				}
			}
		}

		private void RemoveProperties(SimpleTypeLookup<MemberAction<PropertyDefinition>> propActions) {
			foreach (var propAction in propActions[typeof (RemoveThisMemberAttribute)]) {
				var removed = propAction.TypeAction.TargetType.GetPropertiesLike(propAction.YourMember).ToList().Any(x => propAction.TypeAction.TargetType.Properties.Remove(x));
				if (!removed) {
					LogFailedToRemove("property", propAction.YourMember);
				}
			}
		}

		private void RemoveFields(SimpleTypeLookup<MemberAction<FieldDefinition>> fieldActions) {
			foreach (var fieldAction in fieldActions[typeof (RemoveThisMemberAttribute)]) {
				var removed =
					fieldAction.TypeAction.TargetType.Fields.RemoveWhere(x => x.Name == fieldAction.YourMember.Name);
				if (!removed) {
					LogFailedToRemove("field", fieldAction.YourMember);
				}
			}
		}

		private void RemoveEvents(SimpleTypeLookup<MemberAction<EventDefinition>> eventActions) {
			foreach (var eventAction in eventActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
				var removed =
					eventAction.TypeAction.TargetType.Events.RemoveWhere(x => x.Name == eventAction.YourMember.Name);
				if (!removed) {
					LogFailedToRemove("event", eventAction.YourMember);
				}
			}
		}

		/// <summary>
		/// Applies the patch described in the given PatchingManifest to the TargetAssembly.
		/// </summary>
		/// <param name="manifest">The PatchingManifest. Note that the instance will be populated with additional information (such as newly created types) during execution.</param>
		public void PatchManifest(PatchingManifest manifest) {
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
			var targetAssemblyBackup = TargetAssembly.Clone();
			try {
				IntroduceTypes(manifest.TypeActions);

				RemoveMethods(manifest.MethodActions);
				IntroduceMethods(manifest.MethodActions);

				var patchingAssembly = manifest.PatchingAssembly;
				CopyCustomAttributesByImportAttribute(TargetAssembly, patchingAssembly,
					patchingAssembly.GetCustomAttribute<ImportCustomAttributesAttribute>());
				CopyCustomAttributesByImportAttribute(TargetAssembly.MainModule, patchingAssembly.MainModule,
					patchingAssembly.MainModule.GetCustomAttribute<ImportCustomAttributesAttribute>());

				UpdateTypes(manifest.TypeActions);

				RemoveFields(manifest.FieldActions);
				IntroduceFields(manifest.FieldActions);
				UpdateFields(manifest.FieldActions);

				RemoveProperties(manifest.PropertyActions);
				IntroduceProperties(manifest.PropertyActions);
				UpdateProperties(manifest.PropertyActions);

				RemoveEvents(manifest.EventActions);
				IntroduceEvents(manifest.EventActions);
				UpdateEvents(manifest.EventActions);

				UpdateMethods(manifest.MethodActions);
			}
			catch {
				TargetAssembly = targetAssemblyBackup;
				throw;
			}
		}

		public void WriteTo(string path) {
			
			Log.Information("Writing assembly {@OrigName:l} [{@OrigPath:l}] to location {@DestPath:l}",
				TargetAssembly.Name.Name,
				PathHelper.GetUserFriendlyPath(TargetAssembly.MainModule.FullyQualifiedName),
				PathHelper.GetUserFriendlyPath(path));
			TargetAssembly.Write(path);
			Log.Information("Write completed successfuly.");
		}

		private TypeDefinition GetPatchedTypeByName(TypeReference typeRef) {
			var patchedTypeName = typeRef.GetPatchedTypeFullName();
			if (patchedTypeName == null) {
				return null;
			}
			return TargetAssembly.MainModule.GetType(patchedTypeName);
		}

	}

}