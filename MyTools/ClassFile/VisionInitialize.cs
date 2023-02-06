using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyTools
{
    class VisionInitialize
    {
        public static void LoadVPP(MainForm form, RichTextBox richTextBox)
        {
            try
            {
                form.startForm.showBar(30);
                Define.ToolBlock[1] = (CogToolBlock)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\Inspections\\TipAOITopCamera.vpp");
                form.startForm.showBar(50);
                Define.ToolBlock[2] = (CogToolBlock)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\Inspections\\TipAOISideCamera.vpp");
                form.startForm.showBar(70);
                Define.ToolBlock[3] = (CogToolBlock)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\Inspections\\TipAOIFrontCamera.vpp");
                form.startForm.showBar(80);

                Define.CCD[1] = (CogAcqFifoTool)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\AcqFifoTool\\CCD1AcqFifoTool.vpp");
                //CCD1LightOFF.Visible = false;
                form.startForm.showBar(90);
                Define.CCD[2] = (CogAcqFifoTool)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\AcqFifoTool\\CCD2AcqFifoTool.vpp");
                //CCD2LightOFF.Visible = false;
                form.startForm.showBar(95);
                Define.CCD[3] = (CogAcqFifoTool)CogSerializer.LoadObjectFromFile(Application.StartupPath + "\\AcqFifoTool\\CCD3AcqFifoTool.vpp");
                //CCD3LightOFF.Visible = false;
                form.startForm.showBar(100);
            }

            catch
            {
                richTextBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   " + "相机程序加载失败！" + "\r\n");
                richTextBox.ScrollToCaret();
                form.ShowMsg1("相机程序加载失败");
            }
        }
    }
}
