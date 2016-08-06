using System;
using System.Linq;
using Mono.Cecil;
using Patchwork.AutoPatching;
using Patchwork.Engine.Utility;

namespace Patchwork.Engine {


	internal static class Errors {

		public static PatchDeclerationException No_patch_assembly_attribute(AssemblyDefinition assembly) {
			return new PatchDeclerationException($"The assembly does not have the {nameof(PatchAssemblyAttribute)} attribute. This means that it is not a patch assembly.");
		}

		public static PatchDeclerationException More_than_one_PatchInfoAttribute(AssemblyDefinition assembly) {
			return new PatchDeclerationException(
					$"More than one class was found that is decorated with {nameof(PatchInfoAttribute)}");
		}

		public static PatchDeclerationException PatchInfo_doesnt_have_default_ctor(Type type) {
			return new PatchDeclerationException($"The PatchInfo class {type} does not have a default constructor.");
		}

		public static PatchDeclerationException PatchInfo_doesnt_implement_interface(Type type) {
			return new PatchDeclerationException($"The PatchInfo class {type} does not implement the interface {nameof(IPatchInfo)}.");
		}

		public static PatchDeclerationException Multiple_action_attributes(MemberReference yourMember, object[] attributes) {

			return new PatchDeclerationException($"The member {yourMember.UserFriendlyName()} has more than one action attribute: {attributes.Select(x => x.GetType().Name).Join(", ")}");
			
		}

		public static PatchDeclerationException Unknown_action_attribute(object attribute) {
			return new PatchDeclerationException($"Unknown member/type action attribute: {attribute.GetType().FullName}");	
		}

		public static PatchDeclerationException Invalid_decleration(string format, params object[] args) {
			return new PatchDeclerationException(string.Format("This kind of decleration is illegal at this location. More info: " + format, args));
		}

		public static PatchDeclerationException Duplicate_member(string kind, string createFor, string conflictWith) {
			return new PatchDeclerationException(
				$"The declared new {kind} '{createFor}' conflicts with existing member '{conflictWith}'");
		}

		public static PatchDeclerationException Invalid_member(string kind, MemberReference memberRef, string identifier, string info)
		{
			return new PatchDeclerationException(
				$"The attribute on the {kind} '{memberRef.FullName}' refers to '{identifier}', but that member is invalid in this context. More info: {info}");
		}

		public static PatchDeclerationException Missing_member_in_attribute(string kind, MemberReference memberRef, string identifier) {
			return
				new PatchDeclerationException(
					$"The attribute on the {kind} called '{memberRef.FullName}' refers to '{identifier}', but that member doesn't exist in this context (possibly overload resolution failure).");
		}

		internal static PatchImportException Feature_not_supported(string details) {
			return new PatchImportException("Encountered a feature that isn't supported. Details: " + details);
		}

		internal static PatchImportException Could_not_resolve_reference(string kind, MemberReference yourMemberReference) {
			return new PatchImportException(
				$"Could not resolve a {kind} reference, most likely because it wasn't imported. Details: {yourMemberReference.FullName}");
		}

		
	}
}