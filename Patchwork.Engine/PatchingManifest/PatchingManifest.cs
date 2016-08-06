using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Patchwork.AutoPatching;
using Patchwork.Engine.Utility;
using Serilog;

namespace Patchwork.Engine {
	/// <summary>
	/// A patching manifest, which is a collection of patching instructions detected in a patch assembly, organized by kind.
	/// </summary>
	public class PatchingManifest : IDisposable {

		/// <summary>
		/// The patch assembly this manifest was generated from.
		/// </summary>
		public AssemblyDefinition PatchAssembly {
			get;
			internal set;
		}

		/// <summary>
		/// The <see cref="IPatchInfo"/> object for the patch assembly.
		/// </summary>
		public IPatchInfo PatchInfo {
			get;
			internal set;
		}

		/// <summary>
		/// A collection of actions to be performed on types, organized based on attribute type in a special lookup table.
		/// </summary>
		public SimpleTypeLookup<TypeAction> TypeActions {
			get;
			internal set;
		} = new SimpleTypeLookup<TypeAction>();

		/// <summary>
		/// A collection of actions to be performed on fields, organized based on attribute type in a special lookup table.
		/// </summary>
		public SimpleTypeLookup<MemberAction<FieldDefinition>> FieldActions {
			get;
			internal set;
		} = new SimpleTypeLookup<MemberAction<FieldDefinition>>();

		/// <summary>
		/// A collection of actions to be performed on methods, organized based on attribute type in a special lookup table.
		/// </summary>
		public SimpleTypeLookup<MemberAction<MethodDefinition>> MethodActions {
			get;
			internal set;
		} = new SimpleTypeLookup<MemberAction<MethodDefinition>>();
		/// <summary>
		/// A collection of actions to be performed on properties, organized based on attribute type in a special lookup table.
		/// </summary>
		public SimpleTypeLookup<MemberAction<PropertyDefinition>> PropertyActions {
			get;
			internal set;
		} = new SimpleTypeLookup<MemberAction<PropertyDefinition>>();
		/// <summary>
		/// A collection of actions to be performed on events, organized based on attribute type in a special lookup table.
		/// </summary>
		public SimpleTypeLookup<MemberAction<EventDefinition>> EventActions {
			get;
			internal set;
		} = new SimpleTypeLookup<MemberAction<EventDefinition>>();

		private static IEnumerable<KeyValuePair<T, MemberAction<T>>> ToMappingDictionary<T>(SimpleTypeLookup<MemberAction<T>> lookup)
			where T : IMemberDefinition {
			return from action in lookup[typeof (NewMemberAttribute), typeof(ModifiesMemberAttribute), typeof(MemberAliasAttribute)]
				select new KeyValuePair<T, MemberAction<T>>(action.YourMember, action);
		}

		internal MemberCache CreateCache() {
			var cache = new MemberCache();
			cache.Events.AddRange(ToMappingDictionary(EventActions));
			cache.Fields.AddRange(ToMappingDictionary(FieldActions));
			cache.Methods.AddRange(ToMappingDictionary(MethodActions));
			cache.Properties.AddRange(ToMappingDictionary(PropertyActions));

			var typeMappings =
				from action in TypeActions[typeof (ModifiesTypeAttribute), typeof (ReplaceTypeAttribute), typeof(NewTypeAttribute)]
				select new KeyValuePair<TypeDefinition, TypeAction>(action.YourType, action);

			cache.Types.AddRange(typeMappings);
			return cache;
		}

		private T GetPatchedMember<T>(TypeDefinition targetType, T yourMemberDef, MemberActionAttribute actionAttribute = null)
			where T : MemberReference,IMemberDefinition {
			var targetMemberName = GetPatchedMemberName(yourMemberDef,actionAttribute);
			var aliased = actionAttribute as MemberAliasAttribute;
			if (aliased != null) {
				var asAliased = aliased;
				targetType = targetType.Module.MetadataResolver.Resolve((TypeReference) asAliased.AliasedMemberDeclaringType);
			}
			var t = typeof (T);
			if (t == typeof(MethodDefinition)) {
				return targetType.GetMethodLike(yourMemberDef as MethodDefinition, targetMemberName) as T;
			}
			if (t == typeof(PropertyDefinition)) {
				return targetType.GetPropertyLike(yourMemberDef as PropertyDefinition, targetMemberName) as T;
			}
			if (t == typeof(FieldDefinition)) {
				return targetType.GetField(targetMemberName) as T;
			}
			if (t == typeof (EventDefinition)) {
				return targetType.GetEvent(targetMemberName) as T;
			}
			throw new ArgumentException($"Unknown member definition of type {t}");
		}

