using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PCMBinBuilder
{
    public partial class frmAction : Form
    {
        public frmAction()
        {
            InitializeComponent();
        }
        public  uint CalculateChecksumOS(byte[] Data)
        {
            uint sum = 0;
            byte high;
            byte low;

            //OS Segment 1
            for (uint i = globals.PcmSegments[1].Start + 2; i < globals.PcmSegments[1].End; i += 2)
            {
                high = Data[i];
                low = Data[i + 1];
                sum = (uint)(sum + ((high << 8) | low));
            }
            //OS Segment 2
            for (uint i = globals.PcmSegments[0].Start + 2; i < globals.PcmSegments[0].End; i += 2)
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

            for (uint i = StartAddr; i < EndAddr; i += 2)
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

            Logger("Calculating OS checksum", false);
            Calculated = CalculateChecksumOS(buf);
            FromFile = (uint)((buf[0x500] << 8) | buf[0x501]);
            if (Calculated != FromFile)
            {
                buf[0x500] = (byte)((Calculated & 0xFF00) >> 8);
                buf[0x501] = (byte)(Calculated & 0xFF);
            }
            Logger("(OK)");

            Logger("Calculating Segment checksums", false);
            for (int s = 2; s <= 8; s++)
            {
                uint StartAddr = globals.PcmSegments[s].Start + 2;
                uint EndAddr = globals.PcmSegments[s].End;
                Calculated = CalculateChecksum(StartAddr, EndAddr, buf);
                FromFile = (uint)((buf[StartAddr] << 8) | buf[StartAddr + 1]);
                if (Calculated != FromFile)
                {
                    buf[StartAddr] = (byte)((Calculated & 0xFF00) >> 8);
                    buf[StartAddr + 1] = (byte)(Calculated & 0xFF);
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
                if (globals.PcmSegments[s].GetFrom == "cal")
                {
                    string SegFile = 
                    globals.ReadSegment(OSfile + ".ossegment2", 0, globals.PcmSegments[0].End, globals.PcmSegments[0].Start, ref buf);
                }
            }
        }
        private  void SetVinCode(ref byte[] buf)
        {

        }

        private  void AddPatches(ref byte[] buf)
        {

        }

        private void FillEmptyArea(ref byte[] buf)
        {

        }

        private static void LoadOS(ref byte[] buf)
        {
            if (globals.PcmSegments[1].GetFrom == "file") //Get full binary file as OS
            {
                logger("Loading OS segment 1");
                buf = globals.ReadBinFile(globals.PcmSegments[1].SourceFile);
            }
            else
            {
                logger("Loading OS segment 2");
                globals.ReadSegment(globals.PcmSegments[1].SourceFile, 0, globals.PcmSegments[1].End, 0, ref buf);
                //OS Segment 2 file = OS segment 1 file, only last digit differ
                globals.ReadSegment(globals.PcmSegments[1].SourceFile.Replace(".ossegment1",".ossegment2"),0 , globals.PcmSegments[0].End, globals.PcmSegments[0].Start, ref buf);
            }
            logger("(OK)");
        }

        public byte[] BuildBintoMem()
        {
            byte[] buf = new byte[globals.BinSize];

            LoadOS(ref buf);
            LoadCalSegments(ref buf);
            SetVinCode(ref buf);
            AddPatches(ref buf);
            FillEmptyArea(ref buf);
            return (buf);

        }

        public void SaveBintoFile(byte[] buf)
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

        public void CreateBinary()
        {
            try
            {
                byte[] buf = BuildBintoMem();
                SaveBintoFile(buf);
            }
            catch (Exception e)
            {
                Logger(e.Message);
            }
            finally
            {
                Logger("Done");
            }
        }
        private void Logger(string LogText, Boolean newLine = true)
        {
            txtStatus.AppendText(LogText);
            if (newLine)
                txtStatus.AppendText(Environment.NewLine);
        }
        public void ExtractSegments()
        {
            string Fname = globals.SelectFile();
            if (Fname.Length < 1)
            {
                Logger("No file selected");
                return;

            }

            globals.GetPcmType(Fname);
            if (globals.PcmType == "Unknown")
            {
                Logger("Unknown file");
                return;
            }

            FrmAsk testDialog = new FrmAsk();
            string Descr;

            testDialog.TextBox1.Text = Path.GetFileName(Fname).Replace(".bin", "");
            testDialog.AcceptButton = testDialog.btnOK;

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if (testDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Read the contents of testDialog's TextBox.
                Descr = testDialog.TextBox1.Text;
            }
            else
            {
                Descr = "";
            }
            testDialog.Dispose();
            try
            {

                Logger("Reading segment addresses from file: " + Fname,false);
                globals.GetSegmentAddresses(Fname);
                Logger("(OK)");

                byte[] buf = globals.ReadBinFile(Fname);
                string OsFile = globals.PcmType + "-" + globals.GetOSid() + "-" + globals.GetOSVer();
                OsFile = Path.Combine(Application.StartupPath, "OS", OsFile);

                Logger("Writing OS segments",false);
                globals.WriteSegmentToFile(OsFile + ".ossegment1", globals.PcmSegments[1].Start, globals.PcmSegments[1].End, buf);
                globals.WriteSegmentToFile(OsFile + ".ossegment2", globals.PcmSegments[0].Start, globals.PcmSegments[0].End, buf);
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
                    globals.WriteSegmentToFile(SegFname, globals.PcmSegments[s].Start, globals.PcmSegments[s].End, buf);
                    sw = new StreamWriter(SegFname + ".txt");
                    sw.WriteLine(Descr);
                    sw.Close();
                }
                Logger("(OK)");

            }
            catch (Exception e) { 
                Logger(e.Message);
            }
            finally
            {
                Logger("Extract done");

            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
