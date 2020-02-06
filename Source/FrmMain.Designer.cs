namespace PCMBinBuilder
{
    partial class FrmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.btnCreatePatch = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnExtract = new System.Windows.Forms.Button();
            this.btnFileInfo = new System.Windows.Forms.Button();
            this.btnBuildBin = new System.Windows.Forms.Button();
            this.btnModifyBin = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioMulti = new System.Windows.Forms.RadioButton();
            this.radioSingle = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCreatePatch
            // 
            this.btnCreatePatch.Location = new System.Drawing.Point(7, 104);
            this.btnCreatePatch.Name = "btnCreatePatch";
            this.btnCreatePatch.Size = new System.Drawing.Size(147, 25);
            this.btnCreatePatch.TabIndex = 4;
            this.btnCreatePatch.Text = "Create patch";
            this.btnCreatePatch.UseVisualStyleBackColor = true;
            this.btnCreatePatch.Click += new System.EventHandler(this.btnCreatePatch_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(179, 10);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(312, 221);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(6, 65);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(141, 27);
            this.btnExtract.TabIndex = 7;
            this.btnExtract.Text = "Extract calibrations";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // btnFileInfo
            // 
            this.btnFileInfo.Location = new System.Drawing.Point(7, 11);
            this.btnFileInfo.Name = "btnFileInfo";
            this.btnFileInfo.Size = new System.Drawing.Size(147, 25);
            this.btnFileInfo.TabIndex = 1;
            this.btnFileInfo.Text = "BIN file info";
            this.btnFileInfo.UseVisualStyleBackColor = true;
            this.btnFileInfo.Click += new System.EventHandler(this.btnFileInfo_Click);
            // 
            // btnBuildBin
            // 
            this.btnBuildBin.Location = new System.Drawing.Point(7, 73);
            this.btnBuildBin.Name = "btnBuildBin";
            this.btnBuildBin.Size = new System.Drawing.Size(147, 25);
            this.btnBuildBin.TabIndex = 3;
            this.btnBuildBin.Text = "Build new BIN";
            this.btnBuildBin.UseVisualStyleBackColor = true;
            this.btnBuildBin.Click += new System.EventHandler(this.btnBuildBin_Click);
            // 
            // btnModifyBin
            // 
            this.btnModifyBin.Location = new System.Drawing.Point(7, 42);
            this.btnModifyBin.Name = "btnModifyBin";
            this.btnModifyBin.Size = new System.Drawing.Size(147, 25);
            this.btnModifyBin.TabIndex = 2;
            this.btnModifyBin.Text = "Modify BIN";
            this.btnModifyBin.UseVisualStyleBackColor = true;
            this.btnModifyBin.Click += new System.EventHandler(this.btnModifyBin_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioMulti);
            this.groupBox1.Controls.Add(this.radioSingle);
            this.groupBox1.Controls.Add(this.btnExtract);
            this.groupBox1.Location = new System.Drawing.Point(7, 135);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(166, 98);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Extract calibrations";
            // 
            // radioMulti
            // 
            this.radioMulti.AutoSize = true;
            this.radioMulti.Location = new System.Drawing.Point(6, 42);
            this.radioMulti.Name = "radioMulti";
            this.radioMulti.Size = new System.Drawing.Size(155, 17);
            this.radioMulti.TabIndex = 6;
            this.radioMulti.Text = "All files in folder (select one)";
            this.radioMulti.UseVisualStyleBackColor = true;
            // 
            // radioSingle
            // 
            this.radioSingle.AutoSize = true;
            this.radioSingle.Checked = true;
            this.radioSingle.Location = new System.Drawing.Point(6, 19);
            this.radioSingle.Name = "radioSingle";
            this.radioSingle.Size = new System.Drawing.Size(70, 17);
            this.radioSingle.TabIndex = 5;
            this.radioSingle.TabStop = true;
            this.radioSingle.Text = "Single file";
            this.radioSingle.UseVisualStyleBackColor = true;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 243);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnModifyBin);
            this.Controls.Add(this.btnBuildBin);
            this.Controls.Add(this.btnFileInfo);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnCreatePatch);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmMain";
            this.Text = "PCM BIN Builder";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnCreatePatch;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.Button btnFileInfo;
        private System.Windows.Forms.Button btnBuildBin;
        private System.Windows.Forms.Button btnModifyBin;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioMulti;
        private System.Windows.Forms.RadioButton radioSingle;
    }
}

