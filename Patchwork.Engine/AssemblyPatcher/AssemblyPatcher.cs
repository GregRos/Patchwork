using System.IO;
using System.Linq;
using Mono.Cecil;
using Patchwork.Engine.Utility;
using Serilog;
#pragma warning disable 618

namespace Patchwork.Engine {

	/// <summary>
	/// Used to monitor the progress of the patching engine.
	/// </summary>
	public interface IProgressMonitor {
		/// <summary>
		/// The name of the current task executing.
		/// </summary>
		string TaskTitle { get;set; }

		/// <summary>
		/// The name of the current subtask being performed.
		/// </summary>
		string TaskText { get;set; }

		/// <summary>
		/// The progress in the current task.
		/// </summary>
		int Current { get; set; }

		/// <summary>
		/// The number of progress to be done.
		/// </summary>
		int Total { get;set; }
	}


	/// <summary>
	///     A class that patches a specific assembly (a target assembly) with your assemblies.
	/// </summary>
	public partial class AssemblyPatcher {
		/// <summary>
		///     Initializes a new instance of the <see cref="AssemblyPatcher" /> class.
		/// </summary>
		/// <param name="targetAssembly">The target assembly being patched by this instance.</param>
		/// <param name="log"></param>
		private AssemblyPatcher(AssemblyDefinition targetAssembly, ILogger log = null) {
			TargetAssembly = targetAssembly;
			Log = log ?? Serilog.Log.Logger;
			Log.Information("Created patcher for assembly: {0:l}", targetAssembly.Name);
		}
		/// <summary>
		/// Constructs a new assembly patcher patching the assembly in the given path.
		/// </summary>
		/// <param name="targetAssemblyPath">The path to the given assembly.</param>
		/// <param name="log">A log.</param>
		public AssemblyPatcher(string targetAssemblyPath,
			ILogger log = null)
			: this(AssemblyCache.Default.ReadAssembly(targetAssemblyPath), log) {
			OriginalAssemblyMetadata = TargetAssembly.GetAssemblyMetadataString();
		}

		/// <summary>
		/// Specifies whether to embed history, which includes special patching history attributes, as well as most patching attributes. If enabled, creates a dependency on Patchwork.Attributes.
		/// </summary>
		public bool EmbedHistory {
			get;
			set;
		}

		private string OriginalAssemblyMetadata {
			get;
		}

		/// <summary>
		/// Whether or not to backup the TargetAssembly before applying a patch. Set to false for faster execution.
		/// </summary>
		public bool UseBackup {
			get;
			set;
		} = false;

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

		private MemberCache CurrentMemberCache {
			get;
			set;
		} = new MemberCache();

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
		/// <param name="o"></param>
		/// <param name="readSymbols"></param>
		public void PatchAssembly(string path, IProgressMonitor o,  bool readSymbols = true) {
			readSymbols = readSymbols && (File.Exists(Path.ChangeExtension(path, "pdb")) || File.Exists(path + ".mdb"));
			var yourAssembly = AssemblyCache.Default.ReadAssembly(path, readSymbols);
			var creator = new ManifestCreator();
			var manifest = creator.CreateManifest(yourAssembly);
			PatchManifest(manifest, o);
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
				var newType = CreateNewType(newTypeAction.YourType, (NewTypeAttribute) newTypeAction.ActionAttribute);
				if (newType == null) {
					typeActions.Remove(newTypeAction);
					continue;
				}
				newTypeAction.TargetType = newType;
			}
		}

		private void IntroduceMethods(SimpleTypeLookup<MemberAction<MethodDefinition>> methodActions) {
			foreach (var methodAction in methodActions[typeof (NewMemberAttribute)]) {
				var newMethod = CreateNewMethod(methodAction);
				if (newMethod == null) {
					methodActions.Remove(methodAction);
					continue;
				}
			}
		}

		private void UpdateMethodDeclerations(SimpleTypeLookup<MemberAction<MethodDefinition>> methodActions) {
			foreach (var methodAction in methodActions[typeof (NewMemberAttribute)]) {
				ModifyMethodDecleration(methodAction.YourMember, methodAction.TargetMember);
			} 
		}

