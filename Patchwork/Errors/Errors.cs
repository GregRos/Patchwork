using System;
using System.Linq;
using Mono.Cecil;
using Patchwork.Utility;

namespace Patchwork {


	internal static class Errors {

		public static PatchDeclerationException Multiple_action_attributes(MemberReference yourMember, object[] attributes) {

			return new PatchDeclerationException($"The member {yourMember.UserFriendlyName()} has more than one action attribute: {attributes.Select(x => x.GetType().Name)}");
			
		}

		public static PatchDeclerationException Unknown_action_attribute(object attribute) {
			return new PatchDeclerationException(String.Format("Unknown member/type action attribute: {0}", attribute.GetType().FullName));	
		}

		public static PatchDeclerationException Invalid_decleration(string format, params object[] args) {
			return new PatchDeclerationException(string.Format("This kind of decleration is illegal at this location. More info: " + format, args));
		}

		public static PatchDeclerationException Duplicate_member(string kind, string createFor, string conflictWith) {
			return new PatchDeclerationException(string.Format("The declared new {0} '{1}' conflicts with existing member '{2}'", kind, createFor, conflictWith));
		}

		public static PatchDeclerationException Invalid_member(string kind, MemberReference memberRef, string identifier, string info)
		{
			return new PatchDeclerationException(
				string.Format(
				"The attribute on '{1}' refers to '{2}', but that member is invalid in this context. More info: {3}",
				kind,
				memberRef.FullName,
				identifier,
				info
				));
		}

		public static PatchDeclerationException Missing_member_in_attribute(string kind, MemberReference memberRef, string identifier) {
			return new PatchDeclerationException(
				string.Format(
				"The attribute on '{1}' refers to '{2}', but that member doesn't exist.", 
				kind, 
				memberRef.FullName,
				identifier
				));
		}

		internal static PatchImportException Feature_not_supported(string format, params object[] args) {
			return new PatchImportException("Encountered a feature that isn't supported. Details: " + string.Format(format, args));
		}

		internal static PatchImportException Could_not_resolve_reference(string kind, MemberReference yourMemberReference) {
			return new PatchImportException(string.Format(
				"Could not resolve a {0} reference, most likely because it wasn't imported. Details: {1}", kind, yourMemberReference.FullName));
		}

		
	}
}