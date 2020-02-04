using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace PCMBinBuilder
{
    public partial class frmAction : Form
    {
        public frmAction()
        {
            InitializeComponent();
        }

        private Boolean BuildOK = false;

        public void FixSchekSums(ref byte[] buf)
        {
            uint Calculated = 0;
            uint FromFile = 0;

            Logger("Calculating OS checksum");
            Calculated = globals.CalculateChecksumOS(buf);
            FromFile = (uint)((buf[0x500] << 8) | buf[0x501]);
            if (Calculated != FromFile)
            {
                Logger("Fixing OS checksum: " + FromFile.ToString("X2") + " => " + Calculated.ToString("X2"));
                buf[0x500] = (byte)((Calculated & 0xFF00) >> 8);
                buf[0x501] = (byte)(Calculated & 0xFF);
            }
            else
            {
                Logger("OS checksum: " + buf[0x500].ToString("X1") + buf[0x501].ToString("X1") + " [OK]");
            }

            Logger("Calculating Segment checksums");
            for (int s = 2; s <= 8; s++)
            {
                uint StartAddr = globals.PcmSegments[s].Start;
                uint Length = globals.PcmSegments[s].Length;
                Calculated = globals.CalculateChecksum(StartAddr, Length, buf);
                FromFile = (uint)((buf[StartAddr] << 8) | buf[StartAddr + 1]);
                if (Calculated != FromFile)
                {
                    Logger("Fixing "+globals.PcmSegments[s].Name + " segment checksum: " + FromFile.ToString("X2") + " => " + Calculated.ToString("X2"));
                    buf[StartAddr] = (byte)((Calculated & 0xFF00) >> 8);
                    buf[StartAddr + 1] = (byte)(Calculated & 0xFF);
                }
                else
                {
                    Logger(globals.PcmSegments[s].Name + " checksum: " + buf[StartAddr].ToString("X1") + buf[StartAddr + 1].ToString("X1")+" [OK]");
                }
            }
        }

        private  void ApplySegments(ref byte[] buf)
        {
            for (int s = 0; s <= 9; s++)
            {
                if (globals.PcmSegments[s].Data != null) 
                {
                    Array.Copy(globals.PcmSegments[s].Data, 0, buf, globals.PcmSegments[s].Start, globals.PcmSegments[s].Data.Length);
                }
            }
        }

        public Boolean LoadOS(string FileName)
        {
            try
            {
                long fsize = new System.IO.FileInfo(FileName).Length;
                string FileName2 = "";
                byte[] tmpData;
                byte[] tmpData2;

                if (fsize == 0x4000) //OS Segment1 size
                {
                    Logger("Loading OS segment 1");
                    globals.GetPcmType(FileName);
                    if (globals.PcmType == "Unknown")
                    {
                        DialogResult Res = MessageBox.Show("P59 (1MB) = YES\nP01(512kB) = NO", "PCM Type P59?", MessageBoxButtons.YesNoCancel);
                        if (Res == DialogResult.Cancel)
                            throw new Exception("User cancel");
                        if (Res == DialogResult.Yes)
                        {
                            globals.PcmType = "P59";
                            globals.BinSize = 1024 * 1024;
                        }
                        if (Res == DialogResult.No)
                        {
                            globals.PcmType = "P01";
                            globals.BinSize = 512 * 1024;
                        }

                    }
                    globals.GetSegmentAddresses(FileName);

                    tmpData = globals.ReadBin(FileName, 0, (uint)fsize);

                    if (FileName.EndsWith("ossegment1")) //Get OS from segment files
                    {
                        FileName2 = FileName.Replace(".ossegment1", ".ossegment2");
                    }
                    else
                    {
                        FileName2 = globals.SelectFile("Select OS segment 2", true);
                        if (FileName2.Length < 1)
                            throw new FileLoadException("user cancel");
                    }

                    Logger("Loading OS segment 2");
                    fsize = new System.IO.FileInfo(FileName2).Length;
                    if (fsize != globals.PcmSegments[0].Length)
                    {
                        string Msg = "Error: " + Environment.NewLine + "File: " + FileName2 + " size: " + fsize.ToString() + Environment.NewLine;
                        Msg += "Should be: " + globals.PcmSegments[0].Length.ToString();
                        throw new FileLoadException(Msg);
                    }
                    tmpData2 = globals.ReadBin(FileName2, 0, globals.PcmSegments[0].Length);

                    globals.PcmSegments[1].Data = new byte[0x4000];
                    globals.PcmSegments[0].Data = new byte[globals.PcmSegments[0].Length];
                    Array.Copy(tmpData, 0, globals.PcmSegments[1].Data, 0, globals.PcmSegments[1].Length); //OS1
                    Array.Copy(tmpData2, 0, globals.PcmSegments[0].Data, 0, globals.PcmSegments[0].Length); //OS2
                }
                else if (fsize == (512*1024) || fsize == (1024*1024)) //Full binary
                {
                    globals.GetPcmType(FileName);
                    globals.GetSegmentAddresses(FileName);
                    Logger("Loading OS file: " + FileName);
                    globals.PcmSegments[1].Data = globals.ReadBin(FileName, 0, (uint)fsize);
                }
                else
                {
                    string Msg = "Error:" + Environment.NewLine + "File: " + FileName + " size: " + fsize.ToString() + Environment.NewLine ;
                    Msg +=  "Should be: 16384 bytes, 512kB or 1MB";
                    throw new FileLoadException(Msg);
                }
                Logger("[OK]");
                this.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger(ex.Message);
                return false;
            }
        }

        public Boolean LoadCalSegment(int SegNr, string FileName)
        {
            try { 
                Logger("Reading " + globals.PcmSegments[SegNr].Name + " from " + FileName);
                uint SegSize = globals.PcmSegments[SegNr].Length;
                long fsize = new System.IO.FileInfo(FileName).Length;
                byte[] tmpData;

                if (fsize == SegSize)
                {                     
                    tmpData = globals.ReadBin(FileName, 0, SegSize);
                }
                
                else if (fsize == globals.BinSize)
                {
                    string OS1 = globals.GetOSid();
                    string OS2 = globals.GetOsidFromFile(FileName);
                    if (OS1 != OS2)
                    {
                        string err = "OS mismatch!" + Environment.NewLine;
                        err += "file: " + globals.PcmSegments[1].Source + Environment.NewLine + "OS: " + OS1 + Environment.NewLine;
                        err += "file: " + FileName + Environment.NewLine + "OS: " + OS2 + Environment.NewLine;
                        throw new FileLoadException(err);
                    }
                    tmpData = globals.ReadBin(FileName, globals.PcmSegments[SegNr].Start, SegSize);
                }
                else
                {
                    string Msg = "Error: " + Environment.NewLine;
                    Msg += FileName + " file size = " + fsize.ToString() +" bytes" + Environment.NewLine;
                    Msg += "Should be: "  + SegSize.ToString() + " or " + globals.BinSize.ToString() + " bytes";
                    throw new FileLoadException(Msg);
                }
                    
                //Check if readed segment have correct segment number in address start + 3
                uint FileSegNr = tmpData[3];
                if (FileSegNr != SegNr)
                {
                    string Msg = "Error: " + Environment.NewLine;
                    Msg += "Wrong segment number: " + FileSegNr.ToString() + "in file: " + FileName;
                    throw new FileLoadException(Msg);
                }
                globals.PcmSegments[SegNr].Data = new byte[SegSize];
                Array.Copy(tmpData, 0, globals.PcmSegments[SegNr].Data,0,SegSize);

                globals.PcmSegments[SegNr].PN = (uint)((tmpData[4] <<24)|(tmpData[5] << 16)|(tmpData[6] << 8) | (tmpData[7]));
                byte[] Buf = new byte[2];
                Buf[0] = tmpData[8];
                Buf[1] = tmpData[9];
                globals.PcmSegments[SegNr].Ver = System.Text.Encoding.ASCII.GetString(Buf);
                Logger("[OK]");
                this.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger(ex.Message);
                return false;
            }
        }

        private void SetVinCode(ref byte[] buf)
        {
            if (globals.NewVIN != "")
            {
                Logger("Setting VIN to: " + globals.NewVIN);
                byte[] VINbytes = Encoding.ASCII.GetBytes(globals.NewVIN);
                Array.Copy(VINbytes,0, buf, (globals.VINAddr + 33), 17);
                Logger("[OK]");
            }

        }

        private  void ApplyPatches(ref byte[] buf)
        {
            string line;

            Logger("Applying patches");
            foreach (globals.Patch P in globals.PatchList)
            {
                StreamReader sr = new StreamReader(P.FileName);
                while ((line = sr.ReadLine()) != null )
                {
                    string tmp = Regex.Replace(line, "[^0-9:]", "");
                    if (tmp != line)
                        Logger("(Skip: '" + line+"')");
                    else
                    {
                        string[] LineParts = line.Split(':');
                        uint Addr = uint.Parse(LineParts[0]);
                        uint Data = uint.Parse(LineParts[1]);
                        if (Addr > globals.BinSize)
                            throw new FileLoadException(String.Format("File: {0} Address {1} out of range!", P.FileName, Addr.ToString("X4")));
                        if (Data > 0xff)
                            throw new FileLoadException(String.Format("File: {0} Data {1} out of range!", P.FileName, Data.ToString("X4")));
                        //Apply patchrow:
                        Logger(Addr.ToString("X4") + ":" + Data.ToString("X4"));
                        buf[Addr] = byte.Parse(LineParts[1]);
                    }
                }
                sr.Close();

            }
            Logger("OK");
        }

        public void SaveBintoFile(byte[] buf)
        {
            string FileName = globals.SelectSaveFile();
            if (FileName.Length < 1)
                throw new Exception("User cancel");
            using (FileStream stream = new FileStream(FileName, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(buf);
                    writer.Close();
                }
            }

        }

        private void FillBuffer(ref byte[] buf)
        {
            Logger("Filling empty area");
            for (uint i = 0; i < globals.BinSize; i++)
            {
                buf[i] = 0x4A;
                i++;
                buf[i] = 0xFC;
            }
            Logger("[OK]");
        }

        public Boolean CreateBinary()
        {
            try
            {
                byte[] buf = new byte[globals.BinSize];

                FillBuffer(ref buf);
                ApplySegments(ref buf);
                SetVinCode(ref buf);
                ApplyPatches(ref buf);
                FixSchekSums(ref buf);
                SaveBintoFile(buf);
                Logger("Done");
                BuildOK = true;
                return true;
            }
            catch (Exception e)
            {
                Logger("Error:", true);
                Logger(e.Message);
                BuildOK = false;
                return false;
            }
        }
        private void Logger(string LogText, Boolean newLine = true)
        {
            txtStatus.AppendText(LogText);
            if (newLine)
                txtStatus.AppendText(Environment.NewLine);
            Application.DoEvents();
        }

        public void ExtractSegments (string Fname, string Descr)
        {
            uint Fnr = 0;
            globals.GetPcmType(Fname);
            if (globals.PcmType == "Unknown")
            {
                Logger("Unknown file");
                return ;
            }

            Logger("Reading segment addresses from file: " + Fname, false);
            globals.GetSegmentAddresses(Fname);
            globals.GetSegmentInfo(Fname);
            Logger("[OK]");

            long fsize = new System.IO.FileInfo(Fname).Length;
            if (fsize != (long)(512 * 1024) && fsize != (long)(1024 * 1024))
                throw new FileLoadException("Unknown file: " + Fname + ". Size = " + fsize.ToString());

            byte[] buf = globals.ReadBin(Fname,0,(uint)fsize);
            string tmp = Path.Combine(Application.StartupPath, "OS", globals.PcmType + "-" + globals.GetOSid() + "-" + globals.GetOSVer());
            string OsFile = tmp + ".ossegment1";
            Fnr = 0;
            while (File.Exists(OsFile))
            {
                Fnr++;
                OsFile = tmp + "(" + Fnr.ToString() + ").ossegment1";
            }

            Logger("Writing OS segments", false);
            globals.WriteSegmentToFile(OsFile, globals.PcmSegments[1].Start, globals.PcmSegments[1].Length, buf);
            globals.WriteSegmentToFile(OsFile.Replace(".ossegment1", ".ossegment2"), globals.PcmSegments[0].Start, globals.PcmSegments[0].Length, buf);
            Logger("[OK]");


            //Write description to file
            StreamWriter sw = new StreamWriter(OsFile + ".txt");
            sw.WriteLine(Descr);
            sw.Close();

            Logger("Writing calibration segments", false);

            for (int s = 2; s <= 9; s++)
            {
                string SegFname = Path.Combine(Application.StartupPath, "Calibration", globals.GetOSid() + "-" + globals.PcmSegments[s].Name + "-" + globals.PcmSegments[s].PN.ToString() + "-" + globals.PcmSegments[s].Ver) + ".calsegment";
                Fnr = 0;
                while (File.Exists(SegFname))
                {
                    Fnr++;
                    SegFname = Path.Combine(Application.StartupPath, "Calibration", globals.GetOSid() + "-" + globals.PcmSegments[s].Name + "-" + globals.PcmSegments[s].PN.ToString() + "-" + globals.PcmSegments[s].Ver) + "(" + Fnr.ToString() + ").calsegment";
                }
                globals.WriteSegmentToFile(SegFname, globals.PcmSegments[s].Start, globals.PcmSegments[s].Length, buf);
                sw = new StreamWriter(SegFname + ".txt");
                sw.WriteLine(Descr);
                sw.Close();
            }
            Logger("[OK]");


        }
        public void StartExtractSegments(Boolean Multi)
        {
            this.Show();
            try
            {
                string Fname;
                string Descr;
                Fname = globals.SelectFile();
                if (Fname.Length < 1)
                {
                    Logger("No file selected");
                    return;

                }

                FrmAsk frmA = new FrmAsk();

                if (Multi)
                {
                    string BinFolder = Path.GetDirectoryName(Fname);
                    DirectoryInfo d = new DirectoryInfo(BinFolder);
                    FileInfo[] Files = d.GetFiles("*.bin");
                    foreach (FileInfo file in Files)
                    {
                        ExtractSegments(file.FullName, file.Name.Replace(".bin", ""));
                    }
                }
                else
                {
                    frmA.TextBox1.Text = Path.GetFileName(Fname).Replace(".bin", "");
                    frmA.AcceptButton = frmA.btnOK;

                    if (frmA.ShowDialog(this) != DialogResult.OK) {
                        Logger("User cancel");
                        return;
                    }
                    Descr = frmA.TextBox1.Text;
                    frmA.Dispose();
                    ExtractSegments(Fname, Descr);

                }
                Logger("Extract done");

            }
            catch (Exception e) { 
                Logger("Error: " + e.Message);
            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (BuildOK)
                this.DialogResult = DialogResult.OK;
            else
                this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void frmAction_Load(object sender, EventArgs e)
        {

        }

        private void txtStatus_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
