using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolTotal;

namespace MyTools
{
    public class SerialPort_232
    {
        #region 串口1 IO板
        public void loadSerialPort1(MainForm form, RichTextBox richTextBox)
        {
            Define.PortName[0] = form.myIniFile.IniReadValue("COM Port1", "COM Port");
            Define.PortParity[0] = form.myIniFile.IniReadValue("COM Port1", "Parity");
            Define.PortStopBits[0] = form.myIniFile.IniReadValue("COM Port1", "StopBits");
            Define.PortBaudRate[0] = int.Parse(form.myIniFile.IniReadValue("COM Port1", "BaudRate"));
            Define.PortDataBits[0] = int.Parse(form.myIniFile.IniReadValue("COM Port1", "DataBits"));

            Define.sp1.PortName = Define.PortName[0];//名字
            //Define.sp1.Parity = (Parity)Enum.Parse(typeof(Parity), Define.PortParity[0]);
            if (Define.PortParity[0] == "NONE")  //校验位
            {
                Define.sp1.Parity = System.IO.Ports.Parity.None;
            }
            if (Define.PortParity[0] == "Mark")
            {
                Define.sp1.Parity = System.IO.Ports.Parity.Mark;
            }
            if (Define.PortParity[0] == "Even")
            {
                Define.sp1.Parity = System.IO.Ports.Parity.Even;//偶数
            }

            if (Define.PortParity[0] == "Odd")
            {
                Define.sp1.Parity = System.IO.Ports.Parity.Odd;//奇数
            }
            if (Define.PortParity[0] == "Space")
            {
                Define.sp1.Parity = System.IO.Ports.Parity.Space;
            }

            if (Define.PortStopBits[0] == "0")//停止位
            {
                Define.sp1.StopBits = System.IO.Ports.StopBits.None;
            }
            if (Define.PortStopBits[0] == "1")
            {
                Define.sp1.StopBits = System.IO.Ports.StopBits.One;
            }
            if (Define.PortStopBits[0] == "1.5")
            {
                Define.sp1.StopBits = System.IO.Ports.StopBits.OnePointFive;
            }
            if (Define.PortStopBits[0] == "2")
            {
                Define.sp1.StopBits = System.IO.Ports.StopBits.Two;
            }

            Define.sp1.BaudRate = Define.PortBaudRate[0];
            Define.sp1.DataBits = Define.PortDataBits[0];

            Define.sp1.DataReceived += sp1_DataReceived;//添加数据接收事件
                                                        //打开端口
            try
            {
                if (!Define.sp1.IsOpen)
                    Define.sp1.Open();
                bIOOpened = true;
                Define.sp1.Write("Cmd_On_" + Define.绿灯 + "\r\n");
                Thread.Sleep(100);
                Define.sp1.Write("Cmd_Off_" + Define.蜂鸣 + "\r\n");
                Thread.Sleep(100);
                Define.sp1.Write("Cmd_Off_" + Define.黄灯 + "\r\n");
                Thread.Sleep(100);
                Define.sp1.Write("Cmd_Off_" + Define.红灯 + "\r\n");
                Thread.Sleep(100);
                Define.StartButtonDouble = false;
                Define.sp1.Write("Cmd_Off_" + Define.气缸 + "\r\n");
                //ThreadRunIO.Start();
            }
            catch (Exception)
            {
                bIOOpened = false;
                richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + Define.PortName[0] + "端口不存在或者被占用" + "\r\n");
            }

        }

        string ReadIOStatu = "1100000";
        bool Rlarm = false;
        bool qigang = false;
        public string StrBack = "";
        public bool m_bIOReceived = false;
        public bool bIOOpened = false;
        public void sp1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(20);
            string str = Define.sp1.ReadExisting();
            Define.sp1.DiscardInBuffer();//释放串口数据缓存             
            StrBack = str;
            m_bIOReceived = true;
        }

        #endregion


        #region 串口2 扫码枪

        public void loadSerialPort2(MainForm form, RichTextBox richTextBox)
        {
            Define.PortName[1] = form.myIniFile.IniReadValue("COM Port2", "COM Port");
            Define.PortParity[1] = form.myIniFile.IniReadValue("COM Port2", "Parity");
            Define.PortStopBits[1] = form.myIniFile.IniReadValue("COM Port2", "StopBits");
            Define.PortBaudRate[1] = int.Parse(form.myIniFile.IniReadValue("COM Port2", "BaudRate"));
            Define.PortDataBits[1] = int.Parse(form.myIniFile.IniReadValue("COM Port2", "DataBits"));

            Define.sp2.PortName = Define.PortName[1];//名字

            if (Define.PortParity[1] == "NONE")  //校验位
            {
                Define.sp2.Parity = System.IO.Ports.Parity.None;
            }
            if (Define.PortParity[1] == "Mark")
            {
                Define.sp2.Parity = System.IO.Ports.Parity.Mark;
            }
            if (Define.PortParity[1] == "Even")
            {
                Define.sp2.Parity = System.IO.Ports.Parity.Even;//偶数
            }

            if (Define.PortParity[1] == "Odd")
            {
                Define.sp2.Parity = System.IO.Ports.Parity.Odd;//奇数
            }
            if (Define.PortParity[1] == "Space")
            {
                Define.sp2.Parity = System.IO.Ports.Parity.Space;
            }

            if (Define.PortStopBits[1] == "0")//停止位
            {
                Define.sp2.StopBits = System.IO.Ports.StopBits.None;
            }
            if (Define.PortStopBits[1] == "1")
            {
                Define.sp2.StopBits = System.IO.Ports.StopBits.One;
            }
            if (Define.PortStopBits[1] == "1.5")
            {
                Define.sp2.StopBits = System.IO.Ports.StopBits.OnePointFive;
            }
            if (Define.PortStopBits[1] == "2")
            {
                Define.sp2.StopBits = System.IO.Ports.StopBits.Two;
            }

            Define.sp2.BaudRate = Define.PortBaudRate[1];
            Define.sp2.DataBits = Define.PortDataBits[1];

            Define.sp2.DataReceived += sp2_DataReceived;//添加数据接收事件
                                                        //打开端口
            try
            {
                if (!Define.sp2.IsOpen)
                    Define.sp2.Open();
                bScanOpened = true;
            }
            catch (Exception)
            {
                bScanOpened = false;
                richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + Define.PortName[1] + "端口不存在或者被占用" + "\r\n");
            }
        }
        public string strBackSN = "";
        public bool m_bDataReceived = false;
        public bool bScanOpened = false;
        public void sp2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(20);
            string str2 = Define.sp2.ReadExisting();
            Define.sp2.DiscardInBuffer();//释放串口数据缓存
            strBackSN = str2;
            m_bDataReceived = true;

        }
        #endregion
    }
}
