using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cowain_AutoMachine.Flow.IO_Cylinder_Motor;
using MotionBase;
using Cowain_AutoMachine.Flow.IO_Cylinder;

namespace SingleAxisMotion
{
    public class Motion : Base
    {
        //public clsMotors clsMotor;
        //public clsIO_Ports m_IO_Port;

        public Motion()
        {
            m_strMachinePath = Environment.CurrentDirectory + "\\Machine.mdb";

            clsMotors clsMotor = new clsMotors(this, 0, m_strMachinePath, "", 2000);
            AddBase(ref clsMotor.m_NowAddress);
            //m_IO_Port = new clsIO_Ports(this, 0, m_strMachinePath, "", 2000);
            //AddBase(ref m_IO_Port.m_NowAddress);

            StartInitial();
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
