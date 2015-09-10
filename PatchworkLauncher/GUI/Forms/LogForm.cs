using System;
using System.Drawing;
using System.Windows.Forms;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace PatchworkLauncher {
	public partial class LogForm : Form {

		private class Styling {
			public Color? TextColor;
			public Color? BackColor;

			public void Apply(RichTextBox textBox) {
				textBox.SelectionColor = TextColor ?? textBox.ForeColor;
				textBox.SelectionBackColor = BackColor ?? textBox.BackColor;
			}
		}

		private class RichTextBoxSink : ILogEventSink {

			private RichTextBox _textBox;

			public RichTextBoxSink(RichTextBox textBox) {
				_textBox = textBox;
			}

			public void Emit(LogEvent logEvent) {
				_textBox.Text += logEvent.RenderMessage() + "\r\n";
			}
		}

		public LogForm() {
			InitializeComponent();
		}

		private void button3_Click(object sender, EventArgs e) {

		}

		private void LogForm_Load(object sender, EventArgs e) {

		}

		ILogger Logger {
			get;
			set;
		}

		public void Test() {
			
		}

		
	}
}
