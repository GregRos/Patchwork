using System.Collections.Generic;
using Mono.Cecil;

namespace Patchwork.Engine.Utility {
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
