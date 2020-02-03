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
    public static uint VINAddr = 0;
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
    public static string SelectFile(string Title = "Select bin file")
    {

        OpenFileDialog fdlg = new OpenFileDialog();
        fdlg.Title = Title;
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

    public static string GetChecksumStatus(string Fname)
    {
        uint Calculated = 0;
        uint FromFile = 0;
        string Result = "";

        byte[] buf = ReadBin(Fname, 0, BinSize);
        Calculated = globals.CalculateChecksumOS(buf);
        FromFile = (uint)((buf[0x500] << 8) | buf[0x501]);
        if (Calculated == FromFile)
        {
            Result = "OS:".PadRight(12)  + "Bin Checksum: " + FromFile.ToString("X2") + " * Calculated: " + Calculated.ToString("X2") + " [OK]";
            //Logger("OS checksum: " + buf[0x500].ToString("X1") + buf[0x501].ToString("X1") + " OK");
        }
        else
        {
            Result = "OS:".PadRight(12) + "Bin Checksum: " + FromFile.ToString("X2") + " * Calculated: " + Calculated.ToString("X2") + " [FAIL]";
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
                Result += globals.PcmSegments[s].Name.PadRight(12) + "Bin checksum: " + FromFile.ToString("X2") + " * Calculated: " + Calculated.ToString("X2") + " [OK]";
            }
            else
            {
                Result += globals.PcmSegments[s].Name.PadRight(12) + "Bin checksum: " + FromFile.ToString("X2") + " * Calculated: " + Calculated.ToString("X2") + " [FAIL]";
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
            if (globals.PcmSegments[i].Source != "")
            {
                Finfo += globals.PcmSegments[i].Name.PadRight(20) + globals.PcmSegments[i].Source + Environment.NewLine;
                //Finfo += " =>  " + globals.PcmSegments[i].Source + Environment.NewLine;
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

    public static string PcmFileInfo(string Fname)
    {
        GetPcmType(Fname);
        GetSegmentAddresses(Fname);
        GetSegmentInfo(Fname);
        string Finfo = "PCM Type: ".PadRight(20) + PcmType + Environment.NewLine;
        Finfo += "Segments:" + Environment.NewLine;
        for (int i = 1; i <= 9; i++)
        {
            Finfo += globals.PcmSegments[i].Name.PadRight(20) + globals.PcmSegments[i].PN.ToString() + " Version: " + globals.PcmSegments[i].Ver + Environment.NewLine;
        }
        Finfo += "VIN".PadRight(20) + globals.ReadVIN(Fname) + Environment.NewLine + Environment.NewLine;
        Finfo += GetChecksumStatus(Fname);
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

    public static void GetSegmentInfo(string FileName)
    {
        byte[] Buf = new byte[2];
        using (BinaryReader reader = new BinaryReader(File.Open(FileName, FileMode.Open)))
        {
            //Get all segments PN and Version informations. Not for OS
            for (int i = 2; i <= 8; i++)
            {
                reader.BaseStream.Seek(PcmSegments[i].Start + 4, 0);
                PcmSegments[i].PN = reader.ReadUInt32BE();
                Buf[0] = reader.ReadByte();
                Buf[1] = reader.ReadByte();
                PcmSegments[i].Ver = System.Text.Encoding.ASCII.GetString(Buf);
            }
            reader.BaseStream.Close();
        }
        GetEEpromInfo(FileName);

    }

    public static void GetSegmentAddresses(string FileName)
    {
            byte[] Buf = new byte[2];
            using (BinaryReader reader = new BinaryReader(File.Open(FileName, FileMode.Open)))
            {   

                //OS Segments:
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

                reader.BaseStream.Seek(0x504, 0);
                PcmSegments[1].PN = reader.ReadUInt32BE();
                Buf[0] = reader.ReadByte();
                Buf[1] = reader.ReadByte();
                PcmSegments[1].Ver = System.Text.Encoding.ASCII.GetString(Buf);

                //Read Segment Addresses from bin, starting at 0x514
                reader.BaseStream.Seek(0x514, 0);
                for (int i=2; i<=8;i++)
                {
                    PcmSegments[i].Start = reader.ReadUInt32BE();
                    PcmSegments[i].Length = reader.ReadUInt32BE() - PcmSegments[i].Start + 1;
                }

                reader.BaseStream.Close();
            }

    }


    public static string ReadVIN(string Fname)
    {
        byte[] Buf = new byte[17];
        //uint VinAddr=0;
        string VIN ="";

        using (BinaryReader reader = new BinaryReader(File.Open(Fname, FileMode.Open)))
        {
            reader.BaseStream.Seek(0x4088,0);
            Buf[0] = reader.ReadByte();
            Buf[1] = reader.ReadByte();
            reader.BaseStream.Seek(0x6088, 0);
            Buf[2] = reader.ReadByte();
            Buf[3] = reader.ReadByte();
            if (Buf[0] == 0xA5 && Buf[1] == 0xA0)
                VINAddr = 0x4000;
                //VinAddr = 0x4021;
            else if (Buf[2] == 0xA5 && Buf[3] == 0xA0)
                    VINAddr = 0x6000;
                    //VinAddr = 0x6021;
            if (VINAddr >0)
            {
                reader.BaseStream.Seek((VINAddr + 33),0);
                int read = reader.Read(Buf, 0, 17);
                VIN = System.Text.Encoding.ASCII.GetString(Buf);
            }
            reader.BaseStream.Close();
        }
        return VIN;
    }

    public static void GetEEpromInfo(string Fname)
    {
        byte[] Buf = new byte[4];

        using (BinaryReader reader = new BinaryReader(File.Open(Fname, FileMode.Open)))
        {
            reader.BaseStream.Seek(0x4088, 0);
            Buf[0] = reader.ReadByte();
            Buf[1] = reader.ReadByte();
            reader.BaseStream.Seek(0x6088, 0);
            Buf[2] = reader.ReadByte();
            Buf[3] = reader.ReadByte();
            if (Buf[0] == 0xA5 && Buf[1] == 0xA0)
                VINAddr = 0x4000;
            else if (Buf[2] == 0xA5 && Buf[3] == 0xA0)
                VINAddr = 0x6000;
            if (VINAddr > 0)
            {
                reader.BaseStream.Seek(VINAddr + 4, 0);
                PcmSegments[9].PN = reader.ReadUInt32BE();
                reader.BaseStream.Seek(VINAddr+28, 0);
                int read = reader.Read(Buf, 0, 4);
                PcmSegments[9].Ver = System.Text.Encoding.ASCII.GetString(Buf);
            }
            reader.BaseStream.Close();
        }
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


    public static void WriteSegmentToFile(string Fname, uint StartAddr, uint Length, byte[] Buf)
    {

        using (FileStream stream = new FileStream(Fname, FileMode.Create))
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
        for (int s= 0; s < MaxSeg;s++)
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

public static string ValidateVIN(string VINcode)
    {
        VINcode = Regex.Replace(VINcode, "[^a-zA-Z0-9]", "");
        return VINcode.ToUpper();
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