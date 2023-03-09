using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Cognex.VisionPro.FGGigE;
using Cognex.VisionPro.Implementation;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.PMAlign;
using System.Drawing.Imaging;
using MvCamCtrl.NET;
using CSharp_OPTControllerAPI;
using MyTools.ServiceReference1;
using MESLinkTEST;
using ToolTotal;

namespace MyTools
{
    public partial class MainForm : Form
    {

        #region 初始化

        public IniFile myIniFile;
        Log log = new Log();
        public string imagepath;
        public string datapath;
        /// <summary>
        /// 默然光源通讯端
        /// </summary>
        //NetClient myClient1 = new NetClient();
        /// <summary>
        /// Mac Mini端
        /// </summary>
        NetClient myClient2 = new NetClient();
        public SerialPort_232 com232 = new SerialPort_232();
        public StartForm startForm = new StartForm();
        public Thread ThreadRunStatus;//线程1
        public Thread ThreadRunIO;//线程2
        public Thread RemoteIOStatus;//读远程IO线程
        private delegate void FlushClient();//线程代理
        string pathpicture;
        double stop_time;
        double start_time;
        string Start_Ti;
        string Stop_Ti;
        double total, totaldata, passtotal;
        double CL, CLD;
        bool pass = false;
        string DayOrNight = "";
        bool ScanIOCard = true;
        bool DayOrNightRun = true;
        bool PingStatic = false;
        int SNlengthData = 12;
        string Line = "";

        bool bUploadPDCA = true;

        public MainForm()
        {
            InitializeComponent();
            pathpicture = Application.StartupPath + "\\Picture\\";
            myIniFile = new IniFile(Application.StartupPath + "\\Configuration.ini");//初始化配置文件位置   
            total = Convert.ToDouble(myIniFile.IniReadValue("Startup", "LabelPD").ToString());//产品总数
            totaldata = Convert.ToDouble(myIniFile.IniReadValue("Startup", "PData").ToString());//复检数
            CL = Convert.ToDouble(myIniFile.IniReadValue("Startup", "LabelCL").ToString());//检测次数
            //CLD = Convert.ToDouble(myIniFile.IniReadValue("Startup", "CLData").ToString());
            passtotal = Convert.ToDouble(myIniFile.IniReadValue("Startup", "passto").ToString());//pass数
            label41.Text = myIniFile.IniReadValue("功能", "Version").ToString();

            string str = myIniFile.IniReadValue("功能", "SN长度").ToString();
            SNLength.Text = str;
            int.TryParse(str, out SNlengthData);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            startForm.Show();
            VisionInitialize.LoadVPP(this, this.richTextBox1);// LoadVPP();//加载VPP
            startForm.Close();
            try
            {
                MES.SajetTransStart();
                AOIMethod.ViewImage(pictureBox5, pathpicture + "GreenOn2.bmp");
            }
            catch (System.Exception ex)
            {
                label28.Text = "S  F  C 不上传";
                MES.SajetTransClose();
                AOIMethod.ViewImage(pictureBox5, pathpicture + "Alarm.bmp");
                MessageBox.Show(ex.Message);
            }
            //this.WindowState = FormWindowState.Maximized;//当前Form最大化
            CheckForIllegalCrossThreadCalls = false; //不检查线程安全

            totallab.Text = total.ToString();
            if (total == 0)
            {
                totaldatalab.Text = "0%";
                CLDataLB.Text = "0%";
            }
            else
            {
                totaldatalab.Text = (double.Parse((totaldata / total).ToString("0.000")) * 100).ToString() + "%";
                CLDataLB.Text = (double.Parse((passtotal / total).ToString("0.000")) * 100).ToString() + "%";

                //添加PASS FAIL显示
                passnum.Text = passtotal.ToString();
                ngnum.Text = (total - passtotal).ToString();
            }
            Define.StartButtonDouble = false;//双启动状态
            Define.SNOK = false;//SN状态
            Define.挡板状态 = false;
            Define.运行中 = false;

            richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "相机加载完成！" + "\r\n");
            //LightInitialize.RGBParamInitailize(this);
            //LightInitialize.RGBConnect(this, myClient1, richTextBox1);//连接默然光源控制器

            try
            {
                AOIMethod.checkmes("169.254.1.10", ref PingStatic);
                if (PingStatic)
                {
                    myClient2.Open("169.254.1.10", 1111);
                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "Mac mini通讯OK！" + "\r\n");
                    AOIMethod.ViewImage(pictureBox4, pathpicture + "GreenOn2.bmp");
                }
                else
                {
                    label25.Text = "Mac mini 不上传";
                    AOIMethod.ViewImage(pictureBox4, pathpicture + "Alarm.bmp");
                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "未找到Mac mini！" + "\r\n");
                }
            }
            catch
            {
                label25.Text = "Mac mini 不上传";
                AOIMethod.ViewImage(pictureBox4, pathpicture + "Alarm.bmp");
                richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "Mac mini通讯异常！" + "\r\n");
            }
            com232.loadSerialPort1(this, richTextBox1);//加载IO串口
            com232.loadSerialPort2(this, richTextBox1);//加载扫码枪串口
            richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "扫码枪加载完成！" + "\r\n");
            //OPTController = new OPTControllerAPI();//连接光源控制器
            LightInitialize.LightParamInitailize(this);
            LightInitialize.OPTConnect(this.richTextBox1);//连接OPT光源控制器
            LightInitialize.OPTCloseT();
            LightInitialize.OPTCloseS();

            //LightInitialize.MRCloseF(myClient1, richTextBox1);

            RemoteIOStatusThread();//子线程开启            

            ParamInitialize.ReadSettings(this, Days, LogDays);

            timer1.Interval = 100;//打开定时器
            this.timer1.Start();
          
            AOIMethod.ViewImage(pictureBox1, pathpicture + "CCD1.bmp");
            AOIMethod.ViewImage(pictureBox2, pathpicture + "CCD2.bmp");
            AOIMethod.ViewImage(pictureBox3, pathpicture + "CCD3.bmp");
            myIniFile.IniWriteValue("Startup", "Statue", "1");//写配置文件
            richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "整机初始化完成！" + "\r\n");
            if (myIniFile.IniReadValue("Startup", "OpenTime") == "0")//不执行自动清零
                AutoClear.BackColor = Color.LightGray;
            else
                AutoClear.BackColor = Color.LightGreen;
            groupBox13.Enabled = false;
            if (myIniFile.IniReadValue("Startup", "bc") == "0")//不执行自动删除图片
                LoosenCh.Checked = false;
            else
                LoosenCh.Checked = true;
            if (myIniFile.IniReadValue("Startup", "DelPic") == "1")
                DeletePhoto.BackColor = Color.LightGreen;
            else
                DeletePhoto.BackColor = Color.LightGray;

            AOIMethod.DeleteOldFiles(imagepath + "CCD1\\", int.Parse(Days.Text.Trim()));
            AOIMethod.DeleteOldFiles(imagepath + "CCD2\\", int.Parse(Days.Text.Trim()));
            AOIMethod.DeleteOldFiles(imagepath + "CCD3\\", int.Parse(Days.Text.Trim()));
            AOIMethod.DeleteOldLog(Application.StartupPath + "\\Log", int.Parse(LogDays.Text.Trim()));
            AOIMethod.DeleteOldFiles("D:\\Log", int.Parse(Days.Text.Trim()));

        }

        #endregion

        #region VPP运行       
        public void VppRun8()
        {
            try
            {
                Define.ToolBlock[1].Run();
                if (Define.ToolBlock[1].RunStatus.Result.ToString() != "Error")
                {
                    this.cogRecordDisplay1.Image = (ICogImage)Define.ToolBlock[1].Outputs["OutputImage"].Value;
                    this.cogRecordDisplay1.Fit();

                    this.cogRecordDisplay1.StaticGraphics.Clear();
                    this.cogRecordDisplay1.InteractiveGraphics.Clear();
                    this.cogRecordDisplay1.Record = Define.ToolBlock[1].CreateLastRunRecord();
                    Define.GapTL = Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[1].Outputs["GapLeftScore"].Value.ToString()).ToString("0.000"));//更改
                    Define.GapTR = Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[1].Outputs["GapRightScore"].Value.ToString()).ToString("0.000"));//更改
                    Define.OffsetTL = Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[1].Outputs["OFFSetLeft"].Value.ToString()).ToString("0.000"));
                    Define.OffsetTR = Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[1].Outputs["OFFSetRight"].Value.ToString()).ToString("0.000"));
                    if (LoosenCh.Checked)
                    {
                        Define.OffsetTL = AOIMethod.buchang(Define.OffsetTL, double.Parse(textBoxUnder6_270.Text), double.Parse(textBoxUpper6_270.Text), double.Parse(myIniFile.IniReadValue("Startup", "buchang")));
                        Define.OffsetTR = AOIMethod.buchang(Define.OffsetTR, double.Parse(textBoxUnder6_270.Text), double.Parse(textBoxUpper6_270.Text), double.Parse(myIniFile.IniReadValue("Startup", "buchang")));
                    }
                }
                else
                {
                    Define.CCD[1].Run();
                    this.cogRecordDisplay1.Image = (ICogImage)Define.CCD[1].OutputImage;
                    this.cogRecordDisplay1.Fit();
                    this.cogRecordDisplay1.StaticGraphics.Clear();
                    this.cogRecordDisplay1.InteractiveGraphics.Clear();
                    this.cogRecordDisplay1.Record = Define.CCD[1].CreateLastRunRecord();
                    ShowMsg1("CCD1相机检测失败 ");
                    Define.GapTL = 999;
                    Define.GapTR = 999;
                    Define.OffsetTL = 999;
                    Define.OffsetTR = 999;
                }
            }
            catch (Exception)
            {
                ShowMsg1("CCD1异常");
            }
        }

        public void VppRun9()
        {
            try
            {
                Define.ToolBlock[2].Run();
                if (Define.ToolBlock[2].RunStatus.Result.ToString() != "Error")
                {
                    this.cogRecordDisplay2.Image = (ICogImage)Define.ToolBlock[2].Outputs["OutputImage"].Value;
                    this.cogRecordDisplay2.Fit();

                    this.cogRecordDisplay2.StaticGraphics.Clear();
                    this.cogRecordDisplay2.InteractiveGraphics.Clear();
                    this.cogRecordDisplay2.Record = Define.ToolBlock[2].CreateLastRunRecord();
                    Define.GapSL = Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[2].Outputs["GapLeftScore"].Value.ToString()).ToString("0.000"));
                    Define.GapSR = Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[2].Outputs["GapRightScore"].Value.ToString()).ToString("0.000"));
                    Define.OffsetSL = Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[2].Outputs["OFFSetLeft"].Value.ToString()).ToString("0.000"));
                    Define.OffsetSR = Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[2].Outputs["OFFSetRight"].Value.ToString()).ToString("0.000"));
                    if (LoosenCh.Checked)
                    {
                        Define.OffsetSL = AOIMethod.buchang(Define.OffsetSL, double.Parse(textBoxUnder6_270.Text), double.Parse(textBoxUpper6_270.Text), double.Parse(myIniFile.IniReadValue("Startup", "buchang")));
                        Define.OffsetSR = AOIMethod.buchang(Define.OffsetSR, double.Parse(textBoxUnder6_270.Text), double.Parse(textBoxUpper6_270.Text), double.Parse(myIniFile.IniReadValue("Startup", "buchang")));
                    }
                }
                else
                {
                    Define.CCD[2].Run();
                    this.cogRecordDisplay2.Image = (ICogImage)Define.CCD[2].OutputImage;
                    this.cogRecordDisplay2.Fit();
                    this.cogRecordDisplay2.StaticGraphics.Clear();
                    this.cogRecordDisplay2.InteractiveGraphics.Clear();
                    this.cogRecordDisplay2.Record = Define.CCD[2].CreateLastRunRecord();
                    ShowMsg1("CCD2检测失败 ");
                    Define.GapSL = 999;
                    Define.GapSR = 999;
                    Define.OffsetSL = 999;
                    Define.OffsetSR = 999;
                }
            }
            catch (Exception)
            {
                ShowMsg1("CCD2异常");
            }
        }

        //public void VppRun10()
        //{
        //    try
        //    {
        //        Define.ToolBlock[3].Run();
        //        if (Define.ToolBlock[3].RunStatus.Result.ToString() != "Error")
        //        {
        //            this.cogRecordDisplay3.Image = (ICogImage)Define.ToolBlock[3].Outputs["OutputImage"].Value;
        //            this.cogRecordDisplay3.Fit();

        //            this.cogRecordDisplay3.StaticGraphics.Clear();
        //            this.cogRecordDisplay3.InteractiveGraphics.Clear();
        //            this.cogRecordDisplay3.Record = Define.ToolBlock[3].CreateLastRunRecord();
        //            Define.FOffset0 = -Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[3].Outputs["Distance1"].Value.ToString()).ToString("0.000"));
        //            Define.FOffset90 = -Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[3].Outputs["Distance2"].Value.ToString()).ToString("0.000"));
        //            Define.FOffset180 = -Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[3].Outputs["Distance3"].Value.ToString()).ToString("0.000"));
        //            Define.FOffset270 = -Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[3].Outputs["Distance4"].Value.ToString()).ToString("0.000"));
        //            Define.FOffsetMAX = -Convert.ToDouble(Convert.ToDouble(Define.ToolBlock[3].Outputs["Distance"].Value.ToString()).ToString("0.000"));
        //            if (Define.OffsetSR > 0 || Define.FOffset0 > 0)
        //            {
        //                Define.FOffset0 = 0;
        //            }
        //            if (Define.OffsetTR > 0 || Define.FOffset90 > 0)
        //            {
        //                Define.FOffset90 = 0;
        //            }
        //            if (Define.OffsetSL > 0 || Define.FOffset180 > 0)
        //            {
        //                Define.FOffset180 = 0;
        //            }
        //            if (Define.OffsetTL > 0 || Define.FOffset270 > 0)
        //            {
        //                Define.FOffset270 = 0;
        //            }
        //            if (Define.FOffsetMAX > 0)
        //            {
        //                Define.FOffsetMAX = 0;
        //            }
        //            if (LoosenCh.Checked)
        //            {

        //                Define.FOffset0 = AOIMethod.buchang(Define.FOffset0, double.Parse(textBoxUnder6_270.Text), double.Parse(textBoxUpper6_270.Text), double.Parse(myIniFile.IniReadValue("Startup", "buchang")));
        //                Define.FOffset90 = AOIMethod.buchang(Define.FOffset90, double.Parse(textBoxUnder6_270.Text), double.Parse(textBoxUpper6_270.Text), double.Parse(myIniFile.IniReadValue("Startup", "buchang")));
        //                Define.FOffset180 = AOIMethod.buchang(Define.FOffset180, double.Parse(textBoxUnder6_270.Text), double.Parse(textBoxUpper6_270.Text), double.Parse(myIniFile.IniReadValue("Startup", "buchang")));
        //                Define.FOffset270 = AOIMethod.buchang(Define.FOffset270, double.Parse(textBoxUnder6_270.Text), double.Parse(textBoxUpper6_270.Text), double.Parse(myIniFile.IniReadValue("Startup", "buchang")));
        //            }
        //        }
        //        else
        //        {
        //            Define.CCD[3].Run();
        //            this.cogRecordDisplay3.Image = (ICogImage)Define.CCD[3].OutputImage;
        //            this.cogRecordDisplay3.Fit();
        //            this.cogRecordDisplay3.StaticGraphics.Clear();
        //            this.cogRecordDisplay3.InteractiveGraphics.Clear();
        //            this.cogRecordDisplay3.Record = Define.CCD[3].CreateLastRunRecord();
        //            ShowMsg1("CCD3检测失败 ");
        //            Define.FOffset0 = 999;
        //            Define.FOffset90 = 999;
        //            Define.FOffset180 = 999;
        //            Define.FOffset270 = 999;
        //            Define.FOffsetMAX = 999;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        ShowMsg1("CCD3异常");
        //    }
        //}
        #endregion

        #region 按钮事件
        // int ch1 = 0, ch2 = 0, ch3 = 0, ch4 = 0;
        private void timer1_Tick_1(object sender, EventArgs e)
        {
            //this.label_DateTime.Text = DateTime.Now.ToString();
            if (!Define.LimintChange)
            {
                Define.LimintChange = true;
                textBoxUpper5_0.Text = Define.SideSettingS[0].ToString("0.000");
                textBoxUpper5_90.Text = Define.TopSettingS[0].ToString("0.000");
                textBoxUpper5_180.Text = Define.SideSettingS[0].ToString("0.000");
                textBoxUpper5_270.Text = Define.TopSettingS[0].ToString("0.000");

                textBoxUnder5_0.Text = Define.SideSettingS[2].ToString("0.000");
                textBoxUnder5_90.Text = Define.TopSettingS[2].ToString("0.000");
                textBoxUnder5_180.Text = Define.SideSettingS[2].ToString("0.000");
                textBoxUnder5_270.Text = Define.TopSettingS[2].ToString("0.000");


                textBoxUpper6_0.Text = Define.SideSettingS[9].ToString("0.000");
                textBoxUpper6_90.Text = Define.TopSettingS[9].ToString("0.000");
                textBoxUpper6_180.Text = Define.SideSettingS[9].ToString("0.000");
                textBoxUpper6_270.Text = Define.TopSettingS[9].ToString("0.000");

                textBoxUnder6_0.Text = Define.SideSettingS[11].ToString("0.000");
                textBoxUnder6_90.Text = Define.TopSettingS[11].ToString("0.000");
                textBoxUnder6_180.Text = Define.SideSettingS[11].ToString("0.000");
                textBoxUnder6_270.Text = Define.TopSettingS[11].ToString("0.000");

                //textBoxF1.Text = Define.FrontSettingS[0].ToString("0.000");
                //textBoxF2.Text = Define.FrontSettingS[0].ToString("0.000");
                //textBoxF3.Text = Define.FrontSettingS[0].ToString("0.000");
                //textBoxF4.Text = Define.FrontSettingS[0].ToString("0.000");
                //textBoxF5.Text = Define.FrontSettingS[0].ToString("0.000");

                //textBoxF11.Text = Define.FrontSettingS[2].ToString("0.000");
                //textBoxF12.Text = Define.FrontSettingS[2].ToString("0.000");
                //textBoxF13.Text = Define.FrontSettingS[2].ToString("0.000");
                //textBoxF14.Text = Define.FrontSettingS[2].ToString("0.000");
                //textBoxF15.Text = Define.FrontSettingS[2].ToString("0.000");

            }

            textBoxFAI_5_0.Text = Define.GapSR.ToString("0.000");
            textBoxFAI_5_90.Text = Define.GapTR.ToString("0.000");
            textBoxFAI_5_180.Text = Define.GapSL.ToString("0.000");
            textBoxFAI_5_270.Text = Define.GapTL.ToString("0.000");

            textBoxFAI_6_0.Text = Define.OffsetSR.ToString("0.000");
            textBoxFAI_6_90.Text = Define.OffsetTR.ToString("0.000");
            textBoxFAI_6_180.Text = Define.OffsetSL.ToString("0.000");
            textBoxFAI_6_270.Text = Define.OffsetTL.ToString("0.000");

            //textBoxF21.Text = Define.FOffset0.ToString("0.000");
            //textBoxF22.Text = Define.FOffset90.ToString("0.000");
            //textBoxF23.Text = Define.FOffset180.ToString("0.000");
            //textBoxF24.Text = Define.FOffset270.ToString("0.000");
            //textBoxF25.Text = Define.FOffsetMAX.ToString("0.000");
        }

        private void TimeAndCheckPDCA()
        {
            while (true)
            {
                Thread.Sleep(300);
                try
                {

                    if (DayOrNightRun)
                    {
                        if (DateTime.Now.Hour > int.Parse(myIniFile.IniReadValue("Startup", "Day")) && DateTime.Now.Hour < int.Parse(myIniFile.IniReadValue("Startup", "Night")))
                        {
                            DayOrNight = DateTime.Now.ToString("yyyy-MM-dd") + "-Day";
                        }
                        else if (DateTime.Now.Hour > int.Parse(myIniFile.IniReadValue("Startup", "Night")))
                            DayOrNight = DateTime.Now.ToString("yyyy-MM-dd") + "-Night";
                        else
                            DayOrNight = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "-Night";

                    }

                    if (bUploadPDCA)
                    {
                        AOIMethod.checkmes("169.254.1.10", ref PingStatic);
                        Thread.Sleep(2000);
                        if (!PingStatic)
                        {
                            label25.Text = "Mac mini 掉线";
                            AOIMethod.ViewImage(pictureBox4, pathpicture + "Alarm.bmp");
                            richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "Mac mini断连！" + "\r\n");
                            richTextBox1.ScrollToCaret();
                            //Thread.Sleep(500);
                            try
                            {
                                myClient2.StopConnect();
                                Thread.Sleep(500);

                                myClient2.Open("169.254.1.10", 1111);
                                richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "Mac mini尝试重新连接！" + "\r\n");
                                richTextBox1.ScrollToCaret();
                                AOIMethod.ViewImage(pictureBox4, pathpicture + "GreenOn2.bmp");
                                label25.Text = "Mac mini 上传";
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
        }

        private void CheckRoleAndHandleCOMData()
        {
            while (true)
            {
                Thread.Sleep(50);
                try
                {

                    if (myIniFile.IniReadValue("Startup", "Statue") == "1")
                    {
                        label_LoginUser.Text = "操作员";
                    }
                    else if (myIniFile.IniReadValue("Startup", "Statue") == "2")
                    {
                        label_LoginUser.Text = "技术员";
                        if (Manual)
                            groupBox13.Enabled = true;
                        else
                            groupBox13.Enabled = false;
                    }
                    else if (myIniFile.IniReadValue("Startup", "Statue") == "3")
                    {
                        label_LoginUser.Text = "工程师";
                        if (Manual)
                        {
                            groupBox10.Enabled = true;
                            groupBox11.Enabled = true;
                            groupBox13.Enabled = true;
                            groupBox16.Enabled = true;
                            groupBox3.Enabled = true;
                            groupBox17.Enabled = true;
                        }
                        else
                        {
                            groupBox10.Enabled = false;
                            groupBox11.Enabled = false;
                            groupBox13.Enabled = false;
                            groupBox3.Enabled = false;
                            groupBox16.Enabled = false;
                            groupBox17.Enabled = false;
                        }
                    }

                    this.label_DateTime.Text = DateTime.Now.ToString();
                    if (ScanIOCard)
                    {
                        Thread.Sleep(50);
                        Define.sp1.Write("Cmd_MCU_Sensor_Check\r\n");
                        Thread.Sleep(50);
                    }

                    if (Days.Text == "" || int.Parse(Days.Text) <= 0 || Days.Text.Contains("."))
                        Days.Text = "50";
                    if (LoosenCh.Checked)
                        myIniFile.IniWriteValue("Startup", "bc", "1");
                    else
                        myIniFile.IniWriteValue("Startup", "bc", "0");

                    sp1_DataHandle();
                    CheckEmergency();
                    if (com232.m_bDataReceived)
                    {
                        sp2_DataHandle();
                    }
                }
                catch
                {
                    //Define.sp1.Close();
                    //Thread.Sleep(5);
                    //Define.sp1.Open();
                }
            }
        }

        #endregion

        #region 软件运行记录

        public void ShowMsg1(string msg)
        {
            string str = string.Format(DateTime.Now.ToString("HH:mm:ss") + " : " + msg);
            log.save(str + "\r\n");
        }

        private void ShowMsg2(string msg)
        {
            string str = string.Format(DateTime.Now.ToString("HH:mm:ss") + " : " + msg);
            log.save(str + "\r\n");
        }
        private void ShowMsg3(string msg)
        {
            string str = DateTime.Now.ToString("HH:mm:ss") + " : \r\n" + msg;
            log.save(str + "\r\n");
        }
        #endregion

        #region 图像显示全屏

        public int height1 = 44;
        public int height2 = 16;
        public int height3 = 40;
        public int height4 = 0;
        public int width1 = 33;
        public int width2 = 33;
        public int width3 = 34;
        public int width4 = 0;

        private void cogRecordDisplay1_DoubleClick_1(object sender, EventArgs e)
        {
            if (myIniFile.IniReadValue("Startup", "Statue") == "3")
            {
                if (this.displayLayoutPanel.RowStyles[0].Height == 100)
                {
                    this.displayLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[0].Height = height1;
                    this.displayLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[1].Height = height2;
                    this.displayLayoutPanel.RowStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[2].Height = height3;

                    this.displayLayoutPanel.ColumnStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[0].Width = width1;
                    this.displayLayoutPanel.ColumnStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[1].Width = width2;
                    this.displayLayoutPanel.ColumnStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[2].Width = width3;
                    fit();
                }
                else
                {
                    this.displayLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[0].Height = 100;
                    this.displayLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[1].Height = 0;
                    this.displayLayoutPanel.RowStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[2].Height = 0;

                    this.displayLayoutPanel.ColumnStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[0].Width = 100;
                    this.displayLayoutPanel.ColumnStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[1].Width = 0;
                    this.displayLayoutPanel.ColumnStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[2].Width = 0;
                    fit();
                }
            }
        }

        private void cogRecordDisplay2_DoubleClick(object sender, EventArgs e)
        {
            if (myIniFile.IniReadValue("Startup", "Statue") == "3")
            {
                if (this.displayLayoutPanel.ColumnStyles[1].Width == 100)
                {
                    this.displayLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[0].Height = height1;
                    this.displayLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[1].Height = height2;
                    this.displayLayoutPanel.RowStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[2].Height = height3;

                    this.displayLayoutPanel.ColumnStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[0].Width = width1;
                    this.displayLayoutPanel.ColumnStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[1].Width = width2;
                    this.displayLayoutPanel.ColumnStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[2].Width = width3;
                    fit();
                }
                else
                {
                    this.displayLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[0].Height = 100;
                    this.displayLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[1].Height = 0;
                    this.displayLayoutPanel.RowStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[2].Height = 0;

                    this.displayLayoutPanel.ColumnStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[0].Width = 0;
                    this.displayLayoutPanel.ColumnStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[1].Width = 100;
                    this.displayLayoutPanel.ColumnStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[2].Width = 0;
                    fit();
                }
            }
        }

        private void cogRecordDisplay3_DoubleClick(object sender, EventArgs e)
        {
            if (myIniFile.IniReadValue("Startup", "Statue") == "3")
            {
                if (this.displayLayoutPanel.ColumnStyles[2].Width == 100)
                {
                    this.displayLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[0].Height = height1;
                    this.displayLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[1].Height = height2;
                    this.displayLayoutPanel.RowStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[2].Height = height3;

                    this.displayLayoutPanel.ColumnStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[0].Width = width1;
                    this.displayLayoutPanel.ColumnStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[1].Width = width2;
                    this.displayLayoutPanel.ColumnStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[2].Width = width3;
                    fit();
                }
                else
                {
                    this.displayLayoutPanel.RowStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[0].Height = 100;
                    this.displayLayoutPanel.RowStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[1].Height = 0;
                    this.displayLayoutPanel.RowStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.RowStyles[2].Height = 0;

                    this.displayLayoutPanel.ColumnStyles[0].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[0].Width = 0;
                    this.displayLayoutPanel.ColumnStyles[1].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[1].Width = 0;
                    this.displayLayoutPanel.ColumnStyles[2].SizeType = SizeType.Percent;
                    this.displayLayoutPanel.ColumnStyles[2].Width = 100;
                    fit();
                }
            }
        }

        void fit()
        {
            try
            {
                this.cogRecordDisplay1.Fit(true);
                this.cogRecordDisplay2.Fit(true);
                //this.cogRecordDisplay3.Fit(true);
            }
            catch (Exception)
            {

            }
        }

        #endregion

        #region 子线程

        public void RemoteIOStatusThread()
        {
            RemoteIOStatus = new Thread(StartRun);//线程1开启
            RemoteIOStatus.IsBackground = true;
            RemoteIOStatus.Start();
            ThreadRunStatus = new Thread(new ThreadStart(TimeAndCheckPDCA));
            ThreadRunStatus.IsBackground = true;
            ThreadRunStatus.Start();
            ThreadRunIO = new Thread(new ThreadStart(CheckRoleAndHandleCOMData));
            ThreadRunIO.IsBackground = true;
            ThreadRunIO.Start();
        }

        public void StartRun()
        {
            while (true)
            {
                Thread.Sleep(10);
                Working();
            }
        }

        private void Working()
        {
            if (this.richTextBox1.InvokeRequired)
            {
                FlushClient fc1 = new FlushClient(Working);

                try//不加try，未断开连接前关闭软件会出异常
                {
                    this.Invoke(fc1);//通过代理调用刷新方法
                }
                catch
                {

                }
            }
            else
            {
                if (Define.SNOK)
                {
                    //if (YunXu)
                    //{
                    if (Define.挡板状态)//"&& YunXu)   //  if (Define.挡板状态&& YunXu) //
                    {
                        ScanIOCard = false;
                        Thread.Sleep(50);
                        Define.挡板状态 = false;
                        if (check200.Checked)
                            RunToCheck200();
                        else
                            Run();
                        Define.sp1.Write("Cmd_Off_" + Define.气缸 + "\r\n");//气缸上升   
                        Thread.Sleep(50);

                        stop_time = DateTime.Now.Hour * 3600 * 1000 + DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                        label38.Text = ((stop_time - start_time) / 1000).ToString("0.00");
                        label36.Text = "CT:" + label38.Text + "S";

                        Stop_Ti = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        SaveCCDPicture(Define.SN, cogRecordDisplay1.Image, "CCD1");
                        SaveCCDPicture(Define.SN, cogRecordDisplay2.Image, "CCD2");
                        //SaveCCDPicture(Define.SN, cogRecordDisplay3.Image, "CCD3");
                        while (!com232.StrBack.Contains(Define.气缸.Substring(2, 1) + " Off Pass!"))
                        {
                            Define.sp1.Write("Cmd_Off_" + Define.气缸 + "\r\n");//气缸上升    
                            Thread.Sleep(50);
                        }

                        GenerateMESData();
                        ScanIOCard = true;
                        Thread.Sleep(50);
                        if (Define.SN.Length == SNlengthData)
                        {
                            CL++;

                            myIniFile.IniWriteValue("Startup", "LabelCL", CL.ToString());
                            //ShowMsg3(FoxMes);
                            if (Define.GapTR.ToString() != "999" && Define.GapSR.ToString() != "999")
                            {
                                myClient2.SN = Define.SN;
                                myClient2.SendMsg(FoxMes);//上传Mac Mini

                                string sre111 = "";
                                FoxMes = "";
                                if (myClient2.ClientSocket.Connected && myClient2.connectOk && myClient2.TCPStatic)
                                {
                                    if (labelPassFail1.Text == "PASS")//视觉检测通过
                                    {
                                        mesMsg = OPtextbox.Text + ";" + SNtxtBox.Text + ";OK;";
                                        //添加 抽检模式
                                        if (button4.Text == "正常模式")
                                        {
                                            ConnMES(3, ref mesMsg);//上传MES
                                            if (mesMsg.Contains("NG"))
                                                MessageBox.Show("SFC上传信息失败：" + mesMsg);
                                        }
                                        else
                                        {
                                            ConnMES(48, ref mesMsg);
                                        }
                                        sre111 = mesMsg;
                                        SaveSN(Define.SN + "-P");
                                    }
                                    else
                                    {
                                        mesMsg = OPtextbox.Text + ";" + SNtxtBox.Text + ";NG;" + failmes;

                                        //添加 抽检模式
                                        if (button4.Text == "正常模式")
                                        {
                                            ConnMES(3, ref mesMsg);
                                            if (mesMsg.Contains("NG"))
                                                MessageBox.Show("SFC上传信息失败：" + mesMsg);
                                        }
                                        else
                                        {
                                            ConnMES(48, ref mesMsg);
                                        }
                                        sre111 = mesMsg;
                                        SaveSN(Define.SN);
                                    }
                                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + sre111 + "\r\n");
                                    richTextBox1.ScrollToCaret();
                                    Total();
                                }
                                else
                                {
                                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "Mac mini与SFC断连，请重新连接!" + "\r\n");
                                    richTextBox1.ScrollToCaret();
                                    labelPassFail1.Text = "mini Err";
                                    labelPassFail1.BackColor = Color.Yellow;
                                }
                            }

                        }
                        YunXu = false;
                        SNtxtBox.Text = "";
                        HoldSN = "";
                        HoldSNtxtBox.Text = "";
                        failmes = "";

                    }
                }
            }
        }

        private void RunToCheck200()
        {
            Define.sp1.Write("Cmd_On_" + Define.气缸 + "\r\n");
            Thread.Sleep(50);
            while (!com232.StrBack.Contains(Define.气缸.Substring(2, 1) + " On Pass!"))
            {
                Define.sp1.Write("Cmd_On_" + Define.气缸 + "\r\n");//气缸上升    
                Thread.Sleep(50);
            }
            Thread.Sleep(1500);
            for (int i = 0; i < int.Parse(CorrTextBox.Text); i++)
            {
                //Run();
                SaveCCDPicture(Define.SN, cogRecordDisplay1.Image, "CCD1");
                SaveCCDPicture(Define.SN, cogRecordDisplay2.Image, "CCD2");
                //SaveCCDPicture(Define.SN, cogRecordDisplay3.Image, "CCD3");
                GenerateMESData();
            }
        }

        int ScanCount = 0;
        public void Run()
        {
            if (check200.Checked)
            { }
            else
            {

                Define.sp1.Write("Cmd_On_" + Define.气缸 + "\r\n");
                Thread.Sleep(50);
                while (!com232.StrBack.Contains(Define.气缸.Substring(2, 1) + " On Pass!"))
                {
                    Define.sp1.Write("Cmd_On_" + Define.气缸 + "\r\n");//气缸上升    
                    Thread.Sleep(50);
                }
                myClient2.SN = Define.SN;
                myClient2.SendMsg("{\r\n" + Define.SN + "@sfc_unit_check\r\n}\r\n");
                Thread.Sleep(1200);
            }

            Define.运行中 = true;
            Define.SNOK = false;
            Define.StartButtonDouble = false;
            LightInitialize.OPTOpenT();
            Thread.Sleep(10);
            VppRun8();
            LightInitialize.OPTCloseT();

            LightInitialize.OPTOpenS();
            Thread.Sleep(10);
            VppRun9();
            LightInitialize.OPTCloseS();

            //LightInitialize.MROpenF(myClient1, richTextBox1);
            //Thread.Sleep(10);
            //VppRun10();
            //LightInitialize.MRCloseF(myClient1, richTextBox1);
            Define.运行中 = false;

        }
        #endregion

        #region 生成MES文件     
        string FoxMes = "";
        string failmes = "";
        public void GenerateMESData()
        {
            string fails = "";

            #region 数据处理

            //Gap侧右数据判断//此处GAP上下限原为数组3-5，现改为0-1
            if (Define.GapSR <= Define.SideSettingS[0] && Define.GapSR >= Define.SideSettingS[2])
            {
                textBoxUpper5_0.ForeColor = Color.Black;
                textBoxUnder5_0.ForeColor = Color.Black;

                Gap_0.ForeColor = Color.Black;
            }
            else if (Define.GapSR > Define.SideSettingS[0])
            {
                textBoxUpper5_0.ForeColor = Color.Red;
                textBoxUnder5_0.ForeColor = Color.Black;
                fails = "FAI5-0";
                failmes = "Gap-0,";
                Gap_0.ForeColor = Color.Red;
            }
            else
            {
                textBoxUpper5_0.ForeColor = Color.Black;
                textBoxUnder5_0.ForeColor = Color.Red;
                fails = "FAI5-0";
                failmes = "Gap-0,";
                Gap_0.ForeColor = Color.Red;
            }
            //Gap上右数据判断//此处GAP上下限原为数组3-5，现改为0-1
            if (Define.GapTR <= Define.TopSettingS[0] && Define.GapTR >= Define.TopSettingS[2])
            {
                textBoxUpper5_90.ForeColor = Color.Black;
                textBoxUnder5_90.ForeColor = Color.Black;
                Gap_90.ForeColor = Color.Black;
            }
            else if (Define.GapTR > Define.TopSettingS[0])
            {
                textBoxUpper5_90.ForeColor = Color.Red;
                textBoxUnder5_90.ForeColor = Color.Black;
                Gap_90.ForeColor = Color.Red;
                fails += "/FAI5-90";
                failmes += "Gap-90,";
            }
            else
            {
                textBoxUpper5_90.ForeColor = Color.Black;
                textBoxUnder5_90.ForeColor = Color.Red;
                Gap_90.ForeColor = Color.Red;
                fails += "/FAI5-90";
                failmes += "Gap-90,";
            }

            //Gap侧左数据判断//此处GAP上下限原为数组0-2，现改为0-1
            if (Define.GapSL <= Define.SideSettingS[0] && Define.GapSL >= Define.SideSettingS[2])
            {
                textBoxUpper5_180.ForeColor = Color.Black;
                textBoxUnder5_180.ForeColor = Color.Black;

                Gap_180.ForeColor = Color.Black;
            }
            else if (Define.GapSL > Define.SideSettingS[0])
            {
                textBoxUpper5_180.ForeColor = Color.Red;
                textBoxUnder5_180.ForeColor = Color.Black;
                Gap_180.ForeColor = Color.Red;
                fails += "/FAI5-180";
                failmes += "Gap-180,";
            }
            else
            {
                textBoxUpper5_180.ForeColor = Color.Black;
                textBoxUnder5_180.ForeColor = Color.Red;
                Gap_180.ForeColor = Color.Red;
                fails += "/FAI5-180";
                failmes += "Gap-180,";
            }
            //Gap上左数据判断此处GAP上下限原为数组0-2，现改为0-1
            if (Define.GapTL <= Define.TopSettingS[0] && Define.GapTL >= Define.TopSettingS[2])
            {
                textBoxUpper5_270.ForeColor = Color.Black;
                textBoxUnder5_270.ForeColor = Color.Black;
                Gap_270.ForeColor = Color.Black;
            }
            else if (Define.GapTL > Define.TopSettingS[0])
            {
                textBoxUpper5_270.ForeColor = Color.Red;
                textBoxUnder5_270.ForeColor = Color.Black;
                Gap_270.ForeColor = Color.Red;
                fails += "/FAI5-270";
                failmes += "Gap-270,";
            }
            else
            {
                textBoxUpper5_270.ForeColor = Color.Black;
                textBoxUnder5_270.ForeColor = Color.Red;
                Gap_270.ForeColor = Color.Red;
                fails += "/FAI5-270";
                failmes += "Gap-270,";
            }

            //Offset侧右数据判断//现改为9-11
            if (Define.OffsetSR <= Define.SideSettingS[9] && Define.OffsetSR >= Define.SideSettingS[11])
            {
                textBoxUpper6_0.ForeColor = Color.Black;
                textBoxUnder6_0.ForeColor = Color.Black;
                Offset_0.ForeColor = Color.Black;
            }
            else if (Define.OffsetSR > Define.SideSettingS[9])
            {
                textBoxUpper6_0.ForeColor = Color.Red;
                textBoxUnder6_0.ForeColor = Color.Black;
                Offset_0.ForeColor = Color.Red;
                fails += "/FAI6-0";
                failmes += "Offset-0,";
            }
            else if (Define.OffsetSR < Define.SideSettingS[11])
            {
                textBoxUnder6_0.ForeColor = Color.Red;
                textBoxUpper6_0.ForeColor = Color.Black;
                Offset_0.ForeColor = Color.Red;
                fails += "/FAI6-0";
                failmes += "Offset-0,";
            }

            //Offset上右数据判断//此处OFFSET上下限原为数组9-11，现改为9-11
            if (Define.OffsetTR <= Define.TopSettingS[9] && Define.OffsetTR >= Define.TopSettingS[11])
            {
                textBoxUpper6_90.ForeColor = Color.Black;
                textBoxUnder6_90.ForeColor = Color.Black;
                Offset_90.ForeColor = Color.Black;
            }
            else if (Define.OffsetTR > Define.TopSettingS[9])
            {
                textBoxUpper6_90.ForeColor = Color.Red;
                textBoxUnder6_90.ForeColor = Color.Black;
                Offset_90.ForeColor = Color.Red;
                fails += "/FAI6-90";
                failmes += "Offset-90,";
            }
            else if (Define.OffsetTR < Define.TopSettingS[11])
            {
                textBoxUnder6_90.ForeColor = Color.Red;
                textBoxUpper6_90.ForeColor = Color.Black;
                Offset_90.ForeColor = Color.Red;
                fails += "/FAI6-90";
                failmes += "Offset-90,";
            }

            //Offset侧左数据判断
            if (Define.OffsetSL <= Define.SideSettingS[9] && Define.OffsetSL >= Define.SideSettingS[11])
            {
                textBoxUpper6_180.ForeColor = Color.Black;
                textBoxUnder6_180.ForeColor = Color.Black;
                Offset_180.ForeColor = Color.Black;
            }
            else if (Define.OffsetSL > Define.SideSettingS[9])
            {
                textBoxUpper6_180.ForeColor = Color.Red;
                textBoxUnder6_180.ForeColor = Color.Black;
                Offset_180.ForeColor = Color.Red;
                fails += "/FAI6-180";
                failmes += "Offset-180,";
            }
            else if (Define.OffsetSL < Define.SideSettingS[11])
            {
                textBoxUnder6_180.ForeColor = Color.Red;
                textBoxUpper6_180.ForeColor = Color.Black;
                Offset_180.ForeColor = Color.Red;
                fails += "/FAI6-180";
                failmes += "Offset-180,";
            }

            //Offset上左数据判断//此处GAP上下限原为数组6-8，现改为9-11
            if (Define.OffsetTL <= Define.TopSettingS[9] && Define.OffsetTL >= Define.TopSettingS[11])
            {
                textBoxUpper6_270.ForeColor = Color.Black;
                textBoxUnder6_270.ForeColor = Color.Black;
                Offset_270.ForeColor = Color.Black;
            }
            else if (Define.OffsetTL > Define.TopSettingS[9])
            {
                textBoxUpper6_270.ForeColor = Color.Red;
                textBoxUnder6_270.ForeColor = Color.Black;
                Offset_270.ForeColor = Color.Red;
                fails += "/FAI6-270";
                failmes += "Offset-270,";
            }
            else if (Define.OffsetTL < Define.TopSettingS[11])
            {
                textBoxUnder6_270.ForeColor = Color.Red;
                textBoxUpper6_270.ForeColor = Color.Black;
                Offset_270.ForeColor = Color.Red;
                fails += "/FAI6-270";
                failmes += "Offset-270,";
            }

            #region CCD3 Offset


            //CCD3 Offset 0 数据处理//此处前相机OFFset上下限全改为0-2组
            //if (Define.FOffset0 <= Define.FrontSettingS[0] && Define.FOffset0 >= Define.FrontSettingS[2])
            //{
            //    textBoxF1.ForeColor = Color.Black;
            //    textBoxF11.ForeColor = Color.Black;
            //}
            //else if (Define.FOffset0 > Define.FrontSettingS[0])
            //{
            //    textBoxF1.ForeColor = Color.Red;
            //    textBoxF11.ForeColor = Color.Black;
            //}
            //else if (Define.FOffset0 < Define.FrontSettingS[2])
            //{
            //    textBoxF11.ForeColor = Color.Red;
            //    textBoxF1.ForeColor = Color.Black;
            //}

            ////CCD3 Offset 90 数据处理
            //if (Define.FOffset90 <= Define.FrontSettingS[0] && Define.FOffset90 >= Define.FrontSettingS[2])
            //{
            //    textBoxF2.ForeColor = Color.Black;
            //    textBoxF12.ForeColor = Color.Black;
            //}
            //else if (Define.FOffset90 > Define.FrontSettingS[0])
            //{
            //    textBoxF2.ForeColor = Color.Red;
            //    textBoxF12.ForeColor = Color.Black;
            //}
            //else if (Define.FOffset90 < Define.FrontSettingS[2])
            //{
            //    textBoxF12.ForeColor = Color.Red;
            //    textBoxF2.ForeColor = Color.Black;
            //}

            ////CCD3 Offset 180 数据处理
            //if (Define.FOffset180 <= Define.FrontSettingS[0] && Define.FOffset180 >= Define.FrontSettingS[2])
            //{
            //    textBoxF3.ForeColor = Color.Black;
            //    textBoxF13.ForeColor = Color.Black;
            //}
            //else if (Define.FOffset180 > Define.FrontSettingS[0])
            //{
            //    textBoxF3.ForeColor = Color.Red;
            //    textBoxF13.ForeColor = Color.Black;
            //}
            //else if (Define.FOffset180 < Define.FrontSettingS[2])
            //{
            //    textBoxF13.ForeColor = Color.Red;
            //    textBoxF3.ForeColor = Color.Black;
            //}

            ////CCD3 Offset 270 数据处理
            //if (Define.FOffset270 <= Define.FrontSettingS[0] && Define.FOffset270 >= Define.FrontSettingS[2])
            //{
            //    textBoxF4.ForeColor = Color.Black;
            //    textBoxF14.ForeColor = Color.Black;
            //}
            //else if (Define.FOffset270 > Define.FrontSettingS[0])
            //{
            //    textBoxF4.ForeColor = Color.Red;
            //    textBoxF14.ForeColor = Color.Black;
            //}
            //else if (Define.FOffset270 < Define.FrontSettingS[2])
            //{
            //    textBoxF14.ForeColor = Color.Red;
            //    textBoxF4.ForeColor = Color.Black;
            //}

            ////CCD3 Offset MAX 数据处理//此处OFFSET上下限不变
            //if (Define.FOffsetMAX < Define.FrontSettingS[12] && Define.FOffsetMAX >= Define.FrontSettingS[13])
            //{
            //    textBoxF5.ForeColor = Color.Black;
            //    textBoxF15.ForeColor = Color.Black;
            //}
            //else if (Define.FOffsetMAX >= Define.FrontSettingS[12])
            //{
            //    textBoxF5.ForeColor = Color.Red;
            //    textBoxF15.ForeColor = Color.Black;
            //}
            //else if (Define.FOffsetMAX < Define.FrontSettingS[13])
            //{
            //    textBoxF15.ForeColor = Color.Red;
            //    textBoxF5.ForeColor = Color.Black;
            //}


            #endregion

            #endregion

            if (fails == "")
            {
                // T = "TP";
                labelPassFail1.Text = "PASS";
                this.pass = true;
                labelPassFail1.BackColor = Color.Green;
            }
            else
            {
                failmes = failmes.Substring(0, failmes.Length - 1) + ";";
                this.pass = false;
                labelPassFail1.Text = "FAIL";
                labelPassFail1.BackColor = Color.Red;
            }

            if (File.Exists(datapath + DateTime.Now.ToString("yyyy-MM-dd") + "-Data.csv"))
            {
                log.save5(Define.NumberCSV.ToString() + "," + DateTime.Now.ToString() + "," + SNtxtBox.Text + "," + labelPassFail1.Text + "," + fails + "," + Define.GapSR.ToString() + "," + Define.GapTR.ToString() + "," + Define.GapSL.ToString() + "," + Define.GapTL.ToString() + "," + Define.OffsetSR + "," + Define.OffsetTR + "," + Define.OffsetSL + "," + Define.OffsetTL, datapath + DateTime.Now.ToString("yyyy-MM-dd") + "-Data.csv");//FOffset0, FOffset90, FOffset180, FOffset270, FOffsetMAX;//CCD3测试结果
                Define.NumberCSV++;
            }
            else
            {
                Define.NumberCSV = 1;
                string sree = "Number,time,SN,Rec,Fail,FAI5-0deg,FAI5-90deg,FAI5-180deg,FAI5-270deg,FAI6-0deg,FAI6-90deg,FAI6-180deg,FAI6-270deg";//,FAI6-0deg-CCD3,FAI6-90deg-CCD3,FAI6-180deg-CCD3,FAI6-270deg-CCD3,FAI6-MAX-CCD3
                log.save5(sree, datapath + DateTime.Now.ToString("yyyy-MM-dd") + "-Data.csv");
                log.save5(Define.NumberCSV.ToString() + "," + DateTime.Now.ToString() + "," + SNtxtBox.Text + "," + labelPassFail1.Text + "," + fails + "," + Define.GapSR.ToString() + "," + Define.GapTR.ToString() + "," + Define.GapSL.ToString() + "," + Define.GapTL.ToString() + "," + Define.OffsetSR + "," + Define.OffsetTR + "," + Define.OffsetSL + "," + Define.OffsetTL, datapath + DateTime.Now.ToString("yyyy-MM-dd") + "-Data.csv");//FOffset0, FOffset90, FOffset180, FOffset270, FOffsetMAX;//CCD3测试结果
                Define.NumberCSV++;
            }

            if (but_PDCA.Text == "开启上传PDCA")   //添加判断 是否上传PDCA系统
            {
                ToolDefine.SN = Define.SN;
                ToolDefine.开始时间 = Start_Ti;
                ToolDefine.GapSR = Define.GapSR.ToString();
                ToolDefine.GapTR = Define.GapTR.ToString();
                ToolDefine.GapSL = Define.GapSL.ToString();
                ToolDefine.GapTL = Define.GapTL.ToString();
                ToolDefine.OffSetSR = Define.OffsetSR.ToString();
                ToolDefine.OffSetTR = Define.OffsetTR.ToString();
                ToolDefine.OffSetSL = Define.OffsetSL.ToString();
                ToolDefine.OffSetTL = Define.OffsetTL.ToString();
                ToolDefine.Gap下限 = Define.SideSettingS[2].ToString();
                ToolDefine.Gap上限 = Define.SideSettingS[0].ToString();
                ToolDefine.Offset下限 = Define.SideSettingS[11].ToString();
                ToolDefine.Offset上限 = Define.SideSettingS[9].ToString();
                ToolDefine.停止时间 = Stop_Ti;
                ToolDefine.版本号 = label41.Text.Substring(8, 6);
                FoxMes = CatchData.formatMESData();
            }


            CL = Convert.ToDouble(myIniFile.IniReadValue("Startup", "LabelCL").ToString());
            //CLD = Convert.ToDouble(myIniFile.IniReadValue("Startup", "CLData").ToString());
            total = Convert.ToDouble(myIniFile.IniReadValue("Startup", "LabelPD").ToString());
            totaldata = Convert.ToDouble(myIniFile.IniReadValue("Startup", "PData").ToString());
            passtotal = Convert.ToDouble(myIniFile.IniReadValue("Startup", "passto").ToString());
        }

        #endregion

        #region 保存SN并识别

        /// <summary>
        /// 复检率
        /// total       总数
        /// passtotal   通过总数
        /// totaldata   复测总数
        /// </summary>
        private void Total()
        {
            Thread.Sleep(20);
            if (ReadSN3(Define.SN) == 1)
            {
                total++;
                myIniFile.IniWriteValue("Startup", "LabelPD", total.ToString());
                if (this.pass)
                {
                    passtotal++;
                    this.pass = false;
                }
            }
            else if (ReadSN3(Define.SN) == 2)
            {
                totaldata++;
                myIniFile.IniWriteValue("Startup", "PData", totaldata.ToString());
                if (this.pass)
                {
                    if (ReadSN3(Define.SN + "-P") == 1)
                    {
                        passtotal++;
                    }
                }
                else
                {
                    if (ReadSN3(Define.SN + "-P") == 1)
                    {
                        if (passtotal > 0)
                            passtotal--;
                    }
                }
                this.pass = false;
            }
            else if (ReadSN3(Define.SN) == 3)
            {

                if (this.pass)
                {
                    if (ReadSN3(Define.SN + "-P") == 1)//ffp +1/pfp +1/
                    {
                        passtotal++;

                    }
                    else if (ReadSN3(Define.SN + "-P") == 2 && ReadSNt(Define.SN + "-P") < ReadSNt(Define.SN))
                    {
                        passtotal++;
                    }
                }
                else
                {
                    if (ReadSN3(Define.SN + "-P") == 2)//ppf= -1/fpf -1/
                    {
                        if (passtotal > 0)
                            passtotal--;

                    }
                    else if (ReadSN3(Define.SN + "-P") == 1 && ReadSNt(Define.SN + "-P") > ReadSNt(Define.SN))
                    {
                        if (passtotal > 0)
                            passtotal--;
                    }
                }
                this.pass = false;
            }
            //else
            //{
            //    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "一个SN一天只有三次检测结果被纳入良率计算，检测超过三次，当天则不再录入！" + "\r\n");
            //}
            myIniFile.IniWriteValue("Startup", "passto", passtotal.ToString());
            if (total != 0)
            {
                totaldatalab.Text = (double.Parse((totaldata / total).ToString("0.000")) * 100).ToString() + "%";
                CLDataLB.Text = (double.Parse((passtotal / total).ToString("0.000")) * 100).ToString() + "%";

                //添加PASS FAIL显示
                passnum.Text = passtotal.ToString();
                ngnum.Text = (total - passtotal).ToString();

            }
            else
            {
                totaldatalab.Text = "0%";
                CLDataLB.Text = "0%";
            }
            totallab.Text = total.ToString();
        }

        public void SaveSN(string SN)
        {
            log.saveSN(SN + ',', DayOrNight, "");
        }

        public int ReadSN3(string SN)
        {
            return log.Read(SN, DayOrNight);
        }

        public int ReadSNt(string str)
        {
            return log.ReadSNo(str, DayOrNight);
        }
        #endregion

        #region 串口通讯配置

        #region 串口1 IO板信息处理

        string ReadIOStatus = "";
        string IOStatu = "1100000";
        bool Alarm = false;
        bool qigang = false;

        public void sp1_DataHandle()
        {

            if (com232.bIOOpened && com232.m_bIOReceived)
            {
                com232.m_bIOReceived = false;
                if (com232.StrBack.Length > 12)
                    ReadIOStatus = com232.StrBack.Substring(0, 12);
                //if (ReadIOStatus.Length == 12 && ReadIOStatus.Substring(5, 7) == "1111111")
                if (AOIMethod.IsNumber(ReadIOStatus) && ReadIOStatus.Substring(5, 7) == "1111111")
                {
                    IOStatu = ReadIOStatus;
                    string start = IOStatu.Substring(0, 3);
                    //string 气缸感应器 = RemoteIOStatus[0].Substring(0, 3);
                    if (Define.SNOK)
                    {
                        if (start == "000")
                        {
                            ScanIOCard = false;
                            Define.挡板状态 = true;
                            Define.StartButtonDouble = true;
                            Start_Ti = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            start_time = DateTime.Now.Hour * 3600 * 1000 + DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                        }
                        else
                        {
                            Define.挡板状态 = false;
                        }
                    }

                }

            }
        }
        private void CheckEmergency()
        {

            if (IOStatu.Substring(2, 1) == "1")
            {
                if (!Alarm)
                {
                    ScanIOCard = false;
                    Thread.Sleep(50);
                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "急停被按下！" + "\r\n");
                    Alarm = true;
                    if (ManualBtn.BackColor == Color.Green)
                        ManualBtn.PerformClick();
                    if (AutoBtn.BackColor == Color.Green)
                        AutoBtn.PerformClick();
                    ManualBtn.Enabled = false;
                    AutoBtn.Enabled = false;

                    Define.sp1.Write("Cmd_Off_" + Define.绿灯 + "\r\n");
                    Thread.Sleep(100);
                    Define.sp1.Write("Cmd_On_" + Define.红灯 + "\r\n");
                    Thread.Sleep(100);
                    Define.sp1.Write("Cmd_On_" + Define.蜂鸣 + "\r\n");
                    Thread.Sleep(100);
                    Define.sp1.Write("Cmd_Off_" + Define.绿灯 + "\r\n");
                    Thread.Sleep(100);
                    Define.sp1.Write("Cmd_On_" + Define.蜂鸣 + "\r\n");
                    Thread.Sleep(100);
                    ScanIOCard = true;
                }
            }
            else
            {
                if (Alarm)
                {
                    ScanIOCard = false;
                    Thread.Sleep(50);
                    Alarm = false;
                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "急停已复位！" + "\r\n");
                    ManualBtn.Enabled = true;
                    AutoBtn.Enabled = true;
                    Define.sp1.Write("Cmd_On_" + Define.绿灯 + "\r\n");
                    Thread.Sleep(100);
                    Define.sp1.Write("Cmd_Off_" + Define.红灯 + "\r\n");
                    Thread.Sleep(100);
                    Define.sp1.Write("Cmd_Off_" + Define.蜂鸣 + "\r\n");
                    Thread.Sleep(100);
                    Define.sp1.Write("Cmd_Off_" + Define.红灯 + "\r\n");
                    Thread.Sleep(100);
                    Define.sp1.Write("Cmd_Off_" + Define.蜂鸣 + "\r\n");
                    Thread.Sleep(100);
                    Define.sp1.Write("Cmd_On_" + Define.绿灯 + "\r\n");
                    Thread.Sleep(100);
                    ScanIOCard = true;
                }
            }
        }
        #endregion

        #region 扫码枪信息处理


        bool YunXu = false;
        string HoldSN = "";
        public void sp2_DataHandle()
        {
            if (Auto && com232.bScanOpened && com232.m_bDataReceived)
            {
                this.Invoke(new MethodInvoker(delegate {
                    labelPassFail1.Text = "WAIT";
                    labelPassFail1.BackColor = Color.YellowGreen;
                    com232.m_bDataReceived = false;
                    Define.挡板状态 = false;

                    if (SNInput.BackColor == Color.LightGray)//自动扫码
                    {
                        //SNtxtBox.Text = "";
                        Thread.Sleep(20);
                        //string[] RemoteIOStatus = Regex.Split(com232.ProtectSN, "\r\n", RegexOptions.IgnoreCase);

                        string s = com232.strBackSN.Substring(0, 4);


                        if (com232.strBackSN.Length > 0 && Define.运行中 == false)
                        {
                            //Define.SNOK = true;
                            Define.StartButtonDouble = false;

                            if (com232.strBackSN.Length == SNlengthData)
                            {

                                Define.StartButtonDouble = false;//添加
                                                                 //str2 = "GRMCX24BQ7PP";
                                Define.SN = com232.strBackSN;
                                mesMsg = com232.strBackSN + ";";
                                try
                                {
                                    ConnMES(2, ref mesMsg);
                                    if (mesMsg.Contains("OK"))
                                    {
                                        YunXu = true;
                                    }
                                    else
                                    {
                                        YunXu = false;
                                        Define.SNOK = false;
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    label28.Text = "S  F  C 掉线";
                                    MES.SajetTransClose();
                                    AOIMethod.ViewImage(pictureBox5, pathpicture + "Alarm.bmp");
                                    MessageBox.Show(ex.Message);
                                }
                                richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "SN上传反馈信息：" + mesMsg + "\r\n");
                                richTextBox1.ScrollToCaret();
                                SNtxtBox.Text = com232.strBackSN;
                                if (HoldSN != "")
                                {
                                    mesMsg = OPtextbox.Text + ";" + SNtxtBox.Text + ";" + HoldSN + ";";
                                    ConnMES(51, ref mesMsg);
                                    if (mesMsg.Contains("OK") || mesMsg.Contains("DUP"))
                                        Define.SNOK = true;
                                    else
                                        Define.SNOK = false;
                                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + mesMsg + "\r\n");
                                    richTextBox1.ScrollToCaret();
                                }
                            }
                            else if (com232.strBackSN.IndexOf("L") == 0 && com232.strBackSN.Length == 6)
                            {
                                HoldSNtxtBox.Text = com232.strBackSN;
                                HoldSN = com232.strBackSN;
                                if (YunXu)
                                {
                                    mesMsg = OPtextbox.Text + ";" + SNtxtBox.Text + ";" + HoldSN + ";";
                                    ConnMES(51, ref mesMsg);
                                    if (mesMsg.Contains("OK") || mesMsg.Contains("DUP"))
                                        Define.SNOK = true;
                                    else
                                        Define.SNOK = false;
                                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + mesMsg + "\r\n");
                                    richTextBox1.ScrollToCaret();
                                }
                            }
                            else
                            {
                                Define.SN = com232.strBackSN + DateTime.Now.ToString("HH时mm分ss秒");
                                Define.SNOK = false;
                                richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "SN码错误：" + "\r\n");
                                richTextBox1.ScrollToCaret();
                            }
                        }

                        com232.strBackSN = "";
                    }
                    else  //手动输码
                    {
                        if (SNtxtBox.Text.Length == SNlengthData && Define.运行中 == false)
                        {
                            Define.SN = SNtxtBox.Text;
                            Define.StartButtonDouble = false;

                            ////////以下两句正常时启用
                            mesMsg = Define.SN + ";";
                            ConnMES(2, ref mesMsg);
                            string mesreturen = mesMsg;

                            if (mesreturen.Contains("OK"))
                                YunXu = true;
                            else
                            {
                                YunXu = false;

                                Define.SNOK = false;
                            }
                            richTextBox1.AppendText(DateTime.Now.ToString("MM月dd日HH时mm分ss秒") + "   " + "SN上传反馈信息：" + mesreturen + "\r\n");
                            richTextBox1.ScrollToCaret();
                            ///                        
                        }
                        if (HoldSNtxtBox.Text.IndexOf("L") == 0 && HoldSNtxtBox.Text.Length == 6)
                        {
                            HoldSN = HoldSNtxtBox.Text;
                        }
                        Thread.Sleep(50);
                        if (HoldSN.Length == 6 && SNtxtBox.Text.Length == SNlengthData && YunXu)
                        {
                            mesMsg = OPtextbox.Text + ";" + SNtxtBox.Text + ";" + HoldSN + ";";
                            ConnMES(51, ref mesMsg);
                            Define.SNOK = true;
                            richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + mesMsg + "\r\n");
                            richTextBox1.ScrollToCaret();
                        }
                    }
                }));
            }
            else
            {
                com232.strBackSN = "";
                this.Invoke(new MethodInvoker(delegate {
                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "设备未启动" + "\r\n");
                    MessageBox.Show("请先启动自动模式！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                }));
            }
            com232.strBackSN = "";
        }
        #endregion
        #endregion      
       
        #region 存图
        private void CCD1Btn_Click(object sender, EventArgs e)
        {
            LightInitialize.OPTOpenT();
            Thread.Sleep(100);
            VppRun8();
            LightInitialize.OPTCloseT();
            ShowMsg1("CCD1拍照完成");
        }
        private void CCD2Btn_Click(object sender, EventArgs e)
        {
            LightInitialize.OPTOpenS();
            Thread.Sleep(100);
            VppRun9();
            LightInitialize.OPTCloseS();
            ShowMsg1("CCD2拍照完成");
        }

        private void OPT1OpenBtn_Click(object sender, EventArgs e)
        {
            LightInitialize.OPTOpenT();
        }

        private void OPT1CloseBtn_Click(object sender, EventArgs e)
        {
            LightInitialize.OPTCloseT();
        }

        private void OPT2OpenBtn_Click(object sender, EventArgs e)
        {
            LightInitialize.OPTOpenS();
        }

        private void OPT2CloseBtn_Click(object sender, EventArgs e)
        {
            LightInitialize.OPTCloseS();
        }

        private void CCD3Btn_Click(object sender, EventArgs e)
        {
            //LightInitialize.MROpenF(myClient1, richTextBox1);
            //Thread.Sleep(100);
            //VppRun10();
            //LightInitialize.MRCloseF(myClient1, richTextBox1);
            //ShowMsg1("CCD3拍照完成");
        }

        private void RGBOpenBtn_Click(object sender, EventArgs e)
        {
            //LightInitialize.MROpenF(myClient1, richTextBox1);
        }

        private void RGBCloseBtn_Click(object sender, EventArgs e)
        {
            //LightInitialize.MRCloseF(myClient1, richTextBox1);
        }

        private void Video1Btn_Click(object sender, EventArgs e)
        {
            if (this.cogRecordDisplay1.LiveDisplayRunning)
            {
                this.cogRecordDisplay1.StopLiveDisplay();
                Video1Btn.BackColor = Color.Transparent;
                ShowMsg1("CCD1实时结束");
            }
            else
            {
                cogRecordDisplay1.StaticGraphics.Clear();
                cogRecordDisplay1.InteractiveGraphics.Clear();
                cogRecordDisplay1.StartLiveDisplay(Define.CCD[1].Operator, false);
                Video1Btn.BackColor = Color.Green;
                ShowMsg1("CCD1实时中");
            }
        }

        private void Video2Btn_Click(object sender, EventArgs e)
        {
            if (this.cogRecordDisplay2.LiveDisplayRunning)
            {
                this.cogRecordDisplay2.StopLiveDisplay();
                Video2Btn.BackColor = Color.Transparent;
                ShowMsg1("CCD2实时结束");
            }
            else
            {
                cogRecordDisplay2.StaticGraphics.Clear();
                cogRecordDisplay2.InteractiveGraphics.Clear();
                this.cogRecordDisplay2.StartLiveDisplay(Define.CCD[2].Operator, false);
                Video2Btn.BackColor = Color.Green;
                ShowMsg1("CCD2实时中");
            }
        }

        private void Video3Btn_Click(object sender, EventArgs e)
        {
            //if (this.cogRecordDisplay3.LiveDisplayRunning)
            //{
            //    this.cogRecordDisplay3.StopLiveDisplay();
            //    Video3Btn.BackColor = Color.Transparent;
            //    ShowMsg1("CCD3实时结束");
            //}
            //else
            //{
            //    cogRecordDisplay3.StaticGraphics.Clear();
            //    cogRecordDisplay3.InteractiveGraphics.Clear();
            //    this.cogRecordDisplay3.StartLiveDisplay(Define.CCD[3].Operator, false);
            //    Video3Btn.BackColor = Color.Green;
            //    ShowMsg1("CCD3实时中");
            //}
        }

        private void DoorDownBtn_Click(object sender, EventArgs e)
        {
            Define.StartButtonDouble = false;
            Define.sp1.Write("Cmd_On_" + Define.气缸 + "\r\n");
        }

        private void DoorUpBtn_Click(object sender, EventArgs e)
        {
            Define.StartButtonDouble = false;
            Define.sp1.Write("Cmd_Off_" + Define.气缸 + "\r\n");

        }

        private void CCD1ConBtn_Click(object sender, EventArgs e)
        {
            CameraForm form = new CameraForm(Define.CCD[1], 1);
            form.ShowDialog();
        }

        private void CCD2ConBtn_Click(object sender, EventArgs e)
        {
            CameraForm form = new CameraForm(Define.CCD[2], 2);
            form.ShowDialog();
        }

        private void CCD3ConBtn_Click(object sender, EventArgs e)
        {
            CameraForm form = new CameraForm(Define.CCD[3], 3);
            form.ShowDialog();
        }

        private void VP1Btn_Click(object sender, EventArgs e)
        {
            InspectionForm form = new InspectionForm(Define.ToolBlock[1], 1);
            form.ShowDialog();
            ShowMsg1("CCD1程序保存成功");
        }

        private void VP2Btn_Click(object sender, EventArgs e)
        {
            InspectionForm form = new InspectionForm(Define.ToolBlock[2], 2);
            form.ShowDialog();
            ShowMsg2("CCD2程序保存成功");
        }

        private void VP3Btn_Click(object sender, EventArgs e)
        {
            InspectionForm form = new InspectionForm(Define.ToolBlock[3], 3);
            form.ShowDialog();
            ShowMsg2("CCD3程序保存成功");
        }

        private void OpenLOGBtn_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(datapath);
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            if (DialogResult.Cancel == MessageBox.Show("是否退出程序？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
            {
                return;
            }
            MES.SajetTransClose();
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + DayOrNight + ".txt"))
                if (log.SNstr.Length > File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + DayOrNight + ".txt").Length)
                    SaveSN(log.SNstr);
            myIniFile.IniWriteValue("Startup", "Statue", "1");//复位软件状态  
            //LightInitialize.MRCloseF(myClient1, richTextBox1);
            LightInitialize.OPTCloseT();
            LightInitialize.OPTCloseS();
            try
            {
                //myClient1.StopConnect();
                myClient2.StopConnect();
                timer1.Stop();
             
                Define.sp1.Write("Cmd_Off_" + Define.红灯 + "\r\n");
                Thread.Sleep(100);
                Define.sp1.Write("Cmd_Off_" + Define.黄灯 + "\r\n");
                Thread.Sleep(100);
                Define.sp1.Write("Cmd_Off_" + Define.绿灯 + "\r\n");
                Thread.Sleep(100);
                Define.sp1.Write("Cmd_Off_" + Define.蜂鸣 + "\r\n");
                CogFrameGrabberGigEs ff = new CogFrameGrabberGigEs();
                if (ff.Count > 0)
                {
                    foreach (ICogFrameGrabber f in ff)
                    {
                        f.Disconnect(true);
                    }
                }
                GC.Collect();
                Environment.Exit(0);
            }
            catch (Exception)
            {
                ShowMsg1("相机释放失败");
            }
            this.Close();
        }

        private void RGBSaveBtn_Click(object sender, EventArgs e)
        {
            myIniFile.IniWriteValue("功能", "保存天数", Days.Text);
        }

        private void SettingBtn_Click(object sender, EventArgs e)
        {
            SettingForm fr = new SettingForm();//多线程Form
            fr.Show();
        }

        private void ChangeUserLabel_Click(object sender, EventArgs e)
        {

            if (myIniFile.IniReadValue("Startup", "Statue") == "1")
            {
                new logon().ShowDialog(this);
                ChangeUserLabel.Text = "注销用户";
            }
            else
            {
                if (ManualBtn.BackColor == Color.Green)
                    ManualBtn.PerformClick();

                groupBox10.Enabled = false;
                groupBox11.Enabled = false;
                groupBox13.Enabled = false;
                groupBox16.Enabled = false;
                groupBox3.Enabled = false;
                groupBox17.Enabled = false;
                LoosenCh.Visible = false;
                myIniFile.IniWriteValue("Startup", "Statue", "1");//写配置文件
                ChangeUserLabel.Text = "切换用户";

            }

        }
        bool Manual = false, Auto = false;


        private void AutoBtn_Click(object sender, EventArgs e)
        {
            if (AutoBtn.BackColor == Color.LightGray)
            {

                mesMsg = OPtextbox.Text + ";";
                ConnMES(1, ref mesMsg);
                string str = mesMsg;// client.command_code_for_webservice("192.168.112.161", 1, OPtextbox.Text + ";");

                if (str.Contains("OK"))
                {
                    OPtextbox.Enabled = false;
                    Auto = true;
                    ManualBtn.Enabled = false;
                    AutoBtn.BackColor = Color.Green;
                    AutoBtn.Text = "停止自动";
                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "自动模式已开启！" + "\r\n");
                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "上传OP ID反馈：" + str + "\r\n");
                    richTextBox1.ScrollToCaret();
                }
                else
                    MessageBox.Show("请输入员工ID！");
            }
            else
            {
                OPtextbox.Enabled = true;
                Auto = false;
                ManualBtn.Enabled = true;
                AutoBtn.BackColor = Color.LightGray;
                AutoBtn.Text = "自动模式";
                richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "自动模式已关闭！" + "\r\n");
            }
            Manual = false;
            ManualBtn.BackColor = Color.LightGray;
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            myIniFile.IniWriteValue("Startup", "LabelCL", "0");
        }

        private void Production_Click(object sender, EventArgs e)
        {
            DayOrNightRun = true;
            string str = "截止到" + DateTime.Now.ToString() + "\t共检测产品:" + total + "支," + "Pass支数:" + passtotal + ",复检率:" + totaldatalab.Text + "\r\n";//",Retry:" + label41.Text + 
            log.SaveSN("汇总", str);
            myIniFile.IniWriteValue("Startup", "LabelPD", "0");
            myIniFile.IniWriteValue("Startup", "PData", "0");
            myIniFile.IniWriteValue("Startup", "passto", "0");
            CLDataLB.Text = "0%";
            totallab.Text = "0";
            totaldatalab.Text = "0%";
            passnum.Text = "0";
            ngnum.Text = "0";
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + DayOrNight + ".txt"))
            {
                File.Copy(System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + DayOrNight + ".txt", System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + DayOrNight + "-" + DateTime.Now.ToString("HH-mm-ss") + ".txt");
                File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + DayOrNight + ".txt");
            }
        }
        private void ConnectTool_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.BaseDirectory + "Tool");//打开文件夹
        }
        private void ManualBtn_Click(object sender, EventArgs e)
        {
            if (myIniFile.IniReadValue("Startup", "Statue") != "1")
            {
                if (ManualBtn.BackColor == Color.LightGray)
                {
                    Manual = true;
                    AutoBtn.Enabled = false;
                    ManualBtn.BackColor = Color.Green;
                    ManualBtn.Text = "停止手动";
                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "手动模式已开启！" + "\r\n");
                }
                else
                {
                    Manual = false;
                    AutoBtn.Enabled = true;
                    ManualBtn.BackColor = Color.LightGray;
                    ManualBtn.Text = "手动模式";
                    richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "手动模式已关闭！" + "\r\n");
                }
                Auto = false;
                AutoBtn.BackColor = Color.LightGray;
            }
            else
            {
                MessageBox.Show("您的权限不能使用此功能！");
            }
        }

        private void SaveCCDPicture(string sn, ICogImage image, string CCDNo)
        {
            int width = 0, heigth = 0; double size = 0;
            Bitmap myImage = image.ToBitmap();
            string directory = imagepath + CCDNo + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
            string strpath = @"\\169.254.1.10\Public\blobs\" + sn + "//";

            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                if (myImage != null)
                {
                    myImage.Save(directory + sn + ".jpg");

                    ShowMsg2(CCDNo + "-SN:" + sn + "存图成功");
                    if (sn.Length == SNlengthData && Define.GapTR.ToString() != "999" && Define.GapSR.ToString() != "999" && but_PDCA.Text == "开启上传PDCA") //添加判断 是否上传PDCA系统                        
                    {//当SN正常，且检测数据正常，则保持图片至Mac mini 
                        Directory.CreateDirectory(strpath);
                        AOIMethod.VaryQualityLevel(directory + sn + ".jpg", strpath + sn + "-" + CCDNo + ".jpg", ref width, ref heigth, ref size);
                        ShowMsg2(CCDNo + "-SN:" + sn + "-压缩并共享至Mac mini成功!" + "压缩后尺寸(像素)：" + heigth + " X " + width + "," + "占用空间：" + size.ToString() + "KB");
                    }
                }
                else
                {
                    ShowMsg2(CCDNo + "-SN:" + sn + "没有图像！");
                }
            }
            catch
            {
                ShowMsg2(CCDNo + "-SN:" + sn + "存图或上传图片异常！");
                richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + CCDNo + "-SN:" + sn + "存图或上传图片异常！" + "\r\n");
            }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MES.SajetTransClose();
            //LightInitialize.MRCloseF(myClient1, richTextBox1);
            LightInitialize.OPTCloseT();
            LightInitialize.OPTCloseS();
            myIniFile.IniWriteValue("Startup", "Statue", "1");//复位软件状态  
            Define.sp1.Close();
            Define.sp2.Close();
        }

        private void ChangePSW_Click(object sender, EventArgs e)
        {
            new ChangePassword().ShowDialog(this);//显示控制界面
        }
        #endregion

        #region 按钮操作

        private void AutoClear_Click(object sender, EventArgs e)
        {
            if (AutoClear.BackColor == Color.LightGray)
            {
                myIniFile.IniWriteValue("Startup", "OpenTime", "1");
                AutoClear.BackColor = Color.LightGreen;
            }
            else
            {
                AutoClear.BackColor = Color.LightGray;
                myIniFile.IniWriteValue("Startup", "OpenTime", "0");
            }
        }

        private void label41_Click(object sender, EventArgs e)
        {
            Version myversion = new Version();
            myversion.Show();
        }

        private void button4_Click(object sender, EventArgs e)//抽检模式
        {

            if (button4.Text == "正常模式")
            {
                button4.BackColor = Color.LawnGreen;
                button4.Text = "抽检模式";
            }
            else
            {
                button4.BackColor = Color.LightGray;
                button4.Text = "正常模式";
            }

        }

        /// <summary>
        /// 上传PDCA （MES系统）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_PDCA_Click(object sender, EventArgs e)
        {

            if (but_PDCA.Text == "开启上传PDCA")
            {
                but_PDCA.BackColor = Color.LawnGreen;
                but_PDCA.Text = "关闭上传PDCA";
                label25.Text = "Mac mini 不上传";
                AOIMethod.ViewImage(pictureBox4, pathpicture + "Alarm.bmp");
                bUploadPDCA = false;
            }
            else
            {
                but_PDCA.BackColor = Color.LightGray;
                but_PDCA.Text = "开启上传PDCA";
                label25.Text = "Mac mini 上传";
                AOIMethod.ViewImage(pictureBox4, pathpicture + "GreenOn2.bmp");
                bUploadPDCA = true;
            }

        }
        private void button5_Click(object sender, EventArgs e)
        {

            bool a = int.TryParse(SNLength.Text, out SNlengthData);
            if (!a)
                MessageBox.Show("SN长度设置异常！");
            else
                myIniFile.IniWriteValue("功能", "SN长度", SNLength.Text);
            int m = 0, n = 0;
            bool b = int.TryParse(Days.Text, out m);
            if (!b)
                MessageBox.Show("图片保存天数设置异常！");
            else
                myIniFile.IniWriteValue("功能", "保存天数", Days.Text);

            bool c = int.TryParse(LogDays.Text, out n);
            if (!c)
                MessageBox.Show("Log保存天数设置异常！");
            else
                myIniFile.IniWriteValue("功能", "Log保存天数", LogDays.Text);
            //myIniFile.IniWriteValue("UploadMES", "线体", LineTxt.Text);
            //Line = LineTxt.Text;
        }

        private void DeletePhoto_Click(object sender, EventArgs e)
        {
            if (DeletePhoto.BackColor == Color.LightGray)
            {
                myIniFile.IniWriteValue("Startup", "DelPic", "1");
                DeletePhoto.BackColor = Color.LightGreen;
            }
            else
            {
                DeletePhoto.BackColor = Color.LightGray;
                myIniFile.IniWriteValue("Startup", "DelPic", "0");
            }
        }

        private void SNInput_Click(object sender, EventArgs e)
        {
            if (SNInput.Text == "手动输码")
            {
                SNInput.BackColor = Color.LightGray;
                SNInput.Text = "启用扫码";
            }
            else
            {
                SNInput.BackColor = Color.LightGreen;
                SNInput.Text = "手动输码";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 20; i++)
                richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "相机加载完成！" + "\r\n");


            label24.Text = richTextBox1.Lines.Length.ToString();

            //if (richTextBox1.Lines.Length > 3)
            //    richTextBox1.Text = "";
            //sendmes = SNtxtBox.Text + ";";
            //ConnMES(2, ref sendmes);
            //client.Open();           
            //label54.Text = client.command_code_for_webservice("192.168.112.161", 2, SNtxtBox.Text + ";");
            //label24.Text = sendmes;
            //string str = "cws-0,ncf-90,mlc-180,ll-20,";
            //label24.Text = str.Substring(0, str.Length - 1) + ";";
            //
            // DeleteOldFiles("CCD1\\", int.Parse(Days.Text.Trim()));
        }

        #endregion

        #region 16进制与字符串相互转换

        private string GetChsFromHex(string hex)//16进制字符串解码
        {
            if (hex == null)
            {
                //throw new ArgumentException("hex is null!");
            }

            if (hex.Length % 2 != 0)
            {
                hex += "20";//空格
                            //throw new ArgumentException("hex is not a valid number!", "hex");
            }

            // 需要将 hex 转换成 byte 数组。
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    // 每两个字符是一个 byte。
                    bytes[i] = byte.Parse(hex.Substring(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    // Rethrow an exception with custom message.
                    throw new ArgumentException("hex is not a valid hex number!", "hex");
                }
            }

            // 获得 GB2312，Chinese Simplified。
            Encoding chs = System.Text.Encoding.GetEncoding("GB2312");
            //           Encoding chs = System.Text.Encoding.GetEncoding("UTF-8");

            return chs.GetString(bytes);
        }

        #endregion

        #region  连接SFC
        private void OpenPhotoBtn_Click_1(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"E:\Image");
        }

        string mesMsg = "";

        private void ConnMES(short cmd, ref string value)
        {
            int iLen, iCommand;
            string sData;
            int[] ailen;
            byte[] cData;
            iLen = value.Length;
            ailen = new int[1];
            ailen[0] = iLen;
            if (iLen < 1024)
            {
                iLen = 1024;
            }
            sData = value;
            iCommand = Convert.ToInt32(cmd);
            cData = new byte[iLen];


            for (int i = 0; i < sData.Length; i++)
            {
                cData[i] = Convert.ToByte(sData.ToCharArray()[i]);
            }
            if (MES.SajetTransData(iCommand, ref cData[0], ref ailen[0]))
            {
                sData = "";
                for (int i = 0; i < ailen[0]; i++)
                {
                    sData = sData + (char)cData[i];
                }
                value = sData;
            }
            else
            {
                sData = "";
                //连线异常的
                //sData = Encoding.GetEncoding("gb2312").GetString(cData);
                for (int i = 0; i < ailen[0]; i++)
                {
                    sData = sData + (char)cData[i];
                }
                value = sData;
            }
        }

        private void ConnMES_New(short cmd, ref string value)
        {
            int realLen = value.Length;
            var sData = value;
            var iCommand = Convert.ToInt32(cmd);
            var cData = Encoding.UTF8.GetBytes(sData);
            
            if (MES.SajetTransData(iCommand, ref cData[0], ref realLen))
            {
                //上传成功
            }
            else
            {
                //连线异常的
                //sData = Encoding.GetEncoding("gb2312").GetString(cData);
            }
        }
        #endregion
    }
}
