using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PCMBinBuilder
{
    public partial class FrmModBin : Form
    {
        public FrmModBin()
        {
            InitializeComponent();
        }

        private void FrmModBin_Load(object sender, EventArgs e)
        {

        }
        public void LoadBasefile()
        {
            this.Text = "Modify BIN";
            string Fname = globals.SelectFile();
            if (Fname.Length < 1)
                return;
            labelBaseFile.Text = Fname;
            labelBinInfo.Text = globals.PcmFileInfo(Fname);
            globals.PcmSegments[1].Source = Fname;
            globals.PcmSegments[1].GetFrom = "file";

        }

        private void btnApply_Click(object sender, EventArgs e)
        {

            byte[] buf = globals.BuildBintoMem();
            globals.SaveBintoFile(buf);
        }

        private void btnSwapSegments_Click(object sender, EventArgs e)
        {
            FrmSegmentList frm2 = new FrmSegmentList();
            frm2.Text = "Select segments to swap";
            frm2.StartBuilding();
            if (frm2.ShowDialog(this) == DialogResult.OK)
            {
                labelBinInfo.Text = globals.PcmFileInfo(labelBaseFile.Text);
                labelBinInfo.Text += Environment.NewLine + "Modifications: " + Environment.NewLine;
                labelBinInfo.Text += globals.GetModifications();
            }
            frm2.Dispose();

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnChangeVIN_Click(object sender, EventArgs e)
        {
            FrmAsk VinDialog = new FrmAsk();

            Button button = sender as Button;

            if (globals.NewVIN != "")
                VinDialog.TextBox1.Text = globals.NewVIN;
            else
                VinDialog.TextBox1.Text = globals.VIN;
            VinDialog.Text = "Enter VIN Code";
            VinDialog.label1.Text = "Enter VIN Code";
            VinDialog.AcceptButton = VinDialog.btnOK;

            // Show VinDialog as a modal dialog and determine if DialogResult = OK.
            if (VinDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Read the contents of VinDialog's TextBox.
                globals.NewVIN = VinDialog.TextBox1.Text;
                labelBinInfo.Text = globals.PcmFileInfo(labelBaseFile.Text);
                labelBinInfo.Text += Environment.NewLine + "Modifications: " + Environment.NewLine;
                labelBinInfo.Text += globals.GetModifications();
            }
            VinDialog.Dispose();

        }

        private void btnAddPatches_Click(object sender, EventArgs e)
        {
            FrmSelectSegment frm2 = new FrmSelectSegment();
            frm2.Text = "Select patches";
            frm2.labelSelectOS.Text = frm2.Text;
            frm2.Tag = 40;
            frm2.LoadPatches();

            if (frm2.ShowDialog(this) == DialogResult.OK)
            {
                frm2.Dispose();
                labelBinInfo.Text = globals.PcmFileInfo(labelBaseFile.Text);
                labelBinInfo.Text += Environment.NewLine + "            ** Modifications:" + Environment.NewLine;
                labelBinInfo.Text += globals.GetModifications();
            }

        }
    }
}
