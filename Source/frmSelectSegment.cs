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
    public partial class FrmSelectSegment : Form
    {
        public FrmSelectSegment()
        {
            InitializeComponent();           
        }

        public static int SegNr;
        private static Button btnCaller;
        public PCMData PCM1;

        private void FrmModifyBin_Load(object sender, EventArgs e)
        {

        }

        public void LoadOSFiles(Button btn, ref PCMData PCM)
        {            
            btnCaller = btn as Button;

            PCM1 = PCM;

            radioButton2.Checked = true;
            listView1.Enabled = true;
            listView1.Clear();
            listView1.View = View.Details;
            listView1.Columns.Add("OS");
            listView1.Columns.Add("Description");
            listView1.Columns[0].Width = 150;
            listView1.Columns[1].Width = 600;
            SegNr = 1;

            string OSFolder = Path.Combine(Application.StartupPath, "OS");
            DirectoryInfo d = new DirectoryInfo(OSFolder);
            FileInfo[] Files = d.GetFiles("*.ossegment1"); 
            foreach (FileInfo file in Files)
            {
                string OS = file.Name.Replace(".ossegment1", "");
                var item = new ListViewItem(OS);
                item.Tag = file.FullName;
                string DescrFile = file.FullName +  ".txt";
                if (File.Exists(DescrFile))
                {
                    StreamReader sr = new StreamReader(DescrFile);
                    string line = sr.ReadLine();
                    sr.Close();
                    item.SubItems.Add(line);
                }
                listView1.Items.Add(item);
            }

        }

        public void LoadCalibrations(Button btn, ref PCMData PCM)
        {
            //listView1.Enabled = true;
            btnCaller = btn as Button;
            listView1.Clear();
            listView1.View = View.Details;
            listView1.Columns.Add("Calibration");
            listView1.Columns.Add("Description");
            listView1.Columns[0].Width = 100;
            listView1.Columns[1].Width = 600;
            SegNr = (int)this.Tag;
            string SegmentName = SegmentNames[SegNr];

            PCM1 = PCM;

            string CalFolder = Path.Combine(Application.StartupPath, "Calibration");
            DirectoryInfo d = new DirectoryInfo(CalFolder);
            string FileFIlter;
            if (SegNr == 9) //Eeprom_data
                FileFIlter = PCM1.EepromType.ToString()+"-"+ SegmentName + "*.calsegment";
            else
                FileFIlter = PCM1.Segments[1].PN.ToString() + "-" + SegmentName + "*.calsegment";
            FileInfo[] Files = d.GetFiles(FileFIlter);
            foreach (FileInfo file in Files)
            {
                string CalName = file.Name.Replace(".calsegment", "");
                if (SegNr == 9)
                    CalName = CalName.Replace(PCM1.EepromType.ToString() + "-" + SegmentName + "-", "");
                else
                    CalName = CalName.Replace(PCM1.Segments[1].PN.ToString() + "-" + SegmentName + "-", "");
                var item = new ListViewItem(CalName);
                item.Tag = file.FullName;
                string DescrFile = file.FullName +  ".txt";
                if (File.Exists(DescrFile))
                {
                    StreamReader sr = new StreamReader(DescrFile);
                    string line = sr.ReadLine();
                    sr.Close();
                    item.SubItems.Add(line);
                }
                listView1.Items.Add(item);
            }
            radioButton2.Checked = true;

        }


        public void LoadPatches(PCMData PCM)
        {
            //listView1.Enabled = true;
            listView1.Clear();
            listView1.View = View.Details;
            listView1.Columns.Add("Patch");
            listView1.Columns[0].Width = 600;
            listView1.MultiSelect = true;
            listView1.CheckBoxes = true;

            radioButton2.Checked = true;
            radioButton3.Visible = false;
            txtCalFile.Visible = false;
            btnBrowse.Visible = false;

            PCM1 = PCM;
            string CalFolder = Path.Combine(Application.StartupPath, "Patches");
            DirectoryInfo d = new DirectoryInfo(CalFolder);
            
            FileInfo[] Files = d.GetFiles(PCM1.Segments[1].PN.ToString() + "*.pcmpatch");
            foreach (FileInfo file in Files)
            {
                string CalName = file.Name;
                CalName = CalName.Replace(PCM1.Segments[1].PN.ToString() + "-" , "");
                CalName = CalName.Replace(".pcmpatch", "");
                var item = new ListViewItem(CalName);
                item.Tag = file.FullName;
                foreach (Patch P in PCM1.PatchList)
                    if (CalName == P.Name)
                        item.Checked = true;
                listView1.Items.Add(item);
            }

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                listView1.Enabled = true;
            }
            else
            {
                listView1.Enabled = false;
            }

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                listView1.Enabled = false;
            }
            else
            {
                listView1.Enabled = true;
            }

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            radioButton3.Checked = true;
            string FileName = SelectFile("Load segment from file",true);
            if (FileName.Length > 1)
            {
                txtCalFile.Text = FileName;
            }

        }

        private void txtCalFile_TextChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                if (txtCalFile.TextLength > 0)
                    btnOK.Enabled = true;
                else
                    btnOK.Enabled = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            btnOK_Click(sender, e);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {

            if (labelSelectOS.Text=="Select patches")
            {
                PCM1.PatchList.Clear();
                for (int i = 0; i < listView1.CheckedItems.Count; i++)
                {
                    Patch P;
                    P.Name = listView1.CheckedItems[i].Text;
                    P.Description = listView1.CheckedItems[i].SubItems[0].Text;
                    P.FileName = listView1.CheckedItems[i].Tag.ToString();
                    PCM1.PatchList.Add(P);
                    
                }
                this.DialogResult = DialogResult.OK;

            } else {  //Select segment
                string SrcFile = "";
                if (radioButton2.Checked)
                {
                    if (listView1.SelectedItems.Count > 0)
                    {
                        SrcFile = listView1.SelectedItems[0].Tag.ToString();
                    }
                }
                else //radioButton3.Checked
                {
                    SrcFile = txtCalFile.Text;

                }
                //PCM1.Segments[SegNr].Source = SrcFile;
                frmAction frmA = new frmAction();
                frmA.Show();
                if (SegNr > 1) //CAL segments
                {
                    if (SegNr == 9) { 
                        if (!frmA.LoadEepromData(SrcFile, ref PCM1))
                            return;
                    }
                    else { 
                        if (!frmA.LoadCalSegment(SegNr, SrcFile, ref PCM1))
                            return;
                    }
                }
                else //OS 
                {
                    if (!frmA.LoadOS(SrcFile, ref PCM1))
                        return;
                }
                if (btnCaller != null) { 
                    if (btnCaller.Text == "Build new BIN") //FrmMain
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else //From frmSegmentList 
                    { 
                        btnCaller.Text = SegmentNames[SegNr] + ":  " + PCM1.Segments[SegNr].PN.ToString() + " " + PCM1.Segments[SegNr].Ver;
                        this.Close();
                    }
                }
                this.DialogResult = DialogResult.OK;
            }
        }

        

/*        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && radioButton2.Checked)
                btnOK.Enabled = true;
            else
                btnOK.Enabled = false;

        }
        private void listView1_Click(object sender, EventArgs e)
        {
            if (radioButton2.Checked) { 
                if (listView1.SelectedItems.Count > 0 || listView1.CheckedItems.Count > 0) 
                    btnOK.Enabled = true;
                else
                    btnOK.Enabled = false;
            }

        }
*/
    }
}
