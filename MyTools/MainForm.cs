using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Cognex.VisionPro;
using System.Threading;
using System.IO;
using Cognex.VisionPro.FGGigE;
using MESLinkTEST;
using ToolTotal;
using Cowain_Form.FormView;
using SingleAxisMotion;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MyTools
{
    public partial class MainForm : Form
    {

        #region 初始化

        public IniFile myIniFile;
        ClassFile.Log log = new ClassFile.Log();
        public string imagepath;
        public string datapath;
        /// <summary>
        /// Mac Mini端   
        /// </summary>
        readonly NetClient macClient = new NetClient();
        public SerialPort_232 com232 = new SerialPort_232();
        public StartForm startForm = new StartForm();
        public Thread ThreadRunStatus;//线程1
        public Thread ThreadRunIO;//线程2
        public Thread RemoteIOStatus;//读远程IO线程
        private delegate void FlushClient();//线程代理
        string pathpicture;
        double stop_time_milli;
        double start_time_milli;
        string startTime;
        string stopTime;
        double total, totaldata, passtotal;
        double CL;
        bool pass = false;
        string strTime = "";
        volatile bool ScanIOCard = true;
        bool PingResult = false;
        int intSNLength = 12;
        bool Manual = false, Auto = false;

        bool bUploadPDCA = true;
        bool bUploadMES = true;
        private Motion motion;
        private string version;
        private string strSNLength;
        public static bool LoadVppSuccess;
        private bool bReturned;
        private bool bScanCode = true;
        private ConcurrentQueue<Dictionary<string, Color>> MessageQueue = new ConcurrentQueue<Dictionary<string, Color>>();
        private bool bDelPic;
        private bool bRunEmpty;
        private bool bSampling;

        public MainForm()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                File.AppendAllText(@"D:\Exception\UnhandledException\" + DateTime.Now.ToShortDateString() + ".txt", args.ToString());
                MessageBox.Show("出现Unhandled异常, 程序即将关闭");
                Application.Exit();
            };
            Application.ThreadException += (sender, args) =>
            {
                File.AppendAllText(@"D:\Exception\ThreadException\" + DateTime.Now.ToShortDateString() + ".txt", args.ToString());
                MessageBox.Show("出现Thread异常, 程序即将关闭");
                Application.Exit();
            };

            pathpicture = Application.StartupPath + "\\Picture\\";
            myIniFile = new IniFile(Application.StartupPath + "\\Configuration.ini");//初始化配置文件位置   
            total = Convert.ToDouble(myIniFile.IniReadValue("Startup", "LabelPD").ToString());//产品总数
            totaldata = Convert.ToDouble(myIniFile.IniReadValue("Startup", "PData").ToString());//复检数
            CL = Convert.ToDouble(myIniFile.IniReadValue("Startup", "LabelCL").ToString());//检测次数
            //CLD = Convert.ToDouble(myIniFile.IniReadValue("Startup", "CLData").ToString());
            passtotal = Convert.ToDouble(myIniFile.IniReadValue("Startup", "passto").ToString());//pass数
            version = myIniFile.IniReadValue("功能", "Version").ToString();
            strSNLength = myIniFile.IniReadValue("功能", "SN长度").ToString();

            startForm.ShowDialog();
            motion = new Motion(myIniFile);
            frm_LoadingDlg loading = new frm_LoadingDlg(ref motion);//加载板卡
            loading.ShowDialog();

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!LoadVppSuccess)
            {
                AddToQueue("VPP程序加载失败！", Color.Red);
                SaveMsg("VPP程序加载失败");
            }

            CheckForIllegalCrossThreadCalls = false; //不检查线程安全

            SNLength.Text = strSNLength;
            int.TryParse(strSNLength, out intSNLength);
            label41.Text = version;
            totallab.Text = total.ToString();
            if (total == 0)
            {
                totaldatalab.Text = "0%";
                CLDataLB.Text = "0%";
            }
            else
            {
                totaldatalab.Text = double.Parse((totaldata / total).ToString("0.000")) * 100 + "%";
                CLDataLB.Text = double.Parse((passtotal / total).ToString("0.000")) * 100 + "%";

                //添加PASS FAIL显示
                passnum.Text = passtotal.ToString();
                ngnum.Text = (total - passtotal).ToString();
            }
            Define.BindingOK = false;//SN状态
            Define.DoubleButtonDown = false;
            Define.运行中 = false;

            AddToQueue("相机加载完成！", Color.Black);

            com232.loadSerialPort1(myIniFile, AddToQueue);//加载IO串口
            com232.loadSerialPort2(myIniFile, AddToQueue);//加载扫码枪串口
            AddToQueue("扫码枪加载完成！", Color.Black);
            LightInitialize.LightParamInitailize(myIniFile);
            LightInitialize.OPTConnect(AddToQueue);//连接OPT光源控制器
            LightInitialize.OPTCloseT();
            LightInitialize.OPTCloseS();

            if (motion.SingleMotor.SetSevON(true))
            {
                motion.IsMotorServoOn = true;
                AddToQueue("轴使能OK！", Color.Black);
            }
            else
            {
                motion.IsMotorServoOn = false;
                AddToQueue("轴未使能！", Color.Red);
            }

            try
            {
                MES.SajetTransStart();
                AOIMethod.ViewImage(pictureBox5, pathpicture + "GreenOn2.bmp");
            }
            catch (System.Exception ex)
            {
                label_MesState.Text = "S  F  C 不上传";
                MES.SajetTransClose();
                AOIMethod.ViewImage(pictureBox5, pathpicture + "Alarm.bmp");
                MessageBox.Show(ex.Message);
            }

            OpenThreads();//子线程开启            

            ParamInitialize.ReadSettings(this, Days, LogDays);

            AOIMethod.ViewImage(pictureBox1, pathpicture + "CCD1.bmp");
            AOIMethod.ViewImage(pictureBox2, pathpicture + "CCD2.bmp");
            AOIMethod.ViewImage(pictureBox3, pathpicture + "CCD3.bmp");
            myIniFile.IniWriteValue("Startup", "Statue", "1");//操作员权限
            AddToQueue("整机初始化完成！", Color.Black);
            if (myIniFile.IniReadValue("Startup", "OpenTime") == "0")
                AutoClear.BackColor = Color.LightGray;
            else
                AutoClear.BackColor = Color.LawnGreen;
            groupBox13.Enabled = false;
            if (myIniFile.IniReadValue("Startup", "bc") == "0")
                LoosenCh.Checked = false;
            else
                LoosenCh.Checked = true;
            if (myIniFile.IniReadValue("Startup", "DelPic") == "1")
                DeletePhoto.BackColor = Color.LawnGreen;
            else
                DeletePhoto.BackColor = Color.LightGray;

            button_MES.Text = "禁用MES";
            button_MES.BackColor = Color.LawnGreen;
            button_PDCA.Text = "禁用PDCA";
            button_PDCA.BackColor = Color.LawnGreen;
        }

        #endregion

        #region VPP运行

        private void VppRun8()
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
                    SaveMsg("CCD1相机检测失败 ");
                    Define.GapTL = 999;
                    Define.GapTR = 999;
                    Define.OffsetTL = 999;
                    Define.OffsetTR = 999;
                }
            }
            catch (Exception)
            {
                SaveMsg("CCD1异常");
            }
        }

        private void VppRun9()
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
                    SaveMsg("CCD2检测失败 ");
                    Define.GapSL = 999;
                    Define.GapSR = 999;
                    Define.OffsetSL = 999;
                    Define.OffsetSR = 999;
                }
            }
            catch (Exception)
            {
                SaveMsg("CCD2异常");
            }
        }
        #endregion

        #region 按钮事件
        private void Timer_FlashValue_Tick(object sender, EventArgs e)
        {
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
            }

            textBoxFAI_5_0.Text = Define.GapSR.ToString("0.000");
            textBoxFAI_5_90.Text = Define.GapTR.ToString("0.000");
            textBoxFAI_5_180.Text = Define.GapSL.ToString("0.000");
            textBoxFAI_5_270.Text = Define.GapTL.ToString("0.000");

            textBoxFAI_6_0.Text = Define.OffsetSR.ToString("0.000");
            textBoxFAI_6_90.Text = Define.OffsetTR.ToString("0.000");
            textBoxFAI_6_180.Text = Define.OffsetSL.ToString("0.000");
            textBoxFAI_6_270.Text = Define.OffsetTL.ToString("0.000");

            if (richTextBox.Lines.Length > 400)
            {
                richTextBox.Clear();
            }

            if (bUploadPDCA)
            {
                if (!PingResult)
                {
                    if (label_MacState.Text != "Mac mini 掉线")
                    {
                        label_MacState.Text = "Mac mini 掉线";
                        AOIMethod.ViewImage(pictureBox4, pathpicture + "Alarm.bmp");
                    }
                }
                else
                {
                    if (label_MacState.Text != "Mac mini 上传")
                    {
                        label_MacState.Text = "Mac mini 上传";
                        AOIMethod.ViewImage(pictureBox4, pathpicture + "GreenOn2.bmp");
                    }
                }
            }
            else
            {
                if (label_MacState.Text != "Mac mini 不上传")
                {
                    label_MacState.Text = "Mac mini 不上传";
                    AOIMethod.ViewImage(pictureBox4, pathpicture + "Alarm.bmp");
                }
            }

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
            
        }

        private void RecodeTimeAndCheckPDCA()
        {
            while (true)
            {
                Thread.Sleep(5000);
                try
                {
                    if (DateTime.Now.Hour > int.Parse(myIniFile.IniReadValue("Startup", "Day")) &&
                        DateTime.Now.Hour < int.Parse(myIniFile.IniReadValue("Startup", "Night")))
                    {
                        strTime = DateTime.Now.ToString("yyyy-MM-dd") + "-Day";
                    }
                    else if (DateTime.Now.Hour > int.Parse(myIniFile.IniReadValue("Startup", "Night")))
                    {
                        strTime = DateTime.Now.ToString("yyyy-MM-dd") + "-Night";
                    }
                    else
                    {
                        strTime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "-Night";
                    }

                    if (bUploadPDCA)
                    {
                        AOIMethod.checkmes("169.254.1.10", ref PingResult);
                        Thread.Sleep(2000);
                        if (!PingResult)
                        {
                            AddToQueue("Mac mini断连！", Color.Red);
                            try
                            {
                                macClient.StopConnect();
                            }
                            catch { }
                        }
                        else
                        {
                            bool b_Connect = true;
                            if (macClient != null)
                            {
                                b_Connect = b_Connect && macClient.connectOk;
                                if (macClient.ClientSocket != null)
                                {
                                    b_Connect = b_Connect && macClient.ClientSocket.Connected;
                                }
                                if (b_Connect != true)
                                {
                                    macClient.Open("169.254.1.10", 1111);
                                    AddToQueue("Mac mini尝试重新连接！", Color.Black);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                if (bDelPic)
                {
                    AOIMethod.DeleteOldFiles(imagepath + "CCD1\\", int.Parse(Days.Text.Trim()));
                    AOIMethod.DeleteOldFiles(imagepath + "CCD2\\", int.Parse(Days.Text.Trim()));
                    AOIMethod.DeleteOldFiles(imagepath + "CCD3\\", int.Parse(Days.Text.Trim()));
                    AOIMethod.DeleteOldLog(Application.StartupPath + "\\Log", int.Parse(LogDays.Text.Trim()));
                    AOIMethod.DeleteOldFiles("D:\\Log", int.Parse(Days.Text.Trim()));
                }
            }
        }

        private void CheckRoleAndHandleCOMData()
        {
            while (true)
            {
                Thread.Sleep(50);
                try
                {
                    if (ScanIOCard)
                    {
                        Define.sp1.Write("Cmd_MCU_Sensor_Check\r\n");
                        Thread.Sleep(50);
                    }
                    CheckEmergency();

                    if (LoosenCh.Checked)
                        myIniFile.IniWriteValue("Startup", "bc", "1");
                    else
                        myIniFile.IniWriteValue("Startup", "bc", "0");

                    sp1_DataHandle();
                    if (com232.m_bDataReceived)
                    {
                        sp2_DataHandle();
                    }
                }
                catch
                {
                    //ignore
                }
            }
        }

        #endregion

        #region 软件运行记录

        public void SaveMsg(string msg)
        {
            string str = string.Format(DateTime.Now.ToString("HH:mm:ss") + " : " + msg);
            log.SaveMsgInner(str + "\r\n");
        }

        private void AddToQueue(string message, Color color)
        {
            Dictionary<string, Color> dic = new Dictionary<string, Color>();
            dic.Add(message, color);
            MessageQueue.Enqueue(dic);
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

        #region 机台运行线程

        public void OpenThreads()
        {
            Task msgTask = new Task(ShowMessage);
            msgTask.Start();
            RemoteIOStatus = new Thread(StartRun);
            RemoteIOStatus.IsBackground = true;
            RemoteIOStatus.Start();
            ThreadRunStatus = new Thread(new ThreadStart(RecodeTimeAndCheckPDCA));
            ThreadRunStatus.IsBackground = true;
            ThreadRunStatus.Start();
            ThreadRunIO = new Thread(new ThreadStart(CheckRoleAndHandleCOMData));
            ThreadRunIO.IsBackground = true;
            ThreadRunIO.Start();
        }



        private void ShowMessage()
        {
            while (true)
            {
                Thread.Sleep(1);
                if (MessageQueue.Count > 0)
                {
                    Dictionary<string, Color> dic;
                    MessageQueue.TryDequeue(out dic);
                    richTextBox.BeginInvoke((EventHandler)delegate // new Action
                    {
                        richTextBox.SelectionColor = dic.First().Value;
                        richTextBox.AppendText(DateTime.Now.ToString("MM-dd HH:mm:ss") + "  " + dic.First().Key + Environment.NewLine);
                        richTextBox.ScrollToCaret();
                    });
                }
            }
        }

        private void StartRun()
        {
            while (true)
            {
                Thread.Sleep(10);
                Working();
            }
        }

        private void Working()
        {
            //if (this.labelPassFail1.InvokeRequired)
            //{
            //    FlushClient fc1 = new FlushClient(Working);

            //    try//不加try，未断开连接前关闭软件会出异常
            //    {
            //        this.Invoke(fc1);//通过代理调用刷新方法
            //    }
            //    catch
            //    {

            //    }
            //}
            //else
            {
                if ((Define.BindingOK || bRunEmpty) && Define.DoubleButtonDown && bReturned && !EmgAlarm)
                {
                    if (bRunEmpty) { AddToQueue("当前状态是空跑......", Color.Red); }
                    //ScanIOCard = false;
                    Thread.Sleep(50);
                    Define.DoubleButtonDown = false;
                    check200.Invoke(new MethodInvoker(delegate
                    {
                        if (check200.Checked)
                        {
                            RunToCheck200();
                        }
                        else
                        {
                            Run();
                        }
                    }));

                    if (EmgAlarm) { return; }
                    Define.sp1.Write("Cmd_Off_" + Define.气缸 + "\r\n");//气缸上升   
                    Thread.Sleep(50);
                    while (!com232.StrBack.Contains(Define.气缸.Substring(2, 1) + " Off Pass!"))
                    {
                        Define.sp1.Write("Cmd_Off_" + Define.气缸 + "\r\n");//气缸上升    
                        Thread.Sleep(50);
                    }

                    stop_time_milli = DateTime.Now.Hour * 3600 * 1000 + DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                    label_CT.Invoke(new MethodInvoker(delegate
                    {
                        label_CT.Text = "CT:" + ((stop_time_milli - start_time_milli) / 1000).ToString("0.00") + "S";
                    }));

                    stopTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //if (!bRunEmpty) // 不是空跑则保存并上传图片、生成MES信息
                    {
                        cogRecordDisplay1.Invoke(new MethodInvoker(delegate
                        {
                            SaveAndUploadPicture(Define.SN, cogRecordDisplay1.Image, "CCD1");
                        }));
                        cogRecordDisplay2.Invoke(new MethodInvoker(delegate
                        {
                            SaveAndUploadPicture(Define.SN, cogRecordDisplay2.Image, "CCD2");
                        }));
                        GenerateMESData();
                    }

                    ScanIOCard = true;
                    Thread.Sleep(50);
                    if (Define.SN.Length == intSNLength)
                    {
                        CL++;

                        myIniFile.IniWriteValue("Startup", "LabelCL", CL.ToString());
                        if (Define.GapTR.ToString() != "999" && Define.GapSR.ToString() != "999" && bUploadMES && bUploadPDCA)
                        {
                            macClient.SN = Define.SN;
                            macClient.SendMsg(FoxMes);//上传Mac Mini

                            string str = "";
                            FoxMes = "";
                            if (macClient.ClientSocket.Connected && macClient.connectOk && macClient.TCPStatic)
                            {
                                if (this.pass)//视觉检测通过
                                {
                                    mesMsg = Define.UserName + ";" + Define.SN + ";OK;";
                                    //添加 抽检模式
                                    if (bSampling == false)
                                    {
                                        ConnMES(3, ref mesMsg);//上传MES
                                        if (mesMsg.Contains("NG"))
                                        {
                                            MessageBox.Show("SFC上传信息失败：" + mesMsg);
                                        }
                                    }
                                    else
                                    {
                                        ConnMES(48, ref mesMsg);
                                    }
                                    str = mesMsg;
                                    SaveSNInner(Define.SN + "-P");
                                }
                                else
                                {
                                    mesMsg = Define.UserName + ";" + Define.SN + ";NG;" + failmes;

                                    //添加 抽检模式
                                    if (bSampling == false)
                                    {
                                        ConnMES(3, ref mesMsg);
                                        if (mesMsg.Contains("NG"))
                                        {
                                            MessageBox.Show("SFC上传信息失败：" + mesMsg);
                                        }
                                    }
                                    else
                                    {
                                        ConnMES(48, ref mesMsg);
                                    }
                                    str = mesMsg;
                                    SaveSNInner(Define.SN);
                                }
                                AddToQueue(str, Color.Black);
                                Total();
                            }
                            else
                            {
                                AddToQueue("Mac mini与SFC断连，请重新连接!", Color.Red);
                                labelPassFail.Invoke(new MethodInvoker(delegate
                                {
                                    labelPassFail.Text = "mini Err";
                                    labelPassFail.BackColor = Color.Yellow;
                                }));
                            }
                        }
                    }

                    bGetSN = false;
                    failmes = "";

                    Define.SN = "";
                    SNtxtBox.Invoke(new MethodInvoker(delegate
                    {
                        SNtxtBox.Text = "";
                    }));

                    HoldSN = "";
                    HoldSNtxtBox.Invoke(new MethodInvoker(delegate
                    {
                        HoldSNtxtBox.Text = "";
                    }));
                }
            }
        }

        private void RunToCheck200()
        {
            Define.sp1.Write("Cmd_On_" + Define.气缸 + "\r\n");
            Thread.Sleep(50);
            while (!com232.StrBack.Contains(Define.气缸.Substring(2, 1) + " On Pass!"))
            {
                Define.sp1.Write("Cmd_On_" + Define.气缸 + "\r\n");
                Thread.Sleep(50);
            }
            Thread.Sleep(1500);
            for (int i = 0; i < int.Parse(CorrTextBox.Text); i++)
            {
                SaveAndUploadPicture(Define.SN, cogRecordDisplay1.Image, "CCD1");
                SaveAndUploadPicture(Define.SN, cogRecordDisplay2.Image, "CCD2");
                GenerateMESData();
            }
        }

        private void Run()
        {
            if (check200.Checked)
            { }
            else
            {
                if (EmgAlarm) { return; }
                Define.sp1.Write("Cmd_On_" + Define.气缸 + "\r\n");
                Thread.Sleep(50);
                while (!com232.StrBack.Contains(Define.气缸.Substring(2, 1) + " On Pass!"))
                {
                    Define.sp1.Write("Cmd_On_" + Define.气缸 + "\r\n");
                    Thread.Sleep(50);
                }
                if (!bRunEmpty) // 不是空跑则上传Mac mini
                {
                    macClient.SN = Define.SN;
                    macClient.SendMsg("{\r\n" + Define.SN + "@sfc_unit_check\r\n}\r\n");
                }
                Thread.Sleep(1200);
            }

            Define.运行中 = true;
            Define.BindingOK = false;

            if (EmgAlarm) { return; }
            VppRunFlow();
            for (int i = 0; i < motion.PointsArray.GetLength(0); i++)
            {
                if (EmgAlarm) { return; }
                if (motion.PointsArray[i, 0] == "1")
                {
                    double pos = Convert.ToDouble(motion.PointsArray[i, 1]);
                    Thread.Sleep(100);
                    motion.SingleMotor.AbsMove(pos, 65);
                    int times = 0;
                    while (Math.Abs(motion.SingleMotor.GetPosition() - pos) > 0.1)
                    {
                        Thread.Sleep(100);
                        if (++times > 15)
                        {
                            AddToQueue("轴运动超时！", Color.Red);
                            break;
                        }
                    }
                    Thread.Sleep(100);

                    if (EmgAlarm) { return; }
                    VppRunFlow();
                }
            }

            //轴回原
            Thread.Sleep(100);
            motion.SingleMotor.AbsMove(0, 65);

            Define.运行中 = false;

        }

        private void VppRunFlow()
        {
            LightInitialize.OPTOpenT();
            Thread.Sleep(10);
            VppRun8();
            LightInitialize.OPTCloseT();

            LightInitialize.OPTOpenS();
            Thread.Sleep(10);
            VppRun9();
            LightInitialize.OPTCloseS();
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

            #endregion

            if (fails == "")
            {
                this.pass = true;
                labelPassFail.Invoke(new MethodInvoker(delegate
                {
                    labelPassFail.Text = "PASS";
                    labelPassFail.BackColor = Color.Green;
                }));
            }
            else
            {
                failmes = failmes.Substring(0, failmes.Length - 1) + ";";
                this.pass = false;
                labelPassFail.Invoke(new MethodInvoker(delegate
                {
                    labelPassFail.Text = "FAIL";
                    labelPassFail.BackColor = Color.Red;
                }));
            }

            if (File.Exists(datapath + DateTime.Now.ToString("yyyy-MM-dd") + "-Data.csv"))
            {
                log.SaveCSV(Define.NumberCSV.ToString() + "," + DateTime.Now.ToString() + "," + Define.SN + "," + (this.pass ? "PASS" : "FAIL") + "," + fails + "," + 
                    Define.GapSR.ToString() + "," + Define.GapTR.ToString() + "," + Define.GapSL.ToString() + "," + Define.GapTL.ToString() + "," + 
                    Define.OffsetSR + "," + Define.OffsetTR + "," + Define.OffsetSL + "," + Define.OffsetTL, datapath + DateTime.Now.ToString("yyyy-MM-dd") + "-Data.csv");//FOffset0, FOffset90, FOffset180, FOffset270, FOffsetMAX;//CCD3测试结果
                Define.NumberCSV++;
            }
            else
            {
                Define.NumberCSV = 1;
                string title = "Number,time,SN,Rec,Fail,FAI5-0deg,FAI5-90deg,FAI5-180deg,FAI5-270deg,FAI6-0deg,FAI6-90deg,FAI6-180deg,FAI6-270deg";//,FAI6-0deg-CCD3,FAI6-90deg-CCD3,FAI6-180deg-CCD3,FAI6-270deg-CCD3,FAI6-MAX-CCD3
                log.SaveCSV(title, datapath + DateTime.Now.ToString("yyyy-MM-dd") + "-Data.csv");
                log.SaveCSV(Define.NumberCSV.ToString() + "," + DateTime.Now.ToString() + "," + Define.SN + "," + (this.pass ? "PASS" : "FAIL") + "," + fails + "," + 
                    Define.GapSR.ToString() + "," + Define.GapTR.ToString() + "," + Define.GapSL.ToString() + "," + Define.GapTL.ToString() + "," + 
                    Define.OffsetSR + "," + Define.OffsetTR + "," + Define.OffsetSL + "," + Define.OffsetTL, datapath + DateTime.Now.ToString("yyyy-MM-dd") + "-Data.csv");//FOffset0, FOffset90, FOffset180, FOffset270, FOffsetMAX;//CCD3测试结果
                Define.NumberCSV++;
            }

            if (bUploadPDCA)   //添加判断 是否上传PDCA系统
            {
                ToolDefine.SN = Define.SN;
                ToolDefine.开始时间 = startTime;
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
                ToolDefine.停止时间 = stopTime;
                ToolDefine.版本号 = version.Substring(8, 6);
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
            if (ReadNumOfSN(Define.SN) == 1)
            {
                total++;
                myIniFile.IniWriteValue("Startup", "LabelPD", total.ToString());
                if (this.pass)
                {
                    passtotal++;
                    this.pass = false;
                }
            }
            else if (ReadNumOfSN(Define.SN) == 2)
            {
                totaldata++;
                myIniFile.IniWriteValue("Startup", "PData", totaldata.ToString());
                if (this.pass)
                {
                    if (ReadNumOfSN(Define.SN + "-P") == 1)
                    {
                        passtotal++;
                    }
                }
                else
                {
                    if (ReadNumOfSN(Define.SN + "-P") == 1)
                    {
                        if (passtotal > 0)
                            passtotal--;
                    }
                }
                this.pass = false;
            }
            else if (ReadNumOfSN(Define.SN) == 3)
            {

                if (this.pass)
                {
                    if (ReadNumOfSN(Define.SN + "-P") == 1)//ffp +1/pfp +1/
                    {
                        passtotal++;

                    }
                    else if (ReadNumOfSN(Define.SN + "-P") == 2 && ReadPosOfSN(Define.SN + "-P") < ReadPosOfSN(Define.SN))
                    {
                        passtotal++;
                    }
                }
                else
                {
                    if (ReadNumOfSN(Define.SN + "-P") == 2)//ppf= -1/fpf -1/
                    {
                        if (passtotal > 0)
                            passtotal--;

                    }
                    else if (ReadNumOfSN(Define.SN + "-P") == 1 && ReadPosOfSN(Define.SN + "-P") > ReadPosOfSN(Define.SN))
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

        private void SaveSNInner(string SN)
        {
            log.SaveSNInner(SN + ',', strTime);
        }

        private int ReadNumOfSN(string SN)
        {
            return log.ReadNumOfSN(SN, strTime);
        }

        private int ReadPosOfSN(string str)
        {
            return log.ReadPosOfSN(str, strTime);
        }
        #endregion

        #region 串口通讯配置

        #region 串口1 IO板信息处理

        string ReadIOStatus = "";
        volatile string IOStatu = "1100000";
        bool EmgAlarm = false;

        public void sp1_DataHandle()
        {
            if (com232.bIOOpened && com232.m_bIOReceived)
            {
                com232.m_bIOReceived = false;
                if (com232.StrBack.Length > 12)
                {
                    ReadIOStatus = com232.StrBack.Substring(0, 12);
                }
                if (AOIMethod.IsNumber(ReadIOStatus) && ReadIOStatus.Substring(5, 7) == "1111111") // 110011111111动点4原点5
                {
                    IOStatu = ReadIOStatus;
                    string start = IOStatu.Substring(0, 3);
                    if (Define.BindingOK || bRunEmpty)
                    {
                        if (start == "000")
                        {
                            //ScanIOCard = false;
                            Define.DoubleButtonDown = true;
                            startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            start_time_milli = DateTime.Now.Hour * 3600 * 1000 + DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
                        }
                        else
                        {
                            Define.DoubleButtonDown = false;
                        }
                    }
                }
            }
        }

        private void CheckEmergency()
        {
            if (com232.StrBack.Substring(2, 1) == "1")
            {
                if (!EmgAlarm)
                {
                    ScanIOCard = false;
                    AddToQueue("急停被按下！", Color.Red);
                    EmgAlarm = true;
                    bReturned = false;

                    ManualBtn.Invoke(new MethodInvoker(delegate
                    {
                        if (ManualBtn.BackColor == Color.Green)
                        {
                            ManualBtn.PerformClick();
                        }
                        ManualBtn.Enabled = false;
                    }));
                    AutoBtn.Invoke(new MethodInvoker(delegate
                    {
                        if (AutoBtn.BackColor == Color.Green)
                        {
                            AutoBtn.PerformClick();
                        }
                        AutoBtn.Enabled = false;
                    }));

                    Define.sp1.Write("Cmd_Off_" + Define.绿灯 + "\r\n");
                    Thread.Sleep(50);
                    Define.sp1.Write("Cmd_On_" + Define.红灯 + "\r\n");
                    Thread.Sleep(50);
                    Define.sp1.Write("Cmd_On_" + Define.蜂鸣 + "\r\n");
                    Thread.Sleep(50);
                    Define.sp1.Write("Cmd_Off_" + Define.绿灯 + "\r\n");
                    Thread.Sleep(50);
                    Define.sp1.Write("Cmd_On_" + Define.蜂鸣 + "\r\n");
                    Thread.Sleep(50);
                    ScanIOCard = true;
                }
            }
            else
            {
                if (EmgAlarm)
                {
                    ScanIOCard = false;
                    EmgAlarm = false;
                    AddToQueue("急停已复位！", Color.Black);
                    ManualBtn.Invoke(new MethodInvoker(delegate { ManualBtn.Enabled = true; }));
                    AutoBtn.Invoke(new MethodInvoker(delegate { AutoBtn.Enabled = true; }));
                    Define.sp1.Write("Cmd_On_" + Define.绿灯 + "\r\n");
                    Thread.Sleep(50);
                    Define.sp1.Write("Cmd_Off_" + Define.红灯 + "\r\n");
                    Thread.Sleep(50);
                    Define.sp1.Write("Cmd_Off_" + Define.蜂鸣 + "\r\n");
                    Thread.Sleep(50);
                    Define.sp1.Write("Cmd_Off_" + Define.红灯 + "\r\n");
                    Thread.Sleep(50);
                    Define.sp1.Write("Cmd_Off_" + Define.蜂鸣 + "\r\n");
                    Thread.Sleep(50);
                    Define.sp1.Write("Cmd_On_" + Define.绿灯 + "\r\n");
                    Thread.Sleep(50);
                    ScanIOCard = true;
                }
            }
        }
        #endregion

        #region 扫码枪信息处理

        bool bGetSN = false;
        string HoldSN = "";
        public void sp2_DataHandle()
        {
            if (Auto && com232.bScanOpened && com232.m_bDataReceived)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    labelPassFail.Text = "WAIT";
                    labelPassFail.BackColor = Color.YellowGreen;
                    com232.m_bDataReceived = false;
                    Define.DoubleButtonDown = false;

                    if (bScanCode)//自动扫码
                    {
                        Thread.Sleep(20);
                        if (com232.strBackSN.Length > 0 && Define.运行中 == false)
                        {
                            if (com232.strBackSN.Length == intSNLength)
                            {
                                Define.SN = com232.strBackSN;
                                //SNtxtBox.Invoke(new MethodInvoker(delegate
                                //{
                                    SNtxtBox.Text = Define.SN;
                                //}));
                                mesMsg = com232.strBackSN + ";";
                                try
                                {
                                    ConnMES(2, ref mesMsg);
                                    if (mesMsg.Contains("OK"))
                                    {
                                        bGetSN = true;
                                    }
                                    else
                                    {
                                        bGetSN = false;
                                        Define.BindingOK = false;
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    label_MesState.Text = "S  F  C 掉线";
                                    MES.SajetTransClose();
                                    AOIMethod.ViewImage(pictureBox5, pathpicture + "Alarm.bmp");
                                    MessageBox.Show(ex.Message);
                                }

                                AddToQueue("SN上传反馈信息：" + mesMsg, Color.Black);
                                if (HoldSN != "")
                                {
                                    mesMsg = Define.UserName + ";" + Define.SN + ";" + HoldSN + ";";
                                    ConnMES(51, ref mesMsg);
                                    if (mesMsg.Contains("OK") || mesMsg.Contains("DUP"))
                                        Define.BindingOK = true;
                                    else
                                        Define.BindingOK = false;
                                    AddToQueue(mesMsg, Color.Black);
                                }
                            }
                            else if (com232.strBackSN.IndexOf("L") == 0 && com232.strBackSN.Length == 6)
                            {
                                HoldSNtxtBox.Text = com232.strBackSN;
                                HoldSN = com232.strBackSN;
                                if (bGetSN)
                                {
                                    mesMsg = Define.UserName + ";" + Define.SN + ";" + HoldSN + ";";
                                    ConnMES(51, ref mesMsg);
                                    if (mesMsg.Contains("OK") || mesMsg.Contains("DUP"))
                                        Define.BindingOK = true;
                                    else
                                        Define.BindingOK = false;
                                    AddToQueue(mesMsg, Color.Black);
                                }
                            }
                            else
                            {
                                Define.SN = com232.strBackSN + DateTime.Now.ToString("HH时mm分ss秒");
                                Define.BindingOK = false;
                                AddToQueue("SN码错误：", Color.Red);
                            }
                        }

                        com232.strBackSN = "";
                    }
                    else  //手动输码
                    {
                        if (SNtxtBox.Text.Length == intSNLength && Define.运行中 == false)
                        {
                            Define.SN = SNtxtBox.Text;

                            //以下两句正常时启用
                            mesMsg = Define.SN + ";";
                            ConnMES(2, ref mesMsg);

                            if (mesMsg.Contains("OK"))
                            {
                                bGetSN = true;
                            }
                            else
                            {
                                bGetSN = false;
                                Define.BindingOK = false;
                            }
                            AddToQueue("SN上传反馈信息：" + mesMsg, Color.Black);
                        }
                        if (HoldSNtxtBox.Text.IndexOf("L") == 0 && HoldSNtxtBox.Text.Length == 6)
                        {
                            HoldSN = HoldSNtxtBox.Text;
                        }
                        Thread.Sleep(50);
                        if (HoldSN.Length == 6 && SNtxtBox.Text.Length == intSNLength && bGetSN)
                        {
                            mesMsg = Define.UserName + ";" + SNtxtBox.Text + ";" + HoldSN + ";";
                            ConnMES(51, ref mesMsg);
                            Define.BindingOK = true;
                            AddToQueue(mesMsg, Color.Black);
                        }
                    }
                }));
            }
            else
            {
                com232.strBackSN = "";
                AddToQueue("设备未启动", Color.Red);
                MessageBox.Show("请先启动自动模式！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                com232.m_bDataReceived = false;
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
            SaveMsg("CCD1拍照完成");
        }

        private void CCD2Btn_Click(object sender, EventArgs e)
        {
            LightInitialize.OPTOpenS();
            Thread.Sleep(100);
            VppRun9();
            LightInitialize.OPTCloseS();
            SaveMsg("CCD2拍照完成");
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
                SaveMsg("CCD1实时结束");
            }
            else
            {
                cogRecordDisplay1.StaticGraphics.Clear();
                cogRecordDisplay1.InteractiveGraphics.Clear();
                cogRecordDisplay1.StartLiveDisplay(Define.CCD[1].Operator, false);
                Video1Btn.BackColor = Color.Green;
                SaveMsg("CCD1实时中");
            }
        }

        private void Video2Btn_Click(object sender, EventArgs e)
        {
            if (this.cogRecordDisplay2.LiveDisplayRunning)
            {
                this.cogRecordDisplay2.StopLiveDisplay();
                Video2Btn.BackColor = Color.Transparent;
                SaveMsg("CCD2实时结束");
            }
            else
            {
                cogRecordDisplay2.StaticGraphics.Clear();
                cogRecordDisplay2.InteractiveGraphics.Clear();
                this.cogRecordDisplay2.StartLiveDisplay(Define.CCD[2].Operator, false);
                Video2Btn.BackColor = Color.Green;
                SaveMsg("CCD2实时中");
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
            Define.sp1.Write("Cmd_On_" + Define.气缸 + "\r\n");
        }

        private void DoorUpBtn_Click(object sender, EventArgs e)
        {
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
            //CameraForm form = new CameraForm(Define.CCD[3], 3);
            //form.ShowDialog();
        }

        private void VP1Btn_Click(object sender, EventArgs e)
        {
            InspectionForm form = new InspectionForm(Define.ToolBlock[1], 1);
            form.ShowDialog();
            SaveMsg("CCD1程序保存成功");
        }

        private void VP2Btn_Click(object sender, EventArgs e)
        {
            InspectionForm form = new InspectionForm(Define.ToolBlock[2], 2);
            form.ShowDialog();
            SaveMsg("CCD2程序保存成功");
        }

        private void VP3Btn_Click(object sender, EventArgs e)
        {
            //InspectionForm form = new InspectionForm(Define.ToolBlock[3], 3);
            //form.ShowDialog();
            //ShowMsg2("CCD3程序保存成功");
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
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + strTime + ".txt"))
            {
                if (log.SNFileNum.Length > File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + strTime + ".txt").Length)
                {
                    SaveSNInner(log.SNFileNum);
                }
            }
            myIniFile.IniWriteValue("Startup", "Statue", "1");//复位软件状态
            LightInitialize.OPTCloseT();
            LightInitialize.OPTCloseS();
            try
            {
                macClient.StopConnect();
                Timer_FlashValue.Stop();

                Define.sp1.Write("Cmd_Off_" + Define.红灯 + "\r\n");
                Thread.Sleep(30);
                Define.sp1.Write("Cmd_Off_" + Define.黄灯 + "\r\n");
                Thread.Sleep(30);
                Define.sp1.Write("Cmd_Off_" + Define.绿灯 + "\r\n");
                Thread.Sleep(30);
                Define.sp1.Write("Cmd_Off_" + Define.蜂鸣 + "\r\n");
                Define.sp1.Close();
                Define.sp2.Close();
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
                SaveMsg("相机释放失败");
            }

            foreach (Form mdiChild in this.MdiChildren)
            {
                mdiChild.Close();
            }
            Close();
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
                {
                    ManualBtn.PerformClick();
                }
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

        private async void AutoBtn_ClickAsync(object sender, EventArgs e)
        {
            if (!bReturned)
            {
                if (DialogResult.OK != MessageBox.Show("气缸和轴即将开始回原", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
                {
                    return;
                }
                var taskHomingAsync = MachineHomingAsync();
                bReturned = await taskHomingAsync;
                if (bReturned)
                {
                    AddToQueue("回原完成！", Color.Black);
                }
                else
                {
                    AddToQueue("回原失败！", Color.Red);
                }
            }

            if (AutoBtn.BackColor == Color.LightGray && bReturned)
            {
                Define.UserName = OPtextbox.Text;
                mesMsg = Define.UserName + ";";
                ConnMES(1, ref mesMsg);
                string str = mesMsg;// client.command_code_for_webservice("192.168.112.161", 1, OPtextbox.Text + ";");

                if (str.Contains("OK"))
                {
                    OPtextbox.Enabled = false;
                    Auto = true;
                    ManualBtn.Enabled = false;
                    AutoBtn.BackColor = Color.Green;
                    AutoBtn.Text = "停止自动";
                    AddToQueue("自动模式已开启！", Color.Black);
                    AddToQueue("上传OP ID反馈：" + str, Color.Black);
                }
                else
                {
                    MessageBox.Show("请输入员工ID！");
                }
            }
            else
            {
                OPtextbox.Enabled = true;
                Auto = false;
                ManualBtn.Enabled = true;
                AutoBtn.BackColor = Color.LightGray;
                AutoBtn.Text = "自动模式";
                AddToQueue("自动模式已关闭！", Color.Black);
            }
            Manual = false;
            ManualBtn.BackColor = Color.LightGray;
        }

        private async Task<bool> MachineHomingAsync()
        {
            bool result = false;
            await Task.Run(() =>
            {
                result = motion.SingleMotor.DoHome();
                if (!result)
                {
                    AddToQueue("轴回原失败！", Color.Red);
                    MessageBox.Show("轴回原失败！");
                }

                while (result && motion.SingleMotor.GetPosition() != 0)
                {
                    Thread.Sleep(500);
                }

                Define.sp1.Write("Cmd_MCU_Sensor_Check\r\n");
                Thread.Sleep(100);
                if (com232.StrBack == string.Empty || com232.StrBack[3] == com232.StrBack[4])
                {
                    result = false;
                    AddToQueue("气缸回原失败！", Color.Red);
                    MessageBox.Show("气缸回原失败！");
                }
                else if (com232.StrBack[3] == '1')
                {
                    Define.sp1.Write("Cmd_Off_" + Define.气缸 + "\r\n");
                    Thread.Sleep(50);
                    while (!com232.StrBack.Contains(Define.气缸.Substring(2, 1) + " Off Pass!"))
                    {
                        Define.sp1.Write("Cmd_Off_" + Define.气缸 + "\r\n");
                        Thread.Sleep(50);
                    }

                    Thread.Sleep(2500);
                    do
                    {
                        Define.sp1.Write("Cmd_MCU_Sensor_Check\r\n");
                        Thread.Sleep(50);
                    } while (com232.StrBack[4] != '1');
                }
            });

            return result;
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            myIniFile.IniWriteValue("Startup", "LabelCL", "0");
        }

        private void Production_Click(object sender, EventArgs e)
        {
            string str = "截止到" + DateTime.Now.ToString() + "\t共检测产品:" + total + "支," + "Pass支数:" + passtotal + ",复检率:" + totaldatalab.Text + "\r\n";//",Retry:" + label41.Text + 
            log.SaveTotalProduct("汇总", str);
            myIniFile.IniWriteValue("Startup", "LabelPD", "0");
            myIniFile.IniWriteValue("Startup", "PData", "0");
            myIniFile.IniWriteValue("Startup", "passto", "0");
            CLDataLB.Text = "0%";
            totallab.Text = "0";
            totaldatalab.Text = "0%";
            passnum.Text = "0";
            ngnum.Text = "0";
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + strTime + ".txt"))
            {
                File.Copy(System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + strTime + ".txt",
                    System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + strTime + "-" + DateTime.Now.ToString("HH-mm-ss") + ".txt");
                File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "Log\\SN-" + strTime + ".txt");
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
                    AddToQueue("手动模式已开启！", Color.Black);
                }
                else
                {
                    Manual = false;
                    AutoBtn.Enabled = true;
                    ManualBtn.BackColor = Color.LightGray;
                    ManualBtn.Text = "手动模式";
                    AddToQueue("手动模式已关闭！", Color.Black);
                }
                Auto = false;
                AutoBtn.BackColor = Color.LightGray;
            }
            else
            {
                MessageBox.Show("您的权限不能使用此功能！");
            }
        }

        private void SaveAndUploadPicture(string sn, ICogImage image, string CCDNo)
        {
            int width = 0, height = 0;
            double size = 0;
            Bitmap myImage = image.ToBitmap();
            string directory = imagepath + CCDNo + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
            string path = @"\\169.254.1.10\Public\blobs\" + sn + "//";

            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                if (myImage != null)
                {
                    myImage.Save(directory + sn + ".jpg");

                    SaveMsg(CCDNo + "-SN:" + sn + "存图成功");
                    if (sn.Length == intSNLength && Define.GapTR.ToString() != "999" && Define.GapSR.ToString() != "999" && bUploadPDCA) //添加判断 是否上传PDCA系统                        
                    {//当SN正常，且检测数据正常，则保持图片至Mac mini 
                        Directory.CreateDirectory(path);
                        AOIMethod.VaryQualityLevel(directory + sn + ".jpg", path + sn + "-" + CCDNo + ".jpg", ref width, ref height, ref size);
                        SaveMsg(CCDNo + "-SN:" + sn + "-压缩并共享至Mac mini成功!" + "压缩后尺寸(像素)：" + height + " X " + width + "," + "占用空间：" + size.ToString() + "KB");
                    }
                }
                else
                {
                    SaveMsg(CCDNo + "-SN:" + sn + "没有图像！");
                }
            }
            catch
            {
                SaveMsg(CCDNo + "-SN:" + sn + "存图或上传图片异常！");
                AddToQueue(CCDNo + "-SN:" + sn + "存图或上传图片异常！", Color.Red);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MES.SajetTransClose();
            //LightInitialize.MRCloseF(myClient1, richTextBox1);
            LightInitialize.OPTCloseT();
            LightInitialize.OPTCloseS();
            myIniFile.IniWriteValue("Startup", "Statue", "1");//复位软件状态
            Define.sp1.Write("Cmd_Off_" + Define.红灯 + "\r\n");
            Define.sp1.Write("Cmd_Off_" + Define.黄灯 + "\r\n");
            Define.sp1.Write("Cmd_Off_" + Define.绿灯 + "\r\n");
            Define.sp1.Write("Cmd_Off_" + Define.蜂鸣 + "\r\n");
            Define.sp1.Close();
            Define.sp2.Close();
            motion.SingleMotor.SetSevON(false);
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
                AutoClear.BackColor = Color.LawnGreen;
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

        private void button_Sampling_Click(object sender, EventArgs e)//抽检模式
        {
            if (bSampling == false)
            {
                bSampling = true;
                button_Sampling.BackColor = Color.LawnGreen;
                button_Sampling.Text = "抽检模式";
            }
            else
            {
                bSampling = false;
                button_Sampling.BackColor = Color.LightGray;
                button_Sampling.Text = "正常模式";
            }
        }

        /// <summary>
        /// 上传PDCA （MES系统）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_PDCA_Click(object sender, EventArgs e)
        {
            if (bUploadPDCA)
            {
                button_PDCA.BackColor = Color.LightGray;
                button_PDCA.Text = "启用PDCA";
                label_MacState.Text = "Mac mini 上传";
                AOIMethod.ViewImage(pictureBox4, pathpicture + "Alarm.bmp");
                bUploadPDCA = false;
            }
            else
            {
                button_PDCA.BackColor = Color.LawnGreen;
                button_PDCA.Text = "禁用PDCA";
                label_MacState.Text = "Mac mini 不上传";
                AOIMethod.ViewImage(pictureBox4, pathpicture + "GreenOn2.bmp");
                bUploadPDCA = true;
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            bool a = int.TryParse(SNLength.Text, out intSNLength);
            if (!a)
                MessageBox.Show("SN长度设置异常！");
            else
                myIniFile.IniWriteValue("功能", "SN长度", SNLength.Text);

            int m = 0;
            bool b = int.TryParse(Days.Text, out m);
            if (!b)
                MessageBox.Show("图片保存天数设置异常！");
            else
                myIniFile.IniWriteValue("功能", "保存天数", Days.Text);

            int n = 0;
            bool c = int.TryParse(LogDays.Text, out n);
            if (!c)
                MessageBox.Show("Log保存天数设置异常！");
            else
                myIniFile.IniWriteValue("功能", "Log保存天数", LogDays.Text);
        }

        private void DeletePhoto_Click(object sender, EventArgs e)
        {
            if (!bDelPic)
            {
                bDelPic = true;
                myIniFile.IniWriteValue("Startup", "DelPic", "1");
                DeletePhoto.BackColor = Color.LawnGreen;
            }
            else
            {
                bDelPic = false;
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
                bScanCode = true;
            }
            else
            {
                SNInput.BackColor = Color.LawnGreen;
                SNInput.Text = "手动输码";
                bScanCode = false;
            }
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

        private void Button_OpenAxisForm_Click(object sender, EventArgs e)
        {
            AxisForm form = new AxisForm(motion, myIniFile);
            form.Timer.Start();
            form.Show();
        }

        #endregion

        #region  连接SFC
        private void OpenPhotoBtn_Click_1(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"E:\Image");
        }

        private void button_RunEmpty_Click(object sender, EventArgs e)
        {
            bRunEmpty = !bRunEmpty;
            button_RunEmpty.Text = bRunEmpty ? "停止空跑" : "启用空跑";
            button_RunEmpty.BackColor = bRunEmpty ? Color.LawnGreen : Color.LightGray;
        }

        private void button_MES_Click(object sender, EventArgs e)
        {
            bUploadMES = !bUploadMES;
            button_MES.Text = bUploadMES ? "禁用MES" : "启用MES";
            button_MES.BackColor = bUploadMES ? Color.LawnGreen : Color.LightGray;
            label_MesState.Text = bUploadMES ? "S  F  C 上传" : "S  F  C 不上传";
            AOIMethod.ViewImage(pictureBox5, pathpicture + (bUploadMES ? "GreenOn2.bmp" : "Alarm.bmp"));
            bool temp = bUploadMES ? MES.SajetTransStart() : MES.SajetTransClose();
        }

        string mesMsg = "";

        private void ConnMES(short cmd, ref string value)
        {
            if (!bUploadMES)
            {
                return;
            }

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
