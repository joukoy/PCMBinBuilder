using System.Collections.Generic;
using System;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
public class PcmFunctions
{

    public struct PcmSegment
    {
        //public string Name;
        public uint PN;
        public string Ver;
        public uint Start;
        public uint Length;
        public byte[] Data;
        public string Source;
    }

    public struct Patch
    {
        public string Name;
        public string FileName;
        public string Description;
    }

    public struct EepromKey
    {
        public UInt16 Seed;
        public UInt16 Key;
        public UInt16 NewKey;
    }

    public const int MaxSeg = 10;
    public static string[] SegmentNames = new string[MaxSeg];

    public class PCMData
    {         
        public String Type;
        public string Model;
        public uint BinSize;
        public long FileSize;
        public uint EepromType;
        public PcmSegment[] Segments;
        public List<Patch> PatchList;
        public string VIN;
        public string NewVIN;

        public PCMData()
        {
            Segments = new PcmSegment[MaxSeg];
            VIN = "";
            NewVIN = "";
            PatchList = new List<Patch>();

            for (int s = 0; s < MaxSeg; s++)
            {
                Segments[s].PN = 0;
                Segments[s].Ver = "";
                Segments[s].Source = "";
            }

        }
        public string GetModifications()
        {
            string Finfo = "";

            for (int i = 2; i <= 9; i++)
            {
                if (Segments[i].Source != "")
                {
                    //Finfo += globals.PcmSegments[i].Name.PadRight(20) + globals.PcmSegments[i].Source + Environment.NewLine;
                    Finfo += SegmentNames[i].PadRight(20) + Segments[i].PN.ToString() + " " + Segments[i].Ver + Environment.NewLine;
                }

            }
            if (NewVIN != "" && VIN != NewVIN)
                Finfo += Environment.NewLine + "VIN => ".PadRight(20) + NewVIN + Environment.NewLine;
            if (PatchList.Count > 0)
            {
                Finfo += Environment.NewLine + "Patches: ";
                foreach (Patch P in PatchList)
                    Finfo += P.Name + ", ";
            }
            return Finfo;
        }

        public void SetPCMModel(string PCMModel)
        {
            if (PCMModel == "P59")
            {
                Type = "P59";
                Model = "P59";
                BinSize = (1024 * 1024);
                EepromType = 2001;
            }
            if (PCMModel == "P01(01-03)")
            {
                Type = "P01";
                BinSize = (512 * 1024);
                Model = "P01(01-03)";
                EepromType = 2001;
            }
            if (PCMModel == "P01(99-00)")
            {
                Type = "P01";
                BinSize = (512 * 1024);
                Model = "P01(99-00)";
                EepromType = 1999;
            }

        }

    }


