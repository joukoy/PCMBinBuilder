namespace PCMBinBuilder
{
    partial class FrmPatcher
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
            this.btnOrgFile = new System.Windows.Forms.Button();
            this.btnModFile = new System.Windows.Forms.Button();
            this.txtBaseFile = new System.Windows.Forms.TextBox();
            this.txtModifierFile = new System.Windows.Forms.TextBox();
            this.btnCompare = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtPatchName = new System.Windows.Forms.TextBox();
            this.labelOS = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOrgFile
            // 
            this.btnOrgFile.Location = new System.Drawing.Point(12, 15);
            this.btnOrgFile.Name = "btnOrgFile";
            this.btnOrgFile.Size = new System.Drawing.Size(100, 34);
            this.btnOrgFile.TabIndex = 1;
            this.btnOrgFile.Text = "Original file";
            this.btnOrgFile.UseVisualStyleBackColor = true;
            this.btnOrgFile.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnModFile
            // 
            this.btnModFile.Location = new System.Drawing.Point(12, 55);
            this.btnModFile.Name = "btnModFile";
            this.btnModFile.Size = new System.Drawing.Size(100, 34);
            this.btnModFile.TabIndex = 2;
            this.btnModFile.Text = "Modified file";
            this.btnModFile.UseVisualStyleBackColor = true;
            this.btnModFile.Click += new System.EventHandler(this.button2_Click);
            // 
            // txtBaseFile
            // 
            this.txtBaseFile.Location = new System.Drawing.Point(118, 22);
            this.txtBaseFile.Name = "txtBaseFile";
            this.txtBaseFile.Size = new System.Drawing.Size(534, 20);
            this.txtBaseFile.TabIndex = 3;
            // 
            // txtModifierFile
            // 
            this.txtModifierFile.Location = new System.Drawing.Point(120, 63);
            this.txtModifierFile.Name = "txtModifierFile";
            this.txtModifierFile.Size = new System.Drawing.Size(531, 20);
            this.txtModifierFile.TabIndex = 4;
            // 
            // btnCompare
            // 
            this.btnCompare.Location = new System.Drawing.Point(12, 98);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(97, 33);
            this.btnCompare.TabIndex = 5;
            this.btnCompare.Text = "Compare";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.button3_Click);
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(18, 137);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(633, 234);
            this.txtResult.TabIndex = 6;
            // 
            // btnSave
            // 
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(556, 390);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(96, 32);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save patch";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.button4_Click);
            // 
            // txtPatchName
            // 
            this.txtPatchName.Enabled = false;
            this.txtPatchName.Location = new System.Drawing.Point(18, 402);
            this.txtPatchName.Name = "txtPatchName";
            this.txtPatchName.Size = new System.Drawing.Size(523, 20);
            this.txtPatchName.TabIndex = 8;
            // 
            // labelOS
            // 
            this.labelOS.AutoSize = true;
            this.labelOS.Location = new System.Drawing.Point(159, 103);
            this.labelOS.Name = "labelOS";
            this.labelOS.Size = new System.Drawing.Size(10, 13);
            this.labelOS.TabIndex = 9;
            this.labelOS.Text = "-";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(119, 103);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "OS:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 376);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Patch name / Description:";
            // 
            // FrmPatcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 432);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelOS);
            this.Controls.Add(this.txtPatchName);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.txtModifierFile);
            this.Controls.Add(this.txtBaseFile);
            this.Controls.Add(this.btnModFile);
            this.Controls.Add(this.btnOrgFile);
            this.Name = "FrmPatcher";
            this.Text = "Create calibration patch";
            this.Load += new System.EventHandler(this.FrmPatcher_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtBaseFile;
        private System.Windows.Forms.TextBox txtModifierFile;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtPatchName;
        private System.Windows.Forms.Label labelOS;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button btnOrgFile;
        public System.Windows.Forms.Button btnModFile;
        private System.Windows.Forms.Label label2;
    }
}