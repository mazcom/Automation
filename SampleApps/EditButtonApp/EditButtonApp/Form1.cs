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
            System.Diagnostics.Trace.WriteLine("Form is created...");
            btnClear.Click += BtnClear_Click;
            Load += Form1_Load;
            FormClosed += Form1_FormClosed;

            ContextMenu cm = new ContextMenu();
            cm.MenuItems.Add("Item 1");
            cm.MenuItems.Add("Item 2");

            edText.ContextMenu = cm;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("Form is loaded...");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("Form was cloased...");
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            edText.Text = null;
        }
    }
}
