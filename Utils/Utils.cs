using BrotliSharpLib;
using KaliSpammer.Discord;
using Leaf.xNet;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace KaliSpammer
{
    struct Utils
    {
        public static List<string> Proxies = new List<string>(), Users = new List<string>(), Queue = new List<string>();
        public static List<DiscordClient> Clients = new List<DiscordClient>();
        public static string ChannelID = "", GuildID = "";
        public static bool Logs = false, WantToParse = false, DoneParsingGuild = false;
        public static Random random = new Random();
        public static string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/1.0.9003 Chrome/91.0.4472.164 Electron/13.4.0 Safari/537.36";
        public static string XSP = "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRGlzY29yZCBDbGllbnQiLCJyZWxlYXNlX2NoYW5uZWwiOiJzdGFibGUiLCJjbGllbnRfdmVyc2lvbiI6IjEuMC45MDAzIiwib3NfdmVyc2lvbiI6IjEwLjAuMTkwNDMiLCJvc19hcmNoIjoieDY0Iiwic3lzdGVtX2xvY2FsZSI6Iml0IiwiY2xpZW50X2J1aWxkX251bWJlciI6MTA4OTI0LCJjbGllbnRfZXZlbnRfc291cmNlIjpudWxsfQ==";
        public static string Cookies = "__dcfduid=f04b1d3db70d6511bd2958c4b6556f47; __sdcfduid=fe121110f45011ebab1bf5eb50c3455293eb8d1f516f6c7cac46da4d06ef271230585d35a96d560113f26965b83cfebb; __stripe_mid=13afa8b7-e492-4a8b-ab56-bfc7218333484dcbfc; locale=it";

        public static string DecompressResponse(HttpResponse response)
        {
            try
            {
                if (response["content-encoding"].Equals("gzip"))
                {
                    MemoryStream memoryStream = new MemoryStream(response.ToBytes());
                    MemoryStream final = new MemoryStream();

                    GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                    gZipStream.CopyTo(final);

                    return Encoding.UTF8.GetString(final.ToArray());
                }
                else if (response["content-encoding"].Equals("deflate"))
                {
                    MemoryStream memoryStream = new MemoryStream(response.ToBytes());
                    MemoryStream final = new MemoryStream();

                    DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
                    deflateStream.CopyTo(final);

                    return Encoding.UTF8.GetString(final.ToArray());
                }
                else if (response["content-encoding"].Equals("br"))
                {
                    MemoryStream memoryStream = new MemoryStream(response.ToBytes());
                    MemoryStream final = new MemoryStream();

                    BrotliStream brotliStream = new BrotliStream(memoryStream, CompressionMode.Decompress);
                    brotliStream.CopyTo(final);

                    return Encoding.UTF8.GetString(final.ToArray());
                }
                else
                {
                    return response.ToString();
                }
            }
            catch
            {
                return response.ToString();
            }
        }
        public static bool IsProxyValid(string proxy)
        {
            try
            {
                string[] splitted = Strings.Split(proxy, ":");
                if (Information.IsNumeric(splitted[1]))
                {
                    if (PingHost(splitted[0], int.Parse(splitted[1])))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool PingHost(string strIP, int intPort)
        {
            try
            {
                bool blProxy = false;
                try
                {
                    TcpClient client = new TcpClient(strIP, intPort);
                    blProxy = true;
                }
                catch (Exception)
                {
                    return false;
                }
                return blProxy;
            }
            catch
            {
                return false;
            }
        }

        public static HttpProxyClient ParseProxy(string Proxy)
        {
            try
            {
                HttpProxyClient proxy = null;
                string[] splitted = Strings.Split(Proxy, ":");
                if (splitted.Length > 2)
                {
                    proxy = new HttpProxyClient(splitted[0], int.Parse(splitted[1]), splitted[2], splitted[3]);
                }
                else
                {
                    proxy = new HttpProxyClient(splitted[0], int.Parse(splitted[1]));

                }

                return proxy;
            }
            catch
            {
                return new HttpProxyClient();
            }
        }

        public static HttpProxyClient GetRandomProxy()
        {
            try
            {
                HttpProxyClient proxy = null;
                if (Proxies.Count > 0)
                {
                    string[] splitted = Strings.Split(Proxies[new Random().Next(0, Proxies.Count)], ":");
                    if (splitted.Length > 2)
                    {
                        proxy = new HttpProxyClient(splitted[0], int.Parse(splitted[1]), splitted[2], splitted[3]);
                    }
                    else
                    {
                        proxy = new HttpProxyClient(splitted[0], int.Parse(splitted[1]));

                    }

                    return proxy;
                }
                else return new HttpProxyClient();
            }
            catch
            {
                return new HttpProxyClient();
            }
        }

        public static bool IsTokenValid(string token, HttpProxyClient proxyClient)
        {
            try
            {
                Dictionary<string, string> Headers = new Dictionary<string, string>()
                {
                    ["Accept"] = "*/*",
                    ["Accept-Encoding"] = "gzip, deflate, br",
                    ["Authorization"] = token,
                    ["Connection"] = "keep-alive",
                    ["User-Agent"] = UserAgent,
                    ["X-Super-Properties"] = XSP,
                };

                HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/users/@me/library", Headers, proxyClient);
                if (!res.IsOK) return false;
                else return true;
            }
            catch
            {
                return false;
            }
        }

        public static void AddLogsLine(string text)
        {
            try
            {
                if (Logs)
                {
                    Queue.Add(text);
                }
            }
            catch
            {
            }
        }

        public static string GetXCP(DiscordInvite Invite, bool GroupMode)
        {
            string XCP = "eyJsb2NhdGlvbiI6IkpvaW4gR3VpbGQifQ==";
            try
            {
                if (Invite.Channel != null)
                {
                    if (GroupMode)
                    {
                        XCP = "{\"location\":\"Join Guild\",\"location_channel_id\":\"" + Invite.Channel.Id + "\",\"location_channel_type\":" + Invite.Channel.Type + "}";
                        AddLogsLine($"[{DateTime.Now}] (Internal Logger) ⇒ Succesfully built X-Context-Properties. Lenght is: {XCP.Length}");
                    }
                    else
                    {
                        if (Invite.Guild != null)
                        {
                            XCP = "{\"location\":\"Join Guild\",\"location_guild_id\":\"" + Invite.Guild.Id + "\",\"location_channel_id\":\"" + Invite.Channel.Id + "\",\"location_channel_type\":" + Invite.Channel.Type + "}";
                            AddLogsLine($"[{DateTime.Now}] (Internal Logger) ⇒ Succesfully built X-Context-Properties. Lenght is: {XCP.Length}");
                        }
                        else
                        {
                            AddLogsLine($"[{DateTime.Now}] (Internal Logger) ⇒ Failed to get X-Context-Properties. Invite code: {Invite.Code}");
                            return XCP;
                        }
                    }
                }
                else
                {
                    AddLogsLine($"[{DateTime.Now}] (Internal Logger) ⇒ Failed to get X-Context-Properties. Invite code: {Invite.Code}");
                    return XCP;
                }

                return XCP;

            }
            catch
            {
                AddLogsLine($"[{DateTime.Now}] (Internal Logger) ⇒ Failed to get X-Context-Properties. Invite code: {Invite.Code}");
                return XCP;
            }
        }

        public static DiscordInvite GetInvite(string InviteCode, bool GroupMode, HttpProxyClient proxyClient)
        {
            try
            {
                string Link = "";
                if (GroupMode)
                {
                    Link = "https://discord.gg/" + InviteCode;
                }
                else Link = InviteCode;

                Dictionary<string, string> Headers = new Dictionary<string, string>()
                {
                    ["Accept"] = "*/*",
                    ["Connection"] = "keep-alive",
                };

                HttpResponse res = KaliHttp.Get("https://discord.com/api/v9/invites/" + InviteCode + "?inputValue=" + Link + "&with_counts=true&with_expiration=true", Headers, proxyClient);
                if (res.IsOK)
                {
                    AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] (Internal Logger) ⇒ Succesfully got invite {InviteCode}");
                    return JsonConvert.DeserializeObject<DiscordInvite>(res.ToString());
                }
                else
                {
                    AddLogsLine($"[{DateTime.UtcNow.ToShortTimeString()}] (Internal Logger) ⇒ Failed to get invite {InviteCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                AddLogsLine($"[{DateTime.Now}] (Internal Logger) ⇒ Unknown error ⇒ {ex.Message}");
                return null;
            }
        }

        public static string ReplaceText(string text, string search, string replace)
        {
            try
            {
                int s = text.IndexOf(search);

                return text.Substring(0, s) + replace + text.Substring(s + search.Length);
            }
            catch
            {
                return text;
            }
        }

        public static string GetLagMsg()
        {
            return ":flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg::flag_ad: :laughing: :brain: :flag_ac: :chains: :flag_bz: :face_in_clouds: :dart: :flag_dg:";
        }

        public static string RandomUser()
        {
            return "<@" + Users[random.Next(0, Users.Count())] + ">";
        }

        public static string RandomBestemmia()
        {
            string[] Bestemmie = Properties.Resources.Bestemmie.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            return Bestemmie[random.Next(0, Bestemmie.Length)];
        }

        public static string RandomEmoji()
        {
            string emojis = "green_apple:apple:pear:tangerine:lemon:banana:watermelon:grapes:coconut:pineapple:mango:peach:cherries:melon:strawberry:pretzel:french_bread:bread:bagel:croissant:pancakes:bacon:cut_of_meat:poultry_leg:meat_on_bone:burrito:stuffed_flatbread:sandwich:pizza:fries:hamburger:hotdog:egg:cooking:jack_o_lantern:robot:space_invader:alien:skull_crossbones:skull:poop:clown:japanese_goblin:japanese_ogre:imp:smiling_imp:cowboy:money_mouth:head_bandage:thermometer_face:mask:sneezing_face:face_vomiting:nauseated_face:woozy_face:dizzy_face:sleepy:drooling_face:sleeping:astonished:open_mouth:anguished:frowning:hushed:rolling_eyes:grimacing:expressionless:neutral_face:no_mouth:lying_face:shushing_face:face_with_hand_over_mouth:thinking:hugging:sweat:cold_sweat:fearful:scream:cold_face:hot_face:flushed:exploding_head:face_with_symbols_over_mouth:rage:angry:triumph:sob:cry:pleading_face:weary:tired_face:confounded:persevere:frowning2:slight_frown:confused:worried:pensive:disappointed:unamused:partying_face:star_struck:sunglasses:nerd:face_with_monocle:face_with_raised_eyebrow:zany_face:stuck_out_tongue_winking_eye:stuck_out_tongue_closed_eyes:stuck_out_tongue:yum:kissing_closed_eyes:kissing_smiling_eyes:kissing:kissing_heart:smiling_face_with_3_hearts:heart_eyes:relieved:wink:upside_down:slight_smile:innocent:blush:relaxed:joy:sweat_smile:laughing:grin:smile:smiley:grinning";
            string[] splitted = Strings.Split(emojis, ":");
            return ":" + splitted[random.Next(0, splitted.Length)] + ":";
        }

        public static string RandomInt()
        {
            return random.Next(1000, 9999).ToString();
        }

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
