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
using DiscordSharp.Objects;

namespace DiscordSharp
{
    /// <summary>
    /// Properties that Discord uses upon connection to the websocket. Mostly used for analytics internally.
    /// </summary>
    public class DiscordProperties
    {
        /// <summary>
        /// The OS you're on
        /// </summary>
        [JsonProperty("os")]
        public string OS { get; set; }

        /// <summary>
        /// The "browser" you're using.
        /// </summary>
        [JsonProperty("browser")]
        public string Browser { get; set; }

        /// <summary>
        /// Whatever device you want to be on. (Default: DiscordSharp Bot)
        /// </summary>
        [JsonProperty("device")]
        public string Device
        { get; set; } = "DiscordSharp Bot";

        /// <summary>
        /// 
        /// </summary>
        public string referrer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string referring_domain { get; set; }
        
        /// <summary>
        /// Default constructor setting the OS property to Environment.OSVersion.ToString();
        /// </summary>
        public DiscordProperties()
        {
            OS = Environment.OSVersion.ToString();
        }

        /// <summary>
        /// Serializes this object as json.
        /// </summary>
        /// <returns>Json string of this object serialized</returns>
        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// The type of message that the Discord message is.
    /// </summary>
    public enum DiscordMessageType
    {
        /// <summary>
        /// Private/DM
        /// </summary>
        PRIVATE,
        /// <summary>
        /// Public/in a channel.
        /// </summary>
        CHANNEL
    }

    /// <summary>
    /// Where the magic happens. 
    /// </summary>
    public class DiscordClient
    {
        /// <summary>
        /// The token associated with authenticating your bot and ensuring they can send messages.
        /// </summary>
        public static string token { get; internal set; } = null;
        /// <summary>
        /// If this is true, the account this client is running on is a special bot account.
        /// </summary>
        public static bool IsBotAccount { get; internal set; } = false;

        /// <summary>
        /// The URL that the client websocket is currently connected to.
        /// </summary>
        public string CurrentGatewayURL { get; internal set; }
        
        /// <summary>
        /// Any private information assosciated with the account (regular clients only)
        /// </summary>
        public DiscordUserInformation ClientPrivateInformation { get; set; }

        /// <summary>
        /// Custom properties containing parameters such as
        /// * OS
        /// * Referrer
        /// * Browser
        /// Used by Discord internally for connection.
        /// </summary>
        public DiscordProperties DiscordProperties { get; set; } = new DiscordProperties();

        /// <summary>
        /// The current DiscordMember object assosciated with the account you're connected to.
        /// </summary>
        public DiscordMember Me { get; internal set; }

        /// <summary>
        /// Returns the debug logger used to log various debug events.
        /// </summary>
        public Logger GetTextClientLogger => DebugLogger;

        /// <summary>
        /// Returns the last debug logger for when the voice client was last connected.
        /// </summary>
        public Logger GetLastVoiceClientLogger;


        private WebSocket ws;
        private List<DiscordServer> ServersList { get; set; }
        private string CurrentGameName = "";
        private int? IdleSinceUnixTime = null;
        static string UserAgentString = $" (http://github.com/Luigifan/DiscordSharp, {typeof(DiscordClient).Assembly.GetName().Version.ToString()})";
        private DiscordVoiceClient VoiceClient;
        private Logger DebugLogger = new Logger();
        private CancellationTokenSource KeepAliveTaskTokenSource = new CancellationTokenSource();
        private CancellationToken KeepAliveTaskToken;
        private Task KeepAliveTask;
        private static string StrippedEmail = "";

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
        /// Whether or not to request all users in a guild (including offlines) on startup.
        /// </summary>
        public bool RequestAllUsersOnStartup { get; set; } = false;

        /// <summary>
        /// A log of messages kept in a KeyValuePair.
        /// The key is the id of the message, and the value is a DiscordMessage object. If you need raw json, this is contained inside of the DiscordMessage object now.
        /// </summary>
        private List<KeyValuePair<string, DiscordMessage>> MessageLog = new List<KeyValuePair<string, DiscordMessage>>();
        private List<DiscordPrivateChannel> PrivateChannels = new List<DiscordPrivateChannel>();

        #region Event declaration
        public event EventHandler<DiscordMessageEventArgs> MessageReceived;
        public event EventHandler<DiscordConnectEventArgs> Connected;
        public event EventHandler<EventArgs> SocketOpened;
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
        /// <summary>
        /// Occurs when the voice client is fully connected to voice.
        /// </summary>
        public event EventHandler<EventArgs> VoiceClientConnected;
        /// <summary>
        /// Occurs when the voice queue is emptied.
        /// </summary>
        public event EventHandler<EventArgs> VoiceQueueEmpty;
        #endregion
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenOverride">If you have a token you wish to use, provide it here. Else, a login attempt will be made.</param>
        /// <param name="isBotAccount">Set this to true if your bot is going to be a bot account</param>
        public DiscordClient(string tokenOverride = null, bool isBotAccount = false, bool enableLogging = true)
        {
            if (isBotAccount && tokenOverride == null)
                throw new Exception("Token override cannot be null if using a bot account!");
            DebugLogger.EnableLogging = enableLogging;

            token = tokenOverride;
            IsBotAccount = isBotAccount;

            if (IsBotAccount)
                UserAgentString = "DiscordBot " + UserAgentString;
            else
                UserAgentString = "Custom Discord Client " + UserAgentString;

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

        /// <summary>
        /// Current DiscordServers you're connected to.
        /// </summary>
        /// <returns>DiscordServer list of servers you're currently connected to.</returns>
        public List<DiscordServer> GetServersList() => ServersList;

        /// <summary>
        /// Any messages logged since connection to the websocket.
        /// </summary>
        /// <returns>A KeyValuePair list of string-DiscordMessage. Where string is the message's ID</returns>
        public List<KeyValuePair<string, DiscordMessage>> GetMessageLog() => MessageLog;

        /// <summary>
        /// Private channels assosciated with the account.
        /// </summary>
        /// <returns>a list of DiscordPrivateChannels.</returns>
        public List<DiscordPrivateChannel> GetPrivateChannels() => PrivateChannels;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if connected to voice.</returns>
        public bool ConnectedToVoice() => VoiceClient != null ? VoiceClient.Connected : false;

        //eh
        private void GetChannelsList(JObject m)
        {
            if (ServersList == null)
                ServersList = new List<DiscordServer>();
            foreach(var j in m["d"]["guilds"])
            {
                if (!j["unavailable"].IsNullOrEmpty() && j["unavailable"].ToObject<bool>() == true)
                    continue; //unavailable server
                DiscordServer temp = new DiscordServer();
                temp.parentclient = this;
                temp.JoinedAt = j["joined_at"].ToObject<DateTime>();
                temp.ID = j["id"].ToString();
                temp.Name = j["name"].ToString();
                if (!j["icon"].IsNullOrEmpty())
                    temp.icon = j["icon"].ToString();
                else
                    temp.icon = null;

                //temp.owner_id = j["owner_id"].ToString();
                List<DiscordChannel> tempSubs = new List<DiscordChannel>();

                List<DiscordRole> tempRoles = new List<DiscordRole>();
                foreach(var u in j["roles"])
                {
                    DiscordRole t = new DiscordRole
                    {
                        Color = new DiscordSharp.Color(u["color"].ToObject<int>().ToString("x")),
                        Name = u["name"].ToString(),
                        Permissions = new DiscordPermission(u["permissions"].ToObject<uint>()),
                        Position = u["position"].ToObject<int>(),
                        Managed = u["managed"].ToObject<bool>(),
                        ID = u["id"].ToString(),
                        Hoist = u["hoist"].ToObject<bool>()
                    };
                    tempRoles.Add(t);
                }
                temp.Roles = tempRoles;
                foreach(var u in j["channels"])
                {
                    DiscordChannel tempSub = new DiscordChannel();
                    tempSub.ID = u["id"].ToString();
                    tempSub.Name = u["name"].ToString();
                    tempSub.Type = u["type"].ToObject<ChannelType>();
                    if(!u["topic"].IsNullOrEmpty())
                        tempSub.Topic = u["topic"].ToString();
                    if (tempSub.Type == ChannelType.Voice && !u["bitrate"].IsNullOrEmpty())
                        tempSub.Bitrate = u["bitrate"].ToObject<int>();
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
                temp.Channels = tempSubs;
                foreach(var mm in j["members"])
                {
                    DiscordMember member = JsonConvert.DeserializeObject<DiscordMember>(mm["user"].ToString());
                    member.parentclient = this;
                    member.Roles = new List<DiscordRole>();
                    JArray rawRoles = JArray.Parse(mm["roles"].ToString());
                    if(rawRoles.Count > 0)
                    {
                        foreach(var role in rawRoles.Children())
                        {
                            member.Roles.Add(temp.Roles.Find(x => x.ID == role.Value<string>()));
                        }
                    }
                    else
                    {
                        member.Roles.Add(temp.Roles.Find(x => x.Name == "@everyone"));
                    }
                    member.Parent = temp;

                    temp.Members.Add(member);
                }
                if (!j["presences"].IsNullOrEmpty())
                {
                    foreach (var presence in j["presences"])
                    {
                        DiscordMember member = temp.Members.Find(x => x.ID == presence["user"]["id"].ToString());
                        member.SetPresence(presence["status"].ToString());
                        if (!presence["game"].IsNullOrEmpty())
                            member.CurrentGame = presence["game"]["name"].ToString();
                    }
                }
                temp.Region = j["region"].ToString();
                temp.Owner = temp.Members.Find(x => x.ID == j["owner_id"].ToString());
                ServersList.Add(temp);
            }
            if (PrivateChannels == null)
                PrivateChannels = new List<DiscordPrivateChannel>();
            foreach (var privateChannel in m["d"]["private_channels"])
            {
                DiscordPrivateChannel tempPrivate = JsonConvert.DeserializeObject<DiscordPrivateChannel>(privateChannel.ToString());
                tempPrivate.user_id = privateChannel["recipient"]["id"].ToString();
                DiscordServer potentialServer = new DiscordServer();
                ServersList.ForEach(x =>
                {
                    x.Members.ForEach(y =>
                    {
                        if (y.ID == privateChannel["recipient"]["id"].ToString())
                            potentialServer = x;
                    });
                });
                if (potentialServer.Owner != null) //should be a safe test..i hope
                {
                    DiscordMember recipient = potentialServer.Members.Find(x => x.ID == privateChannel["recipient"]["id"].ToString());
                    if (recipient != null)
                    {
                        tempPrivate.recipient = recipient;
                    }
                    else
                    {
                        DebugLogger.Log("Recipient was null!!!!", MessageLevel.Critical);
                    }
                }
                else
                {
                    DebugLogger.Log("No potential server found for user's private channel null! This will probably fix itself.", MessageLevel.Debug);
                }
                PrivateChannels.Add(tempPrivate);
            }

        }

        /// <summary>
        /// Sends an http DELETE request to leave the server you send in this parameter.
        /// </summary>
        /// <param name="server">The DiscordServer object you want to leave.</param>
        public void LeaveServer(DiscordServer server) => LeaveServer(server.ID);

        /// <summary>
        /// (Owner only, non-bot only) Sends an http DELETE request to delete the server you specify.
        /// </summary>
        /// <param name="server">The DiscordServer object you want to delete.</param>
        public void DeleteServer(DiscordServer server) => DeleteServer(server.ID);

        /// <summary>
        /// (Owner only, non-bot only) Sends an http DELETE request to delete the server you specify.
        /// </summary>
        /// <param name="ServerID">The server's ID you want to delete.</param>
        public void LeaveServer(string ServerID)
        {
            string url = //Endpoints.BaseAPI + Endpoints.Guilds + $"/{ServerID}";
                Endpoints.BaseAPI + Endpoints.Users + Endpoints.Me + Endpoints.Guilds + $"/{ServerID}"; //old, left for lulz
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
        /// (Owner only, non-bot only) Sends an http DELETE request to delete the server you specify by ID.
        /// </summary>
        /// <param name="ServerID">The server's ID you want to delete.</param>
        public void DeleteServer(string ServerID)
        {
            if (IsBotAccount)
                throw new Exception("Bot accounts can't own servers!");

            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{ServerID}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Error ocurred while deleting server ({ServerID}): {ex.Message}", MessageLevel.Error);
            }
        }


        /// <summary>
        /// Sends a message to a channel, what else did you expect?
        /// </summary>
        /// <param name="message">The text to send</param>
        /// <param name="channel">DiscordChannel object to send the message to.</param>
        /// <returns>A DiscordMessage object of the message sent to Discord.</returns>
        public DiscordMessage SendMessageToChannel(string message, DiscordChannel channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID}" + Endpoints.Messages;
            try
            {
                JObject result = JObject.Parse(WebWrapper.Post(url, token, JsonConvert.SerializeObject(Utils.GenerateMessage(message))));
                if(result["content"].IsNullOrEmpty())
                    throw new InvalidOperationException("Request returned a blank message, you may not have permission to send messages yet!");

                DiscordMessage m = new DiscordMessage
                {
                    ID = result["id"].ToString(),
                    Attachments = result["attachments"].ToObject<DiscordAttachment[]>(),
                    Author = channel.parent.Members.Find(x => x.ID == result["author"]["id"].ToString()),
                    channel = channel,
                    TypeOfChannelObject = channel.GetType(),
                    Content = result["content"].ToString(),
                    RawJson = result,
                    timestamp = result["timestamp"].ToObject<DateTime>()
                };
                return m;
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while sending message to channel ({channel.Name}): {ex.Message}", MessageLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Sends a file to the specified DiscordChannel with the given message.
        /// </summary>
        /// <param name="channel">The channel to send the message to.</param>
        /// <param name="message">The message you want the file to have with it.</param>
        /// <param name="pathToFile">The path to the file you wish to send (be careful!)</param>
        public void AttachFile(DiscordChannel channel, string message, string pathToFile)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID}" + Endpoints.Messages;
            //WebWrapper.PostWithAttachment(url, message, pathToFile);
            try
            {
                var uploadResult = JObject.Parse(WebWrapper.HttpUploadFile(url, token, pathToFile, "file", "image/jpeg", null));

                if (!string.IsNullOrEmpty(message))
                    EditMessage(uploadResult["id"].ToString(), message, channel);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while sending file ({pathToFile}) to {channel.Name}: {ex.Message}", MessageLevel.Error);
            }
        }

        /// <summary>
        /// Sends a file to the specified DiscordChannel with the given message.
        /// </summary>
        /// <param name="channel">The channel to send the message to.</param>
        /// <param name="message">The message you want the file to have with it.</param>
        /// <param name="stream">A stream object to send the bytes from.</param>
        public void AttachFile(DiscordChannel channel, string message, System.IO.Stream stream)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID}" + Endpoints.Messages;
            //WebWrapper.PostWithAttachment(url, message, pathToFile);
            try
            {
                var uploadResult = JObject.Parse(WebWrapper.HttpUploadFile(url, token, stream, "file", "image/jpeg", null));

                if (!string.IsNullOrEmpty(message))
                    EditMessage(uploadResult["id"].ToString(), message, channel);
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Error ocurred while sending file by stream to {channel.Name}: {ex.Message}", MessageLevel.Error);
            }
        }

        /// <summary>
        /// Changes the current client's avatar.
        /// Any high resolution pictures are automatically downscaled and Discord will perform jpeg compression on them.
        /// </summary>
        /// <param name="image">The Bitmap object assosciated with the avatar you wish to upload.</param>
        public void ChangeClientAvatar(Bitmap image)
        {
            string base64 = Convert.ToBase64String(Utils.ImageToByteArray(image));
            string type = "image/jpeg;base64";
            string req = $"data:{type},{base64}";
            string usernameRequestJson = JsonConvert.SerializeObject(new
            {
                avatar = req,
                email = ClientPrivateInformation.Email,
                password = ClientPrivateInformation.Password,
                username = ClientPrivateInformation.Username
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

        /// <summary>
        /// Changes the icon assosciated with the guild. Discord will perform jpeg compression and this image is automatically downscaled.
        /// </summary>
        /// <param name="image">The bitmap object associated </param>
        /// <param name="guild">The guild of the icon you wish to change.</param>
        public void ChangeGuildIcon(Bitmap image, DiscordServer guild)
        {
            Bitmap resized = new Bitmap((Image)image, 200, 200);

            string base64 = Convert.ToBase64String(Utils.ImageToByteArray(resized));
            string type = "image/jpeg;base64";
            string req = $"data:{type},{base64}";
            string guildjson = JsonConvert.SerializeObject(new { icon = req, name = guild.Name});
            string url = Endpoints.BaseAPI + Endpoints.Guilds + "/" + guild.ID;
            try
            {
                var result = JObject.Parse(WebWrapper.Patch(url, token, guildjson));
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while changing guild {guild.Name}'s icon: {ex.Message}", MessageLevel.Error);
            }
        }

        /// <summary>
        /// Returns a List of DiscordMessages. 
        /// </summary>
        /// <param name="channel">The channel to return them from.</param>
        /// <param name="count">How many to return</param>
        /// <param name="idBefore">Messages before this message ID.</param>
        /// <param name="idAfter">Messages after this message ID.</param>
        /// <returns>A List of DiscordMessages that you can iterate through.</returns>
        public List<DiscordMessage> GetMessageHistory(DiscordChannel channel, int count, string idBefore = "", string idAfter = "")
        {
            string request = "https://discordapp.com/api/channels/" + channel.ID + $"/messages?&limit={count}";
            if (!string.IsNullOrEmpty(idBefore))
                request += $"&before={idBefore}";
            if (string.IsNullOrEmpty(idAfter))
                request += $"&after={idAfter}";

            JArray result = null;

            try
            {
                string res = WebWrapper.Get(request, token);
                result = JArray.Parse(res);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while getting message history for channel {channel.Name}: {ex.Message}", MessageLevel.Error);
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
                        ID = item["id"].ToString(),
                        channel = channel,
                        Attachments = item["attachments"].ToObject<DiscordAttachment[]>(),
                        TypeOfChannelObject = channel.GetType(),
                        Author = GetMemberFromChannel(channel, item["author"]["id"].ToString()),
                        Content = item["content"].ToString(),
                        RawJson = item.ToObject<JObject>(),
                        timestamp = DateTime.Parse(item["timestamp"].ToString())
                    });
                }
                return messageList;
            }

            return null;
        }

        /// <summary>
        /// Changes the channel topic assosciated with the Discord text channel.
        /// </summary>
        /// <param name="Channeltopic">The new channel topic.</param>
        /// <param name="channel">The channel you wish to change the topic for.</param>
        public void ChangeChannelTopic(string Channeltopic, DiscordChannel channel)
        {
            string topicChangeJson = JsonConvert.SerializeObject(
                new
                {
                    name = channel.Name,
                    topic = Channeltopic
                });
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID}";
            try
            {
                var result = JObject.Parse(WebWrapper.Patch(url, token, topicChangeJson));
                ServersList.Find(x => x.Channels.Find(y => y.ID == channel.ID) != null).Channels.Find(x => x.ID == channel.ID).Topic = Channeltopic;
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while changing channel topic for channel {channel.Name}: {ex.Message}", MessageLevel.Error);
            }
        }

        /*
        public List<DiscordRole> GetRoles(DiscordServer server)
        {
            return null;
        }
        */

        /// <summary>
        /// Used for changing the client's email, password, username, etc.
        /// </summary>
        /// <param name="info"></param>
        public void ChangeClientInformation(DiscordUserInformation info)
        {
            string usernameRequestJson;
            if (info.Password != ClientPrivateInformation.Password)
            {
                usernameRequestJson = JsonConvert.SerializeObject(new
                {
                    email = info.Email,
                    new_password = info.Password,
                    password = ClientPrivateInformation.Password,
                    username = info.Username,
                    avatar = info.Avatar
                });
                ClientPrivateInformation.Password = info.Password;
                try
                {
                    File.Delete("token_cache");
                    DebugLogger.Log("Deleted token_cache due to change of password.");
                }
                catch (Exception) { /*ignore*/ }
            }
            else
            {
                usernameRequestJson = JsonConvert.SerializeObject(new
                {
                    email = info.Email,
                    password = info.Password,
                    username = info.Username,
                    avatar = info.Avatar
                });
            }

            string url = Endpoints.BaseAPI + Endpoints.Users + "/@me";
            try
            {
                var result = JObject.Parse(WebWrapper.Patch(url, token, usernameRequestJson));
                foreach (var server in ServersList)
                {
                    foreach (var member in server.Members)
                    {
                        if (member.ID == Me.ID)
                            member.Username = info.Username;
                    }
                }
                Me.Username = info.Username;
                Me.Email = info.Email;
                Me.Avatar = info.Avatar;
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
                email = ClientPrivateInformation.Email,
                password = ClientPrivateInformation.Password,
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
                        foreach (var member in server.Members)
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

        /// <summary>
        /// Sends a private message to the given user.
        /// </summary>
        /// <param name="message">The message text to send them.</param>
        /// <param name="member">The member you want to send this to.</param>
        /// <returns></returns>
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
                        x => x.Members.Find(
                            y => y.ID == result["recipient"]["id"].ToString()) != null).Members.Find(
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
                d.channel = PrivateChannels.Find(x => x.ID == result["channel_id"].ToString());
                d.TypeOfChannelObject = typeof(DiscordPrivateChannel);
                d.Author = Me;
                return d;
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while sending message to user, step 2: {ex.Message}", MessageLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Gets the string value of the current game your bot is 'playing'.
        /// </summary>
        public string GetCurrentGame => CurrentGameName;

        /// <summary>
        /// Returns true if the websocket is not null and is alive.
        /// </summary>
        public bool WebsocketAlive => (ws != null) ? ws.IsAlive : false;

        #region Message Received Crap..

        /// <summary>
        /// Updates the bot's 'Currently playing' status to the following text. Pass in null if you want to remove this.
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

        /// <summary>
        /// Updates the bot's status.
        /// </summary>
        /// <param name="idle">True if you want the bot to report as idle.</param>
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

            if (!message["d"]["guild_id"].IsNullOrEmpty())
            {
                var server = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
                if(server != null)
                {
                    var user = server.Members.Find(x => x.ID.Trim() == message["d"]["user"]["id"].ToString().Trim());
                    if (user != null)
                    {
                        //If usernames change.
                        if (!message["d"]["user"]["username"].IsNullOrEmpty())
                            user.Username = message["d"]["user"]["username"].ToString();

                        //If avatar changes.
                        if (!message["d"]["user"]["avatar"].IsNullOrEmpty())
                            user.Avatar = message["d"]["user"]["avatar"].ToString();

                        //Actual presence update
                        user.SetPresence(message["d"]["status"].ToString());

                        //Updating games.
                        string game = message["d"]["game"].ToString();
                        if (message["d"]["game"].IsNullOrEmpty())
                        {
                            dpuea.game = "";
                            user.CurrentGame = null;
                        }
                        else
                        {
                            if (message["d"]["game"]["name"].IsNullOrEmpty())
                                dpuea.game = message["d"]["game"]["game"].ToString();
                            else
                                dpuea.game = message["d"]["game"]["name"].ToString();
                            user.CurrentGame = dpuea.game;
                        }
                        dpuea.user = user;

                        if (message["d"]["status"].ToString() == "online")
                            dpuea.status = DiscordUserStatus.ONLINE;
                        else if (message["d"]["status"].ToString() == "idle")
                            dpuea.status = DiscordUserStatus.IDLE;
                        else if (message["d"]["status"].ToString() == null || message["d"]["status"].ToString() == "offline")
                            dpuea.status = DiscordUserStatus.OFFLINE;
                        if (PresenceUpdated != null)
                            PresenceUpdated(this, dpuea);
                    }
                    else
                    {
                        if (!message["d"]["guild_id"].IsNullOrEmpty()) //if this is null or empty, that means this pertains to friends list
                        {
                            if (!message["d"]["user"]["username"].IsNullOrEmpty() && !message["d"]["user"]["id"].IsNullOrEmpty())
                            {
                                DebugLogger.Log($"User {message["d"]["user"]["username"]} ({message["d"]["user"]["id"].ToString()}) doesn't exist in server {server.Name} ({server.ID}) no problemo. Creating/adding", MessageLevel.Debug);
                                DiscordMember memeber = JsonConvert.DeserializeObject<DiscordMember>(message["d"]["user"].ToString());
                                memeber.parentclient = this;
                                memeber.SetPresence(message["d"]["status"].ToString());
                                memeber.Parent = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());

                                if (message["d"]["game"].IsNullOrEmpty())
                                {
                                    dpuea.game = "";
                                    memeber.CurrentGame = null;
                                }
                                else
                                {
                                    dpuea.game = message["d"]["game"]["name"].ToString();
                                    memeber.CurrentGame = dpuea.game;
                                }

                                if (message["d"]["status"].ToString() == "online")
                                    dpuea.status = DiscordUserStatus.ONLINE;
                                else if (message["d"]["status"].ToString() == "idle")
                                    dpuea.status = DiscordUserStatus.IDLE;
                                else if (message["d"]["status"].ToString() == null || message["d"]["status"].ToString() == "offline")
                                    dpuea.status = DiscordUserStatus.OFFLINE;

                                memeber.Parent.Members.Add(memeber);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Deletes a message with a specified ID.
        /// This method will only work if the message was sent since the bot has ran.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteMessage(string id)
        {
            var message = MessageLog.Find(x => x.Value.ID == id);
            if(message.Value != null)
                SendDeleteRequest(message.Value);
        }

        /// <summary>
        /// Deletes a specified DiscordMessage.
        /// </summary>
        /// <param name="message"></param>
        public void DeleteMessage(DiscordMessage message)
        {
            SendDeleteRequest(message);
        }

        //public void DeletePrivateMessage(DiscordMessage message)
        //{
        //    SendDeleteRequest(message, true);
        //}

        /// <summary>
        /// Deletes all messages made by the bot since running.
        /// </summary>
        /// <returns>A count of messages deleted.</returns>
        public int DeleteAllMessages()
        {
            int count = 0;
            MessageLog.ForEach(x =>
            {
                if (x.Value.Author.ID == Me.ID)
                {
                    SendDeleteRequest(x.Value);
                    count++;
                }
            });
            
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
                if (x.channel.ID == channel.ID)
                {
                    SendDeleteRequest(x);
                    __count++;
                }
            });

            return __count;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="username"></param>
        /// <param name="caseSensitive"></param>
        /// <returns></returns>
        public DiscordMember GetMemberFromChannel(DiscordChannel channel, string username, bool caseSensitive)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Argument given for username was null/empty.");
            if(channel != null)
            {
                DiscordMember foundMember = channel.parent.Members.Find(x => caseSensitive ? x.Username == username : x.Username.ToLower() == username.ToLower());
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public DiscordMember GetMemberFromChannel(DiscordChannel channel, string id)
        {
            if (channel != null)
            {
                DiscordMember foundMember = channel.parent.Members.Find(x => x.ID == id);
                if (foundMember != null)
                    return foundMember;
                else
                {
                    DebugLogger.Log($"Error in GetMemberFromChannel: foundMember was null! ID: {id}", MessageLevel.Error);
                }
            }
            else
            {
                DebugLogger.Log("Error in GetMemberFromChannel: channel was null!", MessageLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// you probably shouldn't use this.
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public DiscordChannel GetChannelByName(string channelName)
        {
            try
            {
                return ServersList.Find(x => x.Channels.Find(y => y.Name.ToLower() == channelName.ToLower()) != null).Channels.Find(x => x.Name.ToLower() == channelName.ToLower());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DiscordChannel GetChannelByID(long id)
        {
            return ServersList.Find(x => x.Channels.Find(y => y.ID == id.ToString()) != null).Channels.Find(z => z.ID == id.ToString());
        }

        /// <summary>
        /// (Client account only) accepts an invite to a server.
        /// </summary>
        /// <param name="inviteID">The ID of the invite you want to accept. This is NOT the full URL of the invite</param>
        public void AcceptInvite(string inviteID)
        {
            if (!IsBotAccount)
            {
                if (inviteID.StartsWith("http://"))
                    inviteID = inviteID.Substring(inviteID.LastIndexOf('/') + 1);

                string url = Endpoints.BaseAPI + Endpoints.Invite + $"/{inviteID}";
                try
                {
                    var result = WebWrapper.Post(url, token, "", true);
                    DebugLogger.Log("Accept invite result: " + result.ToString());
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"Error accepting invite: {ex.Message}", MessageLevel.Error);
                }
            }
            else
                throw new InvalidOperationException("Bot accounts can't accept invites normally! Please use the OAuth flow to add bots to servers you have the \"Manage Server\" permission in.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The last DiscordMessage sent</returns>
        public DiscordMessage GetLastMessageSent()
        {
            for(int i = MessageLog.Count - 1; i > -1; i--)
            {
                if (MessageLog[i].Value.Author.ID == Me.ID)
                    return MessageLog[i].Value;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inChannel"></param>
        /// <returns>The last DiscordMessage sent in the given channel</returns>
        public DiscordMessage GetLastMessageSent(DiscordChannel inChannel)
        {
            for (int i = MessageLog.Count - 1; i > -1; i--)
            {
                if (MessageLog[i].Value.Author.ID == Me.ID)
                    if(MessageLog[i].Value.channel.ID == inChannel.ID)
                        return MessageLog[i].Value;
            }
            return null;
        }

        /// <summary>
        /// If you screwed up, you can use this method to edit a given message. This sends out an http patch request with a replacement message
        /// </summary>
        /// <param name="MessageID">The ID of the message you want to edit.</param>
        /// <param name="replacementMessage">What you want the text to be edited to.</param>
        /// <param name="channel">The channel the message is in</param>
        /// <returns>the new and improved DiscordMessage object.</returns>
        public DiscordMessage EditMessage(string MessageID, string replacementMessage, DiscordChannel channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID}" + Endpoints.Messages + $"/{MessageID}";
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
                    Attachments = result["attachments"].ToObject<DiscordAttachment[]>(),
                    Author = channel.parent.Members.Find(x=>x.ID == result["author"]["id"].ToString()),
                    TypeOfChannelObject = channel.GetType(),
                    channel = channel,
                    Content = result["content"].ToString(),
                    ID = result["id"].ToString(),
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
        public void SimulateTyping(DiscordChannelBase channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID}" + Endpoints.Typing;
            try
            {
                WebWrapper.Post(url, token, "", true);
            }
            catch(Exception ex)
            {
                DebugLogger.Log("Exception ocurred while simulating typing: " + ex.Message, MessageLevel.Error);
            }
        }

        private void SendDeleteRequest(DiscordMessage message)
        {
            string url;
            //if(!user)
                url = Endpoints.BaseAPI + Endpoints.Channels + $"/{message.channel.ID}" + Endpoints.Messages + $"/{message.ID}";
            //else
                //url = Endpoints.BaseAPI + Endpoints.Channels + $"/{message.channel.id}" + Endpoints.Messages + $"/{message.id}";
            try
            {
                var result = WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Exception ocurred while deleting message (ID: {message.ID}): " + ex.Message, MessageLevel.Error);
            }
        }

        private void MessageUpdateEvents(JObject message)
        {
            try
            {
                DiscordServer pserver = ServersList.Find(x => x.Channels.Find(y => y.ID == message["d"]["channel_id"].ToString()) != null);
                DiscordChannel pchannel = pserver.Channels.Find(x => x.ID == message["d"]["channel_id"].ToString());
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
                                author = pserver.Members.Find(x => x.ID == message["d"]["author"]["id"].ToString()),
                                Channel = pchannel,
                                message = message["d"]["content"].ToString(),
                                MessageType = DiscordMessageType.CHANNEL,
                                MessageEdited = new DiscordMessage
                                {
                                    Author = pserver.Members.Find(x => x.ID == message["d"]["author"]["id"].ToString()),
                                    Content = MessageLog.Find(x => x.Key == message["d"]["id"].ToString()).Value.Content,
                                    Attachments = message["d"]["attachments"].ToObject<DiscordAttachment[]>(),
                                    channel = pserver.Channels.Find(x => x.ID == message["d"]["channel_id"].ToString()),
                                    RawJson = message,
                                    ID = message["d"]["id"].ToString(),
                                    timestamp = message["d"]["timestamp"].ToObject<DateTime>(),
                                },
                                EditedTimestamp = message["d"]["edited_timestamp"].ToObject<DateTime>()
                            });
                        int indexOfMessageToChange = MessageLog.IndexOf(toRemove);
                        MessageLog.Remove(toRemove);
                        DiscordMessage newMessage = toRemove.Value;
                        newMessage.Content = jsonToEdit["d"]["content"].ToString();
                        MessageLog.Insert(indexOfMessageToChange, new KeyValuePair<string, DiscordMessage>(jsonToEdit["d"]["id"].ToString(), newMessage));

                    }
                    else //I know they say assume makes an ass out of you and me...but we're assuming it's Discord's weird auto edit of a just URL message
                    {
                        if (URLMessageAutoUpdate != null)
                        {
                            DiscordURLUpdateEventArgs asdf = new DiscordURLUpdateEventArgs(); //I'm running out of clever names and should probably split these off into different internal voids soon...
                            asdf.id = message["d"]["id"].ToString();
                            asdf.channel = ServersList.Find(x => x.Channels.Find(y => y.ID == message["d"]["channel_id"].ToString()) != null).Channels.Find(x => x.ID == message["d"]["channel_id"].ToString());
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

        private DiscordChannel GetDiscordChannelByID(string id)
        {
            DiscordChannel returnVal = new DiscordChannel { ID = "-1" };
            ServersList.ForEach(x =>
            {
                x.Channels.ForEach(y =>
                {
                    if (y.ID == id)
                        returnVal = y;
                });
            });
            if (returnVal.ID != "-1")
                return returnVal;
            else
                return null;
        }

        private void MessageCreateEvents(JObject message)
        {
            //try
            //{
            string tempChannelID = message["d"]["channel_id"].ToString();

            //DiscordServer foundServerChannel = ServersList.Find(x => x.channels.Find(y => y.id == tempChannelID) != null);
            DiscordChannel potentialChannel = GetDiscordChannelByID(message["d"]["channel_id"].ToString());
            if (potentialChannel == null) //private message create
            {
                if (message["d"]["author"]["id"].ToString() != Me.ID)
                {
                    var foundPM = PrivateChannels.Find(x => x.ID == message["d"]["channel_id"].ToString());
                    DiscordPrivateMessageEventArgs dpmea = new DiscordPrivateMessageEventArgs();
                    dpmea.Channel = foundPM;
                    dpmea.message = message["d"]["content"].ToString();
                    DiscordMember tempMember = new DiscordMember(this);
                    tempMember.Username = message["d"]["author"]["username"].ToString();
                    tempMember.ID = message["d"]["author"]["id"].ToString();
                    dpmea.author = tempMember;
                    tempMember.parentclient = this;
                    dpmea.RawJson = message;

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
                dmea.RawJson = message;
                dmea.Channel = potentialChannel;

                dmea.message_text = message["d"]["content"].ToString();

                DiscordMember tempMember = null;
                tempMember = potentialChannel.parent.Members.Find(x => x.ID == message["d"]["author"]["id"].ToString());
                if (tempMember == null)
                {
                    tempMember = JsonConvert.DeserializeObject<DiscordMember>(message["author"].ToString());
                    tempMember.parentclient = this;
                    tempMember.Parent = potentialChannel.parent;

                    potentialChannel.parent.Members.Add(tempMember);
                }

                dmea.author = tempMember;

                DiscordMessage m = new DiscordMessage();
                m.Author = dmea.author;
                m.channel = dmea.Channel;
                m.TypeOfChannelObject = dmea.Channel.GetType();
                m.Content = dmea.message_text;
                m.ID = message["d"]["id"].ToString();
                m.RawJson = message;
                m.timestamp = DateTime.Now;
                dmea.message = m;
                if (!message["d"]["attachments"].IsNullOrEmpty())
                {
                    List<DiscordAttachment> tempList = new List<DiscordAttachment>();
                    foreach (var attachment in message["d"]["attachments"])
                    {
                        tempList.Add(JsonConvert.DeserializeObject<DiscordAttachment>(attachment.ToString()));
                    }
                    m.Attachments = tempList.ToArray();
                }

                if (!message["d"]["mentions"].IsNullOrEmpty())
                {
                    JArray mentionsAsArray = JArray.Parse(message["d"]["mentions"].ToString());
                    foreach (var mention in mentionsAsArray)
                    {
                        string id = mention["id"].ToString();
                        if (id.Equals(Me.ID))
                        {
                            if (MentionReceived != null)
                                MentionReceived(this, dmea);
                        }
                    }
                }

                KeyValuePair<string, DiscordMessage> toAdd = new KeyValuePair<string, DiscordMessage>(message["d"]["id"].ToString(), m);
                MessageLog.Add(toAdd);

                if (MessageReceived != null)
                    MessageReceived(this, dmea);
            }
            //}
            //catch (Exception ex)
            //{
            //    DebugLogger.Log("Error ocurred during MessageCreateEvents: " + ex.Message, MessageLevel.Error);
            //}
        }

        private void ChannelCreateEvents (JObject message)
        {
            if (message["d"]["is_private"].ToString().ToLower() == "false")
            {
                var foundServer = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
                if (foundServer != null)
                {
                    DiscordChannel tempChannel = new DiscordChannel();
                    tempChannel.Name = message["d"]["name"].ToString();
                    tempChannel.Type = message["d"]["type"].ToObject<ChannelType>();
                    if (tempChannel.Type == ChannelType.Voice && !message["d"]["bitrate"].IsNullOrEmpty())
                        tempChannel.Bitrate = message["d"]["bitrate"].ToObject<int>();

                    tempChannel.ID = message["d"]["id"].ToString();
                    tempChannel.parent = foundServer;
                    foundServer.Channels.Add(tempChannel);
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
                tempPrivate.ID = message["d"]["id"].ToString();
                DiscordMember recipient = ServersList.Find(x => x.Members.Find(y => y.ID == message["d"]["recipient"]["id"].ToString()) != null).Members.Find(x => x.ID == message["d"]["recipient"]["id"].ToString());
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
            if (token == null)
                throw new NullReferenceException("token was null!");

        //i'm ashamed of myself for this but i'm tired
        tryAgain:
            string url = Endpoints.BaseAPI + Endpoints.Gateway;
            try
            {
                string gateway = JObject.Parse(WebWrapper.Get(url, token))["url"].ToString();
                if(!string.IsNullOrEmpty(gateway))
                    return gateway;
                else
                    throw new NullReferenceException("Failed to retrieve Gateway urL!");
            }
            catch(UnauthorizedAccessException) //bad token
            {
                DebugLogger.Log("Got 401 from Discord. Token bad, deleting and retrying login...");
                if(File.Exists(StrippedEmail.GetHashCode() + ".cache"))
                {
                    File.Delete(StrippedEmail.GetHashCode() + ".cache");
                }
                SendLoginRequest();
                goto tryAgain;
            }
            catch(Exception ex)
            {
                DebugLogger.Log("Exception ocurred while retrieving Gateway URL: " + ex.Message, MessageLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public DiscordServer GetServerChannelIsIn(DiscordChannel channel)
        {
            return ServersList.Find(x => x.Channels.Find(y => y.ID == channel.ID) != null);
        }

        /// <summary>
        /// Deletes a specified Discord channel given you have the permission.
        /// </summary>
        /// <param name="channel">The DiscordChannel object to delete</param>
        public void DeleteChannel(DiscordChannel channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log("Exception ocurred while deleting channel: " + ex.Message, MessageLevel.Error);
            }
        }

        /// <summary>
        /// Creates either a text or voice channel in a DiscordServer given a name. Given you have the permission of course.
        /// </summary>
        /// <param name="server">The server to create the channel in.</param>
        /// <param name="ChannelName">The name of the channel (will automatically be lowercased if text)</param>
        /// <param name="voice">True if you want the channel to be a voice channel.</param>
        /// <returns>The newly created DiscordChannel</returns>
        public DiscordChannel CreateChannel(DiscordServer server, string ChannelName, bool voice)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{server.ID}" + Endpoints.Channels;
            var reqJson = JsonConvert.SerializeObject(new { name = ChannelName, type = voice ? "voice" : "text" });
            try
            {
                var result = JObject.Parse(WebWrapper.Post(url, token, reqJson));
                if (result != null)
                {
                    DiscordChannel dc = new DiscordChannel
                    {
                        Name = result["name"].ToString(),
                        ID = result["id"].ToString(),
                        Type = result["type"].ToObject<ChannelType>(),
                        Private = result["is_private"].ToObject<bool>(),
                        
                    };
                    if (!result["topic"].IsNullOrEmpty())
                        dc.Topic = result["topic"].ToString();
                    if (dc.Type == ChannelType.Voice && !result["bitrate"].IsNullOrEmpty())
                        dc.Bitrate = result["bitrate"].ToObject<int>();

                    server.Channels.Add(dc);
                    return dc;
                }
            }
            catch(Exception ex)
            {
                DebugLogger.Log("Exception ocurred while creating channel: " + ex.Message, MessageLevel.Error);
            }
            return null;
        }
        
        /// <summary>
        /// Creates an empty guild with only this client in it given the following name.
        /// Unknown if works on bot accounts or not.
        /// </summary>
        /// <param name="GuildName">The name of the guild you wish to create.</param>
        /// <returns>the created DiscordServer</returns>
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
                    server.JoinedAt = response["joined_at"].ToObject<DateTime>();
                    server.ID = response["id"].ToString();
                    server.Name = response["name"].ToString();
                    server.parentclient = this;

                    string channelGuildUrl = createGuildUrl + $"/{server.ID}" + Endpoints.Channels;
                    var channelRespone = JArray.Parse(WebWrapper.Get(channelGuildUrl, token));
                    foreach (var item in channelRespone.Children())
                    {
                        server.Channels.Add(new DiscordChannel
                        {
                            Name = item["name"].ToString(),
                            ID = item["id"].ToString(),
                            Topic = item["topic"].ToString(),
                            Private = item["is_private"].ToObject<bool>(),
                            Type = item["type"].ToObject<ChannelType>() });
                    }

                    server.Members.Add(Me);
                    server.Owner = server.Members.Find(x => x.ID == response["owner_id"].ToString());
                    if (server.Owner == null)
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

        /// <summary>
        /// Edits the name of the guild, given you have the permission.
        /// </summary>
        /// <param name="guild">The guild's name you wish to edit.</param>
        /// <param name="NewGuildName">The new guild name.</param>
        public void EditGuildName(DiscordServer guild, string NewGuildName)
        {
            string editGuildUrl = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.ID}";
            var newNameJson = JsonConvert.SerializeObject(new { name = NewGuildName });
            try
            {
                WebWrapper.Patch(editGuildUrl, token, newNameJson);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Exception ocurred while editing guild ({guild.Name}) name: " + ex.Message, MessageLevel.Error);
            }
        }

        /// <summary>
        /// Assigns a specified role to a member, given you have the permission.
        /// </summary>
        /// <param name="guild">The guild you and the user are in.</param>
        /// <param name="role">The role you wish to assign them.</param>
        /// <param name="member">The member you wish to assign the role to.</param>
        public void AssignRoleToMember(DiscordServer guild, DiscordRole role, DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.ID}" + Endpoints.Members + $"/{member.ID}";
            string message = JsonConvert.SerializeObject(new { roles = new string[] { role.ID } });
            try
            {
                WebWrapper.Patch(url, token, message);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Exception ocurred while assigning role ({role.Name}) to member ({member.Username}): " 
                    + ex.Message, MessageLevel.Error);
            }
        }

        /// <summary>
        /// Assigns the specified roles to a member, given you have the permission.
        /// </summary>
        /// <param name="guild">The guild you and the user are in.</param>
        /// <param name="roles">The roles you wish to assign them.</param>
        /// <param name="member">The member you wish to assign the role to.</param>
        public void AssignRoleToMember(DiscordServer guild, List<DiscordRole> roles, DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.ID}" + Endpoints.Members + $"/{member.ID}";
            List<string> rolesAsIds = new List<string>();
            roles.ForEach(x => rolesAsIds.Add(x.ID));
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
        /// <returns>The invite's id.</returns>
        public string CreateInvite(DiscordChannel channel)
        {
            string url = Endpoints.BaseAPI + Endpoints.Channels + $"/{channel.ID}" + Endpoints.Invites;
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
                DebugLogger.Log($"Error ocurred while creating invite for channel {channel.Name}: {ex.Message}", MessageLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Deletes an invite by id
        /// </summary>
        /// <param name="id">The ID of the invite you wish to delete.</param>
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

        /// <summary>
        /// Just prepends https://discord.gg/ to a given invite :)
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A full invite URL.</returns>
        public string MakeInviteURLFromCode(string id) => "https://discord.gg/" + id;

        /// <summary>
        /// This method temporarily points to the .Connect(); method.
        /// Eventually, this will be removed entirely. Switch while you can!
        /// </summary>
        [Obsolete]
        public void ConnectAndReadMessages() => Connect();

        /// <summary>
        /// Runs the websocket connection for the client hooking up the appropriate events.
        /// </summary>
        public void Connect()
        {
            CurrentGatewayURL = GetGatewayUrl();
            if(String.IsNullOrEmpty(CurrentGatewayURL))
            {
                DebugLogger.Log("Gateway URL was null or empty?!", MessageLevel.Critical);
                return;
            }
            DebugLogger.Log("Gateway retrieved: " + CurrentGatewayURL);
            ws = new WebSocket(CurrentGatewayURL);
            ws.EnableRedirection = true;
            ws.Log.File = "websocketlog.txt";
                ws.OnMessage += (sender, e) =>
                {
                    var message = JObject.Parse(e.Data);
                    if(message["t"].ToString() != "READY")
                        DebugLogger.Log(message.ToString(), MessageLevel.Unecessary);
                    switch(message["t"].ToString())
                    {
                        case ("READY"):
                            if(WriteLatestReady)
                                using (var sw = new StreamWriter("READY_LATEST.txt"))
                                    sw.Write(message);
                            Me = JsonConvert.DeserializeObject<DiscordMember>(message["d"]["user"].ToString());
                            Me.parentclient = this;
                            ClientPrivateInformation.Avatar = Me.Avatar;
                            ClientPrivateInformation.Username = Me.Username;
                            HeartbeatInterval = int.Parse(message["d"]["heartbeat_interval"].ToString());
                            GetChannelsList(message);

                            //TESTING
                            string[] guildID = new string[ServersList.Count];
                            for (int i = 0; i < guildID.Length; i++)
                                guildID[i] = ServersList[i].ID;

                            if (RequestAllUsersOnStartup)
                            {
                                string wsChunkTest = JsonConvert.SerializeObject(new
                                {
                                    op = 8,
                                    d = new
                                    {
                                        guild_id = guildID,
                                        query = "",
                                        limit = 0
                                    }
                                });
                                ws.Send(wsChunkTest);
                            }

                            if (Connected != null)
                                Connected(this, new DiscordConnectEventArgs { user = Me });
                            break;
                        case ("GUILD_MEMBERS_CHUNK"):
                            GuildMemberChunkEvents(message);
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
                            DiscordServer server = ServersList.Find(x => x.Channels.Find(y => y.ID == message["d"]["channel_id"].ToString()) != null);
                            if (server != null)
                            {
                                DiscordChannel channel = server.Channels.Find(x => x.ID == message["d"]["channel_id"].ToString());
                                DiscordMember uuser = server.Members.Find(x => x.ID == message["d"]["user_id"].ToString());
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
                        v = 3,
                        d = new
                        {
                            token = token,
                            large_threshold = true,
                            properties = DiscordProperties
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
                                break;
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

        private bool GuildHasMemberWithID(DiscordServer guild, string id)
        {
            bool has = false;
            foreach (var x in guild.Members)
                if (x.ID == id)
                    has = true;

            return has;
        }

        private void GuildMemberChunkEvents(JObject message)
        {
            if(!message["d"]["members"].IsNullOrEmpty())
            {
                DiscordServer inServer = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
                JArray membersAsArray = JArray.Parse(message["d"]["members"].ToString());
                foreach (var member in membersAsArray)
                {
                    if (GuildHasMemberWithID(inServer, member["user"]["id"].ToString()))
                        continue;
                    DiscordMember _member = JsonConvert.DeserializeObject<DiscordMember>(member["user"].ToString());
                    if (!member["user"]["roles"].IsNullOrEmpty())
                    {
                        JArray rollsArray = JArray.Parse(member["user"]["roles"].ToString());
                        if (rollsArray.Count > 0)
                        {
                            foreach (var rollID in rollsArray)
                                _member.Roles.Add(inServer.Roles.Find(x => x.ID == rollID.ToString()));
                        }
                    }
                    _member.Muted = member["mute"].ToObject<bool>();
                    _member.Deaf = member["deaf"].ToObject<bool>();
                    _member.Roles.Add(inServer.Roles.Find(x => x.Name == "@everyone"));
                    _member.Status = Status.Offline;
                    _member.parentclient = this;
                    _member.Parent = inServer;
                    inServer.Members.Add(_member);

                    ///Check private channels
                    DiscordPrivateChannel _channel = PrivateChannels.Find(x => x.user_id == _member.ID);
                    if(_channel != null)
                    {
                        DebugLogger.Log("Found user for private channel!", MessageLevel.Debug);
                        _channel.recipient = _member;
                    }
                }
            }
        }

        private void GuildMemberBanRemovedEvents(JObject message)
        {
            DiscordBanRemovedEventArgs e = new DiscordBanRemovedEventArgs();

            e.Guild = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
            e.MemberStub = JsonConvert.DeserializeObject<DiscordMember>(message["d"]["user"].ToString());

            if (BanRemoved != null)
                BanRemoved(this, e);
        }

        private void GuildMemberBannedEvents(JObject message)
        {
            DiscordGuildBanEventArgs e = new DiscordGuildBanEventArgs();
            e.Server = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
            if(e.Server != null)
            {
                e.MemberBanned = e.Server.Members.Find(x => x.ID == message["d"]["user"]["id"].ToString());
                if(e.MemberBanned != null)
                {
                    if (GuildMemberBanned != null)
                        GuildMemberBanned(this, e);
                    ServersList.Find(x => x.ID == e.Server.ID).Members.Remove(e.MemberBanned);
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

        private void VoiceServerUpdateEvents(JObject message)
        {
            VoiceClient.VoiceEndpoint = message["d"]["endpoint"].ToString();
            VoiceClient.Token = message["d"]["token"].ToString();
            
            VoiceClient.Guild = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
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

            ConnectToVoiceAsync();
        }

#if NETFX4_5
        private Task ConnectToVoiceAsync()
        {
            VoiceClient.InitializeOpusEncoder();
            return Task.Run(() => VoiceClient.Initiate());
        }
#else
        private Task ConnectToVoiceAsync()
        {
            VoiceClient.InitializeOpusEncoder();
            return Task.Factory.StartNew(() => VoiceClient.Initiate());
        }
#endif

        /// <summary>
        /// Kicks a specified DiscordMember from the guild that's assumed from their 
        /// parent property.
        /// </summary>
        /// <param name="member"></param>
        public void KickMember(DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{member.Parent.ID}" + Endpoints.Members + $"/{member.ID}";
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
        public DiscordMember BanMember(DiscordMember member, int days = 0)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{member.Parent.ID}" + Endpoints.Bans + $"/{member.ID}";
            url += $"?delete-message-days={days}";
            try
            {
                WebWrapper.Put(url, token);
                return member;
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Error during BanMember\n\t{ex.Message}\n\t{ex.StackTrace}", MessageLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Bans a specified DiscordMember from the guild that's assumed from their
        /// parent property.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="serverOverride"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public DiscordMember BanMember(DiscordMember member, DiscordServer serverOverride, int days = 0)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{serverOverride.ID}" + Endpoints.Bans + $"/{member.ID}";
            url += $"?delete-message-days={days}";
            try
            {
                WebWrapper.Put(url, token);
                return member;
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"Error during BanMember\n\t{ex.Message}\n\t{ex.StackTrace}", MessageLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Retrieves a DiscordMember List of members banned in the specified server.
        /// </summary>
        /// <param name="server"></param>
        /// <returns>Null if no permission.</returns>
        public List<DiscordMember> GetBans(DiscordServer server)
        {
            List<DiscordMember> returnVal = new List<DiscordMember>();
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{server.ID}" + Endpoints.Bans;
            try
            {
                var __res = WebWrapper.Get(url, token);
                var permissionCheck = JObject.Parse(__res);
                {
                    if (!permissionCheck["message"].IsNullOrEmpty())
                        return null; //no permission
                }
                JArray response = JArray.Parse(__res);
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
                DebugLogger.Log($"An error ocurred while retrieving bans for server \"{server.Name}\"\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", 
                    MessageLevel.Error);
            }
            return returnVal;
        }

        /// <summary>
        /// Removes a ban on the user.
        /// </summary>
        /// <param name="guild">The guild to lift the ban from.</param>
        /// <param name="userID">The ID of the user to lift the ban.</param>
        public void RemoveBan(DiscordServer guild, string userID)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.ID}" + Endpoints.Bans + $"/{userID}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error during RemoveBan\n\tMessage: {ex.Message}\n\tStack: {ex.StackTrace}", MessageLevel.Error);
            }
        }

        /// <summary>
        /// Removes a ban on the user.
        /// </summary>
        /// <param name="guild">The guild to lift the ban from.</param>
        /// <param name="member">The DiscordMember object of the user to lift the ban from, assuming you have it.</param>
        public void RemoveBan(DiscordServer guild, DiscordMember member)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.ID}" + Endpoints.Bans + $"/{member.ID}";
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

        /// <summary>
        /// Connects to a given voice channel.
        /// </summary>
        /// <param name="channel">The channel to connect to. </param>
        /// <param name="voiceConfig">The voice configuration to use. If null, default values will be used.</param>
        /// <param name="clientMuted">Whether or not the client will connect muted. Defaults to false.</param>
        /// <param name="clientDeaf">Whether or not the client will connect deaf. Defaults to false.</param>
        public void ConnectToVoiceChannel(DiscordChannel channel, DiscordVoiceConfig voiceConfig = null, bool clientMuted = false, bool clientDeaf = false)
        {
            if (channel.Type != ChannelType.Voice)
                throw new InvalidOperationException($"Channel '{channel.ID}' is not a voice channel!");

            if (ConnectedToVoice())
                DisconnectFromVoice();

            if (VoiceClient == null)
            {
                if (voiceConfig == null)
                {
                    VoiceClient = new DiscordVoiceClient(this, new DiscordVoiceConfig());
                }
                else
                    VoiceClient = new DiscordVoiceClient(this, voiceConfig);
            }
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
            VoiceClient.VoiceConnectionComplete += (sender, e) =>
            {
                if (VoiceClientConnected != null)
                    VoiceClientConnected(this, e);
            };
            VoiceClient.QueueEmpty += (sender, e) =>
            {
                if (VoiceQueueEmpty != null)
                    VoiceQueueEmpty(this, e);
            };

            string joinVoicePayload = JsonConvert.SerializeObject(new
            {
                op = 4,
                d = new
                {
                    guild_id = channel.parent.ID,
                    channel_id = channel.ID,
                    self_mute = clientMuted,
                    self_deaf = clientDeaf
                }
            });

            ws.Send(joinVoicePayload);
        }

        /// <summary>
        /// Clears the internal message log cache
        /// </summary>
        /// <returns>The number of internal messages cleared.</returns>
        public int ClearInternalMessageLog()
        {
            int totalCount = MessageLog.Count;
            MessageLog.Clear();
            return totalCount;
        }

        /// <summary>
        /// Iterates through a server's members and removes offline users.
        /// </summary>
        /// <param name="server"></param>
        /// <returns>The amount of users cleared.</returns>
        public int ClearOfflineUsersFromServer(DiscordServer server)
        {
            return server.Members.RemoveAll(x => x.Status == Status.Offline);
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
                    guild_id = VoiceClient != null && VoiceClient.Channel != null ? VoiceClient.Channel.parent.ID : (object)null,
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
                {}
            }
            if (ws != null)
                ws.Send(disconnectMessage);
            VoiceClient = null;
            DebugLogger.Log($"Disconnected from voice. VoiceClient null: {VoiceClient == null}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The current VoiceClient or null.</returns>
        public DiscordVoiceClient GetVoiceClient()
        {
            if (ConnectedToVoice() && VoiceClient != null)
                return VoiceClient;

            return null;
        }

        private void GuildMemberUpdateEvents(JObject message)
        {
            DiscordServer server = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());

            DiscordMember memberUpdated = server.Members.Find(x => x.ID == message["d"]["user"]["id"].ToString());
            if (memberUpdated != null)
            {
                memberUpdated.Username = message["d"]["user"]["username"].ToString();
                if (!message["d"]["user"]["avatar"].IsNullOrEmpty())
                    memberUpdated.Avatar = message["d"]["user"]["avatar"].ToString();
                memberUpdated.Discriminator = message["d"]["user"]["discriminator"].ToString();
                memberUpdated.ID = message["d"]["user"]["id"].ToString();


                foreach (var roles in message["d"]["roles"])
                {
                    memberUpdated.Roles.Add(server.Roles.Find(x => x.ID == roles.ToString()));
                }

                server.Members.Remove(server.Members.Find(x => x.ID == message["d"]["user"]["id"].ToString()));
                server.Members.Add(memberUpdated);

                if (GuildMemberUpdated != null)
                    GuildMemberUpdated(this, new DiscordGuildMemberUpdateEventArgs { MemberUpdate = memberUpdated, RawJson = message, ServerUpdated = server });
            }
            else
            {
                DebugLogger.Log("memberUpdated was null?!?!?!", MessageLevel.Critical);
            }
        }

        private void GuildRoleUpdateEvents(JObject message)
        {
            DiscordServer inServer = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
            DiscordRole roleUpdated = new DiscordRole
            {
                Name = message["d"]["role"]["name"].ToString(),
                Position = message["d"]["role"]["position"].ToObject<int>(),
                Permissions = new DiscordPermission(message["d"]["role"]["permissions"].ToObject<uint>()),
                Managed = message["d"]["role"]["managed"].ToObject<bool>(),
                Hoist = message["d"]["role"]["hoist"].ToObject<bool>(),
                Color = new Color(message["d"]["role"]["color"].ToObject<int>().ToString("x")),
                ID = message["d"]["role"]["id"].ToString(),
            };

            ServersList.Find(x => x.ID == inServer.ID).Roles.Remove(ServersList.Find(x => x.ID == inServer.ID).Roles.Find(y => y.ID == roleUpdated.ID));
            ServersList.Find(x => x.ID == inServer.ID).Roles.Add(roleUpdated);

            if (RoleUpdated != null)
                RoleUpdated(this, new DiscordGuildRoleUpdateEventArgs { RawJson = message, RoleUpdated = roleUpdated, InServer = inServer });
        }

        private void GuildRoleDeleteEvents(JObject message)
        {
            DiscordServer inServer = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
            DiscordRole deletedRole = inServer.Roles.Find(x => x.ID == message["d"]["role_id"].ToString());

            try
            {
                ServersList.Find(x => x.ID == inServer.ID).Roles.Remove(ServersList.Find(x => x.ID == inServer.ID).Roles.Find(y => y.ID == deletedRole.ID));
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Couldn't delete role with ID {message["d"]["role_id"].ToString()}! ({ex.Message})", MessageLevel.Critical);
            }

            if (RoleDeleted != null)
                RoleDeleted(this, new DiscordGuildRoleDeleteEventArgs { DeletedRole = deletedRole, Guild = inServer, RawJson = message });
        }

        /// <summary>
        /// Creates a default role in the specified guild.
        /// </summary>
        /// <param name="guild">The guild to make the role in.</param>
        /// <returns>The newly created role.</returns>
        public DiscordRole CreateRole(DiscordServer guild)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.ID}" + Endpoints.Roles;

            try
            {
                var result = JObject.Parse(WebWrapper.Post(url, token, ""));

                if (result != null)
                {
                    DiscordRole d = new DiscordRole
                    {
                        Color = new Color(result["color"].ToObject<int>().ToString("x")),
                        Hoist = result["hoist"].ToObject<bool>(),
                        ID = result["id"].ToString(),
                        Managed = result["managed"].ToObject<bool>(),
                        Name = result["name"].ToString(),
                        Permissions = new DiscordPermission(result["permissions"].ToObject<uint>()),
                        Position = result["position"].ToObject<int>()
                    };

                    ServersList.Find(x => x.ID == guild.ID).Roles.Add(d);
                    return d;
                }
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while creating role in guild {guild.Name}: {ex.Message}", MessageLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Edits a role with the new role information.
        /// </summary>
        /// <param name="guild">The guild the role is in.</param>
        /// <param name="newRole">the new role.</param>
        /// <returns>The newly edited role returned from Discord.</returns>
        public DiscordRole EditRole(DiscordServer guild, DiscordRole newRole)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.ID}" + Endpoints.Roles + $"/{newRole.ID}";
            string request = JsonConvert.SerializeObject(
                new
                {
                    color = decimal.Parse(newRole.Color.ToDecimal().ToString()),
                    hoist = newRole.Hoist,
                    name = newRole.Name,
                    permissions = newRole.Permissions.GetRawPermissions()
                }
            );

            try
            {
                var result = JObject.Parse(WebWrapper.Patch(url, token, request));
                if (result != null)
                {
                    DiscordRole d = new DiscordRole
                    {
                        Color = new Color(result["color"].ToObject<int>().ToString("x")),
                        Hoist = result["hoist"].ToObject<bool>(),
                        ID = result["id"].ToString(),
                        Managed = result["managed"].ToObject<bool>(),
                        Name = result["name"].ToString(),
                        Permissions = new DiscordPermission(result["permissions"].ToObject<uint>()),
                        Position = result["position"].ToObject<int>()
                    };

                    ServersList.Find(x => x.ID == guild.ID).Roles.Remove(d);
                    ServersList.Find(x => x.ID == guild.ID).Roles.Add(d);
                    return d;
                }
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while editing role ({newRole.Name}): {ex.Message}", MessageLevel.Error);
            }

            return null;
        }

        /// <summary>
        /// Deletes a specified role.
        /// </summary>
        /// <param name="guild">The guild the role is in.</param>
        /// <param name="role">The role to delete.</param>
        public void DeleteRole(DiscordServer guild, DiscordRole role)
        {
            string url = Endpoints.BaseAPI + Endpoints.Guilds + $"/{guild.ID}" + Endpoints.Roles + $"/{role.ID}";
            try
            {
                WebWrapper.Delete(url, token);
            }
            catch(Exception ex)
            {
                DebugLogger.Log($"Error ocurred while deleting role ({role.Name}): {ex.Message}", MessageLevel.Error);
            }
        }

        private void GuildUpdateEvents(JObject message)
        {
            DiscordServer oldServer = ServersList.Find(x => x.ID == message["d"]["id"].ToString());
            DiscordServer newServer = oldServer.ShallowCopy();

            newServer.Name = message["d"]["name"].ToString();
            newServer.ID = message["d"]["id"].ToString();
            newServer.parentclient = this;
            newServer.Roles = new List<DiscordRole>();
            newServer.Region = message["d"]["region"].ToString();
            if(!message["d"]["icon"].IsNullOrEmpty())
            {
                newServer.icon = message["d"]["icon"].ToString();
            }
            if (!message["d"]["roles"].IsNullOrEmpty())
            {
                foreach (var roll in message["d"]["roles"])
                {
                    DiscordRole t = new DiscordRole
                    {
                        Color = new DiscordSharp.Color(roll["color"].ToObject<int>().ToString("x")),
                        Name = roll["name"].ToString(),
                        Permissions = new DiscordPermission(roll["permissions"].ToObject<uint>()),
                        Position = roll["position"].ToObject<int>(),
                        Managed = roll["managed"].ToObject<bool>(),
                        ID = roll["id"].ToString(),
                        Hoist = roll["hoist"].ToObject<bool>()
                    };
                    newServer.Roles.Add(t);
                }
            }
            else
            {
                newServer.Roles = oldServer.Roles;
            }
            newServer.Channels = new List<DiscordChannel>();
            if (!message["d"]["channels"].IsNullOrEmpty())
            {
                foreach (var u in message["d"]["channels"])
                {
                    DiscordChannel tempSub = new DiscordChannel();
                    tempSub.ID = u["id"].ToString();
                    tempSub.Name = u["name"].ToString();
                    tempSub.Type = u["type"].ToObject<ChannelType>();

                    if(!u["topic"].IsNullOrEmpty())
                        tempSub.Topic = u["topic"].ToString();
                    if (tempSub.Type == ChannelType.Voice && !u["bitrate"].IsNullOrEmpty())
                        tempSub.Bitrate = u["bitrate"].ToObject<int>();

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

                    newServer.Channels.Add(tempSub);
                }
            }
            else
            {
                newServer.Channels = oldServer.Channels;
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
                            member.Roles.Add(newServer.Roles.Find(x => x.ID == role.Value<string>()));
                        }
                    }
                    else
                    {
                        member.Roles.Add(newServer.Roles.Find(x => x.Name == "@everyone"));
                    }

                    newServer.Members.Add(member);
                }
            }
            else
            {
                newServer.Members = oldServer.Members;
            }
            if (!message["d"]["owner_id"].IsNullOrEmpty())
            {
                newServer.Owner = newServer.Members.Find(x => x.ID == message["d"]["owner_id"].ToString());
                DebugLogger.Log($"Transferred ownership from user '{oldServer.Owner.Username}' to {newServer.Owner.Username}.");
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
                e.PrivateChannelDeleted = PrivateChannels.Find(x => x.ID == message["d"]["id"].ToString());
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
                server.Channels.Remove(server.Channels.Find(x => x.ID == e.ChannelDeleted.ID));

                if (ChannelDeleted != null)
                    ChannelDeleted(this, e);
            }
        }

        private void ChannelUpdateEvents(JObject message)
        {
            DiscordChannelUpdateEventArgs e = new DiscordChannelUpdateEventArgs();
            e.RawJson = message;
            DiscordChannel oldChannel = ServersList.Find(x => x.Channels.Find(y => y.ID == message["d"]["id"].ToString()) != null).Channels.Find(x=>x.ID == message["d"]["id"].ToString());
            e.OldChannel = oldChannel.ShallowCopy();
            DiscordChannel newChannel = oldChannel;
            newChannel.Name = message["d"]["name"].ToString();
            if (!message["d"]["topic"].IsNullOrEmpty())
                newChannel.Topic = message["d"]["topic"].ToString();
            else
                newChannel.Topic = oldChannel.Topic;

            newChannel.Private = message["d"]["is_private"].ToObject<bool>();

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

            DiscordServer serverToRemoveFrom = ServersList.Find(x => x.Channels.Find(y => y.ID == newChannel.ID) != null);
            newChannel.parent = serverToRemoveFrom;
            int indexOfServer = ServersList.IndexOf(serverToRemoveFrom);
            serverToRemoveFrom.Channels.Remove(oldChannel);
            serverToRemoveFrom.Channels.Add(newChannel);

            ServersList.RemoveAt(indexOfServer);
            ServersList.Insert(indexOfServer, serverToRemoveFrom);

            if (ChannelUpdated != null)
                ChannelUpdated(this, e);
        }

        private void GuildDeleteEvents(JObject message)
        {
            DiscordGuildDeleteEventArgs e = new DiscordGuildDeleteEventArgs();
            e.server = ServersList.Find(x => x.ID == message["d"]["id"].ToString());
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
            server.JoinedAt = message["d"]["joined_at"].ToObject<DateTime>();
            server.parentclient = this;
            server.ID = message["d"]["id"].ToString();
            server.Name = message["d"]["name"].ToString();
            server.Members = new List<DiscordMember>();
            server.Channels = new List<DiscordChannel>();
            server.Roles = new List<DiscordRole>();
            foreach (var roll in message["d"]["roles"])
            {
                DiscordRole t = new DiscordRole
                {
                    Color = new DiscordSharp.Color(roll["color"].ToObject<int>().ToString("x")),
                    Name = roll["name"].ToString(),
                    Permissions = new DiscordPermission(roll["permissions"].ToObject<uint>()),
                    Position = roll["position"].ToObject<int>(),
                    Managed = roll["managed"].ToObject<bool>(),
                    ID = roll["id"].ToString(),
                    Hoist = roll["hoist"].ToObject<bool>()
                };
                server.Roles.Add(t);
            }
            foreach (var chn in message["d"]["channels"])
            {
                DiscordChannel tempChannel = new DiscordChannel();
                tempChannel.ID = chn["id"].ToString();
                tempChannel.Type = chn["type"].ToObject<ChannelType>();

                if(!chn["topic"].IsNullOrEmpty())
                    tempChannel.Topic = chn["topic"].ToString();
                if (tempChannel.Type == ChannelType.Voice && !chn["bitrate"].IsNullOrEmpty())
                    tempChannel.Bitrate = chn["bitrate"].ToObject<int>();

                tempChannel.Name = chn["name"].ToString();
                tempChannel.Private = false;
                tempChannel.PermissionOverrides = new List<DiscordPermissionOverride>();
                tempChannel.parent = server;
                foreach (var o in chn["permission_overwrites"])
                {
                    if (tempChannel.Type == ChannelType.Voice)
                        continue;
                    DiscordPermissionOverride dpo = new DiscordPermissionOverride(o["allow"].ToObject<uint>(), o["deny"].ToObject<uint>());
                    dpo.id = o["id"].ToString();

                    if (o["type"].ToString() == "member")
                        dpo.type = DiscordPermissionOverride.OverrideType.member;
                    else
                        dpo.type = DiscordPermissionOverride.OverrideType.role;

                    tempChannel.PermissionOverrides.Add(dpo);
                }
                server.Channels.Add(tempChannel);
            }
            foreach(var mbr in message["d"]["members"])
            {
                DiscordMember member = JsonConvert.DeserializeObject<DiscordMember>(mbr["user"].ToString());
                member.parentclient = this;
                member.Parent = server;
                
                foreach(var rollid in mbr["roles"])
                {
                    member.Roles.Add(server.Roles.Find(x => x.ID == rollid.ToString()));
                }
                if (member.Roles.Count == 0)
                    member.Roles.Add(server.Roles.Find(x => x.Name == "@everyone"));
                server.Members.Add(member);
            }
            server.Owner = server.Members.Find(x => x.ID == message["d"]["owner_id"].ToString());

            ServersList.Add(server);
            e.server = server;
            if (GuildCreated != null)
                GuildCreated(this, e);
        }

        private void GuildMemberAddEvents(JObject message)
        {
            DiscordGuildMemberAddEventArgs e = new DiscordGuildMemberAddEventArgs();
            e.RawJson = message;
            e.Guild = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());

            DiscordMember existingMember = e.Guild.Members.Find(x => x.ID == message["d"]["user"]["id"].ToString());
            if (existingMember != null)
            {

                DiscordMember newMember = JsonConvert.DeserializeObject<DiscordMember>(message["d"]["user"].ToString());
                newMember.parentclient = this;
                e.AddedMember = newMember;
                newMember.Parent = e.Guild;
                e.roles = message["d"]["roles"].ToObject<string[]>();
                e.JoinedAt = DateTime.Parse(message["d"]["joined_at"].ToString());

                ServersList.Find(x => x == e.Guild).Members.Add(newMember);
                if (UserAddedToServer != null)
                    UserAddedToServer(this, e);
            }
            else
            {
                DebugLogger.Log($"Skipping guild member add because user already exists in server.", MessageLevel.Debug);
            }
        }
        private void GuildMemberRemoveEvents(JObject message)
        {
            DiscordGuildMemberRemovedEventArgs e = new DiscordGuildMemberRemovedEventArgs();
            DiscordMember removed = new DiscordMember(this);
            removed.parentclient = this;

            List<DiscordMember> membersToRemove = new List<DiscordMember>();
            foreach(var server in ServersList)
            {
                if (server.ID != message["d"]["guild_id"].ToString())
                    continue;
                for(int i = 0; i < server.Members.Count; i++)
                {
                    if(server.Members[i].ID == message["d"]["user"]["id"].ToString())
                    {
                        removed = server.Members[i];
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
                        server.Members.Remove(member);
                    }
                    catch { } //oh, you mean useless?
                }
            }
            e.MemberRemoved = removed;
            e.Server = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
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
                for (int i = 0; i < server.Members.Count; i++)
                {
                    if (server.Members[i].ID == newMember.ID)
                    {
                        server.Members[i] = newMember;
                        oldMember = server.Members[i];
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
                        newMember.Roles.Add(newMember.Parent.Roles.Find(x => x.ID == role.ToString()));
                    }
                }
                else
                {
                    newMember.Roles.Add(newMember.Parent.Roles.Find(x => x.Name == "@everyone"));
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
            inServer = ServersList.Find(x => x.Channels.Find(y => y.ID == message["d"]["channel_id"].ToString()) != null);
            if(inServer == null) //dm delete
            {
                DiscordPrivateMessageDeletedEventArgs dm = new DiscordPrivateMessageDeletedEventArgs();
                dm.DeletedMessage = e.DeletedMessage;
                dm.RawJson = message;
                dm.Channel = PrivateChannels.Find(x => x.ID == message["d"]["channel_id"].ToString());
                if (PrivateMessageDeleted != null)
                    PrivateMessageDeleted(this, dm);
            }
            else
            {
                e.Channel = inServer.Channels.Find(x => x.ID == message["d"]["channel_id"].ToString());
                e.RawJson = message;
            }

            if (MessageDeleted != null)
                MessageDeleted(this, e);
        }

        private void VoiceStateUpdateEvents(JObject message)
        {
            var f = message["d"]["channel_id"];
            if (f.ToString() == null)
            {
                DiscordLeftVoiceChannelEventArgs le = new DiscordLeftVoiceChannelEventArgs();
                DiscordServer inServer = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
                le.user = inServer.Members.Find(x => x.ID == message["d"]["user_id"].ToString());
                le.guild = inServer;
                le.RawJson = message;

                if (VoiceClient != null && VoiceClient.Connected)
                    VoiceClient.MemberRemoved(le.user);
                if (UserLeftVoiceChannel != null)
                    UserLeftVoiceChannel(this, le);
                return;
            }
            DiscordVoiceStateUpdateEventArgs e = new DiscordVoiceStateUpdateEventArgs();
            e.guild = ServersList.Find(x => x.ID == message["d"]["guild_id"].ToString());
            DiscordMember memberToUpdate = e.guild.Members.Find(x => x.ID == message["d"]["user_id"].ToString());
            if (memberToUpdate != null)
            {
                e.channel = e.guild.Channels.Find(x => x.ID == message["d"]["channel_id"].ToString());
                memberToUpdate.CurrentVoiceChannel = e.channel;
                if (!message["d"]["self_deaf"].IsNullOrEmpty())
                    e.self_deaf = message["d"]["self_deaf"].ToObject<bool>();
                e.deaf = message["d"]["deaf"].ToObject<bool>();
                if (!message["d"]["self_mute"].IsNullOrEmpty())
                    e.self_mute = message["d"]["self_mute"].ToObject<bool>();
                e.mute = message["d"]["mute"].ToObject<bool>();
                memberToUpdate.Muted = e.mute;
                e.suppress = message["d"]["suppress"].ToObject<bool>();
                memberToUpdate.Deaf = e.suppress;
                e.RawJson = message;

                e.user = memberToUpdate;
                
                if (VoiceClient != null && VoiceClient.Connected)
                    VoiceClient.MemberAdded(e.user);

                if (!message["d"]["session_id"].IsNullOrEmpty()) //then this has to do with you
                {
                    if (e.user.ID == Me.ID)
                    {
                        Me.Muted = e.self_mute;
                        Me.Deaf = e.self_deaf;
                        if (VoiceClient != null)
                            VoiceClient.SessionID = message["d"]["session_id"].ToString();
                    }
                }

                if (VoiceStateUpdate != null)
                    VoiceStateUpdate(this, e);
            }
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

        /// <summary>
        /// Disposes.
        /// </summary>
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

        /// <summary>
        /// Logs out of Discord and then disposes.
        /// </summary>
        public void Logout()
        {
            string url = Endpoints.BaseAPI + Endpoints.Auth + "/logout";
            string msg = JsonConvert.SerializeObject(new { token = token });
            WebWrapper.Post(url, msg);
            Dispose();
        }
        
        /// <summary>
        /// Sends a login request.
        /// </summary>
        /// <returns>The token if login was succesful, or null if not</returns>
        public string SendLoginRequest()
        {
            if (token == null) //no token override provided, need to read token
            {
                if (String.IsNullOrEmpty(ClientPrivateInformation.Email))
                {
                    throw new ArgumentNullException("Email was null/invalid!");
                }
                StrippedEmail = ClientPrivateInformation.Email.Replace('@', '_').Replace('.', '_'); //strips characters from email for hashing

                if (File.Exists(StrippedEmail.GetHashCode() + ".cache"))
                {
                    string read = "";
                    using (var sr = new StreamReader(StrippedEmail.GetHashCode() + ".cache"))
                    {
                        read = sr.ReadLine();
                        if (read.StartsWith("#")) //comment
                        {
                            token = sr.ReadLine();
                            DebugLogger.Log("Loading token from cache.");
                        }
                        token = token.Trim(); //trim any excess whitespace
                        Console.Write("");
                    }
                }
                else
                {
                    if (ClientPrivateInformation == null || ClientPrivateInformation.Email == null || ClientPrivateInformation.Password == null)
                        throw new ArgumentNullException("You didn't supply login information!");
                    string url = Endpoints.BaseAPI + Endpoints.Auth + Endpoints.Login;
                    string msg = JsonConvert.SerializeObject(new
                    {
                        email = ClientPrivateInformation.Email,
                        password = ClientPrivateInformation.Password
                    });
                    DebugLogger.Log("No token present, sending login request..");
                    var result = JObject.Parse(WebWrapper.Post(url, msg));

                    if (result["token"].IsNullOrEmpty())
                    {
                        string message = "Failed to login to Discord.";
                        if (!result["email"].IsNullOrEmpty())
                            message += " Email was invalid: " + result["email"];
                        if (!result["password"].IsNullOrEmpty())
                            message += " password was invalid: " + result["password"];

                        throw new DiscordLoginException(message);
                    }
                    token = result["token"].ToString();

                    using (var sw = new StreamWriter(StrippedEmail.GetHashCode() + ".cache"))
                    {
                        sw.WriteLine($"#Token cache for {ClientPrivateInformation.Email}");
                        sw.WriteLine(token);
                        DebugLogger.Log($"{StrippedEmail.GetHashCode()}.cache written!");
                    }
                }
            }
            return token;
        }
    }
}
