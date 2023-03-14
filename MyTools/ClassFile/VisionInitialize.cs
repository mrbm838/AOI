using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyTools
{
    class VisionInitialize
    {
        public static void LoadVPP(StartForm form)
        {
            try
            {
                form.BarValue = 30;
                Define.ToolBlock[1] = (CogToolBlock)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\Inspections\\TipAOITopCamera.vpp");
                form.BarValue = 60;
                Define.ToolBlock[2] = (CogToolBlock)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\Inspections\\TipAOISideCamera.vpp");
                form.BarValue = 80;
                //Define.ToolBlock[3] = (CogToolBlock)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\Inspections\\TipAOIFrontCamera.vpp");
                //form.BarValue = 80);

                Define.CCD[1] = (CogAcqFifoTool)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\AcqFifoTool\\CCD1AcqFifoTool.vpp");
                //CCD1LightOFF.Visible = false;
                form.BarValue = 90;
                Thread.Sleep(500);
                Define.CCD[2] = (CogAcqFifoTool)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\AcqFifoTool\\CCD2AcqFifoTool.vpp");
                //CCD2LightOFF.Visible = false;
                //form.BarValue = 80);
                //Define.CCD[3] = (CogAcqFifoTool)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\AcqFifoTool\\CCD3AcqFifoTool.vpp");
                //CCD3LightOFF.Visible = false;
                form.BarValue = 100;
                Thread.Sleep(500);
                MainForm.LoadVppSuccess = true;
            }
            catch
            {
                form.BarValue = 100;
                MainForm.LoadVppSuccess = false;
            }
        }
    }
}
