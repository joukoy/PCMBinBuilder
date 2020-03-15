using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using static PcmFunctions;

namespace PCMBinBuilder
{
    public partial class FrmFileinfo : Form
    {
        public FrmFileinfo()
        {
            InitializeComponent();
        }

        private void FrmFileinfo_Load(object sender, EventArgs e)
        {

        }
        public void ShowSegmentInfo(string FileName)
        {
            try { 
            textBox1.AppendText(Path.GetFileName(FileName) + Environment.NewLine + Environment.NewLine);
            long fsize = new FileInfo(FileName).Length;
            byte[] buf = ReadBin(FileName, 0, (uint)fsize);
            string calculated = CalculateChecksum(0, (uint)fsize, buf).ToString("X4");
            Crc16 C = new Crc16();
            string CVN = SwapBytes(C.ComputeChecksum(buf)).ToString("X4");
            string FromBin = buf[0].ToString("X2") + buf[1].ToString("X2");
            string SegmentNr = buf[2].ToString() + buf[3].ToString();
            string PN = BEToUint32(buf, 4).ToString();
            string Ver = System.Text.Encoding.ASCII.GetString(buf, 8, 2);
            textBox1.AppendText("Calculated checksum: " + calculated + Environment.NewLine);
            textBox1.AppendText("Checksum from bin: " + FromBin + Environment.NewLine);
            textBox1.AppendText("Segment number: " + SegmentNr + Environment.NewLine);
            textBox1.AppendText("PN: " + PN + Environment.NewLine);
            textBox1.AppendText("Ver: " + Ver + Environment.NewLine);
            textBox1.AppendText("CVN: " + CVN + Environment.NewLine + Environment.NewLine);
            }
            catch(Exception ex)
            {
                textBox1.AppendText(ex.Message);
            }
        }

        private void ShowSingleFileInfo(string FileName)
        {
            long fsize = new FileInfo(FileName).Length;
            if (fsize < (512 * 1024))
            {
                //Assume it is segment file (?)
                ShowSegmentInfo(FileName);
                return;
            }
            PCMData PCM = new PCMData();
            GetPcmType(FileName, ref PCM);
            if (PCM.Type == "Unknown")
            {
                textBox1.AppendText("Unknown file"+Environment.NewLine);
                return;
            }
            textBox1.AppendText( Path.GetFileName(FileName) + Environment.NewLine + Environment.NewLine);
            textBox1.AppendText(PcmFileInfo(FileName, PCM));

        }
        public void ShowFileInfo(string FileName, bool Multi)
        {
            try
            {
                textBox1.Text = "";
                if (!Multi)
                {
                    ShowSingleFileInfo(FileName);
                }
                else
                {
                    string Fldr = Path.GetDirectoryName(FileName);
                    DirectoryInfo d = new DirectoryInfo(Fldr);
                    FileInfo[] Files = d.GetFiles("*.bin");
                    foreach (FileInfo file in Files)
                    {
                        ShowSingleFileInfo(file.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                textBox1.AppendText("Error: " + ex.Message);
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            try { 
                string FileName = SelectSaveFile("Text files (*.txt)|*.txt|All files (*.*)|*.*");
                if (FileName.Length>1)
                {
                    StreamWriter sw = new StreamWriter(FileName);
                    sw.WriteLine(textBox1.Text);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error");
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
