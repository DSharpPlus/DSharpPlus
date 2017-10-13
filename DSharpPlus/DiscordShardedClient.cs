using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.Net.Udp;
using DSharpPlus.Net;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Globalization;
using DSharpPlus.Enums;

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
            add => _client_error.Register(value);
            remove => _client_error.Unregister(value);
        }
        private AsyncEvent<ClientErrorEventArgs> _client_error;

        /// <summary>
        /// Fired whenever a WebSocket error occurs within the client.
        /// </summary>
        public event AsyncEventHandler<SocketErrorEventArgs> SocketErrored
        {
            add => _socket_error.Register(value);
            remove => _socket_error.Unregister(value);
        }
        private AsyncEvent<SocketErrorEventArgs> _socket_error;

        /// <summary>
        /// Fired whenever WebSocket connection is established.
        /// </summary>
        public event AsyncEventHandler SocketOpened
        {
            add => _socket_opened.Register(value);
            remove => _socket_opened.Unregister(value);
        }
        private AsyncEvent _socket_opened;

        /// <summary>
        /// Fired whenever WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<SocketCloseEventArgs> SocketClosed
        {
            add => _socket_closed.Register(value);
            remove => _socket_closed.Unregister(value);
        }
        private AsyncEvent<SocketCloseEventArgs> _socket_closed;

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
            add => _channel_created.Register(value);
            remove => _channel_created.Unregister(value);
        }
        private AsyncEvent<ChannelCreateEventArgs> _channel_created;

        /// <summary>
        /// Fired when a new direct message channel is created.
        /// </summary>
        public event AsyncEventHandler<DmChannelCreateEventArgs> DmChannelCreated
        {
            add => _dm_channel_created.Register(value);
            remove => _dm_channel_created.Unregister(value);
        }
        private AsyncEvent<DmChannelCreateEventArgs> _dm_channel_created;

        /// <summary>
        /// Fired when a channel is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelUpdateEventArgs> ChannelUpdated
        {
            add => _channel_updated.Register(value);
            remove => _channel_updated.Unregister(value);
        }
        private AsyncEvent<ChannelUpdateEventArgs> _channel_updated;

        /// <summary>
        /// Fired when a channel is deleted
        /// </summary>
        public event AsyncEventHandler<ChannelDeleteEventArgs> ChannelDeleted
        {
            add => _channel_deleted.Register(value);
            remove => _channel_deleted.Unregister(value);
        }
        private AsyncEvent<ChannelDeleteEventArgs> _channel_deleted;

        /// <summary>
        /// Fired when a dm channel is deleted
        /// </summary>
        public event AsyncEventHandler<DmChannelDeleteEventArgs> DmChannelDeleted
        {
            add => _dm_channel_deleted.Register(value);
            remove => _dm_channel_deleted.Unregister(value);
        }
        private AsyncEvent<DmChannelDeleteEventArgs> _dm_channel_deleted;

        /// <summary>
        /// Fired whenever a channel's pinned message list is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelPinsUpdateEventArgs> ChannelPinsUpdated
        {
            add => _channel_pins_updated.Register(value);
            remove => _channel_pins_updated.Unregister(value);
        }
        private AsyncEvent<ChannelPinsUpdateEventArgs> _channel_pins_updated;

        /// <summary>
        /// Fired when the user joins a new guild.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildCreated
        {
            add => _guild_created.Register(value);
            remove => _guild_created.Unregister(value);
        }
        private AsyncEvent<GuildCreateEventArgs> _guild_created;

        /// <summary>
        /// Fired when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildAvailable
        {
            add => _guild_available.Register(value);
            remove => _guild_available.Unregister(value);
        }
        private AsyncEvent<GuildCreateEventArgs> _guild_available;

        /// <summary>
        /// Fired when a guild is updated.
        /// </summary>
        public event AsyncEventHandler<GuildUpdateEventArgs> GuildUpdated
        {
            add => _guild_updated.Register(value);
            remove => _guild_updated.Unregister(value);
        }
        private AsyncEvent<GuildUpdateEventArgs> _guild_updated;

        /// <summary>
        /// Fired when the user leaves or is removed from a guild.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildDeleted
        {
            add => _guild_deleted.Register(value);
            remove => _guild_deleted.Unregister(value);
        }
        private AsyncEvent<GuildDeleteEventArgs> _guild_deleted;

        /// <summary>
        /// Fired when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildUnavailable
        {
            add => _guild_unavailable.Register(value);
            remove => _guild_unavailable.Unregister(value);
        }
        private AsyncEvent<GuildDeleteEventArgs> _guild_unavailable;

        /// <summary>
        /// Fired when a message is created.
        /// </summary>
        public event AsyncEventHandler<MessageCreateEventArgs> MessageCreated
        {
            add => _message_created.Register(value);
            remove => _message_created.Unregister(value);
        }
        private AsyncEvent<MessageCreateEventArgs> _message_created;

        /// <summary>
        /// Fired when a presence has been updated.
        /// </summary>
        public event AsyncEventHandler<PresenceUpdateEventArgs> PresenceUpdated
        {
            add => _presence_update.Register(value);
            remove => _presence_update.Unregister(value);
        }
        private AsyncEvent<PresenceUpdateEventArgs> _presence_update;

        /// <summary>
        /// Fired when a guild ban gets added
        /// </summary>
        public event AsyncEventHandler<GuildBanAddEventArgs> GuildBanAdded
        {
            add => _guild_ban_add.Register(value);
            remove => _guild_ban_add.Unregister(value);
        }
        private AsyncEvent<GuildBanAddEventArgs> _guild_ban_add;

        /// <summary>
        /// Fired when a guild ban gets removed
        /// </summary>
        public event AsyncEventHandler<GuildBanRemoveEventArgs> GuildBanRemoved
        {
            add => _guild_ban_remove.Register(value);
            remove => _guild_ban_remove.Unregister(value);
        }
        private AsyncEvent<GuildBanRemoveEventArgs> _guild_ban_remove;

        /// <summary>
        /// Fired when a guilds emojis get updated
        /// </summary>
        public event AsyncEventHandler<GuildEmojisUpdateEventArgs> GuildEmojisUpdated
        {
            add => _guild_emojis_update.Register(value);
            remove => _guild_emojis_update.Unregister(value);
        }
        private AsyncEvent<GuildEmojisUpdateEventArgs> _guild_emojis_update;

        /// <summary>
        /// Fired when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
        {
            add => _guild_integrations_update.Register(value);
            remove => _guild_integrations_update.Unregister(value);
        }
        private AsyncEvent<GuildIntegrationsUpdateEventArgs> _guild_integrations_update;

        /// <summary>
        /// Fired when a new user joins a guild.
        /// </summary>
        public event AsyncEventHandler<GuildMemberAddEventArgs> GuildMemberAdded
        {
            add => _guild_member_add.Register(value);
            remove => _guild_member_add.Unregister(value);
        }
        private AsyncEvent<GuildMemberAddEventArgs> _guild_member_add;

        /// <summary>
        /// Fired when a user is removed from a guild (leave/kick/ban).
        /// </summary>
        public event AsyncEventHandler<GuildMemberRemoveEventArgs> GuildMemberRemoved
        {
            add => _guild_member_remove.Register(value);
            remove => _guild_member_remove.Unregister(value);
        }
        private AsyncEvent<GuildMemberRemoveEventArgs> _guild_member_remove;

        /// <summary>
        /// Fired when a guild member is updated.
        /// </summary>
        public event AsyncEventHandler<GuildMemberUpdateEventArgs> GuildMemberUpdated
        {
            add => _guild_member_update.Register(value);
            remove => _guild_member_update.Unregister(value);
        }
        private AsyncEvent<GuildMemberUpdateEventArgs> _guild_member_update;

        /// <summary>
        /// Fired when a guild role is created.
        /// </summary>
        public event AsyncEventHandler<GuildRoleCreateEventArgs> GuildRoleCreated
        {
            add => _guild_role_create.Register(value);
            remove => _guild_role_create.Unregister(value);
        }
        private AsyncEvent<GuildRoleCreateEventArgs> _guild_role_create;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleUpdateEventArgs> GuildRoleUpdated
        {
            add => _guild_role_update.Register(value);
            remove => _guild_role_update.Unregister(value);
        }
        private AsyncEvent<GuildRoleUpdateEventArgs> _guild_role_update;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleDeleteEventArgs> GuildRoleDeleted
        {
            add => _guild_role_delete.Register(value);
            remove => _guild_role_delete.Unregister(value);
        }
        private AsyncEvent<GuildRoleDeleteEventArgs> _guild_role_delete;

        /// <summary>
        /// Fired when a message is updated.
        /// </summary>
        public event AsyncEventHandler<MessageUpdateEventArgs> MessageUpdated
        {
            add => _message_update.Register(value);
            remove => _message_update.Unregister(value);
        }
        private AsyncEvent<MessageUpdateEventArgs> _message_update;

        /// <summary>
        /// Fired when a message is deleted.
        /// </summary>
        public event AsyncEventHandler<MessageDeleteEventArgs> MessageDeleted
        {
            add => _message_delete.Register(value);
            remove => _message_delete.Unregister(value);
        }
        private AsyncEvent<MessageDeleteEventArgs> _message_delete;

        /// <summary>
        /// Fired when multiple messages are deleted at once.
        /// </summary>
        public event AsyncEventHandler<MessageBulkDeleteEventArgs> MessagesBulkDeleted
        {
            add => _message_bulk_delete.Register(value);
            remove => _message_bulk_delete.Unregister(value);
        }
        private AsyncEvent<MessageBulkDeleteEventArgs> _message_bulk_delete;

        /// <summary>
        /// Fired when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<TypingStartEventArgs> TypingStarted
        {
            add => _typing_start.Register(value);
            remove => _typing_start.Unregister(value);
        }
        private AsyncEvent<TypingStartEventArgs> _typing_start;

        /// <summary>
        /// Fired when the current user updates their settings.
        /// </summary>
        public event AsyncEventHandler<UserSettingsUpdateEventArgs> UserSettingsUpdated
        {
            add => _user_settings_update.Register(value);
            remove => _user_settings_update.Unregister(value);
        }
        private AsyncEvent<UserSettingsUpdateEventArgs> _user_settings_update;

        /// <summary>
        /// Fired when properties about the user change.
        /// </summary>
        public event AsyncEventHandler<UserUpdateEventArgs> UserUpdated
        {
            add => _user_update.Register(value);
            remove => _user_update.Unregister(value);
        }
        private AsyncEvent<UserUpdateEventArgs> _user_update;

        /// <summary>
        /// Fired when someone joins/leaves/moves voice channels.
        /// </summary>
        public event AsyncEventHandler<VoiceStateUpdateEventArgs> VoiceStateUpdated
        {
            add => _voice_state_update.Register(value);
            remove => _voice_state_update.Unregister(value);
        }
        private AsyncEvent<VoiceStateUpdateEventArgs> _voice_state_update;

        /// <summary>
        /// Fired when a guild's voice server is updated.
        /// </summary>
        public event AsyncEventHandler<VoiceServerUpdateEventArgs> VoiceServerUpdated
        {
            add => _voice_server_update.Register(value);
            remove => _voice_server_update.Unregister(value);
        }
        private AsyncEvent<VoiceServerUpdateEventArgs> _voice_server_update;

        /// <summary>
        /// Fired in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<GuildMembersChunkEventArgs> GuildMembersChunked
        {
            add => _guild_members_chunk.Register(value);
            remove => _guild_members_chunk.Unregister(value);
        }
        private AsyncEvent<GuildMembersChunkEventArgs> _guild_members_chunk;

        /// <summary>
        /// Fired when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<UnknownEventArgs> UnknownEvent
        {
            add => _unknown_event.Register(value);
            remove => _unknown_event.Unregister(value);
        }
        private AsyncEvent<UnknownEventArgs> _unknown_event;

        /// <summary>
        /// Fired when a reaction gets added to a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionAddEventArgs> MessageReactionAdded
        {
            add => _message_reaction_add.Register(value);
            remove => _message_reaction_add.Unregister(value);
        }
        private AsyncEvent<MessageReactionAddEventArgs> _message_reaction_add;

        /// <summary>
        /// Fired when a reaction gets removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEventArgs> MessageReactionRemoved
        {
            add => _message_reaction_remove.Register(value);
            remove => _message_reaction_remove.Unregister(value);
        }
        private AsyncEvent<MessageReactionRemoveEventArgs> _message_reaction_remove;

        /// <summary>
        /// Fired when all reactions get removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionsClearEventArgs> MessageReactionsCleared
        {
            add => _message_reaction_remove_all.Register(value);
            remove => _message_reaction_remove_all.Unregister(value);
        }
        private AsyncEvent<MessageReactionsClearEventArgs> _message_reaction_remove_all;

        /// <summary>
        /// Fired whenever webhooks update.
        /// </summary>
        public event AsyncEventHandler<WebhooksUpdateEventArgs> WebhooksUpdated
        {
            add => _webhooks_update.Register(value);
            remove => _webhooks_update.Unregister(value);
        }
        private AsyncEvent<WebhooksUpdateEventArgs> _webhooks_update;

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
            _client_error.InvokeAsync(new ClientErrorEventArgs(null) { EventName = evname, Exception = ex }).GetAwaiter().GetResult();
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
        public IReadOnlyDictionary<int, DiscordClient> ShardClients => new ReadOnlyDictionary<int, DiscordClient>(Shards);

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public DiscordUser CurrentUser => _currentUser;
        private DiscordUser _currentUser;

        /// <summary>
        /// Gets the current application.
        /// </summary>
        public DiscordApplication CurrentApplication => _currentApplication;
        private DiscordApplication _currentApplication;

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

            _client_error = new AsyncEvent<ClientErrorEventArgs>(Goof, "CLIENT_ERRORED");
            _socket_error = new AsyncEvent<SocketErrorEventArgs>(Goof, "SOCKET_ERRORED");
            _socket_opened = new AsyncEvent(EventErrorHandler, "SOCKET_OPENED");
            _socket_closed = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "SOCKET_CLOSED");
            _ready = new AsyncEvent<ReadyEventArgs>(EventErrorHandler, "READY");
            _resumed = new AsyncEvent<ReadyEventArgs>(EventErrorHandler, "RESUMED");
            _channel_created = new AsyncEvent<ChannelCreateEventArgs>(EventErrorHandler, "CHANNEL_CREATED");
            _dm_channel_created = new AsyncEvent<DmChannelCreateEventArgs>(EventErrorHandler, "DM_CHANNEL_CREATED");
            _channel_updated = new AsyncEvent<ChannelUpdateEventArgs>(EventErrorHandler, "CHANNEL_UPDATED");
            _channel_deleted = new AsyncEvent<ChannelDeleteEventArgs>(EventErrorHandler, "CHANNEL_DELETED");
            _dm_channel_deleted = new AsyncEvent<DmChannelDeleteEventArgs>(EventErrorHandler, "DM_CHANNEL_DELETED");
            _channel_pins_updated = new AsyncEvent<ChannelPinsUpdateEventArgs>(EventErrorHandler, "CHANNEL_PINS_UPDATED");
            _guild_created = new AsyncEvent<GuildCreateEventArgs>(EventErrorHandler, "GUILD_CREATED");
            _guild_available = new AsyncEvent<GuildCreateEventArgs>(EventErrorHandler, "GUILD_AVAILABLE");
            _guild_updated = new AsyncEvent<GuildUpdateEventArgs>(EventErrorHandler, "GUILD_UPDATED");
            _guild_deleted = new AsyncEvent<GuildDeleteEventArgs>(EventErrorHandler, "GUILD_DELETED");
            _guild_unavailable = new AsyncEvent<GuildDeleteEventArgs>(EventErrorHandler, "GUILD_UNAVAILABLE");
            _message_created = new AsyncEvent<MessageCreateEventArgs>(EventErrorHandler, "MESSAGE_CREATED");
            _presence_update = new AsyncEvent<PresenceUpdateEventArgs>(EventErrorHandler, "PRESENCE_UPDATED");
            _guild_ban_add = new AsyncEvent<GuildBanAddEventArgs>(EventErrorHandler, "GUILD_BAN_ADDED");
            _guild_ban_remove = new AsyncEvent<GuildBanRemoveEventArgs>(EventErrorHandler, "GUILD_BAN_REMOVED");
            _guild_emojis_update = new AsyncEvent<GuildEmojisUpdateEventArgs>(EventErrorHandler, "GUILD_EMOJI_UPDATED");
            _guild_integrations_update = new AsyncEvent<GuildIntegrationsUpdateEventArgs>(EventErrorHandler, "GUILD_INTEGRATIONS_UPDATED");
            _guild_member_add = new AsyncEvent<GuildMemberAddEventArgs>(EventErrorHandler, "GUILD_MEMBER_ADDED");
            _guild_member_remove = new AsyncEvent<GuildMemberRemoveEventArgs>(EventErrorHandler, "GUILD_MEMBER_REMOVED");
            _guild_member_update = new AsyncEvent<GuildMemberUpdateEventArgs>(EventErrorHandler, "GUILD_MEMBER_UPDATED");
            _guild_role_create = new AsyncEvent<GuildRoleCreateEventArgs>(EventErrorHandler, "GUILD_ROLE_CREATED");
            _guild_role_update = new AsyncEvent<GuildRoleUpdateEventArgs>(EventErrorHandler, "GUILD_ROLE_UPDATED");
            _guild_role_delete = new AsyncEvent<GuildRoleDeleteEventArgs>(EventErrorHandler, "GUILD_ROLE_DELETED");
            _message_update = new AsyncEvent<MessageUpdateEventArgs>(EventErrorHandler, "MESSAGE_UPDATED");
            _message_delete = new AsyncEvent<MessageDeleteEventArgs>(EventErrorHandler, "MESSAGE_DELETED");
            _message_bulk_delete = new AsyncEvent<MessageBulkDeleteEventArgs>(EventErrorHandler, "MESSAGE_BULK_DELETED");
            _typing_start = new AsyncEvent<TypingStartEventArgs>(EventErrorHandler, "TYPING_STARTED");
            _user_settings_update = new AsyncEvent<UserSettingsUpdateEventArgs>(EventErrorHandler, "USER_SETTINGS_UPDATED");
            _user_update = new AsyncEvent<UserUpdateEventArgs>(EventErrorHandler, "USER_UPDATED");
            _voice_state_update = new AsyncEvent<VoiceStateUpdateEventArgs>(EventErrorHandler, "VOICE_STATE_UPDATED");
            _voice_server_update = new AsyncEvent<VoiceServerUpdateEventArgs>(EventErrorHandler, "VOICE_SERVER_UPDATED");
            _guild_members_chunk = new AsyncEvent<GuildMembersChunkEventArgs>(EventErrorHandler, "GUILD_MEMBERS_CHUNKED");
            _unknown_event = new AsyncEvent<UnknownEventArgs>(EventErrorHandler, "UNKNOWN_EVENT");
            _message_reaction_add = new AsyncEvent<MessageReactionAddEventArgs>(EventErrorHandler, "MESSAGE_REACTION_ADDED");
            _message_reaction_remove = new AsyncEvent<MessageReactionRemoveEventArgs>(EventErrorHandler, "MESSAGE_REACTION_REMOVED");
            _message_reaction_remove_all = new AsyncEvent<MessageReactionsClearEventArgs>(EventErrorHandler, "MESSAGE_REACTIONS_CLEARED");
            _webhooks_update = new AsyncEvent<WebhooksUpdateEventArgs>(EventErrorHandler, "WEBHOOKS_UPDATED");
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

            var shardc = Config.ShardCount == 1 ? await GetShardCountAsync() : Config.ShardCount;
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
            var shardc = await InitializeShardsAsync();
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

                if (_currentApplication != null)
                {
                    client.CurrentApplication = CurrentApplication;
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
                
                await client.ConnectAsync();
                DebugLogger.LogMessage(LogLevel.Info, "Autoshard", $"Booted shard {i.ToString(CultureInfo.InvariantCulture)}", DateTime.Now);

                if (_currentUser == null)
                {
                    _currentUser = client.CurrentUser;
                }

                if (_currentApplication == null)
                {
                    _currentApplication = client.CurrentApplication;
                }
            }
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

        /// <summary>
        /// Updates playing statuses on all shards.
        /// </summary>
        /// <param name="game">Game to set.</param>
        /// <param name="userStatus">Status of the user.</param>
        /// <param name="idleSince">Since when is the client idle.</param>
        /// <returns>Asynchronous operation.</returns>
        public async Task UpdateStatusAsync(DiscordGame game = null, UserStatus? userStatus = null, DateTimeOffset? idleSince = null)
        {
            var tasks = new List<Task>();
            foreach (var client in ShardClients.Values)
            {
                tasks.Add(client.UpdateStatusAsync(game, userStatus, idleSince));
            }

            await Task.WhenAll(tasks);
        }

        private async Task<int> GetShardCountAsync()
        {
            string url = $"{Utilities.GetApiBaseUri()}{Endpoints.Gateway}{Endpoints.Bot}";

            var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
            http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(Config));
            var resp = await http.GetAsync(url);

            var jo = JObject.Parse(await resp.Content.ReadAsStringAsync());
            if (jo["shards"] != null)
            {
                return jo.Value<int>("shards");
            }

            return 1;
        }

        #region Event Dispatchers
        private Task Client_ClientError(ClientErrorEventArgs e) =>
            _client_error.InvokeAsync(e);

        private Task Client_SocketError(SocketErrorEventArgs e) =>
            _socket_error.InvokeAsync(e);

        private Task Client_SocketOpened() =>
            _socket_opened.InvokeAsync();

        private Task Client_SocketClosed(SocketCloseEventArgs e) =>
            _socket_closed.InvokeAsync(e);

        private Task Client_Ready(ReadyEventArgs e) =>
            _ready.InvokeAsync(e);

        private Task Client_Resumed(ReadyEventArgs e) =>
            _resumed.InvokeAsync(e);

        private Task Client_ChannelCreated(ChannelCreateEventArgs e) =>
            _channel_created.InvokeAsync(e);

        private Task Client_DMChannelCreated(DmChannelCreateEventArgs e) =>
            _dm_channel_created.InvokeAsync(e);

        private Task Client_ChannelUpdated(ChannelUpdateEventArgs e) =>
            _channel_updated.InvokeAsync(e);

        private Task Client_ChannelDeleted(ChannelDeleteEventArgs e) =>
            _channel_deleted.InvokeAsync(e);

        private Task Client_DMChannelDeleted(DmChannelDeleteEventArgs e) =>
            _dm_channel_deleted.InvokeAsync(e);

        private Task Client_ChannelPinsUpdated(ChannelPinsUpdateEventArgs e) =>
            _channel_pins_updated.InvokeAsync(e);

        private Task Client_GuildCreated(GuildCreateEventArgs e) =>
            _guild_created.InvokeAsync(e);

        private Task Client_GuildAvailable(GuildCreateEventArgs e) =>
            _guild_available.InvokeAsync(e);

        private Task Client_GuildUpdated(GuildUpdateEventArgs e) =>
            _guild_updated.InvokeAsync(e);

        private Task Client_GuildDeleted(GuildDeleteEventArgs e) =>
            _guild_deleted.InvokeAsync(e);

        private Task Client_GuildUnavailable(GuildDeleteEventArgs e) =>
            _guild_unavailable.InvokeAsync(e);

        private Task Client_MessageCreated(MessageCreateEventArgs e) =>
            _message_created.InvokeAsync(e);

        private Task Client_PresenceUpdate(PresenceUpdateEventArgs e) =>
            _presence_update.InvokeAsync(e);

        private Task Client_GuildBanAdd(GuildBanAddEventArgs e) =>
            _guild_ban_add.InvokeAsync(e);

        private Task Client_GuildBanRemove(GuildBanRemoveEventArgs e) =>
            _guild_ban_remove.InvokeAsync(e);

        private Task Client_GuildEmojisUpdate(GuildEmojisUpdateEventArgs e) =>
            _guild_emojis_update.InvokeAsync(e);

        private Task Client_GuildIntegrationsUpdate(GuildIntegrationsUpdateEventArgs e) =>
            _guild_integrations_update.InvokeAsync(e);

        private Task Client_GuildMemberAdd(GuildMemberAddEventArgs e) =>
            _guild_member_add.InvokeAsync(e);

        private Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e) =>
            _guild_member_remove.InvokeAsync(e);

        private Task Client_GuildMemberUpdate(GuildMemberUpdateEventArgs e) =>
            _guild_member_update.InvokeAsync(e);

        private Task Client_GuildRoleCreate(GuildRoleCreateEventArgs e) =>
            _guild_role_create.InvokeAsync(e);

        private Task Client_GuildRoleUpdate(GuildRoleUpdateEventArgs e) =>
            _guild_role_update.InvokeAsync(e);

        private Task Client_GuildRoleDelete(GuildRoleDeleteEventArgs e) =>
            _guild_role_delete.InvokeAsync(e);

        private Task Client_MessageUpdate(MessageUpdateEventArgs e) =>
            _message_update.InvokeAsync(e);

        private Task Client_MessageDelete(MessageDeleteEventArgs e) =>
            _message_delete.InvokeAsync(e);

        private Task Client_MessageBulkDelete(MessageBulkDeleteEventArgs e) =>
            _message_bulk_delete.InvokeAsync(e);

        private Task Client_TypingStart(TypingStartEventArgs e) =>
            _typing_start.InvokeAsync(e);

        private Task Client_UserSettingsUpdate(UserSettingsUpdateEventArgs e) =>
            _user_settings_update.InvokeAsync(e);

        private Task Client_UserUpdate(UserUpdateEventArgs e) =>
            _user_update.InvokeAsync(e);

        private Task Client_VoiceStateUpdate(VoiceStateUpdateEventArgs e) =>
            _voice_state_update.InvokeAsync(e);

        private Task Client_VoiceServerUpdate(VoiceServerUpdateEventArgs e) =>
            _voice_server_update.InvokeAsync(e);

        private Task Client_GuildMembersChunk(GuildMembersChunkEventArgs e) =>
            _guild_members_chunk.InvokeAsync(e);

        private Task Client_UnknownEvent(UnknownEventArgs e) =>
            _unknown_event.InvokeAsync(e);

        private Task Client_MessageReactionAdd(MessageReactionAddEventArgs e) =>
            _message_reaction_add.InvokeAsync(e);

        private Task Client_MessageReactionRemove(MessageReactionRemoveEventArgs e) =>
            _message_reaction_remove.InvokeAsync(e);

        private Task Client_MessageReactionRemoveAll(MessageReactionsClearEventArgs e) =>
            _message_reaction_remove_all.InvokeAsync(e);

        private Task Client_WebhooksUpdate(WebhooksUpdateEventArgs e) =>
            _webhooks_update.InvokeAsync(e);

        private Task Client_HeartBeated(HeartbeatEventArgs e) =>
            _heartbeated.InvokeAsync(e);

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e) =>
            DebugLogger.LogMessage(e.Level, e.Application, e.Message, e.Timestamp);
        #endregion
    }
}
