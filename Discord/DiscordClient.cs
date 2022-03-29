using Leaf.xNet;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using WebSocketSharp;
using HttpStatusCode = Leaf.xNet.HttpStatusCode;

namespace KaliSpammer.Discord
{
    class DiscordClient
    {
        public string Token = "";
        public static bool ConnectedToWS = false;
        WebSocket ws;

        private Dictionary<string, string> GetHeaders()
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

            return Headers;
        }

        private Dictionary<string, string> GetRequestHeaders(string Data)
        {
            Dictionary<string, string> Headers = new Dictionary<string, string>()
            {
                ["Accept"] = "*/*",
                ["Accept-Encoding"] = "gzip, deflate, br",
                ["Accept-Language"] = "en-US,it-IT;q=0.9",
                ["Authorization"] = Token,
                ["Content-Length"] = Data.Length.ToString(),
                ["Content-Type"] = "application/json",
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

            return Headers;
        }

        public void SetSpeakingState(DiscordSpeakingFlags flags)
        {
            try
            {
                SendData(JsonConvert.SerializeObject(new
                {
                    op = 5,
                    d = new
                    {
                    }
                }
                ));
                SendData(JsonConvert.SerializeObject(new
                {
                    op = 5,
                    d = new
                    {
                        speaking = flags,
                        delay = 0,
                        ssrc = 1,
                    }
                }
                ));
            }
            catch
            {

            }
        }

        public DiscordClient(string token)
        {
            Token = token;
        }

        public void ParseGuild(string guildId, string channelId)
        {
            try
            {
                SendData("{\"op\":14,\"d\":{\"guild_id\":\"" + guildId + "\",\"typing\":true,\"activities\":true,\"threads\":true,\"channels\":{\"" + channelId + "\":[[0,99]]}}}");
            }
            catch
            {
            }
        }

        public void SetActivity(string activity)
        {
            try
            {
                SendData("{\"op\": 3,\"d\": {\"since\": 91879201,\"activities\": [{\"name\": \"" + activity + "\", \"type\": 0}],\"status\": \"online\",\"afk\": false}}");
            }
            catch
            {
            }
        }

        public void SetStatus(string status)
        {
            try
            {
                SendData("{\"op\": 3,\"d\": {\"since\": 91879201,\"activities\": [{}],\"status\": \"" + status + "\",\"afk\": false}}");
            }
            catch
            {
            }
        }

        public void SendData(string data)
        {
            try
            {
                if (!ConnectedToWS)
                {
                    ConnectToWS();
                }

                ws.Send(data);
            }
            catch
            {
            }
        }

        public void CloseWS()
        {
            try
            {
                if (ConnectedToWS)
                {
                    ws.Close();
                }
            }
            catch
            {
            }
        }

        public void ConnectToWS()
        {
            try
            {
                ws = new WebSocket("wss://gateway.discord.gg/?v=9&encoding=json");
                ws.Connect();
                ws.OnError += Ws_OnError;
                ws.OnClose += Ws_OnClose;
                ws.OnMessage += Ws_OnMessage;
                ws.Send("{\"op\":2,\"d\":{\"token\":\"" + Token + "\",\"client_state\": {\"guild_hashes\": {}, \"highest_last_message_id\": \"0\", \"read_state_version\": 0, \"user_guild_settings_version\": -1}, \"compress\": false, \"presence\": {\"status\": \"online\", \"since\": 0, \"activities\": [], \"afk\": false},\"capabilities\": 125,\"properties\": {\"browser\": \"Chrome\",\"browser_user_agent\": \"" + Utils.UserAgent + "\",\"browser_version\": \"94.0.4606.71\",\"client_build_number\": 100673,\"client_event_source\": null,\"device\": \"\",\"os\": \"Windows\",\"os_version\": \"10.0\",\"referrer\": \"\",\"referrer_current\": \"\",\"referring_domain\": \"\",\"referring_domain_current\": \"\",\"release_channel\": \"stable\",\"system_locale\": \"it-IT\"}}}");
                ConnectedToWS = true;
            }
            catch
            {
            }
        }

        private void Ws_OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Error caught! " + e.Message);
        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            try
            {
                ConnectedToWS = false;
            }
            catch
            {
            }
        }

