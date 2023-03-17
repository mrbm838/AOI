using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro;
using System.IO.Ports;

namespace MyTools
{
    class Define
    {
        public static CogToolBlock[] ToolBlock = new CogToolBlock[20];//图像处理工具
        public static CogAcqFifoTool[] CCD = new CogAcqFifoTool[10];//取图工具
        public static bool trigger;
        public static bool b_Camera1 = false;//相机通讯状态
        public static bool b_Camera2 = false;
        public static bool b_Sever1 = false;//服务器通讯状态
        public static bool b_Sever2 = false;
        public static bool b_Client1 = false;//客户端通讯状态
        public static bool b_Client2 = false;
        public static string UserName;//用户名
        public static string imagepath = "D:" + "\\" + "RawSave";

        /// <summary>
        /// IO串口
        /// </summary>
        public static SerialPort sp1 = new SerialPort();
        /// <summary>
        /// 扫码枪串口
        /// </summary>
        public static SerialPort sp2 = new SerialPort();
        public static string[] PortName = new string[2];
        public static int[] PortBaudRate = new int[2];// 600,1200,2400,4800,9600,19200,38400,115200
        public static string[] PortStopBits = new string[2];//1,1.5,2
        public static string[] PortParity = new string[2];//Parity.None,Parity.Odd(奇校验)，Parity.Even（偶）
        public static int[] PortDataBits = new int[2];

        public static bool BindingOK;
        public static bool Initialize = false;
        public static bool ButtonDisabled = false;
        public static bool DoubleButtonDown;
        public static bool 运行中;
        public static bool LimintChange = false;

        public static double GapTL = 0, GapTR = 0, GapSL = 0, GapSR = 0, OffsetTL = 0, OffsetTR = 0, OffsetSL = 0, OffsetSR = 0;//测试结果
        public static double FOffset0 = 0, FOffset90 = 0, FOffset180 = 0, FOffset270 = 0, FOffsetMAX = 0;//CCD3测试结果
        public static double[] TopSettingS = new double[12];
        public static double[] SideSettingS = new double[12];
        public static double[] FrontSettingS = new double[14];
        public static string SN = string.Empty;
        public static double SNNum;
        public static int NumberCSV = 1;

        //补偿值
        public static double FAI6_270;

        public static bool Function_UploadMES;//上传MES
        public static bool Function_SN3Times;//SN3次限制
        public static bool Function_YieldCleared;//产量每日清零
        public static bool Function_SaveOriginalPhoto;//保存原图
        public static bool Function_SaveScreenshot;//保存截图
        public static bool Function_DeletePhotosAutomatically;//自动删图
        public static double Function_SavePhotoDays;//原图保存天数
        public static double Function_SaveScreenshotDays;//截图保存天数

        //MR RGB光源亮度
        public static int MRRed;
        public static int MRGreen;
        public static int MRBlue;

        public static string 气缸 = "cy8";
        public static string 红灯 = "cy4";
        public static string 黄灯 = "cy5";
        public static string 绿灯 = "cy6";
        public static string 蜂鸣 = "cy7";
    }
}
