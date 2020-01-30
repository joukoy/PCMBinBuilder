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
    public partial class FrmPatcher : Form
    {
        public FrmPatcher()
        {
            InitializeComponent();
        }

        private void FrmPatcher_Load(object sender, EventArgs e)
        {
        }
        private void labelSrcFile_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string BinFile = globals.SelectFile();
            if (BinFile.Length > 1)
            {
                txtBaseFile.Text = BinFile;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string BinFile = globals.SelectFile();
            if (BinFile.Length > 1)
            {
                txtModifierFile.Text = BinFile;
            }

        }
        public void CompareSegment(uint StartAddress,uint EndAddress,byte[] OrgFile, byte[] ModFile)
        {
            for (uint i = StartAddress + 4; i < EndAddress; i++)
            {
                if (OrgFile[i] != ModFile[i])
                {
                    globals.patchAddr.Add(i) ;
                    globals.patchData.Add(ModFile[i]);
                    txtResult.AppendText(i.ToString("X1") + ":" +OrgFile[i].ToString("X1")+ "=>" + ModFile[i].ToString("X1") + Environment.NewLine);
                }

            }
        }
        public void CompareBins()
        {
            long  fsize = new System.IO.FileInfo(txtBaseFile.Text).Length;
            long fsize2 = new System.IO.FileInfo(txtModifierFile.Text).Length;
            if (fsize != fsize2)
            {
                MessageBox.Show("Files are different size, will not compare!");
                return;
            }
            byte[] BaseFile = new byte[fsize];
            byte[] ModifierFile = new byte[fsize];
            globals.patchAddr = new List<uint>();
            globals.patchData = new List<uint>();


            globals.GetSegmentAddresses(txtBaseFile.Text);
            labelOS.Text = globals.GetOSid();
            
            
            globals.ReadSegment(txtModifierFile.Text, 0, (uint)BaseFile.LongLength, (uint)BaseFile.LongLength, ref ModifierFile);
            txtResult.Text = "";
            for (int s=1;s<=8;s++) { 
                CompareSegment(globals.PcmSegments[s].Start, globals.PcmSegments[s].End, BaseFile, ModifierFile);
            }

        }
        private void button3_Click(object sender, EventArgs e)
        {
            CompareBins();
            if (globals.patchAddr.Count>0)
            {
                btnSave.Enabled = true;
                btnSave.Text = "Save patch";
                txtPatchName.Enabled = true;
            }
            else
            {
                btnSave.Enabled = false;
                txtPatchName.Enabled = false;
            }

        }
        public void Logger(string LogText, Boolean NewLine = true)
        {
            txtResult.AppendText(LogText);
            if (NewLine)
                txtResult.AppendText(Environment.NewLine);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string PatchFolder = Path.Combine(Application.StartupPath, "Patches");

            if (!File.Exists(PatchFolder))
                Directory.CreateDirectory(PatchFolder);
            if (txtPatchName.Text.Length >1)
            {
                string PatchName = labelOS.Text + "-" + txtPatchName.Text;
                try
                {
                    StreamWriter sw = new StreamWriter(Path.Combine(PatchFolder,PatchName));
                    for(int i = 0; i < globals.patchAddr.Count ; i++) { 
                        sw.WriteLine(globals.patchAddr[i].ToString() + ":"+globals.patchData[i].ToString());
                    }
                    sw.Close();
                }
                catch (Exception ex)
                {
                    Logger("Exception: " + ex.Message);
                }
                finally
                {
                    Logger("Patch saved");
                }
            }
        }
    }
}
