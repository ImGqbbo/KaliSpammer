using KaliSpammer.Discord;
using Leaf.xNet;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace KaliSpammer
{
    public partial class MainForm : Form
    {
        int DoneCheckingP = 0, DoneCheckingT = 0, DoneParsing = 0, Limit = 0, MassDMIndex = 0;
        public bool MultiSpammer = false, ThSpammer = false, NickSpam = false, MassDMer = false, VCLoop = false;
        public Thread ServerSpammer, ThreadSpammer, ThreadCreator, InviteSpammer;

        List<string> invalidTokens = new List<string>();
        List<string> invalidProxies = new List<string>();

        LiveLogs LiveLogs = new LiveLogs();
        private Random random = new Random();

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private List<DiscordClient> GetDiscordClients()
        {
            try
            {
                List<DiscordClient> discordClients = new List<DiscordClient>();

                Limit = int.Parse(guna2TextBox12.Text);
                int i = 0;
                foreach (DiscordClient client in Utils.Clients)
                {
                    if (guna2CustomCheckBox20.Checked)
                    {
                        if (i == Limit)
                        {
                            break;
                        }

                        discordClients.Add(client);
                        i++;
                    }
                    else
                    {
                        discordClients.Add(client);
                    }
                }

                return discordClients;
            }
            catch
            {
                return Utils.Clients;
            }
        }

        public MainForm()
        {
            try
            {
                InitializeComponent();

                CheckForIllegalCrossThreadCalls = false;
                new Thread(UpdateAll).Start();

                if (!File.Exists("proxies.txt"))
                {
                    File.WriteAllText("proxies.txt", "");
                }

                if (!File.Exists("tokens.txt"))
                {
                    File.WriteAllText("tokens.txt", "");
                }

                metroLabel9.Text = "Tokens: " + File.ReadAllLines("tokens.txt").Count().ToString();
                metroLabel11.Text = "Proxies: " + File.ReadAllLines("proxies.txt").Count().ToString();

                if (File.ReadAllLines("tokens.txt").Length > 0)
                {
                    guna2Button14.Enabled = false;
                    guna2Button13.Enabled = false;
                    new Thread(() => LoadClients("tokens.txt")).Start();
                }


                if (File.ReadAllLines("proxies.txt").Length > 0)
                {
                    guna2Button17.Enabled = false;
                    guna2Button18.Enabled = false;
                    new Thread(() => LoadProxies("proxies.txt")).Start();
                }
            }
            catch
            {
            }
        }

        private void guna2Button14_Click(object sender, EventArgs e)
        {
            DoneCheckingT = 0;
            guna2Button14.Enabled = false;
            guna2Button13.Enabled = false;
            guna2Button14.Text = "Checking...";
            new Thread(TokenChecker).Start();
        }

        private void TokenChecker()
        {
            try
            {
                invalidTokens.Clear();
                foreach (DiscordClient client in Utils.Clients)
                {
                    Thread.Sleep(10);
                    new Thread(() => CheckToken(client)).Start();
                }
                while (DoneCheckingT != Utils.Clients.Count)
                {

                }
                guna2Button14.Text = "Removing...";
                foreach (string token in invalidTokens)
                {
                    foreach (DiscordClient client in Utils.Clients)
                    {
                        try
                        {
                            if (client.Token.Equals(token))
                            {
                                Utils.Clients.Remove(client);
                                break;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                guna2Button14.Text = "Updating tokens...";
                TextBox textBox = new TextBox();
                foreach (DiscordClient client in Utils.Clients)
                {
                    Thread.Sleep(5);
                    if (textBox.Text == string.Empty)
                    {
                        textBox.Text = client.Token;
                    }
                    else
                    {
                        textBox.AppendText(Environment.NewLine + client.Token);
                    }
                }
                File.WriteAllText("tokens.txt", textBox.Text);
                guna2Button14.Enabled = true;
                guna2Button13.Enabled = true;
                guna2Button14.Text = "Check Tokens";
            }
            catch
            {
            }
        }

        private void ProxyChecker()
        {
            try
            {
                invalidProxies.Clear();
                foreach (string proxy in Utils.Proxies)
                {
                    Thread.Sleep(5);
                    new Thread(() => CheckProxy(proxy)).Start();
                }
                while (DoneCheckingP != Utils.Proxies.Count)
                {

                }
                guna2Button17.Text = "Removing...";
                foreach (string proxy in invalidProxies)
                {
                    foreach (string proxi in Utils.Proxies)
                    {
                        try
                        {
                            if (proxi.Equals(proxy))
                            {
                                Utils.Proxies.Remove(proxi);
                                break;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                guna2Button17.Text = "Updating proxies...";
                TextBox textBox = new TextBox();
                foreach (string proxy in Utils.Proxies)
                {
                    Thread.Sleep(5);
                    if (textBox.Text == string.Empty)
                    {
                        textBox.Text = proxy;
                    }
                    else
                    {
                        textBox.AppendText(Environment.NewLine + proxy);
                    }
                }
                File.WriteAllText("proxies.txt", textBox.Text);
                guna2Button17.Enabled = true;
                guna2Button18.Enabled = true;
                guna2Button17.Text = "Check Proxies";
            }
            catch
            {
            }
        }

        private void CheckProxy(string proxy)
        {
            if (!Utils.IsProxyValid(proxy))
            {
                invalidProxies.Add(proxy);
                metroLabel11.Text = "Proxies: " + (Utils.Proxies.Count - invalidProxies.Count).ToString();
            }
            else
            {
            }
            DoneCheckingP++;
        }

        private void CheckToken(DiscordClient client)
        {
            HttpProxyClient proxy = null;
            if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
            {
                proxy = Utils.GetRandomProxy();
            }

            if (!Utils.IsTokenValid(client.Token, proxy))
            {
                Console.WriteLine(client.Token + " is Invalid");
                invalidTokens.Add(client.Token);
                metroLabel9.Text = "Tokens: " + (Utils.Clients.Count - invalidTokens.Count).ToString();
                Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({client.Token}) ⇒ Token status: \"Invalid/Locked\"");
            }
            else
            {
                Console.WriteLine(client.Token + " is valid");
                Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({client.Token}) ⇒ Token status: \"Valid\"");
            }
            DoneCheckingT++;
        }

        private void guna2TrackBar3_Scroll(object sender, ScrollEventArgs e)
        {
            metroLabel5.Text = $"Delay: {guna2TrackBar3.Value}";
        }

        private void guna2TrackBar6_Scroll(object sender, ScrollEventArgs e)
        {
            metroLabel8.Text = $"Delay: {guna2TrackBar6.Value}";
        }

        private void guna2TrackBar4_Scroll(object sender, ScrollEventArgs e)
        {
            metroLabel6.Text = "Disconnect delay: " + guna2TrackBar4.Value;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void metroTabPage1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void metroTabPage7_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void metroTabPage10_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void metroTabPage2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void metroTabPage3_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void metroTabPage4_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void metroTabPage5_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void metroTabPage8_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            metroLabel3.Text = $"Delay: {guna2TrackBar1.Value}";
        }

        private void guna2Button15_Click(object sender, EventArgs e)
        {
            Utils.Clients.Clear();
            File.WriteAllText("tokens.txt", string.Empty);
            metroLabel9.Text = "Tokens: 0";
            guna2TextBox14.Enabled = true;
            guna2TextBox13.Enabled = true;
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            try
            {
                MultiSpammer = true;
                ServerSpammer = new Thread(Spammer);
                ServerSpammer.Start();
                guna2Button4.Enabled = false;
                guna2Button4.Text = "Spamming...";
            }
            catch
            {
            }
        }

        private void Spammer()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    if (guna2CustomCheckBox15.Checked)
                    {
                        if (!Information.IsNumeric(guna2TextBox24.Text))
                        {
                            MessageBox.Show("The number of threads is not valid, please insert a new one.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        for (int i = 0; i < int.Parse(guna2TextBox24.Text); i++)
                        {
                            if (guna2TextBox3.Text.Contains(","))
                            {
                                string[] splitted = Strings.Split(guna2TextBox3.Text, ",");
                                foreach (DiscordClient client in GetDiscordClients())
                                {
                                    foreach (string channelId in splitted)
                                    {
                                        new Thread(() => SpamClient(client, channelId)).Start();
                                    }
                                }
                            }
                            else
                            {
                                foreach (DiscordClient client in GetDiscordClients())
                                {
                                    new Thread(() => SpamClient(client, guna2TextBox3.Text)).Start();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (guna2TextBox3.Text.Contains(","))
                        {
                            string[] splitted = Strings.Split(guna2TextBox3.Text, ",");
                            foreach (DiscordClient client in GetDiscordClients())
                            {
                                foreach (string channelId in splitted)
                                {
                                    new Thread(() => SpamClient(client, channelId)).Start();
                                }
                            }
                        }
                        else
                        {
                            foreach (DiscordClient client in GetDiscordClients())
                            {
                                new Thread(() => SpamClient(client, guna2TextBox3.Text)).Start();
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(GuildJoiner).Start();
            }
            catch
            {
            }
        }

        private void GuildJoiner()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    HttpProxyClient proxy = null;
                    if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                    {
                        proxy = Utils.GetRandomProxy();
                    }

                    string InviteCode = GetInviteLink(guna2TextBox2.Text);
                    DiscordInvite Invite = Utils.GetInvite(InviteCode, guna2CustomCheckBox19.Checked, proxy);
                    if (Invite != null)
                    {

                    }
                    string XCP = Utils.GetXCP(Invite, guna2CustomCheckBox19.Checked);
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        try
                        {
                            if (guna2ToggleSwitch1.Checked)
                            {
                                Thread.Sleep(guna2TrackBar1.Value);
                            }
                            new Thread(() => ClientJoin(client, Invite, Convert.ToBase64String(Encoding.UTF8.GetBytes(XCP)), guna2CustomCheckBox1.Checked, guna2CustomCheckBox26.Checked)).Start();
                        }
                        catch
                        {
                        }
                    }

                    Thread.Sleep(500);
                    if (guna2CustomCheckBox10.Checked)
                    {
                        try
                        {
                            if (guna2CustomCheckBox19.Checked)
                            {
                                new Thread(() => ParseGroupRecipients(Invite.Channel.Id)).Start();
                            }
                            else new Thread(() => ParseGuild(Invite.Guild.Id, Invite.Channel.Id, false)).Start();
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void ClientJoin(DiscordClient client, DiscordInvite Invite, string XCP, bool BypassMembershipScreening, bool BypassDC)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (BypassDC)
                {
                    int CurrentClient = Utils.Clients.FindIndex(x => x.Token == client.Token);
                    if (Utils.Proxies.Count > Utils.Clients.Count)
                    {
                        proxy = Utils.ParseProxy(Utils.Proxies[CurrentClient]);
                    }
                    else if (Utils.Proxies.Count < Utils.Clients.Count)
                    {
                        if (CurrentClient > Utils.Proxies.Count)
                            return;
                        else
                            proxy = Utils.ParseProxy(Utils.Proxies[CurrentClient]);
                    }
                }
                else
                {
                    if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                    {
                        proxy = Utils.GetRandomProxy();
                    }
                }

                client.JoinGuild(Invite, XCP, BypassMembershipScreening, BypassDC, proxy);
            }
            catch
            {
            }
        }

        private void SpamClient(DiscordClient client, string channelId)
        {
            HttpProxyClient proxy = null;
            if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
            {
                proxy = Utils.GetRandomProxy();
            }

            if (guna2CustomCheckBox17.Checked)
            {
                while (MultiSpammer)
                {
                    try
                    {
                        Thread.Sleep(5);

                        if (guna2ToggleSwitch2.Checked)
                        {
                            Thread.Sleep(guna2TrackBar2.Value);
                        }
                        if (guna2CustomCheckBox3.Checked)
                        {
                            if (MultiSpammer)
                            {
                                try
                                {
                                    string[] splitted = Strings.Split(guna2TextBox5.Text, " -- ");
                                    client.SendMessage(channelId, GetSpamMsg(splitted[random.Next(0, splitted.Length)]), guna2TextBox4.Text, guna2CustomCheckBox21.Checked, guna2TextBox28.Text, proxy);
                                }
                                catch
                                {

                                }
                            }
                        }
                        else
                        {
                            if (MultiSpammer)
                            {
                                try
                                {
                                    client.SendMessage(channelId, GetSpamMsg(), guna2TextBox4.Text, guna2CustomCheckBox21.Checked, guna2TextBox28.Text, proxy);
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
            else if (guna2CustomCheckBox16.Checked)
            {
                string ChannelId = client.CreateDM(channelId, proxy);
                while (MultiSpammer)
                {
                    try
                    {
                        Thread.Sleep(5);

                        if (guna2ToggleSwitch2.Checked)
                        {
                            Thread.Sleep(guna2TrackBar2.Value);
                        }
                        if (guna2CustomCheckBox3.Checked)
                        {
                            if (MultiSpammer)
                            {
                                string[] splitted = Strings.Split(guna2TextBox5.Text, " -- ");
                                client.SendMessage(ChannelId, GetDmSpamMsg(channelId, splitted[random.Next(0, splitted.Length)]), guna2TextBox4.Text, guna2CustomCheckBox21.Checked, guna2TextBox28.Text, proxy);
                            }
                        }
                        else
                        {
                            if (MultiSpammer)
                            {
                                client.SendMessage(ChannelId, GetDmSpamMsg(channelId), guna2TextBox4.Text, guna2CustomCheckBox21.Checked, guna2TextBox28.Text, proxy);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        private string GetSpamPlaceHolders(string message)
        {
            try
            {
                while (message.Contains("[random]"))
                {
                    message = Utils.ReplaceText(message, "[random]", Utils.RandomString(10));
                }
                while (message.Contains("[int]"))
                {
                    message = Utils.ReplaceText(message, "[int]", Utils.RandomInt());
                }
                while (message.Contains("[lag]"))
                {
                    message = Utils.ReplaceText(message, "[lag]", Utils.GetLagMsg());
                }
                while (message.Contains("[mtag]"))
                {
                    if (Utils.Users.Count() > 0)
                    {
                        message = Utils.ReplaceText(message, "[mtag]", Utils.RandomUser());
                    }
                    else
                    {
                        message = message.Replace("[mtag]", "");
                    }
                }

                return message;
            }
            catch
            {
                return message;
            }
        }

        private string GetNickSpamPlaceHolders(string message)
        {
            try
            {
                while (message.Contains("[random]"))
                {
                    message = Utils.ReplaceText(message, "[random]", Utils.RandomString(10));
                }
                while (message.Contains("[int]"))
                {
                    message = Utils.ReplaceText(message, "[int]", Utils.RandomInt());
                }

                return message;
            }
            catch
            {
                return message;
            }
        }

        private string GetDmSpamMsg(string userId, string msg = "")
        {
            string message = msg;
            try
            {
                if (!guna2CustomCheckBox3.Checked)
                {
                    if (guna2TextBox5.Lines.Count() > 1)
                    {
                        foreach (string line in guna2TextBox5.Lines)
                        {
                            message = message + " \\u000d" + line;
                        }
                    }
                    else
                    {
                        message = guna2TextBox5.Text;
                    }
                }
                while (message.Contains("[random]"))
                {
                    message = Utils.ReplaceText(message, "[random]", Utils.RandomString(10));
                }
                while (message.Contains("[int]"))
                {
                    message = Utils.ReplaceText(message, "[int]", Utils.RandomInt());
                }
                while (message.Contains("[mention]"))
                {
                    message = Utils.ReplaceText(message, "[mention]", $"<@{userId}>");
                }
                while (message.Contains("[emoji]"))
                {
                    message = Utils.ReplaceText(message, "[emoji]", Utils.RandomEmoji());
                }
                string Placeholders = GetSpamPlaceHolders(guna2TextBox28.Text);
                if (guna2CustomCheckBox23.Checked)
                {
                    message += "||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​|| _ _ _ _ _ _" + Placeholders;
                }
                if (guna2CustomCheckBox21.Checked)
                {
                    message += Placeholders;
                }

                return message;
            }
            catch
            {
                return guna2TextBox5.Text;
            }
        }

        private string GetSpamMsg(string msg = "")
        {
            string message = msg;
            try
            {
                if (!guna2CustomCheckBox3.Checked)
                {
                    if (guna2TextBox5.Lines.Count() > 1)
                    {
                        foreach (string line in guna2TextBox5.Lines)
                        {
                            message = message + " \\u000d" + line;
                        }
                    }
                    else
                    {
                        message = guna2TextBox5.Text;
                    }
                }

                while (message.Contains("[random]"))
                {
                    message = Utils.ReplaceText(message, "[random]", Utils.RandomString(10));
                }

                while (message.Contains("[int]"))
                {
                    message = Utils.ReplaceText(message, "[int]", Utils.RandomInt());
                }

                while (message.Contains("[mtag]"))
                {
                    if (Utils.Users.Count() > 0)
                    {
                        message = Utils.ReplaceText(message, "[mtag]", Utils.RandomUser());
                    }
                    else
                    {
                        message = message.Replace("[mtag]", "");
                    }
                }

                while (message.Contains("[bestemmia]"))
                {
                    message = Utils.ReplaceText(message, "[bestemmia]", Utils.RandomBestemmia());
                }

                while (message.Contains("[emoji]"))
                {
                    message = Utils.ReplaceText(message, "[emoji]", Utils.RandomEmoji());
                }
                string Placeholders = GetSpamPlaceHolders(guna2TextBox28.Text);
                if (guna2CustomCheckBox23.Checked)
                {
                    message += "||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​|| _ _ _ _ _ _" + Placeholders;
                }
                if (guna2CustomCheckBox21.Checked)
                {
                    message += Placeholders;
                }

                return message;
            }
            catch
            {
                return guna2TextBox5.Text;
            }
        }

        private void metroCheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (guna2CustomCheckBox9.Checked)
                {
                    LiveLogs.Show();
                }
                else
                {
                    LiveLogs.Hide();
                }
            }
            catch
            {
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            try
            {
                MultiSpammer = false;
                if (ServerSpammer != null && ServerSpammer.IsAlive)
                {
                    ServerSpammer.Abort();
                }
                Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] (Internal Logger) ⇒ Succesfully stopped server spammer.");
                guna2Button4.Enabled = true;
                guna2Button4.Text = "Start Spamming";
            }
            catch
            {
                Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] (Internal Logger) ⇒ Failed to stop server spammer.");
            }
        }

        private void guna2Button17_Click(object sender, EventArgs e)
        {
            try
            {
                DoneCheckingP = 0;
                guna2Button17.Enabled = false;
                guna2Button18.Enabled = false;
                guna2Button17.Text = "Checking...";
                new Thread(ProxyChecker).Start();
            }
            catch
            {
            }
        }

        private void PlaySB(DiscordClient client, DiscordSpeakingFlags flag)
        {
            try
            {
                client.SetSpeakingState(flag);
            }
            catch
            {

            }
        }

        private void PlaySoundboard()
        {
            try
            {
                foreach (DiscordClient client in GetDiscordClients())
                {
                    new Thread(() => PlaySB(client, DiscordSpeakingFlags.Soundshare)).Start();
                }
            }
            catch
            {

            }
        }

        private void guna2Button25_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(VocalSpammer).Start();
            }
            catch
            {
            }
        }

        private void VocalSpammer()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    if (guna2CustomCheckBox25.Checked)
                    {
                        VCLoop = true;
                    }
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        if (guna2CustomCheckBox25.Checked)
                        {
                            new Thread(() => SpamVC(client, guna2TextBox20.Text, guna2TextBox21.Text, guna2CustomCheckBox5.Checked, guna2CustomCheckBox6.Checked, guna2CustomCheckBox2.Checked, guna2CustomCheckBox8.Checked)).Start();
                        }
                        else
                        {
                            new Thread(() => JoinVocal(client, guna2TextBox20.Text, guna2TextBox21.Text, guna2CustomCheckBox5.Checked, guna2CustomCheckBox6.Checked, guna2CustomCheckBox2.Checked, guna2CustomCheckBox8.Checked)).Start();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void SpamVC(DiscordClient client, string ChannelId, string GuildId, bool Muted, bool Defeaned, bool Video, bool GroupMode)
        {
            try
            {
                while (VCLoop)
                {
                    client.JoinVoice(ChannelId, GuildId, Muted, Defeaned, Video, GroupMode);
                    Thread.Sleep(200);
                    client.LeaveVoice();
                }
            }
            catch
            {

            }
        }

        private void JoinVocal(DiscordClient client, string ChannelId, string GuildId, bool Muted, bool Defeaned, bool Video, bool GroupMode)
        {
            try
            {
                client.JoinVoice(ChannelId, GuildId, Muted, Defeaned, Video, GroupMode);
                if (guna2ToggleSwitch3.Checked)
                {
                    Thread.Sleep(guna2TrackBar4.Value);
                    client.LeaveVoice();
                }
            }
            catch
            {
            }
        }

        private void VocalLeaver()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        new Thread(() => LeaveVocal(client)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void LeaveVocal(DiscordClient client)
        {
            try
            {
                client.LeaveVoice();
            }
            catch
            {
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(LeaveServer).Start();
            }
            catch
            {
            }
        }

        private void LeaveServer()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        if (!guna2CustomCheckBox19.Checked)
                        {
                            new Thread(() => LeaveGuild(client, guna2TextBox1.Text)).Start();
                        }
                        else
                        {
                            new Thread(() => CloseDM(client, guna2TextBox1.Text)).Start();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void LeaveGuild(DiscordClient client, string guildId)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }

                client.LeaveGuild(guildId, proxy);
            }
            catch
            {
            }
        }

        private void CloseDM(DiscordClient client, string channelId)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }

                client.CloseDM(channelId, proxy);
            }
            catch
            {
            }
        }

        private void guna2Button24_Click(object sender, EventArgs e)
        {
            try
            {
                if (guna2CustomCheckBox25.Checked)
                {
                    VCLoop = false;
                }
                else new Thread(VocalLeaver).Start();
            }
            catch
            {
            }
        }

        private void guna2Button13_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Utils.Clients.Clear();
                guna2Button14.Enabled = false;
                new Thread(() => LoadTokens(openFileDialog.FileName)).Start();
            }
        }

        private void LoadTokens(string fileName)
        {
            try
            {
                metroLabel9.Text = "Tokens: " + File.ReadAllLines(fileName).Length;
                new Thread(() => LoadClients(fileName)).Start();
            }
            catch
            {
            }
        }

        private void LoadProxys(string fileName)
        {
            try
            {
                metroLabel11.Text = "Proxies: " + File.ReadAllLines(fileName).Length;
                new Thread(() => LoadProxies(fileName)).Start();
            }
            catch
            {

            }
        }

        private void guna2Button18_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Utils.Proxies.Clear();
                guna2Button17.Enabled = false;
                new Thread(() => LoadProxys(openFileDialog.FileName)).Start();
            }
        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(SpamThreads).Start();
            }
            catch
            {
            }
        }

        private void SpamThreads()
        {
            try
            {
                foreach (DiscordClient client in GetDiscordClients())
                {
                    Thread.Sleep(5);
                    if (guna2ToggleSwitch6.Checked)
                    {
                        Thread.Sleep(guna2TrackBar6.Value);
                    }
                    new Thread(() => CreateThread(client)).Start();
                }
            }
            catch
            {
            }
        }

        private void CreateThread(DiscordClient client)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }

                if (guna2CustomCheckBox18.Checked)
                {
                    int msgs = 1;
                    if (Utils.Clients.Count < 11)
                    {
                        msgs = Utils.Clients.Count();
                    }
                    else msgs = 10;
                    if (guna2TextBox13.Text.Contains(","))
                    {
                        string[] splitted = Strings.Split(guna2TextBox13.Text, ", ");
                        foreach (string channelId in splitted)
                        {
                            client.CreateThread(channelId, GetThreadName(), client.GetChannelMessages(guna2TextBox13.Text, msgs, proxy)[Utils.Clients.FindIndex(a => a.Token == client.Token)].Id, proxy);
                        }
                    }
                    else
                    {
                        client.CreateThread(guna2TextBox13.Text, GetThreadName(), client.GetChannelMessages(guna2TextBox13.Text, msgs, proxy)[Utils.Clients.FindIndex(a => a.Token == client.Token)].Id, proxy);
                    }
                }
                else
                {
                    if (guna2TextBox13.Text.Contains(","))
                    {
                        string[] splitted = Strings.Split(guna2TextBox13.Text, ", ");
                        foreach (string channelId in splitted)
                        {
                            client.CreateThread(channelId, GetThreadName(), "", proxy);
                        }
                    }
                    else
                    {
                        client.CreateThread(guna2TextBox13.Text, GetThreadName(), "", proxy);
                    }
                }
            }
            catch
            {
            }
        }

        private string GetThreadName()
        {
            string message = guna2TextBox16.Text;
            try
            {
                if (message.Equals(string.Empty))
                {
                    message = Utils.RandomString(80);
                }
                if (message.Contains("[random]"))
                {
                    message = message.Replace("[random]", Utils.RandomString(10));
                }
                if (message.Contains("[int]"))
                {
                    message = message.Replace("[int]", new Random().Next(1000, 9999).ToString());
                }
                return message;
            }
            catch
            {
                return "KaliSpammer XR V2";
            }
        }

        private void guna2Button21_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(SetActivity).Start();
            }
            catch
            {
            }
        }

        private void SetActivity()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        new Thread(() => SetGame(client)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void SetGame(DiscordClient client)
        {
            try
            {
                client.SetActivity(guna2TextBox25.Text);
            }
            catch
            {
            }
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(AddReaction).Start();
            }
            catch
            {
            }
        }

        private void AddReaction()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        if (guna2ToggleSwitch4.Checked)
                        {
                            Thread.Sleep(guna2TrackBar5.Value);
                        }
                        new Thread(() => AddClientReaction(client)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void AddClientReaction(DiscordClient client)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }
                client.AddReaction(guna2TextBox15.Text, guna2TextBox8.Text, guna2TextBox7.Text, proxy);
            }
            catch
            {
            }
        }

        private void RemoveReaction()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        if (guna2ToggleSwitch4.Checked)
                        {
                            Thread.Sleep(guna2TrackBar5.Value);
                        }
                        new Thread(() => RemoveClientReaction(client)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void RemoveClientReaction(DiscordClient client)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }
                client.RemoveReaction(guna2TextBox15.Text, guna2TextBox8.Text, guna2TextBox7.Text, proxy);
            }
            catch
            {
            }
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(ButtonSpammer).Start();
            }
            catch
            {
            }
        }

        private void ButtonSpammer()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        new Thread(() => ClientClickBtn(client)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void ClientClickBtn(DiscordClient client)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }

                foreach (string buttonId in FetchMessageButtons(client.Token, guna2TextBox6.Text, guna2TextBox9.Text, proxy))
                {
                    client.ClickButton(buttonId, guna2TextBox6.Text, guna2TextBox9.Text, guna2TextBox10.Text, guna2TextBox17.Text, proxy);
                }
            }
            catch
            {
            }
        }

        private List<string> FetchMessageButtons(string Token, string ChannelId, string MessageId, HttpProxyClient proxyClient)
        {
            List<string> Buttons = new List<string>();
            try
            {
                Dictionary<string, string> Headers = new Dictionary<string, string>()
                {
                    ["Accept"] = "*/*",
                    ["Accept-Encoding"] = "gzip, deflate, br",
                    ["Accept-Language"] = "en-US,it-IT;q=0.9",
                    ["Authorization"] = Token,
                    ["Cookie"] = Utils.Cookies,
                    ["Origin"] = "https://discord.com",
                    ["Referer"] = "https://discord.com/channels/@me",
                    ["Host"] = "discord.com",
                    ["Connection"] = "keep-alive",
                    ["User-Agent"] = Utils.UserAgent,
                    ["X-Debug-Options"] = "bugReporterEnabled",
                    ["X-Discord-Locale"] = "it",
                    ["X-Super-Properties"] = Utils.XSP,
                };

                HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/channels/" + ChannelId + "/messages?limit=50", Headers, proxyClient);
                dynamic json = JsonConvert.DeserializeObject(Utils.DecompressResponse(res));
                foreach (var item in json)
                {
                    try
                    {
                        if ((string)item.id == MessageId)
                        {
                            foreach (var item2 in item.components)
                            {
                                foreach (var item3 in item2.components)
                                {
                                    Buttons.Add((string)item3.custom_id);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                return Buttons;
            }
            catch
            {
                return Buttons;
            }
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(RemoveReaction).Start();
            }
            catch
            {
            }
        }

        private void guna2TrackBar2_Scroll(object sender, ScrollEventArgs e)
        {
            metroLabel4.Text = "Delay: " + guna2TrackBar2.Value.ToString();
        }

        private void guna2Button27_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Utils.Users.Clear();
                new Thread(() => LoadUsers(openFileDialog.FileNames)).Start();
            }
        }

        private void LoadUsers(string[] fileNames)
        {
            try
            {
                foreach (string fileName in fileNames)
                {
                    foreach (string user in File.ReadAllLines(fileName))
                    {
                        try
                        {
                            if (Information.IsNumeric(user))
                            {
                                if (!Utils.Users.Contains(user))
                                {
                                    try
                                    {
                                        Utils.Users.Add(user);
                                        label11.Text = "Parsed users: " + Utils.Users.Count().ToString();
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void UpdateAll()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(2000);
                    label11.Text = "Parsed users: " + Utils.Users.Count().ToString();
                    label21.Text = "Auto Parse (" + Utils.Users.Count().ToString() + ")";
                }
            }
            catch
            {
            }
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            try
            {
                Utils.DoneParsingGuild = false;
                guna2Button7.Text = "Parsing...";
                guna2Button7.Enabled = false;
                new Thread(() => ParseGuild(guna2TextBox19.Text, guna2TextBox30.Text, true)).Start();
            }
            catch
            {
                Utils.DoneParsingGuild = true;
                guna2Button7.Text = "Parse users";
                guna2Button7.Enabled = true;
            }
        }

        private void guna2Button12_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(ThreadJoiner).Start();
            }
            catch
            {
            }
        }

        private void ThreadJoiner()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        new Thread(() => ClientJoinThread(client)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void ClientJoinThread(DiscordClient client)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }
                if (guna2TextBox14.Text.Contains(","))
                {
                    string[] splitted = Strings.Split(guna2TextBox14.Text, ", ");
                    foreach (string channelId in splitted)
                    {
                        client.JoinThread(channelId, proxy);
                    }
                }
                else
                {
                    client.JoinThread(guna2TextBox14.Text, proxy);
                }

            }
            catch
            {
            }
        }

        private void ThreadLeaver()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        new Thread(() => ClientLeaveThread(client)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void ClientLeaveThread(DiscordClient client)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }
                if (guna2TextBox14.Text.Contains(","))
                {
                    string[] splitted = Strings.Split(guna2TextBox14.Text, ", ");
                    foreach (string channelId in splitted)
                    {
                        client.LeaveThread(channelId, proxy);
                    }
                }
                else
                {
                    client.LeaveThread(guna2TextBox14.Text, proxy);
                }

            }
            catch
            {
            }
        }

        private void guna2Button11_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(ThreadLeaver).Start();
            }
            catch
            {
            }
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void guna2Button30_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure to want to logout?", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                Hide();
                new Auth().Show();
            }
        }

        private void guna2Button22_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(ChangeNickname).Start();
            }
            catch
            {
            }
        }

        private void ChangeNickname()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        new Thread(() => SetNickname(client, guna2TextBox11.Text, guna2TextBox26.Text)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void SetNickname(DiscordClient client, string nickname, string guildId)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }

                client.SetNickname(nickname, guildId, proxy);
            }
            catch
            {
            }
        }

        private void guna2Button26_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(StatusSetter).Start();
            }
            catch
            {
            }
        }

        private void StatusSetter()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    string status = "online";
                    if (guna2ComboBox1.SelectedItem.ToString() == "Online")
                    {
                        status = "online";
                    }
                    else if (guna2ComboBox1.SelectedItem.ToString() == "Idle")
                    {
                        status = "idle";
                    }
                    else if (guna2ComboBox1.SelectedItem.ToString() == "Do not Disturb")
                    {
                        status = "dnd";
                    }
                    else if (guna2ComboBox1.SelectedItem.ToString() == "Invisible")
                    {
                        status = "invisible";
                    }
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        new Thread(() => ChangeStatus(client, status)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void guna2Button23_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(BioSetter).Start();
            }
            catch
            {
            }
        }

        private void BioSetter()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    string bio = "";
                    if (guna2TextBox32.Lines.Count() > 0)
                    {
                        foreach (string line in guna2TextBox32.Lines)
                        {
                            bio = bio + "\\n" + line;
                        }
                    }
                    else
                    {
                        bio = guna2TextBox32.Text;
                    }
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        new Thread(() => SetBio(client, bio)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void SetBio(DiscordClient client, string bio)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }

                client.SetBio(bio, proxy);
            }
            catch
            {
            }
        }

        private void guna2Button28_Click(object sender, EventArgs e)
        {
            new Thread(NukeTokens).Start();
            new Thread(() => MessageBox.Show("Nuke started for all tokens!", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)).Start();
        }

        private void NukeTokens()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        new Thread(() => NukeAccount(client)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void NukeAccount(DiscordClient client)
        {
            try
            {
                if (guna2CustomCheckBox7.Checked)
                {
                    HttpProxyClient proxy = null;
                    if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                    {
                        proxy = Utils.GetRandomProxy();
                    }

                    foreach (DiscordGuild guild in client.GetGuilds(proxy))
                    {
                        new Thread(() => LeaveGuild(client, guild.Id)).Start();
                    }
                }
                if (guna2CustomCheckBox24.Checked)
                {
                    HttpProxyClient proxy = null;
                    if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                    {
                        proxy = Utils.GetRandomProxy();
                    }

                    foreach (PrivateChannel channel in client.GetPrivateChannels(proxy))
                    {
                        new Thread(() => CloseDM(client, channel.Id)).Start();
                    }
                }
            }
            catch
            {
            }
        }

        private void guna2Button32_Click(object sender, EventArgs e)
        {
            try
            {
                NickSpam = true;
                new Thread(NickSpammer).Start();
                guna2Button32.Enabled = false;
                guna2Button32.Text = "Spamming...";
            }
            catch
            {
            }
        }

        private void NickSpammer()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    foreach (DiscordClient client in GetDiscordClients())
                    {
                        if (guna2ToggleSwitch7.Checked)
                        {
                            Thread.Sleep(guna2TrackBar7.Value);
                        }
                        new Thread(() => SpamNicks(client)).Start();
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void guna2Button31_Click(object sender, EventArgs e)
        {
            try
            {
                NickSpam = false;
                guna2Button32.Enabled = true;
                guna2Button32.Text = "Start Spamming";
            }
            catch
            {
            }
        }

        private void metroTabPage6_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void SpamNicks(DiscordClient client)
        {
            try
            {
                while (NickSpam)
                {
                    if (!guna2CustomCheckBox12.Checked)
                    {
                        HttpProxyClient proxy = null;
                        if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                        {
                            proxy = Utils.GetRandomProxy();
                        }

                        client.SetNickname(GetSpamNick(), guna2TextBox18.Text, proxy);
                        if (guna2ToggleSwitch7.Checked)
                        {
                            Thread.Sleep(guna2TrackBar7.Value);
                        }
                    }
                    else
                    {
                        string nick = "";
                        string name = GetNickSpamPlaceHolders(guna2TextBox23.Text);
                        for (int i = 0; i < name.Length; i++)
                        {
                            HttpProxyClient proxy = null;
                            if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                            {
                                proxy = Utils.GetRandomProxy();
                            }

                            nick = nick + name[i].ToString();
                            client.SetNickname(nick, guna2TextBox18.Text, proxy);
                            if (guna2ToggleSwitch7.Checked)
                            {
                                Thread.Sleep(guna2TrackBar7.Value);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void guna2TrackBar7_Scroll(object sender, ScrollEventArgs e)
        {
            metroLabel1.Text = "Delay: " + guna2TrackBar7.Value.ToString();
        }

        private void guna2CustomCheckBox11_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox12.Checked)
            {
                guna2CustomCheckBox12.Checked = false;
                guna2CustomCheckBox11.Checked = true;
            }
        }

        private void guna2CustomCheckBox12_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox11.Checked)
            {
                guna2CustomCheckBox11.Checked = false;
                guna2CustomCheckBox12.Checked = true;
            }
        }

        private void guna2CustomCheckBox14_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox14.Checked == false && guna2CustomCheckBox13.Checked == false)
            {
                guna2CustomCheckBox14.Checked = true;
            }
            if (guna2CustomCheckBox13.Checked)
            {
                guna2GradientButton2.Text = "Emojis";
                guna2CustomCheckBox13.Checked = false;
                guna2CustomCheckBox14.Checked = true;
            }
        }

        private void guna2CustomCheckBox13_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox14.Checked == false && guna2CustomCheckBox13.Checked == false)
            {
                guna2CustomCheckBox13.Checked = true;
            }
            if (guna2CustomCheckBox14.Checked)
            {
                guna2CustomCheckBox14.Checked = false;
                guna2CustomCheckBox13.Checked = true;
                guna2GradientButton2.Text = "Emotes";
            }
        }

        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(FetchEmote).Start();
            }
            catch
            {
            }
        }

        private void FetchEmote()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    HttpProxyClient proxy = null;
                    if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                    {
                        proxy = Utils.GetRandomProxy();
                    }

                    string reactions = "";
                    foreach (string item in Utils.Clients[GetFirstValidClient()].GetMessageReactions(guna2TextBox8.Text, guna2TextBox15.Text, guna2CustomCheckBox14.Checked, proxy))
                    {
                        reactions += item + ", ";
                    }
                    guna2TextBox7.Text = reactions.Substring(0, reactions.Length - 2);
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void guna2CustomCheckBox17_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox17.Checked == false && guna2CustomCheckBox16.Checked == false)
            {
                guna2CustomCheckBox17.Checked = true;
            }
            if (guna2CustomCheckBox16.Checked)
            {
                guna2CustomCheckBox16.Checked = false;
                guna2CustomCheckBox17.Checked = true;
            }
        }

        private void guna2CustomCheckBox16_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox17.Checked == false && guna2CustomCheckBox16.Checked == false)
            {
                guna2CustomCheckBox16.Checked = true;
            }
            if (guna2CustomCheckBox17.Checked)
            {
                guna2CustomCheckBox17.Checked = false;
                guna2CustomCheckBox16.Checked = true;
            }
        }

        private void guna2Button34_Click(object sender, EventArgs e)
        {
            try
            {
                MassDMer = true;
                new Thread(MassDM).Start();
                guna2Button34.Enabled = false;
                guna2Button34.Text = "Mass DMing...";
            }
            catch
            {
            }
        }

        private void MassDM()
        {
            try
            {
                if (Utils.Clients.Count() > 0)
                {
                    if (Utils.Users.Count > 0)
                    {
                        MassDMIndex = 0;
                        for (int i = 0; i < (Utils.Users.Count / Utils.Clients.Count); i++)
                        {
                            if (MassDMer)
                            {
                                foreach (DiscordClient client in GetDiscordClients())
                                {
                                    Thread.Sleep(5);
                                    if (MassDMer)
                                    {
                                        new Thread(() => SendMsg(client)).Start();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("You haven't parsed/loaded any user yet!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("You haven't loaded any token/client yet! Please load them and retry.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
            }
        }

        private void SendMsg(DiscordClient client)
        {
            try
            {
                if (MassDMer)
                {
                    HttpProxyClient proxy = null;
                    if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count > 0)
                    {
                        proxy = Utils.GetRandomProxy();
                    }

                    string channelId = client.CreateDM(Utils.Users[MassDMIndex], proxy);
                    if (channelId != "")
                    {
                        if (MassDMer)
                        {
                            if (guna2CustomCheckBox22.Checked)
                            {
                                string[] splitted = Strings.Split(guna2TextBox27.Text, " -- ");
                                client.SendMessage(channelId, GetMassDMMsg(splitted[Utils.random.Next(0, splitted.Length)], Utils.Users[MassDMIndex]), "", false, "", proxy);
                                MassDMIndex++;
                            }
                            else
                            {
                                client.SendMessage(channelId, GetMassDMMsg("", Utils.Users[MassDMIndex]), "", false, "", proxy);
                                MassDMIndex++;
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private string GetMassDMMsg(string Message, string UserId)
        {
            string msg = Message;
            try
            {
                if (!guna2CustomCheckBox3.Checked)
                {
                    if (guna2TextBox27.Lines.Count() > 1)
                    {
                        foreach (string line in guna2TextBox27.Lines)
                        {
                            msg = msg + " \\u000d" + line;
                        }
                    }
                    else
                    {
                        msg = guna2TextBox27.Text;
                    }
                }

                while (msg.Contains("[random]"))
                {
                    msg = Utils.ReplaceText(msg, "[random]", Utils.RandomString(10));
                }
                while (msg.Contains("[int]"))
                {
                    msg = Utils.ReplaceText(msg, "[int]", Utils.RandomInt());
                }
                while (msg.Contains("[lag]"))
                {
                    msg = Utils.ReplaceText(msg, "[lag]", Utils.GetLagMsg());
                }
                if (msg.Contains("[user]"))
                {
                    msg = msg.Replace("[user]", $"<@{UserId}>");
                }
                while (msg.Contains("[emoji]"))
                {
                    msg = Utils.ReplaceText(msg, "[emoji]", Utils.RandomEmoji());
                }

                return msg;
            }
            catch
            {
                return msg;
            }
        }

        private void guna2Button33_Click(object sender, EventArgs e)
        {
            try
            {
                MassDMer = false;
                guna2Button34.Enabled = true;
                guna2Button34.Text = "Start Spamming";
            }
            catch
            {
            }
        }

        private void metroTabPage9_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void metroTabPage11_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        public int GetFirstValidClient()
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }

                int a = 0;
                while (!Utils.IsTokenValid(Utils.Clients[a].Token, proxy))
                {
                    a++;
                }
                return a;
            }
            catch
            {
                return 0;
            }
        }

        private void guna2Button16_Click(object sender, EventArgs e)
        {
            Utils.Proxies.Clear();
            File.WriteAllText("proxies.txt", string.Empty);
            metroLabel11.Text = "Proxies: 0";
            guna2TextBox18.Enabled = true;
            guna2TextBox17.Enabled = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/x9HcDFunF6");
        }

        private void guna2TextBox12_TextChanged(object sender, EventArgs e)
        {
            if (!Information.IsNumeric(guna2TextBox12.Text))
            {
                MessageBox.Show("The number limit of tokens is not valid! Please insert a new one.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void guna2CustomCheckBox23_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox21.Checked)
            {
                guna2CustomCheckBox21.Checked = false;
                guna2CustomCheckBox23.Checked = true;
            }
        }

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            try
            {
                Utils.Logs = true;
                LiveLogs.Show();
            }
            catch
            {
                LiveLogs = new LiveLogs();
                Utils.Logs = true;
                LiveLogs.Show();
            }
        }

        private void guna2CustomCheckBox21_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox23.Checked)
            {
                guna2CustomCheckBox23.Checked = false;
                guna2CustomCheckBox21.Checked = true;
            }
        }

        private void guna2TrackBar8_Scroll(object sender, ScrollEventArgs e)
        {
            metroLabel2.Text = "Delay: " + guna2TrackBar8.Value.ToString();
        }

        private string GetSpamNick()
        {
            try
            {
                if (!guna2CustomCheckBox11.Checked && !guna2CustomCheckBox12.Checked)
                {
                    string[] splitted = Strings.Split(guna2TextBox22.Text, ", ");
                    if (splitted.Length > 0)
                    {
                        return GetNickSpamPlaceHolders(splitted[Utils.random.Next(0, splitted.Length)]);
                    }
                    return Utils.RandomString(16);
                }
                else if (guna2CustomCheckBox11.Checked)
                {
                    return Utils.RandomString(16);
                }

                return Utils.RandomString(16);
            }
            catch
            {
                return Utils.RandomString(16);
            }
        }

        private void guna2CustomCheckBox20_Click(object sender, EventArgs e)
        {
            if (!guna2CustomCheckBox20.Checked)
            {
                if (!Information.IsNumeric(guna2TextBox12.Text))
                {
                    MessageBox.Show("The number limit of tokens to use is not valid!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    guna2CustomCheckBox20.Checked = false;
                }
            }
        }

        private void ChangeStatus(DiscordClient client, string status)
        {
            try
            {
                client.SetStatus(status);
            }
            catch
            {
            }
        }

        private void guna2GradientButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure to want to logout?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        Hide();
                        new Auth().Show();
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

        private void metroTabPage2_Click(object sender, EventArgs e)
        {

        }

        private void ParseGroupRecipients(string channelId)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }

                Dictionary<string, string> Headers = new Dictionary<string, string>()
                {
                    ["Authorization"] = Utils.Clients[GetFirstValidClient()].Token,
                    ["User-Agent"] = Utils.UserAgent,
                    ["X-Super-Properties"] = Utils.XSP,
                };

                HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/channels/" + channelId, Headers, proxy);
                dynamic json = JsonConvert.DeserializeObject(res.ToString());

                foreach (var item in json.recipients)
                    Utils.Users.Add((string)item.id);
            }
            catch
            {

            }
        }

        private void guna2TextBox19_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button10_Click(object sender, EventArgs e)
        {
            try
            {
                new Thread(PlaySoundboard).Start();
            }
            catch
            {

            }
        }

        private void guna2CustomCheckBox26_Click(object sender, EventArgs e)
        {
            if (Utils.Proxies.Count < 1)
            {
                MessageBox.Show("You haven't loaded any proxy yet!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                guna2CustomCheckBox26.Checked = false;
            }
        }

        private void guna2CustomCheckBox25_Click(object sender, EventArgs e)
        {
            try
            {
                if (guna2CustomCheckBox25.Checked)
                {
                    guna2Button25.Text = "Start Spamming";
                    guna2Button24.Text = "Stop Spamming";
                }
                else
                {
                    guna2Button25.Text = "Join Voice Channel";
                    guna2Button24.Text = "Leave Voice Channel";
                }
            }
            catch
            {

            }
        }

        private void ParseGuild(string guildId, string channelId, bool manualParse)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }

                DoneParsing = 0;
                Utils.Users.Clear();
                int c = GetFirstValidClient();
                List<DiscordChannel> channels = Utils.Clients[c].GetGuildChannels(guna2TextBox19.Text, proxy);
                foreach (DiscordChannel channel in channels)
                {
                    if (channel.Type == 0)
                    {
                        new Thread(() => ParseChannel(channel.Id, c)).Start();
                        Thread.Sleep(50);
                    }
                }

                while (channels.Count != DoneParsing)
                {
                }

                Thread.Sleep(1);
                Utils.WantToParse = true;
                Utils.ChannelID = channelId;
                Utils.GuildID = guildId;
                Utils.Clients[c].ConnectToWS();

                if (manualParse)
                {
                    while (!Utils.DoneParsingGuild)
                    {
                    }

                    label11.Text = "Parsed users: " + Utils.Users.Count().ToString();
                    guna2Button7.Text = "Parse users";
                    guna2Button7.Enabled = true;
                }
            }
            catch
            {
            }
        }

        private void ParseChannel(string channelId, int c)
        {
            try
            {
                HttpProxyClient proxy = null;
                if (guna2CustomCheckBox9.Checked && Utils.Proxies.Count() > 0)
                {
                    proxy = Utils.GetRandomProxy();
                }

                foreach (DiscordMessage message in Utils.Clients[c].GetChannelMessages(channelId, 50, proxy))
                {
                    if (!Utils.Users.Contains(message.Author.Id))
                    {
                        Utils.Users.Add(message.Author.Id);
                    }
                }
            }
            catch
            {
            }

            DoneParsing++;
        }

        private void LoadClients(string path)
        {
            try
            {
                string[] Lines = File.ReadAllLines(path);
                foreach (string Token in Lines)
                {
                    try
                    {
                        AddClient(Token);
                    }
                    catch
                    {
                    }
                }
                while (Utils.Clients.Count != Lines.Length)
                {
                }
                guna2Button14.Text = "Updating...";
                TextBox textBox = new TextBox();
                foreach (DiscordClient client in Utils.Clients)
                {
                    try
                    {
                        Thread.Sleep(5);
                        if (textBox.Text == string.Empty)
                        {
                            try
                            {
                                textBox.Text = client.Token;
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            try
                            {
                                textBox.AppendText(Environment.NewLine + client.Token);
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
                guna2Button14.Text = "Check Tokens";
                File.WriteAllText("tokens.txt", textBox.Text);
                guna2Button14.Enabled = true;
                guna2Button13.Enabled = true;
                metroLabel9.Text = "Tokens: " + Utils.Clients.Count;
            }
            catch
            {
            }
        }

        private void LoadProxies(string path)
        {
            try
            {
                string[] Lines = File.ReadAllLines(path);
                foreach (string Proxy in Lines)
                {
                    try
                    {
                        AddProxy(Proxy);
                    }
                    catch
                    {
                    }
                }
                while (Utils.Proxies.Count != Lines.Length)
                {
                }
                guna2Button17.Text = "Updating...";
                TextBox textBox = new TextBox();
                foreach (string proxy in Utils.Proxies)
                {
                    try
                    {
                        Thread.Sleep(5);
                        if (textBox.Text == string.Empty)
                        {
                            try
                            {
                                textBox.Text = proxy;
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            try
                            {
                                textBox.AppendText(Environment.NewLine + proxy);
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
                guna2Button17.Text = "Check Proxies";
                File.WriteAllText("proxies.txt", textBox.Text);
                guna2Button17.Enabled = true;
                guna2Button18.Enabled = true;
                metroLabel11.Text = "Proxies: " + Utils.Proxies.Count;
            }
            catch
            {
            }
        }

        private string GetInviteLink(string text)
        {
            if (text.Contains("/"))
            {
                string[] parti = Strings.Split(text, "/");
                return parti.Last();
            }
            else return text;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void guna2TrackBar5_Scroll(object sender, ScrollEventArgs e)
        {
            metroLabel7.Text = "Delay: " + guna2TrackBar5.Value;
        }

        private void colorChanger()
        {
            try
            {
                int a = 255;
                int b = 0;
                int c = 0;
                while (true)
                {
                    try
                    {
                        if (a == 255 && c == 0)
                        {
                            b++;
                        }
                        if (b == 255 && c == 0)
                        {
                            a--;
                        }
                        if (a == 0 && b == 255)
                        {
                            c++;
                        }
                        if (c == 255 && a == 0)
                        {
                            b--;
                        }
                        if (b == 0 && c == 255)
                        {
                            a++;
                        }
                        if (a == 255 && b == 0)
                        {
                            c--;
                        }
                        label10.ForeColor = Color.FromArgb(a, b, c);
                        Thread.Sleep(5);
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

        public void AddClient(string Token)
        {
            try
            {
                Utils.Clients.Add(new DiscordClient(Token));
            }
            catch
            {
            }
        }

        public void AddProxy(string Proxy)
        {
            try
            {
                Utils.Proxies.Add(Proxy);
            }
            catch
            {
            }
        }

    }
}