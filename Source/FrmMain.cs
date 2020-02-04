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
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }


        private void btnCreatePatch_Click(object sender, EventArgs e)
        {
            globals.CleanMe();
            FrmPatcher frm2 = new FrmPatcher();
            frm2.addCheckBoxes();
            frm2.Show(this);

        }


        private void FrmMain_Load(object sender, EventArgs e)
        {
            globals.InitializeMe();
        }

    private void btnFileInfo_Click(object sender, EventArgs e)
        {
            globals.CleanMe();
            string Fname = globals.SelectFile();
            if (Fname.Length < 1)
                return;
            globals.GetPcmType(Fname);
            if(globals.PcmType == "Unknown")
            {
                MessageBox.Show("Unknown file", "Unknown file");
                return;
            }
            globals.GetSegmentAddresses(Fname);

            FrmFileinfo frmX = new FrmFileinfo();
            frmX.Show(this);

            string Finfo = globals.PcmFileInfo(Fname);
            frmX.labelFileInfo.Text = Finfo;
        }

        private void btnBuildBin_Click(object sender, EventArgs e)
        {
            globals.CleanMe();
            FrmSelectSegment frm2 = new FrmSelectSegment();
            frm2.Text = "Select OS";
            frm2.labelSelectOS.Text = frm2.Text;
            frm2.Tag = 1;
            frm2.LoadOSFiles();

            if (frm2.ShowDialog(this) == DialogResult.OK)
            {
                frm2.Dispose();
                FrmSegmentList FrmB = new FrmSegmentList();
                FrmB.Show(this);                
                FrmB.StartBuilding();
            }
        }

        private void btnModifyBin_Click(object sender, EventArgs e)
        {
            globals.CleanMe();
            FrmModBin FrmB = new FrmModBin();
            FrmB.Show(this);
            FrmB.LoadBasefile();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            globals.CleanMe();
            frmAction frm2 = new frmAction();
            frm2.Text = "Extracting segments";
            frm2.Show(this);
            frm2.StartExtractSegments(radioMulti.Checked);
        }
    }

}