        public void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                JObject json = JObject.Parse(e.Data);
                if (json.Value<int>("op") == 10)
                {
                    new Thread(() => HeartBeat(json["d"].Value<int>("heartbeat_interval"))).Start();
                }
                if (json.Value<string>("t") == "GUILD_MEMBER_LIST_UPDATE")
                {
                    Utils.WantToParse = false;
                    string dato = e.Data;
                    string[] splitted = Strings.Split(dato, "\"id\":\"");
                    foreach (string item in splitted)
                    {
                        string[] splitted2 = Strings.Split(item, "\"");
                        string userId = splitted2[0];
                        if (Information.IsNumeric(userId) && !Utils.Users.Contains(userId))
                        {
                            Utils.Users.Add(userId);
                        }
                    }
                    ws.Close();
                }
                if (json.Value<string>("t") == "READY" && Utils.WantToParse)
                {
                    Thread.Sleep(10);
                    ParseGuild(Utils.GuildID, Utils.ChannelID);
                    Thread.Sleep(500);
                    Utils.DoneParsingGuild = true;
                }
            }
            catch
            {
            }
        }

        public void HeartBeat(int interval)
        {
            try
            {
                while (ConnectedToWS)
                {
                    SendData("{\"op\":1,\"d\":0}");
                    Thread.Sleep(interval);
                }
            }
            catch
            {

            }
        }

        public void JoinVoice(string ChannelId, string GuildId, bool Muted, bool Defeaned, bool Video, bool GroupMode)
        {
            try
            {
                if (GroupMode)
                {
                    SendData("{\"op\":4,\"d\":{\"guild_id\":null,\"channel_id\":\"" + ChannelId + "\",\"self_mute\":" + Muted.ToString().ToLower() + ",\"self_deaf\":" + Defeaned.ToString().ToLower() + ",\"self_video\":" + Video.ToString().ToLower() + ",\"preferred_region\":null}}");
                }
                else SendData("{\"op\":4,\"d\":{\"guild_id\":\"" + GuildId + "\",\"channel_id\":\"" + ChannelId + "\",\"self_mute\":" + Muted.ToString().ToLower() + ",\"self_deaf\":" + Defeaned.ToString().ToLower() + ",\"self_video\":" + Video.ToString().ToLower() + ",\"preferred_region\":null}}");
            }
            catch
            {
            }
        }

        public void LeaveVoice()
        {
            try
            {
                SendData("{\"op\": 4, \"d\": {\"guild_id\": null, \"channel_id\": null, \"self_mute\": false, \"self_deaf\": false, \"self_video\": false}}");
            }
            catch
            {
            }
        }

        public void LeaveGuild(string GuildId, HttpProxyClient proxyClient)
        {
            try
            {
                HttpResponse res = KaliHttp.Delete("https://discord.com/api/v9/users/@me/guilds/" + GuildId, GetRequestHeaders("{\"lurking\": false}"), "{\"lurking\": false}", "application/json", proxyClient);
                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Succesfully leaved from {GuildId}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Succesfully leaved from {GuildId}");
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to leave from {GuildId} » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to leave from {GuildId} » {Utils.DecompressResponse(res)}");
                }
            }
            catch
            {
            }
        }


        public List<string> GetMessageReactions(string ChannelId, string MessageId, bool IsEmoji, HttpProxyClient proxyClient)
        {
            List<string> reactions = new List<string>();
            try
            {
                HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/channels/" + ChannelId + "/messages?limit=50", GetHeaders(), proxyClient);
                dynamic json = JsonConvert.DeserializeObject(Utils.DecompressResponse(res));
                foreach (var item in json)
                {
                    try
                    {
                        if (((string)item.id).Equals(MessageId))
                        {
                            try
                            {
                                foreach (var reaction in item.reactions)
                                {
                                    try
                                    {
                                        if (IsEmoji && (string)reaction.emoji.id == null)
                                        {
                                            reactions.Add((string)reaction.emoji.name);
                                        }
                                        else if (!IsEmoji && (string)reaction.emoji.id != null)
                                        {
                                            string react = (string)reaction.emoji.name;
                                            string emoji = react += ":" + (string)reaction.emoji.id;
                                            reactions.Add(emoji);
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }

                                return reactions;
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

                return reactions;
            }
            catch
            {
                return reactions;
            }
        }

        public List<DiscordChannel> GetGuildChannels(string GuildId, HttpProxyClient proxyClient)
        {
            List<DiscordChannel> channels = new List<DiscordChannel>();
            try
            {
                HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/guilds/" + GuildId + "/channels", GetHeaders(), proxyClient);
                var Channels = JsonConvert.DeserializeObject<List<DiscordChannel>>(Utils.DecompressResponse(res));
                channels.AddRange(Channels);

                return channels;
            }
            catch
            {
                return channels;
            }
        }

        public void JoinGuild(DiscordInvite Invite, string XCP, bool BypassRules, bool BypassDC, HttpProxyClient proxyClient)
        {
            try
            {
                GetGuild(Invite, XCP, proxyClient);
                Dictionary<string, string> Headers = new Dictionary<string, string>()
                {
                    ["Accept"] = "*/*",
                    ["Accept-Encoding"] = "gzip, deflate, br",
                    ["Authorization"] = Token,
                    ["Content-Length"] = "2",
                    ["Content-Type"] = "application/json",
                    ["Cookie"] = Utils.Cookies,
                    ["Origin"] = "https://discord.com",
                    ["Referer"] = "https://discord.com/channels/@me",
                    ["Host"] = "discord.com",
                    ["Connection"] = "keep-alive",
                    ["User-Agent"] = Utils.UserAgent,
                    ["X-Debug-Options"] = "bugReporterEnabled",
                    ["X-Discord-Locale"] = "it",
                    ["X-Context-Properties"] = XCP,
                    ["X-Super-Properties"] = Utils.XSP,
                };

                HttpResponse res = KaliHttp.Post("https://discord.com/api/v9/invites/" + Invite.Code, Headers, "{}", "application/json", proxyClient);
                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Succesfully joined in [{Invite.Guild.Id}] with invite code [{Invite.Code}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Succesfully joined in [{Invite.Guild.Id}] with invite code [{Invite.Code}]");
                    dynamic json = JObject.Parse(Utils.DecompressResponse(res));

                    if (BypassRules && (bool)json.show_verification_form)
                    {
                        string first = GetRules(Invite, proxyClient);
                        string[] splitted = Strings.Split(first, "\"required\": true,");
                        string rules = splitted[0] + "\"response\": true, \"required\": true," + splitted[1];
                        this.BypassRules(Invite, rules, proxyClient);
                    }

                    if (BypassDC)
                    {
                        Thread.Sleep(3000);
                        List<DiscordMessage> messages = GetChannelMessages(CreateDM("703886990948565003", proxyClient), 1, proxyClient);
                        while (messages.Count == 0)
                        {
                            Thread.Sleep(1000);
                            messages = GetChannelMessages(CreateDM("703886990948565003", proxyClient), 1, proxyClient);
                        }

                        DiscordEmbed embed = messages[0].Embeds[0];
                        if (embed.Fields != null && embed.Fields.Count > 0)
                        {
                            string value = messages[0].Embeds[0].Fields[1].Value;
                            string link = value.Replace("[Click me to verify!](", "");
                            link = link.Replace(")", "");

                            Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Succesfully obtained Double Counter verification link ({link})");

                            KaliHttp.Get(link, new Dictionary<string, string>()
                            {
                                ["Host"] = "verify.dcounter.space",
                                ["Connection"] = "keep-alive",
                                ["sec-ch-ua"] = "\" Not;A Brand\";v=\"99\", \"Google Chrome\";v=\"97\", \"Chromium\";v=\"97\"",
                                ["sec-ch-ua-mobile"] = "?0",
                                ["sec-ch-ua-platform"] = "\"Windows\"",
                                ["Upgrade-Insecure-Requests"] = "1",
                                ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.71 Safari/537.36",
                                ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
                                ["Sec-Fetch-Site"] = "none",
                                ["Sec-Fetch-Mode"] = "navigate",
                                ["Sec-Fetch-User"] = "?1",
                                ["Sec-Fetch-Dest"] = "document",
                                ["Accept-Encoding"] = "gzip, deflate, br",
                                ["Accept-Language"] = "it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7",
                            }, proxyClient);
                        }
                    }
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to join in [{Invite.Guild.Id}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to join in [{Invite.Guild.Id}] » {Utils.DecompressResponse(res)}");
                }
            }
            catch
            {

            }
        }

        public void ClickButton(string ButtonId, string ChannelId, string MessageId, string GuildId, string ApplicationId, HttpProxyClient proxyClient)
        {
            try
            {
                string data = "{\"type\":3,\"guild_id\": \"" + MessageId + "\",\"channel_id\": \"" + ChannelId + "\",\"message_id\": \"" + MessageId + "\",\"application_id\": \"" + ApplicationId + "\",\"data\":{\"component_type\":2,\"custom_id\":\"" + ButtonId + "\"},\"message_flags\":0}";
                HttpResponse res = KaliHttp.Post("https://discord.com/api/v9/interactions", GetRequestHeaders(data), data, "application/json", proxyClient);
                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Succesfully clicked button [{ButtonId}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Succesfully clicked button [{ButtonId}]");
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to click button with Id [{ButtonId}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to click button with Id [{ButtonId}] » {Utils.DecompressResponse(res)}");
                }
            }
            catch
            {
            }
        }

        public void JoinThread(string ThreadId, HttpProxyClient proxyClient)
        {
            try
            {
                HttpResponse res = KaliHttp.Post("https://discord.com/api/v9/channels/" + ThreadId + "/thread-members/@me", GetHeaders(), "", "application/json", proxyClient);
                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Succesfully joined in thread [{ThreadId}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Succesfully joined in thread [{ThreadId}]");
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to join in thread [{ThreadId}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to join in thread [{ThreadId}] » {Utils.DecompressResponse(res)}");
                }
            }
            catch
            {
            }
        }

        public void LeaveThread(string ThreadId, HttpProxyClient proxyClient)
        {
            try
            {

                HttpResponse res = KaliHttp.Delete("https://discord.com/api/v9/channels/" + ThreadId + "/thread-members/@me", GetHeaders(), "", "application/json", proxyClient);
                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Succesfully left from thread [{ThreadId}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Succesfully left from thread [{ThreadId}]");
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to leave from thread [{ThreadId}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to leave from thread [{ThreadId}] » {Utils.DecompressResponse(res)}");
                }
            }
            catch
            {
            }
        }

        private void GetGuild(DiscordInvite Invite, string XCP, HttpProxyClient proxyClient)
        {
            try
            {
                Dictionary<string, string> Headers = new Dictionary<string, string>()
                {
                    ["Accept"] = "*/*",
                    ["Accept-Encoding"] = "gzip, deflate, br",
                    ["Authorization"] = Token,
                    ["Content-Lenght"] = "2",
                    ["Content-Type"] = "application/json",
                    ["Cookie"] = Utils.Cookies,
                    ["Origin"] = "https://discord.com",
                    ["Referer"] = "https://discord.com/channels/@me",
                    ["Host"] = "discord.com",
                    ["Connection"] = "keep-alive",
                    ["User-Agent"] = Utils.UserAgent,
                    ["X-Debug-Options"] = "bugReporterEnabled",
                    ["X-Discord-Locale"] = "it",
                    ["X-Context-Properties"] = XCP,
                    ["X-Super-Properties"] = Utils.XSP,
                };

                HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/invites/" + Invite.Code, GetHeaders(), proxyClient);
            }
            catch
            {
            }
        }

        private string GetRules(DiscordInvite Invite, HttpProxyClient proxyClient)
        {
            try
            {
                if (Invite.Guild.Id != "")
                {
                    HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/invites/" + Invite.Code, GetHeaders(), proxyClient);
                    return Utils.DecompressResponse(res);
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        public void CreateThread(string ChannelId, string Name, string MessageId, HttpProxyClient proxyClient)
        {
            try
            {
                string Url = "https://discord.com/api/v9/channels/" + ChannelId + "/threads";
                string Data = "{\"name\": \"" + Name + "\", \"type\": 11, \"auto_archive_duration\": 1440, \"location\": \"Thread Browser Toolbar\"}";
                if (MessageId != "")
                {
                    Url = "https://discord.com/api/v9/channels/" + ChannelId + "/messages/" + MessageId + "/threads";
                    Data = "{\"name\": \"" + Name + "\", \"type\": 11, \"auto_archive_duration\": 1440, \"location\": \"Message\"}";
                }

                HttpResponse res = KaliHttp.Post(Url, GetRequestHeaders(Data), Data, "application/json", proxyClient);
                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Created thread in [{ChannelId}] with name [{Name}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Created thread in [{ChannelId}] with name [{Name}]");
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to create thread in [{ChannelId}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to create thread in [{ChannelId}] » {Utils.DecompressResponse(res)}");
                }
            }
            catch
            {

            }
        }

        private void BypassRules(DiscordInvite Invite, string Rules, HttpProxyClient proxyClient)
        {
            try
            {
                if (Invite.Guild.Id != "")
                {
                    HttpResponse res = KaliHttp.Put("https://discord.com/api/v9/guilds/" + Invite.Guild.Id + "/requests/@me", GetRequestHeaders(Rules), Rules, "application/json", proxyClient);
                    if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                    {
                        if (proxyClient != null)
                        {
                            Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Bypassed rules for [{Invite.Guild.Id}]");
                        }
                        else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Bypassed rules for [{Invite.Guild.Id}]");
                    }
                    else
                    {
                        if (proxyClient != null)
                        {
                            Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to bypass rules for [{Invite.Guild.Id}] » {Utils.DecompressResponse(res)}");
                        }
                        else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to bypass rules for [{Invite.Guild.Id}] » {Utils.DecompressResponse(res)}");
                    }
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to bypass rules for [{Invite.Guild.Id}] » Guild Id is missing");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to bypass rules for [{Invite.Guild.Id}] » Guild Id is missing");
                }
            }
            catch
            {

            }
        }

        public void SetNickname(string Nickname, string GuildId, HttpProxyClient proxyClient)
        {
            try
            {
                string Data = "{\"nick\":\"" + Nickname + "\"}";
                HttpResponse res = KaliHttp.Patch("https://discord.com/api/v9/guilds/" + GuildId + "/members/@me", GetRequestHeaders(Data), Data, "application/json", proxyClient);
                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Changed nickname [{Nickname}] in guild [{GuildId}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Changed nickname [{Nickname}] in guild [{GuildId}]");
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to change nickname [{GuildId}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to change nickname [{GuildId}] » {Utils.DecompressResponse(res)}");
                }
            }
            catch
            {

            }
        }

        public void SetBio(string Biography, HttpProxyClient proxyClient)
        {
            try
            {
                string Data = "{\"bio\": \"" + Biography + "\"}";
                HttpResponse res = KaliHttp.Patch("https://discord.com/api/v9/users/@me", GetRequestHeaders(Data), Data, "application/json", proxyClient);
            }
            catch
            {
            }
        }

        public List<PrivateChannel> GetPrivateChannels(HttpProxyClient proxyClient)
        {
            List<PrivateChannel> PrivateChannels = new List<PrivateChannel>();
            try
            {
                HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/users/@me/channels", GetHeaders(), proxyClient);
                var channels = JsonConvert.DeserializeObject<List<PrivateChannel>>(Utils.DecompressResponse(res));
                PrivateChannels.AddRange(channels);
                return PrivateChannels;
            }
            catch
            {
                return PrivateChannels;
            }
        }

        public List<DiscordGuild> GetGuilds(HttpProxyClient proxyClient)
        {
            List<DiscordGuild> Guilds = new List<DiscordGuild>();
            try
            {
                HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/users/@me/guilds", GetHeaders(), proxyClient);
                var guilds = JsonConvert.DeserializeObject<List<DiscordGuild>>(Utils.DecompressResponse(res));
                Guilds.AddRange(guilds);
                return Guilds;
            }
            catch
            {
                return Guilds;
            }
        }

        public List<DiscordMessage> GetChannelMessages(string ChannelId, int Limit, HttpProxyClient proxyClient)
        {
            List<DiscordMessage> Messages = new List<DiscordMessage>();
            try
            {
                HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/channels/" + ChannelId + "/messages?limit=" + Limit.ToString(), GetHeaders(), proxyClient);
                var messages = JsonConvert.DeserializeObject<List<DiscordMessage>>(Utils.DecompressResponse(res));
                Messages.AddRange(messages);
                return Messages;
            }
            catch
            {
                return Messages;
            }
        }

        public string CreateDM(string UserId, HttpProxyClient proxyClient)
        {
            try
            {
                string Data = "{\"recipients\": [\"" + UserId + "\"]}";

                Dictionary<string, string> Headers = new Dictionary<string, string>()
                {
                    ["Accept"] = "*/*",
                    ["Accept-Encoding"] = "gzip, deflate, br",
                    ["Accept-Language"] = "en-US,it-IT;q=0.9",
                    ["Authorization"] = Token,
                    ["Content-Lenght"] = Data.Length.ToString(),
                    ["Content-Type"] = "application/json",
                    ["Cookie"] = Utils.Cookies,
                    ["Origin"] = "https://discord.com",
                    ["Referer"] = "https://discord.com/channels/@me",
                    ["Host"] = "discord.com",
                    ["Connection"] = "keep-alive",
                    ["User-Agent"] = Utils.UserAgent,
                    ["X-Debug-Options"] = "bugReporterEnabled",
                    ["X-Discord-Locale"] = "it",
                    ["X-Context-Properties"] = "e30=",
                    ["X-Super-Properties"] = Utils.XSP,
                };

                HttpResponse res = KaliHttp.Post("https://discord.com/api/v9/users/@me/channels", Headers, Data, "application/json", proxyClient);
                dynamic json = JObject.Parse(Utils.DecompressResponse(res));
                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Succesfully created a Direct Message with [{UserId}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Succesfully created a Direct Message with [{UserId}]");
                    return (string)json.id;
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to create a Direct Message with [{UserId}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to create a Direct Message with [{UserId}] » {Utils.DecompressResponse(res)}");
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }

        public void CloseDM(string ChannelId, HttpProxyClient proxyClient)
        {
            try
            {
                HttpResponse res = KaliHttp.Delete("https://discord.com/api/v9/channels/" + ChannelId, GetHeaders(), "", "", proxyClient);
                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Succesfully closed Direct Message [{ChannelId}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Succesfully closed Direct Message [{ChannelId}]");
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to close Direct Message [{ChannelId}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to close Direct Message [{ChannelId}] » {Utils.DecompressResponse(res)}");
                }
            }
            catch
            {
            }
        }

        public void SendMessage(string ChannelId, string Message, string Reference, bool GhostPingV2, string MessageToRemove, HttpProxyClient proxyClient)
        {
            try
            {

                string Data = "";
                if (Reference == string.Empty)
                {
                    Data = "{\"content\": \"" + Message + "\", \"tts\":false}";
                }
                else
                {
                    Data = "{\"content\": \"" + Message + "\", \"tts\":false, \"message_reference\": {\"channel_id\": \"" + ChannelId + "\", \"message_id\": \"" + Reference + "\"}}";
                }

                HttpResponse res = KaliHttp.Post("https://discord.com/api/v9/channels/" + ChannelId + "/messages", GetRequestHeaders(Data), Data, "application/json", proxyClient);
                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Spamming in [{ChannelId}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Spamming in [{ChannelId}]");
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to spam in [{ChannelId}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to spam in [{ChannelId}] » {Utils.DecompressResponse(res)}");
                }

                if (GhostPingV2)
                {
                    dynamic json = JObject.Parse(Utils.DecompressResponse(res));
                    ModifyMessage(ChannelId, (string)json.id, Message, MessageToRemove, proxyClient);
                }
            }
            catch
            {

            }
        }

        public void ModifyMessage(string ChannelId, string MessageId, string Text, string MessageToRemove, HttpProxyClient proxyClient)
        {
            try
            {
                string Message = Utils.ReplaceText(Text, MessageToRemove, "");
                string Data = "{\"content\":\"" + Message + "\"}";

                KaliHttp.Patch("https://discord.com/api/v9/channels/" + ChannelId + "/messages/" + MessageId, GetRequestHeaders(Data), Data, "application/json", proxyClient);
            }
            catch
            {
            }
        }

        public void RemoveReaction(string MessageId, string ChannelId, string Emoji, HttpProxyClient proxyClient)
        {
            try
            {
                var Emote = System.Web.HttpUtility.UrlEncode(Emoji);
                HttpResponse res = KaliHttp.Delete("https://discord.com/api/v9/channels/" + ChannelId + "/messages/" + MessageId + "/reactions/" + Emote + "/@me", GetHeaders(), "", "", proxyClient);

                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Succesfully removed reaction {Emoji} from message [{MessageId}] in channel [{ChannelId}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Succesfully removed reaction {Emoji} from message [{MessageId}] in channel [{ChannelId}]");
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to remove reaction {Emoji} from message [{MessageId}] in channel [{ChannelId}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to remove reaction {Emoji} from message [{MessageId}] in channel [{ChannelId}] » {Utils.DecompressResponse(res)}");
                }
            }
            catch
            {

            }
        }

        public void AddReaction(string MessageId, string ChannelId, string Emoji, HttpProxyClient proxyClient)
        {
            try
            {
                var Emote = System.Web.HttpUtility.UrlEncode(Emoji);
                HttpResponse res = KaliHttp.Put("https://discord.com/api/v9/channels/" + ChannelId + "/messages/" + MessageId + "/reactions/" + Emote + "/@me", GetHeaders(), "", "", proxyClient);

                if (res.StatusCode == HttpStatusCode.Created || res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.NoContent || res.StatusCode == HttpStatusCode.None)
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Succesfully added reaction {Emoji} from message [{MessageId}] in channel [{ChannelId}]");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Succesfully added reaction {Emoji} from message [{MessageId}] in channel [{ChannelId}]");
                }
                else
                {
                    if (proxyClient != null)
                    {
                        Utils.AddLogsLine($"[{proxyClient.Host}:{proxyClient.Port}] ({Token}) ⇒ Failed to add reaction {Emoji} in message [{MessageId}] in channel [{ChannelId}] » {Utils.DecompressResponse(res)}");
                    }
                    else Utils.AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] ({Token}) ⇒ Failed to add reaction {Emoji} in message [{MessageId}] in channel [{ChannelId}] » {Utils.DecompressResponse(res)}");
                }
            }
            catch
            {

            }
        }

    }
}
