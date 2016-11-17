using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace DSharpPlus
{
    public class DiscordClient
    {
        #region Events
        public event EventHandler SocketOpened;
        public event EventHandler<CloseEventArgs> SocketClosed;
        public event EventHandler Ready;
        public event EventHandler<ChannelCreateEventArgs> ChannelCreated;
        public event EventHandler<DMChannelCreateEventArgs> DMChannelCreated;
        public event EventHandler<ChannelUpdateEventArgs> ChannelUpdated;
        public event EventHandler<ChannelDeleteEventArgs> ChannelDeleted;
        public event EventHandler<DMChannelDeleteEventArgs> DMChannelDeleted;
        public event EventHandler<GuildCreateEventArgs> GuildCreated;
        public event EventHandler<GuildCreateEventArgs> GuildAvailable;
        public event EventHandler<GuildUpdateEventArgs> GuildUpdated;
        public event EventHandler<GuildDeleteEventArgs> GuildDeleted;
        public event EventHandler<GuildDeleteEventArgs> GuildUnavailable;
        public event EventHandler<MessageCreateEventArgs> MessageCreated;
        #endregion
        
        #region Internal Variables
        internal static CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        internal static CancellationToken _cancelToken = _cancelTokenSource.Token;

        internal static DiscordConfig config;

        internal static List<IModule> _modules = new List<IModule>();
        #endregion

        #region Public Variables
        internal static DebugLogger _debugLogger = new DebugLogger();
        public DebugLogger DebugLogger => _debugLogger;

        internal static int _gatewayVersion = 0;
        public int GatewayVersion => _gatewayVersion;

        internal static string _gatewayUrl = "";
        public string GatewayUrl => _gatewayUrl;

        internal static int _shardCount = 1;
        public int Shards => _shardCount;

        internal static DiscordUser _me;
        public DiscordUser Me => _me;

        internal static List<DiscordDMChannel> _privateChannels = new List<DiscordDMChannel>();
        public List<DiscordDMChannel> PrivateChannels => _privateChannels;

        internal static Dictionary<ulong, DiscordGuild> _guilds = new Dictionary<ulong, DiscordGuild>();
        public Dictionary<ulong, DiscordGuild> Guilds => _guilds;
        #endregion

        public WebSocketClient WebSocketClient;

        public DiscordClient()
        {
            InternalSetup();
        }

        public DiscordClient(DiscordConfig config)
        {
            DiscordClient.config = config;

            InternalSetup();
        }

        internal async void InternalSetup()
        {
            if (config.UseInternalLogHandler)
                DebugLogger.LogMessageReceived += (sender, e) => DebugLogger.LogHandler(sender, e);
        }

        public async Task Connect() => await this.InternalConnect();

        public async Task Connect(string tokenOverride, TokenType tokenType)
        {
            config.Token = tokenOverride;
            config.TokenType = tokenType;

            await InternalConnect();
        }

        public IModule AddModule(IModule module)
        {
            module.Setup(this);
            _modules.Add(module);
            return module;
        }

        public T GetModule<T>() where T : class, IModule
        {
            Console.WriteLine(typeof(T));

            return _modules.Find(x => x.GetType() == typeof(T)) as T;
        }

        // TODO
        internal async Task InternalConnect()
        {
            await InternalUpdateGateway();
            _me = await InternalGetCurrentUser();

            WebSocketClient = new WebSocketClient();
            WebSocketClient.SocketOpened += (sender, e) => {
                if (WebSocketClient._sessionID == "")
                    SendIdentify();
                else
                    SendResume();
                SocketOpened?.Invoke(sender, e);
            };
            WebSocketClient.SocketClosed += (sender, e) =>
            {
                if (!e.WasClean && config.AutoReconnect)
                {
                    WebSocketClient.Disconnect();
                    WebSocketClient.Connect();
                    DebugLogger.LogMessage(LogLevel.Critical, "Bot crashed. Reconnecting", DateTime.Now);
                }
                SocketClosed?.Invoke(sender, e);
            };
            WebSocketClient.SocketMessage += (sender, e) => HandleSocketMessage(e.Data);
            WebSocketClient.Connect();
        }

        internal async Task InternalUpdateGuild(DiscordGuild guild)
        {
            if (Guilds[guild.ID] == null)
                Guilds.Add(guild.ID, guild);
            else
                Guilds[guild.ID] = guild;
        }

        internal async static Task InternalUpdateGateway()
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Gateway;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            if (config.TokenType == TokenType.Bot)
                url += Endpoints.Bot;

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);

            JObject jObj = JObject.Parse(response.Response);
            _gatewayUrl = jObj.Value<string>("url");
            if (jObj["shards"] != null)
                _shardCount = jObj.Value<int>("shards");
        }

        #region Public Functions
        public async Task<DiscordUser> GetUser(string user) => await InternalGetUser(user);
        public async Task DeleteChannel(ulong id) => await InternalDeleteChannel(id);
        public async Task DeleteChannel(DiscordChannel channel) => await InternalDeleteChannel(channel.ID);
        public async Task<DiscordMessage> GetMessage(DiscordChannel channel, ulong MessageID) => await InternalGetMessage(channel.ID, MessageID);
        public async Task<DiscordMessage> GetMessage(ulong ChannelID, ulong MessageID) => await InternalGetMessage(ChannelID, MessageID);
        public async Task<DiscordChannel> GetChannel(ulong ID) => await InternalGetChannel(ID);
        public async Task<DiscordMessage> SendMessage(ulong ChannelID, string content, bool tts = false) => await InternalCreateMessage(ChannelID, content, tts);
        public async Task<DiscordMessage> SendMessage(DiscordChannel Channel, string content, bool tts = false) => await InternalCreateMessage(Channel.ID, content, tts);
        public async Task<DiscordGuild> CreateGuild(string name) => await InternalCreateGuildAsync(name);
        public async Task<DiscordGuild> GetGuild(ulong id) => await InternalGetGuildAsync(id);
        public async Task<DiscordGuild> DeleteGuild(ulong ID) => await InternalDeleteGuild(ID);
        public async Task<DiscordGuild> DeleteGuild(DiscordGuild Guild) => await InternalDeleteGuild(Guild.ID);
        public async Task<DiscordChannel> GetChannelByID(ulong ID) => await InternalGetChannel(ID);
        public async Task<DiscordInvite> GetInviteByCode(string code) => await InternalGetInvite(code);
        public async Task<List<DiscordConnection>> GetConnections() => await InternalGetUsersConnections();
        public async Task<List<DiscordVoiceRegion>> ListRegions() => await InternalListVoiceRegions();
        public async Task<DiscordWebhook> GetWebhook(ulong ID) => await InternalGetWebhook(ID);
        public async Task<DiscordWebhook> GetWebhookWithToken(ulong ID, string token) => await InternalGetWebhookWithToken(ID, token);
        #endregion

        #region Unsorted / Testing / Not working
        // This needs some work.
        internal async static Task<List<DiscordMember>> InternalListGuildMembers(ulong GuildID, int limit = 0, int after = 0)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Members;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (limit != 0)
                j.Add("limit", limit);
            if (after != 0)
                j.Add("after", after);

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            JArray ja = JArray.Parse(response.Response);
            List<DiscordMember> members = new List<DiscordMember>();
            Console.WriteLine(response.Response);
            foreach(JObject m in ja)
            {
                members.Add(m.ToObject<DiscordMember>());
            }
            return members;
        }

        // Not working yet pls fix :^)
        internal async static Task<DiscordMember> InternalModifyGuildMember(ulong GuildID, ulong UserID, string nick = "",
            List<DiscordRole> roles = null, bool muted = false, bool deafened = false, ulong VoiceChannelID = 0)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Members + "/" + UserID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (nick != "")
                j.Add("nick", nick);
            if (roles != null)
            {
                JArray r = new JArray();
                foreach (DiscordRole role in roles)
                {
                    r.Add(Newtonsoft.Json.JsonConvert.SerializeObject(role));
                }
                j.Add("roles", r);
            }
            if (muted)
                j.Add("mute", true);
            if (deafened)
                j.Add("deaf", true);
            if (VoiceChannelID != 0)
                j.Add("channel_id", VoiceChannelID);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordMember>(response.Response);
        }

        internal async static Task<List<DiscordRole>> InternalBatchModifyGuildRole(ulong GuildID)
        {
            // I have no idea how to implement this with our current configuration.
            return new List<DiscordRole>();
        }

        #endregion

        #region Websocket
        internal async void HandleSocketMessage(string data)
        {
            JObject obj = JObject.Parse(data);
            switch (obj.Value<int>("op"))
            {
                case 0: OnDispatch(obj); break;
                case 7: OnReconnect(); break;
                case 10: OnHello(obj); break;
                case 11: OnHeartbeatAck(obj); break;
            }
        }

        internal void OnDispatch(JObject obj)
        {
            switch (obj.Value<string>("t").ToLower())
            {
                case "ready": OnReadyEvent(obj); break;
                case "channel_create": OnChannelCreateEvent(obj); break;
                case "channel_update": OnChannelUpdateEvent(obj); break;
                case "channel_delete": OnChannelDeleteEvent(obj); break;
                case "guild_create": OnGuildCreateEvent(obj); break;
                case "guild_update": OnGuildUpdateEvent(obj); break;
                case "guild_delete": OnGuildDeleteEvent(obj); break;
                case "guild_ban_add": break;
                case "guild_ban_remove": break;
                case "guild_emojis_update": break;
                case "guild_integrations_update": break;
                case "guild_member_add": break;
                case "guild_member_remove": break;
                case "guild_member_update": break;
                case "guild_member_chunk": break;
                case "guild_role_create": break;
                case "guild_role_update": break;
                case "guild_role_delete": break;
                case "message_create": OnMessageCreateEvent(obj); break;
                case "message_update": break;
                case "message_delete": break;
                case "message_bulk_delete": break;
                case "presence_update": break;
                case "typing_start": break;
                case "user_settings_update": break;
                case "user_update": break;
                case "voice_state_update": break;
                case "voice_server_update": break;
                default:
                    DebugLogger.LogMessage(LogLevel.Warning, $"Unknown event: {obj.Value<string>("t")}\n{obj["d"].ToString()}", DateTime.Now);
                    break;
            }
        }

        #region Events
        internal void OnReadyEvent(JObject obj)
        {
            _gatewayVersion = obj["d"]["v"].ToObject<int>();
            _me = obj["d"]["user"].ToObject<DiscordUser>();
            _privateChannels = obj["d"]["private_channels"].ToObject<List<DiscordDMChannel>>();
            foreach (JObject guild in obj["d"]["guilds"])
            {
                _guilds.Add(guild.Value<ulong>("id"), guild.ToObject<DiscordGuild>());
            }
            WebSocketClient._sessionID = obj["d"]["session_id"].ToString();

            Ready?.Invoke(this, new EventArgs());
        }
        internal void OnChannelCreateEvent(JObject obj)
        {
            if (obj["d"]["is_private"] != null && obj["d"]["is_private"].ToObject<bool>())
            {
                DiscordDMChannel channel = obj["d"].ToObject<DiscordDMChannel>();
                _privateChannels.Add(channel);

                DMChannelCreated?.Invoke(this, new DMChannelCreateEventArgs() { Channel = channel });
            }
            else
            {
                DiscordChannel channel = obj["d"].ToObject<DiscordChannel>();

                _guilds[channel.GuildID].Channels.Add(channel);

                ChannelCreated?.Invoke(this, new ChannelCreateEventArgs() { Channel = channel, Guild = _guilds[channel.GuildID] });
            }
        }
        internal void OnChannelUpdateEvent(JObject obj)
        {
            DiscordChannel channel = obj["d"].ToObject<DiscordChannel>();
            int channelIndex = _guilds[channel.GuildID].Channels.FindIndex(x => x.ID == channel.ID);

            _guilds[channel.GuildID].Channels[channelIndex] = channel;

            ChannelUpdated?.Invoke(this, new ChannelUpdateEventArgs() { Channel = channel, Guild = _guilds[channel.GuildID] });
        }
        internal void OnChannelDeleteEvent(JObject obj)
        {
            if (obj["d"]["is_private"] != null && obj["d"]["is_private"].ToObject<bool>())
            {
                DiscordDMChannel channel = obj["d"].ToObject<DiscordDMChannel>();
                int channelIndex = _privateChannels.FindIndex(x => x.ID == channel.ID);
                _privateChannels.RemoveAt(channelIndex);

                DMChannelDeleted?.Invoke(this, new DMChannelDeleteEventArgs() { Channel = channel });
            }
            else
            {
                DiscordChannel channel = obj["d"].ToObject<DiscordChannel>();
                int channelIndex = _guilds[channel.GuildID].Channels.FindIndex(x => x.ID == channel.ID);
                _guilds[channel.GuildID].Channels.RemoveAt(channelIndex);

                ChannelDeleted?.Invoke(this, new ChannelDeleteEventArgs() { Channel = channel, Guild = _guilds[channel.GuildID] });
            }
        }
        internal void OnGuildCreateEvent(JObject obj)
        {
            DiscordGuild guild = obj["d"].ToObject<DiscordGuild>();

            foreach (DiscordChannel channel in guild.Channels)
                if (channel.GuildID == 0) channel.GuildID = guild.ID;

            if (_guilds[obj["d"].Value<ulong>("id")] != null)
            {
                _guilds[guild.ID] = guild;

                GuildAvailable?.Invoke(this, new GuildCreateEventArgs() { Guild = guild });
            }
            else
            {
                _guilds.Add(guild.ID, guild);

                GuildCreated?.Invoke(this, new GuildCreateEventArgs() { Guild = guild });
            }
        }
        internal void OnGuildUpdateEvent(JObject obj)
        {
            DiscordGuild guild = _guilds[obj["d"].Value<ulong>("id")];
            if (guild != null)
            {
                guild = obj["d"].ToObject<DiscordGuild>();
                _guilds[guild.ID] = guild;

                GuildUpdated?.Invoke(this, new GuildUpdateEventArgs() { Guild = guild });
            }
            else
            {
                guild = obj["d"].ToObject<DiscordGuild>();
                _guilds.Add(guild.ID, guild);

                GuildUpdated?.Invoke(this, new GuildUpdateEventArgs() { Guild = guild });
            }
        }
        internal void OnGuildDeleteEvent(JObject obj)
        {
            if (_guilds[obj["d"].Value<ulong>("id")] != null)
            {
                if (obj["d"]["unavailable"] != null)
                {
                    DiscordGuild guild = obj["d"].ToObject<DiscordGuild>(); 

                    _guilds[guild.ID] = guild;

                    GuildUnavailable?.Invoke(this, new GuildDeleteEventArgs() { ID = obj["d"].Value<ulong>("id"), Unavailable = true });
                }
                else
                {
                    _guilds.Remove(obj["d"].Value<ulong>("id"));

                    GuildDeleted?.Invoke(this, new GuildDeleteEventArgs() { ID = obj["d"].Value<ulong>("id") });
                }
            }
        }
        internal void OnGuildBanAdd(JObject obj)
        {

        }
        internal void OnGuildBanRemove(JObject obj)
        {

        }

        internal void OnMessageCreateEvent(JObject obj)
        {
            DiscordMessage message = obj["d"].ToObject<DiscordMessage>();
            _guilds[message.Parent.Parent.ID].Channels.Find(x => x.ID == message.ChannelID).LastMessageID = message.ID;

            List<DiscordMember> MentionedUsers = new List<DiscordMember>();
            foreach (ulong user in Utils.GetUserMentions(message))
            {
                MentionedUsers.Add(_guilds[message.Parent.Parent.ID].Members.Find(x => x.User.ID == user));
            }

            List<DiscordRole> MentionedRoles = new List<DiscordRole>();
            foreach (ulong role in Utils.GetRoleMentions(message))
            {
                MentionedRoles.Add(_guilds[message.Parent.Parent.ID].Roles.Find(x => x.ID == role));
            }

            List<DiscordChannel> MentionedChannels = new List<DiscordChannel>();
            foreach (ulong channel in Utils.GetChannelMentions(message))
            {
                MentionedChannels.Add(_guilds[message.Parent.Parent.ID].Channels.Find(x => x.ID == channel));
            }

            MessageCreateEventArgs args = new MessageCreateEventArgs() { Message = message, MentionedUsers = MentionedUsers, MentionedRoles = MentionedRoles, MentionedChannels = MentionedChannels };
            MessageCreated?.Invoke(this, args);
        }

        #endregion

        internal void OnReconnect()
        {
            _debugLogger.LogMessage(LogLevel.Info, "Received OP 7 - Reconnect. ", DateTime.Now);

            WebSocketClient.Disconnect();
            WebSocketClient.Connect();
        }

        internal void OnHello(JObject obj)
        {
            WebSocketClient._heartbeatInterval = obj["d"].Value<int>("heartbeat_interval");
            WebSocketClient._heartbeatThread = new Thread(() => { StartHeartbeating(); });
            WebSocketClient._heartbeatThread.Start();
        }

        internal void OnHeartbeatAck(JObject obj)
        {
            _debugLogger.LogMessage(LogLevel.Unnecessary, "Received WebSocket Heartbeat Ack", DateTime.Now);
        }

        internal void StartHeartbeating()
        {
            _debugLogger.LogMessage(LogLevel.Unnecessary, "Starting WebSocket Heartbeating", DateTime.Now);
            while (!_cancelToken.IsCancellationRequested)
            {
                SendHeartbeat();
                Thread.Sleep(WebSocketClient._heartbeatInterval);
            }
        }

        internal void SendHeartbeat()
        {
            DiscordClient._debugLogger.LogMessage(LogLevel.Unnecessary, "Sending WebSocket Heartbeat", DateTime.Now);
            JObject obj = new JObject() {
                { "op", 1 },
                { "d", WebSocketClient._sequence }
            };
            WebSocketClient._socket.Send(obj.ToString());
        }

        internal void SendIdentify()
        {
            JObject obj = new JObject()
            {
                { "op", 2 },
                { "d", new JObject()
                    {
                        { "token", Utils.GetFormattedToken() },
                        { "properties", new JObject() {
                            { "$os", "linux" },
                            { "$browser", "DSharpPlus 1.0" },
                            { "$device", "DSharpPlus 1.0" },
                            { "$referrer", "" },
                            { "$referring_domain", "" }
                        } },
                        { "compress", false },
                        { "large_threshold" , config.LargeThreshold },
                        { "shards", new JArray() { 0, _shardCount } }
                    }
                }
            };
            WebSocketClient._socket.Send(obj.ToString());
        }

        internal void SendResume()
        {
            JObject obj = new JObject()
            {
                { "op", 6 },
                { "d", new JObject()
                    {
                        { "token", WebSocketClient._sessionToken },
                        { "session_id", WebSocketClient._sessionID },
                        { "seq", WebSocketClient._sequence }
                    }
                }
            };
            WebSocketClient._socket.SendAsync(obj.ToString(), (x) => { });
        }
        #endregion

        internal static ulong GetGuildIdFromChannelID(ulong channelid)
        {
            foreach(DiscordGuild guild in _guilds.Values)
            {
                if (guild.Channels.Find(x => x.ID == channelid) != null) return guild.ID;
            }
            return 0;
        }

        internal static int GetChannelIndex(ulong channelid)
        {
            foreach (DiscordGuild guild in _guilds.Values)
            {
                if (guild.Channels.Find(x => x.ID == channelid) != null) return guild.Channels.FindIndex(x => x.ID == channelid);
            }
            return 0;
        }

        #region HTTP Actions
        #region Guild
        //
        internal async static Task<DiscordGuild> InternalCreateGuildAsync(string name)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds;
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", Utils.GetFormattedToken());
            JObject payload = new JObject() { { "name", name } };

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, payload.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);

            DiscordGuild guild = Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
            return guild;
        }

        //
        internal async static Task<DiscordGuild> InternalGetGuildAsync(ulong id)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + $"/{id}";
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", Utils.GetFormattedToken());

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);

            DiscordGuild guild = Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
            return guild;
        }


        internal async static void InternalDeleteGuildAsync(ulong id)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + $"/{id}";
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", Utils.GetFormattedToken());

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task<DiscordGuild> InternalModifyGuild(ulong GuildID, string name = "", string region = "", int verification_level = -1, int default_message_notifications = -1,
            ulong akfchannelid = 0, int afktimeout = -1, string icon = "", ulong ownerID = 0, string splash = "")
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID;
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", Utils.GetFormattedToken());
            JObject j = new JObject();
            if (name != "")
                j.Add("name", name);
            if (region != "")
                j.Add("region", region);
            if (verification_level != -1)
                j.Add("verification_level", verification_level);
            if (default_message_notifications != -1)
                j.Add("default_message_notifications", default_message_notifications);
            if (akfchannelid != 0)
                j.Add("akf_channel_id", akfchannelid);
            if (afktimeout != -1)
                j.Add("akf_timeout", afktimeout);
            if (icon != "")
                j.Add("icon", icon);
            if (ownerID != 0)
                j.Add("owner_id", ownerID);
            if (splash != "")
                j.Add("splash", splash);

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
        }

        internal async static Task<List<DiscordMember>> InternalGetGuildBans(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Bans;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordMember> bans = new List<DiscordMember>();
            foreach (JObject obj in j)
            {
                bans.Add(obj.ToObject<DiscordMember>());
            }
            return bans;
        }

        internal async static Task InternalCreateGuildBan(ulong GuildID, ulong UserID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Bans + "/" + UserID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PUT, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task InternalRemoveGuildBan(ulong GuildID, ulong UserID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Bans + "/" + UserID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task InternalLeaveGuild(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Users + "/@me" + Endpoints.Guilds + "/" + GuildID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task<DiscordGuild> InternalCreateGuild(string name, string region, string icon, int verification_level, int default_message_notifications)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("name", name);
            j.Add("region", region);
            j.Add("icon", icon);
            j.Add("verification_level", verification_level);
            j.Add("default_message_notifications", default_message_notifications);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
        }

        internal async static Task<DiscordMember> InternalAddGuildMember(ulong GuildID, ulong UserID, string AccessToken, string nick = "", List<DiscordRole> roles = null,
        bool muted = false, bool deafened = false)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Members + "/" + UserID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("access_token", AccessToken);
            if (nick != "")
                j.Add("nick", nick);
            if (roles != null)
            {
                JArray r = new JArray();
                foreach (DiscordRole role in roles)
                {
                    r.Add(Newtonsoft.Json.JsonConvert.SerializeObject(role));
                }
                j.Add("roles", r);
            }
            if (muted)
                j.Add("mute", true);
            if (deafened)
                j.Add("deaf", true);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PUT, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordMember>(response.Response);
        }
        #endregion
        #region Channel

        internal async static Task<DiscordChannel> InternalCreateGuildChannelAsync(ulong id, string name, ChannelType type)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + $"/{id}" + Endpoints.Channels;
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", Utils.GetFormattedToken());
            JObject payload = new JObject() { { "name", name }, { "type", type.ToString() }, { "permission_overwrites", null } };

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, payload.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordChannel>(response.Response);
        }

        internal async static Task<DiscordChannel> InternalGetChannel(ulong ID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ID;
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", Utils.GetFormattedToken());
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordChannel>(response.Response);
        }

        internal async static Task InternalDeleteChannel(ulong ID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ID;
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", Utils.GetFormattedToken());
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task<DiscordMessage> InternalGetMessage(ulong ChannelID, ulong MessageID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Messages + "/" + MessageID;
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", Utils.GetFormattedToken());
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        internal async static Task<DiscordMessage> InternalCreateMessage(ulong ChannelID, string content, bool tts)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Messages;
            JObject j = new JObject();
            j.Add("content",content);
            j.Add("tts", tts);
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", Utils.GetFormattedToken());
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        internal async static Task<DiscordMessage> InternalUploadFile(ulong ChannelID, string path, string filename, string content = "", bool tts = false)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Messages;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            NameValueCollection values = new NameValueCollection();
            if (content != "")
                values.Add("content", content);
            if (tts)
                values.Add("tts", tts.ToString());
            WebRequest request = await WebRequest.CreateMultipartRequestAsync(url, WebRequestMethod.POST, headers, values, path, filename);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        internal async static Task<List<DiscordChannel>> InternalGetGuildChannels(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Channels;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordChannel> channels = new List<DiscordChannel>();
            foreach(JObject jj in j)
            {
                channels.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordChannel>(jj.ToString()));
            }
            return channels;
        }

        internal async static Task<DiscordChannel> InternalCreateChannel(ulong GuildID, string name, ChannelType type, int bitrate = 0, int userlimit = 0)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Channels;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("name", name);
            if (type == ChannelType.Text)
                j.Add("type", "text");
            else
                j.Add("type", "voice");

            if (type == ChannelType.Voice)
            {
                j.Add("bitrate", bitrate);
                j.Add("userlimit", userlimit);
            }
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            Console.WriteLine(j.ToString());
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordChannel>(j.ToString());
        }

        // TODO
        internal async static Task InternalModifyGuildChannelPosition(ulong GuildID, ulong ChannelID, int position)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Channels;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("id", ChannelID);
            j.Add("position", position);
            Console.WriteLine(j.ToString());
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task<List<DiscordMessage>> InternalGetChannelMessages(ulong ChannelID, ulong around = 0, ulong before = 0, ulong after = 0, int limit = -1)
        {
            // ONLY ONE OUT OF around, before or after MAY BE USED.
            // THESE ARE MESSAGE ID's

            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Messages;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (around != 0)
                j.Add("around", around);
            if (before != 0)
                j.Add("before", before);
            if (after != 0)
                j.Add("after", after);
            if (limit > -1)
                j.Add("limit", limit);

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            JArray ja = JArray.Parse(response.Response);
            List<DiscordMessage> messages = new List<DiscordMessage>();
            foreach (JObject jo in ja)
            {
                messages.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordMessage>(jo.ToString()));
            }
            return messages;
        }

        internal async static Task<DiscordMessage> InternalGetChannelMessage(ulong ChannelID, ulong MessageID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Messages + "/" + MessageID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        internal async static Task<DiscordMessage> InternalEditMessage(ulong ChannelID, ulong MessageID, string content)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Messages + "/" + MessageID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("content", content);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        internal async static Task InternalDeleteMessage(ulong ChannelID, ulong MessageID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Messages + "/" + MessageID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task InternalBulkDeleteMessages(ulong ChannelID, List<ulong> MessageIDs)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Messages + Endpoints.BulkDelete;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            JArray msgs = new JArray();
            foreach (ulong messageID in MessageIDs)
            {
                msgs.Add(messageID);
            }
            j.Add("messages", msgs);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task<List<DiscordInvite>> InternalGetChannelInvites(ulong ChannelID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Invites;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            JArray ja = JArray.Parse(response.Response);
            List<DiscordInvite> invites = new List<DiscordInvite>();
            foreach (JObject jo in ja)
            {
                invites.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordInvite>(jo.ToString()));
            }
            return invites;
        }

        internal async static Task<DiscordInvite> InternalCreateChannelInvite(ulong ChannelID, int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Invites;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("max_age", max_age);
            j.Add("max_uses", max_uses);
            j.Add("temporary", temporary);
            j.Add("unique", unique);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
        }

        internal async static Task InternalDeleteChannelPermission(ulong ChannelID, ulong OverwriteID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Permissions + "/" + OverwriteID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task InternalTriggerTypingIndicator(ulong ChannelID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Typing;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task<List<DiscordMessage>> InternalGetPinnedMessages(ulong ChannelID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Pins;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordMessage> messages = new List<DiscordMessage>();
            foreach (JObject obj in j)
            {
                messages.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordMessage>(obj.ToString()));
            }
            return messages;
        }

        internal async static Task InternalAddPinnedChannelMessage(ulong ChannelID, ulong MessageID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Pins + "/" + MessageID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PUT, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task InternalDeletePinnedChannelMessage(ulong ChannelID, ulong MessageID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Pins + "/" + MessageID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task InternalGroupDMAddRecipient(ulong ChannelID, ulong UserID, string AccessToken)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Recipients + "/" + UserID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("access_token", AccessToken);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PUT, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task InternalGroupDMRemoveRecipient(ulong ChannelID, ulong UserID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Recipients + "/" + UserID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }


        internal async static Task InternalEditChannelPermissions(ulong ChannelID, ulong OverwriteID, int allow, int deny, string type)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Permissions + "/" + OverwriteID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("allow", allow);
            j.Add("deny", deny);
            j.Add("type", type);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PUT, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task<DiscordDMChannel> InternalCreateDM(ulong RecipientID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Users + "/@me" + Endpoints.Channels;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("recipient_id", RecipientID);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordDMChannel>(response.Response);
        }

        internal async static Task<DiscordDMChannel> InternalCreateGroupDM(List<string> access_tokens)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Users + "/@me" + Endpoints.Channels;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JArray tokens = new JArray();
            foreach (string token in access_tokens)
            {
                tokens.Add(token);
            }
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, tokens.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordDMChannel>(response.Response);
        }
        #endregion
        #region Member
        // TODO
        internal async static Task<DiscordMember> InternalGetGuildMemberAsync(ulong guild_id, ulong id)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + $"/{guild_id}" + Endpoints.Members + $"/{id}";
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Authorization", Utils.GetFormattedToken());

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);

            //return DiscordMember.FromJson(response.Response);
            return new DiscordMember();
        }

        internal async static Task<DiscordUser> InternalGetUser(string user)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Users + $"/{user}";
            WebHeaderCollection headers = Utils.GetBaseHeaders();

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordUser>(response.Response);
        }

        internal async static Task<DiscordMember> InternalGetGuildMember(ulong GuildID, ulong MemberID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Members + "/" + MemberID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordMember>(response.Response);
        }

        internal async static Task InternalRemoveGuildMember(ulong GuildID, ulong UserID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Members + "/" + UserID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal async static Task<DiscordUser> InternalGetCurrentUser()
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Users + "/@me";
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordUser>(response.Response);
        }

        internal async static Task<DiscordUser> InternalGetUser(ulong UserID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Users + "/" + UserID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordUser>(response.Response);
        }

        internal async static Task<DiscordUser> InternalModifyCurrentUser(string username = "", string base64avatar = "")
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Users + "/@me";
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (username != "")
                j.Add("", username);
            if (base64avatar != "")
                j.Add("avatar", base64avatar);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordUser>(response.Response);
        }

        internal async static Task<List<DiscordGuild>> InternalGetCurrentUserGuilds()
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Users + "/@me" + Endpoints.Guilds;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            List<DiscordGuild> guilds = new List<DiscordGuild>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                guilds.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordGuild>(j.ToString()));
            }
            return guilds;
        }

        #endregion
        #region Roles
        // TODO
        internal static List<DiscordRole> InternalGetGuildRoles(ulong guild_id)
        {
            return new List<DiscordRole>();
        }

        // TODO
        internal static List<DiscordRole> InternalModifyGuildRolePosition(ulong guild_id, ulong id, int position)
        {
            return new List<DiscordRole>();
        }

        internal async static Task<DiscordGuild> InternalGetGuild(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
        }

        internal async static Task<DiscordGuild> InternalModifyGuild(string name = "", string region = "", string icon = "", int verification_level = -1,
            int default_message_notifications = -1, ulong afk_channel_id = 0, int afk_timeout = -1, ulong owner_id = 0, string splash = "")
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (name != "")
                j.Add("name", name);
            if (region != "")
                j.Add("region", region);
            if (icon != "")
                j.Add("icon", icon);
            if (verification_level > -1)
                j.Add("verification_level", verification_level);
            if (default_message_notifications > -1)
                j.Add("default_message_notifications", default_message_notifications);
            if (afk_channel_id > 0)
                j.Add("afk_channel_id", afk_channel_id);
            if (afk_timeout > -1)
                j.Add("afk_timeout", afk_timeout);
            if (owner_id > 0)
                j.Add("owner_id", owner_id);
            if (splash != "")
                j.Add("splash", splash);

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
        }

        internal async static Task<DiscordGuild> InternalDeleteGuild(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
        }

        internal async static Task<DiscordRole> InternalModifyGuildRole(ulong GuildID, ulong RoleID, string name, int permissions, int position, int color, bool separate, bool mentionable)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Roles + RoleID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("name", name);
            j.Add("permissions", permissions);
            j.Add("position", position);
            j.Add("color", color);
            j.Add("hoist", separate);
            j.Add("mentionable", mentionable);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await request.HandleRequestAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordRole>(response.Response);
        }

        internal async static Task<DiscordRole> InternalDeleteRole(ulong GuildID, ulong RoleID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Roles + "/" + RoleID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordRole>(response.Response);
        }

        internal async static Task<DiscordRole> InternalCreateGuildRole(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordRole>(response.Response);
        }

        #endregion
        #region Prune
        // TODO
        internal async static Task<int> InternalGetGuildPruneCount(ulong GuildID, int days)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Prune;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject payload = new JObject();
            payload.Add("days", days);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers, payload.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            JObject j = JObject.Parse(response.Response);
            return int.Parse(j["pruned"].ToString());
        }

        // TODO
        internal async static Task<int> InternalBeginGuildPrune(ulong GuildID, int days)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Prune;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject payload = new JObject();
            payload.Add("days", days);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, payload.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            JObject j = JObject.Parse(response.Response);
            return int.Parse(j["pruned"].ToString());
        }
        #endregion
        #region GuildVarious

        internal async static Task<List<DiscordIntegration>> InternalGetGuildIntegrations(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Integrations;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await request.HandleRequestAsync();
            JArray j = JArray.Parse(response.Response);
            List<DiscordIntegration> integrations = new List<DiscordIntegration>();
            foreach (JObject obj in j)
            {
                integrations.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordIntegration>(obj.ToString()));
            }
            return integrations;
        }

        internal async static Task<DiscordIntegration> InternalCreateGuildIntegration(ulong GuildID, string type, ulong ID)
        {
            // Attach from user
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Integrations;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("type", type);
            j.Add("id", ID);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, j.ToString());
            WebResponse response = await request.HandleRequestAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordIntegration>(response.Response);
        }

        internal async static Task<DiscordIntegration> InternalModifyGuildIntegration(ulong GuildID, ulong IntegrationID, int expire_behaviour,
            int expire_grace_period, bool enable_emoticons)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Integrations + "/" + IntegrationID;
            JObject j = new JObject();
            j.Add("expire_behaviour", expire_behaviour);
            j.Add("expire_grace_period", expire_grace_period);
            j.Add("enable_emoticons", enable_emoticons);
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await request.HandleRequestAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordIntegration>(response.Response);
        }

        internal async static Task InternalDeleteGuildIntegration(ulong GuildID, DiscordIntegration integration)
        {
            ulong IntegrationID = integration.ID;
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Integrations + "/" + IntegrationID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = JObject.FromObject(integration);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers, j.ToString());
            WebResponse response = await request.HandleRequestAsync();
        }

        internal async static Task InternalSyncGuildIntegration(ulong GuildID, ulong IntegrationID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Integrations + "/" + IntegrationID + Endpoints.Sync;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers);
            WebResponse response = await request.HandleRequestAsync();
        }

        internal async static Task<DiscordGuildEmbed> InternalGetGuildEmbed(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Embed;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await request.HandleRequestAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordGuildEmbed>(response.Response);
        }

        internal async static Task<DiscordGuildEmbed> InternalModifyGuildEmbed(ulong GuildID, DiscordGuildEmbed embed)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Embed;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = JObject.FromObject(embed);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await request.HandleRequestAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordGuildEmbed>(response.Response);
        }

        internal async static Task<List<DiscordVoiceRegion>> InternalGetGuildVoiceRegions(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Regions;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await request.HandleRequestAsync();
            JArray j = JArray.Parse(response.Response);
            List<DiscordVoiceRegion> regions = new List<DiscordVoiceRegion>();
            foreach (JObject obj in j)
            {
                regions.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordVoiceRegion>(obj.ToString()));
            }
            return regions;
        }

        internal async static Task<List<DiscordInvite>> InternalGetGuildInvites(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Invites;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await request.HandleRequestAsync();
            JArray j = JArray.Parse(response.Response);
            List<DiscordInvite> invites = new List<DiscordInvite>();
            foreach (JObject obj in j)
            {
                invites.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordInvite>(obj.ToString()));
            }
            return invites;
        }

        #endregion
        #region Invite
        internal async static Task<DiscordInvite> InternalGetInvite(string InviteCode)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Invites + "/" + InviteCode;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
        }

        internal async static Task<DiscordInvite> InternalDeleteInvite(string InviteCode)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Invites + "/" + InviteCode;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.DELETE, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
        }

        internal async static Task<DiscordInvite> InternalAcceptInvite(string InviteCode)
        {
            // USER ONLY
            string url = Utils.GetAPIBaseUri() + Endpoints.Invites + "/" + InviteCode;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
        }
        #endregion
        #region Connections
        internal async static Task<List<DiscordConnection>> InternalGetUsersConnections()
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Users + "/@me" + Endpoints.Connections;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            List<DiscordConnection> connections = new List<DiscordConnection>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                connections.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordConnection>(j.ToString()));
            }
            return connections;
        }
        #endregion
        #region Voice
        internal async static Task<List<DiscordVoiceRegion>> InternalListVoiceRegions()
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Voice + Endpoints.Regions;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            List<DiscordVoiceRegion> regions = new List<DiscordVoiceRegion>();
            JArray j = JArray.Parse(response.Response);
            foreach (JObject obj in j)
            {
                regions.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordVoiceRegion>(obj.ToString()));
            }
            return regions;
        }
        #endregion
        #region Webhooks
        internal static async Task<DiscordWebhook> InternalCreateWebhook(ulong ChannelID, string name, string base64avatar)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Webhooks;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("name", name);
            j.Add("avatar", base64avatar);

            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.POST, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
        }

        internal static async Task<List<DiscordWebhook>> InternalGetChannelWebhooks(ulong ChannelID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Channels + "/" + ChannelID + Endpoints.Webhooks;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            List<DiscordWebhook> webhooks = new List<DiscordWebhook>();
            foreach(JObject j in JArray.Parse(response.Response))
            {
                webhooks.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordWebhook>(j.ToString()));
            }
            return webhooks;
        }

        internal static async Task<List<DiscordWebhook>> InternalGetGuildWebhooks(ulong GuildID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Guilds + "/" + GuildID + Endpoints.Webhooks;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            List<DiscordWebhook> webhooks = new List<DiscordWebhook>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                webhooks.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordWebhook>(j.ToString()));
            }
            return webhooks;
        }

        internal static async Task<DiscordWebhook> InternalGetWebhook(ulong WebhookID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Webhooks + "/" + WebhookID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
        }

        // Auth header not required
        internal static async Task<DiscordWebhook> InternalGetWebhookWithToken(ulong WebhookID, string WebhookToken)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Webhooks + "/" + WebhookID + "/" + WebhookToken;
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
        }

        internal static async Task<DiscordWebhook> InternalModifyWebhook(ulong WebhookID, string name, string base64avatar)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Webhooks + "/" + WebhookID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            j.Add("name", name);
            j.Add("avatar", base64avatar);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers, j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
        }

        internal static async Task<DiscordWebhook> InternalModifyWebhook(ulong WebhookID, string name, string base64avatar, string WebhookToken)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Webhooks + "/" + WebhookID + "/" + WebhookToken;
            JObject j = new JObject();
            j.Add("name", name);
            j.Add("avatar", base64avatar);
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, payload: j.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
        }

        internal static async Task InternalDeleteWebhook(ulong WebhookID)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Webhooks + "/" + WebhookID;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, headers);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal static async Task InternalDeleteWebhook(ulong WebhookID, string WebhookToken)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Webhooks + "/" + WebhookID + "/" + WebhookToken;
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal static async Task InternalExecuteWebhook(ulong WebhookID, string WebhookToken, string content = "", string username = "", string avatar_url = "",
            bool tts = false, List<DiscordEmbed> embeds = null)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Webhooks + "/" + WebhookID + "/" + WebhookToken;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            JObject req = new JObject();
            if (content != "")
                req.Add("content", content);
            if (username != "")
                req.Add("username", username);
            if (avatar_url != "")
                req.Add("avatar_url", avatar_url);
            if (tts)
                req.Add("tts", tts);
            if(embeds != null)
            {
                JArray arr = new JArray();
                foreach(DiscordEmbed e in embeds)
                {
                    arr.Add(Newtonsoft.Json.JsonConvert.SerializeObject(e));
                }
            }
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, payload: req.ToString());
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal static async Task InternalExecuteWebhookSlack(ulong WebhookID, string WebhookToken, string jsonpayload)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Webhooks + "/" + WebhookID + "/" + WebhookToken + Endpoints.Slack;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, payload: jsonpayload);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        internal static async Task InternalExecuteWebhookGithub(ulong WebhookID, string WebhookToken, string jsonpayload)
        {
            string url = Utils.GetAPIBaseUri() + Endpoints.Webhooks + "/" + WebhookID + "/" + WebhookToken + Endpoints.Github;
            WebHeaderCollection headers = Utils.GetBaseHeaders();
            WebRequest request = await WebRequest.CreateRequestAsync(url, WebRequestMethod.GET, payload: jsonpayload);
            WebResponse response = await WebWrapper.HandleRequestAsync(request);
        }

        #endregion
        #endregion
    }
}
