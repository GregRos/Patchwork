using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Patchwork;

namespace PatchworkLauncher {
	internal class PatchInstruction : INotifyPropertyChanged{
		private bool _isEnabled;

		public PatchingManifest Patch {
			get;
		}

		public bool IsEnabled {
			get {
				return _isEnabled;
			}
			set {
				_isEnabled = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	internal class PatchInstructionSequence {
		public ObservableCollection<PatchInstruction> Instructions {
			get;
		} = new ObservableCollection<PatchInstruction>();

	}
}
