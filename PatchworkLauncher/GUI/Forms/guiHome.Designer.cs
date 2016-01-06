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
			this.guiGameName = new System.Windows.Forms.Label();
			this.guiGameVersion = new System.Windows.Forms.Label();
			this.guiLaunchWithMods = new System.Windows.Forms.Button();
			this.guiLaunchNoMods = new System.Windows.Forms.Button();
			this.guiActiveMods = new System.Windows.Forms.Button();
			this.guiPwVersion = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.guiGameIcon = new System.Windows.Forms.PictureBox();
			this.guiChangeFolder = new System.Windows.Forms.Button();
			this.guiTestRun = new System.Windows.Forms.Button();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.button1 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.guiGameIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// guiGameName
			// 
			this.guiGameName.AutoSize = true;
			this.guiGameName.BackColor = System.Drawing.Color.Transparent;
			this.guiGameName.Font = new System.Drawing.Font("Georgia", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiGameName.Location = new System.Drawing.Point(64, 18);
			this.guiGameName.Name = "guiGameName";
			this.guiGameName.Size = new System.Drawing.Size(309, 43);
			this.guiGameName.TabIndex = 0;
			this.guiGameName.Text = "Pillars of Eternity";
			this.guiGameName.Click += new System.EventHandler(this.guiGameName_Click);
			// 
			// guiGameVersion
			// 
			this.guiGameVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.guiGameVersion.AutoSize = true;
			this.guiGameVersion.BackColor = System.Drawing.Color.Transparent;
			this.guiGameVersion.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiGameVersion.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.guiGameVersion.Location = new System.Drawing.Point(532, 43);
			this.guiGameVersion.Name = "guiGameVersion";
			this.guiGameVersion.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.guiGameVersion.Size = new System.Drawing.Size(70, 18);
			this.guiGameVersion.TabIndex = 1;
			this.guiGameVersion.Text = "2.0.0.0";
			this.guiGameVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// guiLaunchWithMods
			// 
			this.guiLaunchWithMods.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.guiLaunchWithMods.BackColor = System.Drawing.SystemColors.Control;
			this.guiLaunchWithMods.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.guiLaunchWithMods.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.guiLaunchWithMods.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.guiLaunchWithMods.Font = new System.Drawing.Font("Georgia", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiLaunchWithMods.Location = new System.Drawing.Point(21, 65);
			this.guiLaunchWithMods.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.guiLaunchWithMods.Name = "guiLaunchWithMods";
			this.guiLaunchWithMods.Size = new System.Drawing.Size(537, 74);
			this.guiLaunchWithMods.TabIndex = 3;
			this.guiLaunchWithMods.Text = "Launch with Mods";
			this.guiLaunchWithMods.UseVisualStyleBackColor = false;
			this.guiLaunchWithMods.Click += new System.EventHandler(this.guiLaunchWithMods_Click);
			// 
			// guiLaunchNoMods
			// 
			this.guiLaunchNoMods.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.guiLaunchNoMods.BackColor = System.Drawing.SystemColors.Control;
			this.guiLaunchNoMods.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.guiLaunchNoMods.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.guiLaunchNoMods.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.guiLaunchNoMods.Font = new System.Drawing.Font("Georgia", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiLaunchNoMods.Location = new System.Drawing.Point(21, 143);
			this.guiLaunchNoMods.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.guiLaunchNoMods.Name = "guiLaunchNoMods";
			this.guiLaunchNoMods.Size = new System.Drawing.Size(598, 63);
			this.guiLaunchNoMods.TabIndex = 4;
			this.guiLaunchNoMods.Text = "Launch without Mods";
			this.guiLaunchNoMods.UseVisualStyleBackColor = false;
			this.guiLaunchNoMods.Click += new System.EventHandler(this.guiLaunchNoMods_Click);
			// 
			// guiActiveMods
			// 
			this.guiActiveMods.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.guiActiveMods.BackColor = System.Drawing.SystemColors.Control;
			this.guiActiveMods.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.guiActiveMods.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.guiActiveMods.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.guiActiveMods.Font = new System.Drawing.Font("Georgia", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiActiveMods.Location = new System.Drawing.Point(478, 214);
			this.guiActiveMods.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.guiActiveMods.Name = "guiActiveMods";
			this.guiActiveMods.Size = new System.Drawing.Size(141, 36);
			this.guiActiveMods.TabIndex = 5;
			this.guiActiveMods.Text = "Active Mods";
			this.guiActiveMods.UseVisualStyleBackColor = false;
			this.guiActiveMods.Click += new System.EventHandler(this.guiActiveMods_Click);
			// 
			// guiPwVersion
			// 
			this.guiPwVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.guiPwVersion.AutoSize = true;
			this.guiPwVersion.BackColor = System.Drawing.Color.Transparent;
			this.guiPwVersion.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiPwVersion.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.guiPwVersion.Location = new System.Drawing.Point(201, 256);
			this.guiPwVersion.Name = "guiPwVersion";
			this.guiPwVersion.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.guiPwVersion.Size = new System.Drawing.Size(35, 18);
			this.guiPwVersion.TabIndex = 6;
			this.guiPwVersion.Text = "???";
			this.guiPwVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label2.Location = new System.Drawing.Point(118, 255);
			this.label2.Name = "label2";
			this.label2.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label2.Size = new System.Drawing.Size(80, 18);
			this.label2.TabIndex = 7;
			this.label2.Text = "Version:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label2.Click += new System.EventHandler(this.label2_Click);
			// 
			// guiGameIcon
			// 
			this.guiGameIcon.InitialImage = null;
			this.guiGameIcon.Location = new System.Drawing.Point(21, 25);
			this.guiGameIcon.Name = "guiGameIcon";
			this.guiGameIcon.Size = new System.Drawing.Size(37, 33);
			this.guiGameIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.guiGameIcon.TabIndex = 8;
			this.guiGameIcon.TabStop = false;
			// 
			// guiChangeFolder
			// 
			this.guiChangeFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.guiChangeFolder.BackColor = System.Drawing.SystemColors.Control;
			this.guiChangeFolder.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.guiChangeFolder.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.guiChangeFolder.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.guiChangeFolder.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiChangeFolder.Location = new System.Drawing.Point(21, 214);
			this.guiChangeFolder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.guiChangeFolder.Name = "guiChangeFolder";
			this.guiChangeFolder.Size = new System.Drawing.Size(160, 36);
			this.guiChangeFolder.TabIndex = 9;
			this.guiChangeFolder.Text = "Change Game Folder";
			this.guiChangeFolder.UseVisualStyleBackColor = false;
			this.guiChangeFolder.Click += new System.EventHandler(this.guiChangeFolder_Click);
			// 
			// guiTestRun
			// 
			this.guiTestRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.guiTestRun.BackColor = System.Drawing.SystemColors.Control;
			this.guiTestRun.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.guiTestRun.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.guiTestRun.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.guiTestRun.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiTestRun.Location = new System.Drawing.Point(564, 65);
			this.guiTestRun.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.guiTestRun.Name = "guiTestRun";
			this.guiTestRun.Size = new System.Drawing.Size(55, 74);
			this.guiTestRun.TabIndex = 10;
			this.guiTestRun.Text = "Test Run";
			this.guiTestRun.UseVisualStyleBackColor = false;
			this.guiTestRun.Click += new System.EventHandler(this.guiTestRun_Click);
			// 
			// linkLabel1
			// 
			this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold);
			this.linkLabel1.Location = new System.Drawing.Point(18, 254);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(103, 18);
			this.linkLabel1.TabIndex = 11;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Patchwork";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.BackColor = System.Drawing.SystemColors.Control;
			this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button1.Font = new System.Drawing.Font("Georgia", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button1.Location = new System.Drawing.Point(612, 3);
			this.button1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(23, 35);
			this.button1.TabIndex = 12;
			this.button1.Text = "?";
			this.button1.UseVisualStyleBackColor = false;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// guiHome
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Linen;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(639, 281);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.guiTestRun);
			this.Controls.Add(this.guiChangeFolder);
			this.Controls.Add(this.guiGameIcon);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.guiPwVersion);
			this.Controls.Add(this.guiActiveMods);
			this.Controls.Add(this.guiLaunchNoMods);
			this.Controls.Add(this.guiLaunchWithMods);
			this.Controls.Add(this.guiGameVersion);
			this.Controls.Add(this.guiGameName);
			this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(811, 679);
			this.Name = "guiHome";
			this.Text = "Patchwork Launcher";
			this.Load += new System.EventHandler(this.guiHome_Load);
			((System.ComponentModel.ISupportInitialize)(this.guiGameIcon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label guiGameName;
		private System.Windows.Forms.Label guiGameVersion;
		private System.Windows.Forms.Button guiLaunchWithMods;
		private System.Windows.Forms.Button guiLaunchNoMods;
		private System.Windows.Forms.Button guiActiveMods;
		private System.Windows.Forms.Label guiPwVersion;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PictureBox guiGameIcon;
		private System.Windows.Forms.Button guiChangeFolder;
		private System.Windows.Forms.Button guiTestRun;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Button button1;
	}
}

