using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using Patchwork.Attributes;
using Patchwork.Utility.Binding;
using Serilog;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Patchwork.Utility {



	/// <summary>
	///     Helper methods (mainly extension methods) for working with Cecil and .NET reflection classes. Some are publically visible.
	/// </summary>
	public static class CecilHelper {
		/// <summary>
		///     Makes an assembly 'open', whic7h means that everything is public and nothing is sealed. Ideal for writing a patching
		///     assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <param name="modifyEvents">if set to <c>true</c> [modify events].</param>
		public static void MakeOpenAssembly(AssemblyDefinition assembly, bool modifyEvents) {
			var allTypes = assembly.MainModule.GetAllTypes();
			allTypes = allTypes.ToList();
			foreach (var type in allTypes) {
				foreach (var field in type.Fields) {
					field.SetAccessibility(Accessibility.Public);
					field.IsInitOnly = false;
				}
				foreach (var method in type.Methods) {
					method.SetAccessibility(Accessibility.Public);
				}
				if (modifyEvents) {
					foreach (var vent in type.Events) {
						if (type.Fields.Any(x => x.Name == vent.Name) || type.Properties.Any(x => x.Name == vent.Name)) {
							vent.Name += "Event";
						}
					}
				}
				type.IsSealed = false;
				type.SetAccessibility(Accessibility.Public);
			}
		}

		public static AssemblyDefinition Clone(this AssemblyDefinition definition) {
			return AssemblyDefinition.ReadAssembly(new MemoryStream(definition.SerializeAssembly()));
		}

		public static byte[] SerializeAssembly(this AssemblyDefinition definition) {
			var ms = new MemoryStream();
			definition.Write(ms);
			ms.Flush();
			return ms.GetBuffer();
		}

		/// <summary>
		///     Returns a MethodReference to the method. Note that the DeclaringType, ReturnType, etc, aren't fixed.
		/// </summary>
		/// <param name="methodDef">The method definition.</param>
		/// <returns></returns>
		public static MethodReference MakeReference(this MethodReference methodDef) {
			var refTo = new MethodReference(methodDef.Name, methodDef.ReturnType, methodDef.DeclaringType) {
				CallingConvention = methodDef.CallingConvention,
				HasThis = methodDef.HasThis, //important
				ExplicitThis = methodDef.ExplicitThis //important,
			};
			
			foreach (var par in methodDef.Parameters) {
				refTo.Parameters.Add(new ParameterDefinition(par.ParameterType));
			}
			foreach (var gen in methodDef.GenericParameters) {
				refTo.GenericParameters.Add(new GenericParameter(gen.Name, refTo));
			}
			return refTo;
		}

		/// <summary>
		///     Returns a MethodReference to the method. Note that the DeclaringType, ReturnType, etc, aren't fixed.
		/// </summary>
		/// <param name="methodDef">The method definition.</param>
		/// <returns></returns>
		public static FieldReference MakeReference(this FieldReference methodDef) {
			var refTo = new FieldReference(methodDef.Name, methodDef.FieldType, methodDef.DeclaringType);
			return refTo;
		}

		/// <summary>
		///     Gets the C#-like accessbility of this member.
		/// </summary>
		/// <param name="memberDef">The member definition.</param>
		/// <returns></returns>
		internal static Accessibility GetAccessbility(this IMemberDefinition memberDef) {
			return GetAccessibilityDynamic(memberDef);
		}

		/// <summary>
		///     Sets the accessibility attributes of this member to the desired C#-like accessibility.
		/// </summary>
		/// <param name="method">The method.</param>
		/// <param name="newAccessibility">The new accessibility.</param>
		internal static void SetAccessibility(this IMemberDefinition method, Accessibility newAccessibility) {
			SetAccessibilityDynamic(method, newAccessibility);
		}


		private static Accessibility GetAccessibilityDynamic(dynamic whatever) {
			//TODO: Now that I know how things work better, re-implement this method using Attributes

			//the different things that have accessibility don't really have a unifying interface...
			//so we have to resort to this.
			var asTypeDef = whatever as TypeDefinition;
			if (asTypeDef != null && asTypeDef.IsNested) {
				if (asTypeDef.IsNestedAssembly) { return Accessibility.Internal; }
				if (asTypeDef.IsNestedFamily) { return Accessibility.Protected; }
				if (asTypeDef.IsNestedPrivate) { return Accessibility.Private; }
				if (asTypeDef.IsNestedPublic) { return Accessibility.Public; }
				if (asTypeDef.IsNestedFamilyOrAssembly) { return Accessibility.ProtectedInternal; }
			} else if (asTypeDef != null) {
				if (asTypeDef.IsNotPublic) { return Accessibility.Internal; }
			} else {

				if (whatever.IsPrivate) { return Accessibility.Private; }
				if (whatever.IsFamily) { return Accessibility.Protected; }
				if (whatever.IsFamilyOrAssembly) { return Accessibility.ProtectedInternal; }
				if (whatever.IsAssembly) { return Accessibility.Internal; }
			}
			if (whatever.IsPublic) { return Accessibility.Public; }
			return Accessibility.Other;
		}

		private static void SetAccessibilityDynamic(dynamic whatever, Accessibility newAccessibility) {
			//TODO: Now that I know how things work better, re-implement this method using Attributes

			//not sure if the following is required or not. seems safer.
			var asTypeDef = whatever as TypeDefinition;
			if (asTypeDef != null && asTypeDef.IsNested) {
				asTypeDef.IsNestedAssembly = false;
				asTypeDef.IsNestedFamily = false;
				asTypeDef.IsNestedPublic = false;
				asTypeDef.IsNestedPrivate = false;
				asTypeDef.IsNestedFamilyOrAssembly = false;
				asTypeDef.IsNestedFamilyAndAssembly = false;
			} else if (asTypeDef != null) {
				asTypeDef.IsPublic = false;
				asTypeDef.IsNotPublic = false;
			} else {
				whatever.IsPrivate = false;
				whatever.IsPublic = false;
				whatever.IsFamilyOrAssembly = false;
				whatever.IsFamilyAndAssembly = false;
				whatever.IsAssembly = false;
				whatever.IsFamily = false;
			}

			switch (newAccessibility) {
				case Accessibility.Public:
					if (asTypeDef != null && asTypeDef.IsNested) {
						//making a nested type IsPublic is not a good idea... trust me on this one...
						asTypeDef.IsNestedPublic = true;
					} else {
						whatever.IsPublic = true;
					}
					break;
				case Accessibility.ProtectedInternal:
					if (asTypeDef != null && asTypeDef.IsNested) {
						asTypeDef.IsNestedFamilyOrAssembly = true;
					} else if (asTypeDef != null) {
						throw new ArgumentException("Cannot apply this accessibility to a non-nested type.");
					} else {
						whatever.IsFamilyOrAssembly = true;
					}
					break;
				case Accessibility.Internal:
					if (asTypeDef != null && asTypeDef.IsNested) {
						asTypeDef.IsNestedAssembly = true;
					} else if (asTypeDef != null) {
						asTypeDef.IsNotPublic = true;
					} else {
						whatever.IsAssembly = true;
					}
					break;
				case Accessibility.Private:
					if (asTypeDef != null && asTypeDef.IsNested) {
						asTypeDef.IsNestedPrivate = true;
					} else if (asTypeDef != null) {
						throw new ArgumentException("Cannot apply this accessibility to a non-nested type.");
					} else {
						whatever.IsPrivate = true;
					}
					break;
				case Accessibility.Protected:
					if (asTypeDef != null && asTypeDef.IsNested) {
						asTypeDef.IsNestedFamily = true;
					} else if (asTypeDef != null) {
						throw new ArgumentException("Cannot apply this accessibility to a non-nested type.");
					} else {
						whatever.IsFamily = true;
					}
					break;
				default:
					throw new ArgumentException("Unknown accessibility.");
			}
		}


		public static readonly string PatchworkMetadataString =
			typeof (AssemblyPatcher).Assembly.GetAssemblyMetadataString();

		public static string GetAssemblyMetadataString(this AssemblyDefinition assembly) {
			return GetAssemblyMetadataString(assembly.FullName, assembly.MainModule.FullyQualifiedName);
		}

		public static string GetAssemblyMetadataString(this Assembly assembly) {
			return GetAssemblyMetadataString(assembly.FullName, assembly.Location);
		}

		public static string GetAssemblyMetadataString(string fullName, string path) {
			if (!File.Exists(path)) {
				return $"[FullName = '{fullName}']";
			}
			var fileInfo = new FileInfo(path);
			var lastWriteTime = fileInfo.LastWriteTime;
			var size = fileInfo.Length;
			var metadataString =
				$@"[FullName = '{fullName}', Path = '{path}', Length = '{size}']";
			return metadataString;
		}
	}
}