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
        private Timer timer;

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

            timer = new Timer();
            timer.Tick += HandleTimer;
            timer.Interval = 5000;
            //this.edText.Visible = false;
            timer.Start();
        }

        private void HandleTimer(object sender, EventArgs e)
        {
            //this.edText.Visible = false;


            //var edText2 = new System.Windows.Forms.TextBox();
            //edText2.AcceptsTab = true;
            //edText2.Location = new System.Drawing.Point(54, 28);
            //edText2.Margin = new System.Windows.Forms.Padding(8);
            //edText2.Name = "edText2";
            //edText2.Size = new System.Drawing.Size(246, 20);
            //this.Controls.Add(edText2);

            //if (this.edText == null)
            //    return;

            //this.edText.Enabled = false;
            //this.edText.Visible = false;
            //this.Controls.Remove(this.edText);
            //this.edText.Dispose();
            //this.Refresh();
            //this.edText = null;
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
