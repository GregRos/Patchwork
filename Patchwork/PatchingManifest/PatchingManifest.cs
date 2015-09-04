using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Patchwork.Attributes;
using Patchwork.Collections;

namespace Patchwork {
	public class PatchingManifest {

		public AssemblyDefinition PatchingAssembly {
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

	}
}
