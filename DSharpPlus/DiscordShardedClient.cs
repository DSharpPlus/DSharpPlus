using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Web;
using Newtonsoft.Json.Linq;

namespace DSharpPlus
{
    /// <summary>
    /// A Discord client that shards automatically.
    /// </summary>
    public sealed class DiscordShardedClient
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
            this._client_error.InvokeAsync(new ClientErrorEventArgs(null) { EventName = evname, Exception = ex }).GetAwaiter().GetResult();
        }

        private void Goof(string evname, Exception ex)
        {
            this.DebugLogger.LogMessage(LogLevel.Critical, "DSharpPlus", $"An {ex.GetType()} occured in the exception handler.", DateTime.Now);
        }
        #endregion

        private DiscordConfig Config { get; }
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
        /// Initializes new auto-sharding Discord client.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        public DiscordShardedClient(DiscordConfig config)
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

            this.Config = config;
            this.Shards = new ConcurrentDictionary<int, DiscordClient>();
            this.DebugLogger = new DebugLogger(config.LogLevel);

            if (config.UseInternalLogHandler)
                DebugLogger.LogMessageReceived += (sender, e) => DebugLogger.LogHandler(sender, e);
        }

        /// <summary>
        /// Initializes and connects all shards.
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            var shardc = this.Config.ShardCount == 1 ? await this.GetShardCountAsync() : this.Config.ShardCount;
            this.DebugLogger.LogMessage(LogLevel.Info, "Autoshard", $"Booting {shardc} shards", DateTime.Now);

            for (var i = 0; i < shardc; i++)
            {
                var cfg = new DiscordConfig(this.Config)
                {
                    ShardId = i,
                    ShardCount = shardc,
                    UseInternalLogHandler = false
                };

                var client = new DiscordClient(cfg);
                if (!this.Shards.TryAdd(i, client))
                    throw new Exception("Could not initialize shards.");

                if (this.CurrentUser != null)
                    client._current_user = this.CurrentUser;

                client.ClientError += this.Client_ClientError;
                client.SocketOpened += this.Client_SocketOpened;
                client.SocketClosed += this.Client_SocketClosed;
                client.Ready += this.Client_Ready;
                client.ChannelCreated += this.Client_ChannelCreated;
                client.DMChannelCreated += this.Client_DMChannelCreated;
                client.ChannelUpdated += this.Client_ChannelUpdated;
                client.ChannelDeleted += this.Client_ChannelDeleted;
                client.DMChannelDeleted += this.Client_DMChannelDeleted;
                client.GuildCreated += this.Client_GuildCreated;
                client.GuildAvailable += this.Client_GuildAvailable;
                client.GuildUpdated += this.Client_GuildUpdated;
                client.GuildDeleted += this.Client_GuildDeleted;
                client.GuildUnavailable += this.Client_GuildUnavailable;
                client.MessageCreated += this.Client_MessageCreated;
                client.PresenceUpdate += this.Client_PresenceUpdate;
                client.GuildBanAdd += this.Client_GuildBanAdd;
                client.GuildBanRemove += this.Client_GuildBanRemove;
                client.GuildEmojisUpdate += this.Client_GuildEmojisUpdate;
                client.GuildIntegrationsUpdate += this.Client_GuildIntegrationsUpdate;
                client.GuildMemberAdd += this.Client_GuildMemberAdd;
                client.GuildMemberRemove += this.Client_GuildMemberRemove;
                client.GuildMemberUpdate += this.Client_GuildMemberUpdate;
                client.GuildRoleCreate += this.Client_GuildRoleCreate;
                client.GuildRoleUpdate += this.Client_GuildRoleUpdate;
                client.GuildRoleDelete += this.Client_GuildRoleDelete;
                client.MessageUpdate += this.Client_MessageUpdate;
                client.MessageDelete += this.Client_MessageDelete;
                client.MessageBulkDelete += this.Client_MessageBulkDelete;
                client.TypingStart += this.Client_TypingStart;
                client.UserSettingsUpdate += this.Client_UserSettingsUpdate;
                client.UserUpdate += this.Client_UserUpdate;
                client.VoiceStateUpdate += this.Client_VoiceStateUpdate;
                client.VoiceServerUpdate += this.Client_VoiceServerUpdate;
                client.GuildMembersChunk += this.Client_GuildMembersChunk;
                client.UnknownEvent += this.Client_UnknownEvent;
                client.MessageReactionAdd += this.Client_MessageReactionAdd;
                client.MessageReactionRemove += this.Client_MessageReactionRemove;
                client.MessageReactionRemoveAll += this.Client_MessageReactionRemoveAll;
                client.WebhooksUpdate += this.Client_WebhooksUpdate;
                client.HeartBeated += this.Client_HeartBeated;
                client.DebugLogger.LogMessageReceived += this.DebugLogger_LogMessageReceived;
                
                await client.ConnectAsync();
                this.DebugLogger.LogMessage(LogLevel.Info, "Autoshard", $"Booted shard {i}", DateTime.Now);

                if (this._current_user == null)
                    this._current_user = client.CurrentUser;
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
        /// <param name="idle_since">Since when is the client idle.</param>
        /// <returns>Asynchronous operation.</returns>
        public async Task UpdateStatusAsync(string game = "", int idle_since = -1)
        {
            var tasks = new List<Task>();
            foreach (var client in this.ShardClients.Values)
                tasks.Add(client.UpdateStatusAsync(game, idle_since));

            await Task.WhenAll(tasks);
        }

        private async Task<int> GetShardCountAsync()
        {
            string url = $"{Utils.GetApiBaseUri(this.Config)}{Endpoints.Gateway}{Endpoints.Bot}";
            var headers = Utils.GetBaseHeaders();

            var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utils.GetUserAgent());
            http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utils.GetFormattedToken(this.Config));
            var resp = await http.GetAsync(url);

            var jo = JObject.Parse(await resp.Content.ReadAsStringAsync());
            if (jo["shards"] != null)
                return jo.Value<int>("shards");
            return 1;
        }

        #region Event Dispatchers
        private Task Client_ClientError(ClientErrorEventArgs e) =>
            this._client_error.InvokeAsync(e);

        private Task Client_SocketOpened() =>
            this._socket_opened.InvokeAsync();

        private Task Client_SocketClosed(SocketDisconnectEventArgs e) =>
            this._socket_closed.InvokeAsync(e);

        private Task Client_Ready(ReadyEventArgs e) =>
            this._ready.InvokeAsync(e);

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

        private Task Client_MessageReactionRemoveAll(MessageReactionRemoveAllEventArgs e) =>
            this._message_reaction_remove_all.InvokeAsync(e);

        private Task Client_WebhooksUpdate(WebhooksUpdateEventArgs e) =>
            this._webhooks_update.InvokeAsync(e);

        private Task Client_HeartBeated(HeartBeatEventArgs e) =>
            this._heart_beated.InvokeAsync(e);

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e) =>
            this.DebugLogger.LogMessage(e.Level, e.Application, e.Message, e.Timestamp);
        #endregion
    }
}
