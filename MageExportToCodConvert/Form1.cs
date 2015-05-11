using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MageExportToCodConvert
{
    public partial class Form1 : Form
    {
        
        private string strError;

        public Form1()
        {
            InitializeComponent();
        }



        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void ChooseFile(TextBox tb)
        {

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Text Files (.csv)|*.csv|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;

            //openFileDialog1.InitialDirectory = string.IsNullOrEmpty(tb.Text) ? "" : tb.Text;
            openFileDialog1.FileName = string.IsNullOrEmpty(tb.Text) ? "" : tb.Text;


            // Display the openFile dialog.
            DialogResult result = openFileDialog1.ShowDialog();

            // OK button was pressed. 
            if (result == DialogResult.OK)
            {
                tb.Text = openFileDialog1.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChooseFile(textBox1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ProcessFile(textBox1.Text, Path.Combine(Path.GetDirectoryName(textBox1.Text), textBox2.Text)))
            { 
                MessageBox.Show("File was successfully converted and saved.", "Corset Story Conversion Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(String.Format("File was not converted. Error Message: {0}", strError), "Corset Story Conversion Tool", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private bool ProcessFile(string strInputFilename, string strOutputFilename)
        {
            String strOutputData;
            strOutputData = "CURRENCY,COMPANYNAME,ADDRESS1,ADDRESS2,TOWN,POSTCODE,ADDRESS3,COUNTRY,ATTENTION,TELEPHONE,EMAIL,REFERENCE1,WEIGHT\r\n";
            try
            {
                if (FileOperations.CheckFileExists(strInputFilename))
                {
                    string strInputData = FileOperations.ReadFile(strInputFilename);
                    string[] strInputLines = Regex.Split(strInputData, "\n");
                    bool firstline = true;
                    foreach (string strInputLine in strInputLines)
                    {
                        if ((!firstline) && (!String.IsNullOrWhiteSpace(strInputLine)))
                        {
                            string strPattern = @"""\s*,\s*""";
                            string strThisLine = strInputLine.Trim();
                            string[] strInputItem = Regex.Split(strThisLine.Substring(1, strThisLine.Length -2), strPattern);

                    
                            strOutputData += "US Dollar,"
                                + strInputItem[20] + ","
                                + strInputItem[22] + ","
                                + ","
                                + strInputItem[24] + ","
                                + strInputItem[23] + ","
                                + ","
                                + strInputItem[27] + ","
                                + strInputItem[20] + ","
                                + strInputItem[39] + ","
                                + strInputItem[19] + ","
                                + strInputItem[0] + ","
                                + strInputItem[17] + "\r\n";
                        }
                        firstline = false;
                    }
                    FileOperations.WriteFile(strOutputFilename, strOutputData);

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                strError = e.Message;
                return false;
            }
        }

      private string Trim(string str)
        {
          string mystr = str.Trim();
          return mystr.Substring(1, mystr.Length - 2);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            textBox2.Text = "COD-" + DateTime.Now.ToString("ddMMyy") + ".csv";
        }
    }
}
