using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Patchwork.Utility;
using PatchworkLauncher.Properties;

namespace PatchworkLauncher {
	public partial class guiMods : Form {

		public guiMods(LaunchManager manager) {
			Manager = manager;
			InitializeComponent();
		}

		public LaunchManager Manager {
			get;
			private set;
		}

		private void guiMods_Load(object sender, EventArgs e) {
			Icon = Icon.FromHandle(Resources.IconSmall.GetHicon());
			guiInstructionsGridView.AutoGenerateColumns = false;
			guiInstructionsGridView.DataSource = Manager.Instructions;
		}


		private void InstructionsGridView_CellContentClick(object sender, DataGridViewCellEventArgs e) {

		}

		private void guiAdd_Click(object sender, EventArgs e) {
			Manager.Command_Dialog_AddPatch(this);
		}

		private void guiRemove_Click(object sender, EventArgs e) {
			var selected = guiInstructionsGridView?.CurrentRow?.DataBoundItem;
			if (selected == null) {
				return;
			}
			Manager.Instructions.Remove((PatchInstruction) selected);
		}

		private void guiClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void guiMoveUp_Click(object sender, EventArgs e) {
			var current = guiInstructionsGridView?.CurrentRow?.Index;
			if (!current.HasValue) {
				return;
			}
			var newIndex = Manager.Command_MovePatch(current.Value, -1);
			guiInstructionsGridView.CurrentCell = guiInstructionsGridView.Rows[newIndex].Cells[0];
			guiInstructionsGridView.Refresh();
		}

		private void guiMoveDown_Click(object sender, EventArgs e) {
			var current = guiInstructionsGridView?.CurrentRow?.Index;
			if (!current.HasValue) {
				return;
			}
			var newIndex = Manager.Command_MovePatch(current.Value, 1);
			guiInstructionsGridView.CurrentCell = guiInstructionsGridView.Rows[newIndex].Cells[0];
			guiInstructionsGridView.Refresh();
		}

	}
}
