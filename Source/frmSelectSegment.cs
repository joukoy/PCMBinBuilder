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
    public partial class FrmSelectSegment : Form
    {
        public FrmSelectSegment()
        {
            InitializeComponent();           
        }

        public static int SegNr;
        private static Button btnCaller;

        private void FrmModifyBin_Load(object sender, EventArgs e)
        {

        }

        public void LoadOSFiles(Button btn)
        {
            
            btnCaller = btn as Button;

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

        public void LoadCalibrations(Button btn)
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
            string SegmentName = globals.PcmSegments[SegNr].Name;

            string CalFolder = Path.Combine(Application.StartupPath, "Calibration");
            DirectoryInfo d = new DirectoryInfo(CalFolder);
            string FileFIlter;
            if (SegNr == 9) //Eeprom_data
                FileFIlter = globals.PCM.EepromType.ToString()+"-"+ SegmentName + "*.calsegment";
            else
                FileFIlter = globals.GetOSid() + "-" + SegmentName + "*.calsegment";
            FileInfo[] Files = d.GetFiles(FileFIlter);
            foreach (FileInfo file in Files)
            {
                string CalName = file.Name.Replace(".calsegment", "");
                if (SegNr == 9)
                    CalName = CalName.Replace(globals.PCM.EepromType.ToString() + "-" + SegmentName + "-", "");
                else
                    CalName = CalName.Replace(globals.GetOSid() + "-" + SegmentName + "-", "");
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


        public void LoadPatches()
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
            string CalFolder = Path.Combine(Application.StartupPath, "Patches");
            DirectoryInfo d = new DirectoryInfo(CalFolder);
            
            FileInfo[] Files = d.GetFiles(globals.GetOSid() + "*.pcmpatch");
            foreach (FileInfo file in Files)
            {
                string CalName = file.Name;
                CalName = CalName.Replace(globals.GetOSid() +"-" , "");
                CalName = CalName.Replace(".pcmpatch", "");
                var item = new ListViewItem(CalName);
                item.Tag = file.FullName;
                foreach (globals.Patch P in globals.PatchList)
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
            string FileName = globals.SelectFile("Load segment from file",true);
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
                globals.PatchList.Clear();
                for (int i = 0; i < listView1.CheckedItems.Count; i++)
                {
                    globals.Patch P;
                    P.Name = listView1.CheckedItems[i].Text;
                    P.Description = listView1.CheckedItems[i].SubItems[0].Text;
                    P.FileName = listView1.CheckedItems[i].Tag.ToString();
                    globals.PatchList.Add(P);
                    
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
                globals.PcmSegments[SegNr].Source = SrcFile;
                frmAction frmA = new frmAction();
                frmA.Show(this);
                if (SegNr > 1) //CAL segments
                {
                    if (SegNr == 9) { 
                        if (!frmA.LoadEepromData(SrcFile))
                            return;
                    }
                    else { 
                        if (!frmA.LoadCalSegment(SegNr, SrcFile))
                            return;
                    }
                }
                else
                {
                    if (!frmA.LoadOS(SrcFile))
                        return;
                }
                if (btnCaller != null) { 
                    if (btnCaller.Text == "Build new BIN") //FrmMain
                    {
                        FrmSegmentList FrmB = new FrmSegmentList();
                        FrmB.Show();
                        FrmB.StartBuilding();
                        this.Close();
                    }
                    else //From frmSegmentList 
                    { 
                        btnCaller.Text =  globals.PcmSegments[SegNr].Name + ":  " + globals.PcmSegments[SegNr].PN.ToString() + " " + globals.PcmSegments[SegNr].Ver;
                        this.Close();
                    }
                }
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
