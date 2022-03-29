using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace KaliSpammer
{
    public partial class Auth : Form
    {
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private MainForm form = new MainForm();
        private bool AppReady = false;
        public static KaliAPI KeyAuthApp = new KaliAPI("KaliSpammer", "Yz1zfQHYmp", "407e8a2d5e0f0cc9c9b60369a07ee89da41d3e8ad49c832431a8abe948078466", "2.0");
        
        public Auth()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            new Thread(InitApp).Start();
            new Thread(StartUpData).Start();
            new Thread(CheckForIllegalPrograms).Start();
        }

        private void InitApp()
        {
            try
            {
                KeyAuthApp.init();
                AppReady = true;
            }
            catch
            {

            }
        }

        private void WaitForAppReady()
        {
            try
            {
                while (!AppReady)
                {
                }
                AuthenticateBtn.Text = "Authenticate";
                AuthenticateBtn.Enabled = true;
            }
            catch
            {

            }
        }

        private void StartUpData()
        {
            try
            {
                if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/KaliSpammer/data.txt"))
                {
                    try
                    {
                        if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/KaliSpammer"))
                        {
                            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/KaliSpammer");
                        }
                        File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/KaliSpammer/data.txt", JsonConvert.SerializeObject(new { username = username.Text, password = password.Text, remember = guna2CustomCheckBox1.Checked }));
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

        private void CheckForIllegalPrograms()
        {
            try
            {
                string[] illegalPrograms = { "Fiddler", "httpanalyzer", "httpdebug", "wireshark", "fiddler", "proxifier", "mitmproxy" };
                while (true)
                {
                    foreach (Process process in Process.GetProcesses())
                    {
                        if (illegalPrograms.Contains(process.ProcessName) || illegalPrograms.Contains(process.MainWindowTitle))
                        {
                            process.Kill();
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                    Thread.Sleep(2000);
                }
            }
            catch
            {
            }
        }

        private void Authentication()
        {
            try
            {
                AuthenticateBtn.Text = "Authenticating...";
                AuthenticateBtn.Enabled = false;
                KeyAuthApp.login(username.Text, password.Text);
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/KaliSpammer/data.txt", JsonConvert.SerializeObject(new { username = username.Text, password = password.Text, remember = guna2CustomCheckBox1.Checked }));
                AuthenticateBtn.Text = "Logged in";
                Thread.Sleep(500);
                form.Show();
                Hide();
            }
            catch
            {
            }
        }

        private void Auth_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void Auth_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void Auth_Load(object sender, EventArgs e)
        {
            try
            {

                new Thread(WaitForAppReady).Start();
                AuthenticateBtn.Text = "Initializing...";
                AuthenticateBtn.Enabled = false;
                dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/KaliSpammer/data.txt"));
                username.Text = json.username;
                password.Text = json.password;
                guna2CustomCheckBox1.Checked = Convert.ToBoolean(json.remember);
            }
            catch
            {
            }
        }

        private void AuthenticateBtn_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(Authentication);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void guna2PictureBox4_MouseDown(object sender, MouseEventArgs e)
        {
            password.UseSystemPasswordChar = false;
        }

        private void guna2PictureBox4_MouseUp(object sender, MouseEventArgs e)
        {
            password.UseSystemPasswordChar = true;
        }
    }
}