using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cognex.VisionPro;
using ToolTotal;

namespace MyTools
{
    public partial class CameraForm : Form
    {
        int id;
        Log savelog = new Log();

        public CameraForm(CogAcqFifoTool camera, int id)
        {
            InitializeComponent();
            cogAcqFifoEditV21.Subject.Operator = camera.Operator;    
            this.id = id;
            Define.trigger = false;
        }
         
        private void Save_btn_Click(object sender, EventArgs e)
        {
            cogAcqFifoEditV21.LocalDisplayVisible = false;
            CogAcqFifoTool MT = cogAcqFifoEditV21.Subject;

            switch (id)
            {
                ///注意存储路径名字以及ID

                case 0:
                    CogSerializer.SaveObjectToFile(MT, @".\AcqFifoTool\CCD0AcqFifoTool.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save CCD0AcqFifoTool.vpp\r\n");
                    break;
                case 1:
                    CogSerializer.SaveObjectToFile(MT, @".\AcqFifoTool\CCD1AcqFifoTool.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save CCD1AcqFifoTool.vpp\r\n");
                    break;

                case 2:
                    CogSerializer.SaveObjectToFile(MT, @".\AcqFifoTool\CCD2AcqFifoTool.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save CCD2AcqFifoTool.vpp\r\n");
                    break;
                case 3:
                    CogSerializer.SaveObjectToFile(MT, @".\AcqFifoTool\CCD3AcqFifoTool.vpp");
                    savelog.save(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff ") + " Save CCD3AcqFifoTool.vpp\r\n");
                    break;
          
            }
           // this.Close();

        }

        private void CameraForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            cogAcqFifoEditV21.LocalDisplayVisible = false;
            switch (id)
            {
                ///注意存储路径名字以及ID

                case 0:
                    Define.CCD[0] = cogAcqFifoEditV21.Subject;
                    break;
                case 1:
                    Define.CCD[1] = cogAcqFifoEditV21.Subject;
                    break;
                case 2:
                    Define.CCD[2] = cogAcqFifoEditV21.Subject;
                    break;
                case 3:
                    Define.CCD[3] = cogAcqFifoEditV21.Subject;
                    break;

            }
            Define.trigger = true;
            Dispose();
        }

    }
}
