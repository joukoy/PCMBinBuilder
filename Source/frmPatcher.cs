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

        private static List<uint> PatchData;
        private static List<uint> PatchAddr;
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
                    PatchAddr.Add(i) ;
                    PatchData.Add(ModFile[i]);
                    txtResult.AppendText(i.ToString("X1") + ":" +OrgFile[i].ToString("X1")+ "=>" + ModFile[i].ToString("X1") + Environment.NewLine);
                }

            }
        }
        public void CompareBins()
        {
            try 
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
                PatchAddr = new List<uint>();
                PatchData = new List<uint>();

                globals.GetPcmType(txtBaseFile.Text);
                globals.GetSegmentAddresses(txtBaseFile.Text);
                labelOS.Text = globals.GetOSid();


                //globals.ReadSegmentFromBin(txtModifierFile.Text, 0, (uint)BaseFile.LongLength,  ref ModifierFile);

                BaseFile = globals.ReadBinFile(txtBaseFile.Text);
                ModifierFile = globals.ReadBinFile(txtModifierFile.Text);
                txtResult.Text = "";
                for (int s=0;s<=8;s++) { //Compare all segments, but eeprom-data
                    CompareSegment(globals.PcmSegments[s].Start, globals.PcmSegments[s].End, BaseFile, ModifierFile);
                }
            
            }
            catch (Exception ex)
            {
                Logger("Error: " + ex.Message);
            }

        }
        private void button3_Click(object sender, EventArgs e)
        {
            CompareBins();
            if (PatchAddr.Count>0)
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

            try
            {

                if (!File.Exists(PatchFolder))
                    Directory.CreateDirectory(PatchFolder);
                if (txtPatchName.Text.Length < 1)
                {
                    Logger("Supply patch name (File name)");
                    return;
                }
                else
                {
                    string PatchName = labelOS.Text + "-" + txtPatchName.Text;
                    string PatchFile = Path.Combine(PatchFolder, PatchName + ".pcmpatch");
                    uint Fnr = 0;
                    while (File.Exists(PatchFile))
                    {
                        Fnr++;
                        PatchFile = Path.Combine(PatchFolder, PatchName + "(" + Fnr.ToString() + ").pcmpatch");
                    }
                    Logger("Saving to file: " + PatchFile);
                    StreamWriter sw = new StreamWriter(PatchFile);
                    sw.WriteLine(txtPatchName.Text);
                    for (int i = 0; i < PatchAddr.Count; i++)
                    {
                        sw.WriteLine(PatchAddr[i].ToString() + ":" + PatchData[i].ToString());
                    }
                    sw.Close();
                    Logger("Patch saved");
                }
            }
            catch (Exception ex)
            {
                Logger("Error: " + ex.Message);
            }
        }
    }
}
