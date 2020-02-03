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
                Logger("OS checksum: " + buf[0x500].ToString("X1") + buf[0x501].ToString("X1") + " OK");
            }
            Logger("(OK)");

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
                    Logger(globals.PcmSegments[s].Name + " checksum: " + buf[StartAddr].ToString("X1") + buf[StartAddr + 1].ToString("X1")+" OK");
                }
            }
            Logger("(OK)");
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
                if (FileName.EndsWith("ossegment1")) //Get OS from segment files
                {
                    Logger("Loading OS segment 1");
                    globals.GetPcmType(FileName);
                    globals.GetSegmentAddresses(FileName);
                    long fsize = new System.IO.FileInfo(FileName).Length;
                    if (fsize != globals.PcmSegments[1].Length)
                        throw new FileLoadException("File: " + FileName + " size: " +fsize.ToString() + "Expected: " + globals.PcmSegments[1].Length.ToString());                    
                    globals.PcmSegments[1].Data = globals.ReadBin(FileName, 0, globals.PcmSegments[1].Length);
                    Logger("Loading OS segment 2");
                    string FileName2 = FileName.Replace(".ossegment1", ".ossegment2");
                    fsize = new System.IO.FileInfo(FileName2).Length;
                    if (fsize != globals.PcmSegments[1].Length)
                        throw new FileLoadException("File: " + FileName2 + " size: " + fsize.ToString() + "Expected: " + globals.PcmSegments[1].Length.ToString());
                    globals.PcmSegments[0].Data = globals.ReadBin(FileName2, 0, globals.PcmSegments[0].Length);
                }
                else //Get full binary file as OS
                {
                    Logger("Loading OS file: " + FileName);
                    long fsize = new System.IO.FileInfo(FileName).Length;
                    if (fsize != (long)(512 * 1024) && fsize != (long)(1024 * 1024))
                        throw new FileLoadException("Incorrect file size,  file: " + FileName + " Size: " + fsize.ToString());
                    globals.PcmSegments[1].Data = globals.ReadBin(FileName, 0, (uint)fsize);
                }
                globals.GetPcmType(FileName);
                globals.GetSegmentAddresses(FileName);
                Logger("(OK)");
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
                if (FileName.EndsWith(".calsegment"))
                {
                    long fsize = new System.IO.FileInfo(FileName).Length;
                    
                    if (fsize != SegSize)
                    {

                        throw new FileLoadException(String.Format("{0} File size = {1}, Expected =  {2}", FileName, fsize, SegSize));
                    }
                    globals.PcmSegments[SegNr].Data = globals.ReadBin(FileName, 0, SegSize);
                }
                else //Read from full binary
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
                    globals.PcmSegments[SegNr].Data = globals.ReadBin(FileName, globals.PcmSegments[SegNr].Start, SegSize);
                }
                //Check if readed segment have correct segment number in address start + 3
                uint FileSegNr = globals.PcmSegments[SegNr].Data[3];
                if (FileSegNr != SegNr)
                {
                    throw new FileLoadException(String.Format("Wrong segment number: {0} in file: {1}", FileSegNr.ToString(), FileName));
                }
                Logger ("(OK)");
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
                Logger("(OK)");
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
            Logger("(OK)");
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
            Logger("(OK)");

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
            Logger("(OK)");


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
            Logger("(OK)");


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
    }
}
