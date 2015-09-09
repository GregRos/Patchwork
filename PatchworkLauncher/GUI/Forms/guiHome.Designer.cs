namespace PatchworkLauncher
{
	partial class guiHome
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
			this.ctrlGameName = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.ctrlReplaceFiles = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button5 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ctrlGameName
			// 
			this.ctrlGameName.AutoSize = true;
			this.ctrlGameName.BackColor = System.Drawing.Color.Transparent;
			this.ctrlGameName.Font = new System.Drawing.Font("Century", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrlGameName.Location = new System.Drawing.Point(12, 9);
			this.ctrlGameName.Name = "ctrlGameName";
			this.ctrlGameName.Size = new System.Drawing.Size(331, 44);
			this.ctrlGameName.TabIndex = 0;
			this.ctrlGameName.Text = "Pillars of Eternity";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label1.Location = new System.Drawing.Point(516, 30);
			this.label1.Name = "label1";
			this.label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label1.Size = new System.Drawing.Size(97, 18);
			this.label1.TabIndex = 1;
			this.label1.Text = "2.0.07062";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ctrlReplaceFiles
			// 
			this.ctrlReplaceFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ctrlReplaceFiles.AutoSize = true;
			this.ctrlReplaceFiles.BackColor = System.Drawing.Color.Transparent;
			this.ctrlReplaceFiles.Font = new System.Drawing.Font("Georgia", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ctrlReplaceFiles.Location = new System.Drawing.Point(20, 179);
			this.ctrlReplaceFiles.Name = "ctrlReplaceFiles";
			this.ctrlReplaceFiles.Size = new System.Drawing.Size(231, 27);
			this.ctrlReplaceFiles.TabIndex = 2;
			this.ctrlReplaceFiles.Text = "Replace Files by Default";
			this.ctrlReplaceFiles.UseVisualStyleBackColor = false;
			this.ctrlReplaceFiles.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.BackColor = System.Drawing.SystemColors.Control;
			this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button1.Font = new System.Drawing.Font("Georgia", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button1.Location = new System.Drawing.Point(20, 212);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(593, 68);
			this.button1.TabIndex = 3;
			this.button1.Text = "Launch with Mods";
			this.button1.UseVisualStyleBackColor = false;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.BackColor = System.Drawing.SystemColors.Control;
			this.button2.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button2.Font = new System.Drawing.Font("Georgia", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button2.Location = new System.Drawing.Point(20, 286);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(593, 58);
			this.button2.TabIndex = 4;
			this.button2.Text = "Launch without Mods";
			this.button2.UseVisualStyleBackColor = false;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button3.BackColor = System.Drawing.SystemColors.Control;
			this.button3.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button3.Font = new System.Drawing.Font("Georgia", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button3.Location = new System.Drawing.Point(472, 350);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(141, 34);
			this.button3.TabIndex = 5;
			this.button3.Text = "Active Mods";
			this.button3.UseVisualStyleBackColor = false;
			// 
			// button4
			// 
			this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button4.BackColor = System.Drawing.SystemColors.Control;
			this.button4.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button4.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button4.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button4.Font = new System.Drawing.Font("Georgia", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button4.Location = new System.Drawing.Point(20, 350);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(148, 34);
			this.button4.TabIndex = 6;
			this.button4.Text = "Preferences";
			this.button4.UseVisualStyleBackColor = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label2.Location = new System.Drawing.Point(17, 62);
			this.label2.Name = "label2";
			this.label2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label2.Size = new System.Drawing.Size(70, 18);
			this.label2.TabIndex = 8;
			this.label2.Text = "Status:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox1.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox1.Location = new System.Drawing.Point(20, 83);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(593, 90);
			this.textBox1.TabIndex = 7;
			this.textBox1.Text = "Error! Your game files don\'t much the expected files.\r\n\r\nClick Fix! to correct th" +
    "e issue. Otherwise the program can\'t work properly.\r\n";
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// button5
			// 
			this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button5.BackColor = System.Drawing.Color.Red;
			this.button5.FlatAppearance.BorderColor = System.Drawing.Color.White;
			this.button5.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button5.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button5.Location = new System.Drawing.Point(519, 56);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(94, 26);
			this.button5.TabIndex = 9;
			this.button5.Text = "Fix!";
			this.button5.UseVisualStyleBackColor = false;
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// guiHome
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Linen;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(632, 396);
			this.Controls.Add(this.button5);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.ctrlReplaceFiles);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ctrlGameName);
			this.MaximumSize = new System.Drawing.Size(812, 634);
			this.Name = "guiHome";
			this.Text = "Patchwork Launcher";
			this.Load += new System.EventHandler(this.guiHome_Load);
			this.Resize += new System.EventHandler(this.guiHome_Resize);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label ctrlGameName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox ctrlReplaceFiles;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button5;
	}
}

