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
        /// Triggered whenever an error occurs within an event handler.
        /// </summary>
        public event AsyncEventHandler<ClientErrorEventArgs> ClientError
        {
            add { this._client_error.Register(value); }
            remove { this._client_error.Unregister(value); }
        }
        private AsyncEvent<ClientErrorEventArgs> _client_error;

        /// <summary>
        /// Triggered whenever a WebSocket error occurs within the client.
        /// </summary>
        public event AsyncEventHandler<SocketErrorEventArgs> SocketError
        {
            add { this._socket_error.Register(value); }
            remove { this._socket_error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _socket_error;

        /// <summary>
        /// Triggered whenever WebSocket connection is established.
        /// </summary>
        public event AsyncEventHandler SocketOpened
        {
            add { this._socket_opened.Register(value); }
            remove { this._socket_opened.Unregister(value); }
        }
        private AsyncEvent _socket_opened;

        /// <summary>
        /// Triggered whenever WebSocket connection is terminated.
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
        public event AsyncEventHandler<DmChannelCreateEventArgs> DmChannelCreated
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
        public event AsyncEventHandler<DmChannelDeleteEventArgs> DmChannelDeleted
        {
            add { this._dm_channel_deleted.Register(value); }
            remove { this._dm_channel_deleted.Unregister(value); }
        }
        private AsyncEvent<DmChannelDeleteEventArgs> _dm_channel_deleted;

        /// <summary>
        /// Triggered whenever a channel's pinned message list is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelPinsUpdateEventArgs> ChannelPinsUpdated
        {
            add { this._channel_pins_updated.Register(value); }
            remove { this._channel_pins_updated.Unregister(value); }
        }
        private AsyncEvent<ChannelPinsUpdateEventArgs> _channel_pins_updated;

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
        /// Sent when message is acknowledged by the user.
        /// </summary>
        public event AsyncEventHandler<MessageAckEventArgs> MessageAck
        {
            add { this._message_ack.Register(value); }
            remove { this._message_ack.Unregister(value); }
        }
        private AsyncEvent<MessageAckEventArgs> _message_ack;

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

        /// <summary>
        /// Triggered whenever webhooks update.
        /// </summary>
        public event AsyncEventHandler<WebhooksUpdateEventArgs> WebhooksUpdate
        {
            add { this._webhooks_update.Register(value); }
            remove { this._webhooks_update.Unregister(value); }
        }
        private AsyncEvent<WebhooksUpdateEventArgs> _webhooks_update;

        /// <summary>
        /// Triggered on received heartbeat ACK.
        /// </summary>
        public event AsyncEventHandler<HeartBeatEventArgs> Heartbeated
        {
            add { this._heartbeated.Register(value); }
            remove { this._heartbeated.Unregister(value); }
        }
        private AsyncEvent<HeartBeatEventArgs> _heartbeated;

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

        internal DiscordConfig _config;

        internal List<IModule> _modules = new List<IModule>();

        internal BaseWebSocketClient _websocket_client;
        internal DiscordRestClient _rest_client;
        internal string _session_token = "";
        internal string _session_id = "";
        internal int _heartbeat_interval;
        internal Task _heartbeat_task;
        internal DateTimeOffset _last_heartbeat;
        internal long _last_sequence;
        internal int _skipped_heartbeats = 0;
        internal bool _guild_download_completed = false;
        internal static UTF8Encoding UTF8 = new UTF8Encoding(false);
        internal static DateTimeOffset DiscordEpoch = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
        #endregion

        #region Public Variables
        internal DebugLogger _debug_logger;
        /// <summary>
        /// 
        /// </summary>
        public DebugLogger DebugLogger => _debug_logger;

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
        /// Gets the total number of shards the bot is connected to.
        /// </summary>
        public int ShardCount => this._config.ShardCount;

        /// <summary>
        /// Gets the currently connected shard ID.
        /// </summary>
        public int ShardId => this._config.ShardId;

        internal DiscordUser _current_user;
        /// <summary>
        /// Gets the current user.
        /// </summary>
        public DiscordUser CurrentUser => this._current_user;

        internal DiscordApplication _current_application;
        /// <summary>
        /// Gets the current application.
        /// </summary>
        public DiscordApplication CurrentApplication => this._current_application;

        /// <summary>
        /// List of DM Channels
        /// </summary>
        public IReadOnlyList<DiscordDmChannel> PrivateChannels => this._private_channels_lazy.Value;
        internal List<DiscordDmChannel> _private_channels = new List<DiscordDmChannel>();
        private Lazy<IReadOnlyList<DiscordDmChannel>> _private_channels_lazy;

        /// <summary>
        /// List of Guilds
        /// </summary>
        public IReadOnlyDictionary<ulong, DiscordGuild> Guilds => this._guilds_lazy.Value;
        internal Dictionary<ulong, DiscordGuild> _guilds = new Dictionary<ulong, DiscordGuild>();
        private Lazy<IReadOnlyDictionary<ulong, DiscordGuild>> _guilds_lazy;

        /// <summary>
        /// Gets the WS latency for this client.
        /// </summary>
        public int Ping => Volatile.Read(ref this._ping);
        private int _ping;

        public IReadOnlyDictionary<ulong, DiscordPresence> Presences => this._presences_lazy.Value;
        internal Dictionary<ulong, DiscordPresence> _presences = new Dictionary<ulong, DiscordPresence>();
        private Lazy<IReadOnlyDictionary<ulong, DiscordPresence>> _presences_lazy;
        #endregion

        #region Connection semaphore
        private static SemaphoreSlim ConnectionSemaphore => _semaphore_init.Value;
        private static Lazy<SemaphoreSlim> _semaphore_init = new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1));
        #endregion

        /// <summary>
        /// Initializes a new instance of DiscordClient
        /// </summary>
        /// <param name="config">Overwrites the default config</param>
        public DiscordClient(DiscordConfig config)
        {
            this._config = config;

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
            this._socket_error = new AsyncEvent<SocketErrorEventArgs>(this.Goof, "SOCKET_ERROR");
            this._socket_opened = new AsyncEvent(this.EventErrorHandler, "SOCKET_OPENED");
            this._socket_closed = new AsyncEvent<SocketDisconnectEventArgs>(this.EventErrorHandler, "SOCKET_CLOSED");
            this._ready = new AsyncEvent<ReadyEventArgs>(this.EventErrorHandler, "READY");
            this._channel_created = new AsyncEvent<ChannelCreateEventArgs>(this.EventErrorHandler, "CHANNEL_CREATED");
            this._dm_channel_created = new AsyncEvent<DmChannelCreateEventArgs>(this.EventErrorHandler, "DM_CHANNEL_CREATED");
            this._channel_updated = new AsyncEvent<ChannelUpdateEventArgs>(this.EventErrorHandler, "CHANNEL_UPDATED");
            this._channel_deleted = new AsyncEvent<ChannelDeleteEventArgs>(this.EventErrorHandler, "CHANNEL_DELETED");
            this._dm_channel_deleted = new AsyncEvent<DmChannelDeleteEventArgs>(this.EventErrorHandler, "DM_CHANNEL_DELETED");
            this._channel_pins_updated = new AsyncEvent<ChannelPinsUpdateEventArgs>(this.EventErrorHandler, "CHANNEL_PINS_UPDATE");
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
            this._message_ack = new AsyncEvent<MessageAckEventArgs>(this.EventErrorHandler, "MESSAGE_ACK");
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
            this._heartbeated = new AsyncEvent<HeartBeatEventArgs>(this.EventErrorHandler, "HEART_BEATED");

            this._rest_client = new DiscordRestClient(this);
            this._debug_logger = new DebugLogger(this);

            this._private_channels = new List<DiscordDmChannel>();
            this._guilds = new Dictionary<ulong, DiscordGuild>();

            this._private_channels_lazy = new Lazy<IReadOnlyList<DiscordDmChannel>>(() => new ReadOnlyCollection<DiscordDmChannel>(this._private_channels));
            this._guilds_lazy = new Lazy<IReadOnlyDictionary<ulong, DiscordGuild>>(() => new ReadOnlyDictionary<ulong, DiscordGuild>(this._guilds));
            this._presences_lazy = new Lazy<IReadOnlyDictionary<ulong, DiscordPresence>>(() => new ReadOnlyDictionary<ulong, DiscordPresence>(this._presences));

            if (_config.UseInternalLogHandler)
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
        public async Task ConnectAsync()
        {
            var w = 1000;
            var i = 5;
            var s = false;

            while (i-- > 0)
            {
                try
                {
                    await this.InternalConnectAsync();
                    s = true;
                    break;
                }
                catch (UnauthorizedException e)
                {
                    throw new Exception("Authentication failed. Check your token and try again.", e);
                }
                catch (PlatformNotSupportedException e)
                {
                    throw e;
                }
                catch (Exception)
                {
                    await Task.Delay(w);
                    w *= 2;
                }
            }

            if (!s)
                throw new Exception("Could not connect to Discord.");
        }

        public Task ReconnectAsync(bool start_new_session = false)
        {
            if (start_new_session)
                _session_id = "";

            return _websocket_client.InternalDisconnectAsync(null);
        }

        internal Task InternalReconnectAsync() => ConnectAsync();

        internal async Task InternalConnectAsync()
        {
            var an = typeof(DiscordClient).GetTypeInfo().Assembly.GetName();
            this.DebugLogger.LogMessage(LogLevel.Info, "DSharpPlus", $"DSharpPlus, version {an.Version.ToString(3)}, booting", DateTime.Now);

            await InternalUpdateGatewayAsync();

            if (this._current_user == null)
                this._current_user = await this._rest_client.InternalGetCurrentUserAsync();

            if (this._config.TokenType != TokenType.User && this._current_application == null)
                this._current_application = await this.GetCurrentAppAsync();

            Volatile.Write(ref this._skipped_heartbeats, 0);

            _websocket_client = BaseWebSocketClient.Create();

            _cancel_token_source = new CancellationTokenSource();
            _cancel_token = _cancel_token_source.Token;

            _websocket_client.OnConnect += () => this._socket_opened.InvokeAsync();
            _websocket_client.OnDisconnect += async e =>
            {
                _cancel_token_source.Cancel();

                _debug_logger.LogMessage(LogLevel.Debug, "Websocket", $"Connection closed", DateTime.Now);
                await this._socket_closed.InvokeAsync(new SocketDisconnectEventArgs(this) { CloseCode = e.CloseCode, CloseMessage = e.CloseMessage });

                if (_config.AutoReconnect)
                {
                    DebugLogger.LogMessage(LogLevel.Critical, "Websocket", $"Socket connection terminated ({e.CloseCode}, '{e.CloseMessage}'). Reconnecting", DateTime.Now);
                    await ConnectAsync();
                }
            };
            _websocket_client.OnMessage += e => HandleSocketMessageAsync(e.Message);
            _websocket_client.OnError += e => this._socket_error.InvokeAsync(new SocketErrorEventArgs(this) { Exception = e.Exception });
            
            await ConnectionSemaphore.WaitAsync();
            await _websocket_client.ConnectAsync(_gatewayUrl + $"?v=6&encoding=json");
        }

        internal async Task InternalUpdateGatewayAsync()
        {
            string url = Utils.GetApiBaseUri(this) + Endpoints.GATEWAY;
            var headers = Utils.GetBaseHeaders();
            if (_config.TokenType == TokenType.Bot)
                url += Endpoints.BOT;

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
        public async Task DisconnectAsync()
        {
            _config.AutoReconnect = false;
            if (this._websocket_client != null)
                await _websocket_client.InternalDisconnectAsync(null);
        }

        #region Public Functions
        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="user_id">Id of the user</param>
        /// <returns></returns>
        public Task<DiscordUser> GetUserAsync(ulong user_id) => this._rest_client.InternalGetUserAsync(user_id);

        /// <summary>
        /// Deletes a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteChannelAsync(DiscordChannel channel, string reason = null) => this._rest_client.InternalDeleteChannelAsync(channel.Id, reason);

        /// <summary>
        /// Gets a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message_id"></param>
        /// <returns></returns>
        public Task<DiscordMessage> GetMessageAsync(DiscordChannel channel, ulong message_id) => this._rest_client.InternalGetMessageAsync(channel.Id, message_id);

        /// <summary>
        /// Gets a channel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordChannel> GetChannelAsync(ulong id) =>
            this.InternalGetCachedChannel(id) ?? await this._rest_client.InternalGetChannelAsync(id);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content, bool tts = false, DiscordEmbed embed = null) =>
            this._rest_client.InternalCreateMessageAsync(channel.Id, content, tts, embed);

        /// <summary>
        /// Creates a guild. Only for whitelisted bots
        /// </summary>
        /// <param name="name"></param>
        /// <param name="region"></param>
        /// <param name="icon"></param>
        /// <param name="icon_format"></param>
        /// <param name="verification_level"></param>
        /// <param name="default_message_notifications"></param>
        /// <returns></returns>
        public async Task<DiscordGuild> CreateGuildAsync(string name, string region = null, Stream icon = null, ImageFormat? icon_format = null, VerificationLevel? verification_level = null, DefaultMessageNotifications? default_message_notifications = null)
        {
            string iconb64 = null;
            if (icon != null)
            {
                if (icon_format == null)
                    throw new ArgumentNullException("When specifying new avatar, you must specify its format.");

                using (var ms = new MemoryStream((int)(icon.Length - icon.Position)))
                {
                    await icon.CopyToAsync(ms);
                    iconb64 = string.Concat("data:image/", icon_format.Value.ToString().ToLower(), ";base64,", Convert.ToBase64String(ms.ToArray()));
                }
            }

            return await this._rest_client.InternalCreateGuildAsync(name, region, iconb64, verification_level, default_message_notifications);
        }

        /// <summary>
        /// Gets a guild
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordGuild> GetGuildAsync(ulong id)
        {
            if (this._guilds.ContainsKey(id))
            {
                return this._guilds[id];
            }

            var gld = await this._rest_client.InternalGetGuildAsync(id);
            var chns = await this._rest_client.InternalGetGuildChannelsAsync(gld.Id);
            gld._channels.AddRange(chns);

            return gld;
        }

        /// <summary>
        /// Deletes a guild
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public Task DeleteGuildAsync(DiscordGuild guild) => this._rest_client.InternalDeleteGuildAsync(guild.Id);

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
        public Task<IReadOnlyCollection<DiscordConnection>> GetConnectionsAsync() => this._rest_client.InternalGetUsersConnectionsAsync();

        /// <summary>
        /// Gets a list of regions
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyCollection<DiscordVoiceRegion>> ListRegionsAsync() => this._rest_client.InternalListVoiceRegionsAsync();

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookAsync(ulong id) => this._rest_client.InternalGetWebhookAsync(id);

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong id, string token) => this._rest_client.InternalGetWebhookWithTokenAsync(id, token);

        /// <summary>
        /// Creates a dm
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<DiscordDmChannel> CreateDmAsync(DiscordUser user) => this._rest_client.InternalCreateDmAsync(user.Id);

        /// <summary>
        /// Updates current user's status
        /// </summary>
        /// <param name="game">Game you're playing</param>
        /// <param name="user_status"></param>
        /// <param name="idle_since"></param>
        /// <returns></returns>
        public Task UpdateStatusAsync(Game game = null, UserStatus? user_status = null, DateTimeOffset? idle_since = null) => InternalUpdateStatusAsync(game, user_status, idle_since);

        /// <summary>
        /// Gets the current API application.
        /// </summary>
        /// <returns></returns>
        public Task<DiscordApplication> GetCurrentAppAsync() => this._rest_client.InternalGetCurrentApplicationInfoAsync();

        /// <summary>
        /// Edits current user.
        /// </summary>
        /// <param name="username">New username.</param>
        /// <param name="avatar">New avatar.</param>
        /// <param name="avatar_format">Image format of the passed avatar.</param>
        /// <returns></returns>
        public async Task<DiscordUser> EditCurrentUserAsync(string username = null, Stream avatar = null, ImageFormat? avatar_format = null)
        {
            string av64 = null;
            if (avatar != null)
            {
                if (avatar_format == null)
                    throw new ArgumentNullException("When specifying new avatar, you must specify its format.");

                using (var ms = new MemoryStream((int)(avatar.Length - avatar.Position)))
                {
                    await avatar.CopyToAsync(ms);
                    av64 = string.Concat("data:image/", avatar_format.Value.ToString().ToLower(), ";base64,", Convert.ToBase64String(ms.ToArray()));
                }
            }

            return await this._rest_client.InternalModifyCurrentUserAsync(username, av64);
        }

        /// <summary>
        /// Requests guild sync for specified guilds. Guild sync sends information about members and presences for a given guild, and makes gateway dispatch additional events.
        /// 
        /// This can only be done for user tokens.
        /// </summary>
        /// <param name="guilds">Guilds to send a sync request for.</param>
        /// <returns></returns>
        public Task SyncGuildsAsync(params DiscordGuild[] guilds)
        {
            if (this._config.TokenType != TokenType.User)
                throw new InvalidOperationException("This can only be done for user tokens.");

            var to_sync = guilds.Where(xg => !xg.IsSynced).Select(xg => xg.Id);

            if (!to_sync.Any())
                return Task.Delay(0);

            var guild_sync = new GatewayPayload
            {
                OpCode = GatewayOpCode.GuildSync,
                Data = to_sync
            };
            var guild_syncstr = JsonConvert.SerializeObject(guild_sync);

            this._websocket_client.SendMessage(guild_syncstr);
            return Task.Delay(0);
        }
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
            var gid = 0ul;
            var cid = 0ul;
            var mid = 0ul;
            var usr = null as DiscordUser;

            switch (payload.EventName.ToLower())
            {
                case "ready":
                    var glds = (JArray)dat["guilds"];
                    var dmcs = (JArray)dat["private_channels"];
                    await OnReadyEventAsync(dat.ToObject<ReadyPayload>(), glds, dmcs);
                    break;

                case "resumed":
                    await OnResumedAsync();
                    break;

                case "channel_create":
                    chn = dat.ToObject<DiscordChannel>();
                    await OnChannelCreateEventAsync(chn.IsPrivate ? dat.ToObject<DiscordDmChannel>() : chn, dat["recipients"] as JArray);
                    break;

                case "channel_update":
                    await OnChannelUpdateEventAsync(dat.ToObject<DiscordChannel>());
                    break;

                case "channel_delete":
                    chn = dat.ToObject<DiscordChannel>();
                    await OnChannelDeleteEventAsync(chn.IsPrivate ? dat.ToObject<DiscordDmChannel>() : chn);
                    break;

                case "channel_pins_update":
                    cid = (ulong)dat["channel_id"];
                    await this.OnChannelPinsUpdate(this.InternalGetCachedChannel(cid), DateTimeOffset.Parse((string)dat["last_pin_timestamp"]));
                    break;

                case "guild_create":
                    await OnGuildCreateEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"], dat["presences"].ToObject<IEnumerable<DiscordPresence>>());
                    break;

                case "guild_update":
                    await OnGuildUpdateEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"]);
                    break;

                case "guild_delete":
                    await OnGuildDeleteEventAsync(dat.ToObject<DiscordGuild>(), (JArray)dat["members"]);
                    break;

                case "guild_sync":
                    gid = (ulong)dat["id"];
                    await this.OnGuildSyncEventAsync(this._guilds[gid], (bool)dat["large"], (JArray)dat["members"], dat["presences"].ToObject<IEnumerable<DiscordPresence>>());
                    break;

                case "guild_ban_add":
                    usr = dat.ToObject<DiscordUser>();
                    gid = (ulong)dat["guild_id"];
                    await OnGuildBanAddEventAsync(usr, this._guilds[gid]);
                    break;

                case "guild_ban_remove":
                    usr = dat.ToObject<DiscordUser>();
                    gid = (ulong)dat["guild_id"];
                    await OnGuildBanRemoveEventAsync(usr, this._guilds[gid]);
                    break;

                case "guild_emojis_update":
                    gid = (ulong)dat["guild_id"];
                    var ems = dat["emojis"].ToObject<IEnumerable<DiscordEmoji>>();
                    await OnGuildEmojisUpdateEventAsync(this._guilds[gid], ems);
                    break;

                case "guild_integrations_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildIntegrationsUpdateEventAsync(this._guilds[gid]);
                    break;

                case "guild_member_add":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMemberAddEventAsync(dat.ToObject<TransportMember>(), this._guilds[gid]);
                    break;

                case "guild_member_remove":
                    gid = (ulong)dat["guild_id"];
                    if (!this._guilds.ContainsKey(gid))
                    { this.DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"Could not find {gid} in guild cache.", DateTime.Now); return; }
                    await OnGuildMemberRemoveEventAsync(dat["user"].ToObject<TransportUser>(), this._guilds[gid]);
                    break;

                case "guild_member_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMemberUpdateEventAsync(dat["user"].ToObject<TransportUser>(), this._guilds[gid], dat["roles"].ToObject<IEnumerable<ulong>>(), (string)dat["nick"]);
                    break;

                case "guild_member_chunk":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMembersChunkEventAsync(dat["members"].ToObject<IEnumerable<TransportMember>>(), this._guilds[gid]);
                    break;

                case "guild_role_create":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleCreateEventAsync(dat["role"].ToObject<DiscordRole>(), this._guilds[gid]);
                    break;

                case "guild_role_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleUpdateEventAsync(dat["role"].ToObject<DiscordRole>(), this._guilds[gid]);
                    break;

                case "guild_role_delete":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleDeleteEventAsync((ulong)dat["role_id"], this._guilds[gid]);
                    break;

                case "message_ack":
                    cid = (ulong)dat["channel_id"];
                    mid = (ulong)dat["message_id"];
                    await OnMessageAckEventAsync(this.InternalGetCachedChannel(cid), mid);
                    break;

                case "message_create":
                    await OnMessageCreateEventAsync(dat.ToObject<DiscordMessage>(), dat["author"].ToObject<TransportUser>());
                    break;

                case "message_update":
                    await OnMessageUpdateEventAsync(dat.ToObject<DiscordMessage>(), dat["author"] != null ? dat["author"].ToObject<TransportUser>() : null);
                    break;

                case "message_delete":
                    await OnMessageDeleteEventAsync((ulong)dat["id"], this.InternalGetCachedChannel((ulong)dat["channel_id"]));
                    break;

                case "message_delete_bulk":
                    await OnMessageBulkDeleteEventAsync(dat["ids"].ToObject<IEnumerable<ulong>>(), this.InternalGetCachedChannel((ulong)dat["channel_id"]));
                    break;

                case "presence_update":
                    await OnPresenceUpdateEventAsync(dat.ToObject<DiscordPresence>(), dat.ToObject<PresenceUpdateEventArgs>());
                    break;

                case "typing_start":
                    cid = (ulong)dat["channel_id"];
                    await OnTypingStartEventAsync((ulong)dat["user_id"], this.InternalGetCachedChannel(cid), Utils.GetDateTimeOffset((long)dat["timestamp"]));
                    break;

                case "user_settings_update":
                    await OnUserSettingsUpdateEventAsync(dat.ToObject<TransportUser>());
                    break;

                case "user_update":
                    await OnUserUpdateEventAsync(dat.ToObject<TransportUser>());
                    break;

                case "voice_state_update":
                    await OnVoiceStateUpdateEventAsync(dat.ToObject<DiscordVoiceState>());
                    break;

                case "voice_server_update":
                    gid = (ulong)dat["guild_id"];
                    await OnVoiceServerUpdateEventAsync((string)dat["endpoint"], (string)dat["token"], this._guilds[gid]);
                    break;

                case "message_reaction_add":
                    cid = (ulong)dat["channel_id"];
                    await OnMessageReactionAddAsync((ulong)dat["user_id"], (ulong)dat["message_id"], this.InternalGetCachedChannel(cid), dat["emoji"].ToObject<DiscordEmoji>());
                    break;

                case "message_reaction_remove":
                    cid = (ulong)dat["channel_id"];
                    await OnMessageReactionRemoveAsync((ulong)dat["user_id"], (ulong)dat["message_id"], this.InternalGetCachedChannel(cid), dat["emoji"].ToObject<DiscordEmoji>());
                    break;

                case "message_reaction_remove_all":
                    cid = (ulong)dat["channel_id"];
                    await OnMessageReactionRemoveAllAsync((ulong)dat["message_id"], this.InternalGetCachedChannel(cid));
                    break;

                case "webhooks_update":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await OnWebhooksUpdateAsync(this._guilds[gid]._channels.FirstOrDefault(xc => xc.Id == cid), this._guilds[gid]);
                    break;

                default:
                    await OnUnknownEventAsync(payload);
                    DebugLogger.LogMessage(LogLevel.Warning, "Websocket", $"Unknown event: {payload.EventName}\n{payload.Data}", DateTime.Now);
                    break;
            }
        }

        #region Events
        internal async Task OnReadyEventAsync(ReadyPayload ready, JArray raw_guilds, JArray raw_dm_channels)
        {
            ready.CurrentUser.Discord = this;

            this._gateway_version = ready.GatewayVersion;
            this._current_user = ready.CurrentUser;
            this._session_id = ready.SessionId;
            var raw_guild_index = raw_guilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);

            this._private_channels = raw_dm_channels
                .Select(xjt =>
                {
                    var xdc = xjt.ToObject<DiscordDmChannel>();

                    xdc.Discord = this;

                    if (this._config.MessageCacheSize > 0)
                        xdc.MessageCache = new RingBuffer<DiscordMessage>(this._config.MessageCacheSize);

                    xdc._recipients = xjt["recipients"].ToObject<IEnumerable<TransportUser>>()
                        .Select(xtu => this.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this })
                        .ToList();

                    return xdc;
                }).ToList();

            this._guilds = ready.Guilds
                .Select(xg =>
                {
                    xg.Discord = this;

                    if (xg._channels == null)
                        xg._channels = new List<DiscordChannel>();

                    foreach (var xc in xg.Channels)
                    {
                        xc.GuildId = xg.Id;
                        xc.Discord = this;
                        if (xc.Type == ChannelType.Text)
                            xc.MessageCache = xc.MessageCache ?? (this._config.MessageCacheSize > 0 ? new RingBuffer<DiscordMessage>(this._config.MessageCacheSize) : null);
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

                    if (xg._voice_states == null)
                        xg._voice_states = new List<DiscordVoiceState>();

                    foreach (var xvs in xg.VoiceStates)
                        xvs.Discord = this;

                    return xg;
                }).ToDictionary(xg => xg.Id, xg => xg);

            this._guilds_lazy = new Lazy<IReadOnlyDictionary<ulong, DiscordGuild>>(() => new ReadOnlyDictionary<ulong, DiscordGuild>(this._guilds));

            if (this._config.TokenType == TokenType.User && this._config.AutomaticGuildSync)
                await this.SendGuildSyncAsync();
            else if (this._config.TokenType == TokenType.User)
                Volatile.Write(ref this._guild_download_completed, true);

            await this._ready.InvokeAsync(new ReadyEventArgs(this));
        }

        internal Task OnResumedAsync()
        {
            this.DebugLogger.LogMessage(LogLevel.Info, "DSharpPlus", "Session resumed.", DateTime.Now);
            return Task.Delay(0);
        }

        internal async Task OnChannelCreateEventAsync(DiscordChannel channel, JArray raw_recipients)
        {
            channel.Discord = this;

            if (this._config.MessageCacheSize > 0)
                channel.MessageCache = new RingBuffer<DiscordMessage>(this._config.MessageCacheSize);

            //if (channel.IsPrivate)
            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var chn = channel as DiscordDmChannel;

                var recips = raw_recipients.ToObject<IEnumerable<TransportUser>>()
                    .Select(xtu => this.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this });
                chn._recipients = recips.ToList();

                _private_channels.Add(chn);

                await this._dm_channel_created.InvokeAsync(new DmChannelCreateEventArgs(this) { Channel = chn });
            }
            else
            {
                channel.Discord = this;

                _guilds[channel.GuildId]._channels.Add(channel);

                await this._channel_created.InvokeAsync(new ChannelCreateEventArgs(this) { Channel = channel, Guild = channel.Guild });
            }
        }

        internal async Task OnChannelUpdateEventAsync(DiscordChannel channel)
        {
            if (channel == null)
                return;

            channel.Discord = this;

            var gld = channel.Guild;

            var channel_new = this.InternalGetCachedChannel(channel.Id);
            var channel_old = null as DiscordChannel;

            if (channel_new != null)
                channel_old = new DiscordChannel
                {
                    Bitrate = channel_new.Bitrate,
                    Discord = this,
                    GuildId = channel_new.GuildId,
                    Id = channel_new.Id,
                    //IsPrivate = channel_new.IsPrivate,
                    LastMessageId = channel_new.LastMessageId,
                    Name = channel_new.Name,
                    _permission_overwrites = channel_new._permission_overwrites,
                    Position = channel_new.Position,
                    Topic = channel_new.Topic,
                    Type = channel_new.Type,
                    UserLimit = channel_new.UserLimit
                };
            else
                gld._channels.Add(channel);

            channel_new.Bitrate = channel.Bitrate;
            channel_new.Name = channel.Name;
            channel_new._permission_overwrites = channel._permission_overwrites;
            channel_new.Position = channel.Position;
            channel_new.Topic = channel.Topic;
            channel_new.UserLimit = channel.UserLimit;

            await this._channel_updated.InvokeAsync(new ChannelUpdateEventArgs(this) { ChannelAfter = channel_new, Guild = gld, ChannelBefore = channel_old });
        }

        internal async Task OnChannelDeleteEventAsync(DiscordChannel channel)
        {
            if (channel == null)
                return;

            channel.Discord = this;

            //if (channel.IsPrivate)
            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var chn = channel as DiscordDmChannel;

                var index = this._private_channels.FindIndex(xc => xc.Id == chn.Id);
                chn = this._private_channels[index];
                this._private_channels.RemoveAt(index);

                await this._dm_channel_deleted.InvokeAsync(new DmChannelDeleteEventArgs(this) { Channel = chn });
            }
            else
            {
                var gld = channel.Guild;
                var index = gld._channels.FindIndex(xc => xc.Id == channel.Id);
                channel = gld._channels[index];
                gld._channels.RemoveAt(index);

                await this._channel_deleted.InvokeAsync(new ChannelDeleteEventArgs(this) { Channel = channel, Guild = gld });
            }
        }

        internal async Task OnChannelPinsUpdate(DiscordChannel channel, DateTimeOffset last_pin_timestamp)
        {
            if (channel == null)
                return;

            var ea = new ChannelPinsUpdateEventArgs(this)
            {
                Channel = channel,
                LastPinTimestamp = last_pin_timestamp
            };
            await this._channel_pins_updated.InvokeAsync(ea);
        }

        internal async Task OnGuildCreateEventAsync(DiscordGuild guild, JArray raw_members, IEnumerable<DiscordPresence> presences)
        {
            if (presences != null)
            {
                presences = presences.Select(xp => { xp.Discord = this; return xp; });
                foreach (var xp in presences)
                    this._presences[xp.InternalUser.Id] = xp;
            }

            var exists = this._guilds.ContainsKey(guild.Id);

            guild.Discord = this;
            guild.IsUnavailable = false;
            var event_guild = guild;
            if (exists)
                guild = this._guilds[event_guild.Id];

            if (guild._channels == null)
                guild._channels = new List<DiscordChannel>();
            if (guild._roles == null)
                guild._roles = new List<DiscordRole>();
            if (guild._emojis == null)
                guild._emojis = new List<DiscordEmoji>();
            if (guild._voice_states == null)
                guild._voice_states = new List<DiscordVoiceState>();
            if (guild._members == null)
                guild._members = new List<DiscordMember>();

            this.UpdateCachedGuild(event_guild, raw_members);

            guild.JoinedAt = event_guild.JoinedAt;
            guild.IsLarge = event_guild.IsLarge;
            guild.MemberCount = Math.Max(event_guild.MemberCount, guild._members.Count);
            guild.IsUnavailable = event_guild.IsUnavailable;
            guild._voice_states.AddRange(event_guild._voice_states);

            foreach (var xc in guild._channels)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                if (xc.Type == ChannelType.Text)
                    xc.MessageCache = xc.MessageCache ?? (this._config.MessageCacheSize > 0 ? new RingBuffer<DiscordMessage>(this._config.MessageCacheSize) : null);
            }
            foreach (var xe in guild._emojis)
                xe.Discord = this;
            foreach (var xvs in guild._voice_states)
                xvs.Discord = this;
            foreach (var xr in guild._roles)
                xr.Discord = this;

            var dcompl = this._guilds.Values.All(xg => !xg.IsUnavailable);
            Volatile.Write(ref this._guild_download_completed, dcompl);

            if (exists)
                await this._guild_available.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild });
            else
                await this._guild_created.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild });
        }

        internal async Task OnGuildUpdateEventAsync(DiscordGuild guild, JArray raw_members)
        {
            if (!this._guilds.ContainsKey(guild.Id))
                return;

            guild.Discord = this;
            guild.IsUnavailable = false;
            var event_guild = guild;
            guild = this._guilds[event_guild.Id];

            if (guild._channels == null)
                guild._channels = new List<DiscordChannel>();
            if (guild._roles == null)
                guild._roles = new List<DiscordRole>();
            if (guild._emojis == null)
                guild._emojis = new List<DiscordEmoji>();
            if (guild._voice_states == null)
                guild._voice_states = new List<DiscordVoiceState>();
            if (guild._members == null)
                guild._members = new List<DiscordMember>();

            this.UpdateCachedGuild(event_guild, raw_members);

            foreach (var xc in guild._channels)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                if (xc.Type == ChannelType.Text)
                    xc.MessageCache = xc.MessageCache ?? (this._config.MessageCacheSize > 0 ? new RingBuffer<DiscordMessage>(this._config.MessageCacheSize) : null);
            }
            foreach (var xe in guild._emojis)
                xe.Discord = this;
            foreach (var xvs in guild._voice_states)
                xvs.Discord = this;
            foreach (var xr in guild._roles)
                xr.Discord = this;

            await this._guild_updated.InvokeAsync(new GuildUpdateEventArgs(this) { Guild = guild });
        }

        internal async Task OnGuildDeleteEventAsync(DiscordGuild guild, JArray raw_members)
        {
            if (!this._guilds.ContainsKey(guild.Id))
                return;

            var gld = this._guilds[guild.Id];
            if (guild.IsUnavailable)
            {
                gld.IsUnavailable = true;

                await this._guild_unavailable.InvokeAsync(new GuildDeleteEventArgs(this) { Guild = guild, Unavailable = true });
            }
            else
            {
                _guilds.Remove(guild.Id);

                await this._guild_deleted.InvokeAsync(new GuildDeleteEventArgs(this) { Guild = gld });
            }
        }

        internal async Task OnGuildSyncEventAsync(DiscordGuild guild, bool is_large, JArray raw_members, IEnumerable<DiscordPresence> presences)
        {
            presences = presences.Select(xp => { xp.Discord = this; return xp; });
            foreach (var xp in presences)
                this._presences[xp.InternalUser.Id] = xp;

            guild.IsSynced = true;
            guild.IsLarge = is_large;
            
            this.UpdateCachedGuild(guild, raw_members);

            if (this._config.AutomaticGuildSync)
            {
                var dcompl = this._guilds.Values.All(xg => xg.IsSynced);
                Volatile.Write(ref this._guild_download_completed, dcompl);
            }

            await this._guild_available.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild });
        }

        internal async Task OnPresenceUpdateEventAsync(DiscordPresence presence, PresenceUpdateEventArgs ea)
        {
            presence.Discord = this;
            DiscordPresence old = null;

            if (this._presences.ContainsKey(presence.InternalUser.Id))
                old = this._presences[presence.InternalUser.Id];
            this._presences[presence.InternalUser.Id] = presence;

            ea.Client = this;
            ea.PresenceBefore = old;

            await this._presence_update.InvokeAsync(ea);
        }

        internal async Task OnGuildBanAddEventAsync(DiscordUser user, DiscordGuild guild)
        {
            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(user) { Discord = this, _guild_id = guild.Id };
            var ea = new GuildBanAddEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guild_ban_add.InvokeAsync(ea);
        }

        internal async Task OnGuildBanRemoveEventAsync(DiscordUser user, DiscordGuild guild)
        {
            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(user) { Discord = this, _guild_id = guild.Id };
            var ea = new GuildBanRemoveEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guild_ban_remove.InvokeAsync(ea);
        }

        internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild guild, IEnumerable<DiscordEmoji> new_emojis)
        {
            var old_emojis = new List<DiscordEmoji>(guild._emojis);
            guild._emojis.Clear();
            guild._emojis.AddRange(new_emojis.Select(xe => { xe.Discord = this; return xe; }));
            var ea = new GuildEmojisUpdateEventArgs(this)
            {
                Guild = guild,
                EmojisAfter = guild.Emojis,
                EmojisBefore = new ReadOnlyCollection<DiscordEmoji>(old_emojis)
            };
            await this._guild_emojis_update.InvokeAsync(ea);
        }

        internal async Task OnGuildIntegrationsUpdateEventAsync(DiscordGuild guild)
        {
            var ea = new GuildIntegrationsUpdateEventArgs(this)
            {
                Guild = guild
            };
            await this._guild_integrations_update.InvokeAsync(ea);
        }

        internal async Task OnGuildMemberAddEventAsync(TransportMember member, DiscordGuild guild)
        {
            var mbr = new DiscordMember(member)
            {
                Discord = this,
                _guild_id = guild.Id
            };

            guild._members.Add(mbr);
            guild.MemberCount++;

            var ea = new GuildMemberAddEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guild_member_add.InvokeAsync(ea);
        }

        internal async Task OnGuildMemberRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(new DiscordUser(user)) { Discord = this, _guild_id = guild.Id };

            var index = guild._members.FindIndex(xm => xm.Id == mbr.Id);
            if (index > -1)
                guild._members.RemoveAt(index);
            guild.MemberCount--;

            var ea = new GuildMemberRemoveEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guild_member_remove.InvokeAsync(ea);
        }

        internal async Task OnGuildMemberUpdateEventAsync(TransportUser user, DiscordGuild guild, IEnumerable<ulong> roles, string nick)
        {
            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(new DiscordUser(user)) { Discord = this, _guild_id = guild.Id };

            var nick_old = mbr.Nickname;
            var roles_old = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles));

            mbr.Nickname = nick;
            mbr._role_ids.Clear();
            mbr._role_ids.AddRange(roles);

            var ea = new GuildMemberUpdateEventArgs(this)
            {
                Guild = guild,
                Member = mbr,

                NicknameAfter = mbr.Nickname,
                RolesAfter = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles)),

                NicknameBefore = nick_old,
                RolesBefore = roles_old
            };
            await this._guild_member_update.InvokeAsync(ea);
        }

        internal async Task OnGuildRoleCreateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            role.Discord = this;
            guild._roles.Add(role);

            var ea = new GuildRoleCreateEventArgs(this)
            {
                Guild = guild,
                Role = role
            };
            await this._guild_role_create.InvokeAsync(ea);
        }

        internal async Task OnGuildRoleUpdateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            var role_new = guild.Roles.FirstOrDefault(xr => xr.Id == role.Id);
            var role_old = new DiscordRole
            {
                Color = role_new.Color,
                Discord = this,
                Hoist = role_new.Hoist,
                Id = role_new.Id,
                Managed = role_new.Managed,
                Mentionable = role_new.Managed,
                Name = role_new.Name,
                Permissions = role_new.Permissions,
                Position = role_new.Position
            };

            role_new.Color = role.Color;
            role_new.Hoist = role.Hoist;
            role_new.Managed = role.Managed;
            role_new.Mentionable = role.Mentionable;
            role_new.Name = role.Name;
            role_new.Permissions = role.Permissions;
            role_new.Position = role.Position;

            var ea = new GuildRoleUpdateEventArgs(this)
            {
                Guild = guild,
                RoleAfter = role_new,
                RoleBefore = role_old
            };
            await this._guild_role_update.InvokeAsync(ea);
        }

        internal async Task OnGuildRoleDeleteEventAsync(ulong role_id, DiscordGuild guild)
        {
            var index = guild._roles.FindIndex(xr => xr.Id == role_id);
            var role = guild._roles[index];
            guild._roles.RemoveAt(index);

            var ea = new GuildRoleDeleteEventArgs(this)
            {
                Guild = guild,
                Role = role
            };
            await this._guild_role_delete.InvokeAsync(ea);
        }

        internal async Task OnMessageAckEventAsync(DiscordChannel chn, ulong msgid)
        {
            DiscordMessage msg = null;
            if (chn?.MessageCache?.TryGet(xm => xm.Id == msgid, out msg) != true)
                msg = new DiscordMessage
                {
                    Discord = this,
                    Id = msgid,
                    ChannelId = chn.Id
                };

            await this._message_ack.InvokeAsync(new MessageAckEventArgs(this) { Message = msg });
        }

        internal async Task OnMessageCreateEventAsync(DiscordMessage message, TransportUser author)
        {
            message.Discord = this;

            if (message.Channel == null)
                DebugLogger.LogMessage(LogLevel.Warning, "Event", "Could not find channel last message belonged to", DateTime.Now);
            else
                message.Channel.LastMessageId = message.Id;

            var guild = message.Channel?.Guild;

            if (guild != null)
                message.Author = guild.Members.FirstOrDefault(xm => xm.Id == author.Id) ?? new DiscordMember(new DiscordUser(author)) { Discord = this, _guild_id = guild.Id };
            else
                message.Author = this.InternalGetCachedUser(author.Id) ?? new DiscordUser(author) { Discord = this };

            var mentioned_users = new List<DiscordUser>();
            var mentioned_roles = guild != null ? new List<DiscordRole>() : null;
            var mentioned_channels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                if (guild != null)
                {
                    mentioned_users = Utils.GetUserMentions(message).Select(xid => guild._members.FirstOrDefault(xm => xm.Id == xid)).Cast<DiscordUser>().ToList();
                    mentioned_roles = Utils.GetRoleMentions(message).Select(xid => guild._roles.FirstOrDefault(xr => xr.Id == xid)).ToList();
                    mentioned_channels = Utils.GetChannelMentions(message).Select(xid => guild._channels.FirstOrDefault(xc => xc.Id == xid)).ToList();
                }
                else
                {
                    mentioned_users = Utils.GetUserMentions(message).Select(xid => this.InternalGetCachedUser(xid)).ToList();
                }
            }

            message._mentioned_users = mentioned_users;
            message._mentioned_roles = mentioned_roles;
            message._mentioned_channels = mentioned_channels;

            if (message._reactions == null)
                message._reactions = new List<DiscordReaction>();
            foreach (var xr in message._reactions)
                xr.Emoji.Discord = this;

            if (this._config.MessageCacheSize > 0 && message.Channel != null)
                message.Channel.MessageCache.Add(message);

            MessageCreateEventArgs ea = new MessageCreateEventArgs(this)
            {
                Message = message,

                MentionedUsers = new ReadOnlyCollection<DiscordUser>(mentioned_users),
                MentionedRoles = mentioned_roles != null ? new ReadOnlyCollection<DiscordRole>(mentioned_roles) : null,
                MentionedChannels = mentioned_channels != null ? new ReadOnlyCollection<DiscordChannel>(mentioned_channels) : null
            };
            await this._message_created.InvokeAsync(ea);
        }

        internal async Task OnMessageUpdateEventAsync(DiscordMessage message, TransportUser author)
        {
            DiscordGuild guild = null;

            message.Discord = this;
            var event_message = message;

            if (this._config.MessageCacheSize > 0 && message.Channel?.MessageCache.TryGet(xm => xm.Id == event_message.Id, out message) != true)
            {
                message = event_message;
                guild = message.Channel?.Guild;

                if (author != null)
                {
                    if (guild != null)
                        message.Author = guild.Members.FirstOrDefault(xm => xm.Id == author.Id) ?? new DiscordMember(new DiscordUser(author)) { Discord = this, _guild_id = guild.Id };
                    else
                        message.Author = this.InternalGetCachedUser(author.Id) ?? new DiscordUser(author) { Discord = this };
                }

                if (message._reactions == null)
                    message._reactions = new List<DiscordReaction>();
                foreach (var xr in message._reactions)
                    xr.Emoji.Discord = this;
            }
            else
            {
                guild = message.Channel?.Guild;
                message.EditedTimestampRaw = event_message.EditedTimestampRaw;
                if (event_message.Content != null)
                    message.Content = event_message.Content;
                message._embeds.Clear();
                message._embeds.AddRange(event_message._embeds);
                message.Pinned = event_message.Pinned;
                message.IsTTS = event_message.IsTTS;
            }

            var mentioned_users = new List<DiscordUser>();
            var mentioned_roles = guild != null ? new List<DiscordRole>() : null;
            var mentioned_channels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                if (guild != null)
                {
                    mentioned_users = Utils.GetUserMentions(message).Select(xid => guild._members.FirstOrDefault(xm => xm.Id == xid)).Cast<DiscordUser>().ToList();
                    mentioned_roles = Utils.GetRoleMentions(message).Select(xid => guild._roles.FirstOrDefault(xr => xr.Id == xid)).ToList();
                    mentioned_channels = Utils.GetChannelMentions(message).Select(xid => guild._channels.FirstOrDefault(xc => xc.Id == xid)).ToList();
                }
                else
                {
                    mentioned_users = Utils.GetUserMentions(message).Select(xid => this.InternalGetCachedUser(xid)).ToList();
                }
            }

            message._mentioned_users = mentioned_users;
            message._mentioned_roles = mentioned_roles;
            message._mentioned_channels = mentioned_channels;

            var ea = new MessageUpdateEventArgs(this)
            {
                Message = message,

                MentionedUsers = new ReadOnlyCollection<DiscordUser>(mentioned_users),
                MentionedRoles = mentioned_roles != null ? new ReadOnlyCollection<DiscordRole>(mentioned_roles) : null,
                MentionedChannels = mentioned_channels != null ? new ReadOnlyCollection<DiscordChannel>(mentioned_channels) : null
            };
            await this._message_update.InvokeAsync(ea);
        }

        internal async Task OnMessageDeleteEventAsync(ulong message_id, DiscordChannel channel)
        {
            if (this._config.MessageCacheSize == 0 || !channel.MessageCache.TryGet(xm => xm.Id == message_id, out var msg))
            {
                msg = new DiscordMessage { Id = message_id, Discord = this };
            }
            if (this._config.MessageCacheSize > 0)
                channel.MessageCache.Remove(xm => xm.Id == msg.Id);

            var ea = new MessageDeleteEventArgs(this)
            {
                Channel = channel,
                Message = msg
            };
            await this._message_delete.InvokeAsync(ea);
        }

        internal async Task OnMessageBulkDeleteEventAsync(IEnumerable<ulong> message_ids, DiscordChannel channel)
        {
            var msgs = new List<DiscordMessage>(message_ids.Count());
            foreach (var message_id in message_ids)
            {
                DiscordMessage msg = null;
                if (this._config.MessageCacheSize > 0 && !channel.MessageCache.TryGet(xm => xm.Id == message_id, out msg))
                {
                    msg = new DiscordMessage { Id = message_id, Discord = this };
                }
                if (this._config.MessageCacheSize > 0)
                    channel.MessageCache.Remove(xm => xm.Id == msg.Id);
                msgs.Add(msg);
            }

            var ea = new MessageBulkDeleteEventArgs(this)
            {
                Channel = channel,
                Messages = new ReadOnlyCollection<DiscordMessage>(msgs)
            };
            await this._message_bulk_delete.InvokeAsync(ea);
        }

        internal async Task OnTypingStartEventAsync(ulong user_id, DiscordChannel channel, DateTimeOffset started)
        {
            if (channel == null)
                return;

            var usr = channel.Guild != null ? channel.Guild.Members.FirstOrDefault(xm => xm.Id == user_id) : this.InternalGetCachedUser(user_id);

            var ea = new TypingStartEventArgs(this)
            {
                Channel = channel,
                User = usr,
                StartedAt = started
            };
            await this._typing_start.InvokeAsync(ea);
        }

        internal async Task OnUserSettingsUpdateEventAsync(TransportUser user)
        {
            var usr = new DiscordUser(user) { Discord = this };

            var ea = new UserSettingsUpdateEventArgs(this)
            {
                User = usr
            };
            await this._user_settings_update.InvokeAsync(ea);
        }

        internal async Task OnUserUpdateEventAsync(TransportUser user)
        {
            var usr_old = new DiscordUser
            {
                AvatarHash = this._current_user.AvatarHash,
                Discord = this,
                DiscriminatorInt = this._current_user.DiscriminatorInt,
                Email = this._current_user.Email,
                Id = this._current_user.Id,
                IsBot = this._current_user.IsBot,
                MfaEnabled = this._current_user.MfaEnabled,
                Username = this._current_user.Username,
                Verified = this._current_user.Verified
            };

            this._current_user.AvatarHash = user.AvatarHash;
            this._current_user.DiscriminatorInt = user.DiscriminatorInt;
            this._current_user.Email = user.Email;
            this._current_user.Id = user.Id;
            this._current_user.IsBot = user.IsBot;
            this._current_user.MfaEnabled = user.MfaEnabled;
            this._current_user.Username = user.Username;
            this._current_user.Verified = user.Verified;

            var ea = new UserUpdateEventArgs(this)
            {
                UserAfter = this.CurrentUser,
                UserBefore = usr_old
            };
            await this._user_update.InvokeAsync(ea);
        }

        internal async Task OnVoiceStateUpdateEventAsync(DiscordVoiceState voice_state)
        {
            voice_state.Discord = this;

            var index = voice_state.Guild._voice_states.FindIndex(xvs => xvs.UserId == voice_state.UserId);
            if (index < 0)
                voice_state.Guild._voice_states.Add(voice_state);
            else
                voice_state.Guild._voice_states[index] = voice_state;

            var ea = new VoiceStateUpdateEventArgs(this)
            {
                Guild = voice_state.Guild,
                Channel = voice_state.Channel,
                User = voice_state.User,
                SessionId = voice_state.SessionId
            };
            await this._voice_state_update.InvokeAsync(ea);
        }

        internal async Task OnVoiceServerUpdateEventAsync(string endpoint, string token, DiscordGuild guild)
        {
            var ea = new VoiceServerUpdateEventArgs(this)
            {
                Endpoint = endpoint,
                VoiceToken = token,
                Guild = guild
            };
            await this._voice_server_update.InvokeAsync(ea);
        }

        internal async Task OnGuildMembersChunkEventAsync(IEnumerable<TransportMember> members, DiscordGuild guild)
        {
            var ids = guild.Members.Select(xm => xm.Id);
            var mbrs = members.Select(xtm => new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id })
                .Where(xm => !ids.Contains(xm.Id));

            guild._members.AddRange(mbrs);
            guild.MemberCount = guild._members.Count;

            var ea = new GuildMembersChunkEventArgs(this)
            {
                Guild = guild,
                Members = new ReadOnlyCollection<DiscordMember>(new List<DiscordMember>(mbrs))
            };
            await this._guild_members_chunk.InvokeAsync(ea);
        }

        internal async Task OnUnknownEventAsync(GatewayPayload payload)
        {
            var ea = new UnknownEventArgs(this) { EventName = payload.EventName, Json = (payload.Data as JObject)?.ToString() };
            await this._unknown_event.InvokeAsync(ea);
        }

        internal async Task OnMessageReactionAddAsync(ulong user_id, ulong message_id, DiscordChannel channel, DiscordEmoji emoji)
        {
            emoji.Discord = this;

            var usr = null as DiscordUser;
            if (channel.Guild != null)
                usr = channel.Guild._members.FirstOrDefault(xm => xm.Id == user_id) ?? await this._rest_client.InternalGetGuildMemberAsync(channel.Guild.Id, user_id);
            else
                usr = this.InternalGetCachedUser(user_id) ?? await this._rest_client.InternalGetUserAsync(user_id);

            DiscordMessage msg = null;
            if (this._config.MessageCacheSize == 0 || !channel.MessageCache.TryGet(xm => xm.Id == message_id, out msg))
                msg = new DiscordMessage { Id = message_id, Discord = this };

            var ea = new MessageReactionAddEventArgs(this)
            {
                Message = msg,
                Channel = channel,
                User = usr,
                Emoji = emoji
            };
            await this._message_reaction_add.InvokeAsync(ea);
        }

        internal async Task OnMessageReactionRemoveAsync(ulong user_id, ulong message_id, DiscordChannel channel, DiscordEmoji emoji)
        {
            emoji.Discord = this;

            var usr = null as DiscordUser;
            if (channel.Guild != null)
                usr = channel.Guild._members.FirstOrDefault(xm => xm.Id == user_id) ?? await this._rest_client.InternalGetGuildMemberAsync(channel.Guild.Id, user_id);
            else
                usr = this.InternalGetCachedUser(user_id) ?? await this._rest_client.InternalGetUserAsync(user_id);

            DiscordMessage msg = null;
            if (this._config.MessageCacheSize == 0 || !channel.MessageCache.TryGet(xm => xm.Id == message_id, out msg))
                msg = new DiscordMessage { Id = message_id, Discord = this };

            var ea = new MessageReactionRemoveEventArgs(this)
            {
                Message = msg,
                Channel = channel,
                User = usr,
                Emoji = emoji
            };
            await this._message_reaction_remove.InvokeAsync(ea);
        }

        internal async Task OnMessageReactionRemoveAllAsync(ulong message_id, DiscordChannel channel)
        {
            DiscordMessage msg = null;
            if (this._config.MessageCacheSize == 0 || !channel.MessageCache.TryGet(xm => xm.Id == message_id, out msg))
                msg = new DiscordMessage { Id = message_id, Discord = this };

            var ea = new MessageReactionRemoveAllEventArgs(this)
            {
                Message = msg,
                Channel = channel
            };
            await this._message_reaction_remove_all.InvokeAsync(ea);
        }

        internal async Task OnWebhooksUpdateAsync(DiscordChannel channel, DiscordGuild guild)
        {
            var ea = new WebhooksUpdateEventArgs(this)
            {
                Channel = channel,
                Guild = guild
            };
            await this._webhooks_update.InvokeAsync(ea);
        }
        #endregion

        internal async Task OnHeartbeatAsync(long seq)
        {
            _debug_logger.LogMessage(LogLevel.Debug, "Websocket", "Received Heartbeat - Sending Ack.", DateTime.Now);
            await SendHeartbeatAsync(seq);
        }

        internal async Task OnReconnectAsync()
        {
            _debug_logger.LogMessage(LogLevel.Info, "Websocket", "Received OP 7 - Reconnect. ", DateTime.Now);

            await ReconnectAsync();
        }

        internal async Task OnInvalidateSessionAsync(bool data)
        {
            if (data)
            {
                _debug_logger.LogMessage(LogLevel.Debug, "Websocket", "Received true in OP 9 - Waiting a few second and sending resume again.", DateTime.Now);
                await Task.Delay(6000);
                await SendResumeAsync();
            }
            else
            {
                _debug_logger.LogMessage(LogLevel.Debug, "Websocket", "Received false in OP 9 - Starting a new session", DateTime.Now);
                _session_id = "";
                await SendIdentifyAsync();
            }
        }

        internal async Task OnHelloAsync(GatewayHello hello)
        {
            this._debug_logger.LogMessage(LogLevel.Debug, "Websocket", "Received OP 10 (HELLO) - Trying to either resume or identify", DateTime.Now);
            //this._waiting_for_ack = false;
            Interlocked.CompareExchange(ref this._skipped_heartbeats, 0, 0);
            this._heartbeat_interval = hello.HeartbeatInterval;
            this._heartbeat_task = new Task(StartHeartbeating, _cancel_token, TaskCreationOptions.LongRunning);
            this._heartbeat_task.Start();

            if (_session_id == "")
                await SendIdentifyAsync();
            else
                await SendResumeAsync();

#pragma warning disable CS4014
            Task.Delay(5100).ContinueWith(t =>
            {
                ConnectionSemaphore.Release();
            }).ConfigureAwait(false);
#pragma warning restore CS4014
        }

        internal async Task OnHeartbeatAckAsync()
        {
            //_waiting_for_ack = false;
            Interlocked.Decrement(ref this._skipped_heartbeats);

            var ping = Volatile.Read(ref this._ping);
            ping = (int)(DateTime.Now - this._last_heartbeat).TotalMilliseconds;

            _debug_logger.LogMessage(LogLevel.Debug, "Websocket", "Received WebSocket Heartbeat Ack", DateTime.Now);
            _debug_logger.LogMessage(LogLevel.Debug, "Websocket", $"Ping {ping}ms", DateTime.Now);

            Volatile.Write(ref this._ping, ping);

            HeartBeatEventArgs args = new HeartBeatEventArgs(this)
            {
                Ping = this.Ping,
                Timestamp = DateTimeOffset.Now
            };

            await _heartbeated.InvokeAsync(args);
        }

        //internal async Task StartHeartbeatingAsync()
        internal void StartHeartbeating()
        {
            _debug_logger.LogMessage(LogLevel.Debug, "Websocket", "Starting Heartbeat", DateTime.Now);
            var token = this._cancel_token;
            try
            {
                while (true)
                {
                    SendHeartbeatAsync().GetAwaiter().GetResult();
                    Task.Delay(_heartbeat_interval, _cancel_token).GetAwaiter().GetResult();
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException) { }
        }

        internal Task InternalUpdateStatusAsync(Game game, UserStatus? user_status, DateTimeOffset? idle_since)
        {
            if (game != null && game.Name != null && game.Name.Length > 128)
                throw new Exception("Game name can't be longer than 128 characters!");

            var since_unix = idle_since != null ? (long?)Utils.GetUnixTime(idle_since.Value) : null;

            var status = new StatusUpdate
            {
                Game = game ?? new Game(),
                IdleSince = since_unix,
                IsAFK = idle_since != null,
                Status = user_status ?? UserStatus.Online
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
            var _last_heartbeat = DateTimeOffset.Now;
            var _sequence = (long)(_last_heartbeat - DiscordEpoch).TotalMilliseconds;

            return this.SendHeartbeatAsync(_sequence);
        }

        internal async Task SendHeartbeatAsync(long seq)
        {
            //if (_waiting_for_ack)
            //{
            //    _debugLogger.LogMessage(LogLevel.Critical, "Websocket", "Missed a heartbeat ack. Reconnecting.", DateTime.Now);
            //    await ReconnectAsync();
            //}
            var more_than_5 = Volatile.Read(ref this._skipped_heartbeats) > 5;
            var guilds_comp = Volatile.Read(ref this._guild_download_completed);
            if (guilds_comp && more_than_5)
            {
                this._debug_logger.LogMessage(LogLevel.Critical, "DSharpPlus", "More than 5 heartbeats were skipped. Issuing reconnect.", DateTime.Now);
                await ReconnectAsync();
                return;
            }
            else if (!guilds_comp && more_than_5)
            {
                this._debug_logger.LogMessage(LogLevel.Warning, "DSharpPlus", "More than 5 heartbeats were skipped while the guild download is running.", DateTime.Now);
            }

            Volatile.Write(ref this._last_sequence, seq);
            var _last_heartbeat = DateTimeOffset.Now;
            _debug_logger.LogMessage(LogLevel.Debug, "Websocket", "Sending Heartbeat", DateTime.Now);
            var heartbeat = new GatewayPayload
            {
                OpCode = GatewayOpCode.Heartbeat,
                Data = seq
            };
            var heartbeat_str = JsonConvert.SerializeObject(heartbeat);
            _websocket_client.SendMessage(heartbeat_str);

            this._last_heartbeat = DateTimeOffset.Now;

            //_waiting_for_ack = true;
            Interlocked.Increment(ref this._skipped_heartbeats);
        }

        internal Task SendIdentifyAsync()
        {
            var identify = new GatewayIdentify
            {
                Token = Utils.GetFormattedToken(this),
                Compress = this._config.EnableCompression,
                LargeThreshold = this._config.LargeThreshold,
                ShardInfo = new ShardInfo
                {
                    ShardId = this._config.ShardId,
                    ShardCount = this._config.ShardCount
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
                SequenceNumber = Volatile.Read(ref this._last_sequence)
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

        internal Task SendGuildSyncAsync()
        {
            return this.SyncGuildsAsync(this._guilds.Values.ToArray());
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

        internal void UpdateCachedGuild(DiscordGuild new_guild, JArray raw_members)
        {
            if (!this._guilds.ContainsKey(new_guild.Id))
                this._guilds[new_guild.Id] = new_guild;

            var guild = this._guilds[new_guild.Id];

            if (new_guild._channels != null && new_guild._channels.Any())
            {
                var _c = new_guild._channels.Where(xc => !guild._channels.Any(xxc => xxc.Id == xc.Id));
                guild._channels.AddRange(_c);
            }

            var _e = new_guild._emojis.Where(xe => !guild._emojis.Any(xxe => xxe.Id == xe.Id));
            guild._emojis.AddRange(_e);

            if (raw_members != null)
            {
                var _m = raw_members == null ? new List<DiscordMember>() : raw_members.ToObject<IEnumerable<TransportMember>>()
                    .Select(xtm => new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id })
                    .Where(xm => !guild._members.Any(xxm => xxm.Id == xm.Id));
                guild._members.AddRange(_m);
            }

            var _r = new_guild._roles.Where(xr => !guild._roles.Any(xxr => xxr.Id == xr.Id));
            guild._roles.AddRange(_r);

            guild.Name = new_guild.Name;
            guild.AfkChannelId = new_guild.AfkChannelId;
            guild.AfkTimeout = new_guild.AfkTimeout;
            guild.DefaultMessageNotifications = new_guild.DefaultMessageNotifications;
            guild.EmbedChannelId = new_guild.EmbedChannelId;
            guild.EmbedEnabled = new_guild.EmbedEnabled;
            guild.Features = new_guild.Features;
            guild.IconHash = new_guild.IconHash;
            guild.MfaLevel = new_guild.MfaLevel;
            guild.OwnerId = new_guild.OwnerId;
            guild.RegionId = new_guild.RegionId;
            guild.SplashHash = new_guild.SplashHash;
            guild.VerificationLevel = new_guild.VerificationLevel;

            // fields not sent for update:
            // - guild.Channels
            // - voice states
            // - guild.JoinedAt = new_guild.JoinedAt;
            // - guild.Large = new_guild.Large;
            // - guild.MemberCount = Math.Max(new_guild.MemberCount, guild._members.Count);
            // - guild.Unavailable = new_guild.Unavailable;
        }

        ~DiscordClient()
        {
            Dispose();
        }

        private bool disposed;
        /// <summary>
        /// Disposes your DiscordClient.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;

            GC.SuppressFinalize(this);

            DisconnectAsync().GetAwaiter().GetResult();

            _cancel_token_source.Cancel();
            _guilds = null;
            _heartbeat_task = null;
            _current_user = null;
            _modules = null;
            _private_channels = null;
            _websocket_client.InternalDisconnectAsync(null).GetAwaiter().GetResult();

            disposed = true;
        }
    }
}
