using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace Patchwork.Engine.Utility {



	/// <summary>
	///     Helper methods (mainly extension methods) for working with Cecil and .NET reflection classes. Some are publically visible.
	/// </summary>
	public static class CecilHelper {

		/// <summary>
		/// Returns a reloaded clone of the assembly by serializing it and loading it from memory.
		/// </summary>
		/// <param name="definition">The assembly definition.</param>
		/// <returns></returns>
		public static AssemblyDefinition Clone(this AssemblyDefinition definition) {
			return AssemblyDefinition.ReadAssembly(new MemoryStream(definition.SerializeAssembly()));
		}

		/// <summary>
		/// Serializes the assembly by writing it into a memory stream.
		/// </summary>
		/// <param name="definition">The assembly definition.</param>
		/// <returns></returns>
		public static byte[] SerializeAssembly(this AssemblyDefinition definition) {
			var ms = new MemoryStream();
			definition.Write(ms);
			ms.Flush();
			return ms.GetBuffer();
		}

		/// <summary>
		///     Creates another refernece to the same method.
		/// </summary>
		/// <param name="methodRef">The method reference.</param>
		/// <returns></returns>
		public static MethodReference CloneReference(this MethodReference methodRef) {
			var refTo = new MethodReference(methodRef.Name, methodRef.ReturnType, methodRef.DeclaringType) {
				CallingConvention = methodRef.CallingConvention,
				HasThis = methodRef.HasThis, //important
				ExplicitThis = methodRef.ExplicitThis //important,
			};
			
			foreach (var par in methodRef.Parameters) {
				refTo.Parameters.Add(new ParameterDefinition(par.ParameterType));
			}
			foreach (var gen in methodRef.GenericParameters) {
				refTo.GenericParameters.Add(new GenericParameter(gen.Name, refTo));
			}
			return refTo;
		}

		/// <summary>
		///     Returns another reference to the same field.
		/// </summary>
		/// <param name="methodDef">The field reference.</param>
		/// <returns></returns>
		public static FieldReference CloneReference(this FieldReference methodDef) {
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
		public static void SetAccessibility(this IMemberDefinition method, Accessibility newAccessibility) {
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

		/// <summary>
		/// Contains the metadata string of the executing Patchwork.Engine assembly, as determined by the <see cref="GetAssemblyMetadataString(Mono.Cecil.AssemblyDefinition)"/> method.
		/// </summary>
		public static readonly string PatchworkMetadataString =
			typeof (AssemblyPatcher).Assembly.GetAssemblyMetadataString();

		/// <summary>
		/// Returns a human-readable metadata string that describes the specified Cecil assembly definition.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns></returns>
		public static string GetAssemblyMetadataString(this AssemblyDefinition assembly) {
			return GetAssemblyMetadataString(assembly.FullName, assembly.MainModule.FullyQualifiedName);
		}

		/// <summary>
		/// Returns a human-readable metadata string that describes the specified proper, loaded assembly.
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		public static string GetAssemblyMetadataString(this Assembly assembly) {
			return GetAssemblyMetadataString(assembly.FullName, assembly.Location);
		}

		/// <summary>
		/// Returns the metadata string that describes the assembly in the specified location, expected to have the specified full name.
		/// </summary>
		/// <param name="fullName">The full name of the assembly expected. Mainly used </param>
		/// <param name="path"></param>
		/// <returns></returns>
		private static string GetAssemblyMetadataString(string fullName, string path) {
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