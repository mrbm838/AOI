using CSharp_OPTControllerAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolTotal;

namespace MyTools
{
    class LightInitialize
    {

        #region 漠然光源控制器
        public static void RGBParamInitailize(MainForm form)
        {
            Define.MRRed = Convert.ToInt32(form.myIniFile.IniReadValue("MR光源控制器", "Red"));
            Define.MRGreen = Convert.ToInt32(form.myIniFile.IniReadValue("MR光源控制器", "Green"));
            Define.MRBlue = Convert.ToInt32(form.myIniFile.IniReadValue("MR光源控制器", "Blue"));

        }
        public static void RGBConnect(MainForm form, NetClient myClient1, RichTextBox richTextBox)
        {
            try
            {
              
                myClient1.Open("192.168.5.10", 2000);
                //RGBLightOFF.Visible = false;
                richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "漠然光源控制器加载完成！" + "\r\n");
            }
            catch (Exception)
            {
                //RGBLightOFF.Visible = true;
                richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "漠然光源控制器加载失败！" + "\r\n");
                form.ShowMsg1("漠然光源控制器加载失败！");
            }
        }



        public static  void MROpenF( NetClient myClient1, RichTextBox richTextBox)
        {
            try
            {
                for (int i = 1; i <= 3; i++)
                {

                    //通道参数处理
                    string CH = "0" + i.ToString();
                    //亮度参数处理
                    string DA = "0";
                    if (i == 1)
                    {
                        DA = Define.MRRed.ToString();
                    }
                    else if (i == 2)
                    {
                        DA = Define.MRGreen.ToString();
                    }
                    else if (i == 3)
                    {
                        DA = Define.MRBlue.ToString();
                    }

                    if (DA.Length > 3 || Convert.ToDouble(DA.ToString()) > 255)
                    {
                        MessageBox.Show("亮度输入错误");
                        return;
                    }
                    DA = Convert.ToString(Convert.ToInt32(DA), 16).ToUpper(); ;
                    if (DA.Length == 1)
                    {
                        DA = "0" + DA;
                    }

                    //校验位处理
                    byte a = byte.Parse(CH, System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(DA, System.Globalization.NumberStyles.HexNumber);
                    //      byte b = Convert.ToByte(DA.ToString());
                    byte c = byte.Parse("5A", System.Globalization.NumberStyles.HexNumber);
                    string BCC = (a + b + c).ToString("X2");
                    if (BCC.Length == 3)
                    {
                        BCC = BCC.Substring(1);
                    }

                    //数据格式汇总
                    string d = "3D" + "5A" + CH + DA + BCC + "0D";

                    //TCP Client发送
                    //         myClient1.SendMsg2("3D5A0188E30D");
                    myClient1.SendMsg2(d);
                    Thread.Sleep(60);
                }
            }
            catch (Exception)
            {
                richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "光源参数输入错误" + "\r\n");
            }
        }
        public static void MRCloseF(NetClient myClient1, RichTextBox richTextBox)
        {
            try
            {
                for (int i = 1; i <= 3; i++)
                {
                    //通道参数处理
                    string CH = "0" + i.ToString();
                    string DA = "00";
                    //校验位处理
                    byte a = byte.Parse(CH, System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(DA, System.Globalization.NumberStyles.HexNumber);
                    //      byte b = Convert.ToByte(DA.ToString());
                    byte c = byte.Parse("5A", System.Globalization.NumberStyles.HexNumber);
                    string BCC = (a + b + c).ToString("X2");
                    if (BCC.Length == 3)
                    {
                        BCC = BCC.Substring(1);
                    }

                    //数据格式汇总
                    string d = "3D" + "5A" + CH + DA + BCC + "0D";

                    //TCP Client发送 myClient1.SendMsg2("3D5A0188E30D");
                    myClient1.SendMsg2(d);
                    Thread.Sleep(60);
                }
            }
            catch (Exception)
            {
                richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "光源参数输入错误" + "\r\n");
            }
        }

        #endregion

        #region OPT光源
        private static String serialPortName;
        private static String IPAddr;
        private static String SN;
        private static OPTControllerAPI OPTController = null;
        static int ch1 = 0, ch2 = 0, ch3 = 0, ch4 = 0;
        public static void LightParamInitailize(MainForm form)
        {
            ch1 = Convert.ToInt32(form.myIniFile.IniReadValue("OPT光源控制器", "channl1"));
            ch2 = Convert.ToInt32(form.myIniFile.IniReadValue("OPT光源控制器", "channl2"));
            ch3 = Convert.ToInt32(form.myIniFile.IniReadValue("OPT光源控制器", "channl3"));
            ch4 = Convert.ToInt32(form.myIniFile.IniReadValue("OPT光源控制器", "channl4"));
        }
        public static void OPTConnect(RichTextBox richTextBox)
        {
            try
            {
                OPTController = new OPTControllerAPI();//连接光源控制器
                SN = "AA53190328";
                IPAddr = "192.168.4.16";
                long lRet = -1;
                if ("" == IPAddr)
                {
                    //OPTLightOFF.Visible = true;
                    // richTextBox1.Text = "OPT Serial name can not be empty";
                    richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "OPT Serial name can not be empty！" + "\r\n");
                    richTextBox.ScrollToCaret();
                    return;
                }

                lRet = OPTController.CreateEthernetConnectionByIP(IPAddr);
                if (0 != lRet)
                {
                    //OPTLightOFF.Visible = true;
                    // richTextBox1.Text = "OPT Failed to create Ethernet connection by IP";
                    richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "OPT Failed to create Ethernet connection by IP！" + "\r\n");
                    richTextBox.ScrollToCaret();
                }
                else
                {
                    richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "OPT光源控制器加载完成！" + "\r\n");
                }
            }
            catch
            {
                richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "OPT光源控制器加载失败！" + "\r\n");
            }
        }

        //开灯
        public static void OPTOpenT()
        {
            OPTController.SetIntensity(1, ch1);//设置光源亮度（通道号，光度）214
            OPTController.TurnOnChannel(1);
            OPTController.SetIntensity(2, ch2);//设置光源亮度（通道号，光度）58
            OPTController.TurnOnChannel(2);
        }
        public static void OPTOpenS()
        {
            OPTController.SetIntensity(3, ch3);//设置光源亮度（通道号，光度）41
            OPTController.TurnOnChannel(3);
            OPTController.SetIntensity(4, ch4);//设置光源亮度（通道号，光度）17
            OPTController.TurnOnChannel(4);
        }

        //关灯
        public static void OPTCloseT()
        {
            OPTController.TurnOffChannel(1);
            OPTController.TurnOffChannel(2);
        }
        public static void OPTCloseS()
        {
            OPTController.TurnOffChannel(3);
            OPTController.TurnOffChannel(4);
        }
        #endregion
    }
}
