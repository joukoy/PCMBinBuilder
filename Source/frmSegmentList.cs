using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using static PcmFunctions;

namespace PCMBinBuilder
{
    public partial class FrmSegmentList : Form
    {
        public FrmSegmentList()
        {
            InitializeComponent();
        }

        private static Button CurrentButton;

        private void FrmBuild_Load(object sender, EventArgs e)
        {
            
        }

        public PCMData PCM1;
        private Button[] MyButtons;
        public void StartBuilding(ref PCMData PCM)
        {
            PCM1 = PCM; //Store in local object
            labelOS.Text = PCM1.Segments[1].PN + " " + PCM1.Segments[1].Ver;
            labelPCM.Text = PCM1.Model;
            MyButtons = new Button[12]; //Segments + VIN + Patches
            int i = 1;
            for (int s = 2; s <= 9; s++)
            {
                Button newButton = new Button();
                this.Controls.Add(newButton);
                if (PCM1.Segments[s].Source == "")
                    newButton.Text = SegmentNames[s];
                else
                    newButton.Text = SegmentNames[s] + ":  " + PCM1.Segments[s].PN.ToString() + " " + PCM1.Segments[s].Ver;
                newButton.Location = new Point(10, (i * 30));
                newButton.Size = new Size(350, 25);
                newButton.Click += new System.EventHandler(this.newButton_Click);
                newButton.Tag = s;
                MyButtons[s] = newButton;
                i++;
            }

            // Add button for VIN
            Button VINButton = new Button();
            this.Controls.Add(VINButton);
            VINButton.Text = "VIN";
            VINButton.Location = new Point(10, (i * 30));
            VINButton.Size = new Size(350, 25);
            VINButton.Click += new System.EventHandler(this.VINButton_Click);
            VINButton.Tag = 20;
            MyButtons[10] = VINButton;

            i++;
            //Add button for Patches
            Button PatchButton = new Button();
            this.Controls.Add(PatchButton);
            PatchButton.Text = "Patches";
            PatchButton.Location = new Point(10, ( i * 30));
            PatchButton.Size = new Size(350, 50);
            PatchButton.Click += new System.EventHandler(this.PatchButton_Click);
            PatchButton.Tag = 30;
            MyButtons[11] = PatchButton;
        }

        private void PatchButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int SegNr = (int)button.Tag;

            FrmSelectSegment frm2 = new FrmSelectSegment();
            frm2.Text = "Select patches"; 
            frm2.labelSelectOS.Text = frm2.Text;
            frm2.Tag = 30;
            frm2.LoadPatches(PCM1);

            if (frm2.ShowDialog(this) == DialogResult.OK)
            {
                PCM1 = frm2.PCM1;
                if (PCM1.PatchList.Count > 0)
                {
                    button.Text = "Patches: ";
                    foreach (Patch P in PCM1.PatchList)
                        button.Text += P.Name + ", ";

                }
                else
                    button.Text = "Patches";
            }
            frm2.Dispose();

        }
        
        private void VINButton_Click(object sender, EventArgs e)
        {
            FrmAsk VinDialog = new FrmAsk();

            Button button = sender as Button;

            if (PCM1.NewVIN != "")
                VinDialog.TextBox1.Text = PCM1.NewVIN;
            else
                VinDialog.TextBox1.Text = PCM1.VIN;
            VinDialog.Text = "Enter VIN Code";
            VinDialog.label1.Text = "Enter VIN Code";
            VinDialog.btnReadFromFile.Visible = true;
            VinDialog.AcceptButton = VinDialog.btnOK;

            // Show VinDialog as a modal dialog and determine if DialogResult = OK.
            if (VinDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Read the contents of VinDialog's TextBox.
                PCM1.NewVIN = VinDialog.TextBox1.Text;
                button.Text = "VIN: " + PCM1.NewVIN;
            }
            VinDialog.Dispose();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            CurrentButton = sender as Button;
            int SegNr = (int)CurrentButton.Tag;

            FrmSelectSegment frm2 = new FrmSelectSegment();
            frm2.Text = "Select " + SegmentNames[SegNr];
            frm2.Tag = SegNr;
            frm2.labelSelectOS.Text = frm2.Text;
            
            frm2.LoadCalibrations(CurrentButton, ref PCM1);
            if (frm2.ShowDialog(this) == DialogResult.OK)
            {
                PCM1 = frm2.PCM1;
                if (SegNr == 9)
                {                   
                    if (PCM1.NewVIN =="")
                        MyButtons[10].Text = "VIN: " + PCM1.VIN;
                    else
                        MyButtons[10].Text = "VIN: " + PCM1.NewVIN;
                }
            }
            frm2.Dispose();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (this.Text == "Select segments to swap")
            { 
                //Return back to "Modify BIN"
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                for (int s=2; s<=9; s++)
                {
                    if (PCM1.Segments[s].Source == "")
                    {
                        MessageBox.Show("Please select all segments!", "Please select segments");
                        return;
                    }

                }
                frmAction frmA = new frmAction();
                frmA.Show(this);
                frmA.CreateBinary(PCM1);
            }

        }

        
    }
}
