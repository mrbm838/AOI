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
    public partial class SettingForm : Form
    {
        IniFile myIniFile;
        public string S, C, I, N, P, T, O, timeS, timeE;
        public string[] F = new string[2];
        public string[] M = new string[4];
        public string FF, MM;
        public SettingForm()
        {
            InitializeComponent();
        }
        private void SettingForm_Load(object sender, EventArgs e)
        {
            myIniFile = new IniFile(Application.StartupPath + "\\Configuration.ini");//初始化配置文件位置

            ReadUploadMES();//读MES格式参数
            ReadSettings();//读设置参数
            ReadFunction();//读功能设置参数
            CompensationValue();//读补偿值
            timer1.Start();
        }

        private void SettingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            WriteFunction();
            ReadFunction();
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (myIniFile.IniReadValue("Startup", "Statue") == "3")
            {
                groupBox2.Visible = true;
            }
            else
                groupBox2.Visible = false;
        }

        public void CompensationValue()
        {
            textBoxFAI6_270.Text = myIniFile.IniReadValue("Camera_OMM", "FAI6_270");
        }
        public void ReadUploadMES()
        {
            textBox1.Text = myIniFile.IniReadValue("UploadMES", "S");
            textBox2.Text = myIniFile.IniReadValue("UploadMES", "C");
            textBox3.Text = myIniFile.IniReadValue("UploadMES", "I");
            textBox4.Text = myIniFile.IniReadValue("UploadMES", "N");
            textBox5.Text = myIniFile.IniReadValue("UploadMES", "P");
            textBox6.Text = myIniFile.IniReadValue("UploadMES", "O");
        }
        private Control findControl(Control control, string controlname)
        {
            Control c;
            foreach (Control c1 in control.Controls)
            {
                if (c1.Name == controlname)
                    return c1;
                else if (c1.Controls.Count > 0)
                {
                    c = findControl(c1, controlname);
                    if (c != null)
                        return c;
                }
            }
            return null;
        }
        public void ReadSettings()
        {
            string str = "";
            for (int i = 0; i < 12; i++)
            {
                str = "textBoxTOP" + i.ToString();
                if (this.findControl(groupBox3, str) != null)
                {
                    TextBox tb = (TextBox)this.findControl(groupBox3, str);
                    str = "TopSetting" + i.ToString();
                    tb.Text = myIniFile.IniReadValue("TestSettings", str);
                    Define.TopSettingS[i] = Convert.ToDouble(tb.Text);
                }
            }

            for (int i = 0; i < 12; i++)
            {
                str = "textBoxSide" + i.ToString();
                if (this.findControl(groupBox3, str) != null)
                {
                    TextBox tb = (TextBox)this.findControl(groupBox3, str);
                    str = "SideSetting" + i.ToString();
                    tb.Text = myIniFile.IniReadValue("TestSettings", str);
                    Define.SideSettingS[i] = Convert.ToDouble(tb.Text);
                }
            }

            for (int i = 0; i < 14; i++)
            {
                str = "F_Offset" + i.ToString();
                if (this.findControl(groupBox3, str) != null)
                {
                    TextBox tb = (TextBox)this.findControl(groupBox3, str);
                    str = "FrontSetting" + i.ToString();
                    tb.Text = myIniFile.IniReadValue("TestSettings", str);
                    Define.FrontSettingS[i] = Convert.ToDouble(tb.Text);
                }
            }
        }

        public void WriteSettings()
        {
            string str = "";

            for (int i = 0; i < 12; i++)
            {
                str = "textBoxTOP" + i.ToString();
                if (this.findControl(groupBox3, str) != null)
                {
                    TextBox tb = (TextBox)this.findControl(groupBox3, str);
                    str = "TopSetting" + i.ToString();
                    myIniFile.IniWriteValue("TestSettings", str, Convert.ToDouble(tb.Text).ToString("0.000"));
                }
            }

            for (int i = 0; i < 12; i++)
            {
                str = "textBoxSide" + i.ToString();
                if (this.findControl(groupBox3, str) != null)
                {
                    TextBox tb = (TextBox)this.findControl(groupBox3, str);
                    str = "SideSetting" + i.ToString();
                    myIniFile.IniWriteValue("TestSettings", str, Convert.ToDouble(tb.Text).ToString("0.000"));
                }
            }

            for (int i = 0; i < 14; i++)
            {
                str = "F_Offset" + i.ToString();
                if (this.findControl(groupBox3, str) != null)
                {
                    TextBox tb = (TextBox)this.findControl(groupBox3, str);
                    str = "FrontSetting" + i.ToString();
                    myIniFile.IniWriteValue("TestSettings", str, Convert.ToDouble(tb.Text).ToString("0.000"));
                }
            }
        }

        public void ReadFunction()
        {
            if (Convert.ToDouble(myIniFile.IniReadValue("功能", "上传MES").ToString()) == 0)
            {
                Define.Function_UploadMES = false;
                checkBoxF1.Checked = false;
            }
            else if (Convert.ToDouble(myIniFile.IniReadValue("功能", "上传MES").ToString()) == 1)
            {
                Define.Function_UploadMES = true;
                checkBoxF1.Checked = true;
            }
            else
            {
                MessageBox.Show("“上传MES”功能保存错误");
            }


            if (Convert.ToDouble(myIniFile.IniReadValue("功能", "SN3次限制").ToString()) == 0)
            {
                Define.Function_SN3Times = false;
                checkBoxF2.Checked = false;
            }
            else if (Convert.ToDouble(myIniFile.IniReadValue("功能", "SN3次限制").ToString()) == 1)
            {
                Define.Function_SN3Times = true;
                checkBoxF2.Checked = true;
            }
            else
            {
                MessageBox.Show("“SN3次限制”功能保存错误");
            }

            if (Convert.ToDouble(myIniFile.IniReadValue("功能", "保存原图").ToString()) == 0)
            {
                Define.Function_SaveOriginalPhoto = false;
                checkBoxF4.Checked = false;
            }
            else if (Convert.ToDouble(myIniFile.IniReadValue("功能", "保存原图").ToString()) == 1)
            {
                Define.Function_SaveOriginalPhoto = true;
                checkBoxF4.Checked = true;
            }
            else
            {
                MessageBox.Show("“保存原图”功能保存错误");
            }
            if (Convert.ToDouble(myIniFile.IniReadValue("功能", "截图保存天数").ToString()) >= 0)
            {
                Define.Function_SaveScreenshotDays = Convert.ToDouble(myIniFile.IniReadValue("功能", "原图保存天数").ToString());

            }
            else
            {
                MessageBox.Show("“截图保存天数”功能保存错误");
            }

        }

        public void WriteFunction()
        {
            if (checkBoxF1.Checked == false)
            {
                myIniFile.IniWriteValue("功能", "上传MES", "0");
            }
            else
            {
                myIniFile.IniWriteValue("功能", "上传MES", "1");
            }

            if (checkBoxF2.Checked == false)
            {
                myIniFile.IniWriteValue("功能", "SN3次限制", "0");
            }
            else
            {
                myIniFile.IniWriteValue("功能", "SN3次限制", "1");
            }

            if (checkBoxF4.Checked == false)
            {
                myIniFile.IniWriteValue("功能", "保存原图", "0");
            }
            else
            {
                myIniFile.IniWriteValue("功能", "保存原图", "1");
            }

        }
        private void SaveUpLoad()
        {
            myIniFile.IniWriteValue("UploadMES", "S", textBox1.Text);
            myIniFile.IniWriteValue("UploadMES", "C", textBox2.Text);
            myIniFile.IniWriteValue("UploadMES", "I", textBox3.Text);
            myIniFile.IniWriteValue("UploadMES", "N", textBox4.Text);
            myIniFile.IniWriteValue("UploadMES", "P", textBox5.Text);
            myIniFile.IniWriteValue("UploadMES", "O", textBox6.Text);
            myIniFile.IniWriteValue("Startup", "SNNum", textBox8.Text);

            myIniFile.IniWriteValue("Camera_OMM", "FAI6_270", textBoxFAI6_270.Text);
            textBoxFAI6_270.Text = myIniFile.IniReadValue("Camera_OMM", "FAI6_270");

        }
        private void button2_Click(object sender, EventArgs e)//保存设定值
        {
            Define.LimintChange = false;
            try
            {
                WriteSettings();//写入参数
                ReadSettings();//读取参数
                SaveUpLoad();
                MessageBox.Show("数据保存成功");
            }
            catch
            {
                MessageBox.Show("输入的参数有误，请重新输入");
            }
        }
    }
}
