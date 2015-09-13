using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatchworkLauncher {
	public partial class guiMods : Form {
		public guiMods() {
			InitializeComponent();
		}

		public PatchInstructionSequence Instructions {
			get;
			set;
		}


		private void checkBox1_CheckedChanged(object sender, EventArgs e) {

		}

		private void button2_Click(object sender, EventArgs e) {

		}

		private void guiMods_Load(object sender, EventArgs e) {
			InstructionsGridView.DataSource = Instructions.Instructions;
		}

		protected override void OnPaintBackground(PaintEventArgs e) {
			var myLightBlue = Color.FromArgb(255, 197, 202, 255);
			using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle,
				Color.White,
				myLightBlue.Lighten(15),
				90F)) {
				e.Graphics.FillRectangle(brush, this.ClientRectangle);
			}
		}

		private void InstructionsGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{

		}
	}
}