		private void ClearReplacedTypes(SimpleTypeLookup<TypeAction> typeActions) {
			foreach (var pairing in typeActions[typeof (ReplaceTypeAttribute)]) {
				Log.Information("Clearing fields in {0:l}", pairing.TargetType.UserFriendlyName());
				pairing.TargetType.Fields.Clear();
			}
		}

		private void IntroduceFields(SimpleTypeLookup<MemberAction<FieldDefinition>> fieldActions) {
			Log.Header("Creating new fields");
			foreach (var fieldAction in fieldActions[typeof (NewMemberAttribute)]) {
				var newField = CreateNewField(fieldAction.TypeAction.TargetType,
					fieldAction.YourMember,
					(NewMemberAttribute) fieldAction.ActionAttribute);
				if (newField == null) {
					fieldActions.Remove(fieldAction);
					continue;
				}
				fieldAction.TargetMember = newField;
			}
		}

		private void IntroduceProperties(SimpleTypeLookup<MemberAction<PropertyDefinition>> propActions) {
			foreach (var propAction in propActions[typeof (NewMemberAttribute)]) {
				var newProperty = CreateNewProperty(propAction.TypeAction.TargetType, propAction.YourMember, (NewMemberAttribute) propAction.ActionAttribute);
				if (newProperty == null) {
					propActions.Remove(propAction);
					continue;
				}
				propAction.TargetMember = newProperty;
			}
		}

		private void IntroduceEvents(SimpleTypeLookup<MemberAction<EventDefinition>> eventActions) {
			foreach (var eventAction in eventActions[typeof (NewMemberAttribute)]) {
				var newEvent = CreateNewEvent(eventAction.TypeAction.TargetType, eventAction.YourMember, (NewMemberAttribute) eventAction.ActionAttribute);
				if (newEvent == null) {
					eventActions.Remove(eventAction);
					continue;
				}
				eventAction.TargetMember = newEvent;
			}
		}

		private void UpdateMethods(SimpleTypeLookup<MemberAction<MethodDefinition>> methodActions) {
			foreach (var methodAction in methodActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
				ModifyMethod(methodAction.TypeAction.TargetType,
					methodAction.YourMember,
					methodAction.ActionAttribute, methodAction.TargetMember);
			}
		}

		private void UpdateProperties(SimpleTypeLookup<MemberAction<PropertyDefinition>> propActions) {
			foreach (var propAction in propActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
				ModifyProperty(propAction.ActionAttribute, propAction.YourMember, propAction.TargetMember);
			}
		}

		private void UpdateFields(SimpleTypeLookup<MemberAction<FieldDefinition>> fieldActions) {
			foreach (var fieldAction in fieldActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
				ModifyField(fieldAction.ActionAttribute, fieldAction.YourMember, fieldAction.TargetMember);
			}
		}

		private void UpdateEvents(SimpleTypeLookup<MemberAction<EventDefinition>> eventActions) {
			foreach (var eventAction in eventActions[typeof (ModifiesMemberAttribute), typeof (NewMemberAttribute)]) {
				ModifyEvent(eventAction.ActionAttribute, eventAction.YourMember, eventAction.TargetMember);
			}
		}

		private void UpdateTypes(SimpleTypeLookup<TypeAction> typeActions) {
			foreach (var modType in typeActions[typeof (NewTypeAttribute)]) {
				ModifyTypeDecleration(modType.YourType);
			}
		}

		private void RemoveMethods(SimpleTypeLookup<MemberAction<MethodDefinition>> methodActions) {
			foreach (var actionParams in methodActions[typeof (RemoveThisMemberAttribute)]) {
				var method = actionParams.TypeAction.TargetType.GetMethodLike(actionParams.YourMember);
				var result = actionParams.TypeAction.TargetType.Methods.Remove(method);
				if (!result) {
					LogFailedToRemove("method", actionParams.YourMember);
				}
			}
		}

