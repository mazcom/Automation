using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditButtonApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            btnClear.Click += BtnClear_Click;
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            edText.Text = null;
        }
    }
}