    public static void InitializeMe()
    {
        SegmentNames[0] = "OS2";
        SegmentNames[1] = "OS";
        SegmentNames[2] = "EngineCal";
        SegmentNames[3] = "EngineDiag";
        SegmentNames[4] = "TransCal";
        SegmentNames[5] = "TransDiag";
        SegmentNames[6] = "Fuel";
        SegmentNames[7] = "System";
        SegmentNames[8] = "Speedo";
        SegmentNames[9] = "EEprom_data";



        if (!File.Exists(Path.Combine(Application.StartupPath, "OS")))
            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "OS"));
        if (!File.Exists(Path.Combine(Application.StartupPath, "Calibration")))
            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Calibration"));
        if (!File.Exists(Path.Combine(Application.StartupPath, "Patches")))
            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Patches"));
    }

    public static string SelectFile(string Title = "Select bin file", Boolean Allfiles = false)
    {

        OpenFileDialog fdlg = new OpenFileDialog();
        fdlg.Title = Title;
        if (Allfiles)
            fdlg.Filter = "All files (*.*)|*.*|BIN files (*.bin)|*.bin";
        else
            fdlg.Filter = "BIN files (*.bin)|*.bin|All files (*.*)|*.*";
        fdlg.FilterIndex = 1;
        fdlg.RestoreDirectory = true;
        if (fdlg.ShowDialog() == DialogResult.OK)
        {
            return fdlg.FileName;
        }
        return "";

    }
    public static string SelectSaveFile(string Filter = "BIN files (*.bin)|*.bin")
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        //saveFileDialog.Filter = "BIN files (*.bin)|*.bin";
        saveFileDialog.Filter = Filter;
        saveFileDialog.RestoreDirectory = true;
        saveFileDialog.Title = "Select bin file";

        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            return saveFileDialog.FileName;
        }
        else
            return "";

    }

    public static uint CalculateChecksumOS(byte[] Data, PCMData PCM)
    {
        uint sum = 0;

        //OS Segment 1
        for (uint i = 0; i < 0x4FF; i += 2)
        {
            sum += BEToUint16(Data, i);
        }
        //OS Segment 2
        for (uint i = 0x502; i < 0x3FFF; i += 2)
        {
            sum += BEToUint16(Data, i);
        }
        //OS Segment 3
        for (uint i = PCM.Segments[0].Start; i < PCM.Segments[0].Start + PCM.Segments[0].Length - 1; i += 2)
        {
            sum += BEToUint16(Data, i);
        }

        sum = (sum & 0xFFFF);
        return (65536 - sum) & 0xFFFF;
    }

    public static uint CalculateChecksum(uint StartAddr, uint Length, byte[] Data)
    {
        uint sum = 0;
        uint EndAddr = StartAddr + Length - 1;

        for (uint i = StartAddr + 2; i < EndAddr; i += 2)
        {
            sum += BEToUint16(Data, i);
        }
        sum = (sum & 0xFFFF);
        return (65536 - sum) & 0xFFFF;
    }

    public static string GetChecksumStatus(byte[] buf, PCMData PCM)
    {
        uint Calculated = 0;
        uint FromFile = 0;
        string Result = "";

        Calculated = CalculateChecksumOS(buf, PCM);
        FromFile = BEToUint16(buf, 0x500);
        if (Calculated == FromFile)
        {
            Result = "OS:".PadRight(12) + "Bin Checksum: " + FromFile.ToString("X4").PadRight(4) + " [OK]";
        }
        else
        {
            Result = "OS:".PadRight(12) + "Bin Checksum: " + FromFile.ToString("X4").PadRight(4) + " * Calculated: " + Calculated.ToString("X4").PadRight(4) + " [FAIL]";
        }

        Result += Environment.NewLine;

        for (int s = 2; s <= 8; s++)
        {
            uint StartAddr = PCM.Segments[s].Start;
            uint Length = PCM.Segments[s].Length;
            Calculated = CalculateChecksum(StartAddr, Length, buf);
            FromFile = BEToUint16(buf, StartAddr);
            if (Calculated == FromFile)
            {
                Result += SegmentNames[s].PadRight(12) + "Bin checksum: " + FromFile.ToString("X4").PadRight(4) + " [OK]";
            }
            else
            {
                Result += SegmentNames[s].PadRight(12) + "Bin checksum: " + FromFile.ToString("X4").PadRight(4) + " * Calculated: " + Calculated.ToString("X4").PadRight(4) + " [FAIL]";
            }
            Result += Environment.NewLine;
        }
        return Result;
    }

    public static uint GetOsidFromFile(string FileName)
    {
        byte[] Buf = new byte[2];
        using (BinaryReader reader = new BinaryReader(File.Open(FileName, FileMode.Open)))
        {
            reader.BaseStream.Seek(0x503, 0);
            if (reader.ReadByte() != 1)
                throw new Exception("Error: Unknown file");
            reader.BaseStream.Seek(0x504, 0);
            uint PN = reader.ReadUInt32BE();
            reader.BaseStream.Close();
            return PN;
        }

    }


    public static string PcmBufInfo(byte[] buf, PCMData PCM)
    {
        GetSegmentAddresses(buf, ref PCM);
        string Finfo = "PCM Model: ".PadRight(20) + PCM.Model + Environment.NewLine + Environment.NewLine;
        if (buf.Length == 16384)
        {
            Finfo += " ".PadLeft(20) + "* Segments *" + Environment.NewLine;
            for (int i = 1; i <= 8; i++)
            {
                Finfo += SegmentNames[i].PadRight(15);
                Finfo += " Size: " + PCM.Segments[i].Length.ToString().PadRight(5) + " (0x" + PCM.Segments[i].Length.ToString("X4") + ")" + Environment.NewLine;
            }
            Finfo += Environment.NewLine;
        }
        else
        {
            GetSegmentInfo(buf, ref PCM);
            Finfo += " ".PadLeft(20) + "* Segments *" + Environment.NewLine;
            for (int i = 1; i <= 8; i++)
            {
                Finfo += SegmentNames[i].PadRight(15) + PCM.Segments[i].PN.ToString().PadRight(8) + " " + PCM.Segments[i].Ver;
                Finfo += ", Size: " + PCM.Segments[i].Length.ToString().PadRight(5) + " (0x" + PCM.Segments[i].Length.ToString("X4") + ")" + Environment.NewLine;
            }
            Finfo += Environment.NewLine + " ".PadLeft(20) + "* Checksums *" + Environment.NewLine;
            Finfo += GetChecksumStatus(buf, PCM) + Environment.NewLine;
            Finfo += " ".PadLeft(15) + "* Eeprom_data *" + Environment.NewLine + GetEEpromInfo(buf, ref PCM);
            Finfo += "VIN".PadRight(15) + GetVIN(buf) + Environment.NewLine + Environment.NewLine;
        }
        return Finfo;
    }

    public static string PcmFileInfo(string FileName, PCMData PCM)
    {
        GetPcmType(FileName, ref PCM);
        if (PCM.Type == "Unknown")
            return "Unknow file";
        long fsize = new System.IO.FileInfo(FileName).Length;
        byte[] buf = new byte[fsize];
        buf = ReadBin(FileName, 0, (uint)fsize);
        return PcmBufInfo(buf, PCM);
    }

    public static void AskPCMModel(ref PCMData PCM, bool P01)
    {
        PCMBinBuilder.frmPCMModel frmP = new PCMBinBuilder.frmPCMModel();
        if (P01)
            frmP.radioP59.Visible = false;
        if (frmP.ShowDialog() == DialogResult.OK)
        {
            if (frmP.radioP59.Checked)
            {
                PCM.Type = "P59";
                PCM.Model = "P59";
                PCM.BinSize = (512 * 1024);
                PCM.EepromType = 2001;
            }
            else if (frmP.radioP01.Checked)
            {
                PCM.Type = "P01";
                PCM.BinSize = (512 * 1024);
                PCM.Model = "P01(01-03)";
                PCM.EepromType = 2001;
            }
            else if (frmP.radioP0199.Checked)
            {
                PCM.Type = "P01";
                PCM.BinSize = (512 * 1024);
                PCM.Model = "P01(99-00)";
                PCM.EepromType = 1999;
            }
        }
        frmP.Dispose();

    }


    public static void GetPcmType(string FileName, ref PCMData PCM)
    {
        PCM.Type = "Unknown";
        PCM.BinSize = 0;
        PCM.Model = "";
        PCM.EepromType = 0;

        PCM.FileSize = new FileInfo(FileName).Length;
        if (PCM.FileSize != (512 * 1024) && PCM.FileSize != (1024 * 1024) && PCM.FileSize != 16384)
        {
            return;
        }
        if (PCM.FileSize == (512 * 1024)) //P01
        {
            byte[] buf = ReadBin(FileName, 0, (uint)PCM.FileSize);
            PCM.EepromType = GetModelFromEeprom(buf);
            if (PCM.EepromType == 1999)
                PCM.SetPCMModel("P01(99-00)");
            else if (PCM.EepromType == 2001)
                PCM.SetPCMModel("P01(01-03)");
            else
                return;
        }
        if (PCM.FileSize == (1024 * 1024)) //P59
        {
            PCM.SetPCMModel("P59");
        }
        if (PCM.FileSize == 16384) //OS segment
        {
            if (Path.GetFileName(FileName).EndsWith("ossegment1") && Path.GetFileName(FileName).StartsWith("P59"))
            {
                PCM.SetPCMModel("P59");
            }
            else if (Path.GetFileName(FileName).EndsWith("ossegment1") && Path.GetFileName(FileName).StartsWith("P01(01-03)"))
            {
                PCM.SetPCMModel("P01(01-03)");
            }
            else if (Path.GetFileName(FileName).EndsWith("ossegment1") && Path.GetFileName(FileName).StartsWith("P01(99-00)"))
            {
                PCM.SetPCMModel("P01(99-00)");
            }
            else
            {
                AskPCMModel(ref PCM, false);
            }
        }

    }

    public static void GetSegmentInfo(byte[] buf, ref PCMData PCM)
    {
        string Ver;

        //Get all segments PN and Version informations. Not for OS
        for (int s = 2; s <= 8; s++)
        {
            PCM.Segments[s].PN = BEToUint32(buf, PCM.Segments[s].Start + 4);

            Ver = System.Text.Encoding.ASCII.GetString(buf, (int)PCM.Segments[s].Start + 8, 2);
            PCM.Segments[s].Ver = Regex.Replace(Ver, "[^a-zA-Z0-9]", "");

        }
        //GetEEpromInfo(buf);
    }

    public static void GetSegmentAddresses(byte[] buf, ref PCMData PCM)
    {

        PCM.Segments[1].Start = 0;
        PCM.Segments[1].Length = 0x4000;
        PCM.Segments[0].Start = 0x020000;
        if (PCM.Type == "P59")
            PCM.Segments[0].Length = 0xDFFFE;
        else
            PCM.Segments[0].Length = 0x5FFFE;
        //EEprom Data:
        PCM.Segments[9].Start = 0x4000;
        PCM.Segments[9].Length = 0x4000;

        PCM.Segments[1].PN = BEToUint32(buf, 0x504);
        PCM.Segments[1].Ver = System.Text.Encoding.ASCII.GetString(buf, 0x508, 2);

        //Read Segment Addresses from bin, starting at 0x514
        uint offset = 0x514;
        for (int s = 2; s <= 8; s++)
        {
            PCM.Segments[s].Start = BEToUint32(buf, offset);
            offset += 4;
            PCM.Segments[s].Length = BEToUint32(buf, offset) - PCM.Segments[s].Start + 1;
            offset += 4;
            if (PCM.Segments[s].Start > PCM.BinSize || (PCM.Segments[s].Start + PCM.Segments[s].Length) > PCM.BinSize)
                throw new Exception("Error: Corrupted file!");
        }

    }


    public static uint GetVINAddr(byte[] buf)
    {
        uint offset = 0;

        if (buf.Length >= (512 * 1024))
        {
            //Full binary
            offset = 0x4000;
        }

        //Find check-word from Eeprom_data:
        uint CheckAddr = offset + 0x88;
        if (BitConverter.ToUInt16(buf, (int)CheckAddr) == 0xA0A5)
            return offset;
        CheckAddr = offset + 0x56;
        if (BitConverter.ToUInt16(buf, (int)CheckAddr) == 0xA0A5)
            return offset;
        CheckAddr = offset + 0x2088;
        if (BitConverter.ToUInt16(buf, (int)CheckAddr) == 0xA0A5)
            return offset + 0x2000;
        CheckAddr = offset + 0x2056;
        if (BitConverter.ToUInt16(buf, (int)CheckAddr) == 0xA0A5)
            return offset + 0x2000;
        else
            return 1;
    }

    public static uint GetModelFromEeprom(byte[] buf)
    {
        uint offset = 0;

        if (buf.Length >= (512 * 1024))
        {
            //Full binary
            offset = 0x4000;
        }

        //Find check-word from Eeprom_data:
        uint CheckAddr = offset + 0x88;
        if (BitConverter.ToUInt16(buf, (int)CheckAddr) == 0xA0A5)
            return 2001;
        CheckAddr = offset + 0x56;
        if (BitConverter.ToUInt16(buf, (int)CheckAddr) == 0xA0A5)
            return 1999;
        CheckAddr = offset + 0x2088;
        if (BitConverter.ToUInt16(buf, (int)CheckAddr) == 0xA0A5)
            return 2001;
        CheckAddr = offset + 0x2056;
        if (BitConverter.ToUInt16(buf, (int)CheckAddr) == 0xA0A5)
            return 1999;
        else
            return 1; //Unknown
    }

    public static string GetVIN(byte[] buf)
    {
        uint VINoffset = GetVINAddr(buf);
        if (VINoffset == 1) //Error
        {
            return "?";
        }
        return ValidateVIN(System.Text.Encoding.ASCII.GetString(buf, (int)VINoffset + 33, 17));
    }

    public static string ReadVIN(string FileName)
    {
        long fsize = new System.IO.FileInfo(FileName).Length;
        byte[] tmpBuf = new byte[fsize];

        tmpBuf = ReadBin(FileName, 0, (uint)fsize);

        return GetVIN(tmpBuf);
    }

    public static EepromKey GetEepromKey(byte[] buf)
    {
        uint VINAddr = GetVINAddr(buf);
        EepromKey tmpKey;

        //Calculate key
        tmpKey.Seed = BEToUint16(buf, VINAddr);
        tmpKey.Key = BEToUint16(buf, VINAddr + 2);

        tmpKey.NewKey = (UInt16)(tmpKey.Seed + 0x5201);
        tmpKey.NewKey = (UInt16)(SwapBytes(tmpKey.NewKey) + 0x9738);
        tmpKey.NewKey = (UInt16)(0xffff - tmpKey.NewKey - 0xd428);

        return tmpKey;
    }

    public static string GetEEpromInfo(byte[] buf, ref PCMData PCM)
    {
        uint VINAddr = GetVINAddr(buf);
        string Ver;
        EepromKey Key;

        if (VINAddr == 1) //Check word not found
        {
            return "Eeprom_data unreadable" + Environment.NewLine;
        }

        PCM.Segments[9].PN = BEToUint32(buf, VINAddr + 4);
        Ver = System.Text.Encoding.ASCII.GetString(buf, (int)VINAddr + 28, 4);
        PCM.Segments[9].Ver = Regex.Replace(Ver, "[^a-zA-Z0-9]", ""); //Filter out all special chars

        Key = GetEepromKey(buf);

        string Ret = "Seed ".PadRight(20) + Key.Seed.ToString("X4") + Environment.NewLine;
        Ret += "Bin Key ".PadRight(20) + Key.Key.ToString("X4");
        if (Key.Key == Key.NewKey)
            Ret += " [OK]" + Environment.NewLine;
        else
            Ret += " * Calculated: " + Key.NewKey.ToString("X4") + " [Fail]" + Environment.NewLine;
        Ret += "Hardware ".PadRight(20) + BEToUint32(buf, VINAddr + 4).ToString() + Environment.NewLine;
        Ret += "Serial ".PadRight(20) + System.Text.Encoding.ASCII.GetString(buf, (int)VINAddr + 8, 12) + Environment.NewLine;
        Ret += "Id ".PadRight(20) + BEToUint32(buf, VINAddr + 20).ToString() + Environment.NewLine;
        Ret += "Id2 ".PadRight(20) + BEToUint32(buf, VINAddr + 24).ToString() + Environment.NewLine;
        Ret += "Broadcast ".PadRight(20) + System.Text.Encoding.ASCII.GetString(buf, (int)VINAddr + 28, 4) + Environment.NewLine;

        return Ret;
    }

    public static byte[] ReadBin(string FileName, uint FileOffset, uint Length)
    {

        byte[] buf = new byte[Length];

        long offset = 0;
        long remaining = Length;

        using (BinaryReader freader = new BinaryReader(File.Open(FileName, FileMode.Open)))
        {
            freader.BaseStream.Seek(FileOffset, 0);
            while (remaining > 0)
            {
                int read = freader.Read(buf, (int)offset, (int)remaining);
                if (read <= 0)
                    throw new EndOfStreamException
                        (String.Format("End of stream reached with {0} bytes left to read", remaining));
                remaining -= read;
                offset += read;
            }
            freader.Close();
        }
        return buf;
    }

    public static void WriteSegmentToFile(string FileName, uint StartAddr, uint Length, byte[] Buf)
    {

        using (FileStream stream = new FileStream(FileName, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Buf, (int)StartAddr, (int)Length);
                writer.Close();
            }
        }

    }

    public static void ValidateBuffer(byte[] buf, PCMData PCM)
    {
        if (buf[0x503] != 1)
            throw new Exception("Error: OS segment 1 not valid!");
        if (buf[0x20000] != 0x4E || buf[0x20001] != 0x56)
            throw new Exception("Error: OS segment 2 is not valid!");
        GetSegmentInfo(buf, ref PCM);
        if (PCM.Segments[9].Ver == "")
            throw new Exception("Error: Eeprom_data Broadcast code missing!");
        if (GetVIN(buf) == "")
            throw new Exception("Error: VIN code missing!");
        for (int s = 2; s <= 8; s++)
        {
            if (PCM.Segments[s].Ver == "")
                throw new Exception("Error: " + SegmentNames[s] + " version code missing!");
        }
    }

    public static string ValidateVIN(string VINcode)
    {
        VINcode = Regex.Replace(VINcode, "[^a-zA-Z0-9]", "");   //Replace all special chars with ""
        return VINcode.ToUpper();
    }



    public static uint BEToUint32(byte[] buf, uint offset)
    {
        //Shift first byte 24 bits left, second 16bits left...
        return (uint)((buf[offset] << 24) | (buf[offset + 1] << 16) | (buf[offset + 2] << 8) | buf[offset + 3]);
    }

    public static UInt16 BEToUint16(byte[] buf, uint offset)
    {
        return (UInt16)((buf[offset] << 8) | buf[offset + 1]);
    }

    public static ushort SwapBytes(ushort x)
    {
        return (ushort)((ushort)((x & 0xff) << 8) | ((x >> 8) & 0xff));
    }

    public static uint SwapBytes(uint x)
    {
        return ((x & 0x000000ff) << 24) +
               ((x & 0x0000ff00) << 8) +
               ((x & 0x00ff0000) >> 8) +
               ((x & 0xff000000) >> 24);
    }
}




