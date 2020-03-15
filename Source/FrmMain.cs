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
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private void btnCreatePatch_Click(object sender, EventArgs e)
        {
            FrmPatcher frm2 = new FrmPatcher();
            frm2.addCheckBoxes();
            frm2.ShowDialog(this);
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            InitializeMe();
        }

        private void btnFileInfo_Click(object sender, EventArgs e)
        {
            try 
            { 
                string FileName = SelectFile();
                if (FileName.Length < 1)
                    return;

                FrmFileinfo frmX = new FrmFileinfo();
                frmX.Show(this);
                frmX.ShowFileInfo(FileName, radioMultiInfo.Checked);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error");
            }
            
        }

        private void btnBuildBin_Click(object sender, EventArgs e)
        {
            FrmSelectSegment frmSel = new FrmSelectSegment();
            frmSel.Text = "Select OS";
            frmSel.labelSelectOS.Text = frmSel.Text;
            frmSel.Tag = 1;
            PCMData PCM = new PCMData();
            frmSel.LoadOSFiles(sender as Button, ref PCM);
            if (frmSel.ShowDialog() == DialogResult.OK)
            {
                PCM = frmSel.PCM1;
                frmSel.Dispose();
                FrmSegmentList FrmB = new FrmSegmentList();
                FrmB.Show();
                FrmB.StartBuilding(ref PCM);
            }
        }

        private void btnModifyBin_Click(object sender, EventArgs e)
        {
            FrmModBin FrmMod = new FrmModBin();
            if (!FrmMod.LoadBasefile())
                return;
            FrmMod.ShowDialog(this);
            FrmMod.Dispose();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            frmExtract frmEx = new frmExtract();
            frmEx.ShowDialog(this);
            frmEx.Dispose();
        }

        private void radioSingleInfo_CheckedChanged(object sender, EventArgs e)
        {

        }

    }

}
