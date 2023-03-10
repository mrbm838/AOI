using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cowain_AutoMachine;
using System.Threading;
using SingleAxisMotion;

namespace Cowain_Form.FormView
{
    public partial class frm_LoadingDlg : Form
    {
        public frm_LoadingDlg()
        {
            InitializeComponent();
        }

        public frm_LoadingDlg(ref Motion pM)
        {
            pMachine = pM;
            //m_iProgressValue = 0;
            InitStep = Motion.enInitStep.StartLoading;
            InitializeComponent();
        }
        public Motion pMachine;
        //int m_iProgressValue , m_iCount=0;
        Motion.enInitStep InitStep;
        private void timer_Loading_Tick(object sender, EventArgs e)
        {
            if (progressBar1.Value >= 100)
            {
                {
                    Thread.Sleep(1000);
                    timer_Loading.Enabled = false;
                    this.Close();
                }
            }
            //--------------------------
            string[] strStep = { "Start Initial", "Start Loading", "Loading Data", "Checking System", "Checking Data" };
            label_Title.Text = InitStep.ToString();
            int iRetCode = 0;
            pMachine.GetInitialStatus(ref iRetCode);
            //--------------------------
            int iInitialStep = 0;
            if (iRetCode < 200)
                iInitialStep = iRetCode - 100;
            //--------------------------
            if (progressBar1.Value < 100 && iInitialStep >= 0)
            {
                label_Title.Text = strStep[iInitialStep];
                int iMax = iInitialStep * 20;
                if (progressBar1.Value < iMax)
                    progressBar1.Value = progressBar1.Value + 1;
                else
                    progressBar1.Value = iMax;
            }

            if (iRetCode == 1001 || iRetCode == 1002)
            {
                progressBar1.Value = 100;
                string strShow = (iRetCode == 1001) ? "Initial Succseed" : "Initial Fail";
                if (strShow == "Initial Fail")
                    this.BackColor = Color.Red;
                label_Title.Text = strShow;
            }
        }

    }
}
