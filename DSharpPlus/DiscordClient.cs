using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Objects.Transport;
using DSharpPlus.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public event AsyncEventHandler<ClientErrorEventArgs> ClientError
        {
            add { this._client_error.Register(value); }
            remove { this._client_error.Unregister(value); }
        }
        private AsyncEvent<ClientErrorEventArgs> _client_error;

        /// <summary>
        /// 
        /// </summary>
        public event AsyncEventHandler SocketOpened
        {
            add { this._socket_opened.Register(value); }
            remove { this._socket_opened.Unregister(value); }
        }
        private AsyncEvent _socket_opened;
        /// <summary>
        /// 
        /// </summary>
        public event AsyncEventHandler<SocketDisconnectEventArgs> SocketClosed
        {
            add { this._socket_closed.Register(value); }
            remove { this._socket_closed.Unregister(value); }
        }
        private AsyncEvent<SocketDisconnectEventArgs> _socket_closed;
        /// <summary>
        /// The ready event is dispatched when a client completed the initial handshake.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Ready
        {
            add { this._ready.Register(value); }
            remove { this._ready.Unregister(value); }
        }
        private AsyncEvent<ReadyEventArgs> _ready;
        /// <summary>
        /// Sent when a new channel is created.
        /// </summary>
        public event AsyncEventHandler<ChannelCreateEventArgs> ChannelCreated
        {
            add { this._channel_created.Register(value); }
            remove { this._channel_created.Unregister(value); }
        }
        private AsyncEvent<ChannelCreateEventArgs> _channel_created;
        /// <summary>
        /// Sent when a new dm channel is created.
        /// </summary>
        public event AsyncEventHandler<DmChannelCreateEventArgs> DMChannelCreated
        {
            add { this._dm_channel_created.Register(value); }
            remove { this._dm_channel_created.Unregister(value); }
        }
        private AsyncEvent<DmChannelCreateEventArgs> _dm_channel_created;
        /// <summary>
        /// Sent when a channel is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelUpdateEventArgs> ChannelUpdated
        {
            add { this._channel_updated.Register(value); }
            remove { this._channel_updated.Unregister(value); }
        }
        private AsyncEvent<ChannelUpdateEventArgs> _channel_updated;
        /// <summary>
        /// Sent when a channel is deleted
        /// </summary>
        public event AsyncEventHandler<ChannelDeleteEventArgs> ChannelDeleted
        {
            add { this._channel_deleted.Register(value); }
            remove { this._channel_deleted.Unregister(value); }
        }
        private AsyncEvent<ChannelDeleteEventArgs> _channel_deleted;
        /// <summary>
        /// Sent when a dm channel is deleted
        /// </summary>
        public event AsyncEventHandler<DmChannelDeleteEventArgs> DMChannelDeleted
        {
            add { this._dm_channel_deleted.Register(value); }
            remove { this._dm_channel_deleted.Unregister(value); }
        }
        private AsyncEvent<DmChannelDeleteEventArgs> _dm_channel_deleted;
        /// <summary>
        /// Sent when the user joins a new guild.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildCreated
        {
            add { this._guild_created.Register(value); }
            remove { this._guild_created.Unregister(value); }
        }
        private AsyncEvent<GuildCreateEventArgs> _guild_created;
        /// <summary>
        /// Sent when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildAvailable
        {
            add { this._guild_available.Register(value); }
            remove { this._guild_available.Unregister(value); }
        }
        private AsyncEvent<GuildCreateEventArgs> _guild_available;
        /// <summary>
        /// Sent when a guild is updated.
        /// </summary>
        public event AsyncEventHandler<GuildUpdateEventArgs> GuildUpdated
        {
            add { this._guild_updated.Register(value); }
            remove { this._guild_updated.Unregister(value); }
        }
        private AsyncEvent<GuildUpdateEventArgs> _guild_updated;
        /// <summary>
        /// Sent when the user leaves or is removed from a guild.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildDeleted
        {
            add { this._guild_deleted.Register(value); }
            remove { this._guild_deleted.Unregister(value); }
        }
        private AsyncEvent<GuildDeleteEventArgs> _guild_deleted;
        /// <summary>
        /// Sent when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildUnavailable
        {
            add { this._guild_unavailable.Register(value); }
            remove { this._guild_unavailable.Unregister(value); }
        }
        private AsyncEvent<GuildDeleteEventArgs> _guild_unavailable;
        /// <summary>
        /// Sent when a message is created.
        /// </summary>
        public event AsyncEventHandler<MessageCreateEventArgs> MessageCreated
        {
            add { this._message_created.Register(value); }
            remove { this._message_created.Unregister(value); }
        }
        private AsyncEvent<MessageCreateEventArgs> _message_created;

        /// <summary>
        /// Sent when a presence has been updated.
        /// </summary>
        public event AsyncEventHandler<PresenceUpdateEventArgs> PresenceUpdate
        {
            add { this._presence_update.Register(value); }
            remove { this._presence_update.Unregister(value); }
        }
        private AsyncEvent<PresenceUpdateEventArgs> _presence_update;

        /// <summary>
        /// Sent when a guild ban gets added
        /// </summary>
        public event AsyncEventHandler<GuildBanAddEventArgs> GuildBanAdd
        {
            add { this._guild_ban_add.Register(value); }
            remove { this._guild_ban_add.Unregister(value); }
        }
        private AsyncEvent<GuildBanAddEventArgs> _guild_ban_add;

        /// <summary>
        /// Sent when a guild ban gets removed
        /// </summary>
        public event AsyncEventHandler<GuildBanRemoveEventArgs> GuildBanRemove
        {
            add { this._guild_ban_remove.Register(value); }
            remove { this._guild_ban_remove.Unregister(value); }
        }
        private AsyncEvent<GuildBanRemoveEventArgs> _guild_ban_remove;

        /// <summary>
        /// Sent when a guilds emojis get updated
        /// </summary>
        public event AsyncEventHandler<GuildEmojisUpdateEventArgs> GuildEmojisUpdate
        {
            add { this._guild_emojis_update.Register(value); }
            remove { this._guild_emojis_update.Unregister(value); }
        }
        private AsyncEvent<GuildEmojisUpdateEventArgs> _guild_emojis_update;

        /// <summary>
        /// Sent when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdate
        {
            add { this._guild_integrations_update.Register(value); }
            remove { this._guild_integrations_update.Unregister(value); }
        }
        private AsyncEvent<GuildIntegrationsUpdateEventArgs> _guild_integrations_update;

        /// <summary>
        /// Sent when a new user joins a guild.
        /// </summary>
        public event AsyncEventHandler<GuildMemberAddEventArgs> GuildMemberAdd
        {
            add { this._guild_member_add.Register(value); }
            remove { this._guild_member_add.Unregister(value); }
        }
        private AsyncEvent<GuildMemberAddEventArgs> _guild_member_add;
        /// <summary>
        /// Sent when a user is removed from a guild (leave/kick/ban).
        /// </summary>
        public event AsyncEventHandler<GuildMemberRemoveEventArgs> GuildMemberRemove
        {
            add { this._guild_member_remove.Register(value); }
            remove { this._guild_member_remove.Unregister(value); }
        }
        private AsyncEvent<GuildMemberRemoveEventArgs> _guild_member_remove;

        /// <summary>
        /// Sent when a guild member is updated.
        /// </summary>
        public event AsyncEventHandler<GuildMemberUpdateEventArgs> GuildMemberUpdate
        {
            add { this._guild_member_update.Register(value); }
            remove { this._guild_member_update.Unregister(value); }
        }
        private AsyncEvent<GuildMemberUpdateEventArgs> _guild_member_update;

        /// <summary>
        /// Sent when a guild role is created.
        /// </summary>
        public event AsyncEventHandler<GuildRoleCreateEventArgs> GuildRoleCreate
        {
            add { this._guild_role_create.Register(value); }
            remove { this._guild_role_create.Unregister(value); }
        }
        private AsyncEvent<GuildRoleCreateEventArgs> _guild_role_create;

        /// <summary>
        /// Sent when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleUpdateEventArgs> GuildRoleUpdate
        {
            add { this._guild_role_update.Register(value); }
            remove { this._guild_role_update.Unregister(value); }
        }
        private AsyncEvent<GuildRoleUpdateEventArgs> _guild_role_update;

        /// <summary>
        /// Sent when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleDeleteEventArgs> GuildRoleDelete
        {
            add { this._guild_role_delete.Register(value); }
            remove { this._guild_role_delete.Unregister(value); }
        }
        private AsyncEvent<GuildRoleDeleteEventArgs> _guild_role_delete;

        /// <summary>
        /// Sent when a message is updated.
        /// </summary>
        public event AsyncEventHandler<MessageUpdateEventArgs> MessageUpdate
        {
            add { this._message_update.Register(value); }
            remove { this._message_update.Unregister(value); }
        }
        private AsyncEvent<MessageUpdateEventArgs> _message_update;

        /// <summary>
        /// Sent when a message is deleted.
        /// </summary>
        public event AsyncEventHandler<MessageDeleteEventArgs> MessageDelete
        {
            add { this._message_delete.Register(value); }
            remove { this._message_delete.Unregister(value); }
        }
        private AsyncEvent<MessageDeleteEventArgs> _message_delete;

        /// <summary>
        /// Sent when multiple messages are deleted at once.
        /// </summary>
        public event AsyncEventHandler<MessageBulkDeleteEventArgs> MessageBulkDelete
        {
            add { this._message_bulk_delete.Register(value); }
            remove { this._message_bulk_delete.Unregister(value); }
        }
        private AsyncEvent<MessageBulkDeleteEventArgs> _message_bulk_delete;

        /// <summary>
        /// Sent when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<TypingStartEventArgs> TypingStart
        {
            add { this._typing_start.Register(value); }
            remove { this._typing_start.Unregister(value); }
        }
        private AsyncEvent<TypingStartEventArgs> _typing_start;

        /// <summary>
        /// Sent when the current user updates their settings.
        /// </summary>
        public event AsyncEventHandler<UserSettingsUpdateEventArgs> UserSettingsUpdate
        {
            add { this._user_settings_update.Register(value); }
            remove { this._user_settings_update.Unregister(value); }
        }
        private AsyncEvent<UserSettingsUpdateEventArgs> _user_settings_update;

        /// <summary>
        /// Sent when properties about the user change.
        /// </summary>
        public event AsyncEventHandler<UserUpdateEventArgs> UserUpdate
        {
            add { this._user_update.Register(value); }
            remove { this._user_update.Unregister(value); }
        }
        private AsyncEvent<UserUpdateEventArgs> _user_update;

        /// <summary>
        /// Sent when someone joins/leaves/moves voice channels.
        /// </summary>
        public event AsyncEventHandler<VoiceStateUpdateEventArgs> VoiceStateUpdate
        {
            add { this._voice_state_update.Register(value); }
            remove { this._voice_state_update.Unregister(value); }
        }
        private AsyncEvent<VoiceStateUpdateEventArgs> _voice_state_update;

        /// <summary>
        /// Sent when a guild's voice server is updated.
        /// </summary>
        public event AsyncEventHandler<VoiceServerUpdateEventArgs> VoiceServerUpdate
        {
            add { this._voice_server_update.Register(value); }
            remove { this._voice_server_update.Unregister(value); }
        }
        private AsyncEvent<VoiceServerUpdateEventArgs> _voice_server_update;

        /// <summary>
        /// Sent in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<GuildMembersChunkEventArgs> GuildMembersChunk
        {
            add { this._guild_members_chunk.Register(value); }
            remove { this._guild_members_chunk.Unregister(value); }
        }
        private AsyncEvent<GuildMembersChunkEventArgs> _guild_members_chunk;

        /// <summary>
        /// Sent when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<UnknownEventArgs> UnknownEvent
        {
            add { this._unknown_event.Register(value); }
            remove { this._unknown_event.Unregister(value); }
        }
        private AsyncEvent<UnknownEventArgs> _unknown_event;

        /// <summary>
        /// Sent when a reaction gets added to a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionAddEventArgs> MessageReactionAdd
        {
            add { this._message_reaction_add.Register(value); }
            remove { this._message_reaction_add.Unregister(value); }
        }
        private AsyncEvent<MessageReactionAddEventArgs> _message_reaction_add;

        /// <summary>
        /// Sent when a reaction gets removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEventArgs> MessageReactionRemove
        {
            add { this._message_reaction_remove.Register(value); }
            remove { this._message_reaction_remove.Unregister(value); }
        }
        private AsyncEvent<MessageReactionRemoveEventArgs> _message_reaction_remove;

        /// <summary>
        /// Sent when all reactions get removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveAllEventArgs> MessageReactionRemoveAll
        {
            add { this._message_reaction_remove_all.Register(value); }
            remove { this._message_reaction_remove_all.Unregister(value); }
        }
        private AsyncEvent<MessageReactionRemoveAllEventArgs> _message_reaction_remove_all;

        public event AsyncEventHandler<WebhooksUpdateEventArgs> WebhooksUpdate
        {
            add { this._webhooks_update.Register(value); }
            remove { this._webhooks_update.Unregister(value); }
        }
        private AsyncEvent<WebhooksUpdateEventArgs> _webhooks_update;

        public event AsyncEventHandler<HeartBeatEventArgs> HeartBeated
        {
            add { this._heart_beated.Register(value); }
            remove { this._heart_beated.Unregister(value); }
        }
        private AsyncEvent<HeartBeatEventArgs> _heart_beated;

        internal void EventErrorHandler(string evname, Exception ex)
        {
            this.DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"An {ex.GetType()} occured in {evname}.", DateTime.Now);
            this._client_error.InvokeAsync(new ClientErrorEventArgs(this) { EventName = evname, Exception = ex }).GetAwaiter().GetResult();
        }

        private void Goof(string evname, Exception ex)
        {
            this.DebugLogger.LogMessage(LogLevel.Critical, "DSharpPlus", $"An {ex.GetType()} occured in the exception handler.", DateTime.Now);
        }
        #endregion

        #region Internal Variables
        internal CancellationTokenSource _cancel_token_source;
        internal CancellationToken _cancel_token;

        internal DiscordConfig config;

        internal List<IModule> _modules = new List<IModule>();

        internal BaseWebSocketClient _websocket_client;
        internal DiscordRestClient _rest_client;
        internal long _sequence = 0;
        internal string _session_token = "";
        internal string _session_id = "";
        internal int _heartbeat_interval;
        internal Task _heartbeat_task;
        internal DateTime _last_heartbeat;
        internal bool _waiting_for_ack = false;
        internal static UTF8Encoding UTF8 = new UTF8Encoding(false);
        #endregion

        #region Public Variables
        internal DebugLogger _debugLogger;
        /// <summary>
        /// 
        /// </summary>
        public DebugLogger DebugLogger => _debugLogger;

        internal int _gateway_version;
        /// <summary>
        /// Gateway protocol version
        /// </summary>
        public int GatewayVersion => _gateway_version;

        internal string _gatewayUrl = "";
        /// <summary>
        /// Gateway url
        /// </summary>
        public string GatewayUrl => _gatewayUrl;

        internal int _shardCount = 1;
        /// <summary>
        /// Number of shards the bot is connected with
        /// </summary>
        public int ShardCount => this.config.ShardCount;
        public int ShardId => this.config.ShardId;

        internal DiscordUser _current_user;
        /// <summary>
        /// The current user
        /// </summary>
        public DiscordUser CurrentUser => _current_user;

        internal List<DiscordDmChannel> _private_channels = new List<DiscordDmChannel>();
        /// <summary>
        /// List of DM Channels
        /// </summary>
        public IReadOnlyList<DiscordDmChannel> PrivateChannels => new ReadOnlyCollection<DiscordDmChannel>(_private_channels);

        internal Dictionary<ulong, DiscordGuild> _guilds = new Dictionary<ulong, DiscordGuild>();
        /// <summary>
        /// List of Guilds
        /// </summary>
        public IReadOnlyDictionary<ulong, DiscordGuild> Guilds => new ReadOnlyDictionary<ulong, DiscordGuild>(this._guilds);

        /// <summary>
        /// Gets the WS latency for this client.
        /// </summary>
        public int Ping { get; internal set; }
        #endregion

        #region Connection semaphore
        private static SemaphoreSlim ConnectionSemaphore => _semaphore_init.Value;
        private static Lazy<SemaphoreSlim> _semaphore_init = new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1));
        #endregion

        /// <summary>
        /// Intializes a new instance of DiscordClient
        /// </summary>
        public DiscordClient()
        {
            this.config = new DiscordConfig();

            InternalSetup();
        }

        /// <summary>
        /// Initializes a new instance of DiscordClient
        /// </summary>
        /// <param name="config">Overwrites the default config</param>
        public DiscordClient(DiscordConfig config)
        {
            this.config = config;

            InternalSetup();
        }

        /// <summary>
        /// Sets the WebSocket client implementation.
        /// </summary>
        /// <typeparam name="T">Type of the WebSocket client to use.</typeparam>
        public void SetWebSocketClient<T>() where T : BaseWebSocketClient, new()
        {
            BaseWebSocketClient.ClientType = typeof(T);
        }

        /// <summary>
        /// Sets the UDP client implementation.
        /// </summary>
        /// <typeparam name="T">Type of the UDP client to use.</typeparam>
        public void SetUdpClient<T>() where T : BaseUdpClient, new()
        {
            BaseUdpClient.ClientType = typeof(T);
        }

        internal void InternalSetup()
        {
            this._client_error = new AsyncEvent<ClientErrorEventArgs>(this.Goof, "CLIENT_ERROR");
            this._socket_opened = new AsyncEvent(this.EventErrorHandler, "SOCKET_OPENED");
            this._socket_closed = new AsyncEvent<SocketDisconnectEventArgs>(this.EventErrorHandler, "SOCKET_CLOSED");
            this._ready = new AsyncEvent<ReadyEventArgs>(this.EventErrorHandler, "READY");
            this._channel_created = new AsyncEvent<ChannelCreateEventArgs>(this.EventErrorHandler, "CHANNEL_CREATED");
            this._dm_channel_created = new AsyncEvent<DmChannelCreateEventArgs>(this.EventErrorHandler, "DM_CHANNEL_CREATED");
            this._channel_updated = new AsyncEvent<ChannelUpdateEventArgs>(this.EventErrorHandler, "CHANNEL_UPDATED");
            this._channel_deleted = new AsyncEvent<ChannelDeleteEventArgs>(this.EventErrorHandler, "CHANNEL_DELETED");
            this._dm_channel_deleted = new AsyncEvent<DmChannelDeleteEventArgs>(this.EventErrorHandler, "DM_CHANNEL_DELETED");
            this._guild_created = new AsyncEvent<GuildCreateEventArgs>(this.EventErrorHandler, "GUILD_CREATED");
            this._guild_available = new AsyncEvent<GuildCreateEventArgs>(this.EventErrorHandler, "GUILD_AVAILABLE");
            this._guild_updated = new AsyncEvent<GuildUpdateEventArgs>(this.EventErrorHandler, "GUILD_UPDATED");
            this._guild_deleted = new AsyncEvent<GuildDeleteEventArgs>(this.EventErrorHandler, "GUILD_DELETED");
            this._guild_unavailable = new AsyncEvent<GuildDeleteEventArgs>(this.EventErrorHandler, "GUILD_UNAVAILABLE");
            this._message_created = new AsyncEvent<MessageCreateEventArgs>(this.EventErrorHandler, "MESSAGE_CREATED");
            this._presence_update = new AsyncEvent<PresenceUpdateEventArgs>(this.EventErrorHandler, "PRESENCE_UPDATE");
            this._guild_ban_add = new AsyncEvent<GuildBanAddEventArgs>(this.EventErrorHandler, "GUILD_BAN_ADD");
            this._guild_ban_remove = new AsyncEvent<GuildBanRemoveEventArgs>(this.EventErrorHandler, "GUILD_BAN_REMOVE");
            this._guild_emojis_update = new AsyncEvent<GuildEmojisUpdateEventArgs>(this.EventErrorHandler, "GUILD_EMOJI_UPDATE");
            this._guild_integrations_update = new AsyncEvent<GuildIntegrationsUpdateEventArgs>(this.EventErrorHandler, "GUILD_INTEGRATIONS_UPDATE");
            this._guild_member_add = new AsyncEvent<GuildMemberAddEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_ADD");
            this._guild_member_remove = new AsyncEvent<GuildMemberRemoveEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_REMOVE");
            this._guild_member_update = new AsyncEvent<GuildMemberUpdateEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_UPDATE");
            this._guild_role_create = new AsyncEvent<GuildRoleCreateEventArgs>(this.EventErrorHandler, "GUILD_ROLE_CREATE");
            this._guild_role_update = new AsyncEvent<GuildRoleUpdateEventArgs>(this.EventErrorHandler, "GUILD_ROLE_UPDATE");
            this._guild_role_delete = new AsyncEvent<GuildRoleDeleteEventArgs>(this.EventErrorHandler, "GUILD_ROLE_DELETE");
            this._message_update = new AsyncEvent<MessageUpdateEventArgs>(this.EventErrorHandler, "MESSAGE_UPDATE");
            this._message_delete = new AsyncEvent<MessageDeleteEventArgs>(this.EventErrorHandler, "MESSAGE_DELETE");
            this._message_bulk_delete = new AsyncEvent<MessageBulkDeleteEventArgs>(this.EventErrorHandler, "MESSAGE_BULK_DELETE");
            this._typing_start = new AsyncEvent<TypingStartEventArgs>(this.EventErrorHandler, "TYPING_START");
            this._user_settings_update = new AsyncEvent<UserSettingsUpdateEventArgs>(this.EventErrorHandler, "USER_SETTINGS_UPDATE");
            this._user_update = new AsyncEvent<UserUpdateEventArgs>(this.EventErrorHandler, "USER_UPDATE");
            this._voice_state_update = new AsyncEvent<VoiceStateUpdateEventArgs>(this.EventErrorHandler, "VOICE_STATE_UPDATE");
            this._voice_server_update = new AsyncEvent<VoiceServerUpdateEventArgs>(this.EventErrorHandler, "VOICE_SERVER_UPDATE");
            this._guild_members_chunk = new AsyncEvent<GuildMembersChunkEventArgs>(this.EventErrorHandler, "GUILD_MEMBERS_CHUNK");
            this._unknown_event = new AsyncEvent<UnknownEventArgs>(this.EventErrorHandler, "UNKNOWN_EVENT");
            this._message_reaction_add = new AsyncEvent<MessageReactionAddEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_ADD");
            this._message_reaction_remove = new AsyncEvent<MessageReactionRemoveEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_REMOVE");
            this._message_reaction_remove_all = new AsyncEvent<MessageReactionRemoveAllEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_REMOVE_ALL");
            this._webhooks_update = new AsyncEvent<WebhooksUpdateEventArgs>(this.EventErrorHandler, "WEBHOOKS_UPDATE");
            this._heart_beated = new AsyncEvent<HeartBeatEventArgs>(this.EventErrorHandler, "HEART_BEATED");

            this._rest_client = new DiscordRestClient(this);
            this._debugLogger = new DebugLogger(this);

            this._private_channels = new List<DiscordDmChannel>();
            this._guilds = new Dictionary<ulong, DiscordGuild>();

            if (config.UseInternalLogHandler)
                DebugLogger.LogMessageReceived += (sender, e) => DebugLogger.LogHandler(sender, e);
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

        /// <summary>
        /// Connects to the gateway
        /// </summary>
        /// <returns></returns>
        public Task ConnectAsync() => InternalConnectAsync();

        public Task ReconnectAsync(bool start_new_session = false)
        {
            if (start_new_session)
                _session_id = "";

            return _websocket_client.InternalDisconnectAsync(null);
        }

        internal Task InternalReconnectAsync() => InternalConnectAsync();

        internal async Task InternalConnectAsync()
        {
            var an = typeof(DiscordClient).GetTypeInfo().Assembly.GetName();
            this.DebugLogger.LogMessage(LogLevel.Info, "DSharpPlus", $"DSharpPlus, version {an.Version.ToString(3)}, booting", DateTime.Now);

            await ConnectionSemaphore.WaitAsync();
            await Task.Delay(6000);

            await InternalUpdateGatewayAsync();

            if (_current_user == null)
                _current_user = await this._rest_client.InternalGetCurrentUser();

            _websocket_client = BaseWebSocketClient.Create();

            _cancel_token_source = new CancellationTokenSource();
            _cancel_token = _cancel_token_source.Token;

            _websocket_client.OnConnect += () => this._socket_opened.InvokeAsync();
            _websocket_client.OnDisconnect += async e =>
            {
                _cancel_token_source.Cancel();

                _debugLogger.LogMessage(LogLevel.Debug, "Websocket", $"Connection closed", DateTime.Now);
                await this._socket_closed.InvokeAsync(new SocketDisconnectEventArgs(this) { CloseCode = e.CloseCode, CloseMessage = e.CloseMessage });

                if (config.AutoReconnect)
                {
                    DebugLogger.LogMessage(LogLevel.Critical, "Websocket", $"Socket connection terminated ({e.CloseCode}, '{e.CloseMessage}'). Reconnecting", DateTime.Now);
                    await ConnectAsync();
                }
            };
            _websocket_client.OnMessage += e => HandleSocketMessageAsync(e.Message);
            await _websocket_client.ConnectAsync(_gatewayUrl + $"?v={config.GatewayVersion}&encoding=json");
        }

        internal Task InternalUpdateGuildAsync(DiscordGuild guild)
        {
            if (Guilds[guild.Id] == null)
                this._guilds.Add(guild.Id, guild);
            else
                this._guilds[guild.Id] = guild;
            return Task.Delay(0);
        }

        internal async Task InternalUpdateGatewayAsync()
        {
            string url = Utils.GetApiBaseUri(this) + Endpoints.Gateway;
            var headers = Utils.GetBaseHeaders();
            if (config.TokenType == TokenType.Bot)
                url += Endpoints.Bot;

            WebRequest request = WebRequest.CreateRequest(this, url, HttpRequestMethod.GET, headers);
            WebResponse response = await this._rest_client.Rest.HandleRequestAsync(request);

            JObject jObj = JObject.Parse(response.Response);
            _gatewayUrl = jObj.Value<string>("url");
            if (jObj["shards"] != null)
                _shardCount = jObj.Value<int>("shards");
        }

        /// <summary>
        /// Disconnects from the gateway
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DisconnectAsync()
        {
            config.AutoReconnect = false;
            await _websocket_client.InternalDisconnectAsync(null);
            return true;
        }

        #region Public Functions
        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="user">userid or @me</param>
        /// <returns></returns>
        public Task<DiscordUser> GetUserAsync(string user) => this._rest_client.InternalGetUser(user);
        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="user">Id of the user</param>
        /// <returns></returns>
        public Task<DiscordUser> GetUserAsync(ulong user) => this._rest_client.InternalGetUser(user);
        /// <summary>
        /// Deletes a channel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task DeleteChannelAsync(ulong id) => this._rest_client.InternalDeleteChannel(id);
        /// <summary>
        /// Deletes a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public Task DeleteChannelAsync(DiscordChannel channel) => this._rest_client.InternalDeleteChannel(channel.Id);
        /// <summary>
        /// Gets a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message_id"></param>
        /// <returns></returns>
        public Task<DiscordMessage> GetMessageAsync(DiscordChannel channel, ulong message_id) => this._rest_client.InternalGetMessage(channel.Id, message_id);
        /// <summary>
        /// Gets a message
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="message_id"></param>
        /// <returns></returns>
        public Task<DiscordMessage> GetMessageAsync(ulong channel_id, ulong message_id) => this._rest_client.InternalGetMessage(channel_id, message_id);
        /// <summary>
        /// Gets a channel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordChannel> GetChannelAsync(ulong id) => this._rest_client.InternalGetChannel(id);
        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel_id"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMessageAsync(ulong channel_id, string content, bool tts = false, DiscordEmbed embed = null) =>
            this._rest_client.InternalCreateMessage(channel_id, content, tts, embed);
        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content, bool tts = false, DiscordEmbed embed = null) =>
            this._rest_client.InternalCreateMessage(channel.Id, content, tts, embed);
        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMessageAsync(DiscordDmChannel channel, string content, bool tts = false, DiscordEmbed embed = null) =>
            this._rest_client.InternalCreateMessage(channel.Id, content, tts, embed);
        /// <summary>
        /// Creates a guild. Only for whitelisted bots
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Task<DiscordGuild> CreateGuildAsync(string name) => this._rest_client.InternalCreateGuildAsync(name);
        /// <summary>
        /// Gets a guild
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordGuild> GetGuildAsync(ulong id) => this._rest_client.InternalGetGuildAsync(id);
        /// <summary>
        /// Deletes a guild
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordGuild> DeleteGuildAsync(ulong id) => this._rest_client.InternalDeleteGuild(id);
        /// <summary>
        /// Deletes a guild
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public Task<DiscordGuild> DeleteGuildAsync(DiscordGuild guild) => this._rest_client.InternalDeleteGuild(guild.Id);
        /// <summary>
        /// Gets a channel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordChannel> GetChannelByIdAsync(ulong id) => this._rest_client.InternalGetChannel(id);
        /// <summary>
        /// Gets an invite
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Task<DiscordInvite> GetInviteByCodeAsync(string code) => this._rest_client.InternalGetInvite(code);
        /// <summary>
        /// Gets a list of connections
        /// </summary>
        /// <returns></returns>
        public Task<List<DiscordConnection>> GetConnectionsAsync() => this._rest_client.InternalGetUsersConnections();
        /// <summary>
        /// Gets a list of regions
        /// </summary>
        /// <returns></returns>
        public Task<List<DiscordVoiceRegion>> ListRegionsAsync() => this._rest_client.InternalListVoiceRegions();
        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookAsync(ulong id) => this._rest_client.InternalGetWebhook(id);
        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong id, string token) => this._rest_client.InternalGetWebhookWithToken(id, token);
        /// <summary>
        /// Creates a dm
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public Task<DiscordDmChannel> CreateDmAsync(ulong user_id) => this._rest_client.InternalCreateDM(user_id);
        /// <summary>
        /// Updates current user's status
        /// </summary>
        /// <param name="game">Game you're playing</param>
        /// <param name="user_status"></param>
        /// <param name="idle_since"></param>
        /// <param name="afk"></param>
        /// <returns></returns>
        public Task UpdateStatusAsync(Game game = null, UserStatus? user_status = null, long? idle_since = null, bool afk = false) => InternalUpdateStatusAsync(game, user_status, idle_since, afk);

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
        public Task ModifyMemberAsync(ulong guild_id, ulong member_id, string nickname = null, List<ulong> roles = null, bool muted = false, bool deaf = false, ulong voicechannel_id = 0) =>
            this._rest_client.InternalModifyGuildMember(guild_id, member_id, nickname, roles, muted, deaf, voicechannel_id);

        /// <summary>
        /// Gets the current API appication.
        /// </summary>
        /// <returns></returns>
        public Task<DiscordApplication> GetCurrentAppAsync() => this._rest_client.InternalGetApplicationInfo("@me");

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
        public Task<List<DiscordMember>> ListGuildMembersAsync(ulong guild_id, int limit, int after) => this._rest_client.InternalListGuildMembers(guild_id, limit, after);

        /// <summary>
        /// Sets bot avatar
        /// </summary>
        /// <param name="img">Stream with image data. Can contain a PNG, JPG, or GIF file.</param>
        /// <returns></returns>
        public Task SetAvatarAsync(Stream img) => this._rest_client.InternalSetAvatarAsync(img);

        public void WithAuditReason(string reason) { this._rest_client.Rest._reason = reason; this._rest_client.Rest._using_reason = true; }
        #endregion

        #region Websocket
        internal async Task HandleSocketMessageAsync(string data)
        {
            var payload = JsonConvert.DeserializeObject<GatewayPayload>(data);
            switch (payload.OpCode)
            {
                case GatewayOpCode.Dispatch:
                    await HandleDispatchAsync(payload);
                    break;

                case GatewayOpCode.Heartbeat:
                    await OnHeartbeatAsync((long)payload.Data);
                    break;

                case GatewayOpCode.Reconnect:
                    await OnReconnectAsync();
                    break;

                case GatewayOpCode.InvalidSession:
                    await OnInvalidateSessionAsync((bool)payload.Data);
                    break;

                case GatewayOpCode.Hello:
                    await OnHelloAsync((payload.Data as JObject).ToObject<GatewayHello>());
                    break;

                case GatewayOpCode.HeartbeatAck:
                    await OnHeartbeatAckAsync();
                    break;

                default:
                    DebugLogger.LogMessage(LogLevel.Warning, "Websocket", $"Unknown OP-Code: {(int)payload.OpCode}\n{payload.Data}", DateTime.Now);
                    break;
            }
        }

        internal async Task HandleDispatchAsync(GatewayPayload payload)
        {
            var dat = payload.Data as JObject;

            var chn = null as DiscordChannel;

            switch (payload.EventName.ToLower())
            {
                case "ready":
                    var glds = (JArray)dat["guilds"];
                    await OnReadyEventAsync(dat.ToObject<ReadyPayload>(), glds);
                    break;

                case "resumed":
                    await OnResumedAsync();
                    break;

                case "channel_create":
                    chn = dat.ToObject<DiscordChannel>();
                    await OnChannelCreateEventAsync(chn.IsPrivate ? dat.ToObject<DiscordDmChannel>() : chn);
                    break;

                case "channel_update":
                    await OnChannelUpdateEventAsync(dat.ToObject<DiscordChannel>());
                    break;

                case "channel_delete":
                    chn = dat.ToObject<DiscordChannel>();
                    await OnChannelDeleteEventAsync(chn.IsPrivate ? dat.ToObject<DiscordDmChannel>() : chn);
                    break;

                case "guild_create":
                    await OnGuildCreateEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"]);
                    break;

                case "guild_update":
                    await OnGuildUpdateEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"]);
                    break;

                case "guild_delete":
                    await OnGuildDeleteEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"]);
                    break;

                case "guild_ban_add":
                    await OnGuildBanAddEventAsync(obj);
                    break;

                case "guild_ban_remove":
                    await OnGuildBanRemoveEventAsync(obj);
                    break;

                case "guild_emojis_update":
                    await OnGuildEmojisUpdateEventAsync(obj);
                    break;

                case "guild_integrations_update":
                    await OnGuildIntegrationsUpdateEventAsync(obj);
                    break;

                case "guild_member_add":
                    await OnGuildMemberAddEventAsync(obj);
                    break;

                case "guild_member_remove":
                    await OnGuildMemberRemoveEventAsync(obj);
                    break;

                case "guild_member_update":
                    await OnGuildMemberUpdateEventAsync(obj);
                    break;

                case "guild_member_chunk":
                    await OnGuildMembersChunkEventAsync(obj);
                    break;

                case "guild_role_create":
                    await OnGuildRoleCreateEventAsync(obj);
                    break;

                case "guild_role_update":
                    await OnGuildRoleUpdateEventAsync(obj);
                    break;

                case "guild_role_delete":
                    await OnGuildRoleDeleteEventAsync(obj);
                    break;

                case "message_create":
                    await OnMessageCreateEventAsync(obj);
                    break;

                case "message_update":
                    await OnMessageUpdateEventAsync(obj);
                    break;

                case "message_delete":
                    await OnMessageDeleteEventAsync(obj);
                    break;

                case "message_delete_bulk":
                    await OnMessageBulkDeleteEventAsync(obj);
                    break;

                case "presence_update":
                    await OnPresenceUpdateEventAsync(dat.ToObject<DiscordPresence>(), dat.ToObject<PresenceUpdateEventArgs>());
                    break;

                case "typing_start":
                    await OnTypingStartEventAsync(obj);
                    break;

                case "user_settings_update":
                    await OnUserSettingsUpdateEventAsync(obj);
                    break;

                case "user_update":
                    await OnUserUpdateEventAsync(obj);
                    break;

                case "voice_state_update":
                    await OnVoiceStateUpdateEventAsync(obj);
                    break;

                case "voice_server_update":
                    await OnVoiceServerUpdateEventAsync(obj);
                    break;

                case "message_reaction_add":
                    await OnMessageReactionAddAsync(obj);
                    break;

                case "message_reaction_remove":
                    await OnMessageReactionRemoveAsync(obj);
                    break;

                case "message_reaction_remove_all":
                    await OnMessageReactionRemoveAllAsync(obj);
                    break;

                case "webhooks_update":
                    await OnWebhooksUpdateAsync(obj);
                    break;

                default:
                    await OnUnknownEventAsync(payload);
                    DebugLogger.LogMessage(LogLevel.Warning, "Websocket", $"Unknown event: {payload.EventName}\n{payload.Data}", DateTime.Now);
                    break;
            }
        }

        #region Events
        internal async Task OnReadyEventAsync(ReadyPayload ready, JArray raw_guilds)
        {
            ready.CurrentUser.Discord = this;

            this._gateway_version = ready.GatewayVersion;
            this._current_user = ready.CurrentUser;
            this._private_channels = ready.DmChannels.Select(xdc => { xdc.Discord = this; return xdc; }).ToList();
            this._session_id = ready.SessionId;

            var raw_guild_index = raw_guilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);
            this._guilds = ready.Guilds
                .Select(xg =>
                {
                    xg.Discord = this;

                    if (xg._channels == null)
                        xg._channels = new List<DiscordChannel>();
                    foreach (var xc in xg.Channels)
                    {
                        xc.GuildID = xg.Id;
                        xc.Discord = this;
                    }

                    if (xg._roles == null)
                        xg._roles = new List<DiscordRole>();
                    foreach (var xr in xg.Roles)
                        xr.Discord = this;

                    var raw_guild = raw_guild_index[xg.Id];
                    var raw_members = raw_guild["members"];
                    xg._members = raw_members == null ? new List<DiscordMember>() : raw_guild["members"].ToObject<IEnumerable<TransportMember>>()
                        .Select(xtm => new DiscordMember(xtm) { Discord = this, _guild_id = xg.Id })
                        .ToList();

                    if (xg._emojis == null)
                        xg._emojis = new List<DiscordEmoji>();
                    foreach (var xe in xg.Emojis)
                        xe.Discord = this;

                    if (xg._presences == null)
                        xg._presences = new List<DiscordPresence>();
                    if (xg._voice_states == null)
                        xg._voice_states = new List<DiscordVoiceState>();

                    return xg;
                }).ToDictionary(xg => xg.Id, xg => xg);

            await this._ready.InvokeAsync(new ReadyEventArgs(this));
        }

        internal Task OnResumedAsync()
        {
            this.DebugLogger.LogMessage(LogLevel.Info, "DSharpPlus", "Session resumed.", DateTime.Now);
            return Task.Delay(0);
        }

        internal async Task OnChannelCreateEventAsync(DiscordChannel channel)
        {
            channel.Discord = this;

            if (channel.IsPrivate)
            {
                var chn = channel as DiscordDmChannel;

                _private_channels.Add(chn);

                await this._dm_channel_created.InvokeAsync(new DmChannelCreateEventArgs(this) { Channel = chn });
            }
            else
            {
                channel.Discord = this;

                _guilds[channel.GuildID]._channels.Add(channel);

                await this._channel_created.InvokeAsync(new ChannelCreateEventArgs(this) { Channel = channel, Guild = channel.Guild });
            }
        }
        internal async Task OnChannelUpdateEventAsync(DiscordChannel channel)
        {
            channel.Discord = this;
            var old = this.InternalGetCachedChannel(channel.Id);
            var gld = channel.Guild;

            if (old != null)
            {
                var index = channel.Guild._channels.FindIndex(xc => xc.Id == channel.Id);
                gld._channels[index] = channel;
            }
            else
            {
                gld._channels.Add(channel);
            }

            await this._channel_updated.InvokeAsync(new ChannelUpdateEventArgs(this) { Channel = channel, Guild = gld, ChannelBefore = old });
        }
        internal async Task OnChannelDeleteEventAsync(DiscordChannel channel)
        {
            channel.Discord = this;

            if (channel.IsPrivate)
            {
                var chn = channel as DiscordDmChannel;

                var index = this._private_channels.FindIndex(xc => xc.Id == chn.Id);
                this._private_channels.RemoveAt(index);

                await this._dm_channel_deleted.InvokeAsync(new DmChannelDeleteEventArgs(this) { Channel = chn });
            }
            else
            {
                var gld = channel.Guild;
                var index = gld._channels.FindIndex(xc => xc.Id == channel.Id);
                gld._channels.RemoveAt(index);

                await this._channel_deleted.InvokeAsync(new ChannelDeleteEventArgs(this) { Channel = channel, Guild = gld });
            }
        }
        internal async Task OnGuildCreateEventAsync(DiscordGuild guild, JArray raw_members)
        {
            guild.Discord = this;

            if (guild._channels == null)
                guild._channels = new List<DiscordChannel>();
            foreach (var xc in guild.Channels)
            {
                xc.GuildID = guild.Id;
                xc.Discord = this;
            }

            if (guild._roles == null)
                guild._roles = new List<DiscordRole>();
            foreach (var xr in guild.Roles)
                xr.Discord = this;

            guild._members = raw_members == null ? new List<DiscordMember>() : raw_members.ToObject<IEnumerable<TransportMember>>()
                .Select(xtm => new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id })
                .ToList();

            if (guild._emojis == null)
                guild._emojis = new List<DiscordEmoji>();
            foreach (var xe in guild.Emojis)
                xe.Discord = this;

            if (guild._presences == null)
                guild._presences = new List<DiscordPresence>();
            if (guild._voice_states == null)
                guild._voice_states = new List<DiscordVoiceState>();

            var exists = this._guilds.ContainsKey(guild.Id);
            this._guilds[guild.Id] = guild;

            if (exists)
                await this._guild_available.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild });
            else
                await this._guild_created.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild });
        }
        internal async Task OnGuildUpdateEventAsync(DiscordGuild guild, JArray raw_members)
        {
            guild.Discord = this;

            if (guild._channels == null)
                guild._channels = new List<DiscordChannel>();
            foreach (var xc in guild.Channels)
            {
                xc.GuildID = guild.Id;
                xc.Discord = this;
            }

            if (guild._roles == null)
                guild._roles = new List<DiscordRole>();
            foreach (var xr in guild.Roles)
                xr.Discord = this;

            guild._members = raw_members == null ? new List<DiscordMember>() : raw_members.ToObject<IEnumerable<TransportMember>>()
                .Select(xtm => new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id })
                .ToList();

            if (guild._emojis == null)
                guild._emojis = new List<DiscordEmoji>();
            foreach (var xe in guild.Emojis)
                xe.Discord = this;

            if (guild._presences == null)
                guild._presences = new List<DiscordPresence>();
            if (guild._voice_states == null)
                guild._voice_states = new List<DiscordVoiceState>();

            var exists = this._guilds.ContainsKey(guild.Id);
            this._guilds[guild.Id] = guild;

            await this._guild_updated.InvokeAsync(new GuildUpdateEventArgs(this) { Guild = guild });
        }
        internal async Task OnGuildDeleteEventAsync(DiscordGuild guild, JArray raw_members)
        {
            if (this._guilds.ContainsKey(guild.Id))
            {
                if (guild.Unavailable)
                {
                    guild.Discord = this;

                    if (guild._channels == null)
                        guild._channels = new List<DiscordChannel>();
                    foreach (var xc in guild.Channels)
                    {
                        xc.GuildID = guild.Id;
                        xc.Discord = this;
                    }

                    if (guild._roles == null)
                        guild._roles = new List<DiscordRole>();
                    foreach (var xr in guild.Roles)
                        xr.Discord = this;

                    guild._members = raw_members == null ? new List<DiscordMember>() : raw_members.ToObject<IEnumerable<TransportMember>>()
                        .Select(xtm => new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id })
                        .ToList();

                    if (guild._emojis == null)
                        guild._emojis = new List<DiscordEmoji>();
                    foreach (var xe in guild.Emojis)
                        xe.Discord = this;

                    if (guild._presences == null)
                        guild._presences = new List<DiscordPresence>();
                    if (guild._voice_states == null)
                        guild._voice_states = new List<DiscordVoiceState>();

                    this._guilds[guild.Id] = guild;
                    await this._guild_unavailable.InvokeAsync(new GuildDeleteEventArgs(this) { Guild = guild, Unavailable = true });
                }
                else
                {
                    var gld = this._guilds[guild.Id];
                    _guilds.Remove(guild.Id);

                    await this._guild_deleted.InvokeAsync(new GuildDeleteEventArgs(this) { Guild = gld });
                }
            }
        }
        internal async Task OnPresenceUpdateEventAsync(DiscordPresence presence, PresenceUpdateEventArgs ea)
        {
            presence.Discord = this;

            var index = presence.Guild._presences.FindIndex(xp => xp.User.Id == presence.User.Id);
            var old = null as DiscordPresence;
            if (index >= 0)
            {
                old = presence.Guild.Presences[index];
                presence.Guild._presences[index] = presence;
            }
            else
            {
                presence.Guild._presences.Add(presence);
            }

            ea.Client = this;
            ea.PresenceBefore = old;

            await this._presence_update.InvokeAsync(ea);
        }
        internal async Task OnGuildBanAddEventAsync(JObject obj)
        {
            DiscordUser user = obj["d"].ToObject<DiscordUser>();
            user.Discord = this;
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            GuildBanAddEventArgs args = new GuildBanAddEventArgs(this) { User = user, GuildID = guildID };
            await this._guild_ban_add.InvokeAsync(args);
        }
        internal async Task OnGuildBanRemoveEventAsync(JObject obj)
        {
            DiscordUser user = obj["d"].ToObject<DiscordUser>();
            user.Discord = this;
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            GuildBanRemoveEventArgs args = new GuildBanRemoveEventArgs(this) { User = user, GuildID = guildID };
            await this._guild_ban_remove.InvokeAsync(args);
        }
        internal async Task OnGuildEmojisUpdateEventAsync(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            List<DiscordEmoji> emojis = new List<DiscordEmoji>();
            foreach (JObject em in (JArray)obj["d"]["emojis"])
            {
                var ret = em.ToObject<DiscordEmoji>();
                ret.Discord = this;
                emojis.Add(ret);
            }
            List<DiscordEmoji> old = _guilds[guildID].Emojis;
            _guilds[guildID].Emojis = emojis;
            GuildEmojisUpdateEventArgs args = new GuildEmojisUpdateEventArgs(this) { GuildID = guildID, Emojis = new ReadOnlyCollection<DiscordEmoji>(emojis), EmojisBefore = new ReadOnlyCollection<DiscordEmoji>(old) };
            await this._guild_emojis_update.InvokeAsync(args);
        }
        internal async Task OnGuildIntegrationsUpdateEventAsync(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            GuildIntegrationsUpdateEventArgs args = new GuildIntegrationsUpdateEventArgs(this) { GuildID = guildID };
            await this._guild_integrations_update.InvokeAsync(args);
        }
        internal async Task OnGuildMemberAddEventAsync(JObject obj)
        {
            var user = new DiscordMember(obj["d"].ToObject<TransportMember>())
            {
                Discord = this,
                _guild_id = obj["d"]["guild_id"].ToObject<ulong>()
            };

            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            _guilds[guildID].Members.Add(user);
            _guilds[guildID].MemberCount = _guilds[guildID].Members.Count;
            GuildMemberAddEventArgs args = new GuildMemberAddEventArgs(this) { Member = user, GuildID = guildID };
            await this._guild_member_add.InvokeAsync(args);
        }
        internal async Task OnGuildMemberRemoveEventAsync(JObject obj)
        {
            DiscordUser user = obj["d"]["user"].ToObject<DiscordUser>();
            user.Discord = this;
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            if (_guilds[guildID].Members.Find(x => x.Id == user.Id) != null)
            {
                int index = _guilds[guildID].Members.FindIndex(x => x.Id == user.Id);
                _guilds[guildID].Members.RemoveAt(index);
                _guilds[guildID].MemberCount = _guilds[guildID].Members.Count;
            }
            GuildMemberRemoveEventArgs args = new GuildMemberRemoveEventArgs(this) { User = user, GuildID = guildID };
            await this._guild_member_remove.InvokeAsync(args);
        }
        internal async Task OnGuildMemberUpdateEventAsync(JObject obj)
        {
            DiscordUser user = obj["d"]["user"].ToObject<DiscordUser>();
            user.Discord = this;
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
            int index = _guilds[guildID].Members.FindIndex(x => x.Id == user.Id);
            if (_guilds[guildID].Members.Find(x => x.Id == user.Id) != null)
            {
                DiscordMember m = _guilds[guildID].Members[index];
                m.Discord = this;
                m._guild_id = guildID;
                old = m;
                m.Nickname = nick;
                m._role_ids = roles;
                _guilds[guildID].Members[index] = m;
            }
            else
            {
                DiscordMember m = new DiscordMember(user)
                {
                    Discord = this,
                    Nickname = nick,
                    _role_ids = roles,
                    _guild_id = guildID
                };
                _guilds[guildID].Members.Add(m);
            }
            GuildMemberUpdateEventArgs args = new GuildMemberUpdateEventArgs(this) { User = user, GuildID = guildID, Roles = new ReadOnlyCollection<ulong>(roles), NickName = nick, NickNameBefore = old.Nickname, RolesBefore = old.RoleIds };
            await this._guild_member_update.InvokeAsync(args);
        }
        internal async Task OnGuildRoleCreateEventAsync(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            DiscordRole role = obj["d"]["role"].ToObject<DiscordRole>();
            role.Discord = this;
            _guilds[guildID].Roles.Add(role);
            GuildRoleCreateEventArgs args = new GuildRoleCreateEventArgs(this) { GuildID = guildID, Role = role };
            await this._guild_role_create.InvokeAsync(args);
        }
        internal async Task OnGuildRoleUpdateEventAsync(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            DiscordRole role = obj["d"]["role"].ToObject<DiscordRole>();
            role.Discord = this;
            DiscordRole old = role;
            int index = _guilds[guildID].Roles.FindIndex(x => x.Id == role.Id);
            old = _guilds[guildID].Roles[index];
            _guilds[guildID].Roles[index] = role;
            GuildRoleUpdateEventArgs args = new GuildRoleUpdateEventArgs(this) { GuildID = guildID, Role = role, RoleBefore = old };
            await this._guild_role_update.InvokeAsync(args);
        }
        internal async Task OnGuildRoleDeleteEventAsync(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            ulong roleID = obj["d"]["role_id"].ToObject<ulong>();
            int index = _guilds[guildID].Roles.FindIndex(x => x.Id == roleID);
            _guilds[guildID].Roles.RemoveAt(index);
            GuildRoleDeleteEventArgs args = new GuildRoleDeleteEventArgs(this) { GuildID = guildID, RoleID = roleID };
            await this._guild_role_delete.InvokeAsync(args);
        }
        internal async Task OnMessageCreateEventAsync(JObject obj)
        {
            DiscordMessage message;
            try
            {
                message = obj["d"].ToObject<DiscordMessage>();
                message.Discord = this;
            }
            catch (JsonSerializationException)
            {
                JObject msg = (JObject)obj["d"];
                msg["nonce"] = 0;
                message = msg.ToObject<DiscordMessage>();
                message.Discord = this;
            }

            if (config.TokenType != TokenType.User)
            {
                try
                {
                    if (_private_channels.Find(x => x.Id == message.ChannelID) == null)
                    {
                        int channelindex = _guilds[message.Channel.Guild.Id].Channels.FindIndex(x => x.Id == message.ChannelID);
                        _guilds[message.Channel.Guild.Id].Channels[channelindex].LastMessageID = message.Id;
                    }
                    else
                    {
                        int channelindex = _private_channels.FindIndex(x => x.Id == message.ChannelID);
                        _private_channels[channelindex].LastMessageID = message.Id;
                    }
                }
                catch (KeyNotFoundException)
                {
                    DebugLogger.LogMessage(LogLevel.Error, "Event", "Could not find channel last message belonged to?", DateTime.Now);
                }
            }

            List<DiscordMember> MentionedUsers = new List<DiscordMember>();
            List<DiscordRole> MentionedRoles = new List<DiscordRole>();
            List<DiscordChannel> MentionedChannels = new List<DiscordChannel>();
            List<DiscordEmoji> UsedEmojis = new List<DiscordEmoji>();
            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                foreach (ulong user in Utils.GetUserMentions(message))
                {
                    if (message.Channel != null && message.Channel.Guild != null && _guilds[message.Channel.Guild.Id] != null
                    && _guilds[message.Channel.Guild.Id].Members != null
                    && _guilds[message.Channel.Guild.Id].Members.Find(x => x.Id == user) != null)
                        MentionedUsers.Add(_guilds[message.Channel.Guild.Id].Members.Find(x => x.Id == user));
                }

                if (message.Channel.Guild != null && _guilds.ContainsKey(message.Channel.Guild.Id))
                {
                    foreach (ulong role in Utils.GetRoleMentions(message))
                    {
                        if (message.Channel != null && message.Channel.Guild != null && _guilds[message.Channel.Guild.Id] != null
                        && _guilds[message.Channel.Guild.Id].Roles != null
                        && _guilds[message.Channel.Guild.Id].Roles.Find(x => x.Id == role) != null)
                            MentionedRoles.Add(_guilds[message.Channel.Guild.Id].Roles.Find(x => x.Id == role));
                    }

                    foreach (ulong channel in Utils.GetChannelMentions(message))
                    {
                        if (message.Channel != null && message.Channel.Guild != null && _guilds[message.Channel.Guild.Id] != null
                        && _guilds[message.Channel.Guild.Id].Channels != null
                        && _guilds[message.Channel.Guild.Id].Channels.Find(x => x.Id == channel) != null)
                            MentionedChannels.Add(_guilds[message.Channel.Guild.Id].Channels.Find(x => x.Id == channel));
                    }
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
            MessageCreateEventArgs args = new MessageCreateEventArgs(this)
            {
                Message = message,
                MentionedUsers = new ReadOnlyCollection<DiscordMember>(MentionedUsers),
                MentionedRoles = new ReadOnlyCollection<DiscordRole>(MentionedRoles),
                MentionedChannels = new ReadOnlyCollection<DiscordChannel>(MentionedChannels),
                UsedEmojis = new ReadOnlyCollection<DiscordEmoji>(UsedEmojis)
            };
            await this._message_created.InvokeAsync(args);
        }
        internal async Task OnMessageUpdateEventAsync(JObject obj)
        {
            DiscordMessage message = obj["d"].ToObject<DiscordMessage>();
            message.Discord = this;

            List<DiscordMember> MentionedUsers = new List<DiscordMember>();
            List<DiscordRole> MentionedRoles = new List<DiscordRole>();
            List<DiscordChannel> MentionedChannels = new List<DiscordChannel>();
            List<DiscordEmoji> UsedEmojis = new List<DiscordEmoji>();
            if (message.Content != null && message.Content != "" && _guilds.ContainsKey(message.Channel.Guild.Id))
            {
                foreach (ulong user in Utils.GetUserMentions(message))
                {
                    if (message.Channel != null && message.Channel.Guild != null && _guilds[message.Channel.Guild.Id] != null
                    && _guilds[message.Channel.Guild.Id].Members != null
                    && _guilds[message.Channel.Guild.Id].Members.Find(x => x.Id == user) != null)
                        MentionedUsers.Add(_guilds[message.Channel.Guild.Id].Members.Find(x => x.Id == user));
                }

                foreach (ulong role in Utils.GetRoleMentions(message))
                {
                    if (message.Channel != null && message.Channel.Guild != null && _guilds[message.Channel.Guild.Id] != null
                    && _guilds[message.Channel.Guild.Id].Roles != null
                    && _guilds[message.Channel.Guild.Id].Roles.Find(x => x.Id == role) != null)
                        MentionedRoles.Add(_guilds[message.Channel.Guild.Id].Roles.Find(x => x.Id == role));
                }

                foreach (ulong channel in Utils.GetChannelMentions(message))
                {
                    if (message.Channel != null && message.Channel.Guild != null && _guilds[message.Channel.Guild.Id] != null
                    && _guilds[message.Channel.Guild.Id].Channels != null
                    && _guilds[message.Channel.Guild.Id].Channels.Find(x => x.Id == channel) != null)
                        MentionedChannels.Add(_guilds[message.Channel.Guild.Id].Channels.Find(x => x.Id == channel));
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

            MessageUpdateEventArgs args = new MessageUpdateEventArgs(this)
            {
                Message = message,
                MentionedUsers = new ReadOnlyCollection<DiscordMember>(MentionedUsers),
                MentionedRoles = new ReadOnlyCollection<DiscordRole>(MentionedRoles),
                MentionedChannels = new ReadOnlyCollection<DiscordChannel>(MentionedChannels),
                UsedEmojis = new ReadOnlyCollection<DiscordEmoji>(UsedEmojis)
            };
            await this._message_update.InvokeAsync(args);
        }
        internal async Task OnMessageDeleteEventAsync(JObject obj)
        {
            ulong ID = ulong.Parse(obj["d"]["id"].ToString());
            ulong channelID = ulong.Parse(obj["d"]["channel_id"].ToString());
            MessageDeleteEventArgs args = new MessageDeleteEventArgs(this) { ChannelID = channelID, MessageID = ID };
            await this._message_delete.InvokeAsync(args);
        }
        internal async Task OnMessageBulkDeleteEventAsync(JObject obj)
        {
            JArray IDsJson = (JArray)obj["d"]["ids"];
            List<ulong> ids = new List<ulong>();
            foreach (JToken t in IDsJson)
            {
                ids.Add(ulong.Parse(t.ToString()));
            }
            ulong channelID = ulong.Parse(obj["d"]["channel_id"].ToString());
            MessageBulkDeleteEventArgs args = new MessageBulkDeleteEventArgs(this) { MessageIDs = new ReadOnlyCollection<ulong>(ids), ChannelID = channelID };
            await this._message_bulk_delete.InvokeAsync(args);
        }
        internal async Task OnTypingStartEventAsync(JObject obj)
        {
            ulong channelID = ulong.Parse(obj["d"]["channel_id"].ToString());
            ulong userID = ulong.Parse(obj["d"]["user_id"].ToString());
            TypingStartEventArgs args = new TypingStartEventArgs(this) { ChannelID = channelID, UserID = userID };
            await this._typing_start.InvokeAsync(args);
        }
        internal async Task OnUserSettingsUpdateEventAsync(JObject obj)
        {
            DiscordUser user = obj["d"].ToObject<DiscordUser>();
            user.Discord = this;
            UserSettingsUpdateEventArgs args = new UserSettingsUpdateEventArgs(this) { User = user };
            await this._user_settings_update.InvokeAsync(args);
        }
        internal async Task OnUserUpdateEventAsync(JObject obj)
        {
            DiscordUser user = obj["d"].ToObject<DiscordUser>();
            user.Discord = this;
            UserUpdateEventArgs args = new UserUpdateEventArgs(this) { User = user, UserBefore = _current_user };
            _current_user = user;
            await this._user_update.InvokeAsync(args);
        }
        internal async Task OnVoiceStateUpdateEventAsync(JObject obj)
        {
            ulong userID = ulong.Parse(obj["d"]["user_id"].ToString());
            ulong guildID = 0;
            if (obj["d"]["guild_id"] != null)
                ulong.TryParse(obj["d"]["guild_id"].ToString(), out guildID);
            ulong channelID = 0;
            if (obj["d"]["channel_id"] != null)
                ulong.TryParse(obj["d"]["channel_id"].ToString(), out channelID);
            string session_id = obj["d"]["session_id"].ToString();
            VoiceStateUpdateEventArgs args = new VoiceStateUpdateEventArgs(this) { UserID = userID, GuildID = guildID, SessionID = session_id };

            var gld = this.Guilds.ContainsKey(guildID) ? this.Guilds[guildID] : null;

            if (gld != null)
            {
                var vs = new DiscordVoiceState
                {
                    GuildId = gld.Id,
                    ChannelId = channelID,
                    UserId = userID
                };
                var vs1 = gld.VoiceStates.FirstOrDefault(xvs => xvs.UserId == userID);
                if (vs1 != null)
                    gld.VoiceStates.Remove(vs1);
                if (channelID != 0)
                    gld.VoiceStates.Add(vs);
            }

            await this._voice_state_update.InvokeAsync(args);
        }
        internal async Task OnVoiceServerUpdateEventAsync(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            string endpoint = obj["d"]["endpoint"].ToString();
            string token = obj["d"]["token"].ToString();

            VoiceServerUpdateEventArgs args = new VoiceServerUpdateEventArgs(this) { GuildID = guildID, Endpoint = endpoint, VoiceToken = token };
            await this._voice_server_update.InvokeAsync(args);
        }
        internal async Task OnGuildMembersChunkEventAsync(JObject obj)
        {
            ulong guildID = ulong.Parse(obj["d"]["guild_id"].ToString());
            _guilds[guildID].Members = ((JArray)obj["d"]["members"])
                .Select(xjt => new DiscordMember(xjt.ToObject<TransportMember>()) { Discord = this, _guild_id = obj["d"]["guild_id"].ToObject<ulong>() })
                .ToList();
            _guilds[guildID].MemberCount = _guilds[guildID].Members.Count;
            GuildMembersChunkEventArgs args = new GuildMembersChunkEventArgs(this) { GuildID = guildID, Members = new ReadOnlyCollection<DiscordMember>(_guilds[guildID].Members) };
            await this._guild_members_chunk.InvokeAsync(args);
        }

        internal async Task OnUnknownEventAsync(GatewayPayload payload)
        {
            UnknownEventArgs args = new UnknownEventArgs(this) { EventName = payload.EventName, Json = (payload.Data as JObject).ToString() };
            await this._unknown_event.InvokeAsync(args);
        }

        internal async Task OnMessageReactionAddAsync(JObject obj)
        {
            ulong channelid = ulong.Parse(obj["d"]["channel_id"].ToString());
            ulong messageid = ulong.Parse(obj["d"]["message_id"].ToString());
            ulong userid = ulong.Parse(obj["d"]["user_id"].ToString());
            DiscordEmoji emoji = obj["d"]["emoji"].ToObject<DiscordEmoji>();
            emoji.Discord = this;
            MessageReactionAddEventArgs args = new MessageReactionAddEventArgs(this)
            {
                ChannelID = channelid,
                MessageID = messageid,
                UserID = userid,
                Emoji = emoji
            };
            await this._message_reaction_add.InvokeAsync(args);
        }

        internal async Task OnMessageReactionRemoveAsync(JObject obj)
        {
            ulong channelid = ulong.Parse(obj["d"]["channel_id"].ToString());
            ulong messageid = ulong.Parse(obj["d"]["message_id"].ToString());
            ulong userid = ulong.Parse(obj["d"]["user_id"].ToString());
            DiscordEmoji emoji = obj["d"]["emoji"].ToObject<DiscordEmoji>();
            emoji.Discord = this;
            MessageReactionRemoveEventArgs args = new MessageReactionRemoveEventArgs(this)
            {
                ChannelID = channelid,
                MessageID = messageid,
                UserID = userid,
                Emoji = emoji
            };
            await this._message_reaction_remove.InvokeAsync(args);
        }

        internal async Task OnMessageReactionRemoveAllAsync(JObject obj)
        {
            ulong channelid = ulong.Parse(obj["d"]["channel_id"].ToString());
            ulong messageid = ulong.Parse(obj["d"]["message_id"].ToString());
            MessageReactionRemoveAllEventArgs args = new MessageReactionRemoveAllEventArgs(this)
            {
                ChannelID = channelid,
                MessageID = messageid
            };
            await this._message_reaction_remove_all.InvokeAsync(args);
        }

        internal async Task OnWebhooksUpdateAsync(JObject obj)
        {
            ulong channelid = ulong.Parse(obj["d"]["channel_id"].ToString());
            ulong guildid = ulong.Parse(obj["d"]["guild_id"].ToString());
            WebhooksUpdateEventArgs args = new WebhooksUpdateEventArgs(this)
            {
                ChannelID = channelid,
                GuildID = guildid
            };
            await this._webhooks_update.InvokeAsync(args);
        }
        #endregion

        internal async Task OnHeartbeatAsync(long seq)
        {
            _debugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received Heartbeat - Sending Ack.", DateTime.Now);
            await SendHeartbeatAsync(seq);
        }

        internal async Task OnReconnectAsync()
        {
            _debugLogger.LogMessage(LogLevel.Info, "Websocket", "Received OP 7 - Reconnect. ", DateTime.Now);

            await ReconnectAsync();
        }

        internal async Task OnInvalidateSessionAsync(bool data)
        {
            if (data)
            {
                _debugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received true in OP 9 - Waiting a few second and sending resume again.", DateTime.Now);
                await Task.Delay(6000);
                await SendResumeAsync();
            }
            else
            {
                _debugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received false in OP 9 - Starting a new session", DateTime.Now);
                _session_id = "";
                await SendIdentifyAsync();
            }
        }

        internal async Task OnHelloAsync(GatewayHello hello)
        {
            this._debugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received OP 10 (HELLO) - Trying to either resume or identify", DateTime.Now);
            this._waiting_for_ack = false;
            this._heartbeat_interval = hello.HeartbeatInterval;
            this._heartbeat_task = new Task(StartHeartbeating, _cancel_token, TaskCreationOptions.LongRunning);
            this._heartbeat_task.Start();

            if (_session_id == "")
                await SendIdentifyAsync();
            else
                await SendResumeAsync();

            ConnectionSemaphore.Release();
        }

        internal async Task OnHeartbeatAckAsync()
        {
            _waiting_for_ack = false;

            this._sequence++;
            this.Ping = (int)(DateTime.Now - _last_heartbeat).TotalMilliseconds;
            _debugLogger.LogMessage(LogLevel.Unnecessary, "Websocket", "Received WebSocket Heartbeat Ack", DateTime.Now);
            _debugLogger.LogMessage(LogLevel.Debug, "Websocket", $"Ping {this.Ping}ms", DateTime.Now);
            HeartBeatEventArgs args = new HeartBeatEventArgs(this)
            {
                Ping = this.Ping,
                Timestamp = DateTimeOffset.Now
            };

            await _heart_beated.InvokeAsync(args);
        }

        //internal async Task StartHeartbeatingAsync()
        internal void StartHeartbeating()
        {
            _debugLogger.LogMessage(LogLevel.Unnecessary, "Websocket", "Starting Heartbeat", DateTime.Now);
            var token = this._cancel_token;
            try
            {
                while (true)
                {
                    SendHeartbeatAsync(this._sequence).GetAwaiter().GetResult();
                    Task.Delay(_heartbeat_interval, _cancel_token).GetAwaiter().GetResult();
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException) { }
        }

        internal Task InternalUpdateStatusAsync(Game game, UserStatus? user_status, long? idle_since, bool afk)
        {
            if (game != null && game.Name != null && game.Name.Length > 128)
                throw new Exception("Game name can't be longer than 128 characters!");

            var status = new StatusUpdate
            {
                Game = game,
                IdleSince = idle_since,
                IsAFK = afk,
                Status = user_status.HasValue ? user_status.Value : UserStatus.Online
            };
            var status_update = new GatewayPayload
            {
                OpCode = GatewayOpCode.StatusUpdate,
                Data = status
            };
            var statusstr = JsonConvert.SerializeObject(status_update);

            this._websocket_client.SendMessage(statusstr);
            return Task.Delay(0);
        }

        internal Task SendHeartbeatAsync()
        {
            return this.SendHeartbeatAsync(_sequence);
        }

        internal async Task SendHeartbeatAsync(long seq)
        {
            if (_waiting_for_ack)
            {
                _debugLogger.LogMessage(LogLevel.Critical, "Websocket", "Missed a heartbeat ack. Reconnecting.", DateTime.Now);
                await ReconnectAsync();
            }

            _debugLogger.LogMessage(LogLevel.Unnecessary, "Websocket", "Sending Heartbeat", DateTime.Now);
            var heartbeat = new GatewayPayload
            {
                OpCode = GatewayOpCode.Heartbeat,
                Data = seq
            };
            var heartbeat_str = JsonConvert.SerializeObject(heartbeat);
            _websocket_client.SendMessage(heartbeat_str);

            _last_heartbeat = DateTime.Now;
            _waiting_for_ack = true;
        }

        internal Task SendIdentifyAsync()
        {
            var identify = new GatewayIdentify
            {
                Token = Utils.GetFormattedToken(this),
                Compress = false,
                LargeThreshold = this.config.LargeThreshold,
                ShardInfo = new ShardInfo
                {
                    ShardId = this.config.ShardId,
                    ShardCount = this.config.ShardCount
                }
            };
            var payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Identify,
                Data = identify
            };
            var payloadstr = JsonConvert.SerializeObject(payload);
            _websocket_client.SendMessage(payloadstr);
            return Task.Delay(0);
        }

        internal Task SendResumeAsync()
        {
            var resume = new GatewayResume
            {
                Token = Utils.GetFormattedToken(this),
                SessionId = this._session_id,
                SequenceNumber = this._sequence
            };
            var resume_payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Resume,
                Data = resume
            };
            var resumestr = JsonConvert.SerializeObject(resume_payload);

            _websocket_client.SendMessage(resumestr);
            return Task.Delay(0);
        }

        internal Task SendVoiceStateUpdateAsync(DiscordGuild guild, DiscordChannel channel, bool mute = false, bool deaf = false)
        {
            var vsu = new VoiceStateUpdate
            {
                GuildId = guild.Id,
                ChannelId = channel?.Id,
                Mute = mute,
                Deafen = deaf
            };
            var vsu_payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.VoiceStateUpdate,
                Data = vsu
            };
            var vsustr = JsonConvert.SerializeObject(vsu_payload);

            _websocket_client.SendMessage(vsustr);
            return Task.Delay(0);
        }
        #endregion

        internal ulong GetGuildIdFromChannelID(ulong channel_id)
        {
            foreach (DiscordGuild guild in _guilds.Values)
            {
                if (guild.Channels != null && guild.Channels.Find(x => x.Id == channel_id) != null) return guild.Id;
            }
            return 0;
        }

        internal int GetChannelIndex(ulong channel_id)
        {
            foreach (DiscordGuild guild in _guilds.Values)
            {
                if (guild.Channels.Find(x => x.Id == channel_id) != null) return guild.Channels.FindIndex(x => x.Id == channel_id);
            }
            return 0;
        }

        // LINQ :^)
        internal DiscordUser InternalGetCachedUser(ulong user_id) =>
            this.Guilds.Values.SelectMany(xg => xg.Members)
                .GroupBy(xm => xm.Id)
                .Select(xgrp => xgrp.First())
                .FirstOrDefault(xm => xm.Id == user_id);

        // LINQ :^)
        internal DiscordChannel InternalGetCachedChannel(ulong channel_id) =>
            this.Guilds.Values.SelectMany(xg => xg.Channels)
                .Concat(this._private_channels)
                .FirstOrDefault(xc => xc.Id == channel_id);

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

            await DisconnectAsync();

            _cancel_token_source.Cancel();
            _guilds = null;
            _heartbeat_task = null;
            _current_user = null;
            _modules = null;
            _private_channels = null;
            await _websocket_client.InternalDisconnectAsync(null);

            disposed = true;
        }
    }
}
