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
                return "Luigibot";
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
        
        public event DiscordMessageReceived MessageReceived;
        public event DiscordConnect Connected;
        public event DiscordSocketOpened SocketOpened;
        public event DiscordSocketClosed SocketClosed;
        public event DiscordChannelCreate ChannelCreated;
        public event DiscordPrivateChannelCreate PrivateChannelCreated;
        public event DiscordPrivateMessageReceived PrivateMessageReceived;
        
        public DiscordClient()
        {
            if (LoginInformation == null)
                LoginInformation = new DiscordLoginInformation();
        }

        WebSocket ws;

        private List<DiscordServer> ServersList { get; set; }

        public DiscordChannel SubFromID(string id)
        {
            foreach(DiscordServer c in ServersList)
            {
                foreach(DiscordChannel sc in c.channels)
                {
                    if (sc.id == id)
                        return sc;
                }
            }
            return null;
        }

        public DiscordServer ServerFromID(string id)
        {
            foreach(DiscordServer c in ServersList)
            {
                if (c.id == id)
                    return c;
            }
            return null;
        }

        public DiscordServer ServerFromChannelID(string id)
        {
            foreach(DiscordServer c in ServersList)
            {
                foreach (DiscordChannel s in c.channels)
                    if (s.id == id)
                        return c;
            }
            return null;
        }

        public DiscordServer ServerFromName(string name)
        {
            foreach(DiscordServer c in ServersList)
            {
                if (c.name == name)
                    return c;
            }
            return null;
        }

        public DiscordChannel ChannelFromName(DiscordServer server, string name)
        {
            foreach (DiscordServer s in ServersList)
                if (s == server)
                    foreach (DiscordChannel c in s.channels)
                        if (c.name == name)
                            return c;

            return null;
        }

        //eh
        public List<DiscordServer> GetServersList() { return this.ServersList; }

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
            for(var i = 0; i < message.Length; i++)
            {
                if(message[i] == '@')
                {
                    if (message[i + 1] == '@') break;
                    var username = message.Substring(i + 1, message.IndexOf(' ', i) > -1 ? message.IndexOf(' ', i) : message.Length);
                    foreach(var servers in ServersList)
                    {
                        foreach(var user in servers.members)
                        {
                            if (username == user.user.username)
                                foundIDS.Add(user.user.id);
                        }
                    }
                }
            }
            dm.content = message;
            dm.mentions = foundIDS.ToArray();
            return dm;
        }

        private List<DiscordPrivateChannel> PrivateChannels = new List<DiscordPrivateChannel>();

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
                            GetChannelsList(message);
                            if (Connected != null)
                                Connected(this, new DiscordConnectEventArgs { username = this.username, id = this.id }); //Since I already know someone will ask for it.
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
                            //Console.WriteLine(message.ToString());
                            break;
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
                while (ws.IsAlive) ;
        }

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

        private void KeepAlive()
        {
            string keepAliveJson = "{\"op\":1, \"d\":\"" + DateTime.Now.ToString() + "\"}";
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 40000;
            t.Tick += (sender, e) =>
            {
                if (ws != null)
                    if (ws.IsAlive)
                    {
                        keepAliveJson = "{\"op\":1, \"d\":\"" + DateTime.Now.ToString() + "\"}";
                        ws.Send(keepAliveJson);
                        Console.WriteLine("sent keep alive");
                    }
            };
            t.Start();
        }


        public string SendLoginRequest()
        {
            if (LoginInformation == null || LoginInformation.email == null || LoginInformation.email[0].Trim() == "" || LoginInformation.password == null || LoginInformation.password[0].Trim() == "")
                throw new ArgumentNullException("You didn't supply login information!");

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/auth/login");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

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
                        string sessionKeyHeader = httpResponse.Headers["set-cookie"].ToString();

                        string[] split = sessionKeyHeader.Split(new char[] { '=', ';' }, 3);
                        sessionKey = split[1];
                        return "Logged in! Token: " + token;
                    }
                    else
                        return "No";
                }
            }
            catch(WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {

                    string result = s.ReadToEnd();
                    DiscordLoginResult jresult = JsonConvert.DeserializeObject<DiscordLoginResult>(result);
                    if (jresult.password != null)
                        return jresult.password[0];
                    if (jresult.email != null)
                        return jresult.email[0];
                }
                
            }
            return "?";
        }
    }
}
