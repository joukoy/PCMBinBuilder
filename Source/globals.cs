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

    public const int MaxSeg= 10;
    public static PcmSegment[] PcmSegments = new PcmSegment[MaxSeg];
    public static string PcmType="";
    public static uint BinSize = 0;
    public static string VIN="";
    public static string NewVIN="";
    public static List<Patch> PatchList;

    public static void InitializeMe()
    {
        PcmSegments[0].Name = "OS2";
        PcmSegments[1].Name = "OS";
        PcmSegments[0].PN = 0;
        PcmSegments[2].Name = "EngineCal";
        PcmSegments[3].Name = "EngineDiag";
        PcmSegments[4].Name = "TransCal";
        PcmSegments[5].Name = "TransDiag";
        PcmSegments[6].Name = "Fuel";
        PcmSegments[7].Name = "System";
        PcmSegments[8].Name = "Speedo";
        PcmSegments[9].Name = "EEprom_data";

        if (!File.Exists(Path.Combine(Application.StartupPath, "OS")))
            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "OS"));
        if (!File.Exists(Path.Combine(Application.StartupPath, "Calibration")))
            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Calibration"));
        if (!File.Exists(Path.Combine(Application.StartupPath, "Patches")))
            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Patches"));

        PatchList = new List<Patch>();

    }
    public static string SelectFile(string Title = "Select bin file", Boolean Allfiles=false)
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
    public static string SelectSaveFile()
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "BIN files (*.bin)|*.bin";
        saveFileDialog.RestoreDirectory = true;
        saveFileDialog.Title = "Select bin file";

        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            return saveFileDialog.FileName;
        }
        else
            return "";

    }

    public static  uint CalculateChecksumOS(byte[] Data)
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
        for (uint i = globals.PcmSegments[0].Start; i < globals.PcmSegments[0].Start + globals.PcmSegments[0].Length - 1; i += 2)
        {
            high = Data[i];
            low = Data[i + 1];
            sum = (uint)(sum + ((high << 8) | low));
        }

        sum = (sum & 0xFFFF);
        return (65536 - sum) & 0xFFFF;
    }

    public static uint CalculateChecksum(uint StartAddr, uint Length, byte[] Data)
    {
        uint sum = 0;
        byte high;
        byte low;
        uint EndAddr = StartAddr + Length - 1;

        for (uint i = StartAddr + 2; i < EndAddr; i += 2)
        {
            high = Data[i];
            low = Data[i + 1];
            sum = (uint)(sum + ((high << 8) | low));
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
        FromFile = (uint)((buf[0x500] << 8) | buf[0x501]);
        if (Calculated == FromFile)
        {
            Result = "OS:".PadRight(12)  + "Bin Checksum: " + FromFile.ToString("X2").PadRight(4) + " * Calculated: " + Calculated.ToString("X2").PadRight(4) + " [OK]";
            //Logger("OS checksum: " + buf[0x500].ToString("X1") + buf[0x501].ToString("X1") + " OK");
        }
        else
        {
            Result = "OS:".PadRight(12) + "Bin Checksum: " + FromFile.ToString("X2").PadRight(4) + " * Calculated: " + Calculated.ToString("X2").PadRight(4) + " [FAIL]";
        }

        Result += Environment.NewLine;

        for (int s = 2; s <= 8; s++)
        {
            uint StartAddr = globals.PcmSegments[s].Start;
            uint Length = globals.PcmSegments[s].Length;
            Calculated = globals.CalculateChecksum(StartAddr, Length, buf);
            FromFile = (uint)((buf[StartAddr] << 8) | buf[StartAddr + 1]);
            if (Calculated == FromFile)
            {
                Result += globals.PcmSegments[s].Name.PadRight(12) + "Bin checksum: " + FromFile.ToString("X2").PadRight(4) + " * Calculated: " + Calculated.ToString("X2").PadRight(4) + " [OK]";
            }
            else
            {
                Result += globals.PcmSegments[s].Name.PadRight(12) + "Bin checksum: " + FromFile.ToString("X2").PadRight(4) + " * Calculated: " + Calculated.ToString("X2").PadRight(4) + " [FAIL]";
            }
            Result += Environment.NewLine;
        }
        return Result;
    }

    public static string GetOSid()
    {
        return PcmSegments[1].PN.ToString();
    }

    public static string GetOSVer()
    {
        return PcmSegments[1].Ver;
    }

    public static string GetOsidFromFile(string FileName)
    {
        byte[] Buf = new byte[2];
        using (BinaryReader reader = new BinaryReader(File.Open(FileName, FileMode.Open)))
        {
            reader.BaseStream.Seek(0x504, 0);
            uint PN = reader.ReadUInt32BE();
            reader.BaseStream.Close();
            return PN.ToString();
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
            Finfo += "VIN => ".PadRight(20) + NewVIN;
        if (PatchList.Count>0)
        {
            Finfo += Environment.NewLine + "Patches: ";
            foreach (Patch P in PatchList)
                Finfo += P.Name + ", ";
        }
        return Finfo;

    }

    public static string PcmFileInfo(string FileName)
    {        
        GetPcmType(FileName);
        if (PcmType == "Unknown")
            return "Unknow file";
        byte[] buf = new byte[BinSize];
        buf = ReadBin(FileName, 0, BinSize);
        GetSegmentAddresses(buf);
        GetSegmentInfo(buf);
        string Finfo = "PCM Type: ".PadRight(20) + PcmType + Environment.NewLine;
        Finfo += "Segments:" + Environment.NewLine;
        for (int i = 1; i <= 9; i++)
        {
            Finfo += globals.PcmSegments[i].Name.PadRight(20) + globals.PcmSegments[i].PN.ToString().PadRight(10) + " " + globals.PcmSegments[i].Ver + Environment.NewLine;
        }
        Finfo += "VIN".PadRight(20) + globals.ReadVIN(FileName) + Environment.NewLine + Environment.NewLine;
        if (PcmSegments[1].Data != null && (PcmSegments[1].Data.Length == (512*1024) || PcmSegments[1].Data.Length == (1024 * 1024)))
            Finfo += GetChecksumStatus(PcmSegments[1].Data);
        else
        {
            Finfo += GetChecksumStatus(buf);
        }
        return Finfo;
    }

    public static void  GetPcmType(string FileName)
    {
        PcmType = "Unknown";
        long fsize = new System.IO.FileInfo(FileName).Length;
        if (fsize >= (1024 * 1024) || (FileName.EndsWith(".ossegment1") && FileName.Contains("OS\\P59-")))
        { 
            PcmType = "P59";
            BinSize = 1024 * 1024;
        }
        else if(fsize == (512 * 1024) || (FileName.EndsWith(".ossegment1") && FileName.Contains("OS\\P01-"))) { 
            PcmType = "P01";
            BinSize = 512 * 1024;
        }
    }

    public static void GetSegmentInfo(byte[] buf)
    {
        string Ver;
        //Get all segments PN and Version informations. Not for OS
        for (int s = 2; s <= 8; s++)
        {
            PcmSegments[s].PN = BEToUint32(buf, PcmSegments[s].Start + 4);

            Ver = System.Text.Encoding.ASCII.GetString(buf, (int)PcmSegments[s].Start + 8,2);
            PcmSegments[s].Ver = Regex.Replace(Ver, "[^a-zA-Z0-9]", "");

        }
        GetEEpromInfo(buf);

    }

    public static void GetSegmentAddresses(byte[] buf)
    {     
        byte[] tmpBuf = new byte[2];

        PcmSegments[1].Start = 0;
        PcmSegments[1].Length = 0x4000;
        PcmSegments[0].Start = 0x020000;
        if (PcmType == "P01")
            PcmSegments[0].Length = 0x5FFFE;
        else
            PcmSegments[0].Length = 0xDFFFE;
        //EEprom Data:
        PcmSegments[9].Start = 0x4000;
        PcmSegments[9].Length = 0x4000;

        PcmSegments[1].PN = BEToUint32(buf, 0x504);
        tmpBuf[0] = buf[0x508];
        tmpBuf[1] = buf[0x509];
        PcmSegments[1].Ver = System.Text.Encoding.ASCII.GetString(tmpBuf);

        //Read Segment Addresses from bin, starting at 0x514
        uint offset = 0x514;
        for (int s=2; s<=8;s++)
        {
            PcmSegments[s].Start = BEToUint32(buf, offset);
            offset += 4;
            PcmSegments[s].Length = BEToUint32(buf, offset) - PcmSegments[s].Start + 1;
            offset += 4;
        }


    }

    public static uint GetVINAddr(byte[] buf)
    {
        byte[] tmpBuf = new byte[5];
        uint offset = 0;

        if (buf.Length >= (512*1024)) {
            //Full binary
            offset = 0x4000;
        }
        tmpBuf[0] = buf[offset + 0x88];
        tmpBuf[1] = buf[offset + 0x89];
        tmpBuf[2] = buf[offset + 0x2088];
        tmpBuf[3] = buf[offset + 0x2089];
        if (tmpBuf[0] == 0xA5 && tmpBuf[1] == 0xA0)
            return offset; //0x4000 in full binary, 0 in Eeprom-data
        else if (tmpBuf[2] == 0xA5 && tmpBuf[3] == 0xA0)
            return (offset + 0x2000); //0x6000 in full binary, 0x2000 in Eeprom-data
        return 1;
    }

    public static string GetVIN(byte[] buf)
    {
        uint VINoffset = GetVINAddr(buf);

        if (GetVINAddr(buf) == 1)
            return "";
        return ValidateVIN( System.Text.Encoding.ASCII.GetString(buf,(int)VINoffset+33,17));
    }

    public static string ReadVIN(string FileName)
    {        
        long fsize = new System.IO.FileInfo(FileName).Length;
        byte[] tmpBuf = new byte[fsize];

        tmpBuf = ReadBin(FileName, 0, (uint)fsize);
        
        return GetVIN(tmpBuf);
    }


    public static void GetEEpromInfo(byte[] buf)
    {
        uint VINAddr = GetVINAddr(buf);
        string Ver;

        PcmSegments[9].PN = BEToUint32(buf, VINAddr + 4);
        Ver = System.Text.Encoding.ASCII.GetString(buf, (int)VINAddr + 28, 4);
        PcmSegments[9].Ver = Regex.Replace(Ver, "[^a-zA-Z0-9]", "");
    }

    public static byte[] ReadBin(string FileName,uint FileOffset, uint Length)
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
        PcmType = "";
        BinSize = 0;
    }

    public static void ValidateBuffer(byte[] buf)
    {
        GetSegmentInfo(buf);
        if (PcmSegments[9].Ver == "")
            throw new Exception("Error: Eeprom_data version missing!");
        if (GetVIN(buf) =="")
            throw new Exception("Error: VIN code missing!");
        for (int s=2; s<=8; s++)
        {
            if(PcmSegments[s].Ver == "")
                throw new Exception("Error: " + PcmSegments[s].Name + " version code missing!");
        }
        if (buf[0x20000] != 0x4E || buf[0x20001] != 0x56)
            throw new Exception("Error: OS segment 2 is not valid!");
    }
    public static string ValidateVIN(string VINcode)
    {
        VINcode = Regex.Replace(VINcode, "[^a-zA-Z0-9]", "");
        return VINcode.ToUpper();
    }
    public static uint BEToUint32(byte[] buf, uint offset)
    {
        byte[] tmp = new byte[4];
        Array.Copy(buf, offset, tmp, 0, 4);
        Array.Reverse(tmp);
        return (BitConverter.ToUInt32(tmp, 0));
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