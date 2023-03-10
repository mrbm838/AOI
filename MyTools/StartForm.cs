using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MyTools
{
    public partial class StartForm : Form
    {
        private Timer _timer;
        public int BarValue;

        public StartForm()
        {
            InitializeComponent();
            _timer = new Timer { Interval = 100, Enabled = true };
            _timer.Tick += Timer_Tick;

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.progressBar2.Value = BarValue;
            if (BarValue == 100)
            {
                //Thread.Sleep(1000);
                _timer.Stop();
                this.Close();
            }
        }

        //public void ShowBar(int count)
        //{
            
        //}

        private void StartForm_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                VisionInitialize.LoadVPP(this); //加载VPP
            });
        }
    }

}
