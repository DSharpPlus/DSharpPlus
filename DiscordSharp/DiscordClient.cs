using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using WebSocketSharp;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using DiscordSharp.Events;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DiscordSharp
{
    class DiscordLoginResult
    {
        public string token { get; set; }
        public string[] password { get; set; } //errors
        public string[] email { get; set; }
    }

    class DiscordInitObj
    {
        public int op { get; set; }
        public DiscordD d { get; set; }
        public DiscordInitObj() { d = new DiscordD(); }
        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    class DiscordD
    {
        public string token { get; set; }
        public DiscordProperties properties { get; set; }
        public DiscordD() { properties = new DiscordProperties(); }
        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    class DiscordProperties
    {
        public string os { get; set; }
        public string browser { get; set; }
        public string device
        {
            get
            {
                return "DiscordSharp Bot";
            }
        }
        public string referrer { get; set; }
        public string referring_domain { get; set; }

        public DiscordProperties()
        {
            os = "Linux";
        }
        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public enum DiscordMessageType
    {
        PRIVATE, CHANNEL
    }

    public delegate void DiscordMessageReceived(object sender, DiscordMessageEventArgs e);
    public delegate void DiscordConnect(object sender, DiscordConnectEventArgs e);
    public delegate void DiscordSocketOpened(object sender, DiscordSocketOpenedEventArgs e);
    public delegate void DiscordSocketClosed(object sender, DiscordSocketClosedEventArgs e);
    public delegate void DiscordChannelCreate(object sender, DiscordChannelCreateEventArgs e);
    public delegate void DiscordPrivateChannelCreate(object sender, DiscordPrivateChannelEventArgs e);
    public delegate void DiscordPrivateMessageReceived(object sender, DiscordPrivateMessageEventArgs e);
    public delegate void DiscordKeepAliveSent(object sender, DiscordKeepAliveSentEventArgs e);
    public delegate void DiscordMention(object sender, DiscordMessageEventArgs e);
    public delegate void DiscordTypingStart(object sendr, DiscordTypingStartEventArgs e);
    public delegate void DiscordMessageUpdate(object sender, DiscordMessageEventArgs e);

    public class DiscordClient
    {
        public string[] Servers
        {
            get; set;
        }
        public string[] Internals { get; set; }
        public string[] DirectMessages { get; set; }
        public string id { get; internal set; }
        public string username { get; internal set; }
        public DiscordLoginInformation LoginInformation { get; set; }
        public string token { get; set; }
        public string sessionKey { get; set; }
        private string Cookie { get; set; }

        WebSocket ws;
        private List<DiscordServer> ServersList { get; set; }
        public List<DiscordServer> GetServersList() { return this.ServersList; }
        private List<DiscordPrivateChannel> PrivateChannels = new List<DiscordPrivateChannel>();

        public event DiscordMessageReceived MessageReceived;
        public event DiscordConnect Connected;
        public event DiscordSocketOpened SocketOpened;
        public event DiscordSocketClosed SocketClosed;
        public event DiscordChannelCreate ChannelCreated;
        public event DiscordPrivateChannelCreate PrivateChannelCreated;
        public event DiscordPrivateMessageReceived PrivateMessageReceived;
        public event DiscordKeepAliveSent KeepAliveSent;
        public event DiscordMention MentionReceived;
        public event DiscordTypingStart UserTypingStart;
        public event DiscordMessageUpdate MessageEdited;
        
        public DiscordClient()
        {
#if DEBUG
            ni = new NotifyIcon();
            ni.Icon = Properties.Resources.ico;
            ni.Text = "DiscordSharp Debug";
            ni.BalloonTipTitle = "DiscordSharp Debug";
            ni.Visible = true;
#endif
            if (LoginInformation == null)
                LoginInformation = new DiscordLoginInformation();
        }
        
        //eh
        private void GetChannelsList(JObject m)
        {
            if (ServersList == null)
                ServersList = new List<DiscordServer>();
            foreach(var j in m["d"]["guilds"])
            {
                DiscordServer temp = new DiscordServer();
                temp.id = j["id"].ToString();
                temp.name = j["name"].ToString();
                temp.owner_id = j["owner_id"].ToString();
                List<DiscordChannel> tempSubs = new List<DiscordChannel>();
                foreach(var u in j["channels"])
                {
                    DiscordChannel tempSub = new DiscordChannel();
                    tempSub.id = u["id"].ToString();
                    tempSub.name = u["name"].ToString();
                    tempSub.type = u["type"].ToString();
                    tempSubs.Add(tempSub);
                }
                temp.channels = tempSubs;
                foreach(var mm in j["members"])
                {
                    DiscordMember member = new DiscordMember();
                    member.user.id = mm["user"]["id"].ToString();
                    member.user.username = mm["user"]["username"].ToString();
                    temp.members.Add(member);
                }
                ServersList.Add(temp);
            }
            
        }

        /// <summary>
        /// Sends a message to a channel, what else did you expect?
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        public void SendMessageToChannel(string message, DiscordChannel channel)
        {
            //DiscordServer serverID = ServerFromName(server);
           //string initMessage = "{\"recipient_id\":" + channelID.id + "}";
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/channels/" + channel.id + "/messages");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(JsonConvert.SerializeObject(GenerateMessage(message)));
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    string result = s.ReadToEnd();
                    Console.WriteLine("!!! " + result);   
                }
            }
        }

        //Special thanks to the node-discord developer, izy521, for helping me out with this :D
        public void SendMessageToUser(string message, DiscordMember member)
        {
            string initMessage = "{\"recipient_id\":" + member.user.id + "}";
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/users/" + id + "/channels");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(initMessage);
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = JObject.Parse(sr.ReadToEnd());
                    SendActualMessage(result["id"].ToString(), message);
                }
            }
            catch
            {
                //shouldn't even have to worry about this..
            }
        }

        private void SendActualMessage(string id, string message)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/channels/" + id + "/messages");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(JsonConvert.SerializeObject(GenerateMessage(message)));
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    string result = s.ReadToEnd();
                    Console.WriteLine("!!! " + result);
                }
            }
        }

        private DiscordMessage GenerateMessage(string message)
        {
            DiscordMessage dm = new DiscordMessage();
            List<string> foundIDS = new List<string>();
            Regex r = new Regex("\\@\\w+");
            List<KeyValuePair<string, string>> toReplace = new List<KeyValuePair<string, string>>();
            foreach (Match m in r.Matches(message))
            {
                if (message[m.Index - 1] == '<')
                    continue;
                DiscordMember user = ServersList.Find(x => x.members.Find(y => y.user.username == m.Value.Trim('@')) != null).members.Find(y=>y.user.username == m.Value.Trim('@'));
                foundIDS.Add(user.user.id);
                toReplace.Add(new KeyValuePair<string, string>(m.Value, user.user.id));
            }
            foreach(var k in toReplace)
            {
                message = message.Replace(k.Key, "<@" + k.Value + ">");
            }

            dm.content = message;
            dm.mentions = foundIDS.ToArray();
            return dm;
        }
        
        public bool WebsocketAlive
        {
            get
            {
                return (ws != null) ? ws.IsAlive : false;
            }
        }

        public void ConnectAndReadMessages()
        {

            ws = new WebSocket("wss://discordapp.com/hub");
                ws.OnMessage += (sender, e) =>
                {
                    var message = JObject.Parse(e.Data);
                    switch(message["t"].ToString())
                    {
                        case ("READY"):
                            this.username = message["d"]["user"]["username"].ToString();
                            this.id = message["d"]["user"]["id"].ToString();
                            HeartbeatInterval = int.Parse(message["d"]["heartbeat_interval"].ToString());
                            GetChannelsList(message);
                            if (Connected != null)
                                Connected(this, new DiscordConnectEventArgs { username = this.username, id = this.id }); //Since I already know someone will ask for it.
                            break;
                        case ("MESSAGE_UPDATE"):
                            DiscordServer pserver = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["channel_id"].ToString()) != null);
                            DiscordChannel pchannel = pserver.channels.Find(x => x.id == message["d"]["channel_id"].ToString());
                            if(pchannel != null)
                            {
                                if (MessageEdited != null)
                                    MessageEdited(this, new DiscordMessageEventArgs {
                                        author = pserver.members.Find(x => x.user.id == message["d"]["author"]["id"].ToString()),
                                        Channel = pchannel,
                                        message = message["d"]["content"].ToString(), MessageType = DiscordMessageType.CHANNEL
                                    });
                            }
                            break;
                        case ("TYPING_START"):
                            DiscordServer server = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["channel_id"].ToString()) != null);
                            if (server != null)
                            {
                                DiscordChannel channel = server.channels.Find(x => x.id == message["d"]["channel_id"].ToString());
                                DiscordMember user = server.members.Find(x => x.user.id == message["d"]["user_id"].ToString());
                                if (UserTypingStart != null)
                                    UserTypingStart(this, new DiscordTypingStartEventArgs { user = user, channel = channel, timestamp = int.Parse(message["d"]["timestamp"].ToString()) });
                            }
                            break;
                        case ("MESSAGE_CREATE"):
                            try
                            {
                                string tempChannelID = message["d"]["channel_id"].ToString();

                                var foundServerChannel = ServersList.Find(x => x.channels.Find(y => y.id == tempChannelID) != null);
                                if(foundServerChannel == null)
                                {
                                    var foundPM = PrivateChannels.Find(x => x.id == message["d"]["channel_id"].ToString());
                                    DiscordPrivateMessageEventArgs dpmea = new DiscordPrivateMessageEventArgs();
                                    dpmea.Channel = foundPM;
                                    dpmea.message = message["d"]["content"].ToString();
                                    DiscordMember tempMember = new DiscordMember();
                                    tempMember.user.username = message["d"]["author"]["username"].ToString();
                                    tempMember.user.id = message["d"]["author"]["id"].ToString();
                                    dpmea.author = tempMember;

                                    if(PrivateMessageReceived != null)
                                        PrivateMessageReceived(this, dpmea);
                                }
                                else
                                {
                                    DiscordMessageEventArgs dmea = new DiscordMessageEventArgs();
                                    dmea.Channel = foundServerChannel.channels.Find(y=>y.id == tempChannelID);
                                    dmea.message = message["d"]["content"].ToString();

                                    DiscordMember tempMember = new DiscordMember();
                                    tempMember = foundServerChannel.members.Find(x => x.user.id == message["d"]["author"]["id"].ToString());
                                    dmea.author = tempMember;

                                    Regex r = new Regex("\\d+");
                                    foreach(Match mm in r.Matches(dmea.message))
                                        if (mm.Value == id) 
                                            if (MentionReceived != null)
                                                MentionReceived(this, dmea);
                                    if (MessageReceived != null)
                                        MessageReceived(this, dmea);
                                }
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("!!! {0}", ex.Message);
                                Console.Beep(100, 1000);
                            }
                            break;
                        case ("CHANNEL_CREATE"):
                            if(message["d"]["is_private"].ToString().ToLower() == "false")
                            {
                                var foundServer = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
                                if(foundServer != null)
                                {
                                    DiscordChannel tempChannel = new DiscordChannel();
                                    tempChannel.name = message["d"]["name"].ToString();
                                    tempChannel.type = message["d"]["type"].ToString();
                                    tempChannel.id = message["d"]["id"].ToString();
                                    foundServer.channels.Add(tempChannel);
                                    DiscordChannelCreateEventArgs fae = new DiscordChannelCreateEventArgs();
                                    fae.ChannelCreated = tempChannel;
                                    fae.ChannelType = DiscordChannelCreateType.CHANNEL;
                                    if (ChannelCreated != null)
                                        ChannelCreated(this, fae);
                                }
                            }
                            else
                            {
                                DiscordPrivateChannel tempPrivate = new DiscordPrivateChannel();
                                tempPrivate.id = message["d"]["id"].ToString();
                                DiscordRecipient tempRec = new DiscordRecipient();
                                tempRec.id = message["d"]["recipient"]["id"].ToString();
                                tempRec.username = message["d"]["recipient"]["username"].ToString();
                                tempPrivate.recipient = tempRec;
                                PrivateChannels.Add(tempPrivate);
                                DiscordPrivateChannelEventArgs fak = new DiscordPrivateChannelEventArgs { ChannelType = DiscordChannelCreateType.PRIVATE, ChannelCreated = tempPrivate};
                                if (PrivateChannelCreated != null)
                                    PrivateChannelCreated(this, fak);
                            }
                            break;
