using System.Collections.Generic;
using System;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
internal class globals
{


    public struct PcmSegment
    {
        public string Name;
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

    public struct PCMinfo
    {
        public String Type;
        public string Model;
        public uint BinSize;
        public uint EepromType;
    }

    public const int MaxSeg = 10;
    public static PcmSegment[] PcmSegments = new PcmSegment[MaxSeg];
    public static string VIN = "";
    public static string NewVIN = "";
    public static List<Patch> PatchList;
    public static PCMinfo PCM;
    public static void InitializeMe()
    {
        PcmSegments[0].Name = "OS2";
        PcmSegments[0].PN = 0;
        PcmSegments[1].Name = "OS";
        PcmSegments[2].Name = "EngineCal";
        PcmSegments[3].Name = "EngineDiag";
        PcmSegments[4].Name = "TransCal";
        PcmSegments[5].Name = "TransDiag";
        PcmSegments[6].Name = "Fuel";
        PcmSegments[7].Name = "System";
        PcmSegments[8].Name = "Speedo";
        PcmSegments[9].Name = "EEprom_data";

        PatchList = new List<Patch>();

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

    public static uint CalculateChecksumOS(byte[] Data)
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
        for (uint i = globals.PcmSegments[0].Start; i < globals.PcmSegments[0].Start + globals.PcmSegments[0].Length - 1; i += 2)
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

    public static string GetChecksumStatus(byte[] buf)
    {
        uint Calculated = 0;
        uint FromFile = 0;
        string Result = "";

        Calculated = globals.CalculateChecksumOS(buf);
        FromFile = BEToUint16(buf,0x500);
        if (Calculated == FromFile)
        {
            Result = "OS:".PadRight(12) + "Bin Checksum: " + FromFile.ToString("X4").PadRight(4) + " [OK]";
            //Logger("OS checksum: " + buf[0x500].ToString("X1") + buf[0x501].ToString("X1") + " OK");
        }
        else
        {
            Result = "OS:".PadRight(12) + "Bin Checksum: " + FromFile.ToString("X4").PadRight(4) + " * Calculated: " + Calculated.ToString("X4").PadRight(4) + " [FAIL]";
        }

        Result += Environment.NewLine;

        for (int s = 2; s <= 8; s++)
        {
            uint StartAddr = globals.PcmSegments[s].Start;
            uint Length = globals.PcmSegments[s].Length;
            Calculated = globals.CalculateChecksum(StartAddr, Length, buf);
            FromFile = BEToUint16(buf, StartAddr);
            if (Calculated == FromFile)
            {
                Result += globals.PcmSegments[s].Name.PadRight(12) + "Bin checksum: " + FromFile.ToString("X4").PadRight(4) + " [OK]";
            }
            else
            {
                Result += globals.PcmSegments[s].Name.PadRight(12) + "Bin checksum: " + FromFile.ToString("X4").PadRight(4) + " * Calculated: " + Calculated.ToString("X4").PadRight(4) + " [FAIL]";
            }
            Result += Environment.NewLine;
        }
        return Result;
    }

    public static uint GetOSid()
    {
        return PcmSegments[1].PN;
    }

    public static string GetOSVer()
    {
        return PcmSegments[1].Ver;
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

    public static string GetModifications()
    {
        string Finfo = "";

        for (int i = 2; i <= 9; i++)
        {
            if (PcmSegments[i].Source != "")
            {
                //Finfo += globals.PcmSegments[i].Name.PadRight(20) + globals.PcmSegments[i].Source + Environment.NewLine;
                Finfo += PcmSegments[i].Name.PadRight(20) + PcmSegments[i].PN.ToString() + " " + PcmSegments[i].Ver + Environment.NewLine;
            }

        }
        if (NewVIN != "")
            Finfo += Environment.NewLine + "VIN => ".PadRight(20) + NewVIN + Environment.NewLine;
        if (PatchList.Count > 0)
        {
            Finfo += Environment.NewLine + "Patches: ";
            foreach (Patch P in PatchList)
                Finfo += P.Name + ", ";
        }
        return Finfo;

    }

    public static string PcmBufInfo(byte[] buf, PCMinfo P)
    {
        GetSegmentAddresses(buf,P);
        GetSegmentInfo(buf);
        string Finfo = "PCM Model: ".PadRight(20) + P.Model + Environment.NewLine + Environment.NewLine;
        Finfo += " ".PadLeft(20) + "* Segments *" + Environment.NewLine;
        for (int i = 1; i <= 8; i++)
        {
            Finfo += globals.PcmSegments[i].Name.PadRight(15) + globals.PcmSegments[i].PN.ToString().PadRight(8) + " " + globals.PcmSegments[i].Ver;
            Finfo += " Size: " + globals.PcmSegments[i].Length.ToString().PadRight(5) + " (0x" + globals.PcmSegments[i].Length.ToString("X4") +")" + Environment.NewLine;
        }
        Finfo += Environment.NewLine;
        Finfo += " ".PadLeft(15) + "* Eeprom_data *" + Environment.NewLine + globals.GetEEpromInfo(buf);
        Finfo += "VIN".PadRight(15) + globals.GetVIN(buf) + Environment.NewLine + Environment.NewLine;
        Finfo += GetChecksumStatus(buf);
        return Finfo;
    }

    public static string PcmFileInfo(string FileName)
    {
        PCMinfo P =  GetPcmType(FileName);
        if (P.Type == "Unknown")
            return "Unknow file";
        byte[] buf = new byte[P.BinSize];
        buf = ReadBin(FileName, 0, P.BinSize);
        return PcmBufInfo(buf,P);
    }

    public static PCMinfo GetPcmType(string FileName)
    {
        PCMinfo tmp;
        tmp.Type = "Unknown";
        tmp.BinSize = 0;
        tmp.Model = "";
        tmp.EepromType = 0;

        long fsize = new System.IO.FileInfo(FileName).Length;
        if (fsize != (512*1024) && fsize != (1024*1024) && fsize != 16384)
        {
            return tmp;
        }
        if (fsize == (512*1024)) //P01
        {
            tmp.Type = "P01";
            tmp.BinSize = (512 * 1024);
            byte[] buf = ReadBin(FileName,0, (uint)fsize);
            tmp.EepromType = GetModelFromEeprom(buf);
            if (tmp.EepromType == 1999)
                tmp.Model = "P01(99-00)";
            else
                tmp.Model = "P01(01-03)";
        }
        if (fsize == (1024 * 1024)) //P59
        {
            tmp.Type = "P59";
            tmp.Model = "P59";
            tmp.BinSize = (1024 * 1024);
            tmp.EepromType = 2001;
        }
        if (fsize == 16384) //OS segment
        {
            if (Path.GetFileName(FileName).EndsWith("ossegment1") && Path.GetFileName(FileName).StartsWith("P59")) 
            {
                tmp.Type = "P59";
                tmp.Model = "P59";
                tmp.BinSize = (1024 * 1024);
                tmp.EepromType = 2001;
            }
            else if (Path.GetFileName(FileName).EndsWith("ossegment1") && Path.GetFileName(FileName).StartsWith("P01(01-03)")) 
            {
                tmp.Type = "P01";
                tmp.BinSize = (512 * 1024);
                tmp.Model = "P01(01-03)";
                tmp.EepromType = 2001;
            }
            else if (Path.GetFileName(FileName).EndsWith("ossegment1") && Path.GetFileName(FileName).StartsWith("P01(99-00)"))
            {
                tmp.Type = "P01";
                tmp.BinSize = (512 * 1024);
                tmp.Model = "P01(99-00)";
                tmp.EepromType = 1999;
            }
            else
            {
                PCMBinBuilder.frmPCMModel frmP = new PCMBinBuilder.frmPCMModel();
                if (frmP.ShowDialog() == DialogResult.OK)
                {
                    if (frmP.radioP59.Checked) 
                    {
                        tmp.Type = "P59";
                        tmp.Model = "P59";
                        tmp.BinSize = (512 * 1024);
                        tmp.EepromType = 2001;
                    }
                    else if (frmP.radioP01.Checked)
                    {
                        tmp.Type = "P01";
                        tmp.BinSize = (512 * 1024);
                        tmp.Model = "P01(01-03)";
                        tmp.EepromType = 2001;
                    }
                    else if (frmP.radioP0199.Checked)
                    {
                        tmp.Type = "P01";
                        tmp.BinSize = (512 * 1024);
                        tmp.Model = "P01(99-00)";
                        tmp.EepromType = 1999;
                    }
                }
            }
        }
        return tmp;

    }

    public static void GetSegmentInfo(byte[] buf)
    {
        string Ver;
        //Get all segments PN and Version informations. Not for OS
        for (int s = 2; s <= 8; s++)
        {
            PcmSegments[s].PN = BEToUint32(buf, PcmSegments[s].Start + 4);

            Ver = System.Text.Encoding.ASCII.GetString(buf, (int)PcmSegments[s].Start + 8, 2);
            PcmSegments[s].Ver = Regex.Replace(Ver, "[^a-zA-Z0-9]", "");

        }
        GetEEpromInfo(buf);

    }

    public static void GetSegmentAddresses(byte[] buf, PCMinfo P)
    {

        PcmSegments[1].Start = 0;
        PcmSegments[1].Length = 0x4000;
        PcmSegments[0].Start = 0x020000;
        if (P.Type == "P59")
            PcmSegments[0].Length = 0xDFFFE;
        else
            PcmSegments[0].Length = 0x5FFFE;
        //EEprom Data:
        PcmSegments[9].Start = 0x4000;
        PcmSegments[9].Length = 0x4000;

        PcmSegments[1].PN = BEToUint32(buf, 0x504);
        PcmSegments[1].Ver = System.Text.Encoding.ASCII.GetString(buf,0x508,2);

        //Read Segment Addresses from bin, starting at 0x514
        uint offset = 0x514;
        for (int s = 2; s <= 8; s++)
        {
            PcmSegments[s].Start = BEToUint32(buf, offset);
            offset += 4;
            PcmSegments[s].Length = BEToUint32(buf, offset) - PcmSegments[s].Start + 1;
            offset += 4;
            if (PcmSegments[s].Start > P.BinSize || (PcmSegments[s].Start + PcmSegments[s].Length) > P.BinSize)
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
            return 1;
    }

    public static string GetVIN(byte[] buf)
    {
        uint VINoffset = GetVINAddr(buf);

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

    public static string GetEEpromInfo(byte[] buf)
    {
        uint VINAddr = GetVINAddr(buf);
        string Ver;
        EepromKey Key;

        PcmSegments[9].PN = BEToUint32(buf, VINAddr + 4);
        Ver = System.Text.Encoding.ASCII.GetString(buf, (int)VINAddr + 28, 4);
        PcmSegments[9].Ver = Regex.Replace(Ver, "[^a-zA-Z0-9]", ""); //Filter out all special chars

        Key = GetEepromKey(buf);

        string Ret = "Seed ".PadRight(20) + Key.Seed.ToString("X4") + Environment.NewLine;
        Ret += "Bin Key ".PadRight(20) + Key.Key.ToString("X4") ;
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

    public static void CleanMe()
    {
        for (int s = 0; s < MaxSeg; s++)
        {
            PcmSegments[s].Source = "";
            PcmSegments[s].Data = null;
        }
        PatchList = new List<Patch>();
        NewVIN = "";
        VIN = "";
        PCM.Type = "";
        PCM.Model = "";
        PCM.BinSize = 0;

    }

    public static void ValidateBuffer(byte[] buf)
    {
        if(buf[0x503] != 1)
            throw new Exception("Error: OS segment 1 not valid!");
        if (buf[0x20000] != 0x4E || buf[0x20001] != 0x56)
            throw new Exception("Error: OS segment 2 is not valid!");
        GetSegmentInfo(buf);
        if (PcmSegments[9].Ver == "")
            throw new Exception("Error: Eeprom_data Broadcast code missing!");
        if (GetVIN(buf) == "")
            throw new Exception("Error: VIN code missing!");
        for (int s = 2; s <= 8; s++)
        {
            if (PcmSegments[s].Ver == "")
                throw new Exception("Error: " + PcmSegments[s].Name + " version code missing!");
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
        return (uint)((buf[offset] << 24) | (buf[offset  + 1] << 16) | (buf[offset + 2] << 8) | buf[offset + 3]);
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

public static class Helpers
{

    public static byte[] Reverse(this byte[] b)
    {
        Array.Reverse(b);
        return b;
    }
    public static UInt16 ReadUInt16BE(this BinaryReader binRdr)
    {
        return BitConverter.ToUInt16(binRdr.ReadBytesRequired(sizeof(UInt16)).Reverse(), 0);
    }

    public static Int16 ReadInt16BE(this BinaryReader binRdr)
    {
        return BitConverter.ToInt16(binRdr.ReadBytesRequired(sizeof(Int16)).Reverse(), 0);
    }

    public static UInt32 ReadUInt32BE(this BinaryReader binRdr)
    {
        return BitConverter.ToUInt32(binRdr.ReadBytesRequired(sizeof(UInt32)).Reverse(), 0);
    }

    public static Int32 ReadInt32BE(this BinaryReader binRdr)
    {
        return BitConverter.ToInt32(binRdr.ReadBytesRequired(sizeof(Int32)).Reverse(), 0);
    }

    public static byte[] ReadBytesRequired(this BinaryReader binRdr, int byteCount)
    {
        var result = binRdr.ReadBytes(byteCount);

        if (result.Length != byteCount)
            throw new EndOfStreamException(string.Format("{0} bytes required from stream, but only {1} returned.", byteCount, result.Length));

        return result;
    }
}