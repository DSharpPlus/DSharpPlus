using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using DSharpPlus.Web;

namespace DSharpPlus
{
    /// <summary>
    /// A Discord api wrapper
    /// </summary>
    public class DiscordClient : IDisposable
    {
        #region Events
        /// <summary>
        /// 
        /// </summary>
        public event AsyncEventHandler SocketOpened
        {
            add { this._socket_opened.Register(value); }
            remove { this._socket_opened.Unregister(value); }
        }
        private AsyncEvent _socket_opened = new AsyncEvent();
        /// <summary>
        /// 
        /// </summary>
        public event AsyncEventHandler SocketClosed
        {
            add { this._socket_closed.Register(value); }
            remove { this._socket_closed.Unregister(value); }
        }
        private AsyncEvent _socket_closed = new AsyncEvent();
        /// <summary>
        /// The ready event is dispatched when a client completed the initial handshake.
        /// </summary>
        public event AsyncEventHandler Ready
        {
            add { this._ready.Register(value); }
            remove { this._ready.Unregister(value); }
        }
        private AsyncEvent _ready = new AsyncEvent();
        /// <summary>
        /// Sent when a new channel is created.
        /// </summary>
        public event AsyncEventHandler<ChannelCreateEventArgs> ChannelCreated
        {
            add { this._channel_created.Register(value); }
            remove { this._channel_created.Unregister(value); }
        }
        private AsyncEvent<ChannelCreateEventArgs> _channel_created = new AsyncEvent<ChannelCreateEventArgs>();
        /// <summary>
        /// Sent when a new dm channel is created.
        /// </summary>
        public event AsyncEventHandler<DMChannelCreateEventArgs> DMChannelCreated
        {
            add { this._dm_channel_created.Register(value); }
            remove { this._dm_channel_created.Unregister(value); }
        }
        private AsyncEvent<DMChannelCreateEventArgs> _dm_channel_created = new AsyncEvent<DMChannelCreateEventArgs>();
        /// <summary>
        /// Sent when a channel is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelUpdateEventArgs> ChannelUpdated
        {
            add { this._channel_updated.Register(value); }
            remove { this._channel_updated.Unregister(value); }
        }
        private AsyncEvent<ChannelUpdateEventArgs> _channel_updated = new AsyncEvent<ChannelUpdateEventArgs>();
        /// <summary>
        /// Sent when a channel is deleted
        /// </summary>
        public event AsyncEventHandler<ChannelDeleteEventArgs> ChannelDeleted
        {
            add { this._channel_deleted.Register(value); }
            remove { this._channel_deleted.Unregister(value); }
        }
        private AsyncEvent<ChannelDeleteEventArgs> _channel_deleted = new AsyncEvent<ChannelDeleteEventArgs>();
        /// <summary>
        /// Sent when a dm channel is deleted
        /// </summary>
        public event AsyncEventHandler<DMChannelDeleteEventArgs> DMChannelDeleted
        {
            add { this._dm_channel_deleted.Register(value); }
            remove { this._dm_channel_deleted.Unregister(value); }
        }
        private AsyncEvent<DMChannelDeleteEventArgs> _dm_channel_deleted = new AsyncEvent<DMChannelDeleteEventArgs>();
        /// <summary>
        /// Sent when the user joins a new guild.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildCreated
        {
            add { this._guild_created.Register(value); }
            remove { this._guild_created.Unregister(value); }
        }
        private AsyncEvent<GuildCreateEventArgs> _guild_created = new AsyncEvent<GuildCreateEventArgs>();
        /// <summary>
        /// Sent when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildAvailable
        {
            add { this._guild_available.Register(value); }
            remove { this._guild_available.Unregister(value); }
        }
        private AsyncEvent<GuildCreateEventArgs> _guild_available = new AsyncEvent<GuildCreateEventArgs>();
        /// <summary>
        /// Sent when a guild is updated.
        /// </summary>
        public event AsyncEventHandler<GuildUpdateEventArgs> GuildUpdated
        {
            add { this._guild_updated.Register(value); }
            remove { this._guild_updated.Unregister(value); }
        }
        private AsyncEvent<GuildUpdateEventArgs> _guild_updated = new AsyncEvent<GuildUpdateEventArgs>();
        /// <summary>
        /// Sent when the user leaves or is removed from a guild.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildDeleted
        {
            add { this._guild_deleted.Register(value); }
            remove { this._guild_deleted.Unregister(value); }
        }
        private AsyncEvent<GuildDeleteEventArgs> _guild_deleted = new AsyncEvent<GuildDeleteEventArgs>();
        /// <summary>
        /// Sent when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildUnavailable
        {
            add { this._guild_unavailable.Register(value); }
            remove { this._guild_unavailable.Unregister(value); }
        }
        private AsyncEvent<GuildDeleteEventArgs> _guild_unavailable = new AsyncEvent<GuildDeleteEventArgs>();
        /// <summary>
        /// Sent when a message is created.
        /// </summary>
        public event AsyncEventHandler<MessageCreateEventArgs> MessageCreated
        {
            add { this._message_created.Register(value); }
            remove { this._message_created.Unregister(value); }
        }
        private AsyncEvent<MessageCreateEventArgs> _message_created = new AsyncEvent<MessageCreateEventArgs>();

        /// <summary>
        /// Sent when a presence has been updated.
        /// </summary>
        public event AsyncEventHandler<PresenceUpdateEventArgs> PresenceUpdate
        {
            add { this._presence_update.Register(value); }
            remove { this._presence_update.Unregister(value); }
        }
        private AsyncEvent<PresenceUpdateEventArgs> _presence_update = new AsyncEvent<PresenceUpdateEventArgs>();

        /// <summary>
        /// Sent when a guild ban gets added
        /// </summary>
        public event AsyncEventHandler<GuildBanAddEventArgs> GuildBanAdd
        {
            add { this._guild_ban_add.Register(value); }
            remove { this._guild_ban_add.Unregister(value); }
        }
        private AsyncEvent<GuildBanAddEventArgs> _guild_ban_add = new AsyncEvent<GuildBanAddEventArgs>();

        /// <summary>
        /// Sent when a guild ban gets removed
        /// </summary>
        public event AsyncEventHandler<GuildBanRemoveEventArgs> GuildBanRemove
        {
            add { this._guild_ban_remove.Register(value); }
            remove { this._guild_ban_remove.Unregister(value); }
        }
        private AsyncEvent<GuildBanRemoveEventArgs> _guild_ban_remove = new AsyncEvent<GuildBanRemoveEventArgs>();

        /// <summary>
        /// Sent when a guilds emojis get updated
        /// </summary>
        public event AsyncEventHandler<GuildEmojisUpdateEventArgs> GuildEmojisUpdate
        {
            add { this._guild_emojis_update.Register(value); }
            remove { this._guild_emojis_update.Unregister(value); }
        }
        private AsyncEvent<GuildEmojisUpdateEventArgs> _guild_emojis_update = new AsyncEvent<GuildEmojisUpdateEventArgs>();

        /// <summary>
        /// Sent when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdate
        {
            add { this._guild_integrations_update.Register(value); }
            remove { this._guild_integrations_update.Unregister(value); }
        }
        private AsyncEvent<GuildIntegrationsUpdateEventArgs> _guild_integrations_update = new AsyncEvent<GuildIntegrationsUpdateEventArgs>();

        /// <summary>
        /// Sent when a new user joins a guild.
        /// </summary>
        public event AsyncEventHandler<GuildMemberAddEventArgs> GuildMemberAdd
        {
            add { this._guild_member_add.Register(value); }
            remove { this._guild_member_add.Unregister(value); }
        }
        private AsyncEvent<GuildMemberAddEventArgs> _guild_member_add = new AsyncEvent<GuildMemberAddEventArgs>();
        /// <summary>
        /// Sent when a user is removed from a guild (leave/kick/ban).
        /// </summary>
        public event AsyncEventHandler<GuildMemberRemoveEventArgs> GuildMemberRemove
        {
            add { this._guild_member_remove.Register(value); }
            remove { this._guild_member_remove.Unregister(value); }
        }
        private AsyncEvent<GuildMemberRemoveEventArgs> _guild_member_remove = new AsyncEvent<GuildMemberRemoveEventArgs>();

        /// <summary>
        /// Sent when a guild member is updated.
        /// </summary>
        public event AsyncEventHandler<GuildMemberUpdateEventArgs> GuildMemberUpdate
        {
            add { this._guild_member_update.Register(value); }
            remove { this._guild_member_update.Unregister(value); }
        }
        private AsyncEvent<GuildMemberUpdateEventArgs> _guild_member_update = new AsyncEvent<GuildMemberUpdateEventArgs>();

        /// <summary>
        /// Sent when a guild role is created.
        /// </summary>
        public event AsyncEventHandler<GuildRoleCreateEventArgs> GuildRoleCreate
        {
            add { this._guild_role_create.Register(value); }
            remove { this._guild_role_create.Unregister(value); }
        }
        private AsyncEvent<GuildRoleCreateEventArgs> _guild_role_create = new AsyncEvent<GuildRoleCreateEventArgs>();

        /// <summary>
        /// Sent when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleUpdateEventArgs> GuildRoleUpdate
        {
            add { this._guild_role_update.Register(value); }
            remove { this._guild_role_update.Unregister(value); }
        }
        private AsyncEvent<GuildRoleUpdateEventArgs> _guild_role_update = new AsyncEvent<GuildRoleUpdateEventArgs>();

        /// <summary>
        /// Sent when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleDeleteEventArgs> GuildRoleDelete
        {
            add { this._guild_role_delete.Register(value); }
            remove { this._guild_role_delete.Unregister(value); }
        }
        private AsyncEvent<GuildRoleDeleteEventArgs> _guild_role_delete = new AsyncEvent<GuildRoleDeleteEventArgs>();

        /// <summary>
        /// Sent when a message is updated.
        /// </summary>
        public event AsyncEventHandler<MessageUpdateEventArgs> MessageUpdate
        {
            add { this._message_update.Register(value); }
            remove { this._message_update.Unregister(value); }
        }
        private AsyncEvent<MessageUpdateEventArgs> _message_update = new AsyncEvent<MessageUpdateEventArgs>();

        /// <summary>
        /// Sent when a message is deleted.
        /// </summary>
        public event AsyncEventHandler<MessageDeleteEventArgs> MessageDelete
        {
            add { this._message_delete.Register(value); }
            remove { this._message_delete.Unregister(value); }
        }
        private AsyncEvent<MessageDeleteEventArgs> _message_delete = new AsyncEvent<MessageDeleteEventArgs>();

        /// <summary>
        /// Sent when multiple messages are deleted at once.
        /// </summary>
        public event AsyncEventHandler<MessageBulkDeleteEventArgs> MessageBulkDelete
        {
            add { this._message_bulk_delete.Register(value); }
            remove { this._message_bulk_delete.Unregister(value); }
        }
        private AsyncEvent<MessageBulkDeleteEventArgs> _message_bulk_delete = new AsyncEvent<MessageBulkDeleteEventArgs>();

        /// <summary>
        /// Sent when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<TypingStartEventArgs> TypingStart
        {
            add { this._typing_start.Register(value); }
            remove { this._typing_start.Unregister(value); }
        }
        private AsyncEvent<TypingStartEventArgs> _typing_start = new AsyncEvent<TypingStartEventArgs>();

        /// <summary>
        /// Sent when the current user updates their settings.
        /// </summary>
        public event AsyncEventHandler<UserSettingsUpdateEventArgs> UserSettingsUpdate
        {
            add { this._user_settings_update.Register(value); }
            remove { this._user_settings_update.Unregister(value); }
        }
        private AsyncEvent<UserSettingsUpdateEventArgs> _user_settings_update = new AsyncEvent<UserSettingsUpdateEventArgs>();

        /// <summary>
        /// Sent when properties about the user change.
        /// </summary>
        public event AsyncEventHandler<UserUpdateEventArgs> UserUpdate
        {
            add { this._user_update.Register(value); }
            remove { this._user_update.Unregister(value); }
        }
        private AsyncEvent<UserUpdateEventArgs> _user_update = new AsyncEvent<UserUpdateEventArgs>();

        /// <summary>
        /// Sent when someone joins/leaves/moves voice channels.
        /// </summary>
        public event AsyncEventHandler<VoiceStateUpdateEventArgs> VoiceStateUpdate
        {
            add { this._voice_state_update.Register(value); }
            remove { this._voice_state_update.Unregister(value); }
        }
        private AsyncEvent<VoiceStateUpdateEventArgs> _voice_state_update = new AsyncEvent<VoiceStateUpdateEventArgs>();

        /// <summary>
        /// Sent when a guild's voice server is updated.
        /// </summary>
        public event AsyncEventHandler<VoiceServerUpdateEventArgs> VoiceServerUpdate
        {
            add { this._voice_server_update.Register(value); }
            remove { this._voice_server_update.Unregister(value); }
        }
        private AsyncEvent<VoiceServerUpdateEventArgs> _voice_server_update = new AsyncEvent<VoiceServerUpdateEventArgs>();

        /// <summary>
        /// Sent in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<GuildMembersChunkEventArgs> GuildMembersChunk
        {
            add { this._guild_members_chunk.Register(value); }
            remove { this._guild_members_chunk.Unregister(value); }
        }
        private AsyncEvent<GuildMembersChunkEventArgs> _guild_members_chunk = new AsyncEvent<GuildMembersChunkEventArgs>();

        /// <summary>
        /// Sent when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<UnknownEventArgs> UnknownEvent
        {
            add { this._unknown_event.Register(value); }
            remove { this._unknown_event.Unregister(value); }
        }
        private AsyncEvent<UnknownEventArgs> _unknown_event = new AsyncEvent<UnknownEventArgs>();

        /// <summary>
        /// Sent when a reaction gets added to a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionAddEventArgs> MessageReactionAdd
        {
            add { this._message_reaction_add.Register(value); }
            remove { this._message_reaction_add.Unregister(value); }
        }
        private AsyncEvent<MessageReactionAddEventArgs> _message_reaction_add = new AsyncEvent<MessageReactionAddEventArgs>();

        /// <summary>
        /// Sent when a reaction gets removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEventArgs> MessageReactionRemove
        {
            add { this._message_reaction_remove.Register(value); }
            remove { this._message_reaction_remove.Unregister(value); }
        }
        private AsyncEvent<MessageReactionRemoveEventArgs> _message_reaction_remove = new AsyncEvent<MessageReactionRemoveEventArgs>();

        /// <summary>
        /// Sent when all reactions get removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveAllEventArgs> MessageReactionRemoveAll
        {
            add { this._message_reaction_remove_all.Register(value); }
            remove { this._message_reaction_remove_all.Unregister(value); }
        }
        private AsyncEvent<MessageReactionRemoveAllEventArgs> _message_reaction_remove_all = new AsyncEvent<MessageReactionRemoveAllEventArgs>();

        public event AsyncEventHandler<WebhooksUpdateEventArgs> WebhooksUpdate
        {
            add { this._webhooks_update.Register(value); }
            remove { this._webhooks_update.Unregister(value); }
        }
        private AsyncEvent<WebhooksUpdateEventArgs> _webhooks_update = new AsyncEvent<WebhooksUpdateEventArgs>();

        public event AsyncEventHandler<HeartBeatEventArgs> HeartBeated
        {
            add { this._heart_beated.Register(value); }
            remove { this._heart_beated.Unregister(value); }
        }
        private AsyncEvent<HeartBeatEventArgs> _heart_beated = new AsyncEvent<HeartBeatEventArgs>();
        #endregion

        #region Internal Variables
        internal static CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        internal static CancellationToken _cancelToken = _cancelTokenSource.Token;

        internal static DiscordConfig config;

        internal static List<IModule> _modules = new List<IModule>();

        internal static BaseWebSocketClient _websocketClient;
        internal static int _sequence = 0;
        internal static string _sessionToken = "";
        internal static string _sessionID = "";
        internal static int _heartbeatInterval;
        internal Task _heartbeatThread;
        internal static DateTime _lastHeartbeat;
        internal static bool _waitingForAck = false;
        internal static UTF8Encoding UTF8 = new UTF8Encoding(false);

        internal static Dictionary<ulong, DiscordPresence> _presences = new Dictionary<ulong, DiscordPresence>();
        #endregion

        #region Public Variables
        internal static DebugLogger _debugLogger = new DebugLogger();
        /// <summary>
        /// 
        /// </summary>
        public DebugLogger DebugLogger => _debugLogger;

        internal static int _gatewayVersion;
        /// <summary>
        /// Gateway protocol version
        /// </summary>
        public int GatewayVersion => _gatewayVersion;

        internal static string _gatewayUrl = "";
        /// <summary>
        /// Gateway url
        /// </summary>
        public string GatewayUrl => _gatewayUrl;

        internal static int _shardCount = 1;
        /// <summary>
        /// Number of shards the bot is connected with
        /// </summary>
        public int Shards => _shardCount;

        internal static DiscordUser _me;
        /// <summary>
        /// The current user
        /// </summary>
        public DiscordUser Me => _me;

        internal static List<DiscordDMChannel> _privateChannels = new List<DiscordDMChannel>();
        /// <summary>
        /// List of DM Channels
        /// </summary>
        public IReadOnlyList<DiscordDMChannel> PrivateChannels => new ReadOnlyCollection<DiscordDMChannel>(_privateChannels);

        internal static Dictionary<ulong, DiscordGuild> _guilds = new Dictionary<ulong, DiscordGuild>();
        /// <summary>
        /// List of Guilds
        /// </summary>
        public Dictionary<ulong, DiscordGuild> Guilds => _guilds;

        /// <summary>
        /// Gets the WS latency for this client.
        /// </summary>
        public int Ping { get; internal set; }
        #endregion

        /// <summary>
        /// Intializes a new instance of DiscordClient
        /// </summary>
        public DiscordClient()
        {
            InternalSetup();
        }

        /// <summary>
        /// Initializes a new instance of DiscordClient
        /// </summary>
        /// <param name="config">Overwrites the default config</param>
        public DiscordClient(DiscordConfig config)
        {
            DiscordClient.config = config;
            InternalSetup();
        }

        public void SetSocketImplementation<T>() where T : BaseWebSocketClient, new()
        {
            BaseWebSocketClient.ClientType = typeof(T);
        }

        internal void InternalSetup()
        {
            if (config.UseInternalLogHandler)
                DebugLogger.LogMessageReceived += (sender, e) => DebugLogger.LogHandler(sender, e);
        }

        /// <summary>
        /// Connects to the gateway
        /// </summary>
        /// <returns></returns>
        public async Task Connect() => await InternalConnect();

        /// <summary>
        /// Connects to the gateway
        /// </summary>
        /// <param name="token_override"></param>
        /// <param name="token_type"></param>
        /// <returns></returns>
        public async Task Connect(string token_override, TokenType token_type)
        {
            config.Token = token_override;
            config.TokenType = token_type;

            await InternalConnect();
        }

        /// <summary>
        /// Adds a new module to the module list
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public IModule AddModule(IModule module)
        {
            module.Setup(this);
            _modules.Add(module);
            return module;
        }

        /// <summary>
        /// Gets a module from the module list by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetModule<T>() where T : class, IModule
        {
            return _modules.Find(x => x.GetType() == typeof(T)) as T;
        }

        public async Task Reconnect() => await InternalReconnect();

        public async Task Reconnect(string token_override, TokenType token_type) => await InternalReconnect(token_override, token_type);

        internal async Task InternalReconnect(bool start_new_session = false)
        {
            _cancelTokenSource.Cancel();
            await _websocketClient.InternalDisconnectAsync();
            if (start_new_session)
                _sessionID = "";
            // delay task by 2 seconds to make sure everything gets closed correctly
            await Task.Delay(2000);
            await InternalConnect();
        }

        internal async Task InternalReconnect(string token_override, TokenType token_type, bool start_new_session = false)
        {
            _cancelTokenSource.Cancel();
            await _websocketClient.InternalDisconnectAsync();
            if (start_new_session)
                _sessionID = "";
            // delay task by 2 seconds to make sure everything gets closed correctly
            await Task.Delay(2000);
            await Connect(token_override, token_type);
        }

        internal async Task InternalConnect()
        {
            await InternalUpdateGateway();
            _me = await InternalGetCurrentUser();

            _websocketClient = BaseWebSocketClient.Create();
            _websocketClient.OnConnect += async () =>
            {
                _privateChannels = new List<DiscordDMChannel>();
                _guilds = new Dictionary<ulong, DiscordGuild>();

                if (_sessionID == "")
                    await SendIdentify();
                else
                    await SendResume();
                await this._socket_opened.InvokeAsync();
            };
            _websocketClient.OnDisconnect += async () =>
            {
                _cancelTokenSource.Cancel();

                _debugLogger.LogMessage(LogLevel.Debug, "Websocket", $"Connection closed", DateTime.Now);

                if (config.AutoReconnect)
                {
                    await Reconnect();
                    DebugLogger.LogMessage(LogLevel.Critical, "Internal", "Bot crashed. Reconnecting", DateTime.Now);
                }
                await this._socket_closed.InvokeAsync();
            };
            _websocketClient.OnMessage += async e => await HandleSocketMessage(e.Message);
            await _websocketClient.ConnectAsync(_gatewayUrl + "?v=5&encoding=json");
        }

        internal async Task InternalUpdateGuild(DiscordGuild guild)
        {
            await Task.Run(() =>
            {
                if (Guilds[guild.ID] == null)
                    Guilds.Add(guild.ID, guild);
                else
                    Guilds[guild.ID] = guild;
            });
        }

        internal static async Task InternalUpdateGateway()
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Gateway;
            var headers = Utils.GetBaseHeaders();
            if (config.TokenType == TokenType.Bot)
                url += Endpoints.Bot;

            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);

            JObject jObj = JObject.Parse(response.Response);
            _gatewayUrl = jObj.Value<string>("url");
            if (jObj["shards"] != null)
                _shardCount = jObj.Value<int>("shards");
        }

        /// <summary>
        /// Disconnects from the gateway
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Disconnect()
        {
            return await Task.Run(async () =>
            {
                _cancelTokenSource.Cancel();
                await _websocketClient.InternalDisconnectAsync();
                config.AutoReconnect = false;
                return true;
            });
        }

        #region Public Functions
        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="user">userid or @me</param>
        /// <returns></returns>
        public async Task<DiscordUser> GetUser(string user) => await InternalGetUser(user);
        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="user">Id of the user</param>
        /// <returns></returns>
        public Task<DiscordUser> GetUser(ulong user) => InternalGetUser(user);
        /// <summary>
        /// Deletes a channel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteChannel(ulong id) => await InternalDeleteChannel(id);
        /// <summary>
        /// Deletes a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task DeleteChannel(DiscordChannel channel) => await InternalDeleteChannel(channel.ID);
        /// <summary>
        /// Gets a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message_id"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> GetMessage(DiscordChannel channel, ulong message_id) => await InternalGetMessage(channel.ID, message_id);
        /// <summary>
        /// Gets a message
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="message_id"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> GetMessage(ulong channel_id, ulong message_id) => await InternalGetMessage(channel_id, message_id);
        /// <summary>
        /// Gets a channel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordChannel> GetChannel(ulong id) => await InternalGetChannel(id);
        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> SendMessage(ulong channel_id, string content, bool tts = false, DiscordEmbed embed = null) => await InternalCreateMessage(channel_id, content, tts, embed);
        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> SendMessage(DiscordChannel channel, string content, bool tts = false, DiscordEmbed embed = null) => await InternalCreateMessage(channel.ID, content, tts, embed);
        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> SendMessage(DiscordDMChannel channel, string content, bool tts = false, DiscordEmbed embed = null) => await InternalCreateMessage(channel.ID, content, tts, embed);
        /// <summary>
        /// Creates a guild. Only for whitelisted bots
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<DiscordGuild> CreateGuild(string name) => await InternalCreateGuildAsync(name);
        /// <summary>
        /// Gets a guild
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordGuild> GetGuild(ulong id) => await InternalGetGuild(id);
        /// <summary>
        /// Deletes a guild
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordGuild> DeleteGuild(ulong id) => await InternalDeleteGuild(id);
        /// <summary>
        /// Deletes a guild
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task<DiscordGuild> DeleteGuild(DiscordGuild guild) => await InternalDeleteGuild(guild.ID);
        /// <summary>
        /// Gets a channel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordChannel> GetChannelByID(ulong id) => await InternalGetChannel(id);
        /// <summary>
        /// Gets an invite
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<DiscordInvite> GetInviteByCode(string code) => await InternalGetInvite(code);
        /// <summary>
        /// Gets a list of connections
        /// </summary>
        /// <returns></returns>
        public async Task<List<DiscordConnection>> GetConnections() => await InternalGetUsersConnections();
        /// <summary>
        /// Gets a list of regions
        /// </summary>
        /// <returns></returns>
        public async Task<List<DiscordVoiceRegion>> ListRegions() => await InternalListVoiceRegions();
        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordWebhook> GetWebhook(ulong id) => await InternalGetWebhook(id);
        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DiscordWebhook> GetWebhookWithToken(ulong id, string token) => await InternalGetWebhookWithToken(id, token);
        /// <summary>
        /// Creates a dm
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public async Task<DiscordDMChannel> CreateDM(ulong user_id) => await InternalCreateDM(user_id);
        /// <summary>
        /// Updates current user's status
        /// </summary>
        /// <param name="game">Game you're playing</param>
        /// <param name="idle_since"></param>
        /// <returns></returns>
        public async Task UpdateStatus(string game = "", int idle_since = -1) => await InternalUpdateStatus(game, idle_since);

        /// <summary>
        /// Modifies a guild member
        /// </summary>
        /// <param name="guild_id">Guild's ID</param>
        /// <param name="member_id">Member's ID</param>
        /// <param name="nickname">Member's (new) Nickname</param>
        /// <param name="roles">Member's roles</param>
        /// <param name="muted">Wether this member has been muted or not (voice)</param>
        /// <param name="deaf">Wether this member has been deafened or not (voice)</param>
        /// <param name="voicechannel_id">Voice channel ID for moving this user around</param>
        /// <returns></returns>
        public async Task ModifyMember(ulong guild_id, ulong member_id, string nickname = null, List<ulong> roles = null, bool muted = false, bool deaf = false, ulong voicechannel_id = 0) =>
            await InternalModifyGuildMember(guild_id, member_id, nickname, roles, muted, deaf, voicechannel_id);

        /// <summary>
        /// Gets the current API appication.
        /// </summary>
        /// <returns></returns>
        public async Task<DiscordApplication> GetCurrentApp() => await InternalGetApplicationInfo("@me");

        /// <summary>
        /// Gets cached presence for a user.
        /// </summary>
        /// <param name="user_id">User's ID</param>
        /// <returns></returns>
        public DiscordPresence GetUserPresence(ulong user_id) => InternalGetUserPresence(user_id);
        /// <summary>
        /// Lists guild members
        /// </summary>
        /// <param name="guild_id">Guild's ID</param>
        /// <param name="limit">limit of members to return</param>
        /// <param name="after">index to start from</param>
        /// <returns></returns>
        public async Task<List<DiscordMember>> ListGuildMembers(ulong guild_id, int limit, int after) => await InternalListGuildMembers(guild_id, limit, after);

        /// <summary>
        /// Sets bot avatar
        /// </summary>
        /// <param name="img">Stream with image data. Can contain a PNG, JPG, or GIF file.</param>
        /// <returns></returns>
        public async Task SetAvatar(Stream img) => await InternalSetAvatar(img);
        #endregion

        #region Websocket
        internal async Task HandleSocketMessage(string data)
        {
            JObject obj = JObject.Parse(data);
            switch (obj.Value<int>("op"))
            {
                case 0: await OnDispatch(obj); break;
                case 1: await OnHeartbeat((long)obj["d"]); break;
                case 7: await OnReconnect(); break;
                case 9: await OnInvalidateSession(obj); break;
                case 10: await OnHello(obj); break;
                case 11: await OnHeartbeatAck(); break;
                default:
                    {
                        DebugLogger.LogMessage(LogLevel.Warning, "Websocket", $"Unknown OP-Code: {obj.Value<int>("op")}\n{obj}", DateTime.Now);
                        break;
                    }
            }
        }

        internal async Task OnDispatch(JObject obj)
        {
            switch (obj.Value<string>("t").ToLower())
            {
                case "ready": await OnReadyEvent(obj); break;
                case "channel_create": await OnChannelCreateEvent(obj); break;
                case "channel_update": await OnChannelUpdateEvent(obj); break;
                case "channel_delete": await OnChannelDeleteEvent(obj); break;
                case "guild_create": await OnGuildCreateEvent(obj); break;
                case "guild_update": await OnGuildUpdateEvent(obj); break;
                case "guild_delete": await OnGuildDeleteEvent(obj); break;
                case "guild_ban_add": await OnGuildBanAddEvent(obj); break;
                case "guild_ban_remove": await OnGuildBanRemoveEvent(obj); break;
                case "guild_emojis_update": await OnGuildEmojisUpdateEvent(obj); break;
                case "guild_integrations_update": await OnGuildIntegrationsUpdateEvent(obj); break;
                case "guild_member_add": await OnGuildMemberAddEvent(obj); break;
                case "guild_member_remove": await OnGuildMemberRemoveEvent(obj); break;
                case "guild_member_update": await OnGuildMemberUpdateEvent(obj); break;
                case "guild_member_chunk": await OnGuildMembersChunkEvent(obj); break;
                case "guild_role_create": await OnGuildRoleCreateEvent(obj); break;
                case "guild_role_update": await OnGuildRoleUpdateEvent(obj); break;
                case "guild_role_delete": await OnGuildRoleDeleteEvent(obj); break;
                case "message_create": await OnMessageCreateEvent(obj); break;
                case "message_update": await OnMessageUpdateEvent(obj); break;
                case "message_delete": await OnMessageDeleteEvent(obj); break;
                case "message_delete_bulk": await OnMessageBulkDeleteEvent(obj); break;
                case "presence_update": await OnPresenceUpdateEvent(obj); break;
                case "typing_start": await OnTypingStartEvent(obj); break;
                case "user_settings_update": await OnUserSettingsUpdateEvent(obj); break;
                case "user_update": await OnUserUpdateEvent(obj); break;
                case "voice_state_update": await OnVoiceStateUpdateEvent(obj); break;
                case "voice_server_update": await OnVoiceServerUpdateEvent(obj); break;
                case "message_reaction_add": await OnMessageReactionAdd(obj); break;
                case "message_reaction_remove": await OnMessageReactionRemove(obj); break;
                case "message_reaction_remove_all": await OnMessageReactionRemoveAll(obj); break;
                case "webhooks_update": await OnWebhooksUpdate(obj); break;
                default:
                    await OnUnknownEvent(obj);
                    DebugLogger.LogMessage(LogLevel.Warning, "Websocket", $"Unknown event: {obj.Value<string>("t")}\n{obj["d"]}", DateTime.Now);
                    break;
            }
        }

        #region Events
        internal async Task OnReadyEvent(JObject obj)
        {
            _gatewayVersion = obj["d"]["v"].ToObject<int>();
            _me = obj["d"]["user"].ToObject<DiscordUser>();
            _privateChannels = obj["d"]["private_channels"].ToObject<List<DiscordDMChannel>>();
            if (config.TokenType != TokenType.User)
            {
                foreach (JObject guild in obj["d"]["guilds"])
                {
                    if (!_guilds.ContainsKey(guild.Value<ulong>("id")))
                        _guilds.Add(guild.Value<ulong>("id"), guild.ToObject<DiscordGuild>());
                }
            }
            _sessionID = obj["d"]["session_id"].ToString();

            await this._ready.InvokeAsync();
        }
        internal async Task OnChannelCreateEvent(JObject obj)
        {
            if (obj["d"]["is_private"] != null && obj["d"]["is_private"].ToObject<bool>())
            {
                DiscordDMChannel channel = obj["d"].ToObject<DiscordDMChannel>();
                _privateChannels.Add(channel);

                await this._dm_channel_created.InvokeAsync(new DMChannelCreateEventArgs { Channel = channel });
            }
            else
            {
                DiscordChannel channel = obj["d"].ToObject<DiscordChannel>();

                _guilds[channel.GuildID].Channels.Add(channel);

                await this._channel_created.InvokeAsync(new ChannelCreateEventArgs { Channel = channel, Guild = _guilds[channel.GuildID] });
            }
        }
        internal async Task OnChannelUpdateEvent(JObject obj)
        {
            DiscordChannel channel = obj["d"].ToObject<DiscordChannel>();
            DiscordChannel old = channel;

            if (_guilds != null && _guilds.ContainsKey(channel.GuildID)
            && _guilds[channel.GuildID]?.Channels?.Find(x => x.ID == channel.ID) != null)
            {
                int channelIndex = _guilds[channel.GuildID].Channels.FindIndex(x => x.ID == channel.ID);
                old = _guilds[channel.GuildID].Channels[channelIndex];
                _guilds[channel.GuildID].Channels[channelIndex] = channel;
            }
            else
            {
                if (_guilds[channel.GuildID].Channels == null)
                    _guilds[channel.GuildID].Channels = new List<DiscordChannel>();
                _guilds[channel.GuildID].Channels.Add(channel);
            }

            await this._channel_updated.InvokeAsync(new ChannelUpdateEventArgs { Channel = channel, Guild = _guilds[channel.GuildID], ChannelBefore = old });
        }
        internal async Task OnChannelDeleteEvent(JObject obj)
        {
            if (obj["d"]["is_private"] != null && obj["d"]["is_private"].ToObject<bool>())
            {
                DiscordDMChannel channel = obj["d"].ToObject<DiscordDMChannel>();
                int channelIndex = _privateChannels.FindIndex(x => x.ID == channel.ID);
                _privateChannels.RemoveAt(channelIndex);

                await this._dm_channel_deleted.InvokeAsync(new DMChannelDeleteEventArgs { Channel = channel });
            }
            else
            {
                DiscordChannel channel = obj["d"].ToObject<DiscordChannel>();
                _guilds[channel.GuildID].Channels.RemoveAll(x => x.ID == channel.ID);

                await this._channel_deleted.InvokeAsync(new ChannelDeleteEventArgs { Channel = channel, Guild = _guilds[channel.GuildID] });
            }
        }
        internal async Task OnGuildCreateEvent(JObject obj)
        {
            DiscordGuild guild = obj["d"].ToObject<DiscordGuild>();

            foreach (DiscordChannel channel in guild.Channels)
                if (channel.GuildID == 0) channel.GuildID = guild.ID;

            foreach (DiscordPresence Presence in guild.Presences)
            {
                if (_presences.ContainsKey(Presence.UserID))
                    _presences.Remove(Presence.UserID);
                if (_presences != null && Presence != null)
                {
                    try
                    {
                        _presences.Add(Presence.UserID, Presence);
                    }
                    catch (NullReferenceException)
                    {

                    }
                }
            }

            if (_guilds.ContainsKey(obj["d"].Value<ulong>("id")))
            {
                _guilds[guild.ID] = guild;

                await this._guild_available.InvokeAsync(new GuildCreateEventArgs { Guild = guild });
            }
            else
            {
                _guilds.Add(guild.ID, guild);

                await this._guild_created.InvokeAsync(new GuildCreateEventArgs { Guild = guild });
            }
        }
        internal async Task OnGuildUpdateEvent(JObject obj)
        {
            DiscordGuild guild = _guilds[obj["d"].Value<ulong>("id")];
            if (guild != null)
            {
                guild = obj["d"].ToObject<DiscordGuild>();
                _guilds[guild.ID] = guild;
            }
            else
            {
                guild = obj["d"].ToObject<DiscordGuild>();
                _guilds.Add(guild.ID, guild);
            }
            await this._guild_updated.InvokeAsync(new GuildUpdateEventArgs { Guild = guild });
        }
        internal async Task OnGuildDeleteEvent(JObject obj)
        {
            if (_guilds.ContainsKey(obj["d"].Value<ulong>("id")))
            {
                if (obj["d"]["unavailable"] != null)
                {
                    DiscordGuild guild = obj["d"].ToObject<DiscordGuild>();

                    _guilds[guild.ID] = guild;

                    await this._guild_unavailable.InvokeAsync(new GuildDeleteEventArgs { ID = obj["d"].Value<ulong>("id"), Unavailable = true });
                }
                else
                {
                    _guilds.Remove(obj["d"].Value<ulong>("id"));

                    await this._guild_deleted.InvokeAsync(new GuildDeleteEventArgs { ID = obj["d"].Value<ulong>("id") });
                }
            }
        }
        internal async Task OnPresenceUpdateEvent(JObject obj)
        {
            ulong userID = (ulong)obj["d"]["user"]["id"];
            DiscordPresence p = obj["d"].ToObject<DiscordPresence>();
            DiscordPresence old = p;
            if (_presences.ContainsKey(userID))
            {
                old = _presences[userID];
                _presences[userID] = p;
            }
            else 
                _presences.Add(userID, p);

            PresenceUpdateEventArgs args = obj["d"].ToObject<PresenceUpdateEventArgs>();
            args.PresenceBefore = old;
            await this._presence_update.InvokeAsync(args);
        }
        internal async Task OnGuildBanAddEvent(JObject obj)
        {
            DiscordUser user = obj["d"].ToObject<DiscordUser>();
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            GuildBanAddEventArgs args = new GuildBanAddEventArgs { User = user, GuildID = guildID };
            await this._guild_ban_add.InvokeAsync(args);
        }
        internal async Task OnGuildBanRemoveEvent(JObject obj)
        {
            DiscordUser user = obj["d"].ToObject<DiscordUser>();
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            GuildBanRemoveEventArgs args = new GuildBanRemoveEventArgs { User = user, GuildID = guildID };
            await this._guild_ban_remove.InvokeAsync(args);
        }
        internal async Task OnGuildEmojisUpdateEvent(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            List<DiscordEmoji> emojis = new List<DiscordEmoji>();
            foreach (JObject em in (JArray)obj["d"]["emojis"])
            {
                emojis.Add(em.ToObject<DiscordEmoji>());
            }
            List<DiscordEmoji> old = _guilds[guildID].Emojis;
            _guilds[guildID].Emojis = emojis;
            GuildEmojisUpdateEventArgs args = new GuildEmojisUpdateEventArgs { GuildID = guildID, Emojis = new ReadOnlyCollection<DiscordEmoji>(emojis), EmojisBefore = new ReadOnlyCollection<DiscordEmoji>(old) };
            await this._guild_emojis_update.InvokeAsync(args);
        }
        internal async Task OnGuildIntegrationsUpdateEvent(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            GuildIntegrationsUpdateEventArgs args = new GuildIntegrationsUpdateEventArgs { GuildID = guildID };
            await this._guild_integrations_update.InvokeAsync(args);
        }
        internal async Task OnGuildMemberAddEvent(JObject obj)
        {
            DiscordMember user = obj["d"].ToObject<DiscordMember>();
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            _guilds[guildID].Members.Add(user);
            _guilds[guildID].MemberCount = _guilds[guildID].Members.Count;
            GuildMemberAddEventArgs args = new GuildMemberAddEventArgs { Member = user, GuildID = guildID };
            await this._guild_member_add.InvokeAsync(args);
        }
        internal async Task OnGuildMemberRemoveEvent(JObject obj)
        {
            DiscordUser user = obj["d"]["user"].ToObject<DiscordUser>();
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            if (_guilds[guildID].Members.Find(x => x.User.ID == user.ID) != null)
            {
                int index = _guilds[guildID].Members.FindIndex(x => x.User.ID == user.ID);
                _guilds[guildID].Members.RemoveAt(index);
                _guilds[guildID].MemberCount = _guilds[guildID].Members.Count;
            }
            GuildMemberRemoveEventArgs args = new GuildMemberRemoveEventArgs { User = user, GuildID = guildID };
            await this._guild_member_remove.InvokeAsync(args);
        }
        internal async Task OnGuildMemberUpdateEvent(JObject obj)
        {
            DiscordUser user = obj["d"]["user"].ToObject<DiscordUser>();
            DiscordMember old = new DiscordMember();
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            string nick = "";
            nick = obj["d"]["nick"].ToString();
            List<ulong> roles = new List<ulong>();
            if (obj["d"]["roles"] != null)
            {
                JArray rolesjson = (JArray)obj["d"]["roles"];
                foreach (var role in rolesjson)
                {
                    roles.Add(ulong.Parse(role.ToString()));
                }
            }
            int index = _guilds[guildID].Members.FindIndex(x => x.User.ID == user.ID);
            if (_guilds[guildID].Members.Find(x => x.User.ID == user.ID) != null)
            {
                DiscordMember m = _guilds[guildID].Members[index];
                old = m;
                m.Nickname = nick;
                m.Roles = roles;
                _guilds[guildID].Members[index] = m;
            }
            else
            {
                DiscordMember m = new DiscordMember()
                {
                    User = user,
                    Nickname = nick,
                    Roles = roles,
                };
                _guilds[guildID].Members.Add(m);
            }
            GuildMemberUpdateEventArgs args = new GuildMemberUpdateEventArgs { User = user, GuildID = guildID, Roles = new ReadOnlyCollection<ulong>(roles), NickName = nick, NickNameBefore = old.Nickname, RolesBefore = old.Roles };
            await this._guild_member_update.InvokeAsync(args);
        }
        internal async Task OnGuildRoleCreateEvent(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            DiscordRole role = obj["d"]["role"].ToObject<DiscordRole>();
            _guilds[guildID].Roles.Add(role);
            GuildRoleCreateEventArgs args = new GuildRoleCreateEventArgs { GuildID = guildID, Role = role };
            await this._guild_role_create.InvokeAsync(args);
        }
        internal async Task OnGuildRoleUpdateEvent(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            DiscordRole role = obj["d"]["role"].ToObject<DiscordRole>();
            DiscordRole old = role;
            int index = _guilds[guildID].Roles.FindIndex(x => x.ID == role.ID);
            old = _guilds[guildID].Roles[index];
            _guilds[guildID].Roles[index] = role;
            GuildRoleUpdateEventArgs args = new GuildRoleUpdateEventArgs { GuildID = guildID, Role = role, RoleBefore = old };
            await this._guild_role_update.InvokeAsync(args);
        }
        internal async Task OnGuildRoleDeleteEvent(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            ulong roleID = obj["d"]["role_id"].ToObject<ulong>();
            int index = _guilds[guildID].Roles.FindIndex(x => x.ID == roleID);
            _guilds[guildID].Roles.RemoveAt(index);
            GuildRoleDeleteEventArgs args = new GuildRoleDeleteEventArgs { GuildID = guildID, RoleID = roleID };
            await this._guild_role_delete.InvokeAsync(args);
        }
        internal async Task OnMessageCreateEvent(JObject obj)
        {
            DiscordMessage message;
            try
            {
                message = obj["d"].ToObject<DiscordMessage>();
            }
            catch (JsonSerializationException)
            {
                JObject msg = (JObject)obj["d"];
                msg["nonce"] = 0;
                message = msg.ToObject<DiscordMessage>();
            }

            try
            {
                if (_privateChannels.Find(x => x.ID == message.ChannelID) == null)
                {
                    int channelindex = _guilds[message.Parent.Parent.ID].Channels.FindIndex(x => x.ID == message.ChannelID);
                    _guilds[message.Parent.Parent.ID].Channels[channelindex].LastMessageID = message.ID;
                }
                else
                {
                    int channelindex = _privateChannels.FindIndex(x => x.ID == message.ChannelID);
                    _privateChannels[channelindex].LastMessageID = message.ID;
                }
            }
            catch (KeyNotFoundException)
            {
                DebugLogger.LogMessage(LogLevel.Error, "Event", "Could not find channel last message belonged to?", DateTime.Now);
            }

            List<DiscordMember> MentionedUsers = new List<DiscordMember>();
            List<DiscordRole> MentionedRoles = new List<DiscordRole>();
            List<DiscordChannel> MentionedChannels = new List<DiscordChannel>();
            List<DiscordEmoji> UsedEmojis = new List<DiscordEmoji>();
            if (message.Content != null && message.Content != "" && _guilds.ContainsKey(message.Parent.Parent.ID))
            {
                foreach (ulong user in Utils.GetUserMentions(message))
                {
                    if (message.Parent != null && message.Parent.Parent != null && _guilds[message.Parent.Parent.ID] != null
                    && _guilds[message.Parent.Parent.ID].Members != null
                    && _guilds[message.Parent.Parent.ID].Members.Find(x => x.User.ID == user) != null)
                        MentionedUsers.Add(_guilds[message.Parent.Parent.ID].Members.Find(x => x.User.ID == user));
                }

                foreach (ulong role in Utils.GetRoleMentions(message))
                {
                    if (message.Parent != null && message.Parent.Parent != null && _guilds[message.Parent.Parent.ID] != null
                    && _guilds[message.Parent.Parent.ID].Roles != null
                    && _guilds[message.Parent.Parent.ID].Roles.Find(x => x.ID == role) != null)
                        MentionedRoles.Add(_guilds[message.Parent.Parent.ID].Roles.Find(x => x.ID == role));
                }

                foreach (ulong channel in Utils.GetChannelMentions(message))
                {
                    if (message.Parent != null && message.Parent.Parent != null && _guilds[message.Parent.Parent.ID] != null
                    && _guilds[message.Parent.Parent.ID].Channels != null
                    && _guilds[message.Parent.Parent.ID].Channels.Find(x => x.ID == channel) != null)
                        MentionedChannels.Add(_guilds[message.Parent.Parent.ID].Channels.Find(x => x.ID == channel));
                }
                /*
                foreach (ulong emoji in Utils.GetEmojis(message))
                {
                    if (message.Parent != null && message.Parent.Parent != null && _guilds[message.Parent.Parent.ID] != null
                    && _guilds[message.Parent.Parent.ID].Emojis != null
                    && _guilds[message.Parent.Parent.ID].Emojis.Find(x => x.ID == emoji) != null)
                        UsedEmojis.Add(_guilds[message.Parent.Parent.ID].Emojis.Find(x => x.ID == emoji));
                }
                */
            }
            MessageCreateEventArgs args = new MessageCreateEventArgs
            {
                Message = message,
                MentionedUsers = new ReadOnlyCollection<DiscordMember>(MentionedUsers),
                MentionedRoles = new ReadOnlyCollection<DiscordRole>(MentionedRoles),
                MentionedChannels = new ReadOnlyCollection<DiscordChannel>(MentionedChannels),
                UsedEmojis = new ReadOnlyCollection<DiscordEmoji>(UsedEmojis)
            };
            await this._message_created.InvokeAsync(args);
        }
        internal async Task OnMessageUpdateEvent(JObject obj)
        {
            DiscordMessage message;
            message = obj["d"].ToObject<DiscordMessage>();

            List<DiscordMember> MentionedUsers = new List<DiscordMember>();
            List<DiscordRole> MentionedRoles = new List<DiscordRole>();
            List<DiscordChannel> MentionedChannels = new List<DiscordChannel>();
            List<DiscordEmoji> UsedEmojis = new List<DiscordEmoji>();
            if (message.Content != null && message.Content != "" && _guilds.ContainsKey(message.Parent.Parent.ID))
            {
                foreach (ulong user in Utils.GetUserMentions(message))
                {
                    if (message.Parent != null && message.Parent.Parent != null && _guilds[message.Parent.Parent.ID] != null
                    && _guilds[message.Parent.Parent.ID].Members != null
                    && _guilds[message.Parent.Parent.ID].Members.Find(x => x.User.ID == user) != null)
                        MentionedUsers.Add(_guilds[message.Parent.Parent.ID].Members.Find(x => x.User.ID == user));
                }

                foreach (ulong role in Utils.GetRoleMentions(message))
                {
                    if (message.Parent != null && message.Parent.Parent != null && _guilds[message.Parent.Parent.ID] != null
                    && _guilds[message.Parent.Parent.ID].Roles != null
                    && _guilds[message.Parent.Parent.ID].Roles.Find(x => x.ID == role) != null)
                        MentionedRoles.Add(_guilds[message.Parent.Parent.ID].Roles.Find(x => x.ID == role));
                }

                foreach (ulong channel in Utils.GetChannelMentions(message))
                {
                    if (message.Parent != null && message.Parent.Parent != null && _guilds[message.Parent.Parent.ID] != null
                    && _guilds[message.Parent.Parent.ID].Channels != null
                    && _guilds[message.Parent.Parent.ID].Channels.Find(x => x.ID == channel) != null)
                        MentionedChannels.Add(_guilds[message.Parent.Parent.ID].Channels.Find(x => x.ID == channel));
                }
                /*
                foreach (ulong emoji in Utils.GetEmojis(message))
                {
                    if (message.Parent != null && message.Parent.Parent != null && _guilds[message.Parent.Parent.ID] != null
                    && _guilds[message.Parent.Parent.ID].Emojis != null
                    && _guilds[message.Parent.Parent.ID].Emojis.Find(x => x.ID == emoji) != null)
                        UsedEmojis.Add(_guilds[message.Parent.Parent.ID].Emojis.Find(x => x.ID == emoji));
                }
                */
            }

            MessageUpdateEventArgs args = new MessageUpdateEventArgs
            {
                Message = message,
                MentionedUsers = new ReadOnlyCollection<DiscordMember>(MentionedUsers),
                MentionedRoles = new ReadOnlyCollection<DiscordRole>(MentionedRoles),
                MentionedChannels = new ReadOnlyCollection<DiscordChannel>(MentionedChannels),
                UsedEmojis = new ReadOnlyCollection<DiscordEmoji>(UsedEmojis)
            };
            await this._message_update.InvokeAsync(args);
        }
        internal async Task OnMessageDeleteEvent(JObject obj)
        {
            ulong ID = ulong.Parse(obj["d"]["id"].ToString());
            ulong channelID = ulong.Parse(obj["d"]["channel_id"].ToString());
            MessageDeleteEventArgs args = new MessageDeleteEventArgs { ChannelID = channelID, MessageID = ID };
            await this._message_delete.InvokeAsync(args);
        }
        internal async Task OnMessageBulkDeleteEvent(JObject obj)
        {
            JArray IDsJson = (JArray)obj["d"]["ids"];
            List<ulong> ids = new List<ulong>();
            foreach (JToken t in IDsJson)
            {
                ids.Add(ulong.Parse(t.ToString()));
            }
            ulong channelID = ulong.Parse(obj["d"]["channel_id"].ToString());
            MessageBulkDeleteEventArgs args = new MessageBulkDeleteEventArgs { MessageIDs = new ReadOnlyCollection<ulong>(ids), ChannelID = channelID };
            await this._message_bulk_delete.InvokeAsync(args);
        }
        internal async Task OnTypingStartEvent(JObject obj)
        {
            ulong channelID = ulong.Parse(obj["d"]["channel_id"].ToString());
            ulong userID = ulong.Parse(obj["d"]["user_id"].ToString());
            TypingStartEventArgs args = new TypingStartEventArgs { ChannelID = channelID, UserID = userID };
            await this._typing_start.InvokeAsync(args);
        }
        internal async Task OnUserSettingsUpdateEvent(JObject obj)
        {
            DiscordUser user = obj["d"].ToObject<DiscordUser>();
            UserSettingsUpdateEventArgs args = new UserSettingsUpdateEventArgs { User = user };
            await this._user_settings_update.InvokeAsync(args);
        }
        internal async Task OnUserUpdateEvent(JObject obj)
        {
            DiscordUser user = obj["d"].ToObject<DiscordUser>();
            UserUpdateEventArgs args = new UserUpdateEventArgs { User = user, UserBefore = _me };
            _me = user;
            await this._user_update.InvokeAsync(args);
        }
        internal async Task OnVoiceStateUpdateEvent(JObject obj)
        {
            ulong userID = ulong.Parse(obj["d"]["user_id"].ToString());
            ulong guildID = 0;
            if (obj["d"]["guild_id"] != null)
                ulong.TryParse(obj["d"]["guild_id"].ToString(), out guildID);
            ulong channelID = 0;
            if (obj["d"]["channel_id"] != null)
                ulong.TryParse(obj["d"]["channel_id"].ToString(), out channelID);
            string session_id = obj["d"]["session_id"].ToString();
            VoiceStateUpdateEventArgs args = new VoiceStateUpdateEventArgs { UserID = userID, GuildID = guildID, SessionID = session_id };

            var gld = this.Guilds.ContainsKey(guildID) ? this.Guilds[guildID] : null;

            if (gld != null)
            {
                var vs = new DiscordVoiceState
                {
                    GuildID = gld.ID,
                    ChannelID = channelID,
                    UserID = userID
                };
                var vs1 = gld.VoiceStates.FirstOrDefault(xvs => xvs.UserID == userID);
                if (vs1 != null)
                    gld.VoiceStates.Remove(vs1);
                if (channelID != 0)
                    gld.VoiceStates.Add(vs);
            }

            await this._voice_state_update.InvokeAsync(args);
        }
        internal async Task OnVoiceServerUpdateEvent(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            string endpoint = obj["d"]["endpoint"].ToString();
            string token = obj["d"]["token"].ToString();

            VoiceServerUpdateEventArgs args = new VoiceServerUpdateEventArgs { GuildID = guildID, Endpoint = endpoint, VoiceToken = token };
            await this._voice_server_update.InvokeAsync(args);
            //await _voiceClient.Init(token, guildID, endpoint);
        }
        internal async Task OnGuildMembersChunkEvent(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            List<DiscordMember> members = new List<DiscordMember>();
            foreach (JObject mem in (JArray)obj["d"]["members"])
            {
                members.Add(mem.ToObject<DiscordMember>());
            }
            _guilds[guildID].Members = members;
            _guilds[guildID].MemberCount = members.Count;
            GuildMembersChunkEventArgs args = new GuildMembersChunkEventArgs { GuildID = guildID, Members = new ReadOnlyCollection<DiscordMember>(members) };
            await this._guild_members_chunk.InvokeAsync(args);
        }

        internal async Task OnUnknownEvent(JObject obj)
        {
            string name = obj["t"].ToString();
            string json = obj["d"].ToString();
            UnknownEventArgs args = new UnknownEventArgs { EventName = name, Json = json };
            await this._unknown_event.InvokeAsync(args);
        }

        internal async Task OnMessageReactionAdd(JObject obj)
        {
            ulong channelid = ulong.Parse(obj["d"]["channel_id"].ToString());
            ulong messageid = ulong.Parse(obj["d"]["message_id"].ToString());
            ulong userid = ulong.Parse(obj["d"]["user_id"].ToString());
            DiscordEmoji emoji = obj["d"]["emoji"].ToObject<DiscordEmoji>();
            MessageReactionAddEventArgs args = new MessageReactionAddEventArgs
            {
                ChannelID = channelid,
                MessageID = messageid,
                UserID = userid,
                Emoji = emoji
            };
            await this._message_reaction_add.InvokeAsync(args);
        }

        internal async Task OnMessageReactionRemove(JObject obj)
        {
            ulong channelid = ulong.Parse(obj["d"]["channel_id"].ToString());
            ulong messageid = ulong.Parse(obj["d"]["message_id"].ToString());
            ulong userid = ulong.Parse(obj["d"]["user_id"].ToString());
            DiscordEmoji emoji = obj["d"]["emoji"].ToObject<DiscordEmoji>();
            MessageReactionRemoveEventArgs args = new MessageReactionRemoveEventArgs
            {
                ChannelID = channelid,
                MessageID = messageid,
                UserID = userid,
                Emoji = emoji
            };
            await this._message_reaction_remove.InvokeAsync(args);
        }

        internal async Task OnMessageReactionRemoveAll(JObject obj)
        {
            ulong channelid = ulong.Parse(obj["d"]["channel_id"].ToString());
            ulong messageid = ulong.Parse(obj["d"]["message_id"].ToString());
            MessageReactionRemoveAllEventArgs args = new MessageReactionRemoveAllEventArgs
            {
                ChannelID = channelid,
                MessageID = messageid
            };
            await this._message_reaction_remove_all.InvokeAsync(args);
        }

        internal async Task OnWebhooksUpdate(JObject obj)
        {
            ulong channelid = ulong.Parse(obj["d"]["channel_id"].ToString());
            ulong guildid = ulong.Parse(obj["d"]["guild_id"].ToString());
            WebhooksUpdateEventArgs args = new WebhooksUpdateEventArgs
            {
                ChannelID = channelid,
                GuildID = guildid
            };
            await this._webhooks_update.InvokeAsync(args);
        }
        #endregion

        internal async Task OnHeartbeat(long seq)
        {
            _debugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received Heartbeat - Sending Ack.", DateTime.Now);
            await SendHeartbeat(seq);
        }

        internal async Task OnReconnect()
        {
            await Task.Run(async () =>
            {
                _debugLogger.LogMessage(LogLevel.Info, "Websocket", "Received OP 7 - Reconnect. ", DateTime.Now);

                await InternalReconnect();
            });
        }

        internal async Task OnInvalidateSession(JObject obj)
        {
            if (obj.Value<bool>("d"))
            {
                _debugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received true in OP 9 - Waiting a few second and sending resume again.", DateTime.Now);
                await Task.Delay(5000);
                await SendResume();
            }
            else
            {
                _debugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received false in OP 9 - Starting a new session", DateTime.Now);
                _sessionID = "";
                await InternalReconnect(true);
            }
        }

        internal async Task OnHello(JObject obj)
        {
            await Task.Run(() =>
            {
                _waitingForAck = false;
                _heartbeatInterval = obj["d"].Value<int>("heartbeat_interval");
                _heartbeatThread = new Task(StartHeartbeating, _cancelToken, TaskCreationOptions.LongRunning);
                _heartbeatThread.Start();
            });
        }

        internal async Task OnHeartbeatAck()
        {
            await Task.Run(() =>
            {
                _waitingForAck = false;

                this.Ping = (int)(DateTime.Now - _lastHeartbeat).TotalMilliseconds;
                _debugLogger.LogMessage(LogLevel.Unnecessary, "Websocket", "Received WebSocket Heartbeat Ack", DateTime.Now);
                _debugLogger.LogMessage(LogLevel.Debug, "Websocket", $"Ping {this.Ping}ms", DateTime.Now);
                HeartBeatEventArgs args = new HeartBeatEventArgs()
                {
                    Ping = this.Ping,
                    Timestamp = DateTimeOffset.Now
                };
            });
        }

        internal async void StartHeartbeating()
        {
            _debugLogger.LogMessage(LogLevel.Unnecessary, "Websocket", "Starting Heartbeat", DateTime.Now);
            while (!_cancelToken.IsCancellationRequested)
            {
                await SendHeartbeat();
                await Task.Delay(_heartbeatInterval);
            }
        }

        internal static async Task InternalUpdateStatus(string game = "", int idle_since = -1)
        {
            if (game.Length > 128)
                throw new Exception("Game name can't be longer than 128 characters!");

            JObject update = new JObject();
            if (idle_since > -1)
                update.Add("idle_since", idle_since);
            else
                update.Add("idle_since", null);

            update.Add("game", game != "" ? new JObject { { "name", game } } : null);

            JObject obj = new JObject
            {
                { "op", 3 },
                { "d", update }
            };

            await Task.Run(() => _websocketClient.SendMessage(obj.ToString()));
        }

        internal Task SendHeartbeat()
        {
            return this.SendHeartbeat(_sequence);
        }

        internal async Task SendHeartbeat(long seq)
        { 
            if (_waitingForAck)
            {
                _debugLogger.LogMessage(LogLevel.Critical, "Websocket", "Missed a heartbeat ack. Reconnecting.", DateTime.Now);
                await InternalReconnect();
            }

            _debugLogger.LogMessage(LogLevel.Unnecessary, "Websocket", "Sending Heartbeat", DateTime.Now);
            JObject obj = new JObject
            {
                { "op", 1 },
                { "d", seq }
            };
            _websocketClient.SendMessage(obj.ToString());
            _lastHeartbeat = DateTime.Now;
            _waitingForAck = true;
        }

        internal async Task SendIdentify()
        {
            await Task.Run(() =>
            {
                JObject obj = new JObject
                {
                    { "op", 2 },
                    { "d", new JObject
                        {
                            { "token", Utils.GetFormattedToken() },
                            { "properties", new JObject
                            {
                                { "$os", "linux" },
                                { "$browser", "DSharpPlus 1.0" },
                                { "$device", "DSharpPlus 1.0" },
                                { "$referrer", "" },
                                { "$referring_domain", "" }
                            } },
                            { "compress", false },
                            { "large_threshold" , config.LargeThreshold },
                            { "shards", new JArray { 0, _shardCount } }
                        }
                    }
                };
                _websocketClient.SendMessage(obj.ToString());
            });
        }

        internal async Task SendResume()
        {
            await Task.Run(() =>
            {
                JObject obj = new JObject
                {
                    { "op", 6 },
                    { "d", new JObject
                        {
                            { "token", _sessionToken },
                            { "session_id", _sessionID },
                            { "seq", _sequence }
                        }
                    }
                };
                _websocketClient.SendMessage(obj.ToString());
            });
        }

        internal static async Task SendVoiceStateUpdate(DiscordChannel channel, bool mute = false, bool deaf = false)
        {
            await Task.Run(() =>
            {
                JObject obj = new JObject
                {
                    { "op", 4 },
                    { "d", new JObject
                        {
                            { "guild_id", channel.Parent.ID },
                            { "channel_id", channel?.ID },
                            { "self_mute", mute },
                            { "self_deaf", deaf }
                        }
                    }
                };
                _websocketClient.SendMessage(obj.ToString());
            });
        }
        #endregion

        internal static ulong GetGuildIdFromChannelID(ulong channel_id)
        {
            foreach (DiscordGuild guild in _guilds.Values)
            {
                if (guild.Channels != null && guild.Channels.Find(x => x.ID == channel_id) != null) return guild.ID;
            }
            return 0;
        }

        internal static int GetChannelIndex(ulong channel_id)
        {
            foreach (DiscordGuild guild in _guilds.Values)
            {
                if (guild.Channels.Find(x => x.ID == channel_id) != null) return guild.Channels.FindIndex(x => x.ID == channel_id);
            }
            return 0;
        }

        internal static DiscordUser InternalGetCachedUser(ulong user_id)
        {
            foreach (DiscordGuild guild in _guilds.Values)
            {
                if (guild.Members.Find(x => x.User.ID == user_id) != null) return guild.Members.Find(x => x.User.ID == user_id).User;
            }
            return new DiscordUser()
            {
                ID = user_id
            };
        }

        internal static DiscordPresence InternalGetUserPresence(ulong user_id)
        {
            if (_presences.ContainsKey(user_id))
                return _presences[user_id];

            return new DiscordPresence()
            {
                InternalUser = new DiscordUser()
                {
                    ID = user_id
                },
                Status = "offline",
                InternalGame = null
            };
        }

        internal static Permission InternalAddPermission(Permission before, Permission p)
        {
            Permission after = before;
            after |= p;
            return after;
        }

        internal static Permission InternalRemovePermission(Permission before, Permission p)
        {
            Permission after = before;
            after &= ~p;
            return after;
        }

        internal static async Task InternalSetAvatar(Stream image)
        {
            using (var ms = new MemoryStream())
            {
                await image.CopyToAsync(ms);
                var b64 = Convert.ToBase64String(ms.ToArray());

                string url = Utils.GetApiBaseUri() + Endpoints.Users + "/@me";
                var headers = Utils.GetBaseHeaders();
                JObject jo = new JObject
                {
                    { "avatar", $"data:image/jpeg;base64,{b64}" }
                };
                WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, jo.ToString());
                WebResponse response = await RestClient.HandleRequestAsync(request);
            }
        }

        #region HTTP Actions
        #region Guild
        //
        internal static async Task<DiscordGuild> InternalCreateGuildAsync(string name)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds;
            var headers = Utils.GetBaseHeaders();
            JObject payload = new JObject { { "name", name } };

            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, payload.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);

            DiscordGuild guild = JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
            return guild;
        }


        internal static async void InternalDeleteGuildAsync(ulong id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + $"/{id}";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task<DiscordGuild> InternalModifyGuild(ulong guild_id, string name = "", string region = "", int verification_level = -1, int default_message_notifications = -1,
            ulong afk_channel_id = 0, int afk_timeout = -1, string icon = "", ulong owner_id = 0, string splash = "")
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (name != "")
                j.Add("name", name);
            if (region != "")
                j.Add("region", region);
            if (verification_level != -1)
                j.Add("verification_level", verification_level);
            if (default_message_notifications != -1)
                j.Add("default_message_notifications", default_message_notifications);
            if (afk_channel_id != 0)
                j.Add("akf_channel_id", afk_channel_id);
            if (afk_timeout != -1)
                j.Add("akf_timeout", afk_timeout);
            if (icon != "")
                j.Add("icon", icon);
            if (owner_id != 0)
                j.Add("owner_id", owner_id);
            if (splash != "")
                j.Add("splash", splash);

            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
        }

        internal static async Task<List<DiscordMember>> InternalGetGuildBans(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Bans;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordMember> bans = new List<DiscordMember>();
            foreach (JObject obj in j)
            {
                bans.Add(obj.ToObject<DiscordMember>());
            }
            return bans;
        }

        internal static async Task InternalCreateGuildBan(ulong guild_id, ulong user_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Bans + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PUT, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalRemoveGuildBan(ulong guild_id, ulong user_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Bans + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalLeaveGuild(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Users + "/@me" + Endpoints.Guilds + "/" + guild_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task<DiscordGuild> InternalCreateGuild(string name, string region, string icon, int verification_level, int default_message_notifications)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "name", name },
                { "region", region },
                { "icon", icon },
                { "verification_level", verification_level },
                { "default_message_notifications", default_message_notifications }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
        }

        internal static async Task<DiscordMember> InternalAddGuildMember(ulong guild_id, ulong user_id, string access_token, string nick = "", List<DiscordRole> roles = null,
        bool muted = false, bool deafened = false)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "access_token", access_token }
            };
            if (nick != "")
                j.Add("nick", nick);
            if (roles != null)
            {
                JArray r = new JArray();
                foreach (DiscordRole role in roles)
                {
                    r.Add(JsonConvert.SerializeObject(role));
                }
                j.Add("roles", r);
            }
            if (muted)
                j.Add("mute", true);
            if (deafened)
                j.Add("deaf", true);
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PUT, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordMember>(response.Response);
        }

        internal static async Task<List<DiscordMember>> InternalListGuildMembers(ulong guild_id, int limit, int after)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + $"?limit={limit}&after={after}";
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();

            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            JArray ja = JArray.Parse(response.Response);
            List<DiscordMember> members = new List<DiscordMember>();
            foreach (JObject m in ja)
            {
                members.Add(m.ToObject<DiscordMember>());
            }
            return members;
        }

        internal static async Task InternalAddGuildMemberRole(ulong guild_id, ulong user_id, ulong role_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + $"/{user_id}" + Endpoints.Roles + $"/{role_id}";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PUT, headers);
            await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalRemoveGuildMemberRole(ulong guild_id, ulong user_id, ulong role_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + $"/{user_id}" + Endpoints.Roles + $"/{role_id}";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            await RestClient.HandleRequestAsync(request);
        }
        #endregion
        #region Channel

        internal static async Task<DiscordChannel> InternalCreateGuildChannelAsync(ulong id, string name, ChannelType type)
        {
            if (name.Length > 200 || name.Length < 2)
                throw new Exception("Channel names can't be longer than 200 or shorter than 2 characters!");

            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + $"/{id}" + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            JObject payload = new JObject { { "name", name }, { "type", type.ToString() }, { "permission_overwrites", null } };

            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, payload.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);

            return JsonConvert.DeserializeObject<DiscordChannel>(response.Response);
        }

        internal static async Task<DiscordChannel> InternalGetChannel(ulong id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordChannel>(response.Response);
        }

        internal static async Task InternalDeleteChannel(ulong id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task<DiscordMessage> InternalGetMessage(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        internal static async Task<DiscordMessage> InternalCreateMessage(ulong channel_id, string content, bool tts, DiscordEmbed embed = null)
        {
            if (content.Length > 2000)
                throw new Exception("Messages are limited to a total of 2000 characters!");
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages;
            JObject j = new JObject
            {
                { "content", content },
                { "tts", tts }
            };
            if (embed != null)
            {
                JObject jembed = JObject.FromObject(embed);
                if (embed.Timestamp == new DateTime())
                {
                    jembed.Remove("timestamp");
                }
                else
                {
                    jembed["timestamp"] = embed.Timestamp.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
                }
                j.Add("embed", jembed);
            }
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        internal static async Task<DiscordMessage> InternalUploadFile(ulong channel_id, Stream file_data, string file_name, string content = "", bool tts = false)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages;
            var headers = Utils.GetBaseHeaders();
            var values = new Dictionary<string, string>();
            if (content != "")
                values.Add("content", content);
            if (tts)
                values.Add("tts", tts.ToString());
            WebRequest request = WebRequest.CreateMultipartRequest(url, HttpRequestMethod.POST, headers, values, file_data, file_name);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        internal static async Task<List<DiscordChannel>> InternalGetGuildChannels(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordChannel> channels = new List<DiscordChannel>();
            foreach (JObject jj in j)
            {
                channels.Add(JsonConvert.DeserializeObject<DiscordChannel>(jj.ToString()));
            }
            return channels;
        }

        internal static async Task<DiscordChannel> InternalCreateChannel(ulong guild_id, string name, ChannelType type, int bitrate = 0, int user_limit = 0)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "name", name },
                { "type", type == ChannelType.Text ? "text" : "voice" }
            };
            if (type == ChannelType.Voice)
            {
                j.Add("bitrate", bitrate);
                j.Add("userlimit", user_limit);
            }
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordChannel>(response.Response);
        }

        // TODO
        internal static async Task InternalModifyGuildChannelPosition(ulong guild_id, ulong channel_id, int position)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "id", channel_id },
                { "position", position }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task<List<DiscordMessage>> InternalGetChannelMessages(ulong channel_id, ulong around = 0, ulong before = 0, ulong after = 0, int limit = -1)
        {
            // ONLY ONE OUT OF around, before or after MAY BE USED.
            // THESE ARE MESSAGE ID's

            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (around != 0)
                j.Add("around", around);
            if (before != 0)
                j.Add("before", before);
            if (after != 0)
                j.Add("after", after);
            if (limit > -1)
                j.Add("limit", limit);

            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            JArray ja = JArray.Parse(response.Response);
            List<DiscordMessage> messages = new List<DiscordMessage>();
            foreach (JObject jo in ja)
            {
                messages.Add(jo.ToObject<DiscordMessage>());
            }
            return messages;
        }

        internal static async Task<DiscordMessage> InternalGetChannelMessage(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        // hi c:

        internal static async Task<DiscordMessage> InternalEditMessage(ulong channel_id, ulong message_id, string content = null, DiscordEmbed embed = null)
        {
            if (content != null || content.Length > 2000)
                throw new Exception("Messages are limited to a total of 2000 characters!");
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (content != null)
                j.Add("content", content);
            if (embed != null)
            {
                JObject jembed = JObject.FromObject(embed);
                if (embed.Timestamp == new DateTime())
                {
                    jembed.Remove("timestamp");
                }
                else
                {
                    jembed["timestamp"] = embed.Timestamp.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture);
                }
                j.Add("embed", jembed);
            }
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        internal static async Task InternalDeleteMessage(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalBulkDeleteMessages(ulong channel_id, List<ulong> message_ids)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + Endpoints.BulkDelete;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            JArray msgs = new JArray();
            foreach (ulong messageID in message_ids)
            {
                msgs.Add(messageID);
            }
            j.Add("messages", msgs);
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task<List<DiscordInvite>> InternalGetChannelInvites(ulong channel_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Invites;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            JArray ja = JArray.Parse(response.Response);
            List<DiscordInvite> invites = new List<DiscordInvite>();
            foreach (JObject jo in ja)
            {
                invites.Add(JsonConvert.DeserializeObject<DiscordInvite>(jo.ToString()));
            }
            return invites;
        }

        internal static async Task<DiscordInvite> InternalCreateChannelInvite(ulong channel_id, int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Invites;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "max_age", max_age },
                { "max_uses", max_uses },
                { "temporary", temporary },
                { "unique", unique }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
        }

        internal static async Task InternalDeleteChannelPermission(ulong channel_id, ulong overwrite_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Permissions + "/" + overwrite_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalTriggerTypingIndicator(ulong channel_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Typing;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task<List<DiscordMessage>> InternalGetPinnedMessages(ulong channel_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Pins;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            JArray j = JArray.Parse(response.Response);
            List<DiscordMessage> messages = new List<DiscordMessage>();
            foreach (JObject obj in j)
            {
                messages.Add(JsonConvert.DeserializeObject<DiscordMessage>(obj.ToString()));
            }
            return messages;
        }

        internal static async Task InternalAddPinnedChannelMessage(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Pins + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PUT, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalDeletePinnedChannelMessage(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Pins + "/" + message_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalGroupDMAddRecipient(ulong channel_id, ulong user_id, string access_token)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Recipients + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "access_token", access_token }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PUT, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalGroupDMRemoveRecipient(ulong channel_id, ulong user_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Recipients + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }


        internal static async Task InternalEditChannelPermissions(ulong channel_id, ulong overwrite_id, Permission allow, Permission deny, string type)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Permissions + "/" + overwrite_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "allow", (ulong)allow },
                { "deny", (ulong)deny },
                { "type", type }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PUT, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task<DiscordDMChannel> InternalCreateDM(ulong recipient_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Users + "/@me" + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "recipient_id", recipient_id }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordDMChannel>(response.Response);
        }

        internal static async Task<DiscordDMChannel> InternalCreateGroupDM(List<string> access_tokens)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Users + "/@me" + Endpoints.Channels;
            var headers = Utils.GetBaseHeaders();
            JArray tokens = new JArray();
            foreach (string token in access_tokens)
            {
                tokens.Add(token);
            }
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, tokens.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordDMChannel>(response.Response);
        }
        #endregion
        #region Member
        internal static async Task<List<DiscordMember>> InternalGetGuildMembers(ulong guild_id, int member_count)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Members;
            var headers = Utils.GetBaseHeaders();
            List<DiscordMember> result = new List<DiscordMember>();
            int pages = (int)Math.Ceiling((double)member_count / 1000);
            ulong lastId = 0;

            for (int i = 0; i < pages; i++)
            {
                WebRequest request = WebRequest.CreateRequest($"{url}?limit=1000&after={lastId}", HttpRequestMethod.GET, headers);
                WebResponse response = await RestClient.HandleRequestAsync(request);

                List<DiscordMember> items = JsonConvert.DeserializeObject<List<DiscordMember>>(response.Response);
                result.AddRange(items);
                lastId = items[items.Count - 1].User.ID;
            }
            return result;
        }

        internal static Task<DiscordUser> InternalGetUser(ulong user) =>
            InternalGetUser(user.ToString());

        internal static async Task<DiscordUser> InternalGetUser(string user)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Users + $"/{user}";
            var headers = Utils.GetBaseHeaders();

            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);

            return JsonConvert.DeserializeObject<DiscordUser>(response.Response);
        }

        internal static async Task<DiscordMember> InternalGetGuildMember(ulong guild_id, ulong member_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + "/" + member_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordMember>(response.Response);
        }

        internal static async Task InternalRemoveGuildMember(ulong guild_id, ulong user_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task<DiscordUser> InternalGetCurrentUser()
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Users + "/@me";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordUser>(response.Response);
        }

        internal static async Task<DiscordUser> InternalModifyCurrentUser(string username = "", string base64_avatar = "")
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Users + "/@me";
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (username != "")
                j.Add("", username);
            if (base64_avatar != "")
                j.Add("avatar", base64_avatar);
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordUser>(response.Response);
        }

        internal static async Task<List<DiscordGuild>> InternalGetCurrentUserGuilds()
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Users + "/@me" + Endpoints.Guilds;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            List<DiscordGuild> guilds = new List<DiscordGuild>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                guilds.Add(JsonConvert.DeserializeObject<DiscordGuild>(j.ToString()));
            }
            return guilds;
        }

        internal static async Task InternalModifyGuildMember(ulong guild_id, ulong user_id, string nick = null,
            List<ulong> roles = null, bool muted = false, bool deafened = false, ulong voicechannel_id = 0)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Members + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject();
            if (nick != null)
                j.Add("nick", nick);
            if (roles != null)
            {
                JArray r = new JArray();
                foreach (ulong role in roles)
                {
                    r.Add(role);
                }
                j.Add("roles", r);
            }
            if (muted)
                j.Add("mute", true);
            if (deafened)
                j.Add("deaf", true);
            if (voicechannel_id != 0)
                j.Add("channel_id", voicechannel_id);
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, j.ToString());
            await RestClient.HandleRequestAsync(request);
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

        internal static async Task<DiscordGuild> InternalGetGuild(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            DiscordGuild guild = JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
            if (_guilds.ContainsKey(guild_id))
            {
                _guilds[guild_id] = guild;
            }
            else
            {
                _guilds.Add(guild.ID, guild);
            }
            return guild;
        }

        internal static async Task<DiscordGuild> InternalModifyGuild(string name = "", string region = "", string icon = "", int verification_level = -1,
            int default_message_notifications = -1, ulong afk_channel_id = 0, int afk_timeout = -1, ulong owner_id = 0, string splash = "")
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds;
            var headers = Utils.GetBaseHeaders();
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

            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
        }

        internal static async Task<DiscordGuild> InternalDeleteGuild(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordGuild>(response.Response);
        }

        internal static async Task<DiscordRole> InternalModifyGuildRole(ulong guild_id, ulong role_id, string name, Permission permissions, int position, int color, bool separate, bool mentionable)
        {
            string url = $"{Utils.GetApiBaseUri()}{Endpoints.Guilds}/{guild_id}{Endpoints.Roles}/{role_id}";
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "name", name },
                { "permissions", (ulong)permissions },
                { "position", position },
                { "color", color },
                { "hoist", separate },
                { "mentionable", mentionable }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await request.HandleRequestAsync();
            return JsonConvert.DeserializeObject<DiscordRole>(response.Response);
        }

        internal static async Task<DiscordRole> InternalDeleteRole(ulong guild_id, ulong role_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Roles + "/" + role_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordRole>(response.Response);
        }

        internal static async Task<DiscordRole> InternalCreateGuildRole(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Roles;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordRole>(response.Response);
        }

        #endregion
        #region Prune
        // TODO
        internal static async Task<int> InternalGetGuildPruneCount(ulong guild_id, int days)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Prune;
            var headers = Utils.GetBaseHeaders();
            JObject payload = new JObject
            {
                { "days", days }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers, payload.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            JObject j = JObject.Parse(response.Response);
            return int.Parse(j["pruned"].ToString());
        }

        // TODO
        internal static async Task<int> InternalBeginGuildPrune(ulong guild_id, int days)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Prune;
            var headers = Utils.GetBaseHeaders();
            JObject payload = new JObject
            {
                { "days", days }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, payload.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            JObject j = JObject.Parse(response.Response);
            return int.Parse(j["pruned"].ToString());
        }
        #endregion
        #region GuildVarious

        internal static async Task<List<DiscordIntegration>> InternalGetGuildIntegrations(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Integrations;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await request.HandleRequestAsync();
            JArray j = JArray.Parse(response.Response);
            List<DiscordIntegration> integrations = new List<DiscordIntegration>();
            foreach (JObject obj in j)
            {
                integrations.Add(JsonConvert.DeserializeObject<DiscordIntegration>(obj.ToString()));
            }
            return integrations;
        }

        internal static async Task<DiscordIntegration> InternalCreateGuildIntegration(ulong guild_id, string type, ulong id)
        {
            // Attach from user
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Integrations;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "type", type },
                { "id", id }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await request.HandleRequestAsync();
            return JsonConvert.DeserializeObject<DiscordIntegration>(response.Response);
        }

        internal static async Task<DiscordIntegration> InternalModifyGuildIntegration(ulong guild_id, ulong integration_id, int expire_behaviour,
            int expire_grace_period, bool enable_emoticons)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Integrations + "/" + integration_id;
            JObject j = new JObject
            {
                { "expire_behaviour", expire_behaviour },
                { "expire_grace_period", expire_grace_period },
                { "enable_emoticons", enable_emoticons }
            };
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await request.HandleRequestAsync();
            return JsonConvert.DeserializeObject<DiscordIntegration>(response.Response);
        }

        internal static async Task InternalDeleteGuildIntegration(ulong guild_id, DiscordIntegration integration)
        {
            ulong IntegrationID = integration.ID;
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Integrations + "/" + IntegrationID;
            var headers = Utils.GetBaseHeaders();
            JObject j = JObject.FromObject(integration);
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers, j.ToString());
            WebResponse response = await request.HandleRequestAsync();
        }

        internal static async Task InternalSyncGuildIntegration(ulong guild_id, ulong integration_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Integrations + "/" + integration_id + Endpoints.Sync;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers);
            WebResponse response = await request.HandleRequestAsync();
        }

        internal static async Task<DiscordGuildEmbed> InternalGetGuildEmbed(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Embed;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await request.HandleRequestAsync();
            return JsonConvert.DeserializeObject<DiscordGuildEmbed>(response.Response);
        }

        internal static async Task<DiscordGuildEmbed> InternalModifyGuildEmbed(ulong guild_id, DiscordGuildEmbed embed)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Embed;
            var headers = Utils.GetBaseHeaders();
            JObject j = JObject.FromObject(embed);
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await request.HandleRequestAsync();
            return JsonConvert.DeserializeObject<DiscordGuildEmbed>(response.Response);
        }

        internal static async Task<List<DiscordVoiceRegion>> InternalGetGuildVoiceRegions(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Regions;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await request.HandleRequestAsync();
            JArray j = JArray.Parse(response.Response);
            List<DiscordVoiceRegion> regions = new List<DiscordVoiceRegion>();
            foreach (JObject obj in j)
            {
                regions.Add(JsonConvert.DeserializeObject<DiscordVoiceRegion>(obj.ToString()));
            }
            return regions;
        }

        internal static async Task<List<DiscordInvite>> InternalGetGuildInvites(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Invites;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await request.HandleRequestAsync();
            JArray j = JArray.Parse(response.Response);
            List<DiscordInvite> invites = new List<DiscordInvite>();
            foreach (JObject obj in j)
            {
                invites.Add(JsonConvert.DeserializeObject<DiscordInvite>(obj.ToString()));
            }
            return invites;
        }

        #endregion
        #region Invite
        internal static async Task<DiscordInvite> InternalGetInvite(string invite_code)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Invites + "/" + invite_code;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
        }

        internal static async Task<DiscordInvite> InternalDeleteInvite(string invite_code)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Invites + "/" + invite_code;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
        }

        internal static async Task<DiscordInvite> InternalAcceptInvite(string invite_code)
        {
            // USER ONLY
            string url = Utils.GetApiBaseUri() + Endpoints.Invites + "/" + invite_code;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordInvite>(response.Response);
        }
        #endregion
        #region Connections
        internal static async Task<List<DiscordConnection>> InternalGetUsersConnections()
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Users + "/@me" + Endpoints.Connections;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            List<DiscordConnection> connections = new List<DiscordConnection>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                connections.Add(JsonConvert.DeserializeObject<DiscordConnection>(j.ToString()));
            }
            return connections;
        }
        #endregion
        #region Voice
        internal static async Task<List<DiscordVoiceRegion>> InternalListVoiceRegions()
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Voice + Endpoints.Regions;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            List<DiscordVoiceRegion> regions = new List<DiscordVoiceRegion>();
            JArray j = JArray.Parse(response.Response);
            foreach (JObject obj in j)
            {
                regions.Add(JsonConvert.DeserializeObject<DiscordVoiceRegion>(obj.ToString()));
            }
            return regions;
        }
        #endregion
        #region Webhooks
        internal static async Task<DiscordWebhook> InternalCreateWebhook(ulong channel_id, string name, string base64_avatar)
        {
            if (name.Length > 200 || name.Length < 2)
                throw new Exception("Webhook name has to be between 2 and 200 characters!");

            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Webhooks;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "name", name },
                { "avatar", base64_avatar }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);

            return JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
        }

        internal static async Task<List<DiscordWebhook>> InternalGetChannelWebhooks(ulong channel_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Webhooks;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            List<DiscordWebhook> webhooks = new List<DiscordWebhook>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                webhooks.Add(JsonConvert.DeserializeObject<DiscordWebhook>(j.ToString()));
            }
            return webhooks;
        }

        internal static async Task<List<DiscordWebhook>> InternalGetGuildWebhooks(ulong guild_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Guilds + "/" + guild_id + Endpoints.Webhooks;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            List<DiscordWebhook> webhooks = new List<DiscordWebhook>();
            foreach (JObject j in JArray.Parse(response.Response))
            {
                webhooks.Add(JsonConvert.DeserializeObject<DiscordWebhook>(j.ToString()));
            }
            return webhooks;
        }

        internal static async Task<DiscordWebhook> InternalGetWebhook(ulong webhook_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Webhooks + "/" + webhook_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
        }

        // Auth header not required
        internal static async Task<DiscordWebhook> InternalGetWebhookWithToken(ulong webhook_id, string webhook_token)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token;
            WebRequest request = WebRequest.CreateRequest(url);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            DiscordWebhook wh = JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
            wh.Token = webhook_token;
            wh.ID = webhook_id;
            return wh;
        }

        internal static async Task<DiscordWebhook> InternalModifyWebhook(ulong webhook_id, string name, string base64_avatar)
        {
            if (name.Length > 200 || name.Length < 2)
                throw new Exception("Webhook name has to be between 2 and 200 characters!");
            string url = Utils.GetApiBaseUri() + Endpoints.Webhooks + "/" + webhook_id;
            var headers = Utils.GetBaseHeaders();
            JObject j = new JObject
            {
                { "name", name },
                { "avatar", base64_avatar }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, headers, j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
        }

        internal static async Task<DiscordWebhook> InternalModifyWebhook(ulong webhook_id, string name, string base64_avatar, string webhook_token)
        {
            if (name.Length > 200 || name.Length < 2)
                throw new Exception("Webhook name has to be between 2 and 200 characters!");
            string url = Utils.GetApiBaseUri() + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token;
            JObject j = new JObject
            {
                { "name", name },
                { "avatar", base64_avatar }
            };
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PATCH, payload: j.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JsonConvert.DeserializeObject<DiscordWebhook>(response.Response);
        }

        internal static async Task InternalDeleteWebhook(ulong webhook_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Webhooks + "/" + webhook_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalDeleteWebhook(ulong webhook_id, string webhook_token)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token;
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalExecuteWebhook(ulong webhook_id, string webhook_token, string content = "", string username = "", string avatar_url = "",
            bool tts = false, List<DiscordEmbed> embeds = null)
        {
            if (content.Length > 2000)
                throw new Exception("Messages are limited to a total of 2000 characters!");

            string url = Utils.GetApiBaseUri() + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token;
            JObject req = new JObject();
            if (content != "")
                req.Add("content", content);
            if (username != "")
                req.Add("username", username);
            if (avatar_url != "")
                req.Add("avatar_url", avatar_url);
            if (tts)
                req.Add("tts", tts);
            if (embeds != null)
            {
                JArray arr = new JArray();
                foreach (DiscordEmbed e in embeds)
                {
                    arr.Add(JsonConvert.SerializeObject(e));
                }
                req.Add(arr);
            }
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, payload: req.ToString());
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalExecuteWebhookSlack(ulong webhook_id, string webhook_token, string json_payload)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token + Endpoints.Slack;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, payload: json_payload);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalExecuteWebhookGithub(ulong webhook_id, string webhook_token, string json_payload)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Webhooks + "/" + webhook_id + "/" + webhook_token + Endpoints.Github;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.POST, payload: json_payload);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        #endregion
        #region Reactions
        internal static async Task InternalCreateReaction(ulong channel_id, ulong message_id, string emoji)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id + Endpoints.Reactions + "/" + emoji + "/@me";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.PUT, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalDeleteOwnReaction(ulong channel_id, ulong message_id, string emoji)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id + Endpoints.Reactions + "/" + emoji + "/@me";
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task InternalDeleteUserReaction(ulong channel_id, ulong message_id, ulong user_id, string emoji)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id + Endpoints.Reactions + "/" + emoji + "/" + user_id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }

        internal static async Task<List<DiscordUser>> InternalGetReactions(ulong channel_id, ulong message_id, string emoji)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id + Endpoints.Reactions + "/" + emoji;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            List<DiscordUser> reacters = new List<DiscordUser>();
            foreach (JObject obj in JArray.Parse(response.Response))
            {
                reacters.Add(obj.ToObject<DiscordUser>());
            }
            return reacters;
        }

        internal static async Task InternalDeleteAllReactions(ulong channel_id, ulong message_id)
        {
            string url = Utils.GetApiBaseUri() + Endpoints.Channels + "/" + channel_id + Endpoints.Messages + "/" + message_id + Endpoints.Reactions;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.DELETE, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
        }
        #endregion
        #region Misc
        internal static async Task<DiscordApplication> InternalGetApplicationInfo(string id = "@me")
        {
            string url = Utils.GetApiBaseUri() + Endpoints.OAuth2 + Endpoints.Applications + "/" + id;
            var headers = Utils.GetBaseHeaders();
            WebRequest request = WebRequest.CreateRequest(url, HttpRequestMethod.GET, headers);
            WebResponse response = await RestClient.HandleRequestAsync(request);
            return JObject.Parse(response.Response).ToObject<DiscordApplication>();
        }
        #endregion
        #endregion

        ~DiscordClient()
        {
            Dispose();
        }

        private bool disposed;
        /// <summary>
        /// Disposes your DiscordClient.
        /// </summary>
        public async void Dispose()
        {
            if (disposed)
                return;

            GC.SuppressFinalize(this);

            await Disconnect();

            _cancelTokenSource.Cancel();
            _guilds = null;
            _heartbeatThread = null;
            _me = null;
            _modules = null;
            _privateChannels = null;
            await _websocketClient.InternalDisconnectAsync();

            disposed = true;
        }
    }
}
