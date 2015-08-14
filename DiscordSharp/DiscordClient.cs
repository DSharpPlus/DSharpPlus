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

    public delegate void DiscordMessageReceived(object sender, DiscordMessageEventArgs e);

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


        public DiscordClient()
        {
            if (LoginInformation == null)
                LoginInformation = new DiscordLoginInformation();
        }

        WebSocket ws;

        private List<DiscordServer> Channels { get; set; }

        public DiscordSubChannel SubFromID(string id)
        {
            foreach(DiscordServer c in Channels)
            {
                foreach(DiscordSubChannel sc in c.channels)
                {
                    if (sc.id == id)
                        return sc;
                }
            }
            return null;
        }

        public DiscordServer ServerFromID(string id)
        {
            foreach(DiscordServer c in Channels)
            {
                if (c.id == id)
                    return c;
            }
            return null;
        }

        public DiscordServer ServerFromName(string name)
        {
            foreach(DiscordServer c in Channels)
            {
                if (c.name == name)
                    return c;
            }
            return null;
        }

        public DiscordSubChannel ChannelFromName(DiscordServer server, string name)
        {
            foreach (DiscordServer s in Channels)
                if (s == server)
                    foreach (DiscordSubChannel c in s.channels)
                        if (c.name == name)
                            return c;

            return null;
        }

        private void GetChannelsList(JObject m)
        {
            if (Channels == null)
                Channels = new List<DiscordServer>();
            foreach(var j in m["d"]["guilds"])
            {
                DiscordServer temp = new DiscordServer();
                temp.id = j["id"].ToString();
                temp.name = j["name"].ToString();
                temp.owner_id = j["owner_id"].ToString();
                List<DiscordSubChannel> tempSubs = new List<DiscordSubChannel>();
                foreach(var u in j["channels"])
                {
                    DiscordSubChannel tempSub = new DiscordSubChannel();
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
                Channels.Add(temp);
            }
            
        }

        public void SendMessageToChannel(string message, string channel, string server)
        {
            DiscordServer serverID = ServerFromName(server);
            DiscordSubChannel channelID = ChannelFromName(serverID, channel);
            string initMessage = "{\"recipient_id\":" + channelID.id + "}";

            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/channels/" + channelID.id + "/messages");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                //sw.Write(initMessage);
                //sw.Flush();
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
                    //Console.WriteLine(result);
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
                    foreach(var servers in Channels)
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
                            Console.WriteLine("Welcome, " + username);
                            break;
                        case ("MESSAGE_CREATE"):
                            JObject serverInfo = ServerInfo(message["d"]["channel_id"].ToString());

                            DiscordMessageEventArgs temp = new DiscordMessageEventArgs();
                            temp.username = message["d"]["author"]["username"].ToString();
                            temp.message = message["d"]["content"].ToString();
                            temp.ChannelID = message["d"]["channel_id"].ToString();
                            temp.ChannelName = SubFromID(message["d"]["channel_id"].ToString()).name;
                            temp.ServerID = ServerInfo(message["d"]["channel_id"].ToString())["id"].ToString();
                            temp.ServerName = ServerInfo(message["d"]["channel_id"].ToString())["name"].ToString();
                            if (MessageReceived != null)
                                MessageReceived(this, temp);
                            //Console.WriteLine("[- Message from {0} on #{2} in {3}: {1}", 
                            //    message["d"]["author"]["username"].ToString(), 
                            //    message["d"]["content"].ToString(), 
                            //    SubFromID(message["d"]["channel_id"].ToString()).name, 
                            //    serverInfo["name"].ToString());
                            break;
                    }

                    //Console.WriteLine(message["t"].ToString());
                    //Console.WriteLine(e.Data.ToString());
                    
                };
                ws.OnOpen += (sender, e) => 
                {
                    DiscordInitObj initObj = new DiscordInitObj();
                    initObj.op = 2;
                    initObj.d.token = this.token;
                    string json = initObj.AsJson();
                    ws.Send(json);
                };
                ws.OnClose += (sender, e) =>
                {
                    Console.WriteLine("closed, rip. will be missed: {0}", e.Reason);
                };
                ws.Connect();
                while (ws.IsAlive) ;
        }

        //Headers to set
        //"cookie" = "session=" + cookie

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
            /*
            //"https://discordapp.com/api/guilds/" + channelOrServerId
            using (WebClient wc = new WebClient())
            {
                try
                {
                    JObject serverInfo = JObject.Parse(wc.DownloadString("http://discordapp.com/api/guilds/" + channelOrServerId));
                    return serverInfo;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("!!! " + ex.Message);
                }
            }*/
            //return null;
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
                        //httpResponse.Headers["set-cookie"] = token;

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
