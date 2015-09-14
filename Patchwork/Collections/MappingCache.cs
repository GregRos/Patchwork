using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Patchwork.Collections {
	internal class MemberCache {
		public IDictionary<MethodDefinition, MemberAction<MethodDefinition>> Methods {
			get;
		} = new Dictionary<MethodDefinition, MemberAction<MethodDefinition>>();

		public IDictionary<PropertyDefinition, MemberAction<PropertyDefinition>> Properties {
			get;
		} = new Dictionary<PropertyDefinition, MemberAction<PropertyDefinition>>();

		public IDictionary<FieldDefinition, MemberAction<FieldDefinition>> Fields {
			get;
		} = new Dictionary<FieldDefinition, MemberAction<FieldDefinition>>();

		public IDictionary<EventDefinition, MemberAction<EventDefinition>> Events {
			get;
		} = new Dictionary<EventDefinition, MemberAction<EventDefinition>>();

		public IDictionary<TypeDefinition, TypeAction> Types {
			get;
		} = new Dictionary<TypeDefinition, TypeAction>();
	}

}
