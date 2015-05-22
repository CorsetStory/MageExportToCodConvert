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
using MageExportToCodConvert.MagentoService;

namespace MageExportToCodConvert
{
    public partial class Form1 : Form
    {

        private string strError;
        Mage_Api_Model_Server_V2_HandlerPortTypeClient mservice;
        string mage_session;

        public Form1()
        {
            InitializeComponent();
            mservice = null;
            mage_session = null;
            textBox3.Text = Properties.Settings.Default.UserName;
            textBox4.Text = Properties.Settings.Default.APIKey;
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
            //notifyIcon1.BalloonTipTitle = "Corset Story";
            //notifyIcon1.BalloonTipText = "Processing file - please wait!!";
            //notifyIcon1.ShowBalloonTip(30000);
            //Application.DoEvents();
            button2.Enabled = false;
            label3.Visible = true;
            Application.DoEvents();
            if (ProcessFile(textBox1.Text, Path.Combine(Path.GetDirectoryName(textBox1.Text), textBox2.Text)))
            {
                MessageBox.Show("File was successfully converted and saved.", "Corset Story Conversion Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(String.Format("File was not converted. Error Message: {0}", strError), "Corset Story Conversion Tool", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            button2.Enabled = true;
            label3.Visible = false;

        }

        private bool ProcessFile(string strInputFilename, string strOutputFilename)
        {
            MageLogin();
            List<string> strOutputData = new List<string>();
            List<string> lstOrderNumbers = new List<string>();
            string strOutputItem;

            strOutputData.Add("CURRENCY,COMPANYNAME,ADDRESS1,ADDRESS2,TOWN,POSTCODE,ADDRESS3,COUNTRY,ATTENTION,TELEPHONE,EMAIL,REFERENCE1,WEIGHT");

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
                            //string strPattern = @"""\s*,\s*""";
                            string strThisLine = strInputLine.Trim();
                            string[] strInputItem = SplitString(strThisLine);
                            //string[] strInputItem = Regex.Split(strThisLine.Substring(1, strThisLine.Length -2), strPattern);
                           
                            if (!lstOrderNumbers.Contains(strInputItem[0]))
                            {
                                strOutputItem = "US Dollar,"
                                    + strInputItem[19] + ","
                                    + strInputItem[21] + ","
                                    + ","
                                    + strInputItem[23] + ","
                                    + strInputItem[22] + ","
                                    + strInputItem[25] + ","
                                    + strInputItem[27] + ","
                                    + strInputItem[19] + ","
                                    + strInputItem[28] + ","
                                    + strInputItem[18] + ","
                                    + strInputItem[0] + ","
                                    + GetWeight(strInputItem[0]);


                                strOutputData.Add(strOutputItem);
                                lstOrderNumbers.Add(strInputItem[0]);
                            }
                         
                        }
                        firstline = false;
                    }
                    string strFinalOutputData = string.Join("\r\n", strOutputData.ToArray());
                    FileOperations.WriteFile(strOutputFilename, strFinalOutputData, Encoding.UTF8);

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

        private string[] SplitString(string strToBeSplit)
        {
            // strip out '"' and strip out ',' from inside strings

            bool innerString = false;
            bool escaping = false;
            List<string> strWIP = new List<string>();
            String strCurrentItem = "";

            foreach (char c in strToBeSplit)
            {
                if (escaping)
                {
                    strCurrentItem += (@"\" + c);
                    escaping = false;
                }
                else
                {
                    switch (c)
                    {
                        case '"':
                            innerString = !innerString;
                            //strCurrentItem += c;
                            break;
                        case '\\':
                            escaping = true;  // if escaping already then don't not escaping now...
                            break;
                        case ',':
                            if (!innerString)
                            {
                                strWIP.Add(strCurrentItem);
                                strCurrentItem = "";
                            }
                            break;
                        default:
                            strCurrentItem += c;
                            break;
                    }
                }


            }
            return strWIP.ToArray();
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

        private void MageLogin()
        {
            try
            {
                mservice = new Mage_Api_Model_Server_V2_HandlerPortTypeClient();
                mage_session = mservice.login(textBox3.Text, textBox4.Text);
            }
            catch (Exception e)
            {
                MessageBox.Show("Can't login to Magento - default weight of 1KG will be used.  Error message: " + e.Message);
            }
        }
        
        private string GetWeight(string strOrderNo)
        {
            try
            {
                if (!string.IsNullOrEmpty(mage_session))
                {
                    salesOrderEntity soEntity = mservice.salesOrderInfo(mage_session, strOrderNo);
                    return soEntity.weight;
                }
            }
            catch { }

            return "1.00";
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.UserName = textBox3.Text;
            Properties.Settings.Default.APIKey = textBox4.Text;
            Properties.Settings.Default.Save();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MageLogin();
            if (!string.IsNullOrEmpty(mage_session))
            {
                   MessageBox.Show("Login successful!!");
            }
        }
    }
}
