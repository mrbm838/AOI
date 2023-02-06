using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolTotal;

namespace MyTools
{
    public delegate void Show();
    public partial class ChangePassword : Form
    {
        IniFile myIniFile;
        public string pass1, pass2, pass3;

        public ChangePassword()
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


        private void button1_Click(object sender, EventArgs e)
        {

            if (comboBox1.Text == "操作员")
            {
                if (textBox1.Text == pass1)
                {
                    myIniFile.IniWriteValue("Startup", "Pass1", textBox2.Text);//写配置文件
                    this.Close();
                }
                else
                {
                    MessageBox.Show("密码错误");
                }
            }
            else if (comboBox1.Text == "技术员")
            {
                if (textBox1.Text == pass2)
                {
                    myIniFile.IniWriteValue("Startup", "Pass2", textBox2.Text);//写配置文件
                    this.Close();
                }
                else
                {
                    MessageBox.Show("密码错误");
                }
            }
            else if (comboBox1.Text == "工程师")
            {
                if (textBox1.Text == pass3)
                {
                    myIniFile.IniWriteValue("Startup", "Pass3", textBox2.Text);//写配置文件
                    this.Close();
                }
                else
                {
                    MessageBox.Show("密码错误");
                }
            }
            else
            {
                MessageBox.Show("请选择用户名");
            }

        }

        private void logon_Load(object sender, EventArgs e)
        {
            myIniFile = new IniFile(Application.StartupPath + "\\Configuration.ini");
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
