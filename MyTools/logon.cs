using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolTotal;

namespace MyTools
{

    public partial class logon : Form
    {
        IniFile myIniFile;
        public string pass1, pass2, pass3;
        //MainForm mainform = new MainForm();
        public logon()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false; //不检查线程安全
            //  this.show1 = show;
            InitializeComponent();
            myIniFile = new IniFile(Application.StartupPath + "\\Configuration.ini");//初始化配置文件位置
            pass1 = myIniFile.IniReadValue("Startup", "Pass1");//读配置文件
            pass2 = myIniFile.IniReadValue("Startup", "Pass2");//读配置文件
            pass3 = myIniFile.IniReadValue("Startup", "Pass3");//读配置文件
            comboBox1.Text = "操作员";
         
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pass1 = myIniFile.IniReadValue("Startup", "Pass1");//读配置文件
            pass2 = myIniFile.IniReadValue("Startup", "Pass2");//读配置文件
            pass3 = myIniFile.IniReadValue("Startup", "Pass3");//读配置文件
            if (comboBox1.Text == "操作员")
            {
                if (textBox1.Text == pass1)
                {
                    myIniFile.IniWriteValue("Startup", "Statue", "1");//写配置文件
                    this.Close();
                }
                else
                {
                    textBox1.Clear();
                    MessageBox.Show("密码错误");
                }
            }
            else if (comboBox1.Text == "技术员")
            {
                if (textBox1.Text == pass2)
                {
                    //if (myIniFile.IniReadValue("Startup", "Statue") == "0")
                    //    mainform.Show();
                    log.SaveSN("登录记录","自动版-技术员\t"+DateTime.Now.ToString()+"\r\n");
                    myIniFile.IniWriteValue("Startup", "Statue", "2");//写配置文件
                    this.Close();

                }
                else
                {
                    textBox1.Clear();
                    MessageBox.Show("密码错误");
                }
            }
            else if (comboBox1.Text == "工程师")
            {
                if (textBox1.Text == pass3)
                {
                    log.SaveSN("登录记录", "自动版-工程师\t" + DateTime.Now.ToString() + "\r\n");
                    myIniFile.IniWriteValue("Startup", "Statue", "3");//写配置文件
                    this.Close();
                }
                else
                {
                    textBox1.Clear();
                    MessageBox.Show("密码错误");
                }
            }
            else
            {
                MessageBox.Show("请选择用户名");
            }
        }

        Log log = new Log();
        private void CancelBtn_Click(object sender, EventArgs e)
        {
                Close();
        }

        private void logon_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        private void logon_Load(object sender, EventArgs e)
        {
            myIniFile = new IniFile(Application.StartupPath + "\\Configuration.ini");
            textBox1.Focus();
        }
    }
}