		private void RemoveProperties(SimpleTypeLookup<MemberAction<PropertyDefinition>> propActions) {
			foreach (var propAction in propActions[typeof (RemoveThisMemberAttribute)]) {
				var prop = propAction.TypeAction.TargetType.GetPropertyLike(propAction.YourMember);
				var removed = propAction.TypeAction.TargetType.Properties.Remove(prop);
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
			foreach (var eventAction in eventActions[typeof (RemoveThisMemberAttribute)]) {
				var removed =
					eventAction.TypeAction.TargetType.Events.RemoveWhere(x => x.Name == eventAction.YourMember.Name);
				if (!removed) {
					LogFailedToRemove("event", eventAction.YourMember);
				}
			}
		}

		private int _assemblyHistoryIndex = 0;

		private int sum(params int[] xs) => xs.Sum();

		/// <summary>
		/// Applies the patch described in the given PatchingManifest to the TargetAssembly.
		/// </summary>
		/// <param name="manifest">The PatchingManifest. Note that the instance will be populated with additional information (such as newly created types) during execution.</param>
		/// <param name="o"></param>
		public void PatchManifest(PatchingManifest manifest, IProgressMonitor o) {
			/*	The point of this method is to introduce all the patched elements in the correct order.
			
			Standard order of business for order between modify/remove/update:
			1. Remove Xs
			2. Create new Xs
			3. Update existing Xs (+ those we just created)
			 */
			manifest.Specialize(TargetAssembly);
			o.TaskTitle = $"{manifest.PatchAssembly.Name.Name}";
			o.Current = 0;
			o.Total = 11;
			AssemblyDefinition targetAssemblyBackup = null;
			if (UseBackup) {
				targetAssemblyBackup = TargetAssembly.Clone();
			}

			try {
				CurrentMemberCache = manifest.CreateCache();
				//+INTRODUCE NEW TYPES
				//Declare all the new types, in the order of their nesting level (.e.g topmost types, nested types, types nested in those, etc),
				//But don't set anything that references other things, like BaseType, interfaces, custom attributes, etc.
				//Type parameters *are* added at this stage, but no constraints are specified.
				//DEPENDENCIES: None
				o.TaskText = "Introducing Types";
				IntroduceTypes(manifest.TypeActions);
				o.Current++;
				
				//+REMOVE EXISTING METHODS
				//Remove any methods that need removing.
				//DEPENDENCIES: Probably none
				o.TaskText = "Removing Methods";
				RemoveMethods(manifest.MethodActions);
				o.Current++;

				//+INTRODUCE NEW METHOD AND METHOD TYPE DEFINITIONS
				//Declare all the new methods and their type parameters, but do not set nything that references other types,
				//including return type, parameter types, and type parameter constraints
				//This is because method type variables must already exist.
				//This is performed now because types can reference attribute constructors that don't exist until we create them,
				//Don't set custom attributes in this stage.
				//DEPENDENCIES: Type definitions (when introduced to new types)
				o.TaskText = "Introducing Methods";
				IntroduceMethods(manifest.MethodActions);
				o.Current++;

				//+UPDATE METHOD DECLERATIONS
				//Update method declerations with parameters and return types
				//We do this separately, because a method's parameters and return types depend on that method's generic parameters
				//Don't set custom attributes in this stage though it's possible to do so.
				//This is to avoid code duplication.
				//DEPENDENCIES: Type definitions, method definitions
				o.TaskText = "Updating Type Declerations";
				UpdateMethodDeclerations(manifest.MethodActions);
				o.Current++;

				//+IMPORT ASSEMBLY/MODULE CUSTOM ATTRIBUTES
				//Add any custom attributes for things that aren't part of the method/type hierarchy, such as assemblies.
				//DEPENDENCIES: Method definitions, type definitions. (for attribute types and attribute constructors)
				o.TaskText = "Importing Assembly/Module Attributes";
				var patchingAssembly = manifest.PatchAssembly;

				//for assmebly:
				CopyCustomAttributesByImportAttribute(TargetAssembly, patchingAssembly,
					patchingAssembly.GetCustomAttribute<ImportCustomAttributesAttribute>());

				//for module:
				CopyCustomAttributesByImportAttribute(TargetAssembly.MainModule, patchingAssembly.MainModule,
					patchingAssembly.MainModule.GetCustomAttribute<ImportCustomAttributesAttribute>());
				o.Current++;

				//+UPDATE TYPE DEFINITIONS
				//Set the type information we didn't set in (1), according to the same order, including custom attributes.
				//When/if modifying type inheritance is allowed, this process will occur at this stage.
				//DEPENDENCIES: Type definitions (obviously), method definitions (for attribute constructors)
				o.TaskText = "Updating Type Definitions";
				UpdateTypes(manifest.TypeActions);
				o.Current++;

				//+FIELDS
				//remove/define/modify fields. This is where most enum processing happens. Custom attributes are set at this stage.
				//DEPENDENCIES: Type definitions, method definitions (for attribute constructors)
				o.TaskText = "Processing Fields";
				ClearReplacedTypes(manifest.TypeActions);
				RemoveFields(manifest.FieldActions);
				IntroduceFields(manifest.FieldActions);
				UpdateFields(manifest.FieldActions);
				o.Current++;

				//+PROPERTIES
				//Remove/define/modify properties. This doesn't change anything about methods. Custom attributes are set at this stage.
				//DEPENDENCIES: Type definitions, method definitions (attribute constructors, getter/setters)
				o.TaskText = "Processing Properties";
				RemoveProperties(manifest.PropertyActions);
				IntroduceProperties(manifest.PropertyActions);
				UpdateProperties(manifest.PropertyActions);
				o.Current++;

				//+EVENTS
				//Remove/define/modify events. This doesn't change anything about methods. Custom attributes are set at this stage.
				//DEPENENCIES: Type definitions, method definitions (attribute constructors, add/remove/invoke handlers)
				o.TaskText = "Introducing Events";
				RemoveEvents(manifest.EventActions);
				IntroduceEvents(manifest.EventActions);
				UpdateEvents(manifest.EventActions);
				o.Current++;

				//+FINALIZE METHODS, GENERATE METHOD BODIES
				//Fill/modify the bodies of all remaining methods, and also modify accessibility/attributes of existing ones.
				//Note that methods that are the source of DuplicatesBody will be resolved in their unmodified version,
				//so the modifications won't influence this functionality.
				//Custom attributes are set at this stage.
				//Also, explicit overrides (what in C# is explicit interface implementation) are specified here.
				//DEPENDENCIES: Type definitions, method definitions, method signature elements, field definitions
				o.TaskText = "Updating Method Bodies";
				UpdateMethods(manifest.MethodActions);
				o.Current++;

				//+ADD PATCHING HISTORY TO ASSEMBLY
				o.TaskText = "Updating History";
				if (EmbedHistory) {

					TargetAssembly.AddPatchedByAssemblyAttribute(manifest.PatchAssembly, _assemblyHistoryIndex++, OriginalAssemblyMetadata);	
				}
				o.Current++;
			}
			catch {
				if (UseBackup) {
					TargetAssembly = targetAssemblyBackup;	
				}
				throw;
			}
		}

		///  <summary>
		///  This method runs the PEVerify command-line tool on the patched assembly. It does this by first writing it to a temporary file.<br/>
		/// PEVerify is a tool that verifies IL. It goes over it and looks for various issues.<br/>
		/// Some of the errors it reports are relatively harmless but others mean the assembly cannot be loaded.<br/>
		/// Ideally, it should report no errors.<br/>
		/// This operation returns an extended and user-friendly form of the output, translating metadata tokens into user-readable names.
		///  </summary>
		/// <returns></returns>
		public PEVerifyOutput RunPeVerify(PEVerifyInput input) {
			return PeVerifyRunner.RunPeVerify(TargetAssembly, input);
		}

		/// <summary>
		/// Writes the patched result to file.
		/// </summary>
		/// <param name="path">The path into which the assembly will be written to.</param>
		public void WriteTo(string path) {
			
			Log.Information("Writing assembly {@OrigName:l} [{@OrigPath:l}] to location {@DestPath:l}",
				TargetAssembly.Name.Name,
				PathHelper.GetUserFriendlyPath(TargetAssembly.MainModule.FullyQualifiedName),
				PathHelper.GetUserFriendlyPath(path));
			TargetAssembly.Write(path);
			Log.Information("Write completed successfuly.");
		}

	}

}