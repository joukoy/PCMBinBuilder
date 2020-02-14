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
using static PcmFunctions;

namespace PCMBinBuilder
{
    public partial class frmAction : Form
    {
        public frmAction()
        {
            InitializeComponent();
        }


        public void FixSchekSums(ref byte[] buf, ref PCMData PCM)
        {
            uint Calculated = 0;
            uint FromFile = 0;

            Logger("Calculating checksums");
            Calculated = CalculateChecksumOS(buf, PCM);
            FromFile = BEToUint16(buf, 0x500);
            if (Calculated != FromFile)
            {
                Logger("OS              checksum: " + FromFile.ToString("X4").PadRight(5) + "=> " + Calculated.ToString("X4").PadRight(6) + "[Fixed]");
                buf[0x500] = (byte)((Calculated & 0xFF00) >> 8);
                buf[0x501] = (byte)(Calculated & 0xFF);
            }
            else
            {
                Logger("OS              checksum: " + buf[0x500].ToString("X2") + buf[0x501].ToString("X2").PadRight(6) + "[OK]");
            }

            for (int s = 2; s <= 8; s++)
            {
                uint StartAddr = PCM.Segments[s].Start;
                uint Length = PCM.Segments[s].Length;
                Calculated = CalculateChecksum(StartAddr, Length, buf);
                FromFile = BEToUint16(buf, StartAddr);
                if (Calculated != FromFile)
                {
                    Logger(SegmentNames[s].PadRight(16) + "checksum: " + FromFile.ToString("X4").PadRight(5) + "=> " + Calculated.ToString("X4").PadRight(6) + "[Fixed]");
                    buf[StartAddr] = (byte)((Calculated & 0xFF00) >> 8);
                    buf[StartAddr + 1] = (byte)(Calculated & 0xFF);
                }
                else
                {
                    Logger(SegmentNames[s].PadRight(16) + "checksum: " + buf[StartAddr].ToString("X2") + buf[StartAddr + 1].ToString("X2").PadRight(6) + "[OK]");
                }
            }

            Logger("Calculating Eeprom key");
            EepromKey Key;
            Key = GetEepromKey(buf);
            Logger("Seed: ".PadRight(16) + Key.Seed.ToString("X4") + Environment.NewLine + "Bin Key: ".PadRight(16) + Key.Key.ToString("X4"), false);
            if (Key.Key != Key.NewKey)
            {
                uint VINAddr = GetVINAddr(buf);

                buf[VINAddr + 2] = (byte)((Key.NewKey & 0xFF00) >> 8);
                buf[VINAddr + 3] = (byte)(Key.NewKey & 0xFF);
                Logger("  *  Calculated: ".PadRight(16) + Key.NewKey.ToString("X4") + " [Fixed]");
            }
            else
            {
                Logger(" [OK]");
            }

        }

        private  void ApplySegments(ref byte[] buf, PCMData PCM)
        {
            for (int s = 0; s <= 9; s++)
            {
                if (PCM.Segments[s].Data != null) 
                {                    
                    Array.Copy(PCM.Segments[s].Data, 0, buf, PCM.Segments[s].Start, PCM.Segments[s].Data.Length);
                }
            }
        }


        public void SaveBintoFile(byte[] buf)
        {
            string FileName = SelectSaveFile();
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
            Logger("File saved to: " + FileName);

        }

        private void SetVinCode(ref byte[] buf, ref PCMData PCM)
        {
            if (PCM.NewVIN != "")
            {
                Logger("Setting VIN to: " + PCM.NewVIN);
                byte[] VINbytes = Encoding.ASCII.GetBytes(PCM.NewVIN);
                Array.Copy(VINbytes, 0, buf, (GetVINAddr(buf) + 33), 17);
                Logger("[OK]");
            }

        }

        private void ApplyPatches(ref byte[] buf, ref PCMData PCM)
        {
            string line;

            Logger("Applying patches");
            foreach (Patch P in PCM.PatchList)
            {
                StreamReader sr = new StreamReader(P.FileName);
                while ((line = sr.ReadLine()) != null)
                {
                    string tmp = Regex.Replace(line, "[^0-9:]", "");
                    if (tmp != line)
                        Logger("(" + line + ")");
                    else
                    {
                        string[] LineParts = line.Split(':');
                        uint Addr = uint.Parse(LineParts[0]);
                        uint Data = uint.Parse(LineParts[1]);
                        if (Addr > PCM.BinSize)
                            throw new FileLoadException(String.Format("File: {0} Address {1} out of range!", P.FileName, Addr.ToString("X4")));
                        if (Data > 0xff)
                            throw new FileLoadException(String.Format("File: {0} Data {1} out of range!", P.FileName, Data.ToString("X4")));
                        //Apply patchrow:
                        Logger("Set address: ".PadRight(16) + Addr.ToString("X4").PadRight(10) + "Data:   " + Data.ToString("X4"));
                        buf[Addr] = byte.Parse(LineParts[1]);
                    }
                }
                sr.Close();

            }
            Logger("[OK]");
        }

        private void FillBuffer(ref byte[] buf, PCMData PCM)
        {
            Logger("Filling empty area");
            for (uint i = 0; i < PCM.BinSize; i++)
            {
                buf[i] = 0x4A;
                i++;
                buf[i] = 0xFC;
            }
            Logger("[OK]");
        }

