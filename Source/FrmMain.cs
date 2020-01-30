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


        private void button3_Click(object sender, EventArgs e)
        {
            FrmPatcher frm2 = new FrmPatcher();            
            frm2.ShowDialog();

        }


        private void FrmMain_Load(object sender, EventArgs e)
        {
            globals.InitializeMe();
        }

    private void button5_Click(object sender, EventArgs e)
        {
            string Fname = globals.SelectFile();
            if (Fname.Length < 1)
                return;
            globals.GetSegmentAddresses(Fname);

            FrmFileinfo frmX = new FrmFileinfo();
            frmX.Show();

            string Finfo = globals.PcmFileInfo(Fname);
            frmX.labelFileInfo.Text = Finfo;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            globals.CleanSegmentInfo();
            FrmSelectSegment frm2 = new FrmSelectSegment();
            frm2.Text = "Select OS";
            frm2.labelSelectOS.Text = frm2.Text;
            frm2.Tag = 1;
            frm2.LoadOSFiles();

            if (frm2.ShowDialog(this) == DialogResult.OK)
            {
                frm2.Dispose();
                FrmSegmentList FrmB = new FrmSegmentList();
                FrmB.Show();                
                FrmB.StartBuilding();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            globals.CleanSegmentInfo();
            FrmModBin FrmB = new FrmModBin();
            FrmB.Show();
            FrmB.LoadBasefile();

        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            frmAction frm2 = new frmAction();
            frm2.Text = "Extracting segments";
            frm2.ExtractSegments();

            if (frm2.ShowDialog(this) == DialogResult.OK)
            {
                frm2.Dispose();
            }

        }
    }

}
