using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PatchworkLauncher {
	internal class Settings : INotifyPropertyChanged {
		private string _modsFolder;
		private bool _replaceFiles;

		public string ModsFolder {
			get {
				return _modsFolder;
			}
			set {
				_modsFolder = value;
				OnPropertyChanged();
			}
		}

		public bool ReplaceFiles {
			get {
				return _replaceFiles;
			}
			set {
				_replaceFiles = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
