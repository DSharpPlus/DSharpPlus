using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Enums;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Udp;
using DSharpPlus.Net.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    /// <summary>
    /// A Discord api wrapper
    /// </summary>
    public class DiscordClient : BaseDiscordClient
    {
        #region Events
        /// <summary>
        /// Fired whenever an error occurs within an event handler.
        /// </summary>
        public event AsyncEventHandler<ClientErrorEventArgs> ClientErrored
        {
            add => _clientError.Register(value);
            remove => _clientError.Unregister(value);
        }
        private AsyncEvent<ClientErrorEventArgs> _clientError;

        /// <summary>
        /// Fired whenever a WebSocket error occurs within the client.
        /// </summary>
        public event AsyncEventHandler<SocketErrorEventArgs> SocketErrored
        {
            add => _socketError.Register(value);
            remove => _socketError.Unregister(value);
        }
        private AsyncEvent<SocketErrorEventArgs> _socketError;

        /// <summary>
        /// Fired whenever WebSocket connection is established.
        /// </summary>
        public event AsyncEventHandler SocketOpened
        {
            add => _socketOpened.Register(value);
            remove => _socketOpened.Unregister(value);
        }
        private AsyncEvent _socketOpened;

        /// <summary>
        /// Fired whenever WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<SocketCloseEventArgs> SocketClosed
        {
            add => _socketClosed.Register(value);
            remove => _socketClosed.Unregister(value);
        }
        private AsyncEvent<SocketCloseEventArgs> _socketClosed;

        /// <summary>
        /// Fired when the client enters ready state.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Ready
        {
            add => _ready.Register(value);
            remove => _ready.Unregister(value);
        }
        private AsyncEvent<ReadyEventArgs> _ready;

        /// <summary>
        /// Fired whenever a session is resumed.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Resumed
        {
            add => _resumed.Register(value);
            remove => _resumed.Unregister(value);
        }
        private AsyncEvent<ReadyEventArgs> _resumed;

        /// <summary>
        /// Fired when a new channel is created.
        /// </summary>
        public event AsyncEventHandler<ChannelCreateEventArgs> ChannelCreated
        {
            add => _channelCreated.Register(value);
            remove => _channelCreated.Unregister(value);
        }
        private AsyncEvent<ChannelCreateEventArgs> _channelCreated;

        /// <summary>
        /// Fired when a new direct message channel is created.
        /// </summary>
        public event AsyncEventHandler<DmChannelCreateEventArgs> DmChannelCreated
        {
            add => _dmChannelCreated.Register(value);
            remove => _dmChannelCreated.Unregister(value);
        }
        private AsyncEvent<DmChannelCreateEventArgs> _dmChannelCreated;

        /// <summary>
        /// Fired when a channel is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelUpdateEventArgs> ChannelUpdated
        {
            add => _channelUpdated.Register(value);
            remove => _channelUpdated.Unregister(value);
        }
        private AsyncEvent<ChannelUpdateEventArgs> _channelUpdated;

        /// <summary>
        /// Fired when a channel is deleted
        /// </summary>
        public event AsyncEventHandler<ChannelDeleteEventArgs> ChannelDeleted
        {
            add => _channelDeleted.Register(value);
            remove => _channelDeleted.Unregister(value);
        }
        private AsyncEvent<ChannelDeleteEventArgs> _channelDeleted;

        /// <summary>
        /// Fired when a dm channel is deleted
        /// </summary>
        public event AsyncEventHandler<DmChannelDeleteEventArgs> DmChannelDeleted
        {
            add => _dmChannelDeleted.Register(value);
            remove => _dmChannelDeleted.Unregister(value);
        }
        private AsyncEvent<DmChannelDeleteEventArgs> _dmChannelDeleted;

        /// <summary>
        /// Fired whenever a channel's pinned message list is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelPinsUpdateEventArgs> ChannelPinsUpdated
        {
            add => _channelPinsUpdated.Register(value);
            remove => _channelPinsUpdated.Unregister(value);
        }
        private AsyncEvent<ChannelPinsUpdateEventArgs> _channelPinsUpdated;

        /// <summary>
        /// Fired when the user joins a new guild.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildCreated
        {
            add => _guildCreated.Register(value);
            remove => _guildCreated.Unregister(value);
        }
        private AsyncEvent<GuildCreateEventArgs> _guildCreated;

        /// <summary>
        /// Fired when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildAvailable
        {
            add => _guildAvailable.Register(value);
            remove => _guildAvailable.Unregister(value);
        }
        private AsyncEvent<GuildCreateEventArgs> _guildAvailable;

        /// <summary>
        /// Fired when a guild is updated.
        /// </summary>
        public event AsyncEventHandler<GuildUpdateEventArgs> GuildUpdated
        {
            add => _guildUpdated.Register(value);
            remove => _guildUpdated.Unregister(value);
        }
        private AsyncEvent<GuildUpdateEventArgs> _guildUpdated;

        /// <summary>
        /// Fired when the user leaves or is removed from a guild.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildDeleted
        {
            add => _guildDeleted.Register(value);
            remove => _guildDeleted.Unregister(value);
        }
        private AsyncEvent<GuildDeleteEventArgs> _guildDeleted;

        /// <summary>
        /// Fired when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildUnavailable
        {
            add => _guildUnavailable.Register(value);
            remove => _guildUnavailable.Unregister(value);
        }
        private AsyncEvent<GuildDeleteEventArgs> _guildUnavailable;

        /// <summary>
        /// Fired when a message is created.
        /// </summary>
        public event AsyncEventHandler<MessageCreateEventArgs> MessageCreated
        {
            add => _messageCreated.Register(value);
            remove => _messageCreated.Unregister(value);
        }
        private AsyncEvent<MessageCreateEventArgs> _messageCreated;

        /// <summary>
        /// Fired when a presence has been updated.
        /// </summary>
        public event AsyncEventHandler<PresenceUpdateEventArgs> PresenceUpdated
        {
            add => _presenceUpdate.Register(value);
            remove => _presenceUpdate.Unregister(value);
        }
        private AsyncEvent<PresenceUpdateEventArgs> _presenceUpdate;

        /// <summary>
        /// Fired when a guild ban gets added
        /// </summary>
        public event AsyncEventHandler<GuildBanAddEventArgs> GuildBanAdded
        {
            add => _guildBanAdd.Register(value);
            remove => _guildBanAdd.Unregister(value);
        }
        private AsyncEvent<GuildBanAddEventArgs> _guildBanAdd;

        /// <summary>
        /// Fired when a guild ban gets removed
        /// </summary>
        public event AsyncEventHandler<GuildBanRemoveEventArgs> GuildBanRemoved
        {
            add => _guildBanRemove.Register(value);
            remove => _guildBanRemove.Unregister(value);
        }
        private AsyncEvent<GuildBanRemoveEventArgs> _guildBanRemove;

        /// <summary>
        /// Fired when a guilds emojis get updated
        /// </summary>
        public event AsyncEventHandler<GuildEmojisUpdateEventArgs> GuildEmojisUpdated
        {
            add => _guildEmojisUpdate.Register(value);
            remove => _guildEmojisUpdate.Unregister(value);
        }
        private AsyncEvent<GuildEmojisUpdateEventArgs> _guildEmojisUpdate;

        /// <summary>
        /// Fired when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
        {
            add => _guildIntegrationsUpdate.Register(value);
            remove => _guildIntegrationsUpdate.Unregister(value);
        }
        private AsyncEvent<GuildIntegrationsUpdateEventArgs> _guildIntegrationsUpdate;

        /// <summary>
        /// Fired when a new user joins a guild.
        /// </summary>
        public event AsyncEventHandler<GuildMemberAddEventArgs> GuildMemberAdded
        {
            add => _guildMemberAdd.Register(value);
            remove => _guildMemberAdd.Unregister(value);
        }
        private AsyncEvent<GuildMemberAddEventArgs> _guildMemberAdd;

        /// <summary>
        /// Fired when a user is removed from a guild (leave/kick/ban).
        /// </summary>
        public event AsyncEventHandler<GuildMemberRemoveEventArgs> GuildMemberRemoved
        {
            add => _guildMemberRemove.Register(value);
            remove => _guildMemberRemove.Unregister(value);
        }
        private AsyncEvent<GuildMemberRemoveEventArgs> _guildMemberRemove;

        /// <summary>
        /// Fired when a guild member is updated.
        /// </summary>
        public event AsyncEventHandler<GuildMemberUpdateEventArgs> GuildMemberUpdated
        {
            add => _guildMemberUpdate.Register(value);
            remove => _guildMemberUpdate.Unregister(value);
        }
        private AsyncEvent<GuildMemberUpdateEventArgs> _guildMemberUpdate;

        /// <summary>
        /// Fired when a guild role is created.
        /// </summary>
        public event AsyncEventHandler<GuildRoleCreateEventArgs> GuildRoleCreated
        {
            add => _guildRoleCreate.Register(value);
            remove => _guildRoleCreate.Unregister(value);
        }
        private AsyncEvent<GuildRoleCreateEventArgs> _guildRoleCreate;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleUpdateEventArgs> GuildRoleUpdated
        {
            add => _guildRoleUpdate.Register(value);
            remove => _guildRoleUpdate.Unregister(value);
        }
        private AsyncEvent<GuildRoleUpdateEventArgs> _guildRoleUpdate;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleDeleteEventArgs> GuildRoleDeleted
        {
            add => _guildRoleDelete.Register(value);
            remove => _guildRoleDelete.Unregister(value);
        }
        private AsyncEvent<GuildRoleDeleteEventArgs> _guildRoleDelete;

        /// <summary>
        /// Fired when message is acknowledged by the user.
        /// </summary>
        public event AsyncEventHandler<MessageAcknowledgeEventArgs> MessageAcknowledged
        {
            add => _messageAck.Register(value);
            remove => _messageAck.Unregister(value);
        }
        private AsyncEvent<MessageAcknowledgeEventArgs> _messageAck;

        /// <summary>
        /// Fired when a message is updated.
        /// </summary>
        public event AsyncEventHandler<MessageUpdateEventArgs> MessageUpdated
        {
            add => _messageUpdate.Register(value);
            remove => _messageUpdate.Unregister(value);
        }
        private AsyncEvent<MessageUpdateEventArgs> _messageUpdate;

        /// <summary>
        /// Fired when a message is deleted.
        /// </summary>
        public event AsyncEventHandler<MessageDeleteEventArgs> MessageDeleted
        {
            add => _messageDelete.Register(value);
            remove => _messageDelete.Unregister(value);
        }
        private AsyncEvent<MessageDeleteEventArgs> _messageDelete;

        /// <summary>
        /// Fired when multiple messages are deleted at once.
        /// </summary>
        public event AsyncEventHandler<MessageBulkDeleteEventArgs> MessagesBulkDeleted
        {
            add => _messageBulkDelete.Register(value);
            remove => _messageBulkDelete.Unregister(value);
        }
        private AsyncEvent<MessageBulkDeleteEventArgs> _messageBulkDelete;

        /// <summary>
        /// Fired when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<TypingStartEventArgs> TypingStarted
        {
            add => _typingStart.Register(value);
            remove => _typingStart.Unregister(value);
        }
        private AsyncEvent<TypingStartEventArgs> _typingStart;

        /// <summary>
        /// Fired when the current user updates their settings.
        /// </summary>
        public event AsyncEventHandler<UserSettingsUpdateEventArgs> UserSettingsUpdated
        {
            add => _userSettingsUpdate.Register(value);
            remove => _userSettingsUpdate.Unregister(value);
        }
        private AsyncEvent<UserSettingsUpdateEventArgs> _userSettingsUpdate;

        /// <summary>
        /// Fired when properties about the user change.
        /// </summary>
        public event AsyncEventHandler<UserUpdateEventArgs> UserUpdated
        {
            add => _userUpdate.Register(value);
            remove => _userUpdate.Unregister(value);
        }
        private AsyncEvent<UserUpdateEventArgs> _userUpdate;

        /// <summary>
        /// Fired when someone joins/leaves/moves voice channels.
        /// </summary>
        public event AsyncEventHandler<VoiceStateUpdateEventArgs> VoiceStateUpdated
        {
            add => _voiceStateUpdate.Register(value);
            remove => _voiceStateUpdate.Unregister(value);
        }
        private AsyncEvent<VoiceStateUpdateEventArgs> _voiceStateUpdate;

        /// <summary>
        /// Fired when a guild's voice server is updated.
        /// </summary>
        public event AsyncEventHandler<VoiceServerUpdateEventArgs> VoiceServerUpdated
        {
            add => _voiceServerUpdate.Register(value);
            remove => _voiceServerUpdate.Unregister(value);
        }
        private AsyncEvent<VoiceServerUpdateEventArgs> _voiceServerUpdate;

        /// <summary>
        /// Fired in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<GuildMembersChunkEventArgs> GuildMembersChunked
        {
            add => _guildMembersChunk.Register(value);
            remove => _guildMembersChunk.Unregister(value);
        }
        private AsyncEvent<GuildMembersChunkEventArgs> _guildMembersChunk;

        /// <summary>
        /// Fired when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<UnknownEventArgs> UnknownEvent
        {
            add => _unknownEvent.Register(value);
            remove => _unknownEvent.Unregister(value);
        }
        private AsyncEvent<UnknownEventArgs> _unknownEvent;

        /// <summary>
        /// Fired when a reaction gets added to a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionAddEventArgs> MessageReactionAdded
        {
            add => _messageReactionAdd.Register(value);
            remove => _messageReactionAdd.Unregister(value);
        }
        private AsyncEvent<MessageReactionAddEventArgs> _messageReactionAdd;

        /// <summary>
        /// Fired when a reaction gets removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEventArgs> MessageReactionRemoved
        {
            add => _messageReactionRemove.Register(value);
            remove => _messageReactionRemove.Unregister(value);
        }
        private AsyncEvent<MessageReactionRemoveEventArgs> _messageReactionRemove;

        /// <summary>
        /// Fired when all reactions get removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionsClearEventArgs> MessageReactionsCleared
        {
            add => _messageReactionRemoveAll.Register(value);
            remove => _messageReactionRemoveAll.Unregister(value);
        }
        private AsyncEvent<MessageReactionsClearEventArgs> _messageReactionRemoveAll;

        /// <summary>
        /// Fired whenever webhooks update.
        /// </summary>
        public event AsyncEventHandler<WebhooksUpdateEventArgs> WebhooksUpdated
        {
            add => _webhooksUpdate.Register(value);
            remove => _webhooksUpdate.Unregister(value);
        }
        private AsyncEvent<WebhooksUpdateEventArgs> _webhooksUpdate;

        /// <summary>
        /// Fired on received heartbeat ACK.
        /// </summary>
        public event AsyncEventHandler<HeartbeatEventArgs> Heartbeated
        {
            add => _heartbeated.Register(value);
            remove => _heartbeated.Unregister(value);
        }
        private AsyncEvent<HeartbeatEventArgs> _heartbeated;

        internal void EventErrorHandler(string evname, Exception ex)
        {
            DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"An {ex.GetType()} occured in {evname}.", DateTime.Now);
            _clientError.InvokeAsync(new ClientErrorEventArgs(this) { EventName = evname, Exception = ex }).GetAwaiter().GetResult();
        }

        private void Goof(string evname, Exception ex)
        {
            DebugLogger.LogMessage(LogLevel.Critical, "DSharpPlus", $"An {ex.GetType()} occured in the exception handler.", DateTime.Now);
        }
        #endregion

        #region Internal Variables
        // ReSharper disable once InconsistentNaming
        internal static UTF8Encoding UTF8 = new UTF8Encoding(false);
        internal static DateTimeOffset DiscordEpoch = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

        internal CancellationTokenSource CancelTokenSource;
        internal CancellationToken CancelToken;

        internal List<BaseExtension> Modules = new List<BaseExtension>();

        internal BaseWebSocketClient WebsocketClient;
        internal string SessionToken = "";
        internal string SessionId = "";
        internal int HeartbeatInterval;
        internal Task HeartbeatTask;
        internal DateTimeOffset LastHeartbeat;
        internal long LastSequence;
        internal int SkippedHeartbeats;
        internal bool GuildDownloadCompleted;

        internal RingBuffer<DiscordMessage> MessageCache { get; }
        #endregion

        #region Public Variables
        // ReSharper disable once InconsistentNaming
        internal int _gateway_version;
        /// <summary>
        /// Gets the gateway protocol version.
        /// </summary>
        public int GatewayVersion => _gateway_version;

        // ReSharper disable once InconsistentNaming
        internal string _gateway_url = "";
        /// <summary>
        /// Gets the gateway URL.
        /// </summary>
        public string GatewayUrl => _gateway_url;

        // ReSharper disable once InconsistentNaming
        internal int _shard_count = 1;
        /// <summary>
        /// Gets the total number of shards the bot is connected to.
        /// </summary>
        public int ShardCount => Configuration.ShardCount;

        /// <summary>
        /// Gets the currently connected shard ID.
        /// </summary>
        public int ShardId => Configuration.ShardId;

        /// <summary>
        /// List of DM Channels
        /// </summary>
        public IReadOnlyList<DiscordDmChannel> PrivateChannels => _privateChannelsLazy.Value;
        // ReSharper disable once InconsistentNaming
        internal List<DiscordDmChannel> _private_channels = new List<DiscordDmChannel>();
        private Lazy<IReadOnlyList<DiscordDmChannel>> _privateChannelsLazy;

        /// <summary>
        /// List of Guilds
        /// </summary>
        public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds => _guildsLazy.Value;
        // ReSharper disable once InconsistentNaming
        internal Dictionary<ulong, DiscordGuild> _guilds = new Dictionary<ulong, DiscordGuild>();
        private Lazy<IReadOnlyDictionary<ulong, DiscordGuild>> _guildsLazy;

        /// <summary>
        /// Gets the WS latency for this client.
        /// </summary>
        public int Ping => Volatile.Read(ref _ping);
        private int _ping;

        /// <summary>
        /// Gets the collection of presences held by this client.
        /// </summary>
        public IReadOnlyDictionary<ulong, DiscordPresence> Presences => _presencesLazy.Value;
        // ReSharper disable once InconsistentNaming
        internal Dictionary<ulong, DiscordPresence> _presences = new Dictionary<ulong, DiscordPresence>();
        private Lazy<IReadOnlyDictionary<ulong, DiscordPresence>> _presencesLazy;
        #endregion

        #region Connection semaphore
        private static SemaphoreSlim ConnectionSemaphore => _semaphoreInit.Value;
        private static Lazy<SemaphoreSlim> _semaphoreInit = new Lazy<SemaphoreSlim>(() => new SemaphoreSlim(1, 1));
        #endregion

        /// <summary>
        /// Initializes a new instance of DiscordClient.
        /// </summary>
        /// <param name="config">Specifies configuration parameters.</param>
        public DiscordClient(DiscordConfiguration config)
            : base(config)
        {
            if (config.MessageCacheSize > 0)
            {
                MessageCache = new RingBuffer<DiscordMessage>(config.MessageCacheSize);
            }

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
            _clientError = new AsyncEvent<ClientErrorEventArgs>(Goof, "CLIENT_ERRORED");
            _socketError = new AsyncEvent<SocketErrorEventArgs>(Goof, "SOCKET_ERRORED");
            _socketOpened = new AsyncEvent(EventErrorHandler, "SOCKET_OPENED");
            _socketClosed = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "SOCKET_CLOSED");
            _ready = new AsyncEvent<ReadyEventArgs>(EventErrorHandler, "READY");
            _resumed = new AsyncEvent<ReadyEventArgs>(EventErrorHandler, "RESUMED");
            _channelCreated = new AsyncEvent<ChannelCreateEventArgs>(EventErrorHandler, "CHANNEL_CREATED");
            _dmChannelCreated = new AsyncEvent<DmChannelCreateEventArgs>(EventErrorHandler, "DM_CHANNEL_CREATED");
            _channelUpdated = new AsyncEvent<ChannelUpdateEventArgs>(EventErrorHandler, "CHANNEL_UPDATED");
            _channelDeleted = new AsyncEvent<ChannelDeleteEventArgs>(EventErrorHandler, "CHANNEL_DELETED");
            _dmChannelDeleted = new AsyncEvent<DmChannelDeleteEventArgs>(EventErrorHandler, "DM_CHANNEL_DELETED");
            _channelPinsUpdated = new AsyncEvent<ChannelPinsUpdateEventArgs>(EventErrorHandler, "CHANNEL_PINS_UPDATEED");
            _guildCreated = new AsyncEvent<GuildCreateEventArgs>(EventErrorHandler, "GUILD_CREATED");
            _guildAvailable = new AsyncEvent<GuildCreateEventArgs>(EventErrorHandler, "GUILD_AVAILABLE");
            _guildUpdated = new AsyncEvent<GuildUpdateEventArgs>(EventErrorHandler, "GUILD_UPDATED");
            _guildDeleted = new AsyncEvent<GuildDeleteEventArgs>(EventErrorHandler, "GUILD_DELETED");
            _guildUnavailable = new AsyncEvent<GuildDeleteEventArgs>(EventErrorHandler, "GUILD_UNAVAILABLE");
            _messageCreated = new AsyncEvent<MessageCreateEventArgs>(EventErrorHandler, "MESSAGE_CREATED");
            _presenceUpdate = new AsyncEvent<PresenceUpdateEventArgs>(EventErrorHandler, "PRESENCE_UPDATEED");
            _guildBanAdd = new AsyncEvent<GuildBanAddEventArgs>(EventErrorHandler, "GUILD_BAN_ADD");
            _guildBanRemove = new AsyncEvent<GuildBanRemoveEventArgs>(EventErrorHandler, "GUILD_BAN_REMOVED");
            _guildEmojisUpdate = new AsyncEvent<GuildEmojisUpdateEventArgs>(EventErrorHandler, "GUILD_EMOJI_UPDATED");
            _guildIntegrationsUpdate = new AsyncEvent<GuildIntegrationsUpdateEventArgs>(EventErrorHandler, "GUILD_INTEGRATIONS_UPDATED");
            _guildMemberAdd = new AsyncEvent<GuildMemberAddEventArgs>(EventErrorHandler, "GUILD_MEMBER_ADD");
            _guildMemberRemove = new AsyncEvent<GuildMemberRemoveEventArgs>(EventErrorHandler, "GUILD_MEMBER_REMOVED");
            _guildMemberUpdate = new AsyncEvent<GuildMemberUpdateEventArgs>(EventErrorHandler, "GUILD_MEMBER_UPDATED");
            _guildRoleCreate = new AsyncEvent<GuildRoleCreateEventArgs>(EventErrorHandler, "GUILD_ROLE_CREATED");
            _guildRoleUpdate = new AsyncEvent<GuildRoleUpdateEventArgs>(EventErrorHandler, "GUILD_ROLE_UPDATED");
            _guildRoleDelete = new AsyncEvent<GuildRoleDeleteEventArgs>(EventErrorHandler, "GUILD_ROLE_DELETED");
            _messageAck = new AsyncEvent<MessageAcknowledgeEventArgs>(EventErrorHandler, "MESSAGE_ACKNOWLEDGED");
            _messageUpdate = new AsyncEvent<MessageUpdateEventArgs>(EventErrorHandler, "MESSAGE_UPDATED");
            _messageDelete = new AsyncEvent<MessageDeleteEventArgs>(EventErrorHandler, "MESSAGE_DELETED");
            _messageBulkDelete = new AsyncEvent<MessageBulkDeleteEventArgs>(EventErrorHandler, "MESSAGE_BULK_DELETED");
            _typingStart = new AsyncEvent<TypingStartEventArgs>(EventErrorHandler, "TYPING_STARTED");
            _userSettingsUpdate = new AsyncEvent<UserSettingsUpdateEventArgs>(EventErrorHandler, "USER_SETTINGS_UPDATED");
            _userUpdate = new AsyncEvent<UserUpdateEventArgs>(EventErrorHandler, "USER_UPDATED");
            _voiceStateUpdate = new AsyncEvent<VoiceStateUpdateEventArgs>(EventErrorHandler, "VOICE_STATE_UPDATED");
            _voiceServerUpdate = new AsyncEvent<VoiceServerUpdateEventArgs>(EventErrorHandler, "VOICE_SERVER_UPDATED");
            _guildMembersChunk = new AsyncEvent<GuildMembersChunkEventArgs>(EventErrorHandler, "GUILD_MEMBERS_CHUNKED");
            _unknownEvent = new AsyncEvent<UnknownEventArgs>(EventErrorHandler, "UNKNOWN_EVENT");
            _messageReactionAdd = new AsyncEvent<MessageReactionAddEventArgs>(EventErrorHandler, "MESSAGE_REACTION_ADDED");
            _messageReactionRemove = new AsyncEvent<MessageReactionRemoveEventArgs>(EventErrorHandler, "MESSAGE_REACTION_REMOVED");
            _messageReactionRemoveAll = new AsyncEvent<MessageReactionsClearEventArgs>(EventErrorHandler, "MESSAGE_REACTIONS_CLEARED");
            _webhooksUpdate = new AsyncEvent<WebhooksUpdateEventArgs>(EventErrorHandler, "WEBHOOKS_UPDATED");
            _heartbeated = new AsyncEvent<HeartbeatEventArgs>(EventErrorHandler, "HEARTBEATED");

            _private_channels = new List<DiscordDmChannel>();
            _guilds = new Dictionary<ulong, DiscordGuild>();

            _privateChannelsLazy = new Lazy<IReadOnlyList<DiscordDmChannel>>(() => new ReadOnlyCollection<DiscordDmChannel>(_private_channels));
            _guildsLazy = new Lazy<IReadOnlyDictionary<ulong, DiscordGuild>>(() => new ReadOnlyDictionary<ulong, DiscordGuild>(_guilds));
            _presencesLazy = new Lazy<IReadOnlyDictionary<ulong, DiscordPresence>>(() => new ReadOnlyDictionary<ulong, DiscordPresence>(_presences));

            if (Configuration.UseInternalLogHandler)
            {
                DebugLogger.LogMessageReceived += (sender, e) => DebugLogger.LogHandler(sender, e);
            }
        }

        /// <summary>
        /// Adds a new module to the module list
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public BaseExtension AddModule(BaseExtension module)
        {
            module.Setup(this);
            Modules.Add(module);
            return module;
        }

        /// <summary>
        /// Gets a module from the module list by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetModule<T>() where T : BaseExtension
        {
            return Modules.Find(x => x.GetType() == typeof(T)) as T;
        }

        /// <summary>
        /// Connects to the gateway
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            var w = 7500;
            var i = 5;
            var s = false;
            Exception cex = null;

            if (Configuration.TokenType != TokenType.Bot)
            {
                DebugLogger.LogMessage(LogLevel.Warning, "DSharpPlus", "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.", DateTime.Now);
            }
            DebugLogger.LogMessage(LogLevel.Info, "DSharpPlus", $"DSharpPlus, version {VersionString}", DateTime.Now);

            while (i-- > 0)
            {
                try
                {
                    await InternalConnectAsync();
                    s = true;
                    break;
                }
                catch (UnauthorizedException e)
                {
                    throw new Exception("Authentication failed. Check your token and try again.", e);
                }
                catch (PlatformNotSupportedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    cex = ex;
                    if (i <= 0)
                    {
                        break;
                    }

                    DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"Connection attempt failed, retrying in {w / 1000}s", DateTime.Now);
                    await Task.Delay(w);
                    w *= 2;
                }
            }

            if (!s && cex != null)
            {
                throw new Exception("Could not connect to Discord.", cex);
            }
        }

        public Task ReconnectAsync(bool startNewSession = false)
        {
            if (startNewSession)
            {
                SessionId = "";
            }

            return WebsocketClient.InternalDisconnectAsync(null);
        }

        internal Task InternalReconnectAsync() => ConnectAsync();

        internal async Task InternalConnectAsync()
        {
            await InternalUpdateGatewayAsync();
            await InitializeAsync();

            Volatile.Write(ref SkippedHeartbeats, 0);

            WebsocketClient = BaseWebSocketClient.Create();

            CancelTokenSource = new CancellationTokenSource();
            CancelToken = CancelTokenSource.Token;

            WebsocketClient.OnConnect += () => _socketOpened.InvokeAsync();
            WebsocketClient.OnDisconnect += async e =>
            {
                CancelTokenSource.Cancel();

                DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Connection closed", DateTime.Now);
                await _socketClosed.InvokeAsync(new SocketCloseEventArgs(this) { CloseCode = e.CloseCode, CloseMessage = e.CloseMessage });

                if (Configuration.AutoReconnect)
                {
                    DebugLogger.LogMessage(LogLevel.Critical, "Websocket", $"Socket connection terminated ({e.CloseCode.ToString(CultureInfo.InvariantCulture)}, '{e.CloseMessage}'). Reconnecting", DateTime.Now);
                    await ConnectAsync();
                }
            };
            WebsocketClient.OnMessage += e => HandleSocketMessageAsync(e.Message);
            WebsocketClient.OnError += e => _socketError.InvokeAsync(new SocketErrorEventArgs(this) { Exception = e.Exception });

            await ConnectionSemaphore.WaitAsync();
            await WebsocketClient.ConnectAsync(_gateway_url + "?v=6&encoding=json");
        }

        /// <summary>
        /// Disconnects from the gateway
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            Configuration.AutoReconnect = false;
            if (WebsocketClient != null)
            {
                await WebsocketClient.InternalDisconnectAsync(null);
            }
        }

        #region Public Functions
        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        public async Task<DiscordUser> GetUserAsync(ulong userId)
        {
            var usr = InternalGetCachedUser(userId);
            if (usr != null)
            {
                return usr;
            }

            usr = await ApiClient.GetUserAsync(userId);
            usr = UserCache.AddOrUpdate(userId, usr, (id, old) =>
            {
                // ReSharper disable AccessToModifiedClosure
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                // ReSharper restore AccessToModifiedClosure
                return old;
            });

            return usr;
        }

        /// <summary>
        /// Gets a channel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordChannel> GetChannelAsync(ulong id) =>
            InternalGetCachedChannel(id) ?? await ApiClient.GetChannelAsync(id);

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content = null, bool tts = false, DiscordEmbed embed = null) =>
            ApiClient.CreateMessageAsync(channel.Id, content, tts, embed);

        /// <summary>
        /// Creates a guild. This requires the bot to be in less than 10 guilds total.
        /// </summary>
        /// <param name="name">Name of the guild.</param>
        /// <param name="region">Voice region of the guild.</param>
        /// <param name="icon">Stream containing the icon for the guild.</param>
        /// <param name="verificationLevel">Verification level for the guild.</param>
        /// <param name="defaultMessageNotifications">Default message notification settings for the guild.</param>
        /// <returns>The created guild.</returns>
        public Task<DiscordGuild> CreateGuildAsync(string name, string region = null, Stream icon = null, VerificationLevel? verificationLevel = null, 
            DefaultMessageNotifications? defaultMessageNotifications = null)
        {
            string iconb64 = null;
            if (icon != null)
            {
                using (var imgtool = new ImageTool(icon))
                {
                    iconb64 = imgtool.GetBase64();
                }
            }

            return ApiClient.CreateGuildAsync(name, region, iconb64, verificationLevel, defaultMessageNotifications);
        }

        /// <summary>
        /// Gets a guild
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordGuild> GetGuildAsync(ulong id)
        {
            if (_guilds.ContainsKey(id))
            {
                return _guilds[id];
            }

            var gld = await ApiClient.GetGuildAsync(id);
            var chns = await ApiClient.GetGuildChannelsAsync(gld.Id);
            gld._channels.AddRange(chns);

            return gld;
        }

        /// <summary>
        /// Gets an invite
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Task<DiscordInvite> GetInviteByCodeAsync(string code) =>
            ApiClient.GetInviteAsync(code);

        /// <summary>
        /// Gets a list of connections
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordConnection>> GetConnectionsAsync() =>
            ApiClient.GetUsersConnectionsAsync();

        /// <summary>
        /// Gets a list of regions
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordVoiceRegion>> ListRegionsAsync() =>
            ApiClient.ListVoiceRegionsAsync();

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookAsync(ulong id) =>
            ApiClient.GetWebhookAsync(id);

        /// <summary>
        /// Gets a webhook
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong id, string token) =>
            ApiClient.GetWebhookWithTokenAsync(id, token);

        /// <summary>
        /// Creates a dm
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<DiscordDmChannel> CreateDmAsync(DiscordUser user) =>
            PrivateChannels.ToList().Find(x => x.Recipients.First().Id == user.Id) ?? await ApiClient.CreateDmAsync(user.Id);

        /// <summary>
        /// Updates current user's status
        /// </summary>
        /// <param name="game">Game you're playing</param>
        /// <param name="userStatus"></param>
        /// <param name="idleSince"></param>
        /// <returns></returns>
        public Task UpdateStatusAsync(DiscordGame game = null, UserStatus? userStatus = null, DateTimeOffset? idleSince = null) =>
            InternalUpdateStatusAsync(game, userStatus, idleSince);

        /// <summary>
        /// Gets information about specified API application.
        /// </summary>
        /// <param name="id">ID of the application.</param>
        /// <returns>Information about specified API application.</returns>
        public Task<DiscordApplication> GetApplicationAsync(ulong id) =>
            ApiClient.GetApplicationInfoAsync(id);

        /// <summary>
        /// Edits current user.
        /// </summary>
        /// <param name="username">New username.</param>
        /// <param name="avatar">New avatar.</param>
        /// <returns></returns>
        public async Task<DiscordUser> EditCurrentUserAsync(string username = null, Stream avatar = null)
        {
            string av64 = null;
            if (avatar != null)
            {
                using (var imgtool = new ImageTool(avatar))
                {
                    av64 = imgtool.GetBase64();
                }
            }

            var usr = await ApiClient.ModifyCurrentUserAsync(username, av64);

            CurrentUser.Username = usr.Username;
            CurrentUser.Discriminator = usr.Discriminator;
            CurrentUser.AvatarHash = usr.AvatarHash;
            return CurrentUser;
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
            if (Configuration.TokenType != TokenType.User)
            {
                throw new InvalidOperationException("This can only be done for user tokens.");
            }

            var toSync = guilds.Where(xg => !xg.IsSynced).Select(xg => xg.Id);

            if (!toSync.Any())
            {
                return Task.Delay(0);
            }

            var guildSync = new GatewayPayload
            {
                OpCode = GatewayOpCode.GuildSync,
                Data = toSync
            };
            var guildSyncstr = JsonConvert.SerializeObject(guildSync);

            WebsocketClient.SendMessage(guildSyncstr);
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
                    await OnHelloAsync((payload.Data as JObject)?.ToObject<GatewayHello>());
                    break;

                case GatewayOpCode.HeartbeatAck:
                    await OnHeartbeatAckAsync();
                    break;

                default:
                    DebugLogger.LogMessage(LogLevel.Warning, "Websocket", $"Unknown OP-Code: {((int)payload.OpCode).ToString(CultureInfo.InvariantCulture)}\n{payload.Data}", DateTime.Now);
                    break;
            }
        }

        internal async Task HandleDispatchAsync(GatewayPayload payload)
        {
            var dat = (JObject) payload.Data;

            DiscordChannel chn;
            ulong gid;
            ulong cid;
            TransportUser usr;

            switch (payload.EventName.ToLowerInvariant())
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
                    await OnChannelPinsUpdate(InternalGetCachedChannel(cid), DateTimeOffset.Parse((string)dat["last_pin_timestamp"], CultureInfo.InvariantCulture));
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
                    await OnGuildSyncEventAsync(_guilds[gid], (bool)dat["large"], (JArray)dat["members"], dat["presences"].ToObject<IEnumerable<DiscordPresence>>());
                    break;

                case "guild_ban_add":
                    usr = dat["user"].ToObject<TransportUser>();
                    gid = (ulong)dat["guild_id"];
                    await OnGuildBanAddEventAsync(usr, _guilds[gid]);
                    break;

                case "guild_ban_remove":
                    usr = dat["user"].ToObject<TransportUser>();
                    gid = (ulong)dat["guild_id"];
                    await OnGuildBanRemoveEventAsync(usr, _guilds[gid]);
                    break;

                case "guild_emojis_update":
                    gid = (ulong)dat["guild_id"];
                    var ems = dat["emojis"].ToObject<IEnumerable<DiscordEmoji>>();
                    await OnGuildEmojisUpdateEventAsync(_guilds[gid], ems);
                    break;

                case "guild_integrations_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildIntegrationsUpdateEventAsync(_guilds[gid]);
                    break;

                case "guild_member_add":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMemberAddEventAsync(dat.ToObject<TransportMember>(), _guilds[gid]);
                    break;

                case "guild_member_remove":
                    gid = (ulong)dat["guild_id"];
                    if (!_guilds.ContainsKey(gid))
                    { DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"Could not find {gid.ToString(CultureInfo.InvariantCulture)} in guild cache.", DateTime.Now); return; }
                    await OnGuildMemberRemoveEventAsync(dat["user"].ToObject<TransportUser>(), _guilds[gid]);
                    break;

                case "guild_member_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMemberUpdateEventAsync(dat["user"].ToObject<TransportUser>(), _guilds[gid], dat["roles"].ToObject<IEnumerable<ulong>>(), (string)dat["nick"]);
                    break;

                case "guild_member_chunk":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildMembersChunkEventAsync(dat["members"].ToObject<IEnumerable<TransportMember>>(), _guilds[gid]);
                    break;

                case "guild_role_create":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleCreateEventAsync(dat["role"].ToObject<DiscordRole>(), _guilds[gid]);
                    break;

                case "guild_role_update":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleUpdateEventAsync(dat["role"].ToObject<DiscordRole>(), _guilds[gid]);
                    break;

                case "guild_role_delete":
                    gid = (ulong)dat["guild_id"];
                    await OnGuildRoleDeleteEventAsync((ulong)dat["role_id"], _guilds[gid]);
                    break;

                case "message_ack":
                    cid = (ulong)dat["channel_id"];
                    var mid = (ulong)dat["message_id"];
                    await OnMessageAckEventAsync(InternalGetCachedChannel(cid), mid);
                    break;

                case "message_create":
                    await OnMessageCreateEventAsync(dat.ToObject<DiscordMessage>(), dat["author"].ToObject<TransportUser>());
                    break;

                case "message_update":
                    await OnMessageUpdateEventAsync(dat.ToObject<DiscordMessage>(), dat["author"]?.ToObject<TransportUser>());
                    break;

                // delete event does *not* include message object 
                case "message_delete":
                    await OnMessageDeleteEventAsync((ulong)dat["id"], InternalGetCachedChannel((ulong)dat["channel_id"]));
                    break;

                case "message_delete_bulk":
                    await OnMessageBulkDeleteEventAsync(dat["ids"].ToObject<IEnumerable<ulong>>(), InternalGetCachedChannel((ulong)dat["channel_id"]));
                    break;

                case "presence_update":
                    await OnPresenceUpdateEventAsync(dat.ToObject<DiscordPresence>(), (JObject)dat["user"], dat.ToObject<PresenceUpdateEventArgs>());
                    break;

                case "typing_start":
                    cid = (ulong)dat["channel_id"];
                    await OnTypingStartEventAsync((ulong)dat["user_id"], InternalGetCachedChannel(cid), Utilities.GetDateTimeOffset((long)dat["timestamp"]));
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
                    await OnVoiceServerUpdateEventAsync((string)dat["endpoint"], (string)dat["token"], _guilds[gid]);
                    break;

                case "message_reaction_add":
                    cid = (ulong)dat["channel_id"];
                    await OnMessageReactionAddAsync((ulong)dat["user_id"], (ulong)dat["message_id"], InternalGetCachedChannel(cid), dat["emoji"].ToObject<DiscordEmoji>());
                    break;

                case "message_reaction_remove":
                    cid = (ulong)dat["channel_id"];
                    await OnMessageReactionRemoveAsync((ulong)dat["user_id"], (ulong)dat["message_id"], InternalGetCachedChannel(cid), dat["emoji"].ToObject<DiscordEmoji>());
                    break;

                case "message_reaction_remove_all":
                    cid = (ulong)dat["channel_id"];
                    await OnMessageReactionRemoveAllAsync((ulong)dat["message_id"], InternalGetCachedChannel(cid));
                    break;

                case "webhooks_update":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await OnWebhooksUpdateAsync(_guilds[gid]._channels.FirstOrDefault(xc => xc.Id == cid), _guilds[gid]);
                    break;

                default:
                    await OnUnknownEventAsync(payload);
                    DebugLogger.LogMessage(LogLevel.Warning, "Websocket", $"Unknown event: {payload.EventName}\n{payload.Data}", DateTime.Now);
                    break;
            }
        }

        #region Events
        internal async Task OnReadyEventAsync(ReadyPayload ready, JArray rawGuilds, JArray rawDmChannels)
        {
            //ready.CurrentUser.Discord = this;

            var rusr = ready.CurrentUser;
            CurrentUser.Username = rusr.Username;
            CurrentUser.Discriminator = rusr.Discriminator;
            CurrentUser.AvatarHash = rusr.AvatarHash;
            CurrentUser.MfaEnabled = rusr.MfaEnabled;
            CurrentUser.Verified = rusr.Verified;
            CurrentUser.IsBot = rusr.IsBot;

            _gateway_version = ready.GatewayVersion;
            SessionId = ready.SessionId;
            var rawGuildIndex = rawGuilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);

            _private_channels = rawDmChannels
                .Select(xjt =>
                {
                    var xdc = xjt.ToObject<DiscordDmChannel>();

                    xdc.Discord = this;

                    //xdc._recipients = 
                    //    .Select(xtu => this.InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this })
                    //    .ToList();

                    var recipsRaw = xjt["recipients"].ToObject<IEnumerable<TransportUser>>();
                    xdc._recipients = new List<DiscordUser>();
                    foreach (var xr in recipsRaw)
                    {
                        var xu = new DiscordUser(xr) { Discord = this };
                        xu = UserCache.AddOrUpdate(xr.Id, xu, (id, old) =>
                        {
                            // ReSharper disable AccessToModifiedClosure
                            old.Username = xu.Username;
                            old.Discriminator = xu.Discriminator;
                            old.AvatarHash = xu.AvatarHash;
                            // ReSharper restore AccessToModifiedClosure
                            return old;
                        });

                        xdc._recipients.Add(xu);
                    }

                    return xdc;
                }).ToList();

            _guilds = ready.Guilds
                .Select(xg =>
                {
                    xg.Discord = this;

                    if (xg._channels == null)
                    {
                        xg._channels = new List<DiscordChannel>();
                    }

                    foreach (var xc in xg.Channels)
                    {
                        xc.GuildId = xg.Id;
                        xc.Discord = this;
                    }

                    if (xg._roles == null)
                    {
                        xg._roles = new List<DiscordRole>();
                    }

                    foreach (var xr in xg.Roles)
                    {
                        xr.Discord = this;
                    }

                    var rawGuild = rawGuildIndex[xg.Id];
                    var rawMembers = (JArray)rawGuild["members"];
                    xg._members = new List<DiscordMember>();

                    if (rawMembers != null)
                    {
                        foreach (var xj in rawMembers)
                        {
                            var xtm = xj.ToObject<TransportMember>();

                            var xu = new DiscordUser(xtm.User) { Discord = this };
                            UserCache.AddOrUpdate(xtm.User.Id, xu, (id, old) =>
                            {
                                // ReSharper disable AccessToModifiedClosure
                                old.Username = xu.Username;
                                old.Discriminator = xu.Discriminator;
                                old.AvatarHash = xu.AvatarHash;
                                // ReSharper restore AccessToModifiedClosure
                                return old;
                            });

                            xg._members.Add(new DiscordMember(xtm) { Discord = this, GuildId = xg.Id });
                        }
                    }

                    if (xg._emojis == null)
                    {
                        xg._emojis = new List<DiscordEmoji>();
                    }

                    foreach (var xe in xg.Emojis)
                    {
                        xe.Discord = this;
                    }

                    if (xg._voiceStates == null)
                    {
                        xg._voiceStates = new List<DiscordVoiceState>();
                    }

                    foreach (var xvs in xg.VoiceStates)
                    {
                        xvs.Discord = this;
                    }

                    return xg;
                }).ToDictionary(xg => xg.Id, xg => xg);

            _guildsLazy = new Lazy<IReadOnlyDictionary<ulong, DiscordGuild>>(() => new ReadOnlyDictionary<ulong, DiscordGuild>(_guilds));

            if (Configuration.TokenType == TokenType.User && Configuration.AutomaticGuildSync)
            {
                await SendGuildSyncAsync();
            }
            else if (Configuration.TokenType == TokenType.User)
            {
                Volatile.Write(ref GuildDownloadCompleted, true);
            }

            await _ready.InvokeAsync(new ReadyEventArgs(this));
        }

        internal Task OnResumedAsync()
        {
            DebugLogger.LogMessage(LogLevel.Info, "DSharpPlus", "Session resumed.", DateTime.Now);
            return _resumed.InvokeAsync(new ReadyEventArgs(this));
        }

        internal async Task OnChannelCreateEventAsync(DiscordChannel channel, JArray rawRecipients)
        {
            channel.Discord = this;

            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var chn = channel as DiscordDmChannel;

                var recips = rawRecipients.ToObject<IEnumerable<TransportUser>>()
                    .Select(xtu => InternalGetCachedUser(xtu.Id) ?? new DiscordUser(xtu) { Discord = this });
                if (chn != null)
                {
                    chn._recipients = recips.ToList();

                    _private_channels.Add(chn);

                    await _dmChannelCreated.InvokeAsync(new DmChannelCreateEventArgs(this) { Channel = chn });
                }
            }
            else
            {
                channel.Discord = this;

                _guilds[channel.GuildId]._channels.Add(channel);

                await _channelCreated.InvokeAsync(new ChannelCreateEventArgs(this) { Channel = channel, Guild = channel.Guild });
            }
        }

        internal async Task OnChannelUpdateEventAsync(DiscordChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            channel.Discord = this;

            var gld = channel.Guild;

            var channelNew = InternalGetCachedChannel(channel.Id);
            DiscordChannel channelOld = null;

            if (channelNew != null)
            {
                channelOld = new DiscordChannel
                {
                    Bitrate = channelNew.Bitrate,
                    Discord = this,
                    GuildId = channelNew.GuildId,
                    Id = channelNew.Id,
                    //IsPrivate = channel_new.IsPrivate,
                    LastMessageId = channelNew.LastMessageId,
                    Name = channelNew.Name,
                    _permissionOverwrites = new List<DiscordOverwrite>(channelNew._permissionOverwrites),
                    Position = channelNew.Position,
                    Topic = channelNew.Topic,
                    Type = channelNew.Type,
                    UserLimit = channelNew.UserLimit,
                    ParentId = channelNew.ParentId,
                    IsNsfw = channelNew.IsNsfw
                };
            }
            else
            {
                gld._channels.Add(channel);
            }

            if (channelNew != null)
            {
                channelNew.Bitrate = channel.Bitrate;
                channelNew.Name = channel.Name;
                channelNew.Position = channel.Position;
                channelNew.Topic = channel.Topic;
                channelNew.UserLimit = channel.UserLimit;
                channelNew.ParentId = channel.ParentId;

                channelNew._permissionOverwrites.Clear();
                channelNew._permissionOverwrites.AddRange(channel._permissionOverwrites);

                await _channelUpdated.InvokeAsync(new ChannelUpdateEventArgs(this)
                {
                    ChannelAfter = channelNew,
                    Guild = gld,
                    ChannelBefore = channelOld
                });
            }
        }

        internal async Task OnChannelDeleteEventAsync(DiscordChannel channel)
        {
            if (channel == null)
            {
                return;
            }

            channel.Discord = this;

            //if (channel.IsPrivate)
            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var chn = channel as DiscordDmChannel;

                // ReSharper disable AccessToModifiedClosure
                var index = _private_channels.FindIndex(xc => chn != null && xc.Id == chn.Id);
                // ReSharper restore AccessToModifiedClosure
                chn = _private_channels[index];
                _private_channels.RemoveAt(index);

                await _dmChannelDeleted.InvokeAsync(new DmChannelDeleteEventArgs(this) { Channel = chn });
            }
            else
            {
                var gld = channel.Guild;
                // ReSharper disable AccessToModifiedClosure
                var index = gld._channels.FindIndex(xc => xc.Id == channel.Id);
                // ReSharper restore AccessToModifiedClosure
                channel = gld._channels[index];
                gld._channels.RemoveAt(index);

                await _channelDeleted.InvokeAsync(new ChannelDeleteEventArgs(this) { Channel = channel, Guild = gld });
            }
        }

        internal async Task OnChannelPinsUpdate(DiscordChannel channel, DateTimeOffset lastPinTimestamp)
        {
            if (channel == null)
            {
                return;
            }

            var ea = new ChannelPinsUpdateEventArgs(this)
            {
                Channel = channel,
                LastPinTimestamp = lastPinTimestamp
            };
            await _channelPinsUpdated.InvokeAsync(ea);
        }

        internal async Task OnGuildCreateEventAsync(DiscordGuild guild, JArray rawMembers, IEnumerable<DiscordPresence> presences)
        {
            if (presences != null)
            {
                presences = presences.Select(xp => { xp.Discord = this; return xp; });
                foreach (var xp in presences)
                {
                    _presences[xp.InternalUser.Id] = xp;
                }
            }

            var exists = _guilds.ContainsKey(guild.Id);

            guild.Discord = this;
            guild.IsUnavailable = false;
            var eventGuild = guild;
            if (exists)
            {
                guild = _guilds[eventGuild.Id];
            }

            if (guild._channels == null)
            {
                guild._channels = new List<DiscordChannel>();
            }
            if (guild._roles == null)
            {
                guild._roles = new List<DiscordRole>();
            }
            if (guild._emojis == null)
            {
                guild._emojis = new List<DiscordEmoji>();
            }
            if (guild._voiceStates == null)
            {
                guild._voiceStates = new List<DiscordVoiceState>();
            }
            if (guild._members == null)
            {
                guild._members = new List<DiscordMember>();
            }

            UpdateCachedGuild(eventGuild, rawMembers);

            guild.JoinedAt = eventGuild.JoinedAt;
            guild.IsLarge = eventGuild.IsLarge;
            guild.MemberCount = Math.Max(eventGuild.MemberCount, guild._members.Count);
            guild.IsUnavailable = eventGuild.IsUnavailable;
            guild._voiceStates.AddRange(eventGuild._voiceStates);

            foreach (var xc in guild._channels)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
            }
            foreach (var xe in guild._emojis)
            {
                xe.Discord = this;
            }
            foreach (var xvs in guild._voiceStates)
            {
                xvs.Discord = this;
            }
            foreach (var xr in guild._roles)
            {
                xr.Discord = this;
            }

            var dcompl = _guilds.Values.All(xg => !xg.IsUnavailable);
            Volatile.Write(ref GuildDownloadCompleted, dcompl);

            if (exists)
            {
                await _guildAvailable.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild });
            }
            else
            {
                await _guildCreated.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild });
            }
        }

        internal async Task OnGuildUpdateEventAsync(DiscordGuild guild, JArray rawMembers)
        {
            if (!_guilds.ContainsKey(guild.Id))
            {
                _guilds[guild.Id] = guild;
            }

            guild.Discord = this;
            guild.IsUnavailable = false;
            var eventGuild = guild;
            guild = _guilds[eventGuild.Id];

            if (guild._channels == null)
            {
                guild._channels = new List<DiscordChannel>();
            }
            if (guild._roles == null)
            {
                guild._roles = new List<DiscordRole>();
            }
            if (guild._emojis == null)
            {
                guild._emojis = new List<DiscordEmoji>();
            }
            if (guild._voiceStates == null)
            {
                guild._voiceStates = new List<DiscordVoiceState>();
            }
            if (guild._members == null)
            {
                guild._members = new List<DiscordMember>();
            }

            UpdateCachedGuild(eventGuild, rawMembers);

            foreach (var xc in guild._channels)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
            }
            foreach (var xe in guild._emojis)
            {
                xe.Discord = this;
            }
            foreach (var xvs in guild._voiceStates)
            {
                xvs.Discord = this;
            }
            foreach (var xr in guild._roles)
            {
                xr.Discord = this;
            }

            await _guildUpdated.InvokeAsync(new GuildUpdateEventArgs(this) { Guild = guild });
        }

        internal async Task OnGuildDeleteEventAsync(DiscordGuild guild, JArray rawMembers)
        {
            if (!_guilds.ContainsKey(guild.Id))
            {
                return;
            }

            var gld = _guilds[guild.Id];
            if (guild.IsUnavailable)
            {
                gld.IsUnavailable = true;

                await _guildUnavailable.InvokeAsync(new GuildDeleteEventArgs(this) { Guild = guild, Unavailable = true });
            }
            else
            {
                _guilds.Remove(guild.Id);

                await _guildDeleted.InvokeAsync(new GuildDeleteEventArgs(this) { Guild = gld });
            }
        }

        internal async Task OnGuildSyncEventAsync(DiscordGuild guild, bool isLarge, JArray rawMembers, IEnumerable<DiscordPresence> presences)
        {
            presences = presences.Select(xp => { xp.Discord = this; return xp; });
            foreach (var xp in presences)
            {
                _presences[xp.InternalUser.Id] = xp;
            }

            guild.IsSynced = true;
            guild.IsLarge = isLarge;

            UpdateCachedGuild(guild, rawMembers);

            if (Configuration.AutomaticGuildSync)
            {
                var dcompl = _guilds.Values.All(xg => xg.IsSynced);
                Volatile.Write(ref GuildDownloadCompleted, dcompl);
            }

            await _guildAvailable.InvokeAsync(new GuildCreateEventArgs(this) { Guild = guild });
        }

        internal async Task OnPresenceUpdateEventAsync(DiscordPresence presence, JObject rawUser, PresenceUpdateEventArgs ea)
        {
            presence.Discord = this;
            DiscordPresence old = null;

            if (_presences.ContainsKey(presence.InternalUser.Id))
            {
                old = _presences[presence.InternalUser.Id];
            }
            _presences[presence.InternalUser.Id] = presence;

            if (rawUser["username"] != null || rawUser["discriminator"] != null || rawUser["avatar"] != null)
            {
                var newUsername = rawUser["username"] != null ? new Optional<string>((string)rawUser["username"]) : default;
                var newDiscrim = rawUser["discriminator"] != null ? new Optional<string>((string)rawUser["discriminator"]) : default;
                var newAvatar = rawUser["avatar"] != null ? new Optional<string>((string)rawUser["avatar"]) : default;

                if (UserCache.TryGetValue(presence.InternalUser.Id, out var usr))
                {
                    if (newUsername.HasValue)
                    {
                        usr.Username = newUsername.Value;
                    }

                    if (newDiscrim.HasValue)
                    {
                        usr.Discriminator = newDiscrim.Value;
                    }

                    if (newAvatar.HasValue)
                    {
                        usr.AvatarHash = newAvatar.Value;
                    }
                }
            }

            ea.Client = this;
            ea.PresenceBefore = old;

            await _presenceUpdate.InvokeAsync(ea);
        }

        internal async Task OnGuildBanAddEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                // ReSharper disable AccessToModifiedClosure
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                // ReSharper restore AccessToModifiedClosure
                return old;
            });

            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(usr) { Discord = this, GuildId = guild.Id };
            var ea = new GuildBanAddEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await _guildBanAdd.InvokeAsync(ea);
        }

        internal async Task OnGuildBanRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                // ReSharper disable AccessToModifiedClosure
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                // ReSharper restore AccessToModifiedClosure
                return old;
            });

            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(usr) { Discord = this, GuildId = guild.Id };
            var ea = new GuildBanRemoveEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await _guildBanRemove.InvokeAsync(ea);
        }

        internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild guild, IEnumerable<DiscordEmoji> newEmojis)
        {
            var oldEmojis = new List<DiscordEmoji>(guild._emojis);
            guild._emojis.Clear();
            guild._emojis.AddRange(newEmojis.Select(xe => { xe.Discord = this; return xe; }));
            var ea = new GuildEmojisUpdateEventArgs(this)
            {
                Guild = guild,
                EmojisAfter = guild.Emojis,
                EmojisBefore = new ReadOnlyCollection<DiscordEmoji>(oldEmojis)
            };
            await _guildEmojisUpdate.InvokeAsync(ea);
        }

        internal async Task OnGuildIntegrationsUpdateEventAsync(DiscordGuild guild)
        {
            var ea = new GuildIntegrationsUpdateEventArgs(this)
            {
                Guild = guild
            };
            await _guildIntegrationsUpdate.InvokeAsync(ea);
        }

        internal async Task OnGuildMemberAddEventAsync(TransportMember member, DiscordGuild guild)
        {
            var usr = new DiscordUser(member.User) { Discord = this };
            UserCache.AddOrUpdate(member.User.Id, usr, (id, old) =>
            {
                // ReSharper disable AccessToModifiedClosure
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                // ReSharper restore AccessToModifiedClosure
                return old;
            });

            var mbr = new DiscordMember(member)
            {
                Discord = this,
                GuildId = guild.Id
            };

            guild._members.Add(mbr);
            guild.MemberCount++;

            var ea = new GuildMemberAddEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await _guildMemberAdd.InvokeAsync(ea);
        }

        internal async Task OnGuildMemberRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(new DiscordUser(user)) { Discord = this, GuildId = guild.Id };

            var index = guild._members.FindIndex(xm => xm.Id == mbr.Id);
            if (index > -1)
            {
                guild._members.RemoveAt(index);
            }
            guild.MemberCount--;

            var ea = new GuildMemberRemoveEventArgs(this)
            {
                Guild = guild,
                Member = mbr
            };
            await _guildMemberRemove.InvokeAsync(ea);
        }

        internal async Task OnGuildMemberUpdateEventAsync(TransportUser user, DiscordGuild guild, IEnumerable<ulong> roles, string nick)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                // ReSharper disable AccessToModifiedClosure
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                // ReSharper restore AccessToModifiedClosure
                return old;
            });

            var mbr = guild.Members.FirstOrDefault(xm => xm.Id == user.Id) ?? new DiscordMember(usr) { Discord = this, GuildId = guild.Id };

            var nickOld = mbr.Nickname;
            var rolesOld = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles));

            mbr.Nickname = nick;
            mbr._roleIds.Clear();
            mbr._roleIds.AddRange(roles);

            var ea = new GuildMemberUpdateEventArgs(this)
            {
                Guild = guild,
                Member = mbr,

                NicknameAfter = mbr.Nickname,
                RolesAfter = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles)),

                NicknameBefore = nickOld,
                RolesBefore = rolesOld
            };
            await _guildMemberUpdate.InvokeAsync(ea);
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
            await _guildRoleCreate.InvokeAsync(ea);
        }

        internal async Task OnGuildRoleUpdateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            var roleNew = guild.Roles.FirstOrDefault(xr => xr.Id == role.Id);
            if (roleNew != null)
            {
                var roleOld = new DiscordRole
                {
                    _color = roleNew._color,
                    Discord = this,
                    IsHoisted = roleNew.IsHoisted,
                    Id = roleNew.Id,
                    IsManaged = roleNew.IsManaged,
                    IsMentionable = roleNew.IsManaged,
                    Name = roleNew.Name,
                    Permissions = roleNew.Permissions,
                    Position = roleNew.Position
                };

                roleNew._color = role._color;
                roleNew.IsHoisted = role.IsHoisted;
                roleNew.IsManaged = role.IsManaged;
                roleNew.IsMentionable = role.IsMentionable;
                roleNew.Name = role.Name;
                roleNew.Permissions = role.Permissions;
                roleNew.Position = role.Position;

                var ea = new GuildRoleUpdateEventArgs(this)
                {
                    Guild = guild,
                    RoleAfter = roleNew,
                    RoleBefore = roleOld
                };
                await _guildRoleUpdate.InvokeAsync(ea);
            }
        }

        internal async Task OnGuildRoleDeleteEventAsync(ulong roleId, DiscordGuild guild)
        {
            var index = guild._roles.FindIndex(xr => xr.Id == roleId);
            var role = guild._roles[index];
            guild._roles.RemoveAt(index);

            var ea = new GuildRoleDeleteEventArgs(this)
            {
                Guild = guild,
                Role = role
            };
            await _guildRoleDelete.InvokeAsync(ea);
        }

        internal async Task OnMessageAckEventAsync(DiscordChannel chn, ulong msgid)
        {
            DiscordMessage msg = null;
            if (MessageCache?.TryGet(xm => xm.Id == msgid && xm.ChannelId == chn.Id, out msg) != true)
            {
                msg = new DiscordMessage
                {
                    Id = msgid,
                    ChannelId = chn.Id,
                    Discord = this,
                };
            }

            await _messageAck.InvokeAsync(new MessageAcknowledgeEventArgs(this) { Message = msg });
        }

        internal async Task OnMessageCreateEventAsync(DiscordMessage message, TransportUser author)
        {
            message.Discord = this;

            if (message.Channel == null)
            {
                DebugLogger.LogMessage(LogLevel.Warning, "Event", "Could not find channel last message belonged to", DateTime.Now);
            }
            else
            {
                message.Channel.LastMessageId = message.Id;
            }

            var guild = message.Channel?.Guild;

            var usr = new DiscordUser(author) { Discord = this };
            usr = UserCache.AddOrUpdate(author.Id, usr, (id, old) =>
            {
                // ReSharper disable AccessToModifiedClosure
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                // ReSharper restore AccessToModifiedClosure
                return old;
            });

            if (guild != null)
            {
                var mbr = guild.Members.FirstOrDefault(xm => xm.Id == author.Id) ?? new DiscordMember(usr) { Discord = this, GuildId = guild.Id };
                message.Author = mbr;
            }
            else
            {
                message.Author = usr;
            }

            var mentionedUsers = new List<DiscordUser>();
            var mentionedRoles = guild != null ? new List<DiscordRole>() : null;
            var mentionedChannels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                if (guild != null)
                {
                    mentionedUsers = Utilities.GetUserMentions(message).Select(xid => guild._members.FirstOrDefault(xm => xm.Id == xid)).Cast<DiscordUser>().ToList();
                    mentionedRoles = Utilities.GetRoleMentions(message).Select(xid => guild._roles.FirstOrDefault(xr => xr.Id == xid)).ToList();
                    mentionedChannels = Utilities.GetChannelMentions(message).Select(xid => guild._channels.FirstOrDefault(xc => xc.Id == xid)).ToList();
                }
                else
                {
                    mentionedUsers = Utilities.GetUserMentions(message).Select(InternalGetCachedUser).ToList();
                }
            }

            message._mentionedUsers = mentionedUsers;
            message._mentionedRoles = mentionedRoles;
            message._mentionedChannels = mentionedChannels;

            if (message._reactions == null)
            {
                message._reactions = new List<DiscordReaction>();
            }
            foreach (var xr in message._reactions)
            {
                xr.Emoji.Discord = this;
            }

            if (Configuration.MessageCacheSize > 0 && message.Channel != null)
            {
                MessageCache.Add(message);
            }

            MessageCreateEventArgs ea = new MessageCreateEventArgs(this)
            {
                Message = message,

                MentionedUsers = new ReadOnlyCollection<DiscordUser>(mentionedUsers),
                MentionedRoles = mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(mentionedRoles) : null,
                MentionedChannels = mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(mentionedChannels) : null
            };
            await _messageCreated.InvokeAsync(ea);
        }

        internal async Task OnMessageUpdateEventAsync(DiscordMessage message, TransportUser author)
        {
            DiscordGuild guild;

            message.Discord = this;
            var eventMessage = message;

            if (Configuration.MessageCacheSize > 0 && MessageCache.TryGet(xm => xm.Id == eventMessage.Id && xm.ChannelId == eventMessage.ChannelId, out message) != true)
            {
                message = eventMessage;
                guild = message.Channel?.Guild;

                if (author != null)
                {
                    var usr = new DiscordUser(author) { Discord = this };
                    usr = UserCache.AddOrUpdate(author.Id, usr, (id, old) =>
                    {
                        // ReSharper disable AccessToModifiedClosure
                        old.Username = usr.Username;
                        old.Discriminator = usr.Discriminator;
                        old.AvatarHash = usr.AvatarHash;
                        // ReSharper restore AccessToModifiedClosure
                        return old;
                    });

                    if (guild != null)
                    {
                        var mbr = guild.Members.FirstOrDefault(xm => xm.Id == author.Id) ?? new DiscordMember(usr) { Discord = this, GuildId = guild.Id };
                        message.Author = mbr;
                    }
                    else
                    {
                        message.Author = usr;
                    }
                }

                if (message._reactions == null)
                {
                    message._reactions = new List<DiscordReaction>();
                }
                foreach (var xr in message._reactions)
                {
                    xr.Emoji.Discord = this;
                }
            }
            else
            {
                guild = message.Channel?.Guild;
                message.EditedTimestampRaw = eventMessage.EditedTimestampRaw;
                if (eventMessage.Content != null)
                {
                    message.Content = eventMessage.Content;
                }
                message._embeds.Clear();
                message._embeds.AddRange(eventMessage._embeds);
                message.Pinned = eventMessage.Pinned;
                message.IsTts = eventMessage.IsTts;
                message.Content = eventMessage.Content;
            }

            var mentionedUsers = new List<DiscordUser>();
            var mentionedRoles = guild != null ? new List<DiscordRole>() : null;
            var mentionedChannels = guild != null ? new List<DiscordChannel>() : null;

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                if (guild != null)
                {
                    mentionedUsers = Utilities.GetUserMentions(message).Select(xid => guild._members.FirstOrDefault(xm => xm.Id == xid)).Cast<DiscordUser>().ToList();
                    mentionedRoles = Utilities.GetRoleMentions(message).Select(xid => guild._roles.FirstOrDefault(xr => xr.Id == xid)).ToList();
                    mentionedChannels = Utilities.GetChannelMentions(message).Select(xid => guild._channels.FirstOrDefault(xc => xc.Id == xid)).ToList();
                }
                else
                {
                    mentionedUsers = Utilities.GetUserMentions(message).Select(InternalGetCachedUser).ToList();
                }
            }

            message._mentionedUsers = mentionedUsers;
            message._mentionedRoles = mentionedRoles;
            message._mentionedChannels = mentionedChannels;

            var ea = new MessageUpdateEventArgs(this)
            {
                Message = message,

                MentionedUsers = new ReadOnlyCollection<DiscordUser>(mentionedUsers),
                MentionedRoles = mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(mentionedRoles) : null,
                MentionedChannels = mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(mentionedChannels) : null
            };
            await _messageUpdate.InvokeAsync(ea);
        }

        internal async Task OnMessageDeleteEventAsync(ulong messageId, DiscordChannel channel)
        {
            if (Configuration.MessageCacheSize == 0 || !MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channel.Id, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channel.Id,
                    Discord = this,
                };
            }
            if (Configuration.MessageCacheSize > 0)
            {
                MessageCache.Remove(xm => xm.Id == msg.Id && xm.ChannelId == channel.Id);
            }

            var ea = new MessageDeleteEventArgs(this)
            {
                Channel = channel,
                Message = msg
            };
            await _messageDelete.InvokeAsync(ea);
        }

        internal async Task OnMessageBulkDeleteEventAsync(IEnumerable<ulong> messageIds, DiscordChannel channel)
        {
            var enumerable = messageIds as ulong[] ?? messageIds.ToArray();
            var msgs = new List<DiscordMessage>(enumerable.Count());
            foreach (var messageId in enumerable)
            {
                DiscordMessage msg = null;
                if (Configuration.MessageCacheSize > 0 && !MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channel.Id, out msg))
                {
                    msg = new DiscordMessage
                    {
                        Id = messageId,
                        ChannelId = channel.Id,
                        Discord = this,
                    };
                }
                if (Configuration.MessageCacheSize > 0)
                {
                    MessageCache.Remove(xm => msg != null && (xm.Id == msg.Id && xm.ChannelId == channel.Id));
                }
                msgs.Add(msg);
            }

            var ea = new MessageBulkDeleteEventArgs(this)
            {
                Channel = channel,
                Messages = new ReadOnlyCollection<DiscordMessage>(msgs)
            };
            await _messageBulkDelete.InvokeAsync(ea);
        }

        internal async Task OnTypingStartEventAsync(ulong userId, DiscordChannel channel, DateTimeOffset started)
        {
            if (channel == null)
            {
                return;
            }

            if (!UserCache.TryGetValue(userId, out var usr))
            {
                usr = new DiscordUser { Id = userId, Discord = this };
            }

            if (channel.Guild != null)
            {
                usr = channel.Guild.Members.FirstOrDefault(xm => xm.Id == userId) ?? new DiscordMember(usr) { Discord = this, GuildId = channel.GuildId };
            }

            var ea = new TypingStartEventArgs(this)
            {
                Channel = channel,
                User = usr,
                StartedAt = started
            };
            await _typingStart.InvokeAsync(ea);
        }

        internal async Task OnUserSettingsUpdateEventAsync(TransportUser user)
        {
            var usr = new DiscordUser(user) { Discord = this };

            var ea = new UserSettingsUpdateEventArgs(this)
            {
                User = usr
            };
            await _userSettingsUpdate.InvokeAsync(ea);
        }

        internal async Task OnUserUpdateEventAsync(TransportUser user)
        {
            var usrOld = new DiscordUser
            {
                AvatarHash = CurrentUser.AvatarHash,
                Discord = this,
                Discriminator = CurrentUser.Discriminator,
                Email = CurrentUser.Email,
                Id = CurrentUser.Id,
                IsBot = CurrentUser.IsBot,
                MfaEnabled = CurrentUser.MfaEnabled,
                Username = CurrentUser.Username,
                Verified = CurrentUser.Verified
            };

            CurrentUser.AvatarHash = user.AvatarHash;
            CurrentUser.Discriminator = user.Discriminator;
            CurrentUser.Email = user.Email;
            CurrentUser.Id = user.Id;
            CurrentUser.IsBot = user.IsBot;
            CurrentUser.MfaEnabled = user.MfaEnabled;
            CurrentUser.Username = user.Username;
            CurrentUser.Verified = user.Verified;

            var ea = new UserUpdateEventArgs(this)
            {
                UserAfter = CurrentUser,
                UserBefore = usrOld
            };
            await _userUpdate.InvokeAsync(ea);
        }

        internal async Task OnVoiceStateUpdateEventAsync(DiscordVoiceState voiceState)
        {
            voiceState.Discord = this;

            var index = voiceState.Guild._voiceStates.FindIndex(xvs => xvs.UserId == voiceState.UserId);
            if (index < 0)
            {
                voiceState.Guild._voiceStates.Add(voiceState);
            }
            else
            {
                voiceState.Guild._voiceStates[index] = voiceState;
            }

            var ea = new VoiceStateUpdateEventArgs(this)
            {
                Guild = voiceState.Guild,
                Channel = voiceState.Channel,
                User = voiceState.User,
                SessionId = voiceState.SessionId
            };
            await _voiceStateUpdate.InvokeAsync(ea);
        }

        internal async Task OnVoiceServerUpdateEventAsync(string endpoint, string token, DiscordGuild guild)
        {
            var ea = new VoiceServerUpdateEventArgs(this)
            {
                Endpoint = endpoint,
                VoiceToken = token,
                Guild = guild
            };
            await _voiceServerUpdate.InvokeAsync(ea);
        }

        internal async Task OnGuildMembersChunkEventAsync(IEnumerable<TransportMember> members, DiscordGuild guild)
        {
            var ids = guild.Members.Select(xm => xm.Id);
            var mbrs = members.Select(xtm => new DiscordMember(xtm) { Discord = this, GuildId = guild.Id })
                .Where(xm => !ids.Contains(xm.Id));

            var discordMembers = mbrs as DiscordMember[] ?? mbrs.ToArray();
            guild._members.AddRange(discordMembers);
            guild.MemberCount = guild._members.Count;

            var ea = new GuildMembersChunkEventArgs(this)
            {
                Guild = guild,
                Members = new ReadOnlyCollection<DiscordMember>(new List<DiscordMember>(discordMembers))
            };
            await _guildMembersChunk.InvokeAsync(ea);
        }

        internal async Task OnUnknownEventAsync(GatewayPayload payload)
        {
            var ea = new UnknownEventArgs(this) { EventName = payload.EventName, Json = (payload.Data as JObject)?.ToString() };
            await _unknownEvent.InvokeAsync(ea);
        }

        internal async Task OnMessageReactionAddAsync(ulong userId, ulong messageId, DiscordChannel channel, DiscordEmoji emoji)
        {
            emoji.Discord = this;

            if (!UserCache.TryGetValue(userId, out var usr))
            {
                usr = new DiscordUser { Id = userId, Discord = this };
            }

            if (channel.Guild != null)
            {
                usr = channel.Guild.Members.FirstOrDefault(xm => xm.Id == userId) ?? new DiscordMember(usr) { Discord = this, GuildId = channel.GuildId };
            }

            if (Configuration.MessageCacheSize == 0 || !MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channel.Id, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channel.Id,
                    Discord = this,
                    _reactions = new List<DiscordReaction>()
                };
            }

            var react = msg._reactions.FirstOrDefault(xr => xr.Emoji == emoji);
            if (react == null)
            {
                msg._reactions.Add(new DiscordReaction
                {
                    Count = 1,
                    Emoji = emoji,
                    IsMe = CurrentUser.Id == userId
                });
            }
            else
            {
                react.Count++;
                react.IsMe |= CurrentUser.Id == userId;
            }

            var ea = new MessageReactionAddEventArgs(this)
            {
                Message = msg,
                Channel = channel,
                User = usr,
                Emoji = emoji
            };
            await _messageReactionAdd.InvokeAsync(ea);
        }

        internal async Task OnMessageReactionRemoveAsync(ulong userId, ulong messageId, DiscordChannel channel, DiscordEmoji emoji)
        {
            emoji.Discord = this;

            if (!UserCache.TryGetValue(userId, out var usr))
            {
                usr = new DiscordUser { Id = userId, Discord = this };
            }

            if (channel.Guild != null)
            {
                usr = channel.Guild.Members.FirstOrDefault(xm => xm.Id == userId) ?? new DiscordMember(usr) { Discord = this, GuildId = channel.GuildId };
            }

            if (Configuration.MessageCacheSize == 0 ||
                !MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channel.Id, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channel.Id,
                    Discord = this
                };
            }

            var react = msg._reactions?.FirstOrDefault(xr => xr.Emoji == emoji);
            if (react != null)
            {
                react.Count--;
                react.IsMe &= CurrentUser.Id != userId;

                if (msg._reactions != null && react.Count <= 0) // shit happens
                {
                    for (var i = 0; i < msg._reactions.Count; i++)
                    {
                        if (msg._reactions[i].Emoji == emoji)
                        {
                            msg._reactions.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            var ea = new MessageReactionRemoveEventArgs(this)
            {
                Message = msg,
                Channel = channel,
                User = usr,
                Emoji = emoji
            };
            await _messageReactionRemove.InvokeAsync(ea);
        }

        internal async Task OnMessageReactionRemoveAllAsync(ulong messageId, DiscordChannel channel)
        {
            if (Configuration.MessageCacheSize == 0 ||
                !MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channel.Id, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channel.Id,
                    Discord = this
                };
            }

            msg._reactions?.Clear();

            var ea = new MessageReactionsClearEventArgs(this)
            {
                Message = msg,
                Channel = channel
            };
            await _messageReactionRemoveAll.InvokeAsync(ea);
        }

        internal async Task OnWebhooksUpdateAsync(DiscordChannel channel, DiscordGuild guild)
        {
            var ea = new WebhooksUpdateEventArgs(this)
            {
                Channel = channel,
                Guild = guild
            };
            await _webhooksUpdate.InvokeAsync(ea);
        }
        #endregion

        internal async Task OnHeartbeatAsync(long seq)
        {
            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received Heartbeat - Sending Ack.", DateTime.Now);
            await SendHeartbeatAsync(seq);
        }

        internal async Task OnReconnectAsync()
        {
            DebugLogger.LogMessage(LogLevel.Info, "Websocket", "Received OP 7 - Reconnect. ", DateTime.Now);

            await ReconnectAsync();
        }

        internal async Task OnInvalidateSessionAsync(bool data)
        {
            if (data)
            {
                DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received true in OP 9 - Waiting a few second and sending resume again.", DateTime.Now);
                await Task.Delay(6000);
                await SendResumeAsync();
            }
            else
            {
                DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received false in OP 9 - Starting a new session", DateTime.Now);
                SessionId = "";
                await SendIdentifyAsync();
            }
        }

        internal async Task OnHelloAsync(GatewayHello hello)
        {
            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received OP 10 (HELLO) - Trying to either resume or identify", DateTime.Now);
            //this._waiting_for_ack = false;
            Interlocked.CompareExchange(ref SkippedHeartbeats, 0, 0);
            HeartbeatInterval = hello.HeartbeatInterval;
            HeartbeatTask = new Task(StartHeartbeating, CancelToken, TaskCreationOptions.LongRunning);
            HeartbeatTask.Start();

            if (SessionId == "")
            {
                await SendIdentifyAsync();
            }
            else
            {
                await SendResumeAsync();
            }

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
            Interlocked.Decrement(ref SkippedHeartbeats);

            var ping = (int)(DateTime.Now - LastHeartbeat).TotalMilliseconds;

            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Received WebSocket Heartbeat Ack", DateTime.Now);
            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", $"Ping {ping.ToString(CultureInfo.InvariantCulture)}ms", DateTime.Now);

            Volatile.Write(ref _ping, ping);

            var args = new HeartbeatEventArgs(this)
            {
                Ping = Ping,
                Timestamp = DateTimeOffset.Now
            };

            await _heartbeated.InvokeAsync(args);
        }

        //internal async Task StartHeartbeatingAsync()
        internal void StartHeartbeating()
        {
            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Starting Heartbeat", DateTime.Now);
            var token = CancelToken;
            try
            {
                while (true)
                {
                    SendHeartbeatAsync().GetAwaiter().GetResult();
                    Task.Delay(HeartbeatInterval, CancelToken).GetAwaiter().GetResult();
                    token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException) { }
        }

        internal Task InternalUpdateStatusAsync(DiscordGame game, UserStatus? userStatus, DateTimeOffset? idleSince)
        {
            if (game != null && game.Name != null && game.Name.Length > 128)
            {
                throw new Exception("Game name can't be longer than 128 characters!");
            }

            var sinceUnix = idleSince != null ? (long?)Utilities.GetUnixTime(idleSince.Value) : null;

            var status = new StatusUpdate
            {
                Game = new TransportGame(game ?? new DiscordGame()),
                IdleSince = sinceUnix,
                IsAfk = idleSince != null,
                Status = userStatus ?? UserStatus.Online
            };
            var statusUpdate = new GatewayPayload
            {
                OpCode = GatewayOpCode.StatusUpdate,
                Data = status
            };
            var statusstr = JsonConvert.SerializeObject(statusUpdate);

            WebsocketClient.SendMessage(statusstr);
            return Task.Delay(0);
        }

        internal Task SendHeartbeatAsync()
        {
            var lastHeartbeat = DateTimeOffset.Now;
            var sequence = (long)(lastHeartbeat - DiscordEpoch).TotalMilliseconds;

            return SendHeartbeatAsync(sequence);
        }

        internal async Task SendHeartbeatAsync(long seq)
        {
            //if (_waiting_for_ack)
            //{
            //    _debugLogger.LogMessage(LogLevel.Critical, "Websocket", "Missed a heartbeat ack. Reconnecting.", DateTime.Now);
            //    await ReconnectAsync();
            //}
            var moreThan5 = Volatile.Read(ref SkippedHeartbeats) > 5;
            var guildsComp = Volatile.Read(ref GuildDownloadCompleted);
            if (guildsComp && moreThan5)
            {
                DebugLogger.LogMessage(LogLevel.Critical, "DSharpPlus", "More than 5 heartbeats were skipped. Issuing reconnect.", DateTime.Now);
                await ReconnectAsync();
                return;
            }

            if (!guildsComp && moreThan5)
            {
                DebugLogger.LogMessage(LogLevel.Warning, "DSharpPlus", "More than 5 heartbeats were skipped while the guild download is running.", DateTime.Now);
            }

            Volatile.Write(ref LastSequence, seq);
            DebugLogger.LogMessage(LogLevel.Debug, "Websocket", "Sending Heartbeat", DateTime.Now);
            var heartbeat = new GatewayPayload
            {
                OpCode = GatewayOpCode.Heartbeat,
                Data = seq
            };
            var heartbeatStr = JsonConvert.SerializeObject(heartbeat);
            WebsocketClient.SendMessage(heartbeatStr);

            LastHeartbeat = DateTimeOffset.Now;

            //_waiting_for_ack = true;
            Interlocked.Increment(ref SkippedHeartbeats);
        }

        internal Task SendIdentifyAsync()
        {
            var identify = new GatewayIdentify
            {
                Token = Utilities.GetFormattedToken(this),
                Compress = Configuration.EnableCompression,
                LargeThreshold = Configuration.LargeThreshold,
                ShardInfo = new ShardInfo
                {
                    ShardId = Configuration.ShardId,
                    ShardCount = Configuration.ShardCount
                }
            };
            var payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Identify,
                Data = identify
            };
            var payloadstr = JsonConvert.SerializeObject(payload);
            WebsocketClient.SendMessage(payloadstr);
            return Task.Delay(0);
        }

        internal Task SendResumeAsync()
        {
            var resume = new GatewayResume
            {
                Token = Utilities.GetFormattedToken(this),
                SessionId = SessionId,
                SequenceNumber = Volatile.Read(ref LastSequence)
            };
            var resumePayload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Resume,
                Data = resume
            };
            var resumestr = JsonConvert.SerializeObject(resumePayload);

            WebsocketClient.SendMessage(resumestr);
            return Task.Delay(0);
        }

        internal Task SendGuildSyncAsync()
        {
            return SyncGuildsAsync(_guilds.Values.ToArray());
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
            var vsuPayload = new GatewayPayload
            {
                OpCode = GatewayOpCode.VoiceStateUpdate,
                Data = vsu
            };
            var vsustr = JsonConvert.SerializeObject(vsuPayload);

            WebsocketClient.SendMessage(vsustr);
            return Task.Delay(0);
        }
        #endregion

        // LINQ :^)
        internal DiscordChannel InternalGetCachedChannel(ulong channelId) =>
            Guilds.Values.SelectMany(xg => xg.Channels)
                .Concat(_private_channels)
                .FirstOrDefault(xc => xc.Id == channelId);

        internal void UpdateCachedGuild(DiscordGuild newGuild, JArray rawMembers)
        {
            if (!_guilds.ContainsKey(newGuild.Id))
            {
                _guilds[newGuild.Id] = newGuild;
            }

            var guild = _guilds[newGuild.Id];

            if (newGuild._channels != null && newGuild._channels.Any())
            {
                var c = newGuild._channels.Where(xc => !guild._channels.Any(xxc => xxc.Id == xc.Id));
                guild._channels.AddRange(c);
            }

            var e = newGuild._emojis.Where(xe => !guild._emojis.Any(xxe => xxe.Id == xe.Id));
            guild._emojis.AddRange(e);

            if (rawMembers != null)
            {
                guild._members.Clear();

                foreach (var xj in rawMembers)
                {
                    var xtm = xj.ToObject<TransportMember>();

                    var xu = new DiscordUser(xtm.User) { Discord = this };
                    UserCache.AddOrUpdate(xtm.User.Id, xu, (id, old) =>
                    {
                        // ReSharper disable AccessToModifiedClosure
                        old.Username = xu.Username;
                        old.Discriminator = xu.Discriminator;
                        old.AvatarHash = xu.AvatarHash;
                        // ReSharper restore AccessToModifiedClosure
                        return old;
                    });

                    guild._members.Add(new DiscordMember(xtm) { Discord = this, GuildId = guild.Id });
                }
            }

            var r = newGuild._roles.Where(xr => guild._roles.All(xxr => xxr.Id != xr.Id));
            guild._roles.AddRange(r);

            guild.Name = newGuild.Name;
            guild.AfkChannelId = newGuild.AfkChannelId;
            guild.AfkTimeout = newGuild.AfkTimeout;
            guild.DefaultMessageNotifications = newGuild.DefaultMessageNotifications;
            guild.EmbedChannelId = newGuild.EmbedChannelId;
            guild.EmbedEnabled = newGuild.EmbedEnabled;
            guild.Features = newGuild.Features;
            guild.IconHash = newGuild.IconHash;
            guild.MfaLevel = newGuild.MfaLevel;
            guild.OwnerId = newGuild.OwnerId;
            guild.RegionId = newGuild.RegionId;
            guild.SplashHash = newGuild.SplashHash;
            guild.VerificationLevel = newGuild.VerificationLevel;
            guild.ExplicitContentFilter = newGuild.ExplicitContentFilter;

            // fields not sent for update:
            // - guild.Channels
            // - voice states
            // - guild.JoinedAt = new_guild.JoinedAt;
            // - guild.Large = new_guild.Large;
            // - guild.MemberCount = Math.Max(new_guild.MemberCount, guild._members.Count);
            // - guild.Unavailable = new_guild.Unavailable;
        }

        internal async Task InternalUpdateGatewayAsync()
        {
            var headers = Utilities.GetBaseHeaders();

            var route = Endpoints.Gateway;
            if (Configuration.TokenType == TokenType.Bot)
            {
                route = string.Concat(route, Endpoints.Bot);
            }
            var bucket = ApiClient.Rest.GetBucket(RestRequestMethod.Get, route, new { }, out var path);

            var url = new Uri(string.Concat(Utilities.GetApiBaseUri(), path));
            var request = new RestRequest(this, bucket, url, RestRequestMethod.Get, headers);
            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = ApiClient.Rest.ExecuteRequestAsync(request);
            var response = await request.WaitForCompletionAsync();

            var jo = JObject.Parse(response.Response);
            _gateway_url = jo.Value<string>("url");
            if (jo["shards"] != null)
            {
                _shard_count = jo.Value<int>("shards");
            }
        }

        ~DiscordClient()
        {
            Dispose();
        }

        private bool _disposed;
        /// <summary>
        /// Disposes your DiscordClient.
        /// </summary>
        public override void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            GC.SuppressFinalize(this);

            DisconnectAsync().GetAwaiter().GetResult();
            CurrentUser = null;

            CancelTokenSource?.Cancel();
            _guilds = null;
            HeartbeatTask = null;
            Modules = null;
            _private_channels = null;
            WebsocketClient.InternalDisconnectAsync(null).GetAwaiter().GetResult();

            _disposed = true;
        }
    }
}
