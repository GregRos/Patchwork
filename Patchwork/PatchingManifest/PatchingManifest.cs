using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Patchwork.Attributes;
using Patchwork.Collections;
using Patchwork.Utility;

namespace Patchwork {
	public class PatchingManifest {

		public AssemblyDefinition PatchAssembly {
			get;
			internal set;
		}

		public PatchExecutionInfo PatchExecution {
			get;
			internal set;
		}

		public SimpleTypeLookup<TypeAction> TypeActions {
			get;
			internal set;
		} = new SimpleTypeLookup<TypeAction>();

		public SimpleTypeLookup<MemberAction<FieldDefinition>> FieldActions {
			get;
			internal set;
		} = new SimpleTypeLookup<MemberAction<FieldDefinition>>();

		public SimpleTypeLookup<MemberAction<MethodDefinition>> MethodActions {
			get;
			internal set;
		} = new SimpleTypeLookup<MemberAction<MethodDefinition>>();

		public SimpleTypeLookup<MemberAction<PropertyDefinition>> PropertyActions {
			get;
			internal set;
		} = new SimpleTypeLookup<MemberAction<PropertyDefinition>>();

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
				targetType = ((TypeReference) asAliased.AliasedMemberDeclaringType).Resolve();
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
			}
		}

		public void Specialize(AssemblyDefinition assemblyDef) {
			SpecializeTypes(TypeActions, assemblyDef);
			SpecializeMembers(EventActions, assemblyDef);
			SpecializeMembers(MethodActions, assemblyDef);
			SpecializeMembers(FieldActions, assemblyDef);
			SpecializeMembers(PropertyActions, assemblyDef);
		}
	}
}

