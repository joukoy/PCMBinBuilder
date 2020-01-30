using System.Collections.Generic;
using System;
using System.IO;
using System.Windows.Forms;
internal class globals
{
    public static List<uint> patchAddr;
    public static List<uint> patchData;

    public static void InitializeMe()
    {
        PcmSegments[0].Name = "OS2";
        PcmSegments[1].Name = "OS";
        PcmSegments[0].PN = 0;
        PcmSegments[2].Name = "EngineCAL";
        PcmSegments[3].Name = "EngineDiag";
        PcmSegments[4].Name = "TransCal";
        PcmSegments[5].Name = "TransDiag";
        PcmSegments[6].Name = "Fuel";
        PcmSegments[7].Name = "System";
        PcmSegments[8].Name = "Speedo";
        PcmSegments[9].Name = "EEprom-data";

        if (!File.Exists(Path.Combine(Application.StartupPath, "OS")))
            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "OS"));
        if (!File.Exists(Path.Combine(Application.StartupPath, "Calibration")))
            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Calibration"));
        if (!File.Exists(Path.Combine(Application.StartupPath, "Patches")))
            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Patches"));

        PatchList = new List<string>();
        NewVIN = "";
        VIN = "";

    }

    public static string SelectFile(string Title= "Select bin file")
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

    public struct PcmSegment
    {
        public string Name;
        public uint PN;
        public string Ver;
        //public ushort CheckSum;
        public uint Start;
        public uint End;
        //public byte[] Data;
        public string GetFrom;
        public string Source;
    }

    public const int MaxSeg= 10;
    public static PcmSegment[] PcmSegments = new PcmSegment[MaxSeg];
    public static string PcmType="";
    public static uint BinSize = 0;
    public static string VIN="";
    public static string NewVIN="";
    public static List<string> PatchList;
    //public static byte[] BinBuffer;

    public static string GetOSid()
    {
        return PcmSegments[1].PN.ToString();
    }

    public static string GetOSVer()
    {
        return PcmSegments[1].Ver;
    }

    public static string GetModifications()
    {
        string Finfo = "";
        for (int i = 1; i <= 9; i++)
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
            foreach (string P in PatchList)
                Finfo += P + ", ";
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
        return Finfo;
    }

    public static void  GetPcmType(string FileName)
    {
        PcmType = "Unknown";
        long fsize = new System.IO.FileInfo(FileName).Length;
        if (fsize >= (1024 * 1024) || (FileName.Contains(".ossegment") && FileName.Contains("P59-")))
        { 
            PcmType = "P59";
            BinSize = 1024 * 1024;
        }
        else if(fsize == (512 * 1024) || (FileName.Contains(".ossegment") && FileName.Contains("P01-"))) { 
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
                PcmSegments[1].End = 0x4000;
                PcmSegments[0].Start = 0x020000;
                if (PcmType == "P01")
                    PcmSegments[0].End = 0x07FFFD;
                else
                    PcmSegments[0].End = 0x0FFFFD;
                //EEprom Data:
                PcmSegments[9].Start = 0x4000;
                PcmSegments[9].End = 0x8000;

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
                    PcmSegments[i].End = reader.ReadUInt32BE();
                }

                reader.BaseStream.Close();
            }

    }

    public static uint CalculateChecksumOS(byte[] Data)
    {
        uint sum = 0;
        byte high;
        byte low;

        //OS Segment 1
        for (uint i = PcmSegments[1].Start+2; i < globals.PcmSegments[1].End; i += 2)
        {
            high = Data[i];
            low = Data[i + 1];
            sum = (uint)(sum + ((high << 8) | low));
        }
        //OS Segment 2
        for (uint i = PcmSegments[0].Start + 2; i < globals.PcmSegments[0].End; i += 2)
        {
            high = Data[i];
            low = Data[i + 1];
            sum = (uint)(sum + ((high << 8) | low));
        }

        sum = (sum & 0xFFFF);
        return (65536 - sum) & 0xFFFF;
    }

    public static uint CalculateChecksum(uint StartAddr, uint EndAddr, byte[] Data)
    {
        uint sum = 0;
        byte high;
        byte low;

        for (uint i = StartAddr; i < EndAddr; i += 2)
        {
            high = Data[i];
            low = Data[i + 1];
            sum = (uint)(sum + ((high << 8) | low));
        }
        sum = (sum & 0xFFFF);
        return (65536 - sum) & 0xFFFF;
    }

    public static void FixSchekSums(ref byte[] buf)
    {
        uint Calculated = 0;
        uint FromFile = 0;

        //OS:
        Calculated = CalculateChecksumOS(buf);
        FromFile = (uint)((buf[0x500] << 8) | buf[0x501]);
        if (Calculated != FromFile)
        {
            buf[0x500] = (byte)((Calculated & 0xFF00) >> 8);
            buf[0x501] = (byte)(Calculated & 0xFF);
        }

        for (int s=2;s<=8;s++) {
            uint StartAddr = globals.PcmSegments[s].Start + 2;
            uint EndAddr = globals.PcmSegments[s].End;
            Calculated = CalculateChecksum(StartAddr, EndAddr, buf);
            FromFile = (uint)((buf[StartAddr] << 8) | buf[StartAddr+1]);
            if (Calculated != FromFile)
            {
                buf[StartAddr] = (byte)((Calculated & 0xFF00) >> 8);
                buf[StartAddr + 1] = (byte)(Calculated & 0xFF);
            }
        }
    }

    public static string ReadVIN(string Fname)
    {
        byte[] Buf = new byte[21];
        uint VinAddr=0;
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
                VinAddr = 0x4021;
            else if(Buf[2] == 0xA5 && Buf[3] == 0xA0)
                    VinAddr = 0x6021;            
            if (VinAddr >0)
            {
                reader.BaseStream.Seek(VinAddr,0);
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
        uint HWAddr = 0;

        using (BinaryReader reader = new BinaryReader(File.Open(Fname, FileMode.Open)))
        {
            reader.BaseStream.Seek(0x4088, 0);
            Buf[0] = reader.ReadByte();
            Buf[1] = reader.ReadByte();
            reader.BaseStream.Seek(0x6088, 0);
            Buf[2] = reader.ReadByte();
            Buf[3] = reader.ReadByte();
            if (Buf[0] == 0xA5 && Buf[1] == 0xA0)
                HWAddr = 0x4004;
            else if (Buf[2] == 0xA5 && Buf[3] == 0xA0)
                HWAddr = 0x6004;
            if (HWAddr > 0)
            {
                reader.BaseStream.Seek(HWAddr, 0);
                PcmSegments[9].PN = reader.ReadUInt32BE();
                reader.BaseStream.Seek(HWAddr+24, 0);
                int read = reader.Read(Buf, 0, 4);
                PcmSegments[9].Ver = System.Text.Encoding.ASCII.GetString(Buf);
            }
            reader.BaseStream.Close();
        }
    }

    public static byte[] ReadBinFile(string FileName)
    {
        long fsize = new System.IO.FileInfo(FileName).Length;
        byte[] buf = new byte[fsize];

        long offset = 0;
        long remaining = fsize;

        using (BinaryReader freader = new BinaryReader(File.Open(FileName, FileMode.Open)))
        {
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
    public static void ReadSegment(string FileName, uint StartAddressFile, uint EndAddress, uint StartAddressBuf, ref byte[] Buffer)
    {

        long offset = StartAddressBuf;
        long remaining = (EndAddress - StartAddressBuf);

        using (BinaryReader freader = new BinaryReader(File.Open(FileName, FileMode.Open)))
        {
            freader.BaseStream.Seek(StartAddressFile, 0);
            while (remaining > 0)
            {
                int read = freader.Read(Buffer, (int)offset, (int)remaining);
                if (read <= 0)
                    throw new EndOfStreamException
                        (String.Format("End of stream reached with {0} bytes left to read", remaining));
                remaining -= read;
                offset += read;
            }
            freader.Close();
        }

    }
    public static void WriteSegmentToFile(string Fname, uint StartAddr, uint EndAddr, byte[] Buf)
    {
        using (FileStream stream = new FileStream(Fname, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Buf, (int)StartAddr, (int)(EndAddr - StartAddr));
                writer.Close();
            }
        }

    }

    private static void LoadOS(ref byte[] buf)
    {
        if (PcmSegments[1].GetFrom = "file") //Get full binary file as OS
        {
            buf = ReadBinFile(PcmSegments[1].Source);
        } else
        {
            string OSfile = Path.Combine(Application.StartupPath, "OS", PcmSegments[1].Source);
            ReadSegment(OSfile + ".ossegment1", 0, PcmSegments[1].End, 0, ref buf);
            ReadSegment(OSfile + ".ossegment2", PcmSegments[0].Start, PcmSegments[0].End, 0, ref buf);
        }
    }
    private static void LoadCalSegments(ref byte[] buf)
    {

    }
    private static void SetVinCode(ref byte[] buf)
    {

    }
    private static void AddPatches(ref byte[] buf)
    {

    }
    public static byte[] BuildBintoMem()
    {
        byte[] buf = new byte[BinSize];

        LoadOS(ref buf);
        LoadCalSegments(ref buf);
        SetVinCode(ref buf);
        AddPatches(ref buf);
        return (buf);
    }

    public static void SaveBintoFile(byte[] buf)
    {
        string FileName = SelectSaveFile();
        if (FileName.Length < 1)
            return;
        using (FileStream stream = new FileStream(FileName, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(buf);
                writer.Close();
            }
        }

    }

    public static void CleanSegmentInfo()
    {
        for (int s= 0; s < MaxSeg;s++)
        {
            PcmSegments[s].GetFrom = "";
            PcmSegments[s].Source = "";
        }
    }

    public static string ValidateVIN(string VINcode)
    {
        if (VINcode.Length != 17)
            return "";
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