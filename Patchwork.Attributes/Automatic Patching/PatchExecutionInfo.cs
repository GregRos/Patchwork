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
		}

		/// <summary>
		/// The user-visible version of the mod. Defaults to the version of the implementing type's assembly.
		/// </summary>
		public virtual string Version {
			get {
				return _fileVersion;
			}
			set {
				_fileVersion = value;
			}
		}

		/// <summary>
		/// This method is called to determine if this mod can be applied to the target file in its present state.
		/// </summary>
		/// <param name="baseFolder">The base folder (usually, the game's main folder)</param>
		/// <param name="force">An integer that gives the degree of forcing the user wants. Used to override safety features and the like.</param>
		/// <param name="message">A text message which is displayed to the user. If null, no message is displayed.</param>
		/// <returns></returns>
		public virtual bool CanPatch(DirectoryInfo baseFolder, int force, out string message) {
			message = null;
			return true;
		}

		/// <summary>
		/// This method is called after patching is performed. It can contain post-conditions. If the result of patching is invalid, this method should throw an exception. This will undo the operation.
		/// </summary>
		public virtual void AfterPatch(DirectoryInfo baseFolder, int force) {
			
		}
	}
}
