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
            add { this._client_error.Register(value); }
            remove { this._client_error.Unregister(value); }
        }
        private AsyncEvent<ClientErrorEventArgs> _client_error;

        /// <summary>
        /// Fired whenever a WebSocket error occurs within the client.
        /// </summary>
        public event AsyncEventHandler<SocketErrorEventArgs> SocketErrored
        {
            add { this._socket_error.Register(value); }
            remove { this._socket_error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _socket_error;

        /// <summary>
        /// Fired whenever WebSocket connection is established.
        /// </summary>
        public event AsyncEventHandler SocketOpened
        {
            add { this._socket_opened.Register(value); }
            remove { this._socket_opened.Unregister(value); }
        }
        private AsyncEvent _socket_opened;

        /// <summary>
        /// Fired whenever WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<SocketCloseEventArgs> SocketClosed
        {
            add { this._socket_closed.Register(value); }
            remove { this._socket_closed.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _socket_closed;

        /// <summary>
        /// Fired when the client enters ready state.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Ready
        {
            add { this._ready.Register(value); }
            remove { this._ready.Unregister(value); }
        }
        private AsyncEvent<ReadyEventArgs> _ready;

        /// <summary>
        /// Fired whenever a session is resumed.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Resumed
        {
            add { this._resumed.Register(value); }
            remove { this._resumed.Unregister(value); }
        }
        private AsyncEvent<ReadyEventArgs> _resumed;

        /// <summary>
        /// Fired when a new channel is created.
        /// </summary>
        public event AsyncEventHandler<ChannelCreateEventArgs> ChannelCreated
        {
            add { this._channel_created.Register(value); }
            remove { this._channel_created.Unregister(value); }
        }
        private AsyncEvent<ChannelCreateEventArgs> _channel_created;

        /// <summary>
        /// Fired when a new direct message channel is created.
        /// </summary>
        public event AsyncEventHandler<DmChannelCreateEventArgs> DmChannelCreated
        {
            add { this._dm_channel_created.Register(value); }
            remove { this._dm_channel_created.Unregister(value); }
        }
        private AsyncEvent<DmChannelCreateEventArgs> _dm_channel_created;

        /// <summary>
        /// Fired when a channel is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelUpdateEventArgs> ChannelUpdated
        {
            add { this._channel_updated.Register(value); }
            remove { this._channel_updated.Unregister(value); }
        }
        private AsyncEvent<ChannelUpdateEventArgs> _channel_updated;

        /// <summary>
        /// Fired when a channel is deleted
        /// </summary>
        public event AsyncEventHandler<ChannelDeleteEventArgs> ChannelDeleted
        {
            add { this._channel_deleted.Register(value); }
            remove { this._channel_deleted.Unregister(value); }
        }
        private AsyncEvent<ChannelDeleteEventArgs> _channel_deleted;

        /// <summary>
        /// Fired when a dm channel is deleted
        /// </summary>
        public event AsyncEventHandler<DmChannelDeleteEventArgs> DmChannelDeleted
        {
            add { this._dm_channel_deleted.Register(value); }
            remove { this._dm_channel_deleted.Unregister(value); }
        }
        private AsyncEvent<DmChannelDeleteEventArgs> _dm_channel_deleted;

        /// <summary>
        /// Fired whenever a channel's pinned message list is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelPinsUpdateEventArgs> ChannelPinsUpdated
        {
            add { this._channel_pins_updated.Register(value); }
            remove { this._channel_pins_updated.Unregister(value); }
        }
        private AsyncEvent<ChannelPinsUpdateEventArgs> _channel_pins_updated;

        /// <summary>
        /// Fired when the user joins a new guild.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildCreated
        {
            add { this._guild_created.Register(value); }
            remove { this._guild_created.Unregister(value); }
        }
        private AsyncEvent<GuildCreateEventArgs> _guild_created;

        /// <summary>
        /// Fired when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildAvailable
        {
            add { this._guild_available.Register(value); }
            remove { this._guild_available.Unregister(value); }
        }
        private AsyncEvent<GuildCreateEventArgs> _guild_available;

        /// <summary>
        /// Fired when a guild is updated.
        /// </summary>
        public event AsyncEventHandler<GuildUpdateEventArgs> GuildUpdated
        {
            add { this._guild_updated.Register(value); }
            remove { this._guild_updated.Unregister(value); }
        }
        private AsyncEvent<GuildUpdateEventArgs> _guild_updated;

        /// <summary>
        /// Fired when the user leaves or is removed from a guild.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildDeleted
        {
            add { this._guild_deleted.Register(value); }
            remove { this._guild_deleted.Unregister(value); }
        }
        private AsyncEvent<GuildDeleteEventArgs> _guild_deleted;

        /// <summary>
        /// Fired when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildUnavailable
        {
            add { this._guild_unavailable.Register(value); }
            remove { this._guild_unavailable.Unregister(value); }
        }
        private AsyncEvent<GuildDeleteEventArgs> _guild_unavailable;

        /// <summary>
        /// Fired when a message is created.
        /// </summary>
        public event AsyncEventHandler<MessageCreateEventArgs> MessageCreated
        {
            add { this._message_created.Register(value); }
            remove { this._message_created.Unregister(value); }
        }
        private AsyncEvent<MessageCreateEventArgs> _message_created;

        /// <summary>
        /// Fired when a presence has been updated.
        /// </summary>
        public event AsyncEventHandler<PresenceUpdateEventArgs> PresenceUpdated
        {
            add { this._presence_update.Register(value); }
            remove { this._presence_update.Unregister(value); }
        }
        private AsyncEvent<PresenceUpdateEventArgs> _presence_update;

        /// <summary>
        /// Fired when a guild ban gets added
        /// </summary>
        public event AsyncEventHandler<GuildBanAddEventArgs> GuildBanAdded
        {
            add { this._guild_ban_add.Register(value); }
            remove { this._guild_ban_add.Unregister(value); }
        }
        private AsyncEvent<GuildBanAddEventArgs> _guild_ban_add;

        /// <summary>
        /// Fired when a guild ban gets removed
        /// </summary>
        public event AsyncEventHandler<GuildBanRemoveEventArgs> GuildBanRemoved
        {
            add { this._guild_ban_remove.Register(value); }
            remove { this._guild_ban_remove.Unregister(value); }
        }
        private AsyncEvent<GuildBanRemoveEventArgs> _guild_ban_remove;

        /// <summary>
        /// Fired when a guilds emojis get updated
        /// </summary>
        public event AsyncEventHandler<GuildEmojisUpdateEventArgs> GuildEmojisUpdated
        {
            add { this._guild_emojis_update.Register(value); }
            remove { this._guild_emojis_update.Unregister(value); }
        }
        private AsyncEvent<GuildEmojisUpdateEventArgs> _guild_emojis_update;

        /// <summary>
        /// Fired when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
        {
            add { this._guild_integrations_update.Register(value); }
            remove { this._guild_integrations_update.Unregister(value); }
        }
        private AsyncEvent<GuildIntegrationsUpdateEventArgs> _guild_integrations_update;

        /// <summary>
        /// Fired when a new user joins a guild.
        /// </summary>
        public event AsyncEventHandler<GuildMemberAddEventArgs> GuildMemberAdded
        {
            add { this._guild_member_add.Register(value); }
            remove { this._guild_member_add.Unregister(value); }
        }
        private AsyncEvent<GuildMemberAddEventArgs> _guild_member_add;

        /// <summary>
        /// Fired when a user is removed from a guild (leave/kick/ban).
        /// </summary>
        public event AsyncEventHandler<GuildMemberRemoveEventArgs> GuildMemberRemoved
        {
            add { this._guild_member_remove.Register(value); }
            remove { this._guild_member_remove.Unregister(value); }
        }
        private AsyncEvent<GuildMemberRemoveEventArgs> _guild_member_remove;

        /// <summary>
        /// Fired when a guild member is updated.
        /// </summary>
        public event AsyncEventHandler<GuildMemberUpdateEventArgs> GuildMemberUpdated
        {
            add { this._guild_member_update.Register(value); }
            remove { this._guild_member_update.Unregister(value); }
        }
        private AsyncEvent<GuildMemberUpdateEventArgs> _guild_member_update;

        /// <summary>
        /// Fired when a guild role is created.
        /// </summary>
        public event AsyncEventHandler<GuildRoleCreateEventArgs> GuildRoleCreated
        {
            add { this._guild_role_create.Register(value); }
            remove { this._guild_role_create.Unregister(value); }
        }
        private AsyncEvent<GuildRoleCreateEventArgs> _guild_role_create;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleUpdateEventArgs> GuildRoleUpdated
        {
            add { this._guild_role_update.Register(value); }
            remove { this._guild_role_update.Unregister(value); }
        }
        private AsyncEvent<GuildRoleUpdateEventArgs> _guild_role_update;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleDeleteEventArgs> GuildRoleDeleted
        {
            add { this._guild_role_delete.Register(value); }
            remove { this._guild_role_delete.Unregister(value); }
        }
        private AsyncEvent<GuildRoleDeleteEventArgs> _guild_role_delete;

        /// <summary>
        /// Fired when a message is updated.
        /// </summary>
        public event AsyncEventHandler<MessageUpdateEventArgs> MessageUpdated
        {
            add { this._message_update.Register(value); }
            remove { this._message_update.Unregister(value); }
        }
        private AsyncEvent<MessageUpdateEventArgs> _message_update;

        /// <summary>
        /// Fired when a message is deleted.
        /// </summary>
        public event AsyncEventHandler<MessageDeleteEventArgs> MessageDeleted
        {
            add { this._message_delete.Register(value); }
            remove { this._message_delete.Unregister(value); }
        }
        private AsyncEvent<MessageDeleteEventArgs> _message_delete;

        /// <summary>
        /// Fired when multiple messages are deleted at once.
        /// </summary>
        public event AsyncEventHandler<MessageBulkDeleteEventArgs> MessagesBulkDeleted
        {
            add { this._message_bulk_delete.Register(value); }
            remove { this._message_bulk_delete.Unregister(value); }
        }
        private AsyncEvent<MessageBulkDeleteEventArgs> _message_bulk_delete;

        /// <summary>
        /// Fired when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<TypingStartEventArgs> TypingStarted
        {
            add { this._typing_start.Register(value); }
            remove { this._typing_start.Unregister(value); }
        }
        private AsyncEvent<TypingStartEventArgs> _typing_start;

        /// <summary>
        /// Fired when the current user updates their settings.
        /// </summary>
        public event AsyncEventHandler<UserSettingsUpdateEventArgs> UserSettingsUpdated
        {
            add { this._user_settings_update.Register(value); }
            remove { this._user_settings_update.Unregister(value); }
        }
        private AsyncEvent<UserSettingsUpdateEventArgs> _user_settings_update;

        /// <summary>
        /// Fired when properties about the user change.
        /// </summary>
        public event AsyncEventHandler<UserUpdateEventArgs> UserUpdated
        {
            add { this._user_update.Register(value); }
            remove { this._user_update.Unregister(value); }
        }
        private AsyncEvent<UserUpdateEventArgs> _user_update;

        /// <summary>
        /// Fired when someone joins/leaves/moves voice channels.
        /// </summary>
        public event AsyncEventHandler<VoiceStateUpdateEventArgs> VoiceStateUpdated
        {
            add { this._voice_state_update.Register(value); }
            remove { this._voice_state_update.Unregister(value); }
        }
        private AsyncEvent<VoiceStateUpdateEventArgs> _voice_state_update;

        /// <summary>
        /// Fired when a guild's voice server is updated.
        /// </summary>
        public event AsyncEventHandler<VoiceServerUpdateEventArgs> VoiceServerUpdated
        {
            add { this._voice_server_update.Register(value); }
            remove { this._voice_server_update.Unregister(value); }
        }
        private AsyncEvent<VoiceServerUpdateEventArgs> _voice_server_update;

        /// <summary>
        /// Fired in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<GuildMembersChunkEventArgs> GuildMembersChunked
        {
            add { this._guild_members_chunk.Register(value); }
            remove { this._guild_members_chunk.Unregister(value); }
        }
        private AsyncEvent<GuildMembersChunkEventArgs> _guild_members_chunk;

        /// <summary>
        /// Fired when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<UnknownEventArgs> UnknownEvent
        {
            add { this._unknown_event.Register(value); }
            remove { this._unknown_event.Unregister(value); }
        }
        private AsyncEvent<UnknownEventArgs> _unknown_event;

        /// <summary>
        /// Fired when a reaction gets added to a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionAddEventArgs> MessageReactionAdded
        {
            add { this._message_reaction_add.Register(value); }
            remove { this._message_reaction_add.Unregister(value); }
        }
        private AsyncEvent<MessageReactionAddEventArgs> _message_reaction_add;

        /// <summary>
        /// Fired when a reaction gets removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEventArgs> MessageReactionRemoved
        {
            add { this._message_reaction_remove.Register(value); }
            remove { this._message_reaction_remove.Unregister(value); }
        }
        private AsyncEvent<MessageReactionRemoveEventArgs> _message_reaction_remove;

        /// <summary>
        /// Fired when all reactions get removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionsClearEventArgs> MessageReactionsCleared
        {
            add { this._message_reaction_remove_all.Register(value); }
            remove { this._message_reaction_remove_all.Unregister(value); }
        }
        private AsyncEvent<MessageReactionsClearEventArgs> _message_reaction_remove_all;

        /// <summary>
        /// Fired whenever webhooks update.
        /// </summary>
        public event AsyncEventHandler<WebhooksUpdateEventArgs> WebhooksUpdated
        {
            add { this._webhooks_update.Register(value); }
            remove { this._webhooks_update.Unregister(value); }
        }
        private AsyncEvent<WebhooksUpdateEventArgs> _webhooks_update;

        /// <summary>
        /// Fired on received heartbeat ACK.
        /// </summary>
        public event AsyncEventHandler<HeartbeatEventArgs> Heartbeated
        {
            add { this._heartbeated.Register(value); }
            remove { this._heartbeated.Unregister(value); }
        }
        private AsyncEvent<HeartbeatEventArgs> _heartbeated;

        internal void EventErrorHandler(string evname, Exception ex)
        {
            this.DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"An {ex.GetType()} occured in {evname}.", DateTime.Now);
            this._client_error.InvokeAsync(new ClientErrorEventArgs(null) { EventName = evname, Exception = ex }).GetAwaiter().GetResult();
        }

        private void Goof(string evname, Exception ex)
        {
            this.DebugLogger.LogMessage(LogLevel.Critical, "DSharpPlus", $"An {ex.GetType()} occured in the exception handler.", DateTime.Now);
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
        public IReadOnlyDictionary<int, DiscordClient> ShardClients => new ReadOnlyDictionary<int, DiscordClient>(this.Shards);

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public DiscordUser CurrentUser => this._current_user;
        private DiscordUser _current_user;

        /// <summary>
        /// Gets the current application.
        /// </summary>
        public DiscordApplication CurrentApplication => this._current_application;
        private DiscordApplication _current_application;

        /// <summary>
        /// Initializes new auto-sharding Discord client.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        public DiscordShardedClient(DiscordConfiguration config)
        {
            if (config.TokenType == TokenType.User)
                throw new InvalidOperationException("You cannot shard using a user token.");

            this._client_error = new AsyncEvent<ClientErrorEventArgs>(this.Goof, "CLIENT_ERRORED");
            this._socket_error = new AsyncEvent<SocketErrorEventArgs>(this.Goof, "SOCKET_ERRORED");
            this._socket_opened = new AsyncEvent(this.EventErrorHandler, "SOCKET_OPENED");
            this._socket_closed = new AsyncEvent<SocketCloseEventArgs>(this.EventErrorHandler, "SOCKET_CLOSED");
            this._ready = new AsyncEvent<ReadyEventArgs>(this.EventErrorHandler, "READY");
            this._resumed = new AsyncEvent<ReadyEventArgs>(this.EventErrorHandler, "RESUMED");
            this._channel_created = new AsyncEvent<ChannelCreateEventArgs>(this.EventErrorHandler, "CHANNEL_CREATED");
            this._dm_channel_created = new AsyncEvent<DmChannelCreateEventArgs>(this.EventErrorHandler, "DM_CHANNEL_CREATED");
            this._channel_updated = new AsyncEvent<ChannelUpdateEventArgs>(this.EventErrorHandler, "CHANNEL_UPDATED");
            this._channel_deleted = new AsyncEvent<ChannelDeleteEventArgs>(this.EventErrorHandler, "CHANNEL_DELETED");
            this._dm_channel_deleted = new AsyncEvent<DmChannelDeleteEventArgs>(this.EventErrorHandler, "DM_CHANNEL_DELETED");
            this._channel_pins_updated = new AsyncEvent<ChannelPinsUpdateEventArgs>(this.EventErrorHandler, "CHANNEL_PINS_UPDATED");
            this._guild_created = new AsyncEvent<GuildCreateEventArgs>(this.EventErrorHandler, "GUILD_CREATED");
            this._guild_available = new AsyncEvent<GuildCreateEventArgs>(this.EventErrorHandler, "GUILD_AVAILABLE");
            this._guild_updated = new AsyncEvent<GuildUpdateEventArgs>(this.EventErrorHandler, "GUILD_UPDATED");
            this._guild_deleted = new AsyncEvent<GuildDeleteEventArgs>(this.EventErrorHandler, "GUILD_DELETED");
            this._guild_unavailable = new AsyncEvent<GuildDeleteEventArgs>(this.EventErrorHandler, "GUILD_UNAVAILABLE");
            this._message_created = new AsyncEvent<MessageCreateEventArgs>(this.EventErrorHandler, "MESSAGE_CREATED");
            this._presence_update = new AsyncEvent<PresenceUpdateEventArgs>(this.EventErrorHandler, "PRESENCE_UPDATED");
            this._guild_ban_add = new AsyncEvent<GuildBanAddEventArgs>(this.EventErrorHandler, "GUILD_BAN_ADDED");
            this._guild_ban_remove = new AsyncEvent<GuildBanRemoveEventArgs>(this.EventErrorHandler, "GUILD_BAN_REMOVED");
            this._guild_emojis_update = new AsyncEvent<GuildEmojisUpdateEventArgs>(this.EventErrorHandler, "GUILD_EMOJI_UPDATED");
            this._guild_integrations_update = new AsyncEvent<GuildIntegrationsUpdateEventArgs>(this.EventErrorHandler, "GUILD_INTEGRATIONS_UPDATED");
            this._guild_member_add = new AsyncEvent<GuildMemberAddEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_ADDED");
            this._guild_member_remove = new AsyncEvent<GuildMemberRemoveEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_REMOVED");
            this._guild_member_update = new AsyncEvent<GuildMemberUpdateEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_UPDATED");
            this._guild_role_create = new AsyncEvent<GuildRoleCreateEventArgs>(this.EventErrorHandler, "GUILD_ROLE_CREATED");
            this._guild_role_update = new AsyncEvent<GuildRoleUpdateEventArgs>(this.EventErrorHandler, "GUILD_ROLE_UPDATED");
            this._guild_role_delete = new AsyncEvent<GuildRoleDeleteEventArgs>(this.EventErrorHandler, "GUILD_ROLE_DELETED");
            this._message_update = new AsyncEvent<MessageUpdateEventArgs>(this.EventErrorHandler, "MESSAGE_UPDATED");
            this._message_delete = new AsyncEvent<MessageDeleteEventArgs>(this.EventErrorHandler, "MESSAGE_DELETED");
            this._message_bulk_delete = new AsyncEvent<MessageBulkDeleteEventArgs>(this.EventErrorHandler, "MESSAGE_BULK_DELETED");
            this._typing_start = new AsyncEvent<TypingStartEventArgs>(this.EventErrorHandler, "TYPING_STARTED");
            this._user_settings_update = new AsyncEvent<UserSettingsUpdateEventArgs>(this.EventErrorHandler, "USER_SETTINGS_UPDATED");
            this._user_update = new AsyncEvent<UserUpdateEventArgs>(this.EventErrorHandler, "USER_UPDATED");
            this._voice_state_update = new AsyncEvent<VoiceStateUpdateEventArgs>(this.EventErrorHandler, "VOICE_STATE_UPDATED");
            this._voice_server_update = new AsyncEvent<VoiceServerUpdateEventArgs>(this.EventErrorHandler, "VOICE_SERVER_UPDATED");
            this._guild_members_chunk = new AsyncEvent<GuildMembersChunkEventArgs>(this.EventErrorHandler, "GUILD_MEMBERS_CHUNKED");
            this._unknown_event = new AsyncEvent<UnknownEventArgs>(this.EventErrorHandler, "UNKNOWN_EVENT");
            this._message_reaction_add = new AsyncEvent<MessageReactionAddEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_ADDED");
            this._message_reaction_remove = new AsyncEvent<MessageReactionRemoveEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_REMOVED");
            this._message_reaction_remove_all = new AsyncEvent<MessageReactionsClearEventArgs>(this.EventErrorHandler, "MESSAGE_REACTIONS_CLEARED");
            this._webhooks_update = new AsyncEvent<WebhooksUpdateEventArgs>(this.EventErrorHandler, "WEBHOOKS_UPDATED");
            this._heartbeated = new AsyncEvent<HeartbeatEventArgs>(this.EventErrorHandler, "HEARTBEATED");

            this.Config = config;
            this.Shards = new ConcurrentDictionary<int, DiscordClient>();
            this.DebugLogger = new DebugLogger(config.LogLevel, config.DateTimeFormat);

            if (config.UseInternalLogHandler)
                DebugLogger.LogMessageReceived += (sender, e) => DebugLogger.LogHandler(sender, e);
        }

        internal async Task<int> InitializeShardsAsync()
        {
            if (this.Shards.Count != 0)
                return this.Shards.Count;

            var shardc = this.Config.ShardCount == 1 ? await this.GetShardCountAsync() : this.Config.ShardCount;
            for (var i = 0; i < shardc; i++)
            {
                var cfg = new DiscordConfiguration(this.Config)
                {
                    ShardId = i,
                    ShardCount = shardc,
                    UseInternalLogHandler = false
                };

                var client = new DiscordClient(cfg);
                if (!this.Shards.TryAdd(i, client))
                    throw new Exception("Could not initialize shards.");
            }

            return shardc;
        }

        /// <summary>
        /// Initializes and connects all shards.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            var shardc = await this.InitializeShardsAsync();
            this.DebugLogger.LogMessage(LogLevel.Info, "Autoshard", $"Booting {shardc.ToString(CultureInfo.InvariantCulture)} shards", DateTime.Now);

            for (var i = 0; i < shardc; i++)
            {
                if (!this.Shards.TryGetValue(i, out var client))
                    throw new Exception("Could not initialize shards");

                if (this.CurrentUser != null)
                    client.CurrentUser = this.CurrentUser;

                if (this._current_application != null)
                    client.CurrentApplication = this.CurrentApplication;

                client.ClientErrored += this.Client_ClientError;
                client.SocketErrored += this.Client_SocketError;
                client.SocketOpened += this.Client_SocketOpened;
                client.SocketClosed += this.Client_SocketClosed;
                client.Ready += this.Client_Ready;
                client.Resumed += this.Client_Resumed;
                client.ChannelCreated += this.Client_ChannelCreated;
                client.DmChannelCreated += this.Client_DMChannelCreated;
                client.ChannelUpdated += this.Client_ChannelUpdated;
                client.ChannelDeleted += this.Client_ChannelDeleted;
                client.DmChannelDeleted += this.Client_DMChannelDeleted;
                client.ChannelPinsUpdated += this.Client_ChannelPinsUpdated;
                client.GuildCreated += this.Client_GuildCreated;
                client.GuildAvailable += this.Client_GuildAvailable;
                client.GuildUpdated += this.Client_GuildUpdated;
                client.GuildDeleted += this.Client_GuildDeleted;
                client.GuildUnavailable += this.Client_GuildUnavailable;
                client.MessageCreated += this.Client_MessageCreated;
                client.PresenceUpdated += this.Client_PresenceUpdate;
                client.GuildBanAdded += this.Client_GuildBanAdd;
                client.GuildBanRemoved += this.Client_GuildBanRemove;
                client.GuildEmojisUpdated += this.Client_GuildEmojisUpdate;
                client.GuildIntegrationsUpdated += this.Client_GuildIntegrationsUpdate;
                client.GuildMemberAdded += this.Client_GuildMemberAdd;
                client.GuildMemberRemoved += this.Client_GuildMemberRemove;
                client.GuildMemberUpdated += this.Client_GuildMemberUpdate;
                client.GuildRoleCreated += this.Client_GuildRoleCreate;
                client.GuildRoleUpdated += this.Client_GuildRoleUpdate;
                client.GuildRoleDeleted += this.Client_GuildRoleDelete;
                client.MessageUpdated += this.Client_MessageUpdate;
                client.MessageDeleted += this.Client_MessageDelete;
                client.MessagesBulkDeleted += this.Client_MessageBulkDelete;
                client.TypingStarted += this.Client_TypingStart;
                client.UserSettingsUpdated += this.Client_UserSettingsUpdate;
                client.UserUpdated += this.Client_UserUpdate;
                client.VoiceStateUpdated += this.Client_VoiceStateUpdate;
                client.VoiceServerUpdated += this.Client_VoiceServerUpdate;
                client.GuildMembersChunked += this.Client_GuildMembersChunk;
                client.UnknownEvent += this.Client_UnknownEvent;
                client.MessageReactionAdded += this.Client_MessageReactionAdd;
                client.MessageReactionRemoved += this.Client_MessageReactionRemove;
                client.MessageReactionsCleared += this.Client_MessageReactionRemoveAll;
                client.WebhooksUpdated += this.Client_WebhooksUpdate;
                client.Heartbeated += this.Client_HeartBeated;
                client.DebugLogger.LogMessageReceived += this.DebugLogger_LogMessageReceived;
                
                await client.ConnectAsync();
                this.DebugLogger.LogMessage(LogLevel.Info, "Autoshard", $"Booted shard {i.ToString(CultureInfo.InvariantCulture)}", DateTime.Now);

                if (this._current_user == null)
                    this._current_user = client.CurrentUser;

                if (this._current_application == null)
                    this._current_application = client.CurrentApplication;
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
        /// <param name="user_status">Status of the user.</param>
        /// <param name="idle_since">Since when is the client idle.</param>
        /// <returns>Asynchronous operation.</returns>
        public async Task UpdateStatusAsync(DiscordGame game = null, UserStatus? user_status = null, DateTimeOffset? idle_since = null)
        {
            var tasks = new List<Task>();
            foreach (var client in this.ShardClients.Values)
                tasks.Add(client.UpdateStatusAsync(game, user_status, idle_since));

            await Task.WhenAll(tasks);
        }

        private async Task<int> GetShardCountAsync()
        {
            string url = $"{Utilities.GetApiBaseUri()}{Endpoints.GATEWAY}{Endpoints.BOT}";
            var headers = Utilities.GetBaseHeaders();

            var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
            http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(this.Config));
            var resp = await http.GetAsync(url);

            var jo = JObject.Parse(await resp.Content.ReadAsStringAsync());
            if (jo["shards"] != null)
                return jo.Value<int>("shards");
            return 1;
        }

        #region Event Dispatchers
        private Task Client_ClientError(ClientErrorEventArgs e) =>
            this._client_error.InvokeAsync(e);

        private Task Client_SocketError(SocketErrorEventArgs e) =>
            this._socket_error.InvokeAsync(e);

        private Task Client_SocketOpened() =>
            this._socket_opened.InvokeAsync();

        private Task Client_SocketClosed(SocketCloseEventArgs e) =>
            this._socket_closed.InvokeAsync(e);

        private Task Client_Ready(ReadyEventArgs e) =>
            this._ready.InvokeAsync(e);

        private Task Client_Resumed(ReadyEventArgs e) =>
            this._resumed.InvokeAsync(e);

        private Task Client_ChannelCreated(ChannelCreateEventArgs e) =>
            this._channel_created.InvokeAsync(e);

        private Task Client_DMChannelCreated(DmChannelCreateEventArgs e) =>
            this._dm_channel_created.InvokeAsync(e);

        private Task Client_ChannelUpdated(ChannelUpdateEventArgs e) =>
            this._channel_updated.InvokeAsync(e);

        private Task Client_ChannelDeleted(ChannelDeleteEventArgs e) =>
            this._channel_deleted.InvokeAsync(e);

        private Task Client_DMChannelDeleted(DmChannelDeleteEventArgs e) =>
            this._dm_channel_deleted.InvokeAsync(e);

        private Task Client_ChannelPinsUpdated(ChannelPinsUpdateEventArgs e) =>
            this._channel_pins_updated.InvokeAsync(e);

        private Task Client_GuildCreated(GuildCreateEventArgs e) =>
            this._guild_created.InvokeAsync(e);

        private Task Client_GuildAvailable(GuildCreateEventArgs e) =>
            this._guild_available.InvokeAsync(e);

        private Task Client_GuildUpdated(GuildUpdateEventArgs e) =>
            this._guild_updated.InvokeAsync(e);

        private Task Client_GuildDeleted(GuildDeleteEventArgs e) =>
            this._guild_deleted.InvokeAsync(e);

        private Task Client_GuildUnavailable(GuildDeleteEventArgs e) =>
            this._guild_unavailable.InvokeAsync(e);

        private Task Client_MessageCreated(MessageCreateEventArgs e) =>
            this._message_created.InvokeAsync(e);

        private Task Client_PresenceUpdate(PresenceUpdateEventArgs e) =>
            this._presence_update.InvokeAsync(e);

        private Task Client_GuildBanAdd(GuildBanAddEventArgs e) =>
            this._guild_ban_add.InvokeAsync(e);

        private Task Client_GuildBanRemove(GuildBanRemoveEventArgs e) =>
            this._guild_ban_remove.InvokeAsync(e);

        private Task Client_GuildEmojisUpdate(GuildEmojisUpdateEventArgs e) =>
            this._guild_emojis_update.InvokeAsync(e);

        private Task Client_GuildIntegrationsUpdate(GuildIntegrationsUpdateEventArgs e) =>
            this._guild_integrations_update.InvokeAsync(e);

        private Task Client_GuildMemberAdd(GuildMemberAddEventArgs e) =>
            this._guild_member_add.InvokeAsync(e);

        private Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e) =>
            this._guild_member_remove.InvokeAsync(e);

        private Task Client_GuildMemberUpdate(GuildMemberUpdateEventArgs e) =>
            this._guild_member_update.InvokeAsync(e);

        private Task Client_GuildRoleCreate(GuildRoleCreateEventArgs e) =>
            this._guild_role_create.InvokeAsync(e);

        private Task Client_GuildRoleUpdate(GuildRoleUpdateEventArgs e) =>
            this._guild_role_update.InvokeAsync(e);

        private Task Client_GuildRoleDelete(GuildRoleDeleteEventArgs e) =>
            this._guild_role_delete.InvokeAsync(e);

        private Task Client_MessageUpdate(MessageUpdateEventArgs e) =>
            this._message_update.InvokeAsync(e);

        private Task Client_MessageDelete(MessageDeleteEventArgs e) =>
            this._message_delete.InvokeAsync(e);

        private Task Client_MessageBulkDelete(MessageBulkDeleteEventArgs e) =>
            this._message_bulk_delete.InvokeAsync(e);

        private Task Client_TypingStart(TypingStartEventArgs e) =>
            this._typing_start.InvokeAsync(e);

        private Task Client_UserSettingsUpdate(UserSettingsUpdateEventArgs e) =>
            this._user_settings_update.InvokeAsync(e);

        private Task Client_UserUpdate(UserUpdateEventArgs e) =>
            this._user_update.InvokeAsync(e);

        private Task Client_VoiceStateUpdate(VoiceStateUpdateEventArgs e) =>
            this._voice_state_update.InvokeAsync(e);

        private Task Client_VoiceServerUpdate(VoiceServerUpdateEventArgs e) =>
            this._voice_server_update.InvokeAsync(e);

        private Task Client_GuildMembersChunk(GuildMembersChunkEventArgs e) =>
            this._guild_members_chunk.InvokeAsync(e);

        private Task Client_UnknownEvent(UnknownEventArgs e) =>
            this._unknown_event.InvokeAsync(e);

        private Task Client_MessageReactionAdd(MessageReactionAddEventArgs e) =>
            this._message_reaction_add.InvokeAsync(e);

        private Task Client_MessageReactionRemove(MessageReactionRemoveEventArgs e) =>
            this._message_reaction_remove.InvokeAsync(e);

        private Task Client_MessageReactionRemoveAll(MessageReactionsClearEventArgs e) =>
            this._message_reaction_remove_all.InvokeAsync(e);

        private Task Client_WebhooksUpdate(WebhooksUpdateEventArgs e) =>
            this._webhooks_update.InvokeAsync(e);

        private Task Client_HeartBeated(HeartbeatEventArgs e) =>
            this._heartbeated.InvokeAsync(e);

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e) =>
            this.DebugLogger.LogMessage(e.Level, e.Application, e.Message, e.Timestamp);
        #endregion
    }
}
