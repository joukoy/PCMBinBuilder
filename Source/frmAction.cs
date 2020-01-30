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

        private void frmAction_Load(object sender, EventArgs e)
        {
        
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
