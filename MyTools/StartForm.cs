using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyTools
{
    public partial class StartForm : Form
    {
        public StartForm()
        {
            InitializeComponent();
        }

        public void showBar(int count)
        {
            this.progressBar2.Value = count;
            if (count == 100)
            {
                this.Close();
            }
        }
    }

}
