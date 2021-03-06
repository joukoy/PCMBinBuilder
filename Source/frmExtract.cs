﻿using System;
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

        private CheckBox[] chkSegments;
        public void addCheckBoxes()
        {
            chkSegments = new CheckBox[MaxSeg];
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
                chkSegments[s] = chk;
                i++;
            }

        }


        private string SegmentFileName (string FnameStart, string Extension)
        {
            string FileName = FnameStart + Extension;
            if (radioReplace.Checked)
                return FileName;    

            if (!File.Exists(FileName))
            {
                return FileName;
            }

            if (radioSkip.Checked)
            {
                Logger("Skipping duplicate file: " + FileName);
                return "";
            }

            //radioRename checked
            uint Fnr = 0;
            while (File.Exists(FileName))
            {
                Fnr++;
                FileName = FnameStart + "(" + Fnr.ToString() + ")" + Extension;
            }
            return FileName;
        }


        public void ExtractEeprom(string FileName, string Descr, PCMData PCM)
        {
            try {
                byte[] buf;

                Logger("Reading Eeprom_data from file: " + FileName);
                buf = ReadBin(FileName, 0x4000, 0x4000);
                GetEEpromInfo(buf, ref PCM);
                uint Model = GetModelFromEeprom(buf);
                if (Model < 1999)
                {
                    Logger("Unknown Eeprom");
                    return;
                }
                string FnameStart = Path.Combine(Application.StartupPath, "Calibration", Model + "-" + SegmentNames[9] + "-" + PCM.Segments[9].PN.ToString() + "-" + PCM.Segments[9].Ver);
                string SegFname = SegmentFileName(FnameStart, ".calsegment");
                if (SegFname == "")
                    return;
                Logger("Writing " + SegmentNames[9] + " to file: " + SegFname);
                WriteSegmentToFile(SegFname, 0, 0x4000, buf);
                StreamWriter sw = new StreamWriter(SegFname + ".txt");
                sw.WriteLine(Descr);
                sw.Close();
                Logger("[OK]");
                return;
            }

            catch (Exception ex)
            {
                Logger("Error: " + ex.Message);
            }

        }

        public void ExtractSegments(string FileName, string Descr)
        {
            try
            {
                PCMData PCM = new PCMData();
                byte[] buf;

                long fsize = new System.IO.FileInfo(FileName).Length;
                if (fsize != (long)(512 * 1024) && fsize != (long)(1024 * 1024))
                {
                    Logger("Unknown file: " + FileName + ". Size = " + fsize.ToString());
                    return;
                }

                if (chkSegments[9].Checked)
                {
                    ExtractEeprom(FileName, Descr, PCM);
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
                Logger("[OK]");

                for (int s = 1; s <= 8 ; s++)
                {
                    if (chkSegments[s].Checked) //This segment is selected with checkboxes
                    {
                        string FnameStart;
                        if (s == 1) //OS
                        {
                            FnameStart = Path.Combine(Application.StartupPath, "OS", PCM.Model + "-" + PCM.Segments[1].PN.ToString() + "-" + PCM.Segments[1].Ver);
                            string OsFile = SegmentFileName(FnameStart, ".ossegment1");

                            if (OsFile != "")
                            { 

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
                        }
                        else //Cal Segments
                        {
                            FnameStart = Path.Combine(Application.StartupPath, "Calibration", PCM.Segments[1].PN.ToString() + "-" + SegmentNames[s] + "-" + PCM.Segments[s].PN.ToString() + "-" + PCM.Segments[s].Ver);
                            string SegFname =  SegmentFileName(FnameStart, ".calsegment");
                            if (SegFname != "")
                            {
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
            }
            catch (Exception ex)
            {
                Logger(ex.Message);
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
