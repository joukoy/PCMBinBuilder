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

        private void btnOrgFile_Click(object sender, EventArgs e)
        {
            string BinFile = globals.SelectFile();
            if (BinFile.Length > 1)
            {
                txtBaseFile.Text = BinFile;
            }
        }

        private void btnModFile_Click(object sender, EventArgs e)
        {
            string BinFile = globals.SelectFile();
            if (BinFile.Length > 1)
            {
                txtModifierFile.Text = BinFile;
            }

        }
        public void CompareSegment(uint StartAddress,uint Length,byte[] OrgFile, byte[] ModFile)
        {
            uint EndAddress = StartAddress + Length - 1;
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
                long fsize = new System.IO.FileInfo(txtBaseFile.Text).Length;
                long fsize2 = new System.IO.FileInfo(txtModifierFile.Text).Length;
                if (fsize != fsize2)
                {
                    MessageBox.Show("Files are different size, will not compare!");
                    return;
                }
                if (fsize != (uint)512 * 1024 && fsize != (1024 * 1024))
                {

                    MessageBox.Show("Unknown file size (" + fsize.ToString() + "),will not compare!");
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

                BaseFile = globals.ReadBin(txtBaseFile.Text, 0, (uint)fsize);
                ModifierFile = globals.ReadBin(txtModifierFile.Text, 0, (uint)fsize);
                txtResult.Text = "";
                for (int i = 0; i < this.Controls.Count; i++)
                {
                    if (this.Controls[i].Tag != null)
                    {
                        int s = (int)this.Controls[i].Tag;
                        if (this.Controls[i].Text == globals.PcmSegments[s].Name)
                        {
                            CheckBox chk = this.Controls[i] as CheckBox;
                            if (chk.Checked)
                            {
                                CompareSegment(globals.PcmSegments[s].Start, globals.PcmSegments[s].Length, BaseFile, ModifierFile);
                                if (s == 1) //If OS is selected, compare OS2 segment, too
                                    CompareSegment(globals.PcmSegments[0].Start, globals.PcmSegments[0].Length, BaseFile, ModifierFile);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger("Error: " + ex.Message);
            }
        }
         private void btnCompare_Click(object sender, EventArgs e)
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

        private void btnSave_Click(object sender, EventArgs e)
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

        public void addCheckBoxes()
        {
            int i = 0;
            int Left = 12;
            for (int s=1;s<=9; s++) 
            { 
                CheckBox chk = new CheckBox();
                this.Controls.Add(chk);
                chk.Location = new Point(Left, 77);
                chk.Text = globals.PcmSegments[s].Name;
                chk.AutoSize = true;
                Left += chk.Width +5;
                chk.Tag = s;
                if (s < 9) //Don't compare eeprom as default
                    chk.Checked = true;
                i++;
            }

        }
        public void Logger(string LogText, Boolean NewLine = true)
        {
            txtResult.AppendText(LogText);
            if (NewLine)
                txtResult.AppendText(Environment.NewLine);
        }
        private void txtModifierFile_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
