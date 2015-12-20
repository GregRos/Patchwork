namespace PatchworkLauncher
{
	partial class guiInputGameFolder
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
			this.guiLocationTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.guiOkay = new System.Windows.Forms.Button();
			this.guiCancel = new System.Windows.Forms.Button();
			this.guiWarningText = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// guiLocationTextBox
			// 
			this.guiLocationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.guiLocationTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.guiLocationTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
			this.guiLocationTextBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiLocationTextBox.Location = new System.Drawing.Point(15, 38);
			this.guiLocationTextBox.Name = "guiLocationTextBox";
			this.guiLocationTextBox.Size = new System.Drawing.Size(487, 26);
			this.guiLocationTextBox.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Century", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(218, 18);
			this.label1.TabIndex = 1;
			this.label1.Text = "Specify the game\'s base folder";
			this.label1.Click += new System.EventHandler(this.label1_Click);
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Font = new System.Drawing.Font("Century", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button1.Location = new System.Drawing.Point(510, 34);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 34);
			this.button1.TabIndex = 2;
			this.button1.Text = "Browse";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// guiOkay
			// 
			this.guiOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.guiOkay.Font = new System.Drawing.Font("Century", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiOkay.Location = new System.Drawing.Point(510, 83);
			this.guiOkay.Name = "guiOkay";
			this.guiOkay.Size = new System.Drawing.Size(75, 28);
			this.guiOkay.TabIndex = 3;
			this.guiOkay.Text = "Okay";
			this.guiOkay.UseVisualStyleBackColor = true;
			this.guiOkay.Click += new System.EventHandler(this.guiOkay_Click);
			// 
			// guiCancel
			// 
			this.guiCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.guiCancel.Font = new System.Drawing.Font("Century", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiCancel.Location = new System.Drawing.Point(432, 83);
			this.guiCancel.Name = "guiCancel";
			this.guiCancel.Size = new System.Drawing.Size(72, 28);
			this.guiCancel.TabIndex = 4;
			this.guiCancel.Text = "Cancel";
			this.guiCancel.UseVisualStyleBackColor = true;
			this.guiCancel.Click += new System.EventHandler(this.guiCancel_Click);
			// 
			// guiWarningText
			// 
			this.guiWarningText.AutoSize = true;
			this.guiWarningText.Font = new System.Drawing.Font("Century", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiWarningText.ForeColor = System.Drawing.Color.Red;
			this.guiWarningText.Location = new System.Drawing.Point(12, 67);
			this.guiWarningText.Name = "guiWarningText";
			this.guiWarningText.Size = new System.Drawing.Size(105, 18);
			this.guiWarningText.TabIndex = 5;
			this.guiWarningText.Text = "Warning Text";
			// 
			// guiInputGameFolder
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Linen;
			this.ClientSize = new System.Drawing.Size(597, 123);
			this.ControlBox = false;
			this.Controls.Add(this.guiWarningText);
			this.Controls.Add(this.guiCancel);
			this.Controls.Add(this.guiOkay);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.guiLocationTextBox);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "guiInputGameFolder";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Specify Game Folder";
			this.Load += new System.EventHandler(this.guiInputGameFolder_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox guiLocationTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button guiOkay;
		private System.Windows.Forms.Button guiCancel;
		private System.Windows.Forms.Label guiWarningText;
	}
}