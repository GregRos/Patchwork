using System;

namespace Patchwork.Attributes
{

	/// <summary>
	/// This attribute turns a membery you declare into an alias of another member. When a reference to this member is encountered in your code, it is replaced by a refernece to the aliased member.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Constructor)]
	public class MemberAliasAttribute : PatchingAttribute {
		public string AliasedMemberName {
			get;
			private set;
		}

		public object AliasedMemberDeclaringType {
			get;
			private set;
		}

		public AliasCallMode CallMode {
			get;
			private set;
		}

		/// <summary>
		/// Creates a new instance of this attribute.
		/// </summary>
		/// <param name="aliasedMemberName">Optionally, the member to be aliased. 
		/// If not specified, the name of the current member is used.</param>
		/// <param name="aliasedMemberDeclaringType">The declaring type of the member. If not specified, the modified type is used.</param>
		/// <param name="callMode">Specifies whether calls to this alias should be translated in some special way.</param>
		public MemberAliasAttribute(string aliasedMemberName = null, object aliasedMemberDeclaringType = null, AliasCallMode callMode = AliasCallMode.NoChange) {
			AliasedMemberName = aliasedMemberName;
			AliasedMemberDeclaringType = aliasedMemberDeclaringType;
			CallMode = callMode;
		}
	}
}