#if DEBUG
                            default:
                            ni.BalloonTipText = "Check console! New message type!";
                            ni.ShowBalloonTip(10 * 1000);
                            Console.WriteLine(message);
                            break;
#endif
                    }
                };
                ws.OnOpen += (sender, e) => 
                {
                    DiscordInitObj initObj = new DiscordInitObj();
                    initObj.op = 2;
                    initObj.d.token = this.token;
                    string json = initObj.AsJson();
                    ws.Send(json);
                    if (SocketOpened != null)
                        SocketOpened(this, null);
                    Thread keepAlivetimer = new Thread(KeepAlive);
                    keepAlivetimer.Start();
                };
                ws.OnClose += (sender, e) =>
                {
                    DiscordSocketClosedEventArgs scev = new DiscordSocketClosedEventArgs();
                    scev.Code = e.Code;
                    scev.Reason = e.Reason;
                    scev.WasClean = e.WasClean;
                    if (SocketClosed != null)
                        SocketClosed(this, scev);
                };
                ws.Connect();
        }

#if DEBUG
        NotifyIcon ni;
#endif

        private JObject ServerInfo(string channelOrServerId)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/guilds/" + channelOrServerId);
            httpWebRequest.Headers["authorization"] = token;
            httpWebRequest.Method = "GET";

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    return JObject.Parse(result);
                }
            }
            catch(WebException e)
            {
                var result = "";
                using (var sr = new StreamReader(e.Response.GetResponseStream()))
                    result = sr.ReadToEnd();

                Console.WriteLine("!!!" + result);
                return null;
            }
        }

        private int HeartbeatInterval = 41250;
        private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private void KeepAlive()
        {
            string keepAliveJson = "{\"op\":1, \"d\":" + DateTime.Now.Millisecond + "}";
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += (sender, e) =>
            {
                if (ws != null)
                    if (ws.IsAlive)
                    {
                        int unixTime = (int)(DateTime.UtcNow - epoch).TotalMilliseconds;
                        keepAliveJson = "{\"op\":1, \"d\":\"" + unixTime + "\"}";
                        ws.Send(keepAliveJson);
                        if (KeepAliveSent != null)
                            KeepAliveSent(this, new DiscordKeepAliveSentEventArgs { SentAt = DateTime.Now, JsonSent = keepAliveJson } );
                    }
            };
            timer.Interval = HeartbeatInterval;
            timer.Enabled = true;
        }

        //Thread ConnectReadMessagesThread;
        public void Dispose()
        {
            ws.Close();
            ws = null;
            ServersList = null;
            PrivateChannels = null;
            this.id = null;
            this.token = null;
            this.sessionKey = null;
            this.username = null;
            this.LoginInformation = null;
        }

        /// <summary>
        /// Sends a login request.
        /// </summary>
        /// <returns>The token if login was succesful, or null if not</returns>
        public string SendLoginRequest()
        {
            if (LoginInformation == null || LoginInformation.email == null || LoginInformation.email[0].Trim() == "" || LoginInformation.password == null || LoginInformation.password[0].Trim() == "")
                throw new ArgumentNullException("You didn't supply login information!");

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/auth/login");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 30000;

            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                sw.Write(LoginInformation.AsJson());
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    DiscordLoginResult dlr = JsonConvert.DeserializeObject<DiscordLoginResult>(result);
                    if (dlr.token != null || dlr.token.Trim() != "")
                    {
                        this.token = dlr.token;
                        try
                        {
                            string sessionKeyHeader = httpResponse.Headers["set-cookie"].ToString();
                            string[] split = sessionKeyHeader.Split(new char[] { '=', ';' }, 3);
                            sessionKey = split[1];
                        }
                        catch
                        {/*no session key fo u!*/}
                        return token;
                    }
                    else
                        return null;
                }
            }
            catch(WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    string result = s.ReadToEnd();
                    DiscordLoginResult jresult = JsonConvert.DeserializeObject<DiscordLoginResult>(result);
                    if (jresult.password != null)
                        throw new DiscordLoginException(jresult.password[0]);
                    if (jresult.email != null)
                        throw new DiscordLoginException(jresult.email[0]);
                }
            }
            return null;
        }
    }
}
