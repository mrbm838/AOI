using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using ToolTotal;

namespace MyTools
{
    public partial class InspectionForm : Form
    {
        int id;
        Log savelog = new Log();

        /// <summary>
        /// 加载VPP
        /// </summary>
        /// <param name="MT">vpp</param>
        /// <param name="id">vpp ID</param>
        public InspectionForm(CogToolBlock MT, int id)
        {
            InitializeComponent();
            cogToolBlockEditV21.Subject = MT;
            // cogToolBlockEditV21.ActiveControl.ContextMenuStrip.Visible = false;
            this.id = id;
        }

        /// <summary>
        /// 存储
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_btn_Click(object sender, EventArgs e)
        {
            cogToolBlockEditV21.LocalDisplayVisible = false;
            CogToolBlock MT = cogToolBlockEditV21.Subject;
            switch (id)
            {
                ///注意存储路径名字以及ID

                case 0:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Inspections\\1113.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save TopAuto2\r\n");
                    break;
                case 1:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Inspections\\TipAOITopCamera.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save TipAOITopCamera.vpp\r\n");
                    break;
                case 2:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Inspections\\TipAOISideCamera.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save TipAOISideCamera.vpp\r\n");
                    break;
                case 3:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Inspections\\TipAOIFrontCamera.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save TipAOIFrontCamera.vpp\r\n");
                    break;
                case 4:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Inspections\\Inspection_5.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save Inspection_5.vpp\r\n");
                    break;
                case 5:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Inspections\\Inspection_6.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save Inspection_6.vpp\r\n");
                    break;
                case 6:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Inspections\\Inspection_7.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save Inspection_7.vpp\r\n");
                    break;
                case 10:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Calibrations\\TopCalibration.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save TopCalibration.vpp\r\n");
                    break;
                case 11:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Calibrations\\BottomCalibration.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save BottomCalibration.vpp\r\n");
                    break;
                case 12:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Calibrations\\Calibrations_3.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save Calibrations_3.vpp\r\n");
                    break;
                case 13:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Calibrations\\Calibrations_4.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save Calibrations_4.vpp\r\n");
                    break;
                case 14:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Calibrations\\Calibrations_5.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save Calibrations_5.vpp\r\n");
                    break;
                case 15:
                    CogSerializer.SaveObjectToFile(MT, Application.StartupPath + "\\Calibrations\\Calibrations_6.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save Calibrations_6.vpp\r\n");
                    break;
            }
            //this.Close();
            GC.Collect();
            Dispose(true);

        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InspectionForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //cogToolBlockEditV21.LocalDisplayVisible = false;
            switch (id)
            {
                ///注意存储路径名字以及ID

                case 0:
                    Define.ToolBlock[0] = cogToolBlockEditV21.Subject;
                    break;
                case 1:
                    Define.ToolBlock[1] = cogToolBlockEditV21.Subject;
                    break;
                case 2:
                    Define.ToolBlock[2] = cogToolBlockEditV21.Subject;
                    break;
                case 3:
                    Define.ToolBlock[3] = cogToolBlockEditV21.Subject;
                    break;
                case 4:
                    Define.ToolBlock[4] = cogToolBlockEditV21.Subject;
                    break;
                case 5:
                    Define.ToolBlock[5] = cogToolBlockEditV21.Subject;
                    break;
                case 6:
                    Define.ToolBlock[6] = cogToolBlockEditV21.Subject;
                    break;
                case 10:
                    Define.ToolBlock[10] = cogToolBlockEditV21.Subject;
                    break;
                case 11:
                    Define.ToolBlock[11] = cogToolBlockEditV21.Subject;
                    break;
                case 12:
                    Define.ToolBlock[12] = cogToolBlockEditV21.Subject;
                    break;
                case 13:
                    Define.ToolBlock[13] = cogToolBlockEditV21.Subject;
                    break;
                case 14:
                    Define.ToolBlock[14] = cogToolBlockEditV21.Subject;
                    break;
                case 15:
                    Define.ToolBlock[15] = cogToolBlockEditV21.Subject;
                    break;
            }

            GC.Collect();
            Dispose(true);
        }


    }
}
