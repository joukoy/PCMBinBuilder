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
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCreatePatch
            // 
            this.btnCreatePatch.Location = new System.Drawing.Point(12, 144);
            this.btnCreatePatch.Name = "btnCreatePatch";
            this.btnCreatePatch.Size = new System.Drawing.Size(150, 31);
            this.btnCreatePatch.TabIndex = 2;
            this.btnCreatePatch.Text = "Create calibration patch";
            this.btnCreatePatch.UseVisualStyleBackColor = true;
            this.btnCreatePatch.Click += new System.EventHandler(this.button3_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(168, 25);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(312, 221);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(12, 181);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(149, 27);
            this.btnExtract.TabIndex = 4;
            this.btnExtract.Text = "Extract calibrations";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // btnFileInfo
            // 
            this.btnFileInfo.Location = new System.Drawing.Point(12, 24);
            this.btnFileInfo.Name = "btnFileInfo";
            this.btnFileInfo.Size = new System.Drawing.Size(147, 32);
            this.btnFileInfo.TabIndex = 5;
            this.btnFileInfo.Text = "BIN file info";
            this.btnFileInfo.UseVisualStyleBackColor = true;
            this.btnFileInfo.Click += new System.EventHandler(this.button5_Click);
            // 
            // btnBuildBin
            // 
            this.btnBuildBin.Location = new System.Drawing.Point(12, 62);
            this.btnBuildBin.Name = "btnBuildBin";
            this.btnBuildBin.Size = new System.Drawing.Size(147, 29);
            this.btnBuildBin.TabIndex = 6;
            this.btnBuildBin.Text = "Build new BIN";
            this.btnBuildBin.UseVisualStyleBackColor = true;
            this.btnBuildBin.Click += new System.EventHandler(this.button6_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 97);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 27);
            this.button1.TabIndex = 7;
            this.button1.Text = "Modify bin";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 259);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnBuildBin);
            this.Controls.Add(this.btnFileInfo);
            this.Controls.Add(this.btnExtract);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnCreatePatch);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmMain";
            this.Text = "PCM BIN Builder";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnCreatePatch;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.Button btnFileInfo;
        private System.Windows.Forms.Button btnBuildBin;
        private System.Windows.Forms.Button button1;
    }
}

