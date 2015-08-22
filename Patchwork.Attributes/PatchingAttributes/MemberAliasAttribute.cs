using System;

namespace Patchwork.Attributes
{
	/// <summary>
	/// Changes how calls to this alias are translated in the target assembly.
	/// </summary>
	public enum AliasCallMode {
		/// <summary>
		/// The call to the aliased member will use the same calling convention as the original call.
		/// </summary>
		NoChange,
		/// <summary>
		/// The call to the aliased member will always be a virtual call, even if the original was non-virtual.
		/// </summary>
		Virtual,
		/// <summary>
		/// The call to the aliased member will always be non-virtual, even if the original was virtual. This allows you to call overridden members.
		/// </summary>
		NonVirtual
	}

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
