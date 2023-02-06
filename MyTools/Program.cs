using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MyTools
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "OnlyRun");

            if (mutex.WaitOne(0, true))
            {
                //Application.Run(new logon());
                Application.Run(new MainForm());
                //Application.Run(new LightSourceControl());
                
            }
            else
            {
                MessageBox.Show("程序已经在运行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Application.Exit();
            }
         
        }
    }
}
