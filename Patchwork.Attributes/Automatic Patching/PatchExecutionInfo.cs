using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Patchwork.Attributes {
	/// <summary>
	/// Inherit from this class to implement auto-patching behavior.
	/// </summary>
	public abstract class PatchExecutionInfo {
		private string _fileVersion;

		protected PatchExecutionInfo(string name = null, string overrideVersion = null) {
			_fileVersion = overrideVersion ?? FileVersionInfo.GetVersionInfo(GetType().Assembly.Location).FileVersion;
			Name = name;
		}

		public abstract FileInfo TargetFile(DirectoryInfo baseFolder);

		/// <summary>
		/// The user-visible name of the mod.
		/// </summary>
		public virtual string Name {
			get;
			protected set;
		}

		public virtual string Requirements {
			get;
			protected set;
		}

		/// <summary>
		/// The user-visible version of the mod. Defaults to the version of the implementing type's assembly.
		/// </summary>
		public virtual string Version {
			get {
				return _fileVersion;
			}
			protected set {
				_fileVersion = value;
			}
		}

		public delegate Assembly LazyAssemblyResolver();

		/// <summary>
		/// This method is called to determine if this mod can be applied to the target file in its present state.
		/// </summary>
		/// <param name="resolver">Returns the target assembly, loaded in a reflection-only context.</param>
		/// <param name="message">A text message which is displayed to the user. If null, no message is displayed.</param>
		/// <returns></returns>
		public virtual bool CanPatch(LazyAssemblyResolver resolver, out string message) {
			message = null;
			return true;
		}

		/// <summary>
		/// This method is called after patching to check if the result is correct.
		/// </summary>
		/// <param name="resolver">Returns the target assembly, in a reflection-only context.</param>
		public virtual void AfterPatch(LazyAssemblyResolver resolver) {
			
		}

	}
}
