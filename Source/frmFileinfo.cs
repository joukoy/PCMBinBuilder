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

        private void brtnSave_Click(object sender, EventArgs e)
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
