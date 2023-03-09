using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyTools
{
    class ParamInitialize
    {
        #region 读运行参数
        public static  void ReadSettings(MainForm form,TextBox Days,TextBox LogDays)
        {
            Define.TopSettingS[0] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "TopSetting0"));
            Define.TopSettingS[1] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "TopSetting1"));
            Define.TopSettingS[2] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "TopSetting2"));
            //Define.TopSettingS[3] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "TopSetting3"));
            //Define.TopSettingS[4] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "TopSetting4"));
            //Define.TopSettingS[5] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "TopSetting5"));
            //Define.TopSettingS[6] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "TopSetting6"));
            //Define.TopSettingS[7] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "TopSetting7"));
            //Define.TopSettingS[8] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "TopSetting8"));
            Define.TopSettingS[9] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "TopSetting9"));
            //Define.TopSettingS[10] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "TopSetting10"));
            Define.TopSettingS[11] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "TopSetting11"));

            Define.SideSettingS[0] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "SideSetting0"));
            Define.SideSettingS[1] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "SideSetting1"));
            Define.SideSettingS[2] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "SideSetting2"));
            //Define.SideSettingS[3] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "SideSetting3"));
            //Define.SideSettingS[4] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "SideSetting4"));
            //Define.SideSettingS[5] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "SideSetting5"));
            //Define.SideSettingS[6] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "SideSetting6"));
            //Define.SideSettingS[7] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "SideSetting7"));
            //Define.SideSettingS[8] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "SideSetting8"));
            Define.SideSettingS[9] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "SideSetting9"));
            //Define.SideSettingS[10] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "SideSetting10"));
            Define.SideSettingS[11] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "SideSetting11"));

            Define.FrontSettingS[0] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "FrontSetting0"));
            //Define.FrontSettingS[1] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "FrontSetting1"));
            Define.FrontSettingS[2] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "FrontSetting2"));
            //Define.FrontSettingS[3] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "FrontSetting3"));
            //Define.FrontSettingS[4] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "FrontSetting4"));
            //Define.FrontSettingS[5] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "FrontSetting5"));
            //Define.FrontSettingS[6] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "FrontSetting6"));
            //Define.FrontSettingS[7] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "FrontSetting7"));
            //Define.FrontSettingS[8] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "FrontSetting8"));
            //Define.FrontSettingS[9] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "FrontSetting9"));
            //Define.FrontSettingS[10] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "FrontSetting10"));
            //Define.FrontSettingS[11] = Convert.ToDouble(myIniFile.IniReadValue("TestSettings", "FrontSetting11"));
            Define.FrontSettingS[12] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "FrontSetting12"));
            Define.FrontSettingS[13] = Convert.ToDouble(form.myIniFile.IniReadValue("TestSettings", "FrontSetting13"));

            Days.Text = form.myIniFile.IniReadValue("功能", "保存天数");
            LogDays.Text = form.myIniFile.IniReadValue("功能", "Log保存天数");
            form.imagepath = form.myIniFile.IniReadValue("Save Image", "Directory");
            form.datapath = form.myIniFile.IniReadValue("Save Image", "DataDirectory");

            Define.气缸 = form.myIniFile.IniReadValue("IO地址", "气缸").ToString();
            Define.红灯 = form.myIniFile.IniReadValue("IO地址", "红灯").ToString();
            Define.黄灯 = form.myIniFile.IniReadValue("IO地址", "黄灯").ToString();
            Define.绿灯 = form.myIniFile.IniReadValue("IO地址", "绿灯").ToString();
            Define.蜂鸣 = form.myIniFile.IniReadValue("IO地址", "蜂鸣").ToString();



            if (form.com232.bIOOpened)
            {
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
            }
        }
        #endregion
    }
}
