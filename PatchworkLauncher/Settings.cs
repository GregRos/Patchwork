using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PatchworkLauncher {

	internal class ModEntry {

		public bool Enabled {
			get;
			set;
		}

		public string Path {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public string Target {
			get;
			set;
		}
	}

	internal class ModExecutionHistory {
		public DateTime ExecutionDate {
			get;
			set;
		}

		public ObservableCollection<ModEntry> Entries {
			get;
		} = new ObservableCollection<ModEntry>();
	}

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

		public ModExecutionHistory LastExecution {
			get;
			set;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
