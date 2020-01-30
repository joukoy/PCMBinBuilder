namespace PCMBinBuilder
{
    partial class FrmSelectSegment
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.labelSelectOS = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.txtCalFile = new System.Windows.Forms.TextBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.btnBrowseCalFile = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Enabled = false;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 32);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(573, 167);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // labelSelectOS
            // 
            this.labelSelectOS.AutoSize = true;
            this.labelSelectOS.Location = new System.Drawing.Point(235, 16);
            this.labelSelectOS.Name = "labelSelectOS";
            this.labelSelectOS.Size = new System.Drawing.Size(58, 13);
            this.labelSelectOS.TabIndex = 3;
            this.labelSelectOS.Text = "Select OS:";
            // 
            // btnOK
            // 
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(506, 235);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(79, 29);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtCalFile
            // 
            this.txtCalFile.Location = new System.Drawing.Point(111, 209);
            this.txtCalFile.Name = "txtCalFile";
            this.txtCalFile.Size = new System.Drawing.Size(448, 20);
            this.txtCalFile.TabIndex = 23;
            this.txtCalFile.TextChanged += new System.EventHandler(this.txtCalFile_TextChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(14, 12);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(96, 17);
            this.radioButton2.TabIndex = 25;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Select from list:";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(14, 212);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(91, 17);
            this.radioButton3.TabIndex = 26;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "Load from file:";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
            // 
            // btnBrowseCalFile
            // 
            this.btnBrowseCalFile.Location = new System.Drawing.Point(562, 210);
            this.btnBrowseCalFile.Name = "btnBrowseCalFile";
            this.btnBrowseCalFile.Size = new System.Drawing.Size(26, 19);
            this.btnBrowseCalFile.TabIndex = 27;
            this.btnBrowseCalFile.Text = "...";
            this.btnBrowseCalFile.UseVisualStyleBackColor = true;
            this.btnBrowseCalFile.Visible = false;
            this.btnBrowseCalFile.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(418, 238);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(78, 25);
            this.btnCancel.TabIndex = 28;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.button1_Click);
            // 
            // FrmSelectSegment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(606, 277);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnBrowseCalFile);
            this.Controls.Add(this.radioButton3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.txtCalFile);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.labelSelectOS);
            this.Controls.Add(this.listView1);
            this.Name = "FrmSelectSegment";
            this.Text = "Select Segment";
            this.Load += new System.EventHandler(this.FrmModifyBin_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button btnOK;
        public System.Windows.Forms.TextBox txtCalFile;
        public System.Windows.Forms.RadioButton radioButton2;
        public System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.Button btnBrowseCalFile;
        public System.Windows.Forms.Label labelSelectOS;
        private System.Windows.Forms.Button btnCancel;
    }
}