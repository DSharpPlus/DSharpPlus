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
using System.Drawing;

namespace DiscordSharp
{
    class DiscordProperties
    {
        [JsonProperty("os")]
        public string OS { get; set; }

        [JsonProperty("browser")]
        public string Browser { get; set; }

        [JsonProperty("device")]
        public string Device
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
            OS = Environment.OSVersion.ToString();
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

    public class DiscordClient
    {
        public static string token { get; internal set; }

        [Obsolete]
        public string sessionKey { get; set; }
        public string CurrentGatewayURL { get; internal set; }
        [Obsolete]
        private string Cookie { get; set; }
        public DiscordUserInformation ClientPrivateInformation { get; set; }
        public DiscordMember Me { get; internal set; }
        private WebSocket ws;
        private List<DiscordServer> ServersList { get; set; }
        private string CurrentGameName = "";
        private int? IdleSinceUnixTime = null;
        static string UserAgentString = $"DiscordBot (http://github.com/Luigifan/DiscordSharp, {typeof(DiscordClient).Assembly.GetName().Version.ToString()})";
        private DiscordVoiceClient VoiceClient;
        private Logger DebugLogger = new Logger();
        public Logger GetTextClientLogger => DebugLogger;
        public Logger GetLastVoiceClientLogger;

        private CancellationTokenSource KeepAliveTaskTokenSource = new CancellationTokenSource();
        private CancellationToken KeepAliveTaskToken;
        private Task KeepAliveTask;

        /// <summary>
        /// Testing.
        /// </summary>
        private List<DiscordMember> RemovedMembers = new List<DiscordMember>();

        /// <summary>
        /// Whether or not to write the latest READY upon receiving it.
        /// If this is true, the client will write the contents of the READY message to 'READY_LATEST.txt'
        /// If your client is connected to a lot of servers, this file will be quite large.
        /// </summary>
        public bool WriteLatestReady { get; set; } = false;

        /// <summary>
        /// A log of messages kept in a KeyValuePair.
        /// The key is the id of the message, and the value is a DiscordMessage object. If you need raw json, this is contained inside of the DiscordMessage object now.
        /// </summary>
        private List<KeyValuePair<string, DiscordMessage>> MessageLog = new List<KeyValuePair<string, DiscordMessage>>();
        private List<DiscordPrivateChannel> PrivateChannels = new List<DiscordPrivateChannel>();

        #region Event declaration
        public event EventHandler<DiscordMessageEventArgs> MessageReceived;
        public event EventHandler<DiscordConnectEventArgs> Connected;
        public event EventHandler<DiscordSocketOpenedEventArgs> SocketOpened;
        public event EventHandler<DiscordSocketClosedEventArgs> SocketClosed;
        public event EventHandler<DiscordChannelCreateEventArgs> ChannelCreated;
        public event EventHandler<DiscordPrivateChannelEventArgs> PrivateChannelCreated;
        public event EventHandler<DiscordPrivateMessageEventArgs> PrivateMessageReceived;
        public event EventHandler<DiscordKeepAliveSentEventArgs> KeepAliveSent;
        public event EventHandler<DiscordMessageEventArgs> MentionReceived;
        public event EventHandler<DiscordTypingStartEventArgs> UserTypingStart;
        public event EventHandler<DiscordMessageEditedEventArgs> MessageEdited;
        public event EventHandler<DiscordPresenceUpdateEventArgs> PresenceUpdated;
        public event EventHandler<DiscordURLUpdateEventArgs> URLMessageAutoUpdate;
        public event EventHandler<DiscordVoiceStateUpdateEventArgs> VoiceStateUpdate;
        public event EventHandler<UnknownMessageEventArgs> UnknownMessageTypeReceived;
        public event EventHandler<DiscordMessageDeletedEventArgs> MessageDeleted;
        public event EventHandler<DiscordUserUpdateEventArgs> UserUpdate;
        public event EventHandler<DiscordGuildMemberAddEventArgs> UserAddedToServer;
        public event EventHandler<DiscordGuildMemberRemovedEventArgs> UserRemovedFromServer;
        public event EventHandler<DiscordGuildCreateEventArgs> GuildCreated;
        public event EventHandler<DiscordGuildDeleteEventArgs> GuildDeleted;
        public event EventHandler<DiscordChannelUpdateEventArgs> ChannelUpdated;
        public event EventHandler<LoggerMessageReceivedArgs> TextClientDebugMessageReceived;
        public event EventHandler<LoggerMessageReceivedArgs> VoiceClientDebugMessageReceived;
        public event EventHandler<DiscordChannelDeleteEventArgs> ChannelDeleted;
        public event EventHandler<DiscordServerUpdateEventArgs> GuildUpdated;
        public event EventHandler<DiscordGuildRoleDeleteEventArgs> RoleDeleted;
        public event EventHandler<DiscordGuildRoleUpdateEventArgs> RoleUpdated;
        public event EventHandler<DiscordGuildMemberUpdateEventArgs> GuildMemberUpdated;
        public event EventHandler<DiscordGuildBanEventArgs> GuildMemberBanned;
        public event EventHandler<DiscordPrivateChannelDeleteEventArgs> PrivateChannelDeleted;
        public event EventHandler<DiscordBanRemovedEventArgs> BanRemoved;
        public event EventHandler<DiscordPrivateMessageDeletedEventArgs> PrivateMessageDeleted;

        #region Voice
        /// <summary>
        /// For use when connected to voice only.
        /// </summary>
        public event EventHandler<DiscordAudioPacketEventArgs> AudioPacketReceived;
        /// <summary>
        /// For use when connected to voice only.
        /// </summary>
        public event EventHandler<DiscordVoiceUserSpeakingEventArgs> UserSpeaking;
        /// <summary>
        /// For use when connected to voice only.
        /// </summary>
        public event EventHandler<DiscordLeftVoiceChannelEventArgs> UserLeftVoiceChannel;
        #endregion
        #endregion

        public DiscordClient()
        {
            if (ClientPrivateInformation == null)
                ClientPrivateInformation = new DiscordUserInformation();

            DebugLogger.LogMessageReceived += (sender, e) =>
            {
                if (e.message.Level == MessageLevel.Error)
                    DisconnectFromVoice();
                if (TextClientDebugMessageReceived != null)
                    TextClientDebugMessageReceived(this, e);
            };
        }

        public List<DiscordServer> GetServersList() => ServersList;
        public List<KeyValuePair<string, DiscordMessage>> GetMessageLog() => MessageLog;
        public List<DiscordPrivateChannel> GetPrivateChannels() => PrivateChannels;
        public bool ConnectedToVoice() => VoiceClient != null ? VoiceClient.Connected : false;

        //eh
        private void GetChannelsList(JObject m)
        {
            if (ServersList == null)
                ServersList = new List<DiscordServer>();
            foreach(var j in m["d"]["guilds"])
            {
                DiscordServer temp = new DiscordServer();
                temp.parentclient = this;
                temp.id = j["id"].ToString();
                temp.name = j["name"].ToString();
                //temp.owner_id = j["owner_id"].ToString();
                List<DiscordChannel> tempSubs = new List<DiscordChannel>();

                List<DiscordRole> tempRoles = new List<DiscordRole>();
                foreach(var u in j["roles"])
                {
                    DiscordRole t = new DiscordRole
                    {
                        color = new DiscordSharp.Color(u["color"].ToObject<int>().ToString("x")),
                        name = u["name"].ToString(),
                        permissions = new DiscordPermission(u["permissions"].ToObject<uint>()),
                        position = u["position"].ToObject<int>(),
                        managed = u["managed"].ToObject<bool>(),
                        id = u["id"].ToString(),
                        hoist = u["hoist"].ToObject<bool>()
                    };
                    tempRoles.Add(t);
                }
                temp.roles = tempRoles;
                foreach(var u in j["channels"])
                {
                    DiscordChannel tempSub = new DiscordChannel();
                    tempSub.id = u["id"].ToString();
                    tempSub.name = u["name"].ToString();
                    tempSub.type = u["type"].ToString();
                    tempSub.topic = u["topic"].ToString();
                    tempSub.parent = temp;
                    List<DiscordPermissionOverride> permissionoverrides = new List<DiscordPermissionOverride>();
                    foreach(var o in u["permission_overwrites"])
                    {
                        DiscordPermissionOverride dpo = new DiscordPermissionOverride(o["allow"].ToObject<uint>(), o["deny"].ToObject<uint>());
                        dpo.id = o["id"].ToString();

                        if (o["type"].ToString() == "member")
                            dpo.type = DiscordPermissionOverride.OverrideType.member;
                        else
                            dpo.type = DiscordPermissionOverride.OverrideType.role;

                        permissionoverrides.Add(dpo);
                    }
                    tempSub.PermissionOverrides = permissionoverrides;

                    tempSubs.Add(tempSub);
                }
                temp.channels = tempSubs;
                foreach(var mm in j["members"])
                {
                    DiscordMember member = JsonConvert.DeserializeObject<DiscordMember>(mm["user"].ToString());
                    member.parentclient = this;
                    //member.ID = mm["user"]["id"].ToString();
                    //member.Username = mm["user"]["username"].ToString();
                    //member.Avatar = mm["user"]["avatar"].ToString();
                    //member.Discriminator = mm["user"]["discriminator"].ToString();
                    member.Roles = new List<DiscordRole>();
                    JArray rawRoles = JArray.Parse(mm["roles"].ToString());
                    if(rawRoles.Count > 0)
                    {
                        foreach(var role in rawRoles.Children())
                        {
                            member.Roles.Add(temp.roles.Find(x => x.id == role.Value<string>()));
                        }
                    }
                    else
                    {
                        member.Roles.Add(temp.roles.Find(x => x.name == "@everyone"));
                    }
                    member.Parent = temp;

                    temp.members.Add(member);
                }
                temp.owner = temp.members.Find(x => x.ID == j["owner_id"].ToString());
                ServersList.Add(temp);
            }
            if (PrivateChannels == null)
                PrivateChannels = new List<DiscordPrivateChannel>();
            foreach(var privateChannel in m["d"]["private_channels"])
            {
                DiscordPrivateChannel tempPrivate = JsonConvert.DeserializeObject<DiscordPrivateChannel>(privateChannel.ToString());
                DiscordServer potentialServer = new DiscordServer();
                ServersList.ForEach(x =>
                {
                    x.members.ForEach(y =>
                    {
                        if (y.ID == privateChannel["recipient"]["id"].ToString())
                            potentialServer = x;
                    });
                });
                if(potentialServer.owner != null) //should be a safe test..i hope
                {
                    DiscordMember recipient = potentialServer.members.Find(x => x.ID == privateChannel["recipient"]["id"].ToString());
                    if (recipient != null)
                    {
                        tempPrivate.recipient = recipient;
                        PrivateChannels.Add(tempPrivate);
                    }
                    else
                    {
                        DebugLogger.Log("Recipient was null!!!!", MessageLevel.Critical);
                    }
                }
                else
                {
                    DebugLogger.Log("No potential server found for user's private channel null!!!!", MessageLevel.Critical);
                }
            }
        }


