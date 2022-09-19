using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Server
{
    public partial class mainForm : Form
    {
        static Server server;

        public mainForm()
        {
            InitializeComponent();
            Thread thread1 = new Thread(new ParameterizedThreadStart(echo));
            thread1.Start("Wait for connection...\r\n");
            server = new Server(this);
            new Thread(Server.StartServer).Start();
        }

        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendTextBox), new object[] { value });
                return;    
            }
            tbMain.Text += value;
        }

        public void echo(object? message)
        {
            AppendTextBox($"{message}");
           // tbMain.SelectionStart = 0;
        }

    }
}
