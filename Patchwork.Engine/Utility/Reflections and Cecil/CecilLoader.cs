using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Serilog;

namespace Patchwork.Engine.Utility {
	/// <summary>
	/// Helper and extension methods for loading code elements from their Cecil metadata.
	/// </summary>
	public static class CecilLoader {
		private static readonly Dictionary<MethodDefinition, MethodBase> _methodCache = new Dictionary<MethodDefinition, MethodBase>();

		/// <summary>
		/// Loads a type using its Cecil definition. Involves loading the assembly from disk.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <returns></returns>
		/// <exception cref="TypeLoadException">Could not find the type from its Mono.Cecil identifiers, or could not load the
		/// assembly.</exception>
		public static Type LoadType(this TypeDefinition typeDef) {	
			var assembly = typeDef.Module.Assembly.LoadLocalAssembly();
			var ret = assembly.GetType(typeDef.FullName.Replace("/", "+"));
			if (ret == null) {
				throw new TypeLoadException("Could not find the type from its Mono.Cecil full name.");
			}
			return ret;
		}

		/// <summary>
		///     Loads the property from its Cecil metadata. Involves loading the assembly from disk.
		/// </summary>
		/// <param name="propDef">The property definition.</param>
		/// <returns></returns>
		/// <exception cref="System.MissingMemberException">Could not find the property.</exception>
		internal static PropertyInfo LoadProperty(this PropertyDefinition propDef) {
			var type = propDef.DeclaringType.LoadType();
			var prop = type.GetProperty(propDef.Name, CommonBindingFlags.Everything | BindingFlags.DeclaredOnly);
			if (prop == null) {
				throw new MissingMemberException("Could not find the property.");
			}
			return prop;
		}

		/// <summary>
		///     Loads the member from its IMemberDefinition.
		/// </summary>
		/// <param name="memberDef">The member definition.</param>
		/// <returns></returns>
		/// <exception cref="System.NotSupportedException">Unknown IMemberDefinition type.</exception>
		internal static MemberInfo LoadMember(this IMemberDefinition memberDef) {
			var propDef = memberDef as PropertyDefinition;
			if (propDef != null) {
				return propDef.LoadProperty();
			}
			var methodDef = memberDef as MethodDefinition;
			if (methodDef != null) {
				return methodDef.LoadMethod();
			}
			var fieldDef = memberDef as FieldDefinition;
			if (fieldDef != null) {
				return fieldDef.LoadField();
			}
			var typeDef = memberDef as TypeDefinition;
			if (typeDef != null) {
				return typeDef.LoadType();
			}
			throw new NotSupportedException("This member definition is not supported.");
		}

		/// <summary>
		///     Tries to load the assembly from disk by using its Cecil identifiers. Can fail, so don't use it often.
		/// </summary>
		/// <param name="assemblyDef">The assembly definition.</param>
		/// <returns></returns>
		/// <exception cref="TypeLoadException">Could not load the assembly from its Mono.Cecil definition.</exception>
		public static Assembly LoadLocalAssembly(this AssemblyDefinition assemblyDef) {
			Exception prevException;
			try {
				return Assembly.Load(assemblyDef.FullName);
			}
			catch (Exception ex) {
				Log.Error(
					"Failed to LoadLocal the assembly '{@AssemblyName}' from its full name. Going to try FullyQualifiedName as path.",
					assemblyDef.FullName);
				prevException = ex;
			}
			try {
				//FullyQualifiedName is normally the path to the file, if it was loaded from a file.
				return Assembly.LoadFrom(assemblyDef.MainModule.FullyQualifiedName);
			}
			catch (Exception ex) {
				throw new TypeLoadException(
					"Could not load the assembly from its Mono.Cecil definition.",
					new AggregateException(ex, prevException));
			}
		}

		/// <summary>
		///     Converts a Cecil.MethodDefinition to a MethodBase. Also converts constructors. Requires loading the type and/or
		///     assembly.
		/// </summary>
		/// <param name="methodDef">The method definition.</param>
		/// <returns></returns>
		/// <exception cref="MissingMemberException">No member matched the search criteria.</exception>
		/// <exception cref="TypeLoadException">Could not load the type or assembly.</exception>
		/// <exception cref="AmbiguousMatchException">More than one member matched the search criteria.</exception>
		public static MethodBase LoadMethod(this MethodDefinition methodDef) {
			var type = methodDef.DeclaringType.LoadType();
			IEnumerable<MethodBase> rets;
			if (_methodCache.ContainsKey(methodDef)) {
				return _methodCache[methodDef];
			}
			if (methodDef.IsConstructor) {
				var constructors =
					from ctor in type.GetConstructors(CommonBindingFlags.Everything | BindingFlags.DeclaredOnly)
					where ctor.Name == methodDef.Name
					where
						ctor.GetParameters()
							.Select(x => x.ParameterType.Name)
							.SequenceEqual(methodDef.Parameters.Select(x => x.ParameterType.Name))
					select ctor as MethodBase;
				rets = constructors;
			} else {
				var methods =
					from method in type.GetMethods(CommonBindingFlags.Everything | BindingFlags.DeclaredOnly)
					where method.Name == methodDef.Name
					where method.GetParameters()
						.Select(x => x.ParameterType.Name)
						.SequenceEqual(methodDef.Parameters.Select(x => x.ParameterType.Name))
					select method as MethodBase;
				rets = methods;
			}
			rets = rets.ToList();
			if (!rets.Any()) {
				throw new MissingMemberException("No member matched the search criteria.");
			}
			if (rets.Count() > 1) {
				throw new AmbiguousMatchException("More than one member matched the search criteria.");
			}
			
			var ret = rets.Single();
			_methodCache[methodDef] = ret;
			return ret;

		}

		/// <summary>
		///     Loads a Cecil.FieldDefinition as a System.FieldInfo. Requires loading the assembly.
		/// </summary>
		/// <param name="fieldDef">The field definition.</param>
		/// <returns></returns>
		/// <exception cref="System.MissingMemberException">Could not find the field.</exception>
		public static FieldInfo LoadField(this FieldDefinition fieldDef) {
			var type = fieldDef.DeclaringType.LoadType();
			var field = type.GetField(fieldDef.Name, CommonBindingFlags.Everything | BindingFlags.DeclaredOnly);
			if (field == null) {
				throw new MissingMemberException("Could not find the field.");
			}
			return field;
		}

		/// <summary>
		///     Loads a copy of the assembly from memory.
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <param name="reflectionOnly"></param>
		/// <returns></returns>
		public static Assembly LoadIntoMemory(this AssemblyDefinition def, bool reflectionOnly = false) {
			var ms = new MemoryStream();
			def.Write(ms);
			var arr = ms.ToArray();
			
			var assembly = reflectionOnly ? Assembly.ReflectionOnlyLoad(arr) : Assembly.Load(arr);
			return assembly;
		}
	}
}