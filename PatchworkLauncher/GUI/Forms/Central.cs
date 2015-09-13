using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Mono.Cecil;
using Patchwork;
using Patchwork.Attributes;
using Patchwork.Utility;
using Serilog;

namespace PatchworkLauncher {
	internal class Central {
		public PatchInstructionSequence Sequence {
			get;
			private set;
		} = new PatchInstructionSequence();

		public ILogger Logger {
			get;
			private set;
		}


		public bool ReplaceFiles {
			get;
			set;
		}

		private ManifestCreator ManifestMaker {
			get;
			set;
		}

		private PatchInstruction InstructionFromXml(XmlInstruction instr) {
			var manifest = ManifestMaker.CreateManifest(AssemblyDefinition.ReadAssembly(instr.PatchLocation));
			return new PatchInstruction() {
				Patch = manifest,
				Path = instr.PatchLocation,
				IsEnabled = instr.IsEnabled
			};
		}

		static readonly XmlSerializer _serializer = new XmlSerializer(typeof(XmlSettings));
		static readonly Random _rnd = new Random();

		private void RestoreBackup(string current, string backup) {
			File.Move(backup, current);
		}

		private List<string> StartFileCheck(XmlFileHistory[] history) {
			var cannotBeFixed = new List<string>();
			foreach (var fileMods in history) {
				var assembly = AssemblyDefinition.ReadAssembly(fileMods.TargetPath);
				var actualExecutionSeq =
					from attr in assembly.GetCustomAttributes<PatchedByAssemblyAttribute>()
					orderby attr.Index
					select attr.PatchAssembly;

				var actualExecutionList = actualExecutionSeq.ToList();
				if (actualExecutionList.Count > 0) {
					Logger.Error("The file {@filePath:l} was supposed to be unpatched, but it was patched.", fileMods.TargetPath);
					if (!File.Exists(fileMods.BackupPath)) {
						Log_expected_backup(fileMods.TargetPath, fileMods.BackupPath);
						cannotBeFixed.Add(fileMods.TargetPath);
					} else {
						RestoreBackup(fileMods.TargetPath, fileMods.BackupPath);
					}
				}

			}
			return cannotBeFixed;
		}

		private void Log_expected_backup(string targetPath, string backupPath) {

			Logger.Error("The file {@filePath:l} was was supposed to have a backup {@filePath:l}, but the backup didn't exist.",targetPath, backupPath);
		}

		public void LoadSettings() {
			var settings = _serializer.Deserialize("settings.xml", XmlSettings.Default);
			Sequence = InstructionsFromXml(settings.Instructions);
			ReplaceFiles = settings.ReplaceFiles;
			StartFileCheck(settings.LastExecution);
		}

		private PatchInstructionSequence InstructionsFromXml(XmlInstruction[] xmlInstructions) {
			var instructions = xmlInstructions.Select(InstructionFromXml);
			var seq = new PatchInstructionSequence();
			seq.Instructions.AddRange(instructions);
			return seq;
		}
	}
}
