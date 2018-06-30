#pragma warning disable CS0618
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Net;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Globalization;

namespace DSharpPlus
{
    /// <summary>
    /// A Discord client that shards automatically.
    /// </summary>
    public sealed class DiscordShardedClient
    {
        #region Events
        /// <summary>
        /// Fired whenever an error occurs within an event handler.
        /// </summary>
        public event AsyncEventHandler<ClientErrorEventArgs> ClientErrored
        {
            add { _clientErrored.Register(value); }
            remove { _clientErrored.Unregister(value); }
        }
        private AsyncEvent<ClientErrorEventArgs> _clientErrored;

        /// <summary>
        /// Fired whenever a WebSocket error occurs within the client.
        /// </summary>
        public event AsyncEventHandler<SocketErrorEventArgs> SocketErrored
        {
            add { _socketErrored.Register(value); }
            remove { _socketErrored.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _socketErrored;

        /// <summary>
        /// Fired whenever WebSocket connection is established.
        /// </summary>
        public event AsyncEventHandler SocketOpened
        {
            add { _socketOpened.Register(value); }
            remove { _socketOpened.Unregister(value); }
        }
        private AsyncEvent _socketOpened;

        /// <summary>
        /// Fired whenever WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<SocketCloseEventArgs> SocketClosed
        {
            add { _socketClosed.Register(value); }
            remove { _socketClosed.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _socketClosed;

        /// <summary>
        /// Fired when the client enters ready state.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Ready
        {
            add { _ready.Register(value); }
            remove { _ready.Unregister(value); }
        }
        private AsyncEvent<ReadyEventArgs> _ready;

        /// <summary>
        /// Fired whenever a session is resumed.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Resumed
        {
            add { _resumed.Register(value); }
            remove { _resumed.Unregister(value); }
        }
        private AsyncEvent<ReadyEventArgs> _resumed;

        /// <summary>
        /// Fired when a new channel is created.
        /// </summary>
        public event AsyncEventHandler<ChannelCreateEventArgs> ChannelCreated
        {
            add { _channelCreated.Register(value); }
            remove { _channelCreated.Unregister(value); }
        }
        private AsyncEvent<ChannelCreateEventArgs> _channelCreated;

        /// <summary>
        /// Fired when a new direct message channel is created.
        /// </summary>
        public event AsyncEventHandler<DmChannelCreateEventArgs> DmChannelCreated
        {
            add { _dmChannelCreated.Register(value); }
            remove { _dmChannelCreated.Unregister(value); }
        }
        private AsyncEvent<DmChannelCreateEventArgs> _dmChannelCreated;

        /// <summary>
        /// Fired when a channel is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelUpdateEventArgs> ChannelUpdated
        {
            add { _channelUpdated.Register(value); }
            remove { _channelUpdated.Unregister(value); }
        }
        private AsyncEvent<ChannelUpdateEventArgs> _channelUpdated;

        /// <summary>
        /// Fired when a channel is deleted
        /// </summary>
        public event AsyncEventHandler<ChannelDeleteEventArgs> ChannelDeleted
        {
            add { _channelDeleted.Register(value); }
            remove { _channelDeleted.Unregister(value); }
        }
        private AsyncEvent<ChannelDeleteEventArgs> _channelDeleted;

        /// <summary>
        /// Fired when a dm channel is deleted
        /// </summary>
        public event AsyncEventHandler<DmChannelDeleteEventArgs> DmChannelDeleted
        {
            add { _dmChannelDeleted.Register(value); }
            remove { _dmChannelDeleted.Unregister(value); }
        }
        private AsyncEvent<DmChannelDeleteEventArgs> _dmChannelDeleted;

        /// <summary>
        /// Fired whenever a channel's pinned message list is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelPinsUpdateEventArgs> ChannelPinsUpdated
        {
            add { _channelPinsUpdated.Register(value); }
            remove { _channelPinsUpdated.Unregister(value); }
        }
        private AsyncEvent<ChannelPinsUpdateEventArgs> _channelPinsUpdated;

        /// <summary>
        /// Fired when the user joins a new guild.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildCreated
        {
            add { _guildCreated.Register(value); }
            remove { _guildCreated.Unregister(value); }
        }
        private AsyncEvent<GuildCreateEventArgs> _guildCreated;

        /// <summary>
        /// Fired when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildAvailable
        {
            add { _guildAvailable.Register(value); }
            remove { _guildAvailable.Unregister(value); }
        }
        private AsyncEvent<GuildCreateEventArgs> _guildAvailable;

        /// <summary>
        /// Fired when a guild is updated.
        /// </summary>
        public event AsyncEventHandler<GuildUpdateEventArgs> GuildUpdated
        {
            add { _guildUpdated.Register(value); }
            remove { _guildUpdated.Unregister(value); }
        }
        private AsyncEvent<GuildUpdateEventArgs> _guildUpdated;

        /// <summary>
        /// Fired when the user leaves or is removed from a guild.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildDeleted
        {
            add { _guildDeleted.Register(value); }
            remove { _guildDeleted.Unregister(value); }
        }
        private AsyncEvent<GuildDeleteEventArgs> _guildDeleted;

        /// <summary>
        /// Fired when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildUnavailable
        {
            add { _guildUnavailable.Register(value); }
            remove { _guildUnavailable.Unregister(value); }
        }
        private AsyncEvent<GuildDeleteEventArgs> _guildUnavailable;

        /// <summary>
        /// Fired when all guilds finish streaming from Discord.
        /// </summary>
        public event AsyncEventHandler<GuildDownloadCompletedEventArgs> GuildDownloadCompleted
        {
            add { _guildDownloadCompleted.Register(value); }
            remove { _guildDownloadCompleted.Unregister(value); }
        }
        private AsyncEvent<GuildDownloadCompletedEventArgs> _guildDownloadCompleted;

        /// <summary>
        /// Fired when a message is created.
        /// </summary>
        public event AsyncEventHandler<MessageCreateEventArgs> MessageCreated
        {
            add { _messageCreated.Register(value); }
            remove { _messageCreated.Unregister(value); }
        }
        private AsyncEvent<MessageCreateEventArgs> _messageCreated;

        /// <summary>
        /// Fired when a 
        /// has been updated.
        /// </summary>
        public event AsyncEventHandler<PresenceUpdateEventArgs> PresenceUpdated
        {
            add { _presenceUpdated.Register(value); }
            remove { _presenceUpdated.Unregister(value); }
        }
        private AsyncEvent<PresenceUpdateEventArgs> _presenceUpdated;

        /// <summary>
        /// Fired when a guild ban gets added
        /// </summary>
        public event AsyncEventHandler<GuildBanAddEventArgs> GuildBanAdded
        {
            add { _guildBanAdded.Register(value); }
            remove { _guildBanAdded.Unregister(value); }
        }
        private AsyncEvent<GuildBanAddEventArgs> _guildBanAdded;

        /// <summary>
        /// Fired when a guild ban gets removed
        /// </summary>
        public event AsyncEventHandler<GuildBanRemoveEventArgs> GuildBanRemoved
        {
            add { _guildBanRemoved.Register(value); }
            remove { _guildBanRemoved.Unregister(value); }
        }
        private AsyncEvent<GuildBanRemoveEventArgs> _guildBanRemoved;

        /// <summary>
        /// Fired when a guilds emojis get updated
        /// </summary>
        public event AsyncEventHandler<GuildEmojisUpdateEventArgs> GuildEmojisUpdated
        {
            add { _guildEmojisUpdated.Register(value); }
            remove { _guildEmojisUpdated.Unregister(value); }
        }
        private AsyncEvent<GuildEmojisUpdateEventArgs> _guildEmojisUpdated;

        /// <summary>
        /// Fired when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
        {
            add { _guildIntegrationsUpdated.Register(value); }
            remove { _guildIntegrationsUpdated.Unregister(value); }
        }
        private AsyncEvent<GuildIntegrationsUpdateEventArgs> _guildIntegrationsUpdated;

        /// <summary>
        /// Fired when a new user joins a guild.
        /// </summary>
        public event AsyncEventHandler<GuildMemberAddEventArgs> GuildMemberAdded
        {
            add { _guildMemberAdded.Register(value); }
            remove { _guildMemberAdded.Unregister(value); }
        }
        private AsyncEvent<GuildMemberAddEventArgs> _guildMemberAdded;

        /// <summary>
        /// Fired when a user is removed from a guild (leave/kick/ban).
        /// </summary>
        public event AsyncEventHandler<GuildMemberRemoveEventArgs> GuildMemberRemoved
        {
            add { _guildMemberRemoved.Register(value); }
            remove { _guildMemberRemoved.Unregister(value); }
        }
        private AsyncEvent<GuildMemberRemoveEventArgs> _guildMemberRemoved;

        /// <summary>
        /// Fired when a guild member is updated.
        /// </summary>
        public event AsyncEventHandler<GuildMemberUpdateEventArgs> GuildMemberUpdated
        {
            add { _guildMemberUpdated.Register(value); }
            remove { _guildMemberUpdated.Unregister(value); }
        }
        private AsyncEvent<GuildMemberUpdateEventArgs> _guildMemberUpdated;

        /// <summary>
        /// Fired when a guild role is created.
        /// </summary>
        public event AsyncEventHandler<GuildRoleCreateEventArgs> GuildRoleCreated
        {
            add { _guildRoleCreated.Register(value); }
            remove { _guildRoleCreated.Unregister(value); }
        }
        private AsyncEvent<GuildRoleCreateEventArgs> _guildRoleCreated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleUpdateEventArgs> GuildRoleUpdated
        {
            add { _guildRoleUpdated.Register(value); }
            remove { _guildRoleUpdated.Unregister(value); }
        }
        private AsyncEvent<GuildRoleUpdateEventArgs> _guildRoleUpdated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleDeleteEventArgs> GuildRoleDeleted
        {
            add { _guildRoleDeleted.Register(value); }
            remove { _guildRoleDeleted.Unregister(value); }
        }
        private AsyncEvent<GuildRoleDeleteEventArgs> _guildRoleDeleted;

        /// <summary>
        /// Fired when a message is updated.
        /// </summary>
        public event AsyncEventHandler<MessageUpdateEventArgs> MessageUpdated
        {
            add { _messageUpdated.Register(value); }
            remove { _messageUpdated.Unregister(value); }
        }
        private AsyncEvent<MessageUpdateEventArgs> _messageUpdated;

        /// <summary>
        /// Fired when a message is deleted.
        /// </summary>
        public event AsyncEventHandler<MessageDeleteEventArgs> MessageDeleted
        {
            add { _messageDeleted.Register(value); }
            remove { _messageDeleted.Unregister(value); }
        }
        private AsyncEvent<MessageDeleteEventArgs> _messageDeleted;

        /// <summary>
        /// Fired when multiple messages are deleted at once.
        /// </summary>
        public event AsyncEventHandler<MessageBulkDeleteEventArgs> MessagesBulkDeleted
        {
            add { _messageBulkDeleted.Register(value); }
            remove { _messageBulkDeleted.Unregister(value); }
        }
        private AsyncEvent<MessageBulkDeleteEventArgs> _messageBulkDeleted;

        /// <summary>
        /// Fired when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<TypingStartEventArgs> TypingStarted
        {
            add { _typingStarted.Register(value); }
            remove { _typingStarted.Unregister(value); }
        }
        private AsyncEvent<TypingStartEventArgs> _typingStarted;

        /// <summary>
        /// Fired when the current user updates their settings.
        /// </summary>
        public event AsyncEventHandler<UserSettingsUpdateEventArgs> UserSettingsUpdated
        {
            add { _userSettingsUpdated.Register(value); }
            remove { _userSettingsUpdated.Unregister(value); }
        }
        private AsyncEvent<UserSettingsUpdateEventArgs> _userSettingsUpdated;

        /// <summary>
        /// Fired when properties about the user change.
        /// </summary>
        public event AsyncEventHandler<UserUpdateEventArgs> UserUpdated
        {
            add { _userUpdated.Register(value); }
            remove { _userUpdated.Unregister(value); }
        }
        private AsyncEvent<UserUpdateEventArgs> _userUpdated;

        /// <summary>
        /// Fired when someone joins/leaves/moves voice channels.
        /// </summary>
        public event AsyncEventHandler<VoiceStateUpdateEventArgs> VoiceStateUpdated
        {
            add { _voiceStateUpdated.Register(value); }
            remove { _voiceStateUpdated.Unregister(value); }
        }
        private AsyncEvent<VoiceStateUpdateEventArgs> _voiceStateUpdated;

        /// <summary>
        /// Fired when a guild's voice server is updated.
        /// </summary>
        public event AsyncEventHandler<VoiceServerUpdateEventArgs> VoiceServerUpdated
        {
            add { _voiceServerUpdated.Register(value); }
            remove { _voiceServerUpdated.Unregister(value); }
        }
        private AsyncEvent<VoiceServerUpdateEventArgs> _voiceServerUpdated;

        /// <summary>
        /// Fired in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<GuildMembersChunkEventArgs> GuildMembersChunked
        {
            add { _guildMembersChunk.Register(value); }
            remove { _guildMembersChunk.Unregister(value); }
        }
        private AsyncEvent<GuildMembersChunkEventArgs> _guildMembersChunk;

        /// <summary>
        /// Fired when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<UnknownEventArgs> UnknownEvent
        {
            add { _unknownEvent.Register(value); }
            remove { _unknownEvent.Unregister(value); }
        }
        private AsyncEvent<UnknownEventArgs> _unknownEvent;

        /// <summary>
        /// Fired when a reaction gets added to a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionAddEventArgs> MessageReactionAdded
        {
            add { _messageReactionAdded.Register(value); }
            remove { _messageReactionAdded.Unregister(value); }
        }
        private AsyncEvent<MessageReactionAddEventArgs> _messageReactionAdded;

        /// <summary>
        /// Fired when a reaction gets removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEventArgs> MessageReactionRemoved
        {
            add { _messageReactionRemoved.Register(value); }
            remove { _messageReactionRemoved.Unregister(value); }
        }
        private AsyncEvent<MessageReactionRemoveEventArgs> _messageReactionRemoved;

        /// <summary>
        /// Fired when all reactions get removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionsClearEventArgs> MessageReactionsCleared
        {
            add { _messageReactionsCleared.Register(value); }
            remove { _messageReactionsCleared.Unregister(value); }
        }
        private AsyncEvent<MessageReactionsClearEventArgs> _messageReactionsCleared;

        /// <summary>
        /// Fired whenever webhooks update.
        /// </summary>
        public event AsyncEventHandler<WebhooksUpdateEventArgs> WebhooksUpdated
        {
            add { _webhooksUpdated.Register(value); }
            remove { _webhooksUpdated.Unregister(value); }
        }
        private AsyncEvent<WebhooksUpdateEventArgs> _webhooksUpdated;

        /// <summary>
        /// Fired on received heartbeat ACK.
        /// </summary>
        public event AsyncEventHandler<HeartbeatEventArgs> Heartbeated
        {
            add { _heartbeated.Register(value); }
            remove { _heartbeated.Unregister(value); }
        }
        private AsyncEvent<HeartbeatEventArgs> _heartbeated;

        internal void EventErrorHandler(string evname, Exception ex)
        {
            DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"An {ex.GetType()} occured in {evname}.", DateTime.Now);
            _clientErrored.InvokeAsync(new ClientErrorEventArgs(null) { EventName = evname, Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void Goof(string evname, Exception ex)
        {
            DebugLogger.LogMessage(LogLevel.Critical, "DSharpPlus", $"An {ex.GetType()} occured in the exception handler.", DateTime.Now);
        }
        #endregion

        private DiscordConfiguration Config { get; }
        private ConcurrentDictionary<int, DiscordClient> Shards { get; }

        /// <summary>
        /// Gets the logger for this client.
        /// </summary>
        public DebugLogger DebugLogger { get; }

        /// <summary>
        /// Gets all client shards.
        /// </summary>
        public IReadOnlyDictionary<int, DiscordClient> ShardClients 
            => new ReadOnlyDictionary<int, DiscordClient>(Shards);

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public DiscordUser CurrentUser 
            => _current_user;

        private DiscordUser _current_user;

        /// <summary>
        /// Gets the current application.
        /// </summary>
        public DiscordApplication CurrentApplication 
            => _current_application;

        private DiscordApplication _current_application;

        /// <summary>
        /// Gets the list of available voice regions. Note that this property will not contain VIP voice regions.
        /// </summary>
        public IReadOnlyDictionary<string, DiscordVoiceRegion> VoiceRegions 
            => _voice_regions_lazy?.Value;

        /// <summary>
        /// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
        /// </summary>
        private ConcurrentDictionary<string, DiscordVoiceRegion> InternalVoiceRegions { get; set; }
        private Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> _voice_regions_lazy;

        /// <summary>
        /// Initializes new auto-sharding Discord client.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        public DiscordShardedClient(DiscordConfiguration config)
        {
            if (config.TokenType == TokenType.User)
            {
                throw new InvalidOperationException("You cannot shard using a user token.");
            }

            _clientErrored = new AsyncEvent<ClientErrorEventArgs>(Goof, "CLIENT_ERRORED");
            _socketErrored = new AsyncEvent<SocketErrorEventArgs>(Goof, "SOCKET_ERRORED");
            _socketOpened = new AsyncEvent(EventErrorHandler, "SOCKET_OPENED");
            _socketClosed = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "SOCKET_CLOSED");
            _ready = new AsyncEvent<ReadyEventArgs>(EventErrorHandler, "READY");
            _resumed = new AsyncEvent<ReadyEventArgs>(EventErrorHandler, "RESUMED");
            _channelCreated = new AsyncEvent<ChannelCreateEventArgs>(EventErrorHandler, "CHANNEL_CREATED");
            _dmChannelCreated = new AsyncEvent<DmChannelCreateEventArgs>(EventErrorHandler, "DM_CHANNEL_CREATED");
            _channelUpdated = new AsyncEvent<ChannelUpdateEventArgs>(EventErrorHandler, "CHANNEL_UPDATED");
            _channelDeleted = new AsyncEvent<ChannelDeleteEventArgs>(EventErrorHandler, "CHANNEL_DELETED");
            _dmChannelDeleted = new AsyncEvent<DmChannelDeleteEventArgs>(EventErrorHandler, "DM_CHANNEL_DELETED");
            _channelPinsUpdated = new AsyncEvent<ChannelPinsUpdateEventArgs>(EventErrorHandler, "CHANNEL_PINS_UPDATED");
            _guildCreated = new AsyncEvent<GuildCreateEventArgs>(EventErrorHandler, "GUILD_CREATED");
            _guildAvailable = new AsyncEvent<GuildCreateEventArgs>(EventErrorHandler, "GUILD_AVAILABLE");
            _guildUpdated = new AsyncEvent<GuildUpdateEventArgs>(EventErrorHandler, "GUILD_UPDATED");
            _guildDeleted = new AsyncEvent<GuildDeleteEventArgs>(EventErrorHandler, "GUILD_DELETED");
            _guildUnavailable = new AsyncEvent<GuildDeleteEventArgs>(EventErrorHandler, "GUILD_UNAVAILABLE");
            _guildDownloadCompleted = new AsyncEvent<GuildDownloadCompletedEventArgs>(EventErrorHandler, "GUILD_DOWNLOAD_COMPLETED");
            _messageCreated = new AsyncEvent<MessageCreateEventArgs>(EventErrorHandler, "MESSAGE_CREATED");
            _presenceUpdated = new AsyncEvent<PresenceUpdateEventArgs>(EventErrorHandler, "PRESENCE_UPDATED");
            _guildBanAdded = new AsyncEvent<GuildBanAddEventArgs>(EventErrorHandler, "GUILD_BAN_ADDED");
            _guildBanRemoved = new AsyncEvent<GuildBanRemoveEventArgs>(EventErrorHandler, "GUILD_BAN_REMOVED");
            _guildEmojisUpdated = new AsyncEvent<GuildEmojisUpdateEventArgs>(EventErrorHandler, "GUILD_EMOJI_UPDATED");
            _guildIntegrationsUpdated = new AsyncEvent<GuildIntegrationsUpdateEventArgs>(EventErrorHandler, "GUILD_INTEGRATIONS_UPDATED");
            _guildMemberAdded = new AsyncEvent<GuildMemberAddEventArgs>(EventErrorHandler, "GUILD_MEMBER_ADDED");
            _guildMemberRemoved = new AsyncEvent<GuildMemberRemoveEventArgs>(EventErrorHandler, "GUILD_MEMBER_REMOVED");
            _guildMemberUpdated = new AsyncEvent<GuildMemberUpdateEventArgs>(EventErrorHandler, "GUILD_MEMBER_UPDATED");
            _guildRoleCreated = new AsyncEvent<GuildRoleCreateEventArgs>(EventErrorHandler, "GUILD_ROLE_CREATED");
            _guildRoleUpdated = new AsyncEvent<GuildRoleUpdateEventArgs>(EventErrorHandler, "GUILD_ROLE_UPDATED");
            _guildRoleDeleted = new AsyncEvent<GuildRoleDeleteEventArgs>(EventErrorHandler, "GUILD_ROLE_DELETED");
            _messageUpdated = new AsyncEvent<MessageUpdateEventArgs>(EventErrorHandler, "MESSAGE_UPDATED");
            _messageDeleted = new AsyncEvent<MessageDeleteEventArgs>(EventErrorHandler, "MESSAGE_DELETED");
            _messageBulkDeleted = new AsyncEvent<MessageBulkDeleteEventArgs>(EventErrorHandler, "MESSAGE_BULK_DELETED");
            _typingStarted = new AsyncEvent<TypingStartEventArgs>(EventErrorHandler, "TYPING_STARTED");
            _userSettingsUpdated = new AsyncEvent<UserSettingsUpdateEventArgs>(EventErrorHandler, "USER_SETTINGS_UPDATED");
            _userUpdated = new AsyncEvent<UserUpdateEventArgs>(EventErrorHandler, "USER_UPDATED");
            _voiceStateUpdated = new AsyncEvent<VoiceStateUpdateEventArgs>(EventErrorHandler, "VOICE_STATE_UPDATED");
            _voiceServerUpdated = new AsyncEvent<VoiceServerUpdateEventArgs>(EventErrorHandler, "VOICE_SERVER_UPDATED");
            _guildMembersChunk = new AsyncEvent<GuildMembersChunkEventArgs>(EventErrorHandler, "GUILD_MEMBERS_CHUNKED");
            _unknownEvent = new AsyncEvent<UnknownEventArgs>(EventErrorHandler, "UNKNOWN_EVENT");
            _messageReactionAdded = new AsyncEvent<MessageReactionAddEventArgs>(EventErrorHandler, "MESSAGE_REACTION_ADDED");
            _messageReactionRemoved = new AsyncEvent<MessageReactionRemoveEventArgs>(EventErrorHandler, "MESSAGE_REACTION_REMOVED");
            _messageReactionsCleared = new AsyncEvent<MessageReactionsClearEventArgs>(EventErrorHandler, "MESSAGE_REACTIONS_CLEARED");
            _webhooksUpdated = new AsyncEvent<WebhooksUpdateEventArgs>(EventErrorHandler, "WEBHOOKS_UPDATED");
            _heartbeated = new AsyncEvent<HeartbeatEventArgs>(EventErrorHandler, "HEARTBEATED");

            Config = config;
            Shards = new ConcurrentDictionary<int, DiscordClient>();
            DebugLogger = new DebugLogger(config.LogLevel, config.DateTimeFormat);

            if (config.UseInternalLogHandler)
            {
                DebugLogger.LogMessageReceived += (sender, e) => DebugLogger.LogHandler(sender, e);
            }
        }

        internal async Task<int> InitializeShardsAsync()
        {
            if (Shards.Count != 0)
            {
                return Shards.Count;
            }

            var shardc = Config.ShardCount == 1 ? await GetShardCountAsync().ConfigureAwait(false) : Config.ShardCount;
            for (var i = 0; i < shardc; i++)
            {
                var cfg = new DiscordConfiguration(Config)
                {
                    ShardId = i,
                    ShardCount = shardc,
                    UseInternalLogHandler = false
                };

                var client = new DiscordClient(cfg);
                if (!Shards.TryAdd(i, client))
                {
                    throw new Exception("Could not initialize shards.");
                }
            }

            return shardc;
        }

        /// <summary>
        /// Initializes and connects all shards.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            var shardc = await InitializeShardsAsync().ConfigureAwait(false);
            DebugLogger.LogMessage(LogLevel.Info, "Autoshard", $"Booting {shardc.ToString(CultureInfo.InvariantCulture)} shards", DateTime.Now);

            for (var i = 0; i < shardc; i++)
            {
                if (!Shards.TryGetValue(i, out var client))
                {
                    throw new Exception("Could not initialize shards");
                }

                if (CurrentUser != null)
                {
                    client.CurrentUser = CurrentUser;
                }

                if (_current_application != null)
                {
                    client.CurrentApplication = CurrentApplication;
                }

                if (InternalVoiceRegions != null)
                {
                    client.InternalVoiceRegions = InternalVoiceRegions;
                    client._voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(client.InternalVoiceRegions));
                }

                client.ClientErrored += Client_ClientError;
                client.SocketErrored += Client_SocketError;
                client.SocketOpened += Client_SocketOpened;
                client.SocketClosed += Client_SocketClosed;
                client.Ready += Client_Ready;
                client.Resumed += Client_Resumed;
                client.ChannelCreated += Client_ChannelCreated;
                client.DmChannelCreated += Client_DMChannelCreated;
                client.ChannelUpdated += Client_ChannelUpdated;
                client.ChannelDeleted += Client_ChannelDeleted;
                client.DmChannelDeleted += Client_DMChannelDeleted;
                client.ChannelPinsUpdated += Client_ChannelPinsUpdated;
                client.GuildCreated += Client_GuildCreated;
                client.GuildAvailable += Client_GuildAvailable;
                client.GuildUpdated += Client_GuildUpdated;
                client.GuildDeleted += Client_GuildDeleted;
                client.GuildUnavailable += Client_GuildUnavailable;
                client.GuildDownloadCompleted += Client_GuildDownloadCompleted;
                client.MessageCreated += Client_MessageCreated;
                client.PresenceUpdated += Client_PresenceUpdate;
                client.GuildBanAdded += Client_GuildBanAdd;
                client.GuildBanRemoved += Client_GuildBanRemove;
                client.GuildEmojisUpdated += Client_GuildEmojisUpdate;
                client.GuildIntegrationsUpdated += Client_GuildIntegrationsUpdate;
                client.GuildMemberAdded += Client_GuildMemberAdd;
                client.GuildMemberRemoved += Client_GuildMemberRemove;
                client.GuildMemberUpdated += Client_GuildMemberUpdate;
                client.GuildRoleCreated += Client_GuildRoleCreate;
                client.GuildRoleUpdated += Client_GuildRoleUpdate;
                client.GuildRoleDeleted += Client_GuildRoleDelete;
                client.MessageUpdated += Client_MessageUpdate;
                client.MessageDeleted += Client_MessageDelete;
                client.MessagesBulkDeleted += Client_MessageBulkDelete;
                client.TypingStarted += Client_TypingStart;
                client.UserSettingsUpdated += Client_UserSettingsUpdate;
                client.UserUpdated += Client_UserUpdate;
                client.VoiceStateUpdated += Client_VoiceStateUpdate;
                client.VoiceServerUpdated += Client_VoiceServerUpdate;
                client.GuildMembersChunked += Client_GuildMembersChunk;
                client.UnknownEvent += Client_UnknownEvent;
                client.MessageReactionAdded += Client_MessageReactionAdd;
                client.MessageReactionRemoved += Client_MessageReactionRemove;
                client.MessageReactionsCleared += Client_MessageReactionRemoveAll;
                client.WebhooksUpdated += Client_WebhooksUpdate;
                client.Heartbeated += Client_HeartBeated;
                client.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;
                
                await client.ConnectAsync().ConfigureAwait(false);
                DebugLogger.LogMessage(LogLevel.Info, "Autoshard", $"Booted shard {i.ToString(CultureInfo.InvariantCulture)}", DateTime.Now);

                if (_current_user == null)
                {
                    _current_user = client.CurrentUser;
                }

                if (_current_application == null)
                {
                    _current_application = client.CurrentApplication;
                }

                if (InternalVoiceRegions == null)
                {
                    InternalVoiceRegions = client.InternalVoiceRegions;
                    _voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(InternalVoiceRegions));
                }
            }
        }

        /// <summary>
        /// Updates playing statuses on all shards.
        /// </summary>
        /// <param name="activity">Activity to set.</param>
        /// <param name="userStatus">Status of the user.</param>
        /// <param name="idleSince">Since when is the client performing the specified activity.</param>
        /// <returns>Asynchronous operation.</returns>
        public async Task UpdateStatusAsync(DiscordActivity activity = null, UserStatus? userStatus = null, DateTimeOffset? idleSince = null)
        {
            var tasks = new List<Task>();
            foreach (var client in ShardClients.Values)
            {
                tasks.Add(client.UpdateStatusAsync(activity, userStatus, idleSince));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task<int> GetShardCountAsync()
        {
            string url = $"{Utilities.GetApiBaseUri()}{Endpoints.GATEWAY}{Endpoints.BOT}";
            var headers = Utilities.GetBaseHeaders();

            var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
            http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(Config));
            var resp = await http.GetAsync(url).ConfigureAwait(false);

            var jo = JObject.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
            if (jo["shards"] != null)
            {
                return jo.Value<int>("shards");
            }

            return 1;
        }

        #region Event Dispatchers
        private Task Client_ClientError(ClientErrorEventArgs e) 
            => _clientErrored.InvokeAsync(e);

        private Task Client_SocketError(SocketErrorEventArgs e) 
            => _socketErrored.InvokeAsync(e);

        private Task Client_SocketOpened() 
            => _socketOpened.InvokeAsync();

        private Task Client_SocketClosed(SocketCloseEventArgs e) 
            => _socketClosed.InvokeAsync(e);

        private Task Client_Ready(ReadyEventArgs e) 
            => _ready.InvokeAsync(e);

        private Task Client_Resumed(ReadyEventArgs e) 
            => _resumed.InvokeAsync(e);

        private Task Client_ChannelCreated(ChannelCreateEventArgs e) 
            => _channelCreated.InvokeAsync(e);

        private Task Client_DMChannelCreated(DmChannelCreateEventArgs e) 
            => _dmChannelCreated.InvokeAsync(e);

        private Task Client_ChannelUpdated(ChannelUpdateEventArgs e) 
            => _channelUpdated.InvokeAsync(e);

        private Task Client_ChannelDeleted(ChannelDeleteEventArgs e) 
            => _channelDeleted.InvokeAsync(e);

        private Task Client_DMChannelDeleted(DmChannelDeleteEventArgs e) 
            => _dmChannelDeleted.InvokeAsync(e);

        private Task Client_ChannelPinsUpdated(ChannelPinsUpdateEventArgs e) 
            => _channelPinsUpdated.InvokeAsync(e);

        private Task Client_GuildCreated(GuildCreateEventArgs e) 
            => _guildCreated.InvokeAsync(e);

        private Task Client_GuildAvailable(GuildCreateEventArgs e) 
            => _guildAvailable.InvokeAsync(e);

        private Task Client_GuildUpdated(GuildUpdateEventArgs e) 
            => _guildUpdated.InvokeAsync(e);

        private Task Client_GuildDeleted(GuildDeleteEventArgs e) 
            => _guildDeleted.InvokeAsync(e);

        private Task Client_GuildUnavailable(GuildDeleteEventArgs e) 
            => _guildUnavailable.InvokeAsync(e);

        private Task Client_GuildDownloadCompleted(GuildDownloadCompletedEventArgs e)
            => _guildDownloadCompleted.InvokeAsync(e);

        private Task Client_MessageCreated(MessageCreateEventArgs e) 
            => _messageCreated.InvokeAsync(e);

        private Task Client_PresenceUpdate(PresenceUpdateEventArgs e) 
            => _presenceUpdated.InvokeAsync(e);

        private Task Client_GuildBanAdd(GuildBanAddEventArgs e) 
            => _guildBanAdded.InvokeAsync(e);

        private Task Client_GuildBanRemove(GuildBanRemoveEventArgs e) 
            => _guildBanRemoved.InvokeAsync(e);

        private Task Client_GuildEmojisUpdate(GuildEmojisUpdateEventArgs e) 
            => _guildEmojisUpdated.InvokeAsync(e);

        private Task Client_GuildIntegrationsUpdate(GuildIntegrationsUpdateEventArgs e) 
            => _guildIntegrationsUpdated.InvokeAsync(e);

        private Task Client_GuildMemberAdd(GuildMemberAddEventArgs e) 
            => _guildMemberAdded.InvokeAsync(e);

        private Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e) 
            => _guildMemberRemoved.InvokeAsync(e);

        private Task Client_GuildMemberUpdate(GuildMemberUpdateEventArgs e) 
            => _guildMemberUpdated.InvokeAsync(e);

        private Task Client_GuildRoleCreate(GuildRoleCreateEventArgs e) 
            => _guildRoleCreated.InvokeAsync(e);

        private Task Client_GuildRoleUpdate(GuildRoleUpdateEventArgs e) 
            => _guildRoleUpdated.InvokeAsync(e);

        private Task Client_GuildRoleDelete(GuildRoleDeleteEventArgs e) 
            => _guildRoleDeleted.InvokeAsync(e);

        private Task Client_MessageUpdate(MessageUpdateEventArgs e) 
            => _messageUpdated.InvokeAsync(e);

        private Task Client_MessageDelete(MessageDeleteEventArgs e) 
            => _messageDeleted.InvokeAsync(e);

        private Task Client_MessageBulkDelete(MessageBulkDeleteEventArgs e) 
            => _messageBulkDeleted.InvokeAsync(e);

        private Task Client_TypingStart(TypingStartEventArgs e) 
            => _typingStarted.InvokeAsync(e);

        private Task Client_UserSettingsUpdate(UserSettingsUpdateEventArgs e) 
            => _userSettingsUpdated.InvokeAsync(e);

        private Task Client_UserUpdate(UserUpdateEventArgs e) 
            => _userUpdated.InvokeAsync(e);

        private Task Client_VoiceStateUpdate(VoiceStateUpdateEventArgs e) 
            => _voiceStateUpdated.InvokeAsync(e);

        private Task Client_VoiceServerUpdate(VoiceServerUpdateEventArgs e) 
            => _voiceServerUpdated.InvokeAsync(e);

        private Task Client_GuildMembersChunk(GuildMembersChunkEventArgs e) 
            => _guildMembersChunk.InvokeAsync(e);

        private Task Client_UnknownEvent(UnknownEventArgs e) 
            => _unknownEvent.InvokeAsync(e);

        private Task Client_MessageReactionAdd(MessageReactionAddEventArgs e) 
            => _messageReactionAdded.InvokeAsync(e);

        private Task Client_MessageReactionRemove(MessageReactionRemoveEventArgs e) 
            => _messageReactionRemoved.InvokeAsync(e);

        private Task Client_MessageReactionRemoveAll(MessageReactionsClearEventArgs e) 
            => _messageReactionsCleared.InvokeAsync(e);

        private Task Client_WebhooksUpdate(WebhooksUpdateEventArgs e) 
            => _webhooksUpdated.InvokeAsync(e);

        private Task Client_HeartBeated(HeartbeatEventArgs e) 
            => _heartbeated.InvokeAsync(e);

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e) 
            => DebugLogger.LogMessage(e.Level, e.Application, e.Message, e.Timestamp);
        #endregion
    }
}
