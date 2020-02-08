namespace PCMBinBuilder
{
    partial class frmPCMModel
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
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioP59 = new System.Windows.Forms.RadioButton();
            this.radioP01 = new System.Windows.Forms.RadioButton();
            this.radioP0199 = new System.Windows.Forms.RadioButton();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(11, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(291, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Warning! wrong selection leads to bricked PCM!";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioP0199);
            this.groupBox1.Controls.Add(this.radioP01);
            this.groupBox1.Controls.Add(this.radioP59);
            this.groupBox1.Location = new System.Drawing.Point(-1, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 97);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select PCM Model";
            // 
            // radioP59
            // 
            this.radioP59.AutoSize = true;
            this.radioP59.Location = new System.Drawing.Point(18, 22);
            this.radioP59.Name = "radioP59";
            this.radioP59.Size = new System.Drawing.Size(75, 17);
            this.radioP59.TabIndex = 0;
            this.radioP59.TabStop = true;
            this.radioP59.Text = "P59 - 1MB";
            this.radioP59.UseVisualStyleBackColor = true;
            // 
            // radioP01
            // 
            this.radioP01.AutoSize = true;
            this.radioP01.Location = new System.Drawing.Point(18, 45);
            this.radioP01.Name = "radioP01";
            this.radioP01.Size = new System.Drawing.Size(153, 17);
            this.radioP01.TabIndex = 1;
            this.radioP01.TabStop = true;
            this.radioP01.Text = "P01 - 512 kB (2001 - 2003)";
            this.radioP01.UseVisualStyleBackColor = true;
            // 
            // radioP0199
            // 
            this.radioP0199.AutoSize = true;
            this.radioP0199.Location = new System.Drawing.Point(18, 68);
            this.radioP0199.Name = "radioP0199";
            this.radioP0199.Size = new System.Drawing.Size(147, 17);
            this.radioP0199.TabIndex = 2;
            this.radioP0199.TabStop = true;
            this.radioP0199.Text = "P01 - 512 kB (1999-2000)";
            this.radioP0199.UseVisualStyleBackColor = true;
            this.radioP0199.CheckedChanged += new System.EventHandler(this.radioP0199_CheckedChanged);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(221, 10);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(82, 30);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(221, 47);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(81, 25);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // frmPCMModel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(315, 137);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Name = "frmPCMModel";
            this.Text = "Select PCM model";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.RadioButton radioP0199;
        public System.Windows.Forms.RadioButton radioP01;
        public System.Windows.Forms.RadioButton radioP59;
    }
}