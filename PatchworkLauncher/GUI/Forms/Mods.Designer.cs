namespace PatchworkLauncher
{
	partial class guiMods
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
			this.InstructionsGridView = new System.Windows.Forms.DataGridView();
			this.button3 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.On = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Target = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Requirements = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.InstructionsGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// InstructionsGridView
			// 
			this.InstructionsGridView.AllowUserToAddRows = false;
			this.InstructionsGridView.AllowUserToDeleteRows = false;
			this.InstructionsGridView.AllowUserToResizeRows = false;
			this.InstructionsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InstructionsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.InstructionsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.On,
            this.Name,
            this.Target,
            this.Requirements});
			this.InstructionsGridView.Location = new System.Drawing.Point(12, 12);
			this.InstructionsGridView.Name = "InstructionsGridView";
			this.InstructionsGridView.Size = new System.Drawing.Size(482, 425);
			this.InstructionsGridView.TabIndex = 0;
			this.InstructionsGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.InstructionsGridView_CellContentClick);
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button3.BackColor = System.Drawing.Color.Transparent;
			this.button3.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button3.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button3.Location = new System.Drawing.Point(505, 12);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(111, 34);
			this.button3.TabIndex = 6;
			this.button3.Text = "Move Up";
			this.button3.UseVisualStyleBackColor = false;
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.BackColor = System.Drawing.Color.Transparent;
			this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button1.Location = new System.Drawing.Point(505, 52);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(111, 34);
			this.button1.TabIndex = 7;
			this.button1.Text = "Move Down";
			this.button1.UseVisualStyleBackColor = false;
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.BackColor = System.Drawing.Color.Transparent;
			this.button2.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button2.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button2.Location = new System.Drawing.Point(505, 157);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(111, 34);
			this.button2.TabIndex = 9;
			this.button2.Text = "Remove";
			this.button2.UseVisualStyleBackColor = false;
			// 
			// button4
			// 
			this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button4.BackColor = System.Drawing.Color.Transparent;
			this.button4.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button4.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button4.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button4.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button4.Location = new System.Drawing.Point(505, 117);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(111, 34);
			this.button4.TabIndex = 8;
			this.button4.Text = "Add";
			this.button4.UseVisualStyleBackColor = false;
			// 
			// button5
			// 
			this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button5.BackColor = System.Drawing.Color.Transparent;
			this.button5.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button5.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button5.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button5.Location = new System.Drawing.Point(505, 449);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(111, 34);
			this.button5.TabIndex = 10;
			this.button5.Text = "Save";
			this.button5.UseVisualStyleBackColor = false;
			// 
			// button6
			// 
			this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button6.BackColor = System.Drawing.Color.Transparent;
			this.button6.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.button6.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.button6.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button6.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button6.Location = new System.Drawing.Point(388, 449);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(111, 34);
			this.button6.TabIndex = 11;
			this.button6.Text = "Cancel";
			this.button6.UseVisualStyleBackColor = false;
			// 
			// On
			// 
			this.On.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.On.DataPropertyName = "IsEnabled";
			this.On.HeaderText = "On";
			this.On.Name = "On";
			this.On.Width = 27;
			// 
			// Name
			// 
			this.Name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.Name.DataPropertyName = "Name";
			this.Name.HeaderText = "Name";
			this.Name.Name = "Name";
			this.Name.ReadOnly = true;
			this.Name.Width = 60;
			// 
			// Target
			// 
			this.Target.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.Target.DataPropertyName = "Target";
			this.Target.HeaderText = "Target";
			this.Target.Name = "Target";
			this.Target.ReadOnly = true;
			this.Target.Width = 63;
			// 
			// Requirements
			// 
			this.Requirements.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.Requirements.DataPropertyName = "Requirements";
			this.Requirements.HeaderText = "Requirements";
			this.Requirements.Name = "Requirements";
			this.Requirements.ReadOnly = true;
			this.Requirements.Width = 97;
			// 
			// guiMods
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Linen;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(628, 489);
			this.Controls.Add(this.button6);
			this.Controls.Add(this.button5);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.InstructionsGridView);
			this.MaximumSize = new System.Drawing.Size(812, 634);
			base.Name = "guiHome";
			this.Text = "Patchwork Launcher";
			this.Load += new System.EventHandler(this.guiMods_Load);
			((System.ComponentModel.ISupportInitialize)(this.InstructionsGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView InstructionsGridView;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.DataGridViewCheckBoxColumn On;
		private System.Windows.Forms.DataGridViewTextBoxColumn Name;
		private System.Windows.Forms.DataGridViewTextBoxColumn Target;
		private System.Windows.Forms.DataGridViewTextBoxColumn Requirements;
	}
}

