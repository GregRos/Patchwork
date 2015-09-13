using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Patchwork;
using Patchwork.Attributes;
using Patchwork.Utility;
using Serilog;

namespace PatchworkLauncher {
	internal class PatchInstruction : INotifyPropertyChanged{
		private bool _isEnabled;

		public PatchingManifest Patch {
			get;
			set;
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

		public void Execute(DirectoryInfo baseFolder, ILogger log) {
			var patches =
				from instr in Instructions
				where instr.IsEnabled
				let target = instr.Patch.PatchExecution.TargetFile(baseFolder)
				group instr by target.FullName into instrs
				let targetPath = instrs.Key
				let patcher = new AssemblyPatcher(targetPath, log)
				from instr in instrs
				select new {
					targetPath,
					patcher,
					instr
				};

			foreach (var patch in patches) {
				patch.patcher.PatchManifest(patch.instr.Patch);
			}
		}

	}
}
