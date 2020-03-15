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
    public partial class FrmPatcher : Form
    {
        public FrmPatcher()
        {
            InitializeComponent();
        }

        private static List<uint> PatchData;
        private static List<uint> PatchAddr;
        private PCMData BasePCM;
        private PCMData ModPCM;
        private CheckBox[] chkSegments;

        private void FrmPatcher_Load(object sender, EventArgs e)
        {
        }

        private void btnOrgFile_Click(object sender, EventArgs e)
        {
            string BinFile = SelectFile();
            if (BinFile.Length > 1)
            {
                try { 
                txtBaseFile.Text = BinFile;
                BasePCM = new PCMData();
                Logger("Original file:");
                Logger(PcmFileInfo(BinFile, BasePCM));
                if (txtModifierFile.Text != "")
                    CheckSegmentCompatibility();
                }
                catch (Exception ex)
                {
                    Logger(ex.Message);
                }
            }
        }

        private void btnModFile_Click(object sender, EventArgs e)
        {
            string BinFile = SelectFile();
            if (BinFile.Length > 1)
            {
                try { 
                txtModifierFile.Text = BinFile;
                ModPCM = new PCMData();
                Logger("Modified file:");
                Logger(PcmFileInfo(BinFile, ModPCM));
                if (txtBaseFile.Text != "")
                    CheckSegmentCompatibility();
                }
                catch (Exception ex)
                {
                    Logger(ex.Message);
                }
            }
        }

        private void CheckSegmentCompatibility()
        {
            long fsize = new System.IO.FileInfo(txtBaseFile.Text).Length;
            byte[] BaseFile = new byte[fsize];
            byte[] ModifierFile = new byte[fsize];

            GetPcmType(txtBaseFile.Text, ref BasePCM);
            GetPcmType(txtModifierFile.Text, ref ModPCM);
            BaseFile = ReadBin(txtBaseFile.Text, 0, (uint)fsize);
            GetSegmentAddresses(BaseFile, ref BasePCM);
            GetSegmentInfo(BaseFile, ref BasePCM);
            ModifierFile = ReadBin(txtModifierFile.Text, 0, (uint)fsize);
            GetSegmentAddresses(ModifierFile, ref ModPCM);
            GetSegmentInfo(ModifierFile, ref ModPCM);
            for (int s=1; s<=8; s++)
            {
                if (BasePCM.Segments[s].PN != ModPCM.Segments[s].PN || BasePCM.Segments[s].Ver != ModPCM.Segments[s].Ver)
                {
                    Logger(SegmentNames[s].PadLeft(11) + " differ: " + BasePCM.Segments[s].PN.ToString().PadRight(8) + " " + BasePCM.Segments[s].Ver + " <> " + ModPCM.Segments[s].PN.ToString().PadRight(8) + " " + ModPCM.Segments[s].Ver);
                    chkSegments[s].Enabled = false;
                }
                else
                {
                    chkSegments[s].Enabled = true;
                }
            }
        }

        public void CompareSegment(uint StartAddress,uint Length,byte[] OrgFile, byte[] ModFile)
        {
            uint EndAddress = StartAddress + Length - 1;
            uint Mods = 0;
            for (uint i = StartAddress; i < EndAddress; i++)
            {
                if (OrgFile[i] != ModFile[i])
                {
                    PatchAddr.Add(i) ;
                    PatchData.Add(ModFile[i]);
                    if (Mods <= (uint)numLimitRows.Value)
                        Logger(i.ToString("X1") + ":" +OrgFile[i].ToString("X1")+ "=>" + ModFile[i].ToString("X1") );
                    Mods++;
                }
            }
            if (Mods >= (uint)numLimitRows.Value)
                Logger("Showing " + numLimitRows.Value.ToString() + " of " + Mods.ToString() + " differences");

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

                BasePCM = new PCMData();
                ModPCM = new PCMData();

                GetPcmType(txtBaseFile.Text, ref BasePCM);
                GetPcmType(txtModifierFile.Text, ref ModPCM);

                uint BaseOS = GetOsidFromFile(txtBaseFile.Text);
                uint ModOS = GetOsidFromFile(txtModifierFile.Text);
                if (BaseOS != ModOS)
                {
                    MessageBox.Show("File1 OS = " + BaseOS.ToString() + ", File2 OS = " + ModOS.ToString() + Environment.NewLine + "Will not compare!", "OS Mismatch");
                    return;
                }
                BaseFile = ReadBin(txtBaseFile.Text, 0, (uint)fsize);
                GetSegmentAddresses(BaseFile,ref BasePCM);
                labelOS.Text = BasePCM.Segments[1].PN.ToString();
                ModifierFile = ReadBin(txtModifierFile.Text, 0, (uint)fsize);
                txtResult.Text = "";
                for (int s = 1; s <= 9 ; s++)
                {
                    if (chkSegments[s].Enabled && chkSegments[s].Checked)
                    {
                        Logger("Comparing segment " + SegmentNames[s]);

                        CompareSegment(BasePCM.Segments[s].Start, BasePCM.Segments[s].Length, BaseFile, ModifierFile);
                        if (s == 1) //If OS is selected, compare OS2 segment, too
                            CompareSegment(BasePCM.Segments[0].Start, BasePCM.Segments[0].Length, BaseFile, ModifierFile);
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
            if (PatchAddr != null && PatchAddr.Count>0)
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
            chkSegments = new CheckBox[MaxSeg];
            int Left = 12;
            for (int s=1;s<=9; s++) 
            { 
                CheckBox chk = new CheckBox();
                this.Controls.Add(chk);
                chk.Location = new Point(Left, 77);
                chk.Text = SegmentNames[s];
                chk.AutoSize = true;
                Left += chk.Width +5;
                chk.Tag = s;
                if (s < 9) //Don't compare eeprom as default
                    chk.Checked = true;
                chkSegments[s] = chk;
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
