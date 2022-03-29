using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KaliSpammer
{
    public partial class LiveLogs : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        public LiveLogs()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        public void FetchQueue()
        {
            try
            {
                Thread.Sleep(300);
                while(true)
                {
                    try
                    {
                        if (Utils.Queue.Count() > 0)
                        {
                            Thread.Sleep(5);
                            if (richTextBox1.Text.Length >= 2000000000)
                            {
                                try
                                {
                                    richTextBox1.Text = string.Empty;
                                }
                                catch
                                {
                                }
                            }

                            try
                            {
                                richTextBox1.Text += Environment.NewLine + Utils.Queue[0];
                                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                                richTextBox1.ScrollToCaret();
                                Utils.Queue.RemoveAt(0);
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        private void LiveLogs_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch
            {

            }
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {
            if(WindowState == FormWindowState.Maximized)
            {
                try
                {
                    WindowState = FormWindowState.Normal;
                }
                catch
                {
                }
            }
            else WindowState = FormWindowState.Maximized;
        }

        private void guna2PictureBox3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void LiveLogs_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Utils.Logs = false;
                if (richTextBox1.Text.Length > 0)
                {
                    richTextBox1.Text = "";
                }

                if (Utils.Queue.Count > 0)
                {
                    Utils.Queue.Clear();
                }
            }
            catch
            {
            }
        }

        private void LiveLogs_Load(object sender, EventArgs e)
        {
            try
            {
                new Thread(FetchQueue).Start();
            }
            catch
            {
            }
        }
    }
}
