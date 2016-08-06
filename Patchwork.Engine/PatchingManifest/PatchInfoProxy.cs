using System;
using System.IO;
using System.Reflection;
using Patchwork.AutoPatching;
using Patchwork.Engine.Utility;

namespace Patchwork.Engine {
	internal class PatchInfoProxy : MarshalByRefObject, IPatchInfo, IDisposable {
		public bool IsDisposed {
			get;
			private set;
		}

		public IPatchInfo Info {
			get;
			private set;
		}

		public AppDomain Domain {
			get;
			private set;
		}

		public string AssemblyLocation {
			get;
			private set;
		}

		public string AssemblyName {
			get;
			private set;
		}


		public string FullTypeName {
			get;
			private set;
		}


		private PatchInfoProxy() {
			
		}

		/// <summary>
		/// Loads the PatchInfo type of the specified name from the specified assembly, but in a separate AppDomain to facilitate unloading. Returns a special proxy object.
		/// </summary>
		/// <param name="assemblyLocation">The full path to the assembly in question. Required for locating the assembly.</param>
		/// <param name="assemblyName">The expected short name of the assembly. This parameter is mainly optional and is only used for diagonostics.</param>
		/// <param name="fullTypeName">The full name of the type to be instantiated.</param>
		/// <returns></returns>
		internal static PatchInfoProxy FromPatchAssembly(string assemblyLocation, string assemblyName, string fullTypeName) {
			var domainSetup = new AppDomainSetup() {
				ApplicationBase = Environment.CurrentDirectory
			};
			var appDomain = AppDomain.CreateDomain(assemblyName, AppDomain.CurrentDomain.Evidence, domainSetup);
			var myType = typeof (PatchInfoProxy);
			var theActualProxy = (PatchInfoProxy)appDomain.CreateInstanceAndUnwrap(myType.Assembly.FullName, myType.FullName, false,
				CommonBindingFlags.Everything, null, null, null, null);
			theActualProxy.SetInnerInfo(assemblyLocation, fullTypeName);
			
			var proxyProxy = new PatchInfoProxy() {
				Domain = appDomain,
				Info = theActualProxy,
				AssemblyName = assemblyName,
				AssemblyLocation = assemblyLocation,
				FullTypeName = fullTypeName
			};
			return proxyProxy;
		}

		/// <summary>
		/// This method contains code that is meant to execute in a separate AppDomain. Any assemblies and types loaded here are constrained to that domain.
		/// </summary>
		/// <param name="assemblyPath"></param>
		/// <param name="fullTypeName"></param>
		/// <exception cref="PatchDeclerationException">The PatchInfo class was not declared correctly.</exception>
		/// <exception cref="PatchExecutionException">Code within the PatchInfo class threw an exception or otherwise failed to execute.</exception>
		private void SetInnerInfo(string assemblyPath, string fullTypeName) {
			//we do all the checks here because the type is only loaded in this AppDomain. It would be awkward to do these checks outside.
			//important to note that all exception classes must be Serializable.
			Type type;
			try {
				Assembly assembly = Assembly.LoadFile(assemblyPath);
				type = assembly.GetType(fullTypeName);
			}
			catch (Exception ex) {
				throw new PatchDeclerationException("The PatchInfo type could not be loaded from the patch assembly. This is usually because the patch was written against an incompatible version of Pathwork.Attributes.", ex);
			}
			
			if (!typeof (IPatchInfo).IsAssignableFrom(type)) {
				throw Errors.PatchInfo_doesnt_implement_interface(type);
			}
			var ctor = type.GetConstructorEx(CommonBindingFlags.Everything, new Type[] {});
			if (ctor == null) {
				Errors.PatchInfo_doesnt_have_default_ctor(type);
			}
			IPatchInfo info;
			try {
				info = (IPatchInfo) ctor.Invoke(null);
			}
			catch (Exception ex) {
				throw new PatchExecutionException(
					$"Executing the constructor of the PatchInfo class threw an exception.", ex);
			}
			Info = info;
		}

		public override object InitializeLifetimeService() {
			//required for the proxy object to have unlimited lifetime -- otherwise it would be collected.
			return null;
		}

		public FileInfo GetTargetFile(AppInfo app) {
			CheckDisposed();
			return Info.GetTargetFile(app);
		}

		public string CanPatch(AppInfo app) {
			CheckDisposed();
			return Info.CanPatch(app);
		}

		public string PatchVersion {
			get {
				CheckDisposed();
				return Info.PatchVersion;
			}
		}

		public string Requirements {
			get {
				CheckDisposed();
				return Info.Requirements;
			}
		}

		public string PatchName {
			get {
				CheckDisposed();
				return Info.PatchName;
			}
		}

		private void CheckDisposed() {
			if (IsDisposed) {
				throw new ObjectDisposedException(FullTypeName, $"The AppDomain {AssemblyName} hosting the assembly of the same name has been unloaded.");
			}
		}
		/// <summary>
		/// Unloads the AppDomain hosting the associated assembly.
		/// </summary>
		public void Dispose() {
			IsDisposed = true;
			AppDomain.Unload(Domain);
			Info = null;
			Domain = null;
		}
	}
}