using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PatchworkLauncher {
	internal class LauncherState : INotifyPropertyChanged {
		private List<string> _knownMods;
		public IEnumerable<string> KnownMods {
			get {
				return _knownMods;
			}
			set {
				_knownMods = value?.ToList();
				OnPropertyChanged();
			}
		}

		public Settings Options {
			get;
			set;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