        private void ValidateSegments(byte[] buf, PCMData PCM)
        {
            Logger("Validating segments");
            if (PCM.Segments[1].Data.Length == (512 * 1024) || PCM.Segments[1].Data.Length == (1024 * 1024))
            {
                //Full binary
                if (PCM.Segments[0].Data != null)
                    throw new Exception("Error: OS segment 1 is full binary but OS segment 2 is not empty");
            }
            else
            {
                //Not full binary in OS segment 1
                if (PCM.Segments[0].Data == null)
                    throw new Exception("Error: OS segment 2 is empty");
                if (PCM.Segments[1].Data.Length != PCM.Segments[1].Length)
                {
                    string Msg = "Error: "+ Environment.NewLine+"OS segment 1 size =" + PCM.Segments[1].Data.Length + Environment.NewLine;
                    Msg += "Should be: " + PCM.Segments[1].Length + "bytes";
                    throw new Exception(Msg);
                }
                if (PCM.Segments[0].Data.Length != PCM.Segments[0].Length)
                {
                    string Msg = "Error: " + Environment.NewLine + "OS segment 2 size =" + PCM.Segments[0].Data.Length + Environment.NewLine;
                    Msg += "Should be: " + PCM.Segments[0].Length + "bytes";
                    throw new Exception(Msg);
                }
                for (int s = 2; s <= 9; s++)
                {
                    if (PCM.Segments[s].Data == null) //No data
                    {
                        string Msg = "Error: " + Environment.NewLine + "Segment: " + SegmentNames[s] + " missing!";
                        throw new Exception(Msg);
                    }
                    if (s < 9 && (PCM.Segments[s].Data[2] != 0 || PCM.Segments[s].Data[3] != s))
                    {
                        //Correct segment number? (No checking for Eeprom_data [segment 9])
                        string Msg = "Error: " + Environment.NewLine + "Segment: " + SegmentNames[s] + " have wrong segment number!";
                        throw new Exception(Msg);
                    }

                }

            }
            Logger("[OK]");
        }
        public Boolean LoadOS(string FileName, ref PCMData PCM)
        {
            //Load OS segment(s) from file(s) to segment buffer
            try
            {
                long fsize = new System.IO.FileInfo(FileName).Length;
                string FileName2 = "";
                byte[] tmpData;
                byte[] tmpData2;

                if (fsize == 0x4000) //OS Segment1 size
                {
                    Logger("Loading OS segment 1");
                    GetPcmType(FileName, ref PCM);
                    tmpData = ReadBin(FileName, 0, (uint)fsize);
                    if (tmpData[0] != 0 || tmpData[1] != 0xFF)
                        throw new Exception("Error: OS segment 1 not valid!");
                    GetSegmentAddresses(tmpData, ref PCM);

                    if (FileName.EndsWith("ossegment1")) //Get OS from segment files
                    {
                        FileName2 = FileName.Replace(".ossegment1", ".ossegment2");
                    }
                    else
                    {
                        FileName2 = SelectFile("Select OS segment 2", true);
                        if (FileName2.Length < 1)
                            throw new FileLoadException("user cancel");
                    }

                    Logger("Loading OS segment 2");
                    fsize = new System.IO.FileInfo(FileName2).Length;
                    if (fsize != PCM.Segments[0].Length)
                    {
                        string Msg = "Error: " + Environment.NewLine + "File: " + FileName2 + " size: " + fsize.ToString() + Environment.NewLine;
                        Msg += "Should be: " + PCM.Segments[0].Length.ToString();
                        throw new FileLoadException(Msg);
                    }
                    tmpData2 = ReadBin(FileName2, 0, PCM.Segments[0].Length);

                    PCM.Segments[1].Data = new byte[0x4000];
                    PCM.Segments[0].Data = new byte[PCM.Segments[0].Length];
                    Array.Copy(tmpData, 0, PCM.Segments[1].Data, 0, PCM.Segments[1].Length); //OS1
                    Array.Copy(tmpData2, 0, PCM.Segments[0].Data, 0, PCM.Segments[0].Length); //OS2
                }
                else if (fsize == (512 * 1024) || fsize == (1024 * 1024)) //Full binary
                {
                    GetPcmType(FileName, ref PCM);
                    Logger("Loading OS file: " + FileName);
                    PCM.Segments[1].Data = ReadBin(FileName, 0, (uint)fsize);
                    if (PCM.Segments[1].Data[0] != 0 || PCM.Segments[1].Data[1] != 0xFF)
                        throw new Exception("Error: OS segment 1 not valid!");
                    GetSegmentAddresses(PCM.Segments[1].Data, ref PCM);
                    PCM.VIN = GetVIN(PCM.Segments[1].Data);
                }
                else
                {
                    string Msg = "Error:" + Environment.NewLine + "File: " + FileName + " size: " + fsize.ToString() + Environment.NewLine;
                    Msg += "Should be: 16384 bytes, 512kB or 1MB";
                    throw new FileLoadException(Msg);
                }
                PCM.Segments[1].Source = FileName;
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

        public Boolean LoadCalSegment(int SegNr, string FileName, ref PCMData PCM)
        {
            //Load CAL segment from file to segment buffer
            try
            {
                Logger("Reading " + SegmentNames[SegNr] + " from " + FileName);
                uint SegSize = PCM.Segments[SegNr].Length;
                long fsize = new System.IO.FileInfo(FileName).Length;
                byte[] tmpData;

                if (fsize == SegSize)
                {
                    tmpData = ReadBin(FileName, 0, SegSize);
                }
                else if (fsize == PCM.BinSize)
                {
                    uint OS1 = PCM.Segments[1].PN;
                    uint OS2 = GetOsidFromFile(FileName);
                    if (OS1 != OS2)
                    {
                        string err = "OS mismatch!" + Environment.NewLine;
                        err += "file: " + PCM.Segments[1].Source + Environment.NewLine + "OS: " + OS1 + Environment.NewLine;
                        err += "file: " + FileName + Environment.NewLine + "OS: " + OS2 + Environment.NewLine;
                        throw new FileLoadException(err);
                    }
                    tmpData = ReadBin(FileName, PCM.Segments[SegNr].Start, SegSize);
                }
                else
                {
                    string Msg = "Error: " + Environment.NewLine;
                    Msg += FileName + " file size = " + fsize.ToString() + " bytes" + Environment.NewLine;
                    Msg += "Should be: " + SegSize.ToString() + " or " + PCM.BinSize.ToString() + " bytes";
                    throw new FileLoadException(Msg);
                }

                //Check if readed segment have correct segment number in address start + 3
                uint FileSegNr = tmpData[3];
                if (FileSegNr != SegNr || tmpData[2] != 0)
                {
                    string Msg = "Error: " + Environment.NewLine;
                    Msg += "Wrong segment number: " + FileSegNr.ToString() + "in file: " + FileName;
                    throw new FileLoadException(Msg);
                }

                PCM.Segments[SegNr].PN = BEToUint32(tmpData, 4);
                string Ver;
                Ver = System.Text.Encoding.ASCII.GetString(tmpData, 8, 2);
                PCM.Segments[SegNr].Ver = Regex.Replace(Ver, "[^a-zA-Z0-9]", "?");

                PCM.Segments[SegNr].Data = new byte[SegSize];
                Array.Copy(tmpData, 0, PCM.Segments[SegNr].Data, 0, SegSize);
                PCM.Segments[SegNr].Source = FileName;
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

        public Boolean LoadEepromData(string FileName, ref PCMData PCM)
        {
            //Load Eeprom segment from file to segment buffer
            try
            {
                Logger("Reading " + SegmentNames[9] + " from " + FileName);
                uint SegSize = PCM.Segments[9].Length;
                long fsize = new System.IO.FileInfo(FileName).Length;
                byte[] tmpData;

                if (fsize == SegSize)
                {
                    tmpData = ReadBin(FileName, 0, SegSize);
                }
                else if (fsize == (512 * 1024) || fsize == (1024 * 1024))
                {
                    tmpData = ReadBin(FileName, PCM.Segments[9].Start, SegSize);
                }
                else
                {
                    string Msg = "Error: " + Environment.NewLine;
                    Msg += FileName + " file size = " + fsize.ToString() + " bytes" + Environment.NewLine;
                    Msg += "Should be: " + SegSize.ToString() + " or " + PCM.BinSize.ToString() + " bytes";
                    throw new FileLoadException(Msg);
                }
                if (GetModelFromEeprom(tmpData) != PCM.EepromType)
                {
                    string Msg = "Error: " + Environment.NewLine;
                    Msg += "Incompatible Eeprom!" + Environment.NewLine;
                    if (PCM.EepromType == 1999)
                    { 
                        Msg += "PCM model " + PCM.Model + " is compatible only with Eeprom from year 1999-2000";
                        throw new FileLoadException(Msg);
                    }
                    else if (PCM.EepromType == 2001)
                    { 
                        Msg += "PCM model " + PCM.Model + " is compatible only with Eeprom from year 2001 and newer";
                        throw new FileLoadException(Msg);
                    }
                    else if(PCM.EepromType == 0)
                    {
                        MessageBox.Show("X");
                    }

                }
                GetEEpromInfo(tmpData, ref PCM);
                if (PCM.Segments[9].Ver == "")
                {
                    string Msg = "Error: " + Environment.NewLine;
                    Msg += "No Eeprom_data version number in file: " + FileName + Environment.NewLine + "Corrupted file?";
                    throw new FileLoadException(Msg);
                }
                PCM.VIN = GetVIN(tmpData);
                if (PCM.VIN == "")
                {
                    string Msg = "Error: " + Environment.NewLine;
                    Msg += "No VIN number in file: " + FileName + Environment.NewLine + "Corrupted file?";
                    throw new FileLoadException(Msg);
                }

                PCM.Segments[9].Data = new byte[SegSize];
                Array.Copy(tmpData, 0, PCM.Segments[9].Data, 0, SegSize);
                PCM.Segments[9].Source = FileName;
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

        public Boolean CreateBinary(PCMData PCM)
        {
            try
            {
                byte[] buf = new byte[PCM.BinSize];

                FillBuffer(ref buf,PCM);
                ValidateSegments(buf,PCM);
                ApplySegments(ref buf,PCM);
                SetVinCode(ref buf, ref PCM);
                ApplyPatches(ref buf,ref PCM);
                FixSchekSums(ref buf,ref PCM);
                Logger("Validating data");
                ValidateBuffer(buf, PCM);
                Logger("[OK]");
                SaveBintoFile(buf);
                Logger("Done");
                return true;
            }
            catch (Exception e)
            {
                Logger("Error: ", true);
                Logger(e.Message);
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


        private void btnClose_Click(object sender, EventArgs e)
        {
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
