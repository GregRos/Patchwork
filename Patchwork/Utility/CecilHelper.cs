using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using Patchwork.Attributes;
using Serilog;
using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;
namespace Patchwork.Utility {
	/// <summary>
	///     Helper methods (mainly extension methods) for working with Cecil and .NET reflection classes. Some are publically visible.
	/// </summary>
	public static class CecilHelper {
		/// <summary>
		///     Makes an assembly 'open', which means that everything is public and nothing is sealed. Ideal for writing a patching
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
			return ms.ToArray();
		}

		public static MethodDefinition MaybeResolve(this MethodReference mRef) {
			return mRef == null ? null : mRef.Resolve();
		}

		private static ConstructorInfo _instructionConstructorInfo;

		public static Instruction CreateInstruction(OpCode opCode, object operand) {
			//I have to do this because the constructor is internal for some strange reason. Other ways of creating instructions
			//involve strongly-typed signatures which will require switch statements to get around.
			//While it's true I could modify the source and make it public, this isn't a good idea.
			if (_instructionConstructorInfo == null) {
				_instructionConstructorInfo = typeof (Instruction).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
					null, new[] {
						typeof (OpCode), typeof (object)
					}, null);
				if (_instructionConstructorInfo == null) {
					throw new MissingMemberException("Could not find the Instruction constructor");
				}
			}
			var instr = _instructionConstructorInfo.Invoke(new[] {
				opCode, operand
			});
			return (Instruction) instr;
		}


		/// <summary>
		///     Loads a copy of the assembly from memory.
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <returns></returns>
		public static Assembly LoadIntoMemory(this AssemblyDefinition def) {
			var ms = new MemoryStream();
			def.Write(ms);
			var arr = ms.ToArray();
			var assembly = Assembly.Load(arr);
			return assembly;
		}

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
		///     Gets the field.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static FieldDefinition GetField(this TypeDefinition typeDef, string name) {
			return typeDef.Fields.SingleOrDefault(x => x.Name == name);
		}

		/// <summary>
		/// Returns the event with the specified name, or null.
		/// </summary>
		/// <param name="typeDef"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static EventDefinition GetEvent(this TypeDefinition typeDef, string name) {
			var vent = typeDef.Events.SingleOrDefault(e => e.Name == name);
			return vent;
		}

		/// <summary>
		///     Gets the property.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <param name="name">The name.</param>
		/// <param name="types"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyDefinition> GetProperties(this TypeDefinition typeDef, string name, IEnumerable<TypeReference> types) {
			var typesList = types.ToList();
			var props = 
				from prop in typeDef.Properties
				where prop.Name == name
				&& prop.Parameters.Count == typesList.Count()
				&& prop.Parameters.Select(x => x.ParameterType).Zip(typesList, AreTypesEquivForOverloading).All(x => x)
				select prop;

			return props;
		}

		public static IEnumerable<PropertyDefinition> GetPropertiesLike(this TypeDefinition typeDef,
			PropertyDefinition likeWhat) {
			return typeDef.GetProperties(likeWhat.Name, likeWhat.Parameters.Select(x => x.ParameterType));
		} 

		/// <summary>
		///     Gets the property.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static PropertyDefinition GetProperty(this TypeDefinition typeDef, string name, IEnumerable<TypeReference> types) {
			return typeDef.GetProperties(name, types).SingleOrDefault();
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
		///     Determines whether the custom attribute provider has the right custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="memberDef">The member definition.</param>
		/// <returns></returns>
		public static bool HasCustomAttribute<T>(this ICustomAttributeProvider memberDef) {
			return memberDef.GetCustomAttributes<T>().Any();
		}

		/// <summary>
		///     Finds a nested type with the specified local name in the given type. It only works for immediate descendants.
		/// </summary>
		/// <param name="typeDef">The type definition.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		/// <exception cref="System.Reflection.AmbiguousMatchException">More than one nested type matched the search criteria.</exception>
		/// <exception cref="System.MissingMemberException">Could not find a nested type with that name.</exception>
		public static TypeReference GetNestedType(this TypeReference typeDef, string name) {
			var ret = typeDef.Resolve().NestedTypes.Where(x => x.Name == name).Select(x => x.Resolve()).ToList();

			if (ret.Count > 1) {
				throw new AmbiguousMatchException("More than one nested type matched the search criteria.");
			}
			if (ret.Count == 0) {
				throw new MissingMemberException("Could not find a nested type with that name.");
			}
			return ret[0];
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

		private static readonly Dictionary<MethodDefinition, MethodBase> _methodCache = new Dictionary<MethodDefinition, MethodBase>(); 

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

		/// <summary>
		///     Determines whether this is a patching assembly. Normally, if it has PatchingAssemblyAttribute.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns></returns>
		internal static bool IsPatchingAssembly(this AssemblyDefinition assembly) {
			return assembly.HasCustomAttribute<PatchAssemblyAttribute>();
		}

		private static readonly Dictionary<ICustomAttributeProvider, List<object>> _attributeCache =
			new Dictionary<ICustomAttributeProvider, List<object>>();


		private static IEnumerable<object> GetAllCustomAttributes(ICustomAttributeProvider provider) {
			if (_attributeCache.ContainsKey(provider)) {
				return _attributeCache[provider];
			}
			var attributes =
				from attr in provider.CustomAttributes
				where attr.AttributeType.FullName.ContainsAny("Patchwork", "System")
				select attr.TryConstructAttribute();
			_attributeCache[provider] = attributes.ToList();
			return _attributeCache[provider];
		} 

		/// <summary>
		///     Gets the custom attributes. However, it can fail in some cases, so use it with care.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="provider">The provider.</param>
		/// <returns></returns>
		internal static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider provider) {
			//this is a bit more complicated than you'd think. If we try to load the type itself, we'll get an error about dependencies.
			//loading in a ReflectionOnly context will create problems of their own.
			//so we're passively reading the attributes using Cecil, without loading anything, until we find the one we want
			//then we just instantiate another copy, using the same constructor parameters.
			return GetAllCustomAttributes(provider).OfType<T>();
		}

		/// <summary>
		///     Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="provider">The provider.</param>
		/// <returns></returns>
		internal static T GetCustomAttribute<T>(this ICustomAttributeProvider provider) {
			return provider.GetCustomAttributes<T>().FirstOrDefault();
		}

		internal static void AddCustomAttribute(this ICustomAttributeProvider provider, ModuleDefinition module,
			Type attribType,
			params object[] constructorArgs) {
			var ctor =
				attribType.GetConstructor(CommonBindingFlags.Everything, null, constructorArgs.Select(x => x.GetType()).ToArray(),
					null);
			var imported = module.Import(ctor);
			var ctorArgs =
				from ctorArg in constructorArgs
				let type = module.Import(ctorArg.GetType())
				select new CustomAttributeArgument(type, ctorArg);

			var customAttr = new CustomAttribute(imported);
			customAttr.ConstructorArguments.AddRange(ctorArgs);
			provider.CustomAttributes.Add(customAttr);
		} 

		/// <summary>
		///     If given a ref to a patching type, returns the type that it patches. Otherwise, returns null.
		/// </summary>
		/// <param name="typeRef">The type reference.</param>
		/// <returns></returns>
		internal static string GetPatchedTypeFullName(this TypeReference typeRef) {
			var typeActionAttr = typeRef.Resolve().GetCustomAttribute<TypeActionAttribute>();
			if (typeActionAttr is NewTypeAttribute || typeActionAttr == null) {
				if (!typeRef.IsNested) {
					return typeRef.FullName;
				}
				var declaringType = typeRef.DeclaringType;
				declaringType.AssertUnequal(null);
				var decPatchedName = declaringType.GetPatchedTypeFullName();
				return decPatchedName + "/" + typeRef.Name;
			}
			var typeAttr = typeActionAttr as ModifiesTypeAttribute;
			if (typeAttr == null) {
				throw new NotSupportedException("Encountered an unknown TypeActionAttribute.");
			}
			return typeAttr.FullTypeName == "base" ? typeRef.Resolve().BaseType.FullName : typeAttr.FullTypeName;
		}

		internal static bool IsDisablePatching(this IMemberDefinition member) {
			if (member == null) { return false; }
			return member.HasCustomAttribute<DisablePatchingAttribute>()
				|| (member.DeclaringType != null && member.HasCustomAttribute<DisablePatchingAttribute>());
		}

		/// <summary>
		///     Determines whether the member was compiler generated.
		/// </summary>
		/// <param name="attrProvider">The attribute provider.</param>
		/// <returns></returns>
		internal static bool IsCompilerGenerated(this IMemberDefinition attrProvider) {

			return attrProvider.HasCustomAttribute<CompilerGeneratedAttribute>()
				|| (attrProvider.DeclaringType?.IsCompilerGenerated() == true);
		}

		public static MethodReference GetMethod<T>(this ModuleDefinition module, Expression<Func<T>> expr) {
			return module.Import(ExprHelper.GetMethod(expr));
		}

		public static MethodReference GetMethod(this ModuleDefinition module, Expression<Action> expr) {
			return module.Import(ExprHelper.GetMethod(expr));
		}

		/// <summary>
		///     Returns a user-friendly name for the reference.
		///     It's not as short as Name, but not as long as FullName.
		/// </summary>
		/// <param name="memberRef">The member reference.</param>
		/// <returns></returns>
		internal static string UserFriendlyName(this MemberReference memberRef) {
			if (memberRef is FieldReference) {
				return UserFriendlyName((FieldReference) memberRef);
			}
			if (memberRef is PropertyReference) {
				return UserFriendlyName((PropertyReference) memberRef);
			}
			if (memberRef is TypeReference) {
				return UserFriendlyName((TypeReference) memberRef, true);
			}
			if (memberRef is MethodReference) {
				return UserFriendlyName((MethodReference) memberRef);
			}
			return memberRef.FullName;
		}

		internal static IEnumerable<MethodDefinition> GetMethodsLike(this TypeDefinition type, MethodReference methodRef) {
			return type.GetMethods(methodRef.Name, methodRef.Parameters.Select(x => x.ParameterType), methodRef.ReturnType);
		}

		/// <summary>
		/// This method only considers the return type of the method if its name is op_Explicit or op_Implicit.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="methodName"></param>
		/// <param name="similarParams"></param>
		/// <param name="returnType"></param>
		/// <returns></returns>
		internal static IEnumerable<MethodDefinition> GetMethods(this TypeDefinition type, string methodName,
			IEnumerable<TypeReference> similarParams, TypeReference returnType) {
			var sameName =
				from method in type.Methods
				where method.Name == methodName
				select method;

			similarParams = similarParams.ToList();
			var ignoreReturnType = !methodName.EqualsAny("op_Implicit", "op_Explicit");
			var sameSig =
				from method in sameName
				where method.Parameters.Count == similarParams.Count()
				where similarParams == null
					|| method.Parameters.Select(x => x.ParameterType).Zip(similarParams, AreTypesEquivForOverloading).All(x => x)
				where ignoreReturnType || returnType == null || AreTypesEquivForOverloading(method.ReturnType, returnType)
				select method;



			return sameSig.ToList();
		}

		/// <summary>
		///     Determines if the types are equivalent for the purpose of choosing overloads.
		/// </summary>
		/// <param name="a">Type a.</param>
		/// <param name="b">Type b.</param>
		/// <returns></returns>
		private static bool AreTypesEquivForOverloading(TypeReference a, TypeReference b) {
			if (a.IsGenericParameter || b.IsGenericParameter) {
				//there are two kinds of generic parameters, Var and MVar (class/method).
				//they are different. otherwise, all gen params are the same.
				return a.MetadataType == b.MetadataType;
			}
			//first check is probably unnecessary because Name returns the type in a special way
			//that encodes things like ByRef.
			
			//this isn't really a comprehensive identity test. At some undetermined point in the future it will be fixed.
			//however, the problem cases are pretty rare.
			return a.Name == b.Name; // [a.MetadataType == b.MetadataType] this was deleted because apparently the types can match even though the MT is different.

		}

		private static string UserFriendlyName(this FieldReference fRef) {
			var declaringType = fRef.DeclaringType.UserFriendlyName();
			var fieldType = fRef.FieldType.UserFriendlyName();
			return string.Format("{0} {1}::{2}", fieldType, declaringType, fRef.Name);
		}

		public static bool IsVarOrMVar(this TypeReference typeRef) {
			return typeRef.MetadataType == MetadataType.MVar || typeRef.MetadataType == MetadataType.Var;
		}

		private static string UserFriendlyName(this PropertyReference fRef) {
			var declaringType = fRef.DeclaringType.UserFriendlyName();
			var fieldType = fRef.PropertyType.UserFriendlyName();
			return string.Format("{0} {1}::{2}", fieldType, declaringType, fRef.Name);
		}

		/// <summary>
		///     Returns a user-friendly name.
		/// </summary>
		/// <param name="typeRef">The type reference.</param>
		/// <param name="longForm">If true, returns a longer form of the name.</param>
		/// <returns></returns>
		private static string UserFriendlyName(this TypeReference typeRef, bool longForm = false) {
			string baseType;
			switch (typeRef.MetadataType) {
				case MetadataType.Array:
					var asArray = (ArrayType) typeRef;
					return UserFriendlyName(asArray.ElementType) + string.Format("[{0}]", ",".Replicate(asArray.Rank - 1));
				case MetadataType.GenericInstance:
					var asInst = (GenericInstanceType) typeRef;
					baseType = UserFriendlyName(asInst.ElementType);
					var genericArgs = "<" + asInst.GenericArguments.Select(UserFriendlyName).Join(", ") + ">";
					return baseType + genericArgs;
				case MetadataType.ByReference:
					var asRef = (ByReferenceType) typeRef;
					baseType = UserFriendlyName(asRef.ElementType);
					return "ref " + baseType;
				default:
					var name = !longForm ? typeRef.Name : typeRef.FullName;
					return name;
			}
		}

		private static string UserFriendlyName(this MethodReference mRef) {
			var returnType = mRef.ReturnType.UserFriendlyName();
			var paramTypes = mRef.Parameters.Select(x => x.ParameterType.UserFriendlyName()).Join(", ");
			var declaringType = mRef.DeclaringType.UserFriendlyName();
			var displayName = string.Format("{0} {1}::{2}", returnType, declaringType, mRef.Name);
			if (mRef.IsGenericInstance) {
				var asGeneric = (GenericInstanceMethod) mRef;
				var sig = asGeneric.GenericArguments.Select(UserFriendlyName).Join(", ");
				displayName = string.Format("{0}<{1}>", displayName, sig);
			}
			displayName = string.Format("{0}({1})", displayName, paramTypes);
			return displayName;
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

		private static object UnpackArgument(CustomAttributeArgument arg) {
			if (arg.Value is CustomAttributeArgument) {
				return UnpackArgument((CustomAttributeArgument) arg.Value);
			}
			else if (arg.Value is CustomAttributeArgument[]) {
				var asArray = (CustomAttributeArgument[]) arg.Value;
				var unpacked = asArray.Select(UnpackArgument).ToArray();
				return unpacked;
			}
			return arg.Value;
		}

		
		/// <summary>
		///     Constructs an attribute instance from its metadata.
		///     The method will fail if the attribute constructor has a System.Type parameter.
		/// </summary>
		/// <param name="customAttrData">The custom attribute data.</param>
		/// <returns></returns>
		private static object TryConstructAttribute(this CustomAttribute customAttrData) {
			var constructor = (ConstructorInfo) customAttrData.Constructor.Resolve().LoadMethod();
			var args = customAttrData.ConstructorArguments.Select(UnpackArgument).ToArray();
			if (constructor.GetParameters().Any(p => p.ParameterType == typeof (Type))) {
				//we cannot invoke this constructor.
				return null;
			}
			var ret = constructor.Invoke(args);
			return ret;
		}
	}
}