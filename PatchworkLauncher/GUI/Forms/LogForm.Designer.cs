namespace PatchworkLauncher
{
	partial class LogForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.button3 = new System.Windows.Forms.Button();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// richTextBox1
			// 
			this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBox1.BackColor = System.Drawing.Color.White;
			this.richTextBox1.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.richTextBox1.ForeColor = System.Drawing.SystemColors.MenuText;
			this.richTextBox1.Location = new System.Drawing.Point(12, 97);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(533, 136);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "This is!\nOne to wo";
			this.richTextBox1.WordWrap = false;
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button3.BackColor = System.Drawing.SystemColors.Control;
			this.button3.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button3.Font = new System.Drawing.Font("Georgia", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button3.Location = new System.Drawing.Point(404, 239);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(141, 34);
			this.button3.TabIndex = 6;
			this.button3.Text = "Close";
			this.button3.UseVisualStyleBackColor = false;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(12, 12);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(533, 48);
			this.progressBar1.TabIndex = 9;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Font = new System.Drawing.Font("Georgia", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(6, 63);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 31);
			this.label2.TabIndex = 10;
			this.label2.Text = "Errors:";
			// 
			// LogForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(557, 285);
			this.ControlBox = false;
			this.Controls.Add(this.label2);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.richTextBox1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LogForm";
			this.Text = "Working...";
			this.Load += new System.EventHandler(this.LogForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label label2;
	}
}