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

	public partial class guiHome : Form {
		public guiHome() {
			InitializeComponent();
		}



		protected override void OnPaintBackground(PaintEventArgs e) {
			if (ClientRectangle.Height == 0 || ClientRectangle.Width == 0) {
				return;
			}
			var myLightBlue = Color.FromArgb(255, 197, 202, 255);
			using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle,
				Color.White,
				Color.Azure,
				90F)) {
				e.Graphics.FillRectangle(brush, ClientRectangle);
			}
		}

		private const int InvalidateInterval = 100;
		private long LastInvalidated = -1;
		private readonly guiMods _mods = new guiMods();

		private void guiHome_Resize(object sender, EventArgs e) {
			var ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			if (ms - LastInvalidated > InvalidateInterval) {
				Invalidate();
				LastInvalidated = ms;
			}

		}

		

		private void guiActiveMods_Click(object sender, EventArgs e) {
		
		}

		private void guiHome_Load(object sender, EventArgs e) {

		}
	}
}
