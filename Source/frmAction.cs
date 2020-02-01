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
        public  uint CalculateChecksumOS(byte[] Data)
        {
            uint sum = 0;
            byte high;
            byte low;

            //OS Segment 0
            for (uint i = 0; i < 0x4FF; i += 2)
            {
                high = Data[i];
                low = Data[i + 1];
                sum = (uint)(sum + ((high << 8) | low));
            }
            //OS Segment 1
            for (uint i = 0x502; i < 0x3FFF; i += 2)
            {
                high = Data[i];
                low = Data[i + 1];
                sum = (uint)(sum + ((high << 8) | low));
            }
            //OS Segment 2
            for (uint i = globals.PcmSegments[0].Start; i < globals.PcmSegments[0].End; i += 2)
            {
                high = Data[i];
                low = Data[i + 1];
                sum = (uint)(sum + ((high << 8) | low));
            }

            sum = (sum & 0xFFFF);
            return (65536 - sum) & 0xFFFF;
        }

        public  uint CalculateChecksum(uint StartAddr, uint EndAddr, byte[] Data)
        {
            uint sum = 0;
            byte high;
            byte low;

            for (uint i = StartAddr+2; i < EndAddr; i += 2)
            {
                high = Data[i];
                low = Data[i + 1];
                sum = (uint)(sum + ((high << 8) | low));
            }
            sum = (sum & 0xFFFF);
            return (65536 - sum) & 0xFFFF;
        }

        public void FixSchekSums(ref byte[] buf)
        {
            uint Calculated = 0;
            uint FromFile = 0;

            Logger("Calculating OS checksum");
            Calculated = CalculateChecksumOS(buf);
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
                uint EndAddr = globals.PcmSegments[s].End;
                Calculated = CalculateChecksum(StartAddr, EndAddr, buf);
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

        private void frmAction_Load(object sender, EventArgs e)
        {
        
        }
        private  void LoadCalSegments(ref byte[] buf)
        {
            for (int s = 2; s<9; s++)
            {
                if (globals.PcmSegments[s].GetFrom != "") { 
                    string FileName = globals.PcmSegments[s].SourceFile;
                    if (globals.PcmSegments[s].GetFrom == "cal")
                    {
                        long fsize = new System.IO.FileInfo(FileName).Length;
                        long SegSize = globals.PcmSegments[s].End - globals.PcmSegments[s].Start;
                        if (fsize != SegSize)
                        {

                            throw new FileLoadException(String.Format("{0} File size = {1}, Expected =  {2}", FileName, fsize, SegSize));
                        }
                        globals.ReadSegmentFile(FileName, globals.PcmSegments[s].Start, globals.PcmSegments[s].End, ref buf);
                    }
                    else if (globals.PcmSegments[s].GetFrom == "file")
                    {
                        string OS1 = globals.GetOSid();
                        string OS2 = globals.GetOsidFromFile(FileName);
                        if (OS1 != OS2)
                        {
                            string err = "OS mismatch!" + Environment.NewLine;
                            err += "file: " + globals.PcmSegments[1].SourceFile + " OS: " + OS1 + Environment.NewLine;
                            err += "file: " + FileName + " OS: " + OS2 + Environment.NewLine; 
                            throw new FileLoadException(err);
                        }
                            
                        globals.ReadSegmentFromBin(FileName, globals.PcmSegments[s].Start, globals.PcmSegments[s].End, ref buf);
                    }
                    //Check if readed segment have correct segment number in address start + 3
                    uint SegNr = buf[globals.PcmSegments[s].Start + 3];
                    if (SegNr != s)
                    {
                        throw new FileLoadException(String.Format("Wrong segment number {0} in file: {1}", SegNr.ToString(), FileName));
                    }
                }
            }
        }
        private  void SetVinCode(ref byte[] buf)
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

        private void LoadOS(ref byte[] buf)
        {
            if (globals.PcmSegments[1].GetFrom == "file") //Get full binary file as OS
            {
                Logger("Loading OS file: " + globals.PcmSegments[1].SourceFile);
                buf = globals.ReadBinFile(globals.PcmSegments[1].SourceFile);
            }
            else
            {
                Logger("Loading OS segment 1");
                globals.ReadSegmentFile(globals.PcmSegments[1].SourceFile, 0, globals.PcmSegments[1].End, ref buf);
                Logger("Loading OS segment 2");
                //OS Segment 2 file = OS segment 1 file, only last digit differ
                globals.ReadSegmentFile(globals.PcmSegments[1].SourceFile.Replace(".ossegment1",".ossegment2"), globals.PcmSegments[0].Start, globals.PcmSegments[0].End, ref buf);
            }
            Logger("(OK)");
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

        public Boolean CreateBinary()
        {
            try
            {
                byte[] buf = new byte[globals.BinSize];

                FillBuffer(ref buf);
                LoadOS(ref buf);
                LoadCalSegments(ref buf);
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

            byte[] buf = globals.ReadBinFile(Fname);
            string tmp = Path.Combine(Application.StartupPath, "OS", globals.PcmType + "-" + globals.GetOSid() + "-" + globals.GetOSVer());
            string OsFile = tmp + ".ossegment1";
            Fnr = 0;
            while (File.Exists(OsFile))
            {
                Fnr++;
                OsFile = tmp + "(" + Fnr.ToString() + ").ossegment1";
            }

            Logger("Writing OS segments", false);
            globals.WriteSegmentToFile(OsFile, globals.PcmSegments[1].Start, globals.PcmSegments[1].End, buf);
            globals.WriteSegmentToFile(OsFile.Replace(".ossegment1", ".ossegment2"), globals.PcmSegments[0].Start, globals.PcmSegments[0].End, buf);
            Logger("(OK)");

            //EEprom Data:
            //globals.WriteSegmentToFile(OsFile + ".eepromdata", globals.PcmSegments[9].Start, globals.PcmSegments[9].End, buf);

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
                globals.WriteSegmentToFile(SegFname, globals.PcmSegments[s].Start, globals.PcmSegments[s].End, buf);
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

                    // Show frmA as a modal dialog and determine if DialogResult = OK.
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
    }
}
