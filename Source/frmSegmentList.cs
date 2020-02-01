using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PCMBinBuilder
{
    public partial class FrmSegmentList : Form
    {
        public FrmSegmentList()
        {
            InitializeComponent();
        }

        private void FrmBuild_Load(object sender, EventArgs e)
        {

        }

        public void StartBuilding()
        {
            string Fname;
            labelOS.Text = globals.PcmSegments[1].Source;
            Fname = globals.PcmSegments[1].SourceFile;
            globals.GetPcmType(Fname);
            globals.GetSegmentAddresses(Fname);
            int i = 1;
            for (int s = 2; s <= 9; s++)
            {
                //button1[] BtnX = new button1;
                Button newButton = new Button();
                this.Controls.Add(newButton);
                if (globals.PcmSegments[s].Source == "")
                    newButton.Text = globals.PcmSegments[s].Name;
                else
                    newButton.Text = globals.PcmSegments[s].Name + ":  " + globals.PcmSegments[s].Source;
                newButton.Location = new Point(10, (i * 30));
                newButton.Size = new Size(640, 25);
                newButton.Click += new System.EventHandler(this.newButton_Click);
                newButton.Tag = s;
                i++;
            }

            // Add button for VIN
            Button VINButton = new Button();
            this.Controls.Add(VINButton);
            VINButton.Text = "VIN";
            VINButton.Location = new Point(10, (i * 30));
            VINButton.Size = new Size(640, 25);
            VINButton.Click += new System.EventHandler(this.VINButton_Click);
            VINButton.Tag = 20;


            i++;
            //Add button for Patches
            Button PatchButton = new Button();
            this.Controls.Add(PatchButton);
            PatchButton.Text = "Patches";
            PatchButton.Location = new Point(10, ( i * 30));
            PatchButton.Size = new Size(640, 25);
            PatchButton.Click += new System.EventHandler(this.PatchButton_Click);
            PatchButton.Tag = 30;

        }

        private void PatchButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int SegNr = (int)button.Tag;

            FrmSelectSegment frm2 = new FrmSelectSegment();
            frm2.Text = "Select patches"; 
            frm2.labelSelectOS.Text = frm2.Text;
            frm2.Tag = 30;
            frm2.LoadPatches();

            //frm2.ShowDialog();

            if (frm2.ShowDialog(this) == DialogResult.OK)
            {
                button.Text = "Patches selected";
            }
            frm2.Dispose();

        }

        

        private void VINReadButton_Click(object sender, EventArgs e)
        {
            string VINFile = globals.SelectFile("Load VIN from file");
            if (VINFile.Length > 1)
            {
                string VIN = globals.ReadVIN(VINFile);
                if (VIN != "")
                {
                    for (int i = 0; i < this.Controls.Count; i++)
                        if (this.Controls[i].Tag != null && (int)this.Controls[i].Tag == 20)
                        {
                            globals.NewVIN = VIN;
                            this.Controls[i].Text = "VIN: " + VIN;
                        }
                }
            }

        }

        private void VINButton_Click(object sender, EventArgs e)
        {
            FrmAsk VinDialog = new FrmAsk();

            Button button = sender as Button;

            if (button.Text == "VIN")
                VinDialog.TextBox1.Text = "";
            else
                VinDialog.TextBox1.Text = button.Text.Replace("VIN: ", "");
            VinDialog.Text = "Enter VIN Code";
            VinDialog.label1.Text = "Enter VIN Code";
            VinDialog.btnReadFromFile.Visible = true;
            VinDialog.AcceptButton = VinDialog.btnOK;

            // Show VinDialog as a modal dialog and determine if DialogResult = OK.
            if (VinDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Read the contents of VinDialog's TextBox.
                globals.NewVIN = VinDialog.TextBox1.Text;
                button.Text = "VIN: " + globals.NewVIN;
            }
            VinDialog.Dispose();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int SegNr = (int)button.Tag;

            FrmSelectSegment frm2 = new FrmSelectSegment();
            frm2.Text = "Select " + globals.PcmSegments[SegNr].Name;
            frm2.Tag = SegNr;
            frm2.labelSelectOS.Text = frm2.Text;
            frm2.LoadCalibrations();

            if (frm2.ShowDialog(this) == DialogResult.OK)
            {
                button.Text = globals.PcmSegments[SegNr].Name + ":  " + globals.PcmSegments[SegNr].Source;
            }
            frm2.Dispose();

            //MessageBox.Show(button.Tag.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.Text == "Select segments to swap")
            { 
                //Return back to "Modify BIN
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                for (int s=2; s<=8; s++)
                {
                    if (globals.PcmSegments[s].Source == "")
                    {
                        MessageBox.Show("Please select all segments!", "Please select segments");
                        return;
                    }

                }
                frmAction frmA = new frmAction();
                frmA.CreateBinary();
                if (frmA.ShowDialog(this) == DialogResult.OK)
                {
                    frmA.Dispose();
                    this.Close();
                }
                frmA.Dispose();
            }

        }

    }
}
