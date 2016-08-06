using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Patchwork.Engine.Utility {
	/// <summary>
	/// Contains extension methods that are regarded as hacks circumventing some of Cecil's limitations. Strong points of failure should the version change.
	/// </summary>
	internal static class Hacks {
		private static IDictionary<BaseAssemblyResolver, IList<AssemblyResolveEventHandler>> _resolveDictionary =
			new Dictionary<BaseAssemblyResolver, IList<AssemblyResolveEventHandler>>();

		private static ConstructorInfo _instructionConstructorInfo;

		private static ConditionalWeakTable<ModuleDefinition, IDictionary<uint, IMetadataTokenProvider>> _tokenProviderCache =
			new ConditionalWeakTable<ModuleDefinition, IDictionary<uint, IMetadataTokenProvider>>();

		/// <summary>
		/// Creates a system of multiple assembly resolvers. The latest resolver is fired first. The list can be cleared.
		/// </summary>
		/// <param name="resolver"></param>
		/// <param name="handler"></param>
		internal static void RegisterSpecialResolveFailureHandler(this BaseAssemblyResolver resolver,
			AssemblyResolveEventHandler handler) {
			if (_resolveDictionary.ContainsKey(resolver)) {
				_resolveDictionary[resolver].Add(handler);
			} else {
				_resolveDictionary[resolver] = new List<AssemblyResolveEventHandler> {
					handler
				};
				resolver.ResolveFailure += (sender, name) => {
					foreach (var curHandler in _resolveDictionary[resolver].Reverse()) {
						var assembly = curHandler(sender, name);
						if (assembly != null) {
							return assembly;
						}
					}
					return null;
				};
			}
		}

		internal static void ClearExtraResolvers(this BaseAssemblyResolver resolver) {
			if (_resolveDictionary.ContainsKey(resolver)) {
				_resolveDictionary[resolver].Clear();
			}
		}

		internal static void ForceClearCache(this BaseAssemblyResolver resolver) {
			var asDefault = resolver as DefaultAssemblyResolver;
			if (asDefault == null) {
				throw new Exception($"Failed to clear the cache because the resolver type wasn't {nameof(DefaultAssemblyResolver)}.");
			}
			//sometimes old versions of assemblies get stuck in the cache. We want to kick them out, so we have to resort to this... 
			var field = typeof (DefaultAssemblyResolver).GetField("cache", CommonBindingFlags.Everything);
			if (field == null) {
				throw new Exception($"Failed to clear the cache because the resolver didn't have a field called 'cache'.");
			}
			try {
				var cache = (IDictionary<string, AssemblyDefinition>) field.GetValue(resolver);
				cache.Clear();
			}
			catch (Exception ex) {
				throw new Exception($"Failed to clear the cache because an exception was thrown.", ex);
			}
		}

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

		private static IDictionary<uint, IMetadataTokenProvider> InitializeTokenProviderCache(ModuleDefinition module) {
			var dict = new Dictionary<uint, IMetadataTokenProvider>();
			var allTypes = module.GetAllTypes();
			foreach (var type in allTypes) {
				foreach (var prop in type.Properties) {
					dict[prop.MetadataToken.ToUInt32()] = prop;
				}
				foreach (var vent in type.Events) {
					dict[vent.MetadataToken.ToUInt32()] = vent;
				}
			}
			foreach (var expType in module.ExportedTypes) {
				dict[expType.MetadataToken.ToUInt32()] = expType;
			}
			foreach (var modRef in module.ModuleReferences) {
				dict[modRef.MetadataToken.ToUInt32()] = modRef;
			}
			foreach (var assRef in module.AssemblyReferences) {
				dict[assRef.MetadataToken.ToUInt32()] = assRef;
			}
			return dict;
		}

		/// <summary>
		/// This method returns the member with the specified metadata token in the given module. It supports more TokenTypes than the standard Cecil method.
		/// </summary>
		/// <param name="module"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		internal static IMetadataTokenProvider LookupTokenExtended(this ModuleDefinition module, MetadataToken token) {
			switch (token.TokenType) {
				case TokenType.TypeDef:
				case TokenType.TypeRef:
				case TokenType.MemberRef:
				case TokenType.Field:
				case TokenType.Method:
				case TokenType.MethodSpec:
				case TokenType.TypeSpec:
					return module.LookupToken(token);
				default:
					IMetadataTokenProvider provider;
					var success = _tokenProviderCache.GetValue(module, InitializeTokenProviderCache).TryGetValue(token.ToUInt32(),
						out provider);
					return success ? provider : null;
			}
		}
	}
}