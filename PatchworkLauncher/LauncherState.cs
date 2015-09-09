using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Patchwork;

namespace PatchworkLauncher {

	internal class LauncherState : INotifyPropertyChanged {
		public IDictionary<string, PatchingManifest> Mods {
			get;
		} = new Dictionary<string, PatchingManifest>();

		public IDictionary<string,AssemblyDefinition> Targets {
			get;
		}  = new Dictionary<string, AssemblyDefinition>();

		public Settings SavedSettings {
			get;
			set;
		}

		

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