		private static string GetPatchedMemberName(IMemberDefinition yourMemberDef, MemberActionAttribute actionAttribute = null) {
			actionAttribute = actionAttribute ?? yourMemberDef.GetCustomAttribute<MemberActionAttribute>();
			var asModifiesAttr = actionAttribute as ModifiesMemberAttribute;
			var newMemberName = actionAttribute as NewMemberAttribute;
			var asAliasedMember = actionAttribute as MemberAliasAttribute;
			var memberName = asModifiesAttr?.MemberName
				?? asAliasedMember?.AliasedMemberName ?? newMemberName?.NewMemberName ?? yourMemberDef.Name;
			return memberName;
		}

		private void SpecializeTypes(SimpleTypeLookup<TypeAction> lookup, AssemblyDefinition toTargetAssembly) {
			foreach (var item in lookup[typeof (ModifiesTypeAttribute), typeof (ReplaceTypeAttribute)]) {
				var name = item.YourType.GetPatchedTypeFullName();
				item.TargetType = toTargetAssembly.MainModule.GetType(name);
			}
		}

		private void SpecializeMembers<T>(SimpleTypeLookup<MemberAction<T>> lookup, AssemblyDefinition toTargetAssembly) 
			where T : MemberReference,IMemberDefinition  {
			foreach (var item in lookup[typeof (ModifiesMemberAttribute), typeof (MemberAliasAttribute)]) {
				item.TargetMember = GetPatchedMember(item.TypeAction.TargetType, item.YourMember, item.ActionAttribute);
				
				if (item.TargetMember == null) {
					var memberName = GetPatchedMemberName(item.YourMember, item.ActionAttribute);
					throw Errors.Missing_member_in_attribute(DisplayNameHelper.CommonNameForMemberDef<T>(), item.YourMember, memberName);
				} 
			}
		}

		internal void Specialize(AssemblyDefinition assemblyDef) {
			var someResolver = PatchAssembly.MainModule.AssemblyResolver;
			if (someResolver == null) {
				throw new ArgumentException("The specified patch assembly could not be specialized because its AssemblyResolver wasn't a BaseAssemblyResolver. This means Cecil has changed.");
			}
			var resolver = (ExpandedAssemblyResolver) someResolver;
			try {
				resolver.ForceClearCache();
			}
			catch (Exception ex) {
				Log.Error(ex, "Failed to ForceClearCache of an assembly resolver due to an exception.");
				//ForceClearCache isn't really required. It's only relevant when we try to Specialize the same AssemblyDefinition multiple times.
				//And this shouldn't really happen, unless we're debugging.
			}
			//we need this special resolver because we want the patch assembly to resolve the assembly being patched, even if its version is different.
			//We also want to resolve assemblies in the same folder as the patched assembly. While AddSearchDirectory would be sufficient for this,
			//We want resolving the target assembly (the one that's being modified) to be prioritized over the original.
			resolver.RegisterAssembly(assemblyDef);
			resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyDef.MainModule.FullyQualifiedName));
			SpecializeTypes(TypeActions, assemblyDef);
			SpecializeMembers(EventActions, assemblyDef);
			SpecializeMembers(MethodActions, assemblyDef);
			SpecializeMembers(FieldActions, assemblyDef);
			SpecializeMembers(PropertyActions, assemblyDef);
		}

		/// <summary>
		/// Disposes of the AppDomain hosting the PatchInfo instance. This needs to be called so the AppDomain can be unloaded.
		/// </summary>
		public void Dispose() {
			(PatchInfo as IDisposable)?.Dispose();
		}
	}
}

