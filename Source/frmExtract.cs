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
    public partial class frmExtract : Form
    {
        public frmExtract()
        {
            InitializeComponent();
            addCheckBoxes();
        }

        public void addCheckBoxes()
        {
            int i = 0;
            int Left = 12;
            for (int s = 1; s <= 9; s++)
            {
                CheckBox chk = new CheckBox();
                this.Controls.Add(chk);
                chk.Location = new Point(Left, 77);
                chk.Text = SegmentNames[s];
                chk.AutoSize = true;
                Left += chk.Width + 5;
                chk.Tag = s;
                chk.Checked = true;
                i++;
            }

        }

        private bool[] GetSelections()
        {
            /* Check what checkboxes are selected 
             * Retun Boolean array of selections */

            bool[] Sel = new bool[MaxSeg];

            for (int i = 0; i < this.Controls.Count; i++)
            {
                int x;
                if (this.Controls[i].Tag != null && int.TryParse(this.Controls[i].Tag.ToString(), out x))
                {
                    int s = (int)this.Controls[i].Tag;
                    if (this.Controls[i].Text == SegmentNames[s])
                    {
                        CheckBox chk = this.Controls[i] as CheckBox;
                        if (chk.Checked)
                            Sel[s] = true;
                        else
                            Sel[s] = false;
                    }
                }
            }
            return Sel;

        }
        public void ExtractSegments(string FileName, string Descr)
        {
            try { 
                uint Fnr = 0;
                PCMData PCM = InitPCM();
                byte[] buf;

                long fsize = new System.IO.FileInfo(FileName).Length;
                if (fsize != (long)(512 * 1024) && fsize != (long)(1024 * 1024))
                { 
                    Logger("Unknown file: " + FileName + ". Size = " + fsize.ToString());
                    return;
                }

                bool[] Selections = GetSelections();
                int scount = 0;
                for (int i = 1; i < MaxSeg; i++)
                {
                    if (Selections[i])
                        scount++;
                }
                if (scount == 1 && Selections[9])   // Only Eeprom_data is selected. Special case.
                {
                    Logger("Reading Eeprom_data from file: " + FileName);
                    buf = ReadBin(FileName, 0x4000, 0x4000);
                    GetEEpromInfo(buf, ref PCM);
                    uint Model = GetModelFromEeprom(buf);
                    if (Model< 1999)
                    {
                        Logger("Unknown Eeprom");
                        return;
                    }
                    string FnameStart = Path.Combine(Application.StartupPath, "Calibration", Model + "-" + SegmentNames[9] + "-" + PCM.Segments[9].PN.ToString() + "-" + PCM.Segments[9].Ver);
                    string SegFname = FnameStart + ".calsegment";
                    Fnr = 0;
                    while (File.Exists(SegFname))
                    {
                        Fnr++;
                        SegFname = FnameStart + "(" + Fnr.ToString() + ").calsegment";
                    }
                    Logger("Writing " + SegmentNames[9] + " to file: " + SegFname );
                    WriteSegmentToFile(SegFname, 0, 0x4000, buf);
                    StreamWriter sw = new StreamWriter(SegFname + ".txt");
                    sw.WriteLine(Descr);
                    sw.Close();
                    Logger("[OK]");
                    return;
                }

                GetPcmType(FileName, ref PCM);
                if (PCM.Type == "Unknown")
                {
                    Logger("Unknown file");
                    return;
                }

                Logger("Reading segment addresses from file: " + FileName);
                buf = ReadBin(FileName, 0, PCM.BinSize);
                GetSegmentAddresses(buf, ref PCM);

                GetSegmentInfo(buf, ref PCM);
                GetEEpromInfo(buf, ref PCM);
                Logger("[OK]");

                for (int s = 1; s < MaxSeg; s++)
                {
                    if (Selections[s])
                    {
                        if (s == 1) //OS
                        {
                            string tmp = Path.Combine(Application.StartupPath, "OS", PCM.Model + "-" + PCM.Segments[1].PN.ToString() + "-" + PCM.Segments[1].Ver);
                            string OsFile = tmp + ".ossegment1";
                            Fnr = 0;
                            while (File.Exists(OsFile))
                            {
                                Fnr++;
                                OsFile = tmp + "(" + Fnr.ToString() + ").ossegment1";
                            }

                            Logger("Writing " + SegmentNames[s] + " to file: " + OsFile + ", size: " + PCM.Segments[s].Length.ToString() + " (0x" + PCM.Segments[s].Length.ToString("X4") + ")");
                            WriteSegmentToFile(OsFile, PCM.Segments[1].Start, PCM.Segments[1].Length, buf);
                            Logger("Writing " + SegmentNames[0] + " to file: " + OsFile.Replace(".ossegment1", ".ossegment2") + ", size: " + PCM.Segments[0].Length.ToString() + " (0x" + PCM.Segments[0].Length.ToString("X4") + ")");
                            WriteSegmentToFile(OsFile.Replace(".ossegment1", ".ossegment2"), PCM.Segments[0].Start, PCM.Segments[0].Length, buf);
                            Logger("[OK]");

                            //Write description to file
                            StreamWriter sw = new StreamWriter(OsFile + ".txt");
                            sw.WriteLine(Descr);
                            sw.Close();
                        }
                        else //Cal Segments
                        {
                            string FnameStart;
                            if (s == 9) //Eeprom_data
                            {
                                FnameStart = Path.Combine(Application.StartupPath, "Calibration", PCM.EepromType.ToString() + "-" + SegmentNames[s] + "-" + PCM.Segments[s].PN.ToString() + "-" + PCM.Segments[s].Ver);
                                GetEEpromInfo(buf, ref PCM);
                            }
                            else
                                FnameStart = Path.Combine(Application.StartupPath, "Calibration", PCM.Segments[1].PN.ToString() + "-" + SegmentNames[s] + "-" + PCM.Segments[s].PN.ToString() + "-" + PCM.Segments[s].Ver);
                            string SegFname = FnameStart + ".calsegment";
                            Fnr = 0;
                            while (File.Exists(SegFname))
                            {
                                Fnr++;
                                SegFname = FnameStart + "(" + Fnr.ToString() + ").calsegment";
                            }
                            Logger("Writing " + SegmentNames[s] + " to file: " + SegFname + ", size: " + PCM.Segments[s].Length.ToString() + " (0x" + PCM.Segments[s].Length.ToString("X4") + ")");
                            WriteSegmentToFile(SegFname, PCM.Segments[s].Start, PCM.Segments[s].Length, buf);
                            StreamWriter sw = new StreamWriter(SegFname + ".txt");
                            sw.WriteLine(Descr);
                            sw.Close();
                            Logger("[OK]");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger("Error: " + ex.Message);
            }

        }

        private void Logger(string LogText, Boolean newLine = true)
        {
            txtStatus.AppendText(LogText);
            if (newLine)
                txtStatus.AppendText(Environment.NewLine);
            Application.DoEvents();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            if (txtPath.Text.Length < 1)
            {
                Logger("No file/folder selected");
                return;
            }
            if (radioSingle.Checked && txtDescr.Text.Length < 1)
            {
                Logger("Please add description");
                return;
            }

            txtStatus.Text = "";
            if (radioMulti.Checked)
            {
                DirectoryInfo d = new DirectoryInfo(txtPath.Text);
                FileInfo[] Files = d.GetFiles("*.bin");
                foreach (FileInfo file in Files)
                {
                    ExtractSegments(file.FullName, file.Name.Replace(".bin", ""));
                }
            }
            else
            {
                ExtractSegments(txtPath.Text, txtDescr.Text);

            }
            Logger("Extract done");

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (radioSingle.Checked)
            { 
                txtPath.Text = SelectFile("");
                txtPath.Tag = txtPath.Text;
                txtDescr.Text = Path.GetFileNameWithoutExtension(txtPath.Text);
            }
            else
            {
                txtPath.Tag = SelectFile("Select one file from folder");
                txtPath.Text = Path.GetDirectoryName (txtPath.Tag.ToString());
            }
        }

        private void radioSingle_CheckedChanged(object sender, EventArgs e)
        {
            if (radioSingle.Checked)
            {
                labelDescr.Visible = true;
                txtDescr.Visible = true;
                if (txtPath.Tag != null)
                    txtPath.Text = txtPath.Tag.ToString();
            }
            else
            {
                labelDescr.Visible = false;
                txtDescr.Visible = false;
                if (txtPath.Tag != null)
                    txtPath.Text = Path.GetDirectoryName(txtPath.Tag.ToString());
            }
        }

        private void labelFile_Click(object sender, EventArgs e)
        {

        }

        private void frmExtract_Load(object sender, EventArgs e)
        {

        }
    }
}