        public void LeaveServer(string ServerID)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{ServerID}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while leaving server ({ServerID}): {ex.Message}", MessageLevel.Error);
            }
        }
        /// <summary>
        /// Sends a message to a channel, what else did you expect?
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        public DiscordMessage SendMessageToChannel(string message, DiscordChannel channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.id}" + Endpoints.Messages;
            try
            {
                JObject result = JObject.Parse(WebWrapper.Post(url, token, JsonConvert.SerializeObject(Utils.GenerateMessage(message))));
                DiscordMessage m = new DiscordMessage
                {
                    id = result["id"].ToString(),
                    attachments = result["attachments"].ToObject<string[]>(),
                    author = channel.parent.members.Find(x => x.ID == result["author"]["id"].ToString()),
                    channel = channel,
                    TypeOfChannelObject = channel.GetType(),
                    content = result["content"].ToString(),
                    RawJson = result,
                    timestamp = result["timestamp"].ToObject<DateTime>()
                };
                return m;
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while sending message to channel ({channel.name}): {ex.Message}", MessageLevel.Error);
            }
            return null;
        }

        public void AttachFile(DiscordChannel channel, string message, string pathToFile)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.id}" + Endpoints.Messages;
            //WebWrapper.PostWithAttachment(url, message, pathToFile);
            try
            {
                var uploadResult = JObject.Parse(WebWrapper.HttpUploadFile(url, token, pathToFile, "file", "image/jpeg", null));

                if (!string.IsNullOrWhiteSpace(message))
                    EditMessage(uploadResult["id"].ToString(), message, channel);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while sending file ({pathToFile}) to {channel.name}: {ex.Message}", MessageLevel.Error);
            }
        }

        //tysm voltana <3
        public void ChangeClientAvatar(Bitmap image)
        {
            string base64 = Convert.ToBase64String(Utils.ImageToByteArray(image));
            string type = "image/jpeg;base64";
            string req = $"data:{type},{base64}";
            string usernameRequestJson = JsonConvert.SerializeObject(new
            {
                avatar = req,
                email = ClientPrivateInformation.email,
                password = ClientPrivateInformation.password,
                username = ClientPrivateInformation.username
            });
            string url = Endpoints.BaseAPI + Endpoints.Users + "/@me";
            try
            {
                WebWrapper.Patch(url, token, usernameRequestJson);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while changing client's avatar: {ex.Message}", MessageLevel.Error);
            }
        }

        public void ChangeGuildIcon(Bitmap image, DiscordServer guild)
        {
            Bitmap resized = new Bitmap((Image)image, 200, 200);

            string base64 = Convert.ToBase64String(Utils.ImageToByteArray(resized));
            string type = "image/jpeg;base64";
            string req = $"data:{type},{base64}";
            string guildjson = JsonConvert.SerializeObject(new { icon = req, name = guild.name});
            string url = Endpoints.BaseAPI + Endpoints.Guilds + "/" + guild.id;
            try
            {
                var result = JObject.Parse(WebWrapper.Patch(url, token, guildjson));
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while changing guild {guild.name}'s icon: {ex.Message}", MessageLevel.Error);
            }
        }

        /// <summary>
        /// Returns a List of DiscordMessages. 
        /// </summary>
        /// <param name="channel">The channel to return them from.</param>
        /// <param name="count">How many to return</param>
        /// <param name="idBefore">Messages before this message ID.</param>
        /// <param name="idAfter">Messages after this message ID.</param>
        /// <returns></returns>
        public List<DiscordMessage> GetMessageHistory(DiscordChannel channel, int count, int? idBefore, int ?idAfter)
        {
            string request = "https://discordapp.com/api/channels/" + channel.id + $"/messages?&limit={count}";
            if (idBefore != null)
                request += $"&before={idBefore}";
            if (idAfter != null)
                request += $"&after={idAfter}";

            JArray result = null;

            try
            {
                result = JArray.Parse(WebWrapper.Get(request, token));
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while getting message history for channel {channel.name}: {ex.Message}", MessageLevel.Error);
            }

            if(result != null)
            {
                List<DiscordMessage> messageList = new List<DiscordMessage>();
                /// NOTE
                /// For some reason, the d object is excluded from this.
                foreach (var item in result.Children())
                {
                    messageList.Add(new DiscordMessage
                    {
                        id = item["id"].ToString(),
                        channel = channel,
                        TypeOfChannelObject = channel.GetType(),
                        author = GetMemberFromChannel(channel, item["author"]["id"].ToString()),
                        content = item["content"].ToString(),
                        RawJson = item.ToObject<JObject>(),
                        timestamp = DateTime.Parse(item["timestamp"].ToString())
                    });
                }
                return messageList;
            }

            return null;
        }

        public void ChangeChannelTopic(string Channeltopic, DiscordChannel channel)
        {
            string topicChangeJson = JsonConvert.SerializeObject(
                new
                {
                    name = channel.name,
                    topic = Channeltopic
                });
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.id}";
            try
            {
                var result = JObject.Parse(WebWrapper.Patch(url, token, topicChangeJson));
                ServersList.Find(x => x.channels.Find(y => y.id == channel.id) != null).channels.Find(x => x.id == channel.id).topic = Channeltopic;
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while changing channel topic for channel {channel.name}: {ex.Message}", MessageLevel.Error);
            }
        }

        public List<DiscordRole> GetRoles(DiscordServer server)
        {
            return null;
        }

        public void ChangeClientInformation(DiscordUserInformation info)
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
                File.Delete("token_cache");
                DebugLogger.Log("Deleted token_cache due to change of password.");
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

            string url = Endpoints.BaseAPI + Endpoints.Users + "/@me";
            try
            {
                var result = JObject.Parse(WebWrapper.Patch(url, token, usernameRequestJson));
                foreach (var server in ServersList)
                {
                    foreach (var member in server.members)
                    {
                        if (member.ID == Me.ID)
                            member.Username = info.username;
                    }
                }
                Me.Username = info.username;
                Me.Email = info.email;
                Me.Avatar = info.avatar;
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while changing client's information: {ex.Message}", MessageLevel.Error);
            }
        }

        private void ChangeClientUsername(string newUsername)
        {
            string url = Endpoints.BaseAPI + Endpoints.Users + "/@me";
            string usernameRequestJson = JsonConvert.SerializeObject(new
            {
                email = ClientPrivateInformation.email,
                password = ClientPrivateInformation.password,
                username = newUsername,
                avatar = Me.Avatar,
            });
            try
            {
                var result = JObject.Parse(WebWrapper.Patch(url, token, usernameRequestJson));
                if (result != null)
                {
                    foreach (var server in ServersList)
                    {
                        foreach (var member in server.members)
                        {
                            if (member.ID == Me.ID)
                                member.Username = newUsername;
                        }
                    }
                    Me.Username = newUsername;
                }
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while changing client's username: {ex.Message}", MessageLevel.Error);
            }
        }

        //Special thanks to the node-discord developer, izy521, for helping me out with this :D
        public DiscordMessage SendMessageToUser(string message, DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Users + $"/{Me.ID}" + Endpoints.Channels;
            string initMessage = "{\"recipient_id\":" + member.ID + "}";

            try
            {
                var result = JObject.Parse(WebWrapper.Post(url, token, initMessage));
                if (result != null)
                {
                    DiscordMember recipient = ServersList.Find(
                        x => x.members.Find(
                            y => y.ID == result["recipient"]["id"].ToString()) != null).members.Find(
                        x => x.ID == result["recipient"]["id"].ToString());

                    return SendActualMessage(result["id"].ToString(), message, recipient);
                }
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while sending message to user, step 1: {ex.Message}", MessageLevel.Error);
            }

            return null;
        }

        private DiscordMessage SendActualMessage(string id, string message, DiscordMember recipient)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{id}" + Endpoints.Messages;
            DiscordMessage toSend = Utils.GenerateMessage(message);

            try
            {
                var result = JObject.Parse(WebWrapper.Post(url, token, JsonConvert.SerializeObject(toSend).ToString()));
                DiscordMessage d = JsonConvert.DeserializeObject<DiscordMessage>(result.ToString());
                d.Recipient = recipient;
                d.channel = PrivateChannels.Find(x => x.id == result["channel_id"].ToString());
                d.TypeOfChannelObject = typeof(DiscordPrivateChannel);
                d.author = Me;
                return d;
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while sending message to user, step 2: {ex.Message}", MessageLevel.Error);
            }
            return null;
        }

        public string GetCurrentGame => CurrentGameName;

        public bool WebsocketAlive => (ws != null) ? ws.IsAlive : false;

        #region Message Received Crap..

        /// <summary>
        /// Set gameId to null if you want to remove the current game.
        /// </summary>
        /// <param name="gameName">The game's name. Old gameid lookup can be seen at: http://hastebin.com/azijiyaboc.json/ </param>
        public void UpdateCurrentGame(string gameName)
        {
            string msg;
            if (gameName.ToLower().Trim() != "")
            {
                msg = JsonConvert.SerializeObject(
                    new
                    {
                        op = 3,
                        d = new
                        {
                            idle_since = IdleSinceUnixTime == null ? (object)null : IdleSinceUnixTime,
                            game = new { name = gameName }
                        }
                    });
                CurrentGameName = gameName;
                DebugLogger.Log($"Updating client's current game as '{gameName}'");
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
                            game = (object)null
                        }
                    });
                DebugLogger.Log("Setting current game to null.");
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
                        game = CurrentGameName.ToLower().Trim() == "" ? (object)null : new { name = CurrentGameName }
                    }
                });
            ws.Send(msg.ToString()); //let's try it!
        }

        private void PresenceUpdateEvents(JObject message)
        {
            DiscordPresenceUpdateEventArgs dpuea = new DiscordPresenceUpdateEventArgs();
            dpuea.RawJson = message;
            var pserver = ServersList.Find(x => x.members.Find(y => y.ID == message["d"]["id"].ToString()) != null);
            if (pserver != null)
            {
                var user = pserver.members.Find(x => x.ID == message["d"]["id"].ToString());
                dpuea.user = user;

                string game = message["d"]["game"].ToString();
                if (message["d"]["game"].IsNullOrEmpty())
                    dpuea.game = "";
                else
                    dpuea.game = message["d"]["game"]["name"].ToString();

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

        public void DeletePrivateMessage(DiscordMessage message)
        {
            SendDeleteRequest(message, true);
        }

        /// <summary>
        /// Deletes all messages made by the bot since running.
        /// </summary>
        /// <returns>A count of messages deleted.</returns>
        public int DeleteAllMessages()
        {
            int count = 0;
            MessageLog.ForEach(x =>
            {
                if (x.Value.author.ID == Me.ID)
                {
                    SendDeleteRequest(x.Value);
                    count++;
                }
            });
            
            return count;
        }
        
        /// <summary>
        /// Deletes messages from the client's internal logs in a given channel.
        /// Only deletes those sent by the client.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>How many messages were deleted.</returns>
        public int DeleteMessagesFromClientInChannel(DiscordChannel channel)
        {
            int count = 0;

            foreach(var message in this.MessageLog)
            {
                if (message.Value.channel == channel)
                    if (message.Value.author.ID == Me.ID)
                    {
                        SendDeleteRequest(message.Value);
                        count++;
                    }
            }

            return count;
        }

        /// <summary>
        /// Deletes the specified number of messages in a given channel.
        /// Thank you to Siegen for this idea/method!
        /// </summary>
        /// <param name="channel">The channel to delete messages in.</param>
        /// <param name="count">The amount of messages to delete (max 100)</param>
        /// <returns>The count of messages deleted.</returns>
        public int DeleteMultipleMessagesInChannel(DiscordChannel channel, int count)
        {
            if (count > 100)
                count = 100;

            int __count = 0;

            var messages = GetMessageHistory(channel, count, null, null);

            messages.ForEach(x => 
            {
                if (x.channel.id == channel.id)
                {
                    SendDeleteRequest(x);
                    __count++;
                }
            });

            return __count;
        }
        
        public DiscordMember GetMemberFromChannel(DiscordChannel channel, string username, bool caseSensitive)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Argument given for username was null/empty.");
            if(channel != null)
            {
                DiscordMember foundMember = channel.parent.members.Find(x => caseSensitive ? x.Username == username : x.Username.ToLower() == username.ToLower());
                if(foundMember != null)
                {
                    return foundMember;
                }
                else
                {
                    DebugLogger.Log("Error in GetMemberFromChannel: foundMember was null!", MessageLevel.Error);
                }
            }
            else
            {
                DebugLogger.Log("Error in GetMemberFromChannel: channel was null!", MessageLevel.Error);
            }
            return null;
        }

        public DiscordMember GetMemberFromChannel(DiscordChannel channel, string id)
        {
            if (channel != null)
            {
                DiscordMember foundMember = channel.parent.members.Find(x => x.ID == id);
                if (foundMember != null)
                    return foundMember;
                else
                    DebugLogger.Log("Error in GetMemberFromChannel: foundMember was null!", MessageLevel.Error);
            }
            else
            {
                DebugLogger.Log("Error in GetMemberFromChannel: channel was null!", MessageLevel.Error);
            }
            return null;
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

        public DiscordChannel GetChannelByID(long id)
        {
            return ServersList.Find(x => x.channels.Find(y => y.id == id.ToString()) != null).channels.Find(z => z.id == id.ToString());
        }

        public void AcceptInvite(string inviteID)
        {
            string url = Endpoints.BaseAPI + Endpoints.Invite + $"/{inviteID}";
            try
            {
                var result = WebWrapper.Post(url, token, "", true);
                DebugLogger.Log("Accept invite result: " + result.ToString());
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error accepting invite: {ex.Message}", MessageLevel.Error);
            }
        }

        public DiscordMessage GetLastMessageSent()
        {
            for(int i = MessageLog.Count - 1; i > -1; i--)
            {
                if (MessageLog[i].Value.author.ID == Me.ID)
                    return MessageLog[i].Value;
            }
            return null;
        }
        public DiscordMessage GetLastMessageSent(DiscordChannel inChannel)
        {
            for (int i = MessageLog.Count - 1; i > -1; i--)
            {
                if (MessageLog[i].Value.author.ID == Me.ID)
                    if(MessageLog[i].Value.channel.id == inChannel.id)
                        return MessageLog[i].Value;
            }
            return null;
        }

        public DiscordMessage EditMessage(string MessageID, string replacementMessage, DiscordChannel channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.id}" + Endpoints.Messages + $"/{MessageID}";
            try
            {
                string replacement = JsonConvert.SerializeObject(
                    new
                    {
                        content = replacementMessage,
                        mentions = new string[0]
                    }
                );
                JObject result = JObject.Parse(WebWrapper.Patch(url, token, replacement));

                DiscordMessage m = new DiscordMessage
                {
                    RawJson = result,
                    attachments = result["attachments"].ToObject<string[]>(),
                    author = channel.parent.members.Find(x=>x.ID == result["author"]["id"].ToString()),
                    TypeOfChannelObject = channel.GetType(),
                    channel = channel,
                    content = result["content"].ToString(),
                    id = result["id"].ToString(),
                    timestamp = result["timestamp"].ToObject<DateTime>()
                };
                return m;
            }
            catch(Exception ex)
            {
                DebugLogger.Log("Exception ocurred while editing: " + ex.Message, MessageLevel.Error);
            }

            return null;
        }

        /// <summary>
        /// Simulates typing in the specified channel. Automatically times out/stops after either:
        /// -10 Seconds
        /// -A message is sent
        /// </summary>
        /// <param name="channel"></param>
        public void SimulateTyping(DiscordChannel channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.id}" + Endpoints.Typing;
            try
            {
                WebWrapper.Post(url, token, "");
            }
            catch(Exception ex)
            {
                DebugLogger.Log("Exception ocurred while simulating typing: " + ex.Message, MessageLevel.Error);
            }
        }

        private void SendDeleteRequest(DiscordMessage message, bool user = false)
        {
            string url;
            //if(!user)
                url = Endpoints.BaseAPI + Endpoints.Channels + $"/{message.channel.id}" + Endpoints.Messages + $"/{message.id}";
            //else
                //url = Endpoints.BaseAPI + Endpoints.Channels + $"/{message.channel.id}" + Endpoints.Messages + $"/{message.id}";
            try
            {
                var result = WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Exception ocurred while deleting message (ID: {message.id}): " + ex.Message, MessageLevel.Error);
            }
        }

        private void MessageUpdateEvents(JObject message)
        {
            try
            {
                DiscordServer pserver = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["channel_id"].ToString()) != null);
                DiscordChannel pchannel = pserver.channels.Find(x => x.id == message["d"]["channel_id"].ToString());
                if (pchannel != null)
                {
                    if (message["d"]["author"] != null)
                    {
                        KeyValuePair<string, DiscordMessage> toRemove = MessageLog.Find(x => x.Key == message["d"]["id"].ToString());
                        if (toRemove.Value == null)
                            return; //No message exists
                        var jsonToEdit = toRemove.Value.RawJson;
                        jsonToEdit["d"]["content"].Replace(JToken.FromObject(message["d"]["content"].ToString()));
                        if (MessageEdited != null)
                            MessageEdited(this, new DiscordMessageEditedEventArgs
                            {
                                author = pserver.members.Find(x => x.ID == message["d"]["author"]["id"].ToString()),
                                Channel = pchannel,
                                message = message["d"]["content"].ToString(),
                                MessageType = DiscordMessageType.CHANNEL,
                                MessageEdited = new DiscordMessage
                                {
                                    author = pserver.members.Find(x => x.ID == message["d"]["author"]["id"].ToString()),
                                    content = MessageLog.Find(x => x.Key == message["d"]["id"].ToString()).Value.content,
                                    attachments = message["d"]["attachments"].ToObject<string[]>(),
                                    channel = pserver.channels.Find(x => x.id == message["d"]["channel_id"].ToString()),
                                    RawJson = message,
                                    id = message["d"]["id"].ToString(),
                                    timestamp = message["d"]["timestamp"].ToObject<DateTime>(),
                                },
                                EditedTimestamp = message["d"]["edited_timestamp"].ToObject<DateTime>()
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
                else
                {
                    DebugLogger.Log("Couldn't find channel!", MessageLevel.Critical);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Exception during MessageUpdateEvents.\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", MessageLevel.Critical);
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
                    if (message["d"]["author"]["id"].ToString() != Me.ID)
                    {
                        var foundPM = PrivateChannels.Find(x => x.id == message["d"]["channel_id"].ToString());
                        DiscordPrivateMessageEventArgs dpmea = new DiscordPrivateMessageEventArgs();
                        dpmea.Channel = foundPM;
                        dpmea.message = message["d"]["content"].ToString();
                        DiscordMember tempMember = new DiscordMember(this);
                        tempMember.Username = message["d"]["author"]["username"].ToString();
                        tempMember.ID = message["d"]["author"]["id"].ToString();
                        dpmea.author = tempMember;
                        tempMember.parentclient = this;


                        if (PrivateMessageReceived != null)
                            PrivateMessageReceived(this, dpmea);
                    }
                    else
                    {
                        //if (DebugMessageReceived != null)
                        //    DebugMessageReceived(this, new DiscordDebugMessagesEventArgs { message = "Ignoring MESSAGE_CREATE for private channel for message sent from this client." });
                    }
                }
                else
                {
                    DiscordMessageEventArgs dmea = new DiscordMessageEventArgs();
                    dmea.Channel = foundServerChannel.channels.Find(y => y.id == tempChannelID);

                    dmea.message_text = message["d"]["content"].ToString();

                    DiscordMember tempMember = new DiscordMember(this);
                    tempMember.parentclient = this;
                    tempMember = foundServerChannel.members.Find(x => x.ID == message["d"]["author"]["id"].ToString());
                    dmea.author = tempMember;

                    DiscordMessage m = new DiscordMessage();
                    m.author = dmea.author;
                    m.channel = dmea.Channel;
                    m.TypeOfChannelObject = dmea.Channel.GetType();
                    m.content = dmea.message_text;
                    m.id = message["d"]["id"].ToString();
                    m.RawJson = message;
                    m.timestamp = DateTime.Now;
                    dmea.message = m;

                    Regex r = new Regex("\\d+");
                    foreach (Match mm in r.Matches(dmea.message_text))
                        if (mm.Value == Me.ID)
                            if (MentionReceived != null)
                                MentionReceived(this, dmea);

                    KeyValuePair<string, DiscordMessage> toAdd = new KeyValuePair<string, DiscordMessage>(message["d"]["id"].ToString(), m);
                    MessageLog.Add(toAdd);

                    if (MessageReceived != null)
                        MessageReceived(this, dmea);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log("Error ocurred during MessageCreateEvents: " + ex.Message, MessageLevel.Error);
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
                    tempChannel.parent = foundServer;
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
                DiscordMember recipient = ServersList.Find(x => x.members.Find(y => y.ID == message["d"]["recipient"]["id"].ToString()) != null).members.Find(x => x.ID == message["d"]["recipient"]["id"].ToString());
                tempPrivate.recipient = recipient;
                PrivateChannels.Add(tempPrivate);
                DiscordPrivateChannelEventArgs fak = new DiscordPrivateChannelEventArgs { ChannelType = DiscordChannelCreateType.PRIVATE, ChannelCreated = tempPrivate };
                if (PrivateChannelCreated != null)
                    PrivateChannelCreated(this, fak);
            }
        }
        #endregion
        private string GetGatewayUrl()
        {
            string url = Endpoints.BaseAPI + Endpoints.Gateway;
            try
            {
                return JObject.Parse(WebWrapper.Get(url, token))["url"].ToString();
            }
            catch(Exception ex)
            {
                DebugLogger.Log("Exception ocurred while retrieving Gateway URL: " + ex.Message, MessageLevel.Error);
                return null;
            }
        }

        public DiscordServer GetServerChannelIsIn(DiscordChannel channel)
        {
            return ServersList.Find(x => x.channels.Find(y => y.id == channel.id) != null);
        }

        public void DeleteChannel(DiscordChannel channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.id}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log("Exception ocurred while deleting channel: " + ex.Message, MessageLevel.Error);
            }
        }

        public DiscordChannel CreateChannel(DiscordServer server, string ChannelName, bool voice)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{server.id}" + Endpoints.Channels;
            var reqJson = JsonConvert.SerializeObject(new { name = ChannelName, type = voice ? "voice" : "text" });
            try
            {
                var result = JObject.Parse(WebWrapper.Post(url, token, reqJson));
                if (result != null)
                {
                    DiscordChannel dc = new DiscordChannel { name = result["name"].ToString(), id = result["id"].ToString(), type = result["type"].ToString(), is_private = result["is_private"].ToObject<bool>(), topic = result["topic"].ToString() };
                    server.channels.Add(dc);
                    return dc;
                }
            }
            catch(Exception ex)
            {
                DebugLogger.Log("Exception ocurred while creating channel: " + ex.Message, MessageLevel.Error);
            }
            return null;
        }
        
        public DiscordServer CreateGuild(string GuildName)
        {
            string createGuildUrl = Endpoints.BaseAPI + Endpoints.Guilds;
            string req = JsonConvert.SerializeObject(new { name = GuildName });

            try
            {
                var response = JObject.Parse(WebWrapper.Post(createGuildUrl, token, req));
                if (response != null)
                {
                    DiscordServer server = new DiscordServer();
                    server.id = response["id"].ToString();
                    server.name = response["name"].ToString();
                    server.parentclient = this;

                    string channelGuildUrl = createGuildUrl + $"/{server.id}" + Endpoints.Channels;
                    var channelRespone = JArray.Parse(WebWrapper.Get(channelGuildUrl, token));
                    foreach (var item in channelRespone.Children())
                    {
                        server.channels.Add(new DiscordChannel { name = item["name"].ToString(), id = item["id"].ToString(), topic = item["topic"].ToString(), is_private = item["is_private"].ToObject<bool>(), type = item["type"].ToString() });
                    }

                    server.members.Add(Me);
                    server.owner = server.members.Find(x => x.ID == response["owner_id"].ToString());
                    if (server.owner == null)
                        DebugLogger.Log("Owner is null in CreateGuild!", MessageLevel.Critical);

                    ServersList.Add(server);
                    return server;
                }
            }
            catch(Exception ex)
            {
                DebugLogger.Log("Exception ocurred while creating guild: " + ex.Message, MessageLevel.Error);
            }
            return null;
        }

        public void EditGuildName(DiscordServer guild, string NewGuildName)
        {
            string editGuildUrl = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.id}";
            var newNameJson = JsonConvert.SerializeObject(new { name = NewGuildName });
            try
            {
                WebWrapper.Patch(editGuildUrl, token, newNameJson);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Exception ocurred while editing guild ({guild.name}) name: " + ex.Message, MessageLevel.Error);
            }
        }

        public void AssignRoleToMember(DiscordServer guild, DiscordRole role, DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.id}" + Endpoints.Members + $"/{member.ID}";
            string message = JsonConvert.SerializeObject(new { roles = new string[] { role.id } });
            try
            {
                WebWrapper.Patch(url, token, message);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Exception ocurred while assigning role ({role.name}) to member ({member.Username}): " 
                    + ex.Message, MessageLevel.Error);
            }
        }
        public void AssignRoleToMember(DiscordServer guild, List<DiscordRole> roles, DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.id}" + Endpoints.Members + $"/{member.ID}";
            List<string> rolesAsIds = new List<string>();
            roles.ForEach(x => rolesAsIds.Add(x.id));
            string message = JsonConvert.SerializeObject(new { roles = rolesAsIds.ToArray() });
            try
            {
                WebWrapper.Patch(url, token, message);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Exception ocurred while assigning {roles.Count} role(s) to member ({member.Username}): "
                    + ex.Message, MessageLevel.Error);
            }
        }

        /// <summary>
        /// Creates and invite to the given channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>The invite's code.</returns>
        public string CreateInvite(DiscordChannel channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.id}" + Endpoints.Invites;
            try
            {
                var resopnse = JObject.Parse(WebWrapper.Post(url, token, "{\"validate\":\"\"}"));
                if (resopnse != null)
                {
                    return resopnse["code"].ToString();
                }
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while creating invite for channel {channel.name}: {ex.Message}", MessageLevel.Error);
            }
            return null;
        }

        public void DeleteInvite(string id)
        {
            string url = Endpoints.BaseAPI + Endpoints.Invites + $"/{id}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while deleting invite: {ex.Message}", MessageLevel.Error);
            }
        }

        public string MakeInviteURLFromCode(string code) => "https://discord.gg/" + code;

        /// <summary>
        /// This method temporarily points to the .Connect(); method.
        /// Eventually, this will be removed entirely. Switch while you can!
        /// </summary>
        [Obsolete]
        public void ConnectAndReadMessages() => Connect();

        public void Connect()
        {
            CurrentGatewayURL = GetGatewayUrl();
            DebugLogger.Log("Gateway retrieved: " + CurrentGatewayURL);
            ws = new WebSocket(CurrentGatewayURL);
            ws.EnableRedirection = true;
            ws.Log.File = "websocketlog.txt";
                ws.OnMessage += (sender, e) =>
                {
                    var message = JObject.Parse(e.Data);
                    switch(message["t"].ToString())
                    {
                        case ("READY"):
                            if(WriteLatestReady)
                                using (var sw = new StreamWriter("READY_LATEST.txt"))
                                    sw.Write(message);

                            Me = JsonConvert.DeserializeObject<DiscordMember>(message["d"]["user"].ToString());
                            Me.parentclient = this;
                            ClientPrivateInformation.avatar = Me.Avatar;
                            ClientPrivateInformation.username = Me.Username;
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
                        case ("GUILD_MEMBER_UPDATE"):
                            GuildMemberUpdateEvents(message);
                            break;
                        case ("GUILD_UPDATE"):
                            GuildUpdateEvents(message);
                            break;
                        case ("GUILD_ROLE_DELETE"):
                            GuildRoleDeleteEvents(message);
                            break;
                        case ("GUILD_ROLE_UPDATE"):
                            GuildRoleUpdateEvents(message);
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
                                DiscordMember uuser = server.members.Find(x => x.ID == message["d"]["user_id"].ToString());
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
                        case ("VOICE_SERVER_UPDATE"):
                            VoiceServerUpdateEvents(message);
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
                        case ("CHANNEL_DELETE"):
                            ChannelDeleteEvents(message);
                            break;
                        case ("GUILD_BAN_ADD"):
                            GuildMemberBannedEvents(message);
                            break;
                        case ("GUILD_BAN_REMOVE"):
                            GuildMemberBanRemovedEvents(message);
                            break;
                        case("MESSAGE_ACK"): //ignore this message, it's irrelevant
                            break;
                        default:
                            if (UnknownMessageTypeReceived != null)
                                UnknownMessageTypeReceived(this, new UnknownMessageEventArgs { RawJson = message });
                            break;
                    }
                };
                ws.OnOpen += (sender, e) =>
                {
                    string initJson = JsonConvert.SerializeObject(new
                    {
                        op = 2,
                        d = new
                        {
                            token = token,
                            properties = new DiscordProperties()
                        }
                    });

                    DebugLogger.Log("Sending initJson ( " + initJson + " )");
                    
                    ws.Send(initJson);
                    if (SocketOpened != null)
                        SocketOpened(this, null);

                    KeepAliveTaskToken = KeepAliveTaskTokenSource.Token;
                    KeepAliveTask = new Task(() => 
                    {
                        while (ws.IsAlive)
                        {
                            DebugLogger.Log("Hello from inside KeepAliveTask! Sending..");
                            KeepAlive();
                            Thread.Sleep(HeartbeatInterval);
                            if (KeepAliveTaskToken.IsCancellationRequested)
                                KeepAliveTaskToken.ThrowIfCancellationRequested();
                        }
                    }, KeepAliveTaskToken, TaskCreationOptions.LongRunning);
                    KeepAliveTask.Start();
                    DebugLogger.Log("Began keepalive task..");
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
            DebugLogger.Log("Connecting..");
        }

        private void GuildMemberBanRemovedEvents(JObject message)
        {
            DiscordBanRemovedEventArgs e = new DiscordBanRemovedEventArgs();

            e.Guild = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            e.MemberStub = JsonConvert.DeserializeObject<DiscordMember>(message["d"]["user"].ToString());

            if (BanRemoved != null)
                BanRemoved(this, e);
        }

        private void GuildMemberBannedEvents(JObject message)
        {
            DiscordGuildBanEventArgs e = new DiscordGuildBanEventArgs();
            e.Server = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            if(e.Server != null)
            {
                e.MemberBanned = e.Server.members.Find(x => x.ID == message["d"]["user"]["id"].ToString());
                if(e.MemberBanned != null)
                {
                    if (GuildMemberBanned != null)
                        GuildMemberBanned(this, e);
                    ServersList.Find(x => x.id == e.Server.id).members.Remove(e.MemberBanned);
                }
                else
                {
                    DebugLogger.Log("Error in GuildMemberBannedEvents: MemberBanned is null, attempting internal index of removed members.", MessageLevel.Error);
                    e.MemberBanned = RemovedMembers.Find(x => x.ID == message["d"]["user"]["id"].ToString());
                    if(e.MemberBanned != null)
                    {
                        if (GuildMemberBanned != null)
                            GuildMemberBanned(this, e);
                    }
                    else
                    {
                        DebugLogger.Log("Error in GuildMemberBannedEvents: MemberBanned is null, not even found in internal index!", MessageLevel.Error);
                    }
                }
            }
            else
            {
                DebugLogger.Log("Error in GuildMemberBannedEvents: Server is null?!", MessageLevel.Error);
            }
        }

        private async void VoiceServerUpdateEvents(JObject message)
        {
            VoiceClient.VoiceEndpoint = message["d"]["endpoint"].ToString();
            VoiceClient.Token = message["d"]["token"].ToString();
            
            VoiceClient.Guild = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            VoiceClient.Me = Me;

            VoiceClient.PacketReceived += (sender, e) =>
            {
                if (AudioPacketReceived != null)
                    AudioPacketReceived(sender, e);
            };

            VoiceClient.DebugMessageReceived += (sender, e) =>
            {
                if (VoiceClientDebugMessageReceived != null)
                    VoiceClientDebugMessageReceived(this, e);
            };

            VoiceClient.Initiate();
        }

        /// <summary>
        /// Kicks a specified DiscordMember from the guild that's assumed from their 
        /// parent property.
        /// </summary>
        /// <param name="member"></param>
        public void KickMember(DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{member.Parent.id}" + Endpoints.Members + $"/{member.ID}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error during KickMember\n\t{ex.Message}\n\t{ex.StackTrace}", MessageLevel.Error);
            }
        }
        
        /// <summary>
        /// Bans a specified DiscordMember from the guild that's assumed from their
        /// parent property.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="days">The number of days the user should be banned for, or 0 for infinite.</param>
        public void BanMember(DiscordMember member, int days = 0)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{member.Parent.id}" + Endpoints.Bans + $"/{member.ID}";
            if (days >= 0)
                url += $"?delete-message-days={days}";
            try
            {
                WebWrapper.Put(url, token);
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Error during BanMember\n\t{ex.Message}\n\t{ex.StackTrace}", MessageLevel.Error);
            }
        }

        /// <summary>
        /// Retrieves a DiscordMember List of members banned in the specified server.
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public List<DiscordMember> GetBans(DiscordServer server)
        {
            List<DiscordMember> returnVal = new List<DiscordMember>();
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{server.id}" + Endpoints.Bans;
            try
            {
                JArray response = JArray.Parse(WebWrapper.Get(url, token));
                if (response != null && response.Count > 0)
                {
                    DebugLogger.Log($"Ban count: {response.Count}");

                    foreach(var memberStub in response)
                    {
                        DiscordMember temp = JsonConvert.DeserializeObject<DiscordMember>(memberStub["user"].ToString());
                        if (temp != null)
                            returnVal.Add(temp);
                        else
                            DebugLogger.Log($"memberStub[\"user\"] was null?! Username: {memberStub["user"]["username"].ToString()} ID: {memberStub["user"]["username"].ToString()}", MessageLevel.Error);
                    }
                }
                else
                    return returnVal;
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"An error ocurred while retrieving bans for server \"{server.name}\"\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", 
                    MessageLevel.Error);
            }
            return returnVal;
        }

        public void RemoveBan(DiscordServer guild, string userID)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.id}" + Endpoints.Bans + $"/{userID}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error during RemoveBan\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", MessageLevel.Error);
            }
        }
        public void RemoveBan(DiscordServer guild, DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.id}" + Endpoints.Bans + $"/{member.ID}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Error during RemoveBan\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", MessageLevel.Error);
            }
        }

        /// <summary>
        /// Echoes a received audio packet back.
        /// </summary>
        /// <param name="packet"></param>
        public void EchoPacket(DiscordAudioPacket packet)
        {
            if(VoiceClient != null && ConnectedToVoice())
                VoiceClient.EchoPacket(packet).Wait();
        }

        public void ConnectToVoiceChannel(DiscordChannel channel, bool clientMuted = false, bool clientDeaf = false)
        {
            if (channel.type != "voice")
                throw new InvalidOperationException($"Channel '{channel.name}' is not a voice channel!");

            if (ConnectedToVoice())
                DisconnectFromVoice();

            if (VoiceClient == null)
                VoiceClient = new DiscordVoiceClient(this);
            VoiceClient.Channel = channel;
            VoiceClient.ErrorReceived += (sender, e) =>
            {
                GetLastVoiceClientLogger = VoiceClient.GetDebugLogger;
                DisconnectFromVoice();
            };
            VoiceClient.UserSpeaking += (sender, e) =>
            {
                if (UserSpeaking != null)
                    UserSpeaking(this, e);
            };

            string joinVoicePayload = JsonConvert.SerializeObject(new
            {
                op = 4,
                d = new
                {
                    guild_id = channel.parent.id,
                    channel_id = channel.id,
                    self_mute = clientMuted,
                    self_deaf = clientDeaf
                }
            });

            ws.Send(joinVoicePayload);
        }

        /// <summary>
        /// Also disposes
        /// </summary>
        public void DisconnectFromVoice()
        {
            string disconnectMessage = JsonConvert.SerializeObject(new
            {
                op = 4,
                d = new
                {
                    guild_id = (object)null,
                    channel_id = (object)null,
                    self_mute = true,
                    self_deaf = false
                }
            });
            if (VoiceClient != null)
            {
                try
                {
                    VoiceClient.Dispose();
                    VoiceClient = null;
                    

                    ws.Send(disconnectMessage);
                }
                catch
                {
                    if(ws != null)
                        ws.Send(disconnectMessage);
                }
            }
            VoiceClient = null;
        }

        public DiscordVoiceClient GetVoiceClient()
        {
            if (ConnectedToVoice() && VoiceClient != null)
                return VoiceClient;

            return null;
        }

        private void GuildMemberUpdateEvents(JObject message)
        {
            DiscordServer server = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            
            DiscordMember memberUpdated = JsonConvert.DeserializeObject<DiscordMember>(message["d"]["user"].ToString());
            memberUpdated.parentclient = this;
            memberUpdated.Verified = server.members.Find(x => x.ID == message["d"]["user"]["id"].ToString()).Verified;
            memberUpdated.Parent = server;

            foreach(var roles in message["d"]["roles"])
            {
                memberUpdated.Roles.Add(server.roles.Find(x => x.id == roles.ToString()));    
            }
            //need to ask voltana if this is actually necessary or i can use the reference i got to server above
            ServersList.Find(x => x.id == message["d"]["guild_id"].ToString()).members.Remove(ServersList.Find(x => x.id == message["d"]["guild_id"].ToString()).members.Find(x => x.ID == message["d"]["user"]["id"].ToString()));
            ServersList.Find(x => x.id == message["d"]["guild_id"].ToString()).members.Add(memberUpdated);

            if (GuildMemberUpdated != null)
                GuildMemberUpdated(this, new DiscordGuildMemberUpdateEventArgs { MemberUpdate = memberUpdated, RawJson = message, ServerUpdated = server });            
        }

        private void GuildRoleUpdateEvents(JObject message)
        {
            DiscordServer inServer = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            DiscordRole roleUpdated = new DiscordRole
            {
                name = message["d"]["role"]["name"].ToString(),
                position = message["d"]["role"]["position"].ToObject<int>(),
                permissions = new DiscordPermission(message["d"]["role"]["permissions"].ToObject<uint>()),
                managed = message["d"]["role"]["managed"].ToObject<bool>(),
                hoist = message["d"]["role"]["hoist"].ToObject<bool>(),
                color = new Color(message["d"]["role"]["color"].ToObject<int>().ToString("x")),
                id = message["d"]["role"]["id"].ToString(),
            };

            ServersList.Find(x => x.id == inServer.id).roles.Remove(ServersList.Find(x => x.id == inServer.id).roles.Find(y => y.id == roleUpdated.id));
            ServersList.Find(x => x.id == inServer.id).roles.Add(roleUpdated);

            if (RoleUpdated != null)
                RoleUpdated(this, new DiscordGuildRoleUpdateEventArgs { RawJson = message, RoleUpdated = roleUpdated, InServer = inServer });
        }

        private void GuildRoleDeleteEvents(JObject message)
        {
            DiscordServer inServer = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            DiscordRole deletedRole = inServer.roles.Find(x => x.id == message["d"]["role_id"].ToString());

            ServersList.Find(x => x.id == inServer.id).roles.Remove(ServersList.Find(x => x.id == inServer.id).roles.Find(y => y.id == deletedRole.id));

            if (RoleDeleted != null)
                RoleDeleted(this, new DiscordGuildRoleDeleteEventArgs { DeletedRole = deletedRole, Guild = inServer, RawJson = message });
        }

        public DiscordRole CreateRole(DiscordServer guild)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.id}" + Endpoints.Roles;

            try
            {
                var result = JObject.Parse(WebWrapper.Post(url, token, ""));

                if (result != null)
                {
                    DiscordRole d = new DiscordRole
                    {
                        color = new Color(result["color"].ToObject<int>().ToString("x")),
                        hoist = result["hoist"].ToObject<bool>(),
                        id = result["id"].ToString(),
                        managed = result["managed"].ToObject<bool>(),
                        name = result["name"].ToString(),
                        permissions = new DiscordPermission(result["permissions"].ToObject<uint>()),
                        position = result["position"].ToObject<int>()
                    };

                    ServersList.Find(x => x.id == guild.id).roles.Add(d);
                    return d;
                }
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while creating role in guild {guild.name}: {ex.Message}", MessageLevel.Error);
            }
            return null;
        }

        public DiscordRole EditRole(DiscordServer guild, DiscordRole newRole)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.id}" + Endpoints.Roles + $"/{newRole.id}";
            string request = JsonConvert.SerializeObject(
                new
                {
                    color = decimal.Parse(newRole.color.ToDecimal().ToString()),
                    hoist = newRole.hoist,
                    name = newRole.name,
                    permissions = newRole.permissions.GetRawPermissions()
                }
            );

            try
            {
                var result = JObject.Parse(WebWrapper.Patch(url, token, request));
                if (result != null)
                {
                    DiscordRole d = new DiscordRole
                    {
                        color = new Color(result["color"].ToObject<int>().ToString("x")),
                        hoist = result["hoist"].ToObject<bool>(),
                        id = result["id"].ToString(),
                        managed = result["managed"].ToObject<bool>(),
                        name = result["name"].ToString(),
                        permissions = new DiscordPermission(result["permissions"].ToObject<uint>()),
                        position = result["position"].ToObject<int>()
                    };

                    ServersList.Find(x => x.id == guild.id).roles.Remove(d);
                    ServersList.Find(x => x.id == guild.id).roles.Add(d);
                    return d;
                }
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while editing role ({newRole.name}): {ex.Message}", MessageLevel.Error);
            }

            return null;
        }

        public void DeleteRole(DiscordServer guild, DiscordRole role)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.id}" + Endpoints.Roles + $"/{role.id}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while deleting role ({role.name}): {ex.Message}", MessageLevel.Error);
            }
        }

        private void GuildUpdateEvents(JObject message)
        {
            DiscordServer oldServer = ServersList.Find(x => x.id == message["d"]["id"].ToString());
            DiscordServer newServer = new DiscordServer
            {
                name = message["d"]["name"].ToString(),
                id = message["d"]["name"].ToString()
            };
            newServer.parentclient = this;
            newServer.roles = new List<DiscordRole>();
            if (!message["d"]["roles"].IsNullOrEmpty())
            {
                foreach (var roll in message["d"]["roles"])
                {
                    DiscordRole t = new DiscordRole
                    {
                        color = new DiscordSharp.Color(roll["color"].ToObject<int>().ToString("x")),
                        name = roll["name"].ToString(),
                        permissions = new DiscordPermission(roll["permissions"].ToObject<uint>()),
                        position = roll["position"].ToObject<int>(),
                        managed = roll["managed"].ToObject<bool>(),
                        id = roll["id"].ToString(),
                        hoist = roll["hoist"].ToObject<bool>()
                    };
                    newServer.roles.Add(t);
                }
            }
            newServer.channels = new List<DiscordChannel>();
            if (!message["d"]["channels"].IsNullOrEmpty())
            {
                foreach (var u in message["d"]["channels"])
                {
                    DiscordChannel tempSub = new DiscordChannel();
                    tempSub.id = u["id"].ToString();
                    tempSub.name = u["name"].ToString();
                    tempSub.type = u["type"].ToString();
                    tempSub.topic = u["topic"].ToString();
                    tempSub.parent = newServer;
                    List<DiscordPermissionOverride> permissionoverrides = new List<DiscordPermissionOverride>();
                    foreach (var o in u["permission_overwrites"])
                    {
                        DiscordPermissionOverride dpo = new DiscordPermissionOverride(o["allow"].ToObject<uint>(), o["deny"].ToObject<uint>());
                        dpo.type = o["type"].ToObject<DiscordPermissionOverride.OverrideType>();
                        dpo.id = o["id"].ToString();

                        permissionoverrides.Add(dpo);
                    }
                    tempSub.PermissionOverrides = permissionoverrides;

                    newServer.channels.Add(tempSub);
                }
            }
            if (!message["d"]["members"].IsNullOrEmpty())
            {
                foreach (var mm in message["d"]["members"])
                {
                    DiscordMember member = JsonConvert.DeserializeObject<DiscordMember>(mm["user"].ToString());
                    member.parentclient = this;
                    member.Parent = newServer;

                    JArray rawRoles = JArray.Parse(mm["roles"].ToString());
                    if (rawRoles.Count > 0)
                    {
                        foreach (var role in rawRoles.Children())
                        {
                            member.Roles.Add(newServer.roles.Find(x => x.id == role.Value<string>()));
                        }
                    }
                    else
                    {
                        member.Roles.Add(newServer.roles.Find(x => x.name == "@everyone"));
                    }

                    newServer.members.Add(member);
                }
            }
            if (!message["d"]["owner_id"].IsNullOrEmpty())
            {
                newServer.owner = newServer.members.Find(x => x.ID == message["d"]["owner_id"].ToString());
                DebugLogger.Log($"Transferred ownership from user '{oldServer.owner.Username}' to {newServer.owner.Username}.");
            }
            ServersList.Remove(oldServer);
            ServersList.Add(newServer);
            DiscordServerUpdateEventArgs dsuea = new DiscordServerUpdateEventArgs { NewServer = newServer, OldServer = oldServer };
            if (GuildUpdated != null)
                GuildUpdated(this, dsuea);
        }

        private void ChannelDeleteEvents(JObject message)
        {
            if (!message["d"]["recipient"].IsNullOrEmpty())
            {
                //private channel removed
                DiscordPrivateChannelDeleteEventArgs e = new DiscordPrivateChannelDeleteEventArgs();
                e.PrivateChannelDeleted = PrivateChannels.Find(x => x.id == message["d"]["id"].ToString());
                if(e.PrivateChannelDeleted != null)
                {
                    if (PrivateChannelDeleted != null)
                        PrivateChannelDeleted(this, e);
                    PrivateChannels.Remove(e.PrivateChannelDeleted);
                }
                else
                {
                    DebugLogger.Log("Error in ChannelDeleteEvents: PrivateChannel is null!", MessageLevel.Error);
                }
            }
            else
            {
                DiscordChannelDeleteEventArgs e = new DiscordChannelDeleteEventArgs { ChannelDeleted = GetChannelByID(message["d"]["id"].ToObject<long>()) };
                DiscordServer server;
                server = e.ChannelDeleted.parent;
                server.channels.Remove(server.channels.Find(x => x.id == e.ChannelDeleted.id));

                if (ChannelDeleted != null)
                    ChannelDeleted(this, e);
            }
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

            List<DiscordPermissionOverride> permissionoverrides = new List<DiscordPermissionOverride>();
            foreach (var o in message["d"]["permission_overwrites"])
            {
                DiscordPermissionOverride dpo = new DiscordPermissionOverride(o["allow"].ToObject<uint>(), o["deny"].ToObject<uint>());
                dpo.type = o["type"].ToObject<DiscordPermissionOverride.OverrideType>();
                dpo.id = o["id"].ToString();

                permissionoverrides.Add(dpo);
            }
            newChannel.PermissionOverrides = permissionoverrides;

            e.NewChannel = newChannel;

            DiscordServer serverToRemoveFrom = ServersList.Find(x => x.channels.Find(y => y.id == newChannel.id) != null);
            newChannel.parent = serverToRemoveFrom;
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
            server.parentclient = this;
            server.id = message["d"]["id"].ToString();
            server.name = message["d"]["name"].ToString();
            server.members = new List<DiscordMember>();
            server.channels = new List<DiscordChannel>();
            server.roles = new List<DiscordRole>();
            foreach (var roll in message["d"]["roles"])
            {
                DiscordRole t = new DiscordRole
                {
                    color = new DiscordSharp.Color(roll["color"].ToObject<int>().ToString("x")),
                    name = roll["name"].ToString(),
                    permissions = new DiscordPermission(roll["permissions"].ToObject<uint>()),
                    position = roll["position"].ToObject<int>(),
                    managed = roll["managed"].ToObject<bool>(),
                    id = roll["id"].ToString(),
                    hoist = roll["hoist"].ToObject<bool>()
                };
                server.roles.Add(t);
            }
            foreach (var chn in message["d"]["channels"])
            {
                DiscordChannel tempChannel = new DiscordChannel();
                tempChannel.id = chn["id"].ToString();
                tempChannel.type = chn["type"].ToString();
                tempChannel.topic = chn["topic"].ToString();
                tempChannel.name = chn["name"].ToString();
                tempChannel.is_private = false;
                tempChannel.PermissionOverrides = new List<DiscordPermissionOverride>();
                tempChannel.parent = server;
                foreach (var o in chn["permission_overwrites"])
                {
                    if (tempChannel.type == "voice")
                        continue;
                    DiscordPermissionOverride dpo = new DiscordPermissionOverride(o["allow"].ToObject<uint>(), o["deny"].ToObject<uint>());
                    dpo.id = o["id"].ToString();

                    if (o["type"].ToString() == "member")
                        dpo.type = DiscordPermissionOverride.OverrideType.member;
                    else
                        dpo.type = DiscordPermissionOverride.OverrideType.role;

                    tempChannel.PermissionOverrides.Add(dpo);
                }
                server.channels.Add(tempChannel);
            }
            foreach(var mbr in message["d"]["members"])
            {
                DiscordMember member = JsonConvert.DeserializeObject<DiscordMember>(mbr["user"].ToString());
                member.parentclient = this;
                member.Parent = server;
                
                foreach(var rollid in mbr["roles"])
                {
                    member.Roles.Add(server.roles.Find(x => x.id == rollid.ToString()));
                }
                if (member.Roles.Count == 0)
                    member.Roles.Add(server.roles.Find(x => x.name == "@everyone"));
                server.members.Add(member);
            }
            server.owner = server.members.Find(x => x.ID == message["d"]["owner_id"].ToString());

            ServersList.Add(server);
            e.server = server;
            if (GuildCreated != null)
                GuildCreated(this, e);
        }

        private void GuildMemberAddEvents(JObject message)
        {
            DiscordGuildMemberAddEventArgs e = new DiscordGuildMemberAddEventArgs();
            e.RawJson = message;
            DiscordMember newMember = JsonConvert.DeserializeObject<DiscordMember>(message["d"]["user"].ToString());
            newMember.parentclient = this;
            e.AddedMember = newMember;
            e.Guild = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            newMember.Parent = e.Guild;
            e.roles = message["d"]["roles"].ToObject<string[]>();
            e.JoinedAt = DateTime.Parse(message["d"]["joined_at"].ToString());

            ServersList.Find(x => x == e.Guild).members.Add(newMember);
            if (UserAddedToServer != null)
                UserAddedToServer(this, e);
        }
        private void GuildMemberRemoveEvents(JObject message)
        {
            DiscordGuildMemberRemovedEventArgs e = new DiscordGuildMemberRemovedEventArgs();
            DiscordMember removed = new DiscordMember(this);
            removed.parentclient = this;

            List<DiscordMember> membersToRemove = new List<DiscordMember>();
            foreach(var server in ServersList)
            {
                if (server.id != message["d"]["guild_id"].ToString())
                    continue;
                for(int i = 0; i < server.members.Count; i++)
                {
                    if(server.members[i].ID == message["d"]["user"]["id"].ToString())
                    {
                        removed = server.members[i];
                        membersToRemove.Add(removed);
                        RemovedMembers.Add(removed);
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
            DiscordMember newMember = JsonConvert.DeserializeObject<DiscordMember>(message["d"].ToString());
            newMember.parentclient = this;

            DiscordMember oldMember = new DiscordMember(this);
            oldMember.parentclient = this;
            //Update members
            foreach (var server in ServersList)
            {
                for (int i = 0; i < server.members.Count; i++)
                {
                    if (server.members[i].ID == newMember.ID)
                    {
                        server.members[i] = newMember;
                        oldMember = server.members[i];
                    }
                }
            }

            newMember.Parent = oldMember.Parent;

            if (!message["roles"].IsNullOrEmpty())
            {
                JArray rawRoles = JArray.Parse(message["roles"].ToString());
                if (rawRoles.Count > 0)
                {
                    foreach (var role in rawRoles.Children())
                    {
                        newMember.Roles.Add(newMember.Parent.roles.Find(x => x.id == role.Value<string>()));
                    }
                }
                else
                {
                    newMember.Roles.Add(newMember.Parent.roles.Find(x => x.name == "@everyone"));
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

            DiscordServer inServer;
            inServer = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["channel_id"].ToString()) != null);
            if(inServer == null) //dm delete
            {
                DiscordPrivateMessageDeletedEventArgs dm = new DiscordPrivateMessageDeletedEventArgs();
                dm.DeletedMessage = e.DeletedMessage;
                dm.RawJson = message;
                dm.Channel = PrivateChannels.Find(x => x.id == message["d"]["channel_id"].ToString());
                if (PrivateMessageDeleted != null)
                    PrivateMessageDeleted(this, dm);
            }
            else
            {
                e.Channel = inServer.channels.Find(x => x.id == message["d"]["channel_id"].ToString());
                e.RawJson = message;
            }

            if (MessageDeleted != null)
                MessageDeleted(this, e);
        }

        private void VoiceStateUpdateEvents(JObject message)
        {
            var f = message["d"]["channel_id"];
            if (f.Value<String>() == null)
            {
                DiscordLeftVoiceChannelEventArgs le = new DiscordLeftVoiceChannelEventArgs();
                DiscordServer inServer = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
                le.user = inServer.members.Find(x => x.ID == message["d"]["user_id"].ToString());
                le.guild = inServer;
                le.RawJson = message;

                if (UserLeftVoiceChannel != null)
                    UserLeftVoiceChannel(this, le);
                return;
            }
            DiscordVoiceStateUpdateEventArgs e = new DiscordVoiceStateUpdateEventArgs();
            e.user = ServersList.Find(x => x.members.Find(y => y.ID == message["d"]["user_id"].ToString()) != null).members.Find(x => x.ID == message["d"]["user_id"].ToString());
            e.guild = ServersList.Find(x => x.id == message["d"]["guild_id"].ToString());
            e.channel = ServersList.Find(x => x.channels.Find(y => y.id == message["d"]["channel_id"].ToString()) != null).channels.Find(x => x.id == message["d"]["channel_id"].ToString());
            if(!message["d"]["self_deaf"].IsNullOrEmpty())
                e.self_deaf = message["d"]["self_deaf"].ToObject<bool>();
            e.deaf = message["d"]["deaf"].ToObject<bool>();
            if(!message["d"]["self_mute"].IsNullOrEmpty())
                e.self_mute = message["d"]["self_mute"].ToObject<bool>();
            e.mute = message["d"]["mute"].ToObject<bool>();
            e.suppress = message["d"]["suppress"].ToObject<bool>();
            e.RawJson = message;

            if (!message["d"]["session_id"].IsNullOrEmpty())
            {
                if(e.user.ID == Me.ID)
                {
                    Me.Muted = e.self_mute;
                    Me.Deaf = e.self_deaf;
                    if(VoiceClient != null)
                        VoiceClient.SessionID = message["d"]["session_id"].ToString();
                }
            }

            if (VoiceStateUpdate != null)
                VoiceStateUpdate(this, e);
        }
        private JObject ServerInfo(string channelOrServerId)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{channelOrServerId}";
            try
            {
                return JObject.Parse(WebWrapper.Get(url, token));
            }
            catch(Exception ex)
            {
                DebugLogger.Log("(Private) Error ocurred while retrieving server info: " + ex.Message, MessageLevel.Error);
            }
            return null;
        }

        private int HeartbeatInterval = 41250;
        private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private void KeepAlive()
        {
            string keepAliveJson = "{\"op\":1, \"d\":" + DateTime.Now.Millisecond + "}";
                if (ws != null)
                if (ws.IsAlive)
                {
                    int unixTime = (int)(DateTime.UtcNow - epoch).TotalMilliseconds;
                    keepAliveJson = "{\"op\":1, \"d\":\"" + unixTime + "\"}";
                    ws.Send(keepAliveJson);
                    if (KeepAliveSent != null)
                        KeepAliveSent(this, new DiscordKeepAliveSentEventArgs { SentAt = DateTime.Now, JsonSent = keepAliveJson } );
                }
        }

        
        //Thread ConnectReadMessagesThread;
        public void Dispose()
        {
            try
            {
                KeepAliveTaskTokenSource.Cancel();
                ws.Close();
                ws = null;
                ServersList = null;
                PrivateChannels = null;
                Me = null;
                token = null;
                this.ClientPrivateInformation = null;
            }
            catch { /*already been disposed elsewhere */ }
        }

        public void Logout()
        {
            string url = Endpoints.BaseAPI + Endpoints.Auth + "/logout";
            string msg = JsonConvert.SerializeObject(new { token = token });
            WebWrapper.Post(url, msg);
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
                await sw.WriteAsync(msg).ConfigureAwait(false);
                sw.Flush();
                sw.Close();
            }
            try
            {
                var httpResponseT = await httpWebRequest.GetResponseAsync().ConfigureAwait(false);
                var httpResponse = (HttpWebResponse)httpResponseT;
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = await sr.ReadToEndAsync().ConfigureAwait(false);
                    var jsonResult = JObject.Parse(result);

                    if(!jsonResult["token"].IsNullOrEmpty() || jsonResult["token"].ToString() != "")
                    {
                        token = jsonResult["token"].ToString();
                    }
                    else
                        return null;
                }
            }
            catch (WebException e)
            {
                using (StreamReader s = new StreamReader(e.Response.GetResponseStream()))
                {
                    string result = await s.ReadToEndAsync().ConfigureAwait(false);
                    var jsonResult = JObject.Parse(result);

                    if (!jsonResult["password"].IsNullOrEmpty())
                        throw new DiscordLoginException((jsonResult["password"].ToObject<JArray>()[0].ToString()));
                    if(!jsonResult["email"].IsNullOrEmpty())
                        throw new DiscordLoginException((jsonResult["email"].ToObject<JArray>()[0].ToString()));
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
            if (File.Exists("token_cache"))
            {
                using (var sr = new StreamReader("token_cache"))
                {
                    token = sr.ReadLine();
                    DebugLogger.Log("Loading token from cache.");
                }
            }
            else
            {
                if (ClientPrivateInformation == null || ClientPrivateInformation.email == null || ClientPrivateInformation.password == null)
                    throw new ArgumentNullException("You didn't supply login information!");
                string url = Endpoints.BaseAPI + Endpoints.Auth + Endpoints.Login;
                string msg = JsonConvert.SerializeObject(new
                {
                    email = ClientPrivateInformation.email,
                    password = ClientPrivateInformation.password
                });
                DebugLogger.Log("No token present, sending login request..");
                var result = JObject.Parse(WebWrapper.Post(url, msg));
                token = result["token"].ToString();

                using (var sw = new StreamWriter("token_cache"))
                {
                    sw.WriteLine(token);
                    DebugLogger.Log("token_cache written!");
                }
            }

            return token;
        }
    }
}
