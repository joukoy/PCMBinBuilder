namespace PCMBinBuilder
{
    partial class FrmModBin
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
            this.label1 = new System.Windows.Forms.Label();
            this.labelBaseFile = new System.Windows.Forms.Label();
            this.btnAddPatches = new System.Windows.Forms.Button();
            this.btnChangeVIN = new System.Windows.Forms.Button();
            this.btnSwapSegments = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.labelBinInfo = new System.Windows.Forms.Label();
            this.btnFixCheckSums = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Base file:";
            // 
            // labelBaseFile
            // 
            this.labelBaseFile.AutoSize = true;
            this.labelBaseFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBaseFile.Location = new System.Drawing.Point(92, 10);
            this.labelBaseFile.Name = "labelBaseFile";
            this.labelBaseFile.Size = new System.Drawing.Size(12, 16);
            this.labelBaseFile.TabIndex = 5;
            this.labelBaseFile.Text = "-";
            // 
            // btnAddPatches
            // 
            this.btnAddPatches.Location = new System.Drawing.Point(12, 39);
            this.btnAddPatches.Name = "btnAddPatches";
            this.btnAddPatches.Size = new System.Drawing.Size(99, 27);
            this.btnAddPatches.TabIndex = 7;
            this.btnAddPatches.Text = "Add pacthes";
            this.btnAddPatches.UseVisualStyleBackColor = true;
            this.btnAddPatches.Click += new System.EventHandler(this.btnAddPatches_Click);
            // 
            // btnChangeVIN
            // 
            this.btnChangeVIN.Location = new System.Drawing.Point(117, 39);
            this.btnChangeVIN.Name = "btnChangeVIN";
            this.btnChangeVIN.Size = new System.Drawing.Size(99, 27);
            this.btnChangeVIN.TabIndex = 8;
            this.btnChangeVIN.Text = "Change VIN";
            this.btnChangeVIN.UseVisualStyleBackColor = true;
            this.btnChangeVIN.Click += new System.EventHandler(this.btnChangeVIN_Click);
            // 
            // btnSwapSegments
            // 
            this.btnSwapSegments.Location = new System.Drawing.Point(222, 39);
            this.btnSwapSegments.Name = "btnSwapSegments";
            this.btnSwapSegments.Size = new System.Drawing.Size(99, 26);
            this.btnSwapSegments.TabIndex = 9;
            this.btnSwapSegments.Text = "Swap Segments";
            this.btnSwapSegments.UseVisualStyleBackColor = true;
            this.btnSwapSegments.Click += new System.EventHandler(this.btnSwapSegments_Click);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(443, 39);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(83, 26);
            this.btnApply.TabIndex = 10;
            this.btnApply.Text = "Save As...";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(443, 71);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(83, 26);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // labelBinInfo
            // 
            this.labelBinInfo.AutoSize = true;
            this.labelBinInfo.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBinInfo.Location = new System.Drawing.Point(9, 69);
            this.labelBinInfo.Name = "labelBinInfo";
            this.labelBinInfo.Size = new System.Drawing.Size(63, 15);
            this.labelBinInfo.TabIndex = 12;
            this.labelBinInfo.Text = "BIN Info";
            // 
            // btnFixCheckSums
            // 
            this.btnFixCheckSums.Location = new System.Drawing.Point(327, 39);
            this.btnFixCheckSums.Name = "btnFixCheckSums";
            this.btnFixCheckSums.Size = new System.Drawing.Size(90, 27);
            this.btnFixCheckSums.TabIndex = 13;
            this.btnFixCheckSums.Text = "Fix Checksums";
            this.btnFixCheckSums.UseVisualStyleBackColor = true;
            this.btnFixCheckSums.Click += new System.EventHandler(this.btnFixCheckSums_Click);
            // 
            // FrmModBin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 470);
            this.Controls.Add(this.btnFixCheckSums);
            this.Controls.Add(this.labelBinInfo);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnSwapSegments);
            this.Controls.Add(this.btnChangeVIN);
            this.Controls.Add(this.btnAddPatches);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelBaseFile);
            this.Name = "FrmModBin";
            this.Text = "Modify Bin";
            this.Load += new System.EventHandler(this.FrmModBin_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelBaseFile;
        private System.Windows.Forms.Button btnAddPatches;
        private System.Windows.Forms.Button btnChangeVIN;
        private System.Windows.Forms.Button btnSwapSegments;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label labelBinInfo;
        private System.Windows.Forms.Button btnFixCheckSums;
    }
}