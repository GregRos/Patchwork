using System;
using System.IO;
using System.Windows.Forms;
using Patchwork.Engine.Utility;
using Patchwork.Utility;
using Patchwork.Utility.Binding;

namespace PatchworkLauncher {
	public partial class guiInputGameFolder : Form {
		private readonly FolderBrowserDialog _folderBrowser = new FolderBrowserDialog() {
			Description = "Pick game folder",
			ShowNewFolderButton = false,
			SelectedPath = PathHelper.GetAbsolutePath("")
		};
		public guiInputGameFolder(string warningText = "") {
			InitializeComponent();
			guiWarningText.Text = warningText;
		}

		private void label1_Click(object sender, EventArgs e) {

		}

		private void guiInputGameFolder_Load(object sender, EventArgs e) {
			Folder.SetRule("Folder must exist", Directory.Exists);
			Folder.IsValid.Binding = guiOkay.Bind(x => x.Enabled).ToBinding(BindingMode.FromTarget);
			Folder.Binding = guiLocationTextBox.Bind(x => x.Text, "TextChanged").ToBinding(BindingMode.TwoWay);
		}

		public IBindable<string> Folder {
			get;
		} = Bindable.Variable("");

		private void guiOkay_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.OK;
			Close();
		}

		private void guiCancel_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void button1_Click(object sender, EventArgs e) {
			var result = _folderBrowser.ShowDialog();
			if (result != DialogResult.OK) {
				return;
			}
			Folder.Value = _folderBrowser.SelectedPath;
		}
	}
}
