using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cowain_AutoMachine.Flow.IO_Cylinder_Motor;
using MotionBase;
using Cowain_AutoMachine.Flow.IO_Cylinder;
using Cowain_Form.FormView;
using ToolTotal;

namespace SingleAxisMotion
{
    public class Motion : Base
    {
        //public clsMotors clsMotor;
        //public clsIO_Ports m_IO_Port;
        public readonly DrvMotor SingleMotor = null;
        public bool IsMotorServoOn;
        public readonly string[,] PointsArray = new string[3, 2];

        public Motion(IniFile myIniFile)
        {
            PointsArray[0,0] = myIniFile.IniReadValue("Axis", "PointF_Enable");
            PointsArray[0,1] = myIniFile.IniReadValue("Axis", "PointF_Pos");
            PointsArray[1,0] = myIniFile.IniReadValue("Axis", "PointS_Enable");
            PointsArray[1,1] = myIniFile.IniReadValue("Axis", "PointS_Pos");
            PointsArray[2,0] = myIniFile.IniReadValue("Axis", "PointT_Enable");
            PointsArray[2,1] = myIniFile.IniReadValue("Axis", "PointT_Pos");

            m_strMachinePath = Environment.CurrentDirectory + "\\Machine.mdb";
            clsMotors clsMotor = new clsMotors(this, 0, m_strMachinePath, "", 2000);
            AddBase(ref clsMotor.m_NowAddress);
            //m_IO_Port = new clsIO_Ports(this, 0, m_strMachinePath, "", 2000);
            //AddBase(ref m_IO_Port.m_NowAddress);

            StartInitial();

            SingleMotor = clsMotors.MotorList["M01_00"];
        }

        public enum enInitStep
        {
            StartLoading,
            載入Machine_Data,
            載入Work_Data,
            系統資料Init,
            系統Init完成,
            enMax,
        }
    }
}
