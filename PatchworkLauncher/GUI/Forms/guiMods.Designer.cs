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
			this.guiInstructionsGridView = new System.Windows.Forms.DataGridView();
			this.guiOn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.guiName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.guiTarget = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.guiRequirements = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.guiPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.guiMoveUp = new System.Windows.Forms.Button();
			this.guiMoveDown = new System.Windows.Forms.Button();
			this.guiRemove = new System.Windows.Forms.Button();
			this.guiAdd = new System.Windows.Forms.Button();
			this.guiClose = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.guiInstructionsGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// guiInstructionsGridView
			// 
			this.guiInstructionsGridView.AllowUserToAddRows = false;
			this.guiInstructionsGridView.AllowUserToDeleteRows = false;
			this.guiInstructionsGridView.AllowUserToOrderColumns = true;
			this.guiInstructionsGridView.AllowUserToResizeRows = false;
			this.guiInstructionsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.guiInstructionsGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
			this.guiInstructionsGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
			this.guiInstructionsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.guiInstructionsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.guiOn,
            this.guiName,
            this.guiTarget,
            this.guiRequirements,
            this.guiPath});
			this.guiInstructionsGridView.Location = new System.Drawing.Point(12, 12);
			this.guiInstructionsGridView.MultiSelect = false;
			this.guiInstructionsGridView.Name = "guiInstructionsGridView";
			this.guiInstructionsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.guiInstructionsGridView.Size = new System.Drawing.Size(500, 445);
			this.guiInstructionsGridView.TabIndex = 0;
			this.guiInstructionsGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.InstructionsGridView_CellContentClick);
			// 
			// guiOn
			// 
			this.guiOn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.guiOn.DataPropertyName = "IsEnabled";
			this.guiOn.HeaderText = "On";
			this.guiOn.Name = "guiOn";
			this.guiOn.Width = 27;
			// 
			// guiName
			// 
			this.guiName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.guiName.DataPropertyName = "Name";
			this.guiName.HeaderText = "Name";
			this.guiName.Name = "guiName";
			this.guiName.ReadOnly = true;
			this.guiName.Width = 60;
			// 
			// guiTarget
			// 
			this.guiTarget.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.guiTarget.DataPropertyName = "Target";
			this.guiTarget.HeaderText = "Target";
			this.guiTarget.Name = "guiTarget";
			this.guiTarget.ReadOnly = true;
			this.guiTarget.Width = 63;
			// 
			// guiRequirements
			// 
			this.guiRequirements.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			this.guiRequirements.DataPropertyName = "Requirements";
			this.guiRequirements.HeaderText = "Requirements";
			this.guiRequirements.Name = "guiRequirements";
			this.guiRequirements.ReadOnly = true;
			this.guiRequirements.Width = 97;
			// 
			// guiPath
			// 
			this.guiPath.DataPropertyName = "PatchLocation";
			this.guiPath.HeaderText = "Path";
			this.guiPath.Name = "guiPath";
			this.guiPath.ReadOnly = true;
			this.guiPath.Width = 54;
			// 
			// guiMoveUp
			// 
			this.guiMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.guiMoveUp.BackColor = System.Drawing.Color.Transparent;
			this.guiMoveUp.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.guiMoveUp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.guiMoveUp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.guiMoveUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.guiMoveUp.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiMoveUp.Location = new System.Drawing.Point(523, 12);
			this.guiMoveUp.Name = "guiMoveUp";
			this.guiMoveUp.Size = new System.Drawing.Size(111, 34);
			this.guiMoveUp.TabIndex = 6;
			this.guiMoveUp.Text = "Move Up";
			this.guiMoveUp.UseVisualStyleBackColor = false;
			this.guiMoveUp.Click += new System.EventHandler(this.guiMoveUp_Click);
			// 
			// guiMoveDown
			// 
			this.guiMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.guiMoveDown.BackColor = System.Drawing.Color.Transparent;
			this.guiMoveDown.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.guiMoveDown.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.guiMoveDown.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.guiMoveDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.guiMoveDown.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiMoveDown.Location = new System.Drawing.Point(523, 52);
			this.guiMoveDown.Name = "guiMoveDown";
			this.guiMoveDown.Size = new System.Drawing.Size(111, 34);
			this.guiMoveDown.TabIndex = 7;
			this.guiMoveDown.Text = "Move Down";
			this.guiMoveDown.UseVisualStyleBackColor = false;
			this.guiMoveDown.Click += new System.EventHandler(this.guiMoveDown_Click);
			// 
			// guiRemove
			// 
			this.guiRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.guiRemove.BackColor = System.Drawing.Color.Transparent;
			this.guiRemove.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.guiRemove.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.guiRemove.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.guiRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.guiRemove.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiRemove.Location = new System.Drawing.Point(523, 157);
			this.guiRemove.Name = "guiRemove";
			this.guiRemove.Size = new System.Drawing.Size(111, 34);
			this.guiRemove.TabIndex = 9;
			this.guiRemove.Text = "Remove";
			this.guiRemove.UseVisualStyleBackColor = false;
			this.guiRemove.Click += new System.EventHandler(this.guiRemove_Click);
			// 
			// guiAdd
			// 
			this.guiAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.guiAdd.BackColor = System.Drawing.Color.Transparent;
			this.guiAdd.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.guiAdd.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.guiAdd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.guiAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.guiAdd.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiAdd.Location = new System.Drawing.Point(523, 117);
			this.guiAdd.Name = "guiAdd";
			this.guiAdd.Size = new System.Drawing.Size(111, 34);
			this.guiAdd.TabIndex = 8;
			this.guiAdd.Text = "Add";
			this.guiAdd.UseVisualStyleBackColor = false;
			this.guiAdd.Click += new System.EventHandler(this.guiAdd_Click);
			// 
			// guiClose
			// 
			this.guiClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.guiClose.BackColor = System.Drawing.Color.Transparent;
			this.guiClose.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.guiClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Azure;
			this.guiClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.AliceBlue;
			this.guiClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.guiClose.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.guiClose.Location = new System.Drawing.Point(523, 463);
			this.guiClose.Name = "guiClose";
			this.guiClose.Size = new System.Drawing.Size(111, 34);
			this.guiClose.TabIndex = 11;
			this.guiClose.Text = "Close";
			this.guiClose.UseVisualStyleBackColor = false;
			this.guiClose.Click += new System.EventHandler(this.guiClose_Click);
			// 
			// guiMods
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Linen;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(646, 509);
			this.Controls.Add(this.guiClose);
			this.Controls.Add(this.guiRemove);
			this.Controls.Add(this.guiAdd);
			this.Controls.Add(this.guiMoveDown);
			this.Controls.Add(this.guiMoveUp);
			this.Controls.Add(this.guiInstructionsGridView);
			this.Name = "guiMods";
			this.Text = "Patchwork Launcher";
			this.Load += new System.EventHandler(this.guiMods_Load);
			((System.ComponentModel.ISupportInitialize)(this.guiInstructionsGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView guiInstructionsGridView;
		private System.Windows.Forms.Button guiMoveUp;
		private System.Windows.Forms.Button guiMoveDown;
		private System.Windows.Forms.Button guiRemove;
		private System.Windows.Forms.Button guiAdd;
		private System.Windows.Forms.Button guiClose;
		private System.Windows.Forms.DataGridViewCheckBoxColumn guiOn;
		private System.Windows.Forms.DataGridViewTextBoxColumn guiName;
		private System.Windows.Forms.DataGridViewTextBoxColumn guiTarget;
		private System.Windows.Forms.DataGridViewTextBoxColumn guiRequirements;
		private System.Windows.Forms.DataGridViewTextBoxColumn guiPath;
	}
}

