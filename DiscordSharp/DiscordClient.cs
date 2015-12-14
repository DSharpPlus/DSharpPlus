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
using System.Drawing;

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
    public delegate void DiscordMessageUpdate(object sender, DiscordMessageEditedEventArgs e);
    public delegate void DiscordPresenceUpdate(object sender, DiscordPresenceUpdateEventArgs e);
    public delegate void DiscordURLUpdate(object sender, DiscordURLUpdateEventArgs e);
    public delegate void DiscordVoiceStateUpdate(object sender, DiscordVoiceStateUpdateEventArgs e);
    public delegate void DiscordLeftVoiceChannel(object sender, DiscordLeftVoiceChannelEventArgs e);
    public delegate void UnknownMessage(object sender, UnknownMessageEventArgs e);
    public delegate void DiscordMessageDeleted(object sender, DiscordMessageDeletedEventArgs e);
    public delegate void DiscordUserUpdate(object sender, DiscordUserUpdateEventArgs e);
    public delegate void DiscordGuildMemberAdded(object sender, DiscordGuildMemberAddEventArgs e);
    public delegate void DiscordGuildMemberRemoved(object sender, DiscordGuildMemberRemovedEventArgs e);
    public delegate void DiscordGuildCreate(object sender, DiscordGuildCreateEventArgs e);
    public delegate void DiscordGuildDelete(object sender, DiscordGuildDeleteEventArgs e);
    public delegate void DiscordChannelUpdate(object sender, DiscordChannelUpdateEventArgs e);
    public delegate void DiscordDebugMessages(object sender, DiscordDebugMessagesEventArgs e);

    public class DiscordClient
    {
        public string token { get; set; }
        public string sessionKey { get; set; }
        public string CurrentGatewayURL { get; set; }
        private string Cookie { get; set; }
        public DiscordUserInformation ClientPrivateInformation { get; set; }
        public DiscordMember Me { get; internal set; }
        private WebSocket ws;
        private List<DiscordServer> ServersList { get; set; }
        private int? CurrentGameID = null;
        private int? IdleSinceUnixTime = null;
        static string UserAgentString = $"DiscordBot (http://github.com/Luigifan/DiscordSharp, 0.4.1.8)";

        /// <summary>
        /// A log of messages kept in a KeyValuePair.
        /// The key is the id of the message, and the value is a DiscordMessage object. If you need raw json, this is contained inside of the DiscordMessage object now.
        /// </summary>
        private List<KeyValuePair<string, DiscordMessage>> MessageLog = new List<KeyValuePair<string, DiscordMessage>>();
        private List<DiscordPrivateChannel> PrivateChannels = new List<DiscordPrivateChannel>();

        #region Event declaration
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
        public event DiscordPresenceUpdate PresenceUpdated;
        public event DiscordURLUpdate URLMessageAutoUpdate;
        public event DiscordVoiceStateUpdate VoiceStateUpdate;
        public event UnknownMessage UnknownMessageTypeReceived;
        public event DiscordMessageDeleted MessageDeleted;
        public event DiscordUserUpdate UserUpdate;
        public event DiscordGuildMemberAdded UserAddedToServer;
        public event DiscordGuildMemberRemoved UserRemovedFromServer;
        public event DiscordGuildCreate GuildCreated;
        public event DiscordGuildDelete GuildDeleted;
        public event DiscordChannelUpdate ChannelUpdated;
        public event DiscordDebugMessages DebugMessageReceived;
        #endregion
        
        public DiscordClient()
        {
            if (ClientPrivateInformation == null)
                ClientPrivateInformation = new DiscordUserInformation();
        }

        public List<DiscordServer> GetServersList() { return this.ServersList; }

        public List<KeyValuePair<string, DiscordMessage>> GetMessageLog => MessageLog;

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
                    tempSub.topic = u["topic"].ToString();
                    tempSubs.Add(tempSub);
                }
                temp.channels = tempSubs;
                foreach(var mm in j["members"])
                {
                    DiscordMember member = new DiscordMember();
                    member.user.id = mm["user"]["id"].ToString();
                    member.user.username = mm["user"]["username"].ToString();
                    member.user.avatar = mm["user"]["avatar"].ToString();
                    member.user.discriminator = mm["user"]["discriminator"].ToString();
                    temp.members.Add(member);
                }
                ServersList.Add(temp);
            }
            
        }


        public void LeaveServer(string ServerID)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/guilds/" + ServerID);
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "DELETE";
            httpRequest.UserAgent += $" {UserAgentString}";
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

        public static byte[] ImageToByte2(Image img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
        }

        //tysm voltana <3
        public void ChangeBotPicture(Bitmap image)
        {
            string base64 = Convert.ToBase64String(ImageToByte2(image));
            string type = "image/jpeg;base64";
            string req = $"data:{type},{base64}";
            string usernameRequestJson = JsonConvert.SerializeObject(new
            {
                avatar = req,
                email = ClientPrivateInformation.email,
                password = ClientPrivateInformation.password,
                username = ClientPrivateInformation.username
            });
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/users/@me");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "PATCH";
            httpRequest.UserAgent += $" {UserAgentString}";
            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(usernameRequestJson.ToString());
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = JObject.Parse(sr.ReadLine());
                    //Console.WriteLine(result.ToString());
                }
            }
            catch (Exception ex)
            {
                if (DebugMessageReceived != null)
                    DebugMessageReceived(this, new DiscordDebugMessagesEventArgs { message = ex.Message });
            }
        }

        public void ChangeChannelTopic(string Channeltopic, DiscordChannel channel)
        {
            string topicChangeJson = JsonConvert.SerializeObject(
                new
                {
                    name = channel.name,
                    topic = Channeltopic
                });

            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/channels/" + channel.id);
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "PATCH";
            httpRequest.UserAgent += $" {UserAgentString}";

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(topicChangeJson.ToString());
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = JObject.Parse(sr.ReadLine());
                    ServersList.Find(x => x.channels.Find(y => y.id == channel.id) != null).channels.Find(x => x.id == channel.id).topic = Channeltopic;
                }
            }
            catch (Exception ex)
            {
                if (DebugMessageReceived != null)
                    DebugMessageReceived(this, new DiscordDebugMessagesEventArgs { message = ex.Message });
            }
        }

        public void ChangeBotInformation(DiscordUserInformation info)
        {
            string usernameRequestJson;
            if (info.password != ClientPrivateInformation.password)
            {
                usernameRequestJson = JsonConvert.SerializeObject(new
                {
                    email = info.email,
                    new_password = info.password,
                    password = ClientPrivateInformation.password,
                    username = info.username,
                    avatar = info.avatar
                });
                ClientPrivateInformation.password = info.password;
            }
            else
            {
                usernameRequestJson = JsonConvert.SerializeObject(new
                {
                    email = info.email,
                    password = info.password,
                    username = info.username,
                    avatar = info.avatar
                });
            }

            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/users/@me");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "PATCH";
            httpRequest.UserAgent += $" {UserAgentString}";
            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(usernameRequestJson.ToString());
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = JObject.Parse(sr.ReadLine());
                    foreach (var server in ServersList)
                    {
                        foreach (var member in server.members)
                        {
                            if (member.user.id == Me.user.id)
                                member.user.username = info.username;
                        }
                    }
                    Me.user.username = info.username;
                    Me.user.email = info.email;
                    Me.user.avatar = info.avatar;
                }
            }
            catch (Exception ex)
            {
                if (DebugMessageReceived != null)
                    DebugMessageReceived(this, new DiscordDebugMessagesEventArgs { message = ex.Message });
            }
        }

        private void ChangeBotUsername(string newUsername)
        {
            string usernameRequestJson = JsonConvert.SerializeObject(new
            {
                email = ClientPrivateInformation.email,
                password = ClientPrivateInformation.password,
                username = newUsername,
                avatar = Me.user.avatar,
            });
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/users/@me");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "PATCH";
            httpRequest.UserAgent += $" {UserAgentString}";
            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(usernameRequestJson.ToString());
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = JObject.Parse(sr.ReadLine());
                    foreach(var server in ServersList)
                    {
                        foreach(var member in server.members)
                        {
                            if (member.user.id == Me.user.id)
                                member.user.username = newUsername;
                        }
                    }
                    Me.user.username = newUsername;
                }
            }
            catch(Exception ex)
            {
                if (DebugMessageReceived != null)
                    DebugMessageReceived(this, new DiscordDebugMessagesEventArgs { message = ex.Message });
            }
        }

        //Special thanks to the node-discord developer, izy521, for helping me out with this :D
        public void SendMessageToUser(string message, DiscordMember member)
        {
            string initMessage = "{\"recipient_id\":" + member.user.id + "}";
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/users/" + Me.user.id + "/channels");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";
            httpRequest.UserAgent += $" {UserAgentString}";

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
            catch(Exception ex)
            {
                //if (DebugMessageReceived != null)
                //    DebugMessageReceived(this, new DiscordDebugMessagesEventArgs { message = ex.Message });
            }
        }

        private void SendActualMessage(string id, string message)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/channels/" + id + "/messages");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";
            httpRequest.UserAgent += $" {UserAgentString}";

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
                if (m.Index > 0 && message[m.Index - 1] == '<')
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

        #region Message Received Crap..

        /// <summary>
        /// Set gameId to null if you want to remove the current game.
        /// </summary>
        /// <param name="gameId">The game's ID. These values can be found at http://hastebin.com/azijiyaboc.json/ </param>
        public void UpdateCurrentGame(int? gameId)
        {
            string msg;
            if (gameId != null)
            {
                msg = JsonConvert.SerializeObject(
                    new
                    {
                        op = 3,
                        d = new
                        {
                            idle_since = IdleSinceUnixTime == null ? (object)null : IdleSinceUnixTime,
                            game_id = gameId
                        }
                    });
                CurrentGameID = gameId;
            }
            else
            {
                msg = JsonConvert.SerializeObject(
                    new
                    {
                        op = 3,
                        d = new
                        {
                            idle_since = IdleSinceUnixTime == null ? (object)null : IdleSinceUnixTime,
                            game_id = (object)null
                        }
                    });
            }
            ws.Send(msg.ToString());
        }

        public void UpdateBotStatus(bool idle)
        {
            string msg;
            msg = JsonConvert.SerializeObject(
                new
                {
                    op = 3,
                    d = new
                    {
                        idle_since = idle ? (int)(DateTime.UtcNow - epoch).TotalMilliseconds : (object)null,
                        game_id = CurrentGameID
                    }
                });
            ws.Send(msg.ToString()); //let's try it!
        }

        private void PresenceUpdateEvents(JObject message)
        {
            DiscordPresenceUpdateEventArgs dpuea = new DiscordPresenceUpdateEventArgs();
            dpuea.RawJson = message;
            var pserver = ServersList.Find(x => x.members.Find(y => y.user.id == message["d"]["id"].ToString()) != null);
            if (pserver != null)
            {
                var user = pserver.members.Find(x => x.user.id == message["d"]["id"].ToString());
                dpuea.game_id = message["d"]["game_id"].ToObject<int?>();
                if (message["d"]["status"].ToString() == "online")
                    dpuea.status = DiscordUserStatus.ONLINE;
                else if (message["d"]["status"].ToString() == "idle")
                    dpuea.status = DiscordUserStatus.IDLE;
                else if (message["d"]["status"].ToString() == null || message["d"]["status"].ToString() == "offline")
                    dpuea.status = DiscordUserStatus.OFFLINE;
                if (PresenceUpdated != null)
                    PresenceUpdated(this, dpuea);
            }
        }


        /// <summary>
        /// Deletes a message with a specified ID.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteMessage(string id)
        {
            var message = MessageLog.Find(x => x.Value.id == id);
            SendDeleteRequest(message.Value);
        }

        /// <summary>
        /// Deletes all messages made by the bot since running.
        /// </summary>
        /// <returns>A count of messages deleted.</returns>
        public int DeleteAllMessages()
        {
            int count = 0;
            foreach (var message in this.MessageLog)
            {
                if (message.Value.author.user.id == Me.user.id)
                {
                    SendDeleteRequest(message.Value);
                    count++;
                }
            }
            return count;
        }

        public int DeleteAllMessagesInChannel(DiscordChannel channel)
        {
            int count = 0;

            foreach(var message in this.MessageLog)
            {
                if (message.Value.channel == channel)
                    if (message.Value.author.user.id == Me.user.id)
                    {
                        SendDeleteRequest(message.Value);
                        count++;
                    }
            }

            return count;
        }

        public DiscordChannel GetChannelByName(string channelName)
        {
            try
            {
                return ServersList.Find(x => x.channels.Find(y => y.name.ToLower() == channelName.ToLower()) != null).channels.Find(x => x.name.ToLower() == channelName.ToLower());
            }
            catch
            {
                return null;
            }
        }


        public void AcceptInvite(string inviteID)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/invite/" + inviteID);
            httpRequest.Headers["authorization"] = token;
            httpRequest.Method = "POST";
            httpRequest.UserAgent += $" {UserAgentString}";

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

        public DiscordMessage GetLastMessageSent()
        {
            for(int i = MessageLog.Count - 1; i > -1; i--)
            {
                if (MessageLog[i].Value.author.user.id == Me.user.id)
                    return MessageLog[i].Value;
            }
            return null;
        }
        public DiscordMessage GetLastMessageSent(DiscordChannel inChannel)
        {
            for (int i = MessageLog.Count - 1; i > -1; i--)
            {
                if (MessageLog[i].Value.author.user.id == Me.user.id)
                    if(MessageLog[i].Value.channel.id == inChannel.id)
                        return MessageLog[i].Value;
            }
            return null;
        }

        public void EditMessage(string MessageID, string replacementMessage, DiscordChannel channel)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/channels/" + channel.id + "/messages/" + MessageID);
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "PATCH";
            httpRequest.UserAgent += $" {UserAgentString}";

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Write(JsonConvert.SerializeObject(GenerateMessage(replacementMessage)));
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

        public void SimulateTyping(DiscordChannel channel)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create($"https://discordapp.com/api/channels/{channel.id}/typing");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";
            httpRequest.UserAgent += $" {UserAgentString}";

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    Console.WriteLine(result);
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

        private void SendDeleteRequest(DiscordMessage message)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/channels/" + message.channel.id + "/messages/" + message.id);
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "DELETE";
            httpRequest.UserAgent += $" {UserAgentString}";

            using (var sw = new StreamWriter(httpRequest.GetRequestStream()))
            {
                //sw.Write("DELETE");
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

        private void MessageUpdateEvents(JObject message)
        {
            DiscordServer pserver = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["channel_id"].ToString()) != null);
            DiscordChannel pchannel = pserver.channels.Find(x => x.id == message["d"]["channel_id"].ToString());
            if (pchannel != null)
            {
                if (message["d"]["author"] != null)
                {
                    KeyValuePair<string, DiscordMessage> toRemove = MessageLog.Find(x => x.Key == message["d"]["id"].ToString());
                    var jsonToEdit = toRemove.Value.RawJson;
                    jsonToEdit["d"]["content"].Replace(JToken.FromObject(message["d"]["content"].ToString()));
                    if (MessageEdited != null)
                        MessageEdited(this, new DiscordMessageEditedEventArgs
                        {
                            author = pserver.members.Find(x => x.user.id == message["d"]["author"]["id"].ToString()),
                            Channel = pchannel,
                            message = message["d"]["content"].ToString(),
                            MessageType = DiscordMessageType.CHANNEL,
                            MessageEdited = new DiscordMessage
                            {
                                author = pserver.members.Find(x => x.user.id == message["d"]["author"]["id"].ToString()),
                                content = MessageLog.Find(x => x.Key == message["d"]["id"].ToString()).Value.content,
                            }
                        });
                    int indexOfMessageToChange = MessageLog.IndexOf(toRemove);
                    MessageLog.Remove(toRemove);
                    DiscordMessage newMessage = toRemove.Value;
                    newMessage.content = jsonToEdit["d"]["content"].ToString();
                    MessageLog.Insert(indexOfMessageToChange, new KeyValuePair<string, DiscordMessage>(jsonToEdit["d"]["id"].ToString(), newMessage));

                }
                else //I know they say assume makes an ass out of you and me...but we're assuming it's Discord's weird auto edit of a just URL message
                {
                    if (URLMessageAutoUpdate != null)
                    {
                        DiscordURLUpdateEventArgs asdf = new DiscordURLUpdateEventArgs(); //I'm running out of clever names and should probably split these off into different internal voids soon...
                        asdf.id = message["d"]["id"].ToString();
                        asdf.channel = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["channel_id"].ToString()) != null).channels.Find(x => x.id == message["d"]["channel_id"].ToString());
                        foreach (var embed in message["d"]["embeds"])
                        {
                            DiscordEmbeds temp = new DiscordEmbeds();
                            temp.url = embed["url"].ToString();
                            temp.description = embed["description"].ToString();
                            try
                            {
                                temp.provider_name = embed["provider"]["name"] == null ? null : embed["provider"]["name"].ToString();
                                temp.provider_url = embed["provider"]["url"].ToString();
                            }
                            catch { }//noprovider
                            temp.title = embed["title"].ToString();
                            temp.type = embed["type"].ToString();
                            asdf.embeds.Add(temp);
                        }
                        URLMessageAutoUpdate(this, asdf);
                    }
                }
            }
        }

        private void MessageCreateEvents(JObject message)
        {
            try
            {
                string tempChannelID = message["d"]["channel_id"].ToString();

                var foundServerChannel = ServersList.Find(x => x.channels.Find(y => y.id == tempChannelID) != null);
                if (foundServerChannel == null)
                {
                    var foundPM = PrivateChannels.Find(x => x.id == message["d"]["channel_id"].ToString());
                    DiscordPrivateMessageEventArgs dpmea = new DiscordPrivateMessageEventArgs();
                    dpmea.Channel = foundPM;
                    dpmea.message = message["d"]["content"].ToString();
                    DiscordMember tempMember = new DiscordMember();
                    tempMember.user.username = message["d"]["author"]["username"].ToString();
                    tempMember.user.id = message["d"]["author"]["id"].ToString();
                    dpmea.author = tempMember;


                    if (PrivateMessageReceived != null)
                        PrivateMessageReceived(this, dpmea);
                }
                else
                {
                    DiscordMessageEventArgs dmea = new DiscordMessageEventArgs();
                    dmea.Channel = foundServerChannel.channels.Find(y => y.id == tempChannelID);

                    dmea.message_text = message["d"]["content"].ToString();

                    DiscordMember tempMember = new DiscordMember();
                    tempMember = foundServerChannel.members.Find(x => x.user.id == message["d"]["author"]["id"].ToString());
                    dmea.author = tempMember;

                    DiscordMessage m = new DiscordMessage();
                    m.author = dmea.author;
                    m.channel = dmea.Channel;
                    m.content = dmea.message_text;
                    m.id = message["d"]["id"].ToString();
                    m.RawJson = message;
                    dmea.message = m;

                    Regex r = new Regex("\\d+");
                    foreach (Match mm in r.Matches(dmea.message_text))
                        if (mm.Value == Me.user.id)
                            if (MentionReceived != null)
                                MentionReceived(this, dmea);
                    if (MessageReceived != null)
                        MessageReceived(this, dmea);
                    
                    KeyValuePair<string, DiscordMessage> toAdd = new KeyValuePair<string, DiscordMessage>(message["d"]["id"].ToString(), m);
                    MessageLog.Add(toAdd);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!! {0}", ex.Message);
                Console.Beep(100, 1000);
            }
        }

        private void ChannelCreateEvents (JObject message)
        {
            if (message["d"]["is_private"].ToString().ToLower() == "false")
            {
                var foundServer = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
                if (foundServer != null)
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
                DiscordPrivateChannelEventArgs fak = new DiscordPrivateChannelEventArgs { ChannelType = DiscordChannelCreateType.PRIVATE, ChannelCreated = tempPrivate };
                if (PrivateChannelCreated != null)
                    PrivateChannelCreated(this, fak);
            }
        }
        #endregion
        private String GetGatewayUrl()
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/gateway");
            httpRequest.Headers["authorization"] = token;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "GET";
            httpRequest.UserAgent += $" {UserAgentString}";
            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    JObject result = JObject.Parse(sr.ReadToEnd());
                    return result["url"].ToString();
                }
            }
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    string result = s.ReadToEnd();
                    Console.WriteLine("!!! " + result);
                    return null;
                }
            }
        }

        public void ConnectAndReadMessages()
        {
            CurrentGatewayURL = GetGatewayUrl();
            ws = new WebSocket(CurrentGatewayURL);
            ws.EnableRedirection = true;
            ws.Log.File = "websocketlog.txt";
                ws.OnMessage += (sender, e) =>
                {
                    var message = JObject.Parse(e.Data);
                    switch(message["t"].ToString())
                    {
                        case ("READY"):
                            using (var sw = new StreamWriter("READY_LATEST.txt"))
                                sw.Write(message);
                            Me = new DiscordMember
                            {
                                user = new DiscordUser
                                {
                                    username = message["d"]["user"]["username"].ToString(),
                                    id = message["d"]["user"]["id"].ToString(),
                                    verified = message["d"]["user"]["verified"].ToObject<bool>(),
                                    avatar = message["d"]["user"]["avatar"].ToString(),
                                    discriminator = message["d"]["user"]["discriminator"].ToString(),
                                    email = message["d"]["user"]["email"].ToString()
                                }
                            };
                            ClientPrivateInformation.avatar = Me.user.avatar;
                            ClientPrivateInformation.username = Me.user.username;
                            HeartbeatInterval = int.Parse(message["d"]["heartbeat_interval"].ToString());
                            GetChannelsList(message);
                            if (Connected != null)
                                Connected(this, new DiscordConnectEventArgs { user = Me }); //Since I already know someone will ask for it.
                            break;
                        case ("GUILD_MEMBER_REMOVE"):
                            GuildMemberRemoveEvents(message);
                            break;
                        case ("GUILD_MEMBER_ADD"):
                            GuildMemberAddEvents(message);
                            break;
                        case ("GUILD_DELETE"):
                            GuildDeleteEvents(message);
                            break;
                        case ("GUILD_CREATE"):
                            GuildCreateEvents(message);
                            break;
                        case ("PRESENCE_UPDATE"):
                            PresenceUpdateEvents(message);
                            break;
                        case ("MESSAGE_UPDATE"):
                            MessageUpdateEvents(message);
                            break;
                        case ("TYPING_START"):
                            DiscordServer server = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["channel_id"].ToString()) != null);
                            if (server != null)
                            {
                                DiscordChannel channel = server.channels.Find(x => x.id == message["d"]["channel_id"].ToString());
                                DiscordMember uuser = server.members.Find(x => x.user.id == message["d"]["user_id"].ToString());
                                if (UserTypingStart != null)
                                    UserTypingStart(this, new DiscordTypingStartEventArgs { user = uuser, channel = channel, timestamp = int.Parse(message["d"]["timestamp"].ToString()) });
                            }
                            break;
                        case ("MESSAGE_CREATE"):
                            MessageCreateEvents(message);
                            break;
                        case ("CHANNEL_CREATE"):
                            ChannelCreateEvents(message);
                            break;
                        case ("VOICE_STATE_UPDATE"):
                            VoiceStateUpdateEvents(message);
                            break;
                        case ("MESSAGE_DELETE"):
                            MessageDeletedEvents(message);
                            break;
                        case ("USER_UPDATE"):
                            UserUpdateEvents(message);
                            break;
                        case ("CHANNEL_UPDATE"):
                            ChannelUpdateEvents(message);
                            break;
                        default:
                            if (UnknownMessageTypeReceived != null)
                                UnknownMessageTypeReceived(this, new UnknownMessageEventArgs { RawJson = message });
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

        private void ChannelUpdateEvents(JObject message)
        {
            DiscordChannelUpdateEventArgs e = new DiscordChannelUpdateEventArgs();
            e.RawJson = message;
            DiscordChannel oldChannel = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["id"].ToString()) != null).channels.Find(x=>x.id == message["d"]["id"].ToString());
            e.OldChannel = oldChannel;
            DiscordChannel newChannel = oldChannel;
            newChannel.name = message["d"]["name"].ToString();
            newChannel.topic = message["d"]["topic"].ToString();
            newChannel.is_private = message["d"]["is_private"].ToObject<bool>();
            e.NewChannel = newChannel;

            DiscordServer serverToRemoveFrom = ServersList.Find(x => x.channels.Find(y => y.id == newChannel.id) != null);
            int indexOfServer = ServersList.IndexOf(serverToRemoveFrom);
            serverToRemoveFrom.channels.Remove(oldChannel);
            serverToRemoveFrom.channels.Add(newChannel);

            ServersList.RemoveAt(indexOfServer);
            ServersList.Insert(indexOfServer, serverToRemoveFrom);

            if (ChannelUpdated != null)
                ChannelUpdated(this, e);
        }

        private void GuildDeleteEvents(JObject message)
        {
            DiscordGuildDeleteEventArgs e = new DiscordGuildDeleteEventArgs();
            e.server = ServersList.Find(x => x.id == message["d"]["id"].ToString());
            e.RawJson = message;
            ServersList.Remove(e.server);
            if (GuildDeleted != null)
                GuildDeleted(this, e);
        }

        private void GuildCreateEvents(JObject message)
        {
            DiscordGuildCreateEventArgs e = new DiscordGuildCreateEventArgs();
            e.RawJson = message;
            DiscordServer server = new DiscordServer();
            server.id = message["d"]["id"].ToString();
            server.name = message["d"]["name"].ToString();
            server.owner_id = message["d"]["owner_id"].ToString();
            server.members = new List<DiscordMember>();
            server.channels = new List<DiscordChannel>();
            foreach(var chn in message["d"]["channels"])
            {
                DiscordChannel tempChannel = new DiscordChannel();
                tempChannel.id = chn["id"].ToString();
                tempChannel.type = chn["type"].ToString();
                tempChannel.topic = chn["topic"].ToString();
                tempChannel.name = chn["name"].ToString();
                tempChannel.is_private = false;
                server.channels.Add(tempChannel);
            }
            foreach(var mbr in message["d"]["members"])
            {
                DiscordMember member = new DiscordMember();
                member.user.avatar = mbr["user"]["avatar"].ToString();
                member.user.username = mbr["user"]["username"].ToString();
                member.user.id = mbr["user"]["id"].ToString();
                member.user.discriminator = mbr["user"]["discriminator"].ToString();
                server.members.Add(member);
            }
            ServersList.Add(server);
            e.server = server;
            if (GuildCreated != null)
                GuildCreated(this, e);
        }

        private void GuildMemberAddEvents(JObject message)
        {
            DiscordGuildMemberAddEventArgs e = new DiscordGuildMemberAddEventArgs();
            e.RawJson = message;
            DiscordMember newMember = new DiscordMember
            {
                user = new DiscordUser
                {
                    username = message["d"]["user"]["username"].ToString(),
                    id = message["d"]["user"]["id"].ToString(),
                    discriminator = message["d"]["user"]["discriminator"].ToString(),
                    avatar = message["d"]["user"]["avatar"].ToString(),
                }
            };
            e.AddedMember = newMember;
            e.Guild = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            e.roles = message["d"]["roles"].ToObject<string[]>();
            e.JoinedAt = DateTime.Parse(message["d"]["joined_at"].ToString());

            ServersList.Find(x => x == e.Guild).members.Add(newMember);
            if (UserAddedToServer != null)
                UserAddedToServer(this, e);
        }
        private void GuildMemberRemoveEvents(JObject message)
        {
            DiscordGuildMemberRemovedEventArgs e = new DiscordGuildMemberRemovedEventArgs();
            DiscordMember removed = new DiscordMember();

            List<DiscordMember> membersToRemove = new List<DiscordMember>();
            foreach(var server in ServersList)
            {
                if (server.id != message["d"]["guild_id"].ToString())
                    continue;
                for(int i = 0; i < server.members.Count; i++)
                {
                    if(server.members[i].user.id == message["d"]["user"]["id"].ToString())
                    {
                        removed = server.members[i];
                        membersToRemove.Add(removed);
                    }
                }
            }
            
            foreach(var member in membersToRemove)
            {
                foreach (var server in ServersList)
                {
                    try
                    {
                        server.members.Remove(member);
                    }
                    catch { } //oh, you mean useless?
                }
            }
            e.MemberRemoved = removed;
            e.Server = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            e.RawJson = message;

            if (UserRemovedFromServer != null)
                UserRemovedFromServer(this, e);
        }

        private void UserUpdateEvents(JObject message)
        {
            DiscordUserUpdateEventArgs e = new DiscordUserUpdateEventArgs();
            e.RawJson = message;
            DiscordMember newMember = new DiscordMember
            {
                user = new DiscordUser
                {
                    username = message["d"]["username"].ToString(),
                    id = message["d"]["id"].ToString(),
                    verified = message["d"]["verified"].ToObject<bool>(),
                    email = message["d"]["email"].ToString(),
                    avatar = message["d"]["avatar"].ToString(),
                    discriminator = message["d"]["discriminator"].ToString()
                }
            };

            DiscordMember oldMember = new DiscordMember();
            //Update members
            foreach(var server in ServersList)
            {
                for(int i = 0; i < server.members.Count; i++)
                {
                    if(server.members[i].user.id == newMember.user.id)
                    {
                        server.members[i] = newMember;
                        
                        oldMember = server.members[i];
                        
                    }
                }
            }
            e.NewMember = newMember;
            e.OriginalMember = oldMember;
            if (UserUpdate != null)
                UserUpdate(this, e);
        }

        private void MessageDeletedEvents(JObject message)
        {
            DiscordMessageDeletedEventArgs e = new DiscordMessageDeletedEventArgs();
            e.DeletedMessage = MessageLog.Find(x => x.Key == message["d"]["id"].ToString()).Value;
            e.Channel = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["channel_id"].ToString()) != null).channels.Find(x => x.id == message["d"]["channel_id"].ToString());
            e.RawJson = message;

            if (MessageDeleted != null)
                MessageDeleted(this, e);
        }

        private void VoiceStateUpdateEvents(JObject message)
        {
            var f = message["d"]["channel_id"];
            if (f.Value<String>() == null)
            {
                DiscordLeftVoiceChannelEventArgs le = new DiscordLeftVoiceChannelEventArgs();
                le.user = ServersList.Find(x => x.members.Find(y => y.user.id == message["d"]["user_id"].ToString()) != null).members.Find(x => x.user.id == message["d"]["user_id"].ToString());
                le.guild = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
                le.RawJson = message;
                return;
            }
            DiscordVoiceStateUpdateEventArgs e = new DiscordVoiceStateUpdateEventArgs();
            e.user = ServersList.Find(x => x.members.Find(y => y.user.id == message["d"]["user_id"].ToString()) != null).members.Find(x => x.user.id == message["d"]["user_id"].ToString());
            e.guild = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            e.channel = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["channel_id"].ToString()) != null).channels.Find(x => x.id == message["d"]["channel_id"].ToString());
            e.self_deaf = message["d"]["self_deaf"].ToObject<bool>();
            e.deaf = message["d"]["deaf"].ToObject<bool>();
            e.self_mute = message["d"]["self_mute"].ToObject<bool>();
            e.mute = message["d"]["mute"].ToObject<bool>();
            e.suppress = message["d"]["suppress"].ToObject<bool>();
            e.RawJson = message;
            if (VoiceStateUpdate != null)
                VoiceStateUpdate(this, e);
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
            try
            {
                ws.Close();
                ws = null;
                ServersList = null;
                PrivateChannels = null;
                Me = null;
                this.token = null;
                this.sessionKey = null;
                this.ClientPrivateInformation = null;
            }
            catch { /*already been disposed elsewhere */ }
        }

        public void Logout()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/auth/logout");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string msg = JsonConvert.SerializeObject(new
                {
                    token = this.token
                });
                sw.Write(msg);
                sw.Flush();
                sw.Close();
            }

            try
            {
                var httpResponseT = httpWebRequest.GetResponse();
                var httpResponse = (HttpWebResponse)httpResponseT;
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    Console.WriteLine(result);
                }
            }
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    string result = s.ReadToEnd();
                    Console.WriteLine("Result from exception: " + e.Message);
                }
            }
            Dispose();
        }

        public async Task<string> SendLoginRequestAsync()
        {
            if (ClientPrivateInformation == null || ClientPrivateInformation.email == null || ClientPrivateInformation.password == null)
                throw new ArgumentNullException("You didn't supply login information!");
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/auth/login");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 20000;

            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string msg = JsonConvert.SerializeObject(new
                {
                    email = ClientPrivateInformation.email,
                    password = ClientPrivateInformation.password
                });
                await sw.WriteAsync(msg);
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponseT = await httpWebRequest.GetResponseAsync();
                var httpResponse = (HttpWebResponse)httpResponseT;
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = await sr.ReadToEndAsync();
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
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    string result = await s.ReadToEndAsync();
                    DiscordLoginResult jresult = JsonConvert.DeserializeObject<DiscordLoginResult>(result);
                    if (jresult.password != null)
                        throw new DiscordLoginException(jresult.password[0]);
                    if (jresult.email != null)
                        throw new DiscordLoginException(jresult.email[0]);
                }
            }

            return "";
        }

        /// <summary>
        /// Sends a login request.
        /// </summary>
        /// <returns>The token if login was succesful, or null if not</returns>
        public string SendLoginRequest()
        {
            if (ClientPrivateInformation == null || ClientPrivateInformation.email == null || ClientPrivateInformation.password == null)
                throw new ArgumentNullException("You didn't supply login information!");

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/auth/login");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 30000;

            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string msg = JsonConvert.SerializeObject(new
                {
                    email = ClientPrivateInformation.email,
                    password = ClientPrivateInformation.password
                });
                sw.Write(msg);
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
