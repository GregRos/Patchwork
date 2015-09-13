using System;
using System.Collections;
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
using Patchwork.Collections;

namespace PatchworkLauncher {
	internal class LauncherState : INotifyPropertyChanged {
		public Dictionary<string, PatchingManifest> Mods {
			get;
		}

		public Dictionary<string,AssemblyDefinition> Targets {
			get;
		}

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
