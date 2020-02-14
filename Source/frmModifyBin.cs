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
    public partial class FrmModBin : Form
    {
        public FrmModBin()
        {
            InitializeComponent();
        }

        private void FrmModBin_Load(object sender, EventArgs e)
        {

        }

        private static PCMData PCM1;

        public bool LoadBasefile()
        {
            this.Text = "Modify BIN";
            string FileName = SelectFile();
            if (FileName.Length < 1)
                return false;
            long fsize = new System.IO.FileInfo(FileName).Length;
            if (fsize != (512*1024) && fsize != (1024*1024))
            {
                MessageBox.Show("Unknown file", "Unknown file");
                return false;
            }
            PCM1 = InitPCM();
            PCM1.Segments[1].Source = FileName;
            frmAction frmA = new frmAction();
            frmA.Show();
            if (!frmA.LoadOS(FileName, ref PCM1))
               return false;
            labelBaseFile.Text = Path.GetFileName(FileName);
            labelBinInfo.Text = PcmBufInfo(PCM1.Segments[1].Data,PCM1);
            return true;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            frmAction frmA = new frmAction();
            frmA.Show(this);
            frmA.CreateBinary(PCM1);
        }

        private void btnSwapSegments_Click(object sender, EventArgs e)
        {
            FrmSegmentList frm2 = new FrmSegmentList();
            frm2.Text = "Select segments to swap";
            frm2.StartBuilding(ref PCM1);
            if (frm2.ShowDialog(this) == DialogResult.OK)
            {
                PCM1 = frm2.PCM1;
                labelBinInfo.Text = PcmBufInfo(PCM1.Segments[1].Data, PCM1);
                //labelBinInfo.Text += Environment.NewLine + "Modifications: " + Environment.NewLine;
                labelMods.Text = GetModifications(PCM1);
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

            if (PCM1.NewVIN != "")
                VinDialog.TextBox1.Text = PCM1.NewVIN;
            else
                VinDialog.TextBox1.Text = PCM1.VIN;
            VinDialog.Text = "Enter VIN Code";
            VinDialog.label1.Text = "Enter VIN Code";
            VinDialog.AcceptButton = VinDialog.btnOK;
            VinDialog.btnReadFromFile.Visible = true;

            // Show VinDialog as a modal dialog and determine if DialogResult = OK.
            if (VinDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Read the contents of VinDialog's TextBox.
                PCM1.NewVIN = VinDialog.TextBox1.Text;
                labelBinInfo.Text = PcmBufInfo(PCM1.Segments[1].Data, PCM1);
                //labelBinInfo.Text += Environment.NewLine + "Modifications: " + Environment.NewLine;
                labelMods.Text = GetModifications(PCM1);
            }
            VinDialog.Dispose();

        }

        private void btnAddPatches_Click(object sender, EventArgs e)
        {
            FrmSelectSegment frmSS = new FrmSelectSegment();
            frmSS.Text = "Select patches";
            frmSS.labelSelectOS.Text = frmSS.Text;
            frmSS.Tag = 40;
            frmSS.LoadPatches(PCM1);

            if (frmSS.ShowDialog(this) == DialogResult.OK)
            {
                PCM1 = frmSS.PCM1;
                frmSS.Dispose();
                labelBinInfo.Text = PcmBufInfo(PCM1.Segments[1].Data, PCM1);
                //labelBinInfo.Text += Environment.NewLine + "            ** Modifications:" + Environment.NewLine;
                labelMods.Text = GetModifications(PCM1);
            }

        }

        private void btnFixCheckSums_Click(object sender, EventArgs e)
        {
            frmAction frmA = new frmAction();
            frmA.FixSchekSums(ref PCM1.Segments[1].Data ,ref PCM1);
            labelBinInfo.Text = PcmBufInfo(PCM1.Segments[1].Data, PCM1);
            //labelBinInfo.Text += Environment.NewLine + "            ** Modifications:" + Environment.NewLine;
            labelMods.Text = GetModifications(PCM1);
            frmA.ShowDialog(this);

        }
    }
}
