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
            add => this._clientErrored.Register(value);
            remove => this._clientErrored.Unregister(value);
        }
        private AsyncEvent<ClientErrorEventArgs> _clientErrored;

        /// <summary>
        /// Fired whenever a WebSocket error occurs within the client.
        /// </summary>
        public event AsyncEventHandler<SocketErrorEventArgs> SocketErrored
        {
            add => this._socketErrored.Register(value);
            remove => this._socketErrored.Unregister(value);
        }
        private AsyncEvent<SocketErrorEventArgs> _socketErrored;

        /// <summary>
        /// Fired whenever WebSocket connection is established.
        /// </summary>
        public event AsyncEventHandler SocketOpened
        {
            add => this._socketOpened.Register(value);
            remove => this._socketOpened.Unregister(value);
        }
        private AsyncEvent _socketOpened;

        /// <summary>
        /// Fired whenever WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<SocketCloseEventArgs> SocketClosed
        {
            add => this._socketClosed.Register(value);
            remove => this._socketClosed.Unregister(value);
        }
        private AsyncEvent<SocketCloseEventArgs> _socketClosed;

        /// <summary>
        /// Fired when the client enters ready state.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Ready
        {
            add => this._ready.Register(value);
            remove => this._ready.Unregister(value);
        }
        private AsyncEvent<ReadyEventArgs> _ready;

        /// <summary>
        /// Fired whenever a session is resumed.
        /// </summary>
        public event AsyncEventHandler<ReadyEventArgs> Resumed
        {
            add => this._resumed.Register(value);
            remove => this._resumed.Unregister(value);
        }
        private AsyncEvent<ReadyEventArgs> _resumed;

        /// <summary>
        /// Fired when a new channel is created.
        /// </summary>
        public event AsyncEventHandler<ChannelCreateEventArgs> ChannelCreated
        {
            add => this._channelCreated.Register(value);
            remove => this._channelCreated.Unregister(value);
        }
        private AsyncEvent<ChannelCreateEventArgs> _channelCreated;

        /// <summary>
        /// Fired when a new direct message channel is created.
        /// </summary>
        public event AsyncEventHandler<DmChannelCreateEventArgs> DmChannelCreated
        {
            add => this._dmChannelCreated.Register(value);
            remove => this._dmChannelCreated.Unregister(value);
        }
        private AsyncEvent<DmChannelCreateEventArgs> _dmChannelCreated;

        /// <summary>
        /// Fired when a channel is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelUpdateEventArgs> ChannelUpdated
        {
            add => this._channelUpdated.Register(value);
            remove => this._channelUpdated.Unregister(value);
        }
        private AsyncEvent<ChannelUpdateEventArgs> _channelUpdated;

        /// <summary>
        /// Fired when a channel is deleted
        /// </summary>
        public event AsyncEventHandler<ChannelDeleteEventArgs> ChannelDeleted
        {
            add => this._channelDeleted.Register(value);
            remove => this._channelDeleted.Unregister(value);
        }
        private AsyncEvent<ChannelDeleteEventArgs> _channelDeleted;

        /// <summary>
        /// Fired when a dm channel is deleted
        /// </summary>
        public event AsyncEventHandler<DmChannelDeleteEventArgs> DmChannelDeleted
        {
            add => this._dmChannelDeleted.Register(value);
            remove => this._dmChannelDeleted.Unregister(value);
        }
        private AsyncEvent<DmChannelDeleteEventArgs> _dmChannelDeleted;

        /// <summary>
        /// Fired whenever a channel's pinned message list is updated.
        /// </summary>
        public event AsyncEventHandler<ChannelPinsUpdateEventArgs> ChannelPinsUpdated
        {
            add => this._channelPinsUpdated.Register(value);
            remove => this._channelPinsUpdated.Unregister(value);
        }
        private AsyncEvent<ChannelPinsUpdateEventArgs> _channelPinsUpdated;

        /// <summary>
        /// Fired when the user joins a new guild.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildCreated
        {
            add => this._guildCreated.Register(value);
            remove => this._guildCreated.Unregister(value);
        }
        private AsyncEvent<GuildCreateEventArgs> _guildCreated;

        /// <summary>
        /// Fired when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<GuildCreateEventArgs> GuildAvailable
        {
            add => this._guildAvailable.Register(value);
            remove => this._guildAvailable.Unregister(value);
        }
        private AsyncEvent<GuildCreateEventArgs> _guildAvailable;

        /// <summary>
        /// Fired when a guild is updated.
        /// </summary>
        public event AsyncEventHandler<GuildUpdateEventArgs> GuildUpdated
        {
            add => this._guildUpdated.Register(value);
            remove => this._guildUpdated.Unregister(value);
        }
        private AsyncEvent<GuildUpdateEventArgs> _guildUpdated;

        /// <summary>
        /// Fired when the user leaves or is removed from a guild.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildDeleted
        {
            add => this._guildDeleted.Register(value);
            remove => this._guildDeleted.Unregister(value);
        }
        private AsyncEvent<GuildDeleteEventArgs> _guildDeleted;

        /// <summary>
        /// Fired when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<GuildDeleteEventArgs> GuildUnavailable
        {
            add => this._guildUnavailable.Register(value);
            remove => this._guildUnavailable.Unregister(value);
        }
        private AsyncEvent<GuildDeleteEventArgs> _guildUnavailable;

        /// <summary>
        /// Fired when all guilds finish streaming from Discord.
        /// </summary>
        public event AsyncEventHandler<GuildDownloadCompletedEventArgs> GuildDownloadCompleted
        {
            add => this._guildDownloadCompleted.Register(value);
            remove => this._guildDownloadCompleted.Unregister(value);
        }
        private AsyncEvent<GuildDownloadCompletedEventArgs> _guildDownloadCompleted;

        /// <summary>
        /// Fired when an invite is created.
        /// </summary>
        public event AsyncEventHandler<InviteCreateEventArgs> InviteCreated
        {
            add => this._inviteCreated.Register(value);
            remove => this._inviteCreated.Unregister(value);
        }
        private AsyncEvent<InviteCreateEventArgs> _inviteCreated;

        /// <summary>
        /// Fired when an invite is deleted.
        /// </summary>
        public event AsyncEventHandler<InviteDeleteEventArgs> InviteDeleted
        {
            add => this._inviteDeleted.Register(value);
            remove => this._inviteDeleted.Unregister(value);
        }
        private AsyncEvent<InviteDeleteEventArgs> _inviteDeleted;

        /// <summary>
        /// Fired when a message is created.
        /// </summary>
        public event AsyncEventHandler<MessageCreateEventArgs> MessageCreated
        {
            add => this._messageCreated.Register(value);
            remove => this._messageCreated.Unregister(value);
        }
        private AsyncEvent<MessageCreateEventArgs> _messageCreated;

        /// <summary>
        /// Fired when a presence has been updated.
        /// </summary>
        public event AsyncEventHandler<PresenceUpdateEventArgs> PresenceUpdated
        {
            add => this._presenceUpdated.Register(value);
            remove => this._presenceUpdated.Unregister(value);
        }
        private AsyncEvent<PresenceUpdateEventArgs> _presenceUpdated;

        /// <summary>
        /// Fired when a guild ban gets added
        /// </summary>
        public event AsyncEventHandler<GuildBanAddEventArgs> GuildBanAdded
        {
            add => this._guildBanAdded.Register(value);
            remove => this._guildBanAdded.Unregister(value);
        }
        private AsyncEvent<GuildBanAddEventArgs> _guildBanAdded;

        /// <summary>
        /// Fired when a guild ban gets removed
        /// </summary>
        public event AsyncEventHandler<GuildBanRemoveEventArgs> GuildBanRemoved
        {
            add => this._guildBanRemoved.Register(value);
            remove => this._guildBanRemoved.Unregister(value);
        }
        private AsyncEvent<GuildBanRemoveEventArgs> _guildBanRemoved;

        /// <summary>
        /// Fired when a guilds emojis get updated
        /// </summary>
        public event AsyncEventHandler<GuildEmojisUpdateEventArgs> GuildEmojisUpdated
        {
            add => this._guildEmojisUpdated.Register(value);
            remove => this._guildEmojisUpdated.Unregister(value);
        }
        private AsyncEvent<GuildEmojisUpdateEventArgs> _guildEmojisUpdated;

        /// <summary>
        /// Fired when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
        {
            add => this._guildIntegrationsUpdated.Register(value);
            remove => this._guildIntegrationsUpdated.Unregister(value);
        }
        private AsyncEvent<GuildIntegrationsUpdateEventArgs> _guildIntegrationsUpdated;

        /// <summary>
        /// Fired when a new user joins a guild.
        /// </summary>
        public event AsyncEventHandler<GuildMemberAddEventArgs> GuildMemberAdded
        {
            add => this._guildMemberAdded.Register(value);
            remove => this._guildMemberAdded.Unregister(value);
        }
        private AsyncEvent<GuildMemberAddEventArgs> _guildMemberAdded;

        /// <summary>
        /// Fired when a user is removed from a guild (leave/kick/ban).
        /// </summary>
        public event AsyncEventHandler<GuildMemberRemoveEventArgs> GuildMemberRemoved
        {
            add => this._guildMemberRemoved.Register(value);
            remove => this._guildMemberRemoved.Unregister(value);
        }
        private AsyncEvent<GuildMemberRemoveEventArgs> _guildMemberRemoved;

        /// <summary>
        /// Fired when a guild member is updated.
        /// </summary>
        public event AsyncEventHandler<GuildMemberUpdateEventArgs> GuildMemberUpdated
        {
            add => this._guildMemberUpdated.Register(value);
            remove => this._guildMemberUpdated.Unregister(value);
        }
        private AsyncEvent<GuildMemberUpdateEventArgs> _guildMemberUpdated;

        /// <summary>
        /// Fired when a guild role is created.
        /// </summary>
        public event AsyncEventHandler<GuildRoleCreateEventArgs> GuildRoleCreated
        {
            add => this._guildRoleCreated.Register(value);
            remove => this._guildRoleCreated.Unregister(value);
        }
        private AsyncEvent<GuildRoleCreateEventArgs> _guildRoleCreated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleUpdateEventArgs> GuildRoleUpdated
        {
            add => this._guildRoleUpdated.Register(value);
            remove => this._guildRoleUpdated.Unregister(value);
        }
        private AsyncEvent<GuildRoleUpdateEventArgs> _guildRoleUpdated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<GuildRoleDeleteEventArgs> GuildRoleDeleted
        {
            add => this._guildRoleDeleted.Register(value);
            remove => this._guildRoleDeleted.Unregister(value);
        }
        private AsyncEvent<GuildRoleDeleteEventArgs> _guildRoleDeleted;

        /// <summary>
        /// Fired when a message is updated.
        /// </summary>
        public event AsyncEventHandler<MessageUpdateEventArgs> MessageUpdated
        {
            add => this._messageUpdated.Register(value);
            remove => this._messageUpdated.Unregister(value);
        }
        private AsyncEvent<MessageUpdateEventArgs> _messageUpdated;

        /// <summary>
        /// Fired when a message is deleted.
        /// </summary>
        public event AsyncEventHandler<MessageDeleteEventArgs> MessageDeleted
        {
            add => this._messageDeleted.Register(value);
            remove => this._messageDeleted.Unregister(value);
        }
        private AsyncEvent<MessageDeleteEventArgs> _messageDeleted;

        /// <summary>
        /// Fired when multiple messages are deleted at once.
        /// </summary>
        public event AsyncEventHandler<MessageBulkDeleteEventArgs> MessagesBulkDeleted
        {
            add => this._messageBulkDeleted.Register(value);
            remove => this._messageBulkDeleted.Unregister(value);
        }
        private AsyncEvent<MessageBulkDeleteEventArgs> _messageBulkDeleted;

        /// <summary>
        /// Fired when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<TypingStartEventArgs> TypingStarted
        {
            add => this._typingStarted.Register(value);
            remove => this._typingStarted.Unregister(value);
        }
        private AsyncEvent<TypingStartEventArgs> _typingStarted;

        /// <summary>
        /// Fired when the current user updates their settings.
        /// </summary>
        public event AsyncEventHandler<UserSettingsUpdateEventArgs> UserSettingsUpdated
        {
            add => this._userSettingsUpdated.Register(value);
            remove => this._userSettingsUpdated.Unregister(value);
        }
        private AsyncEvent<UserSettingsUpdateEventArgs> _userSettingsUpdated;

        /// <summary>
        /// Fired when properties about the user change.
        /// </summary>
        public event AsyncEventHandler<UserUpdateEventArgs> UserUpdated
        {
            add => this._userUpdated.Register(value);
            remove => this._userUpdated.Unregister(value);
        }
        private AsyncEvent<UserUpdateEventArgs> _userUpdated;

        /// <summary>
        /// Fired when someone joins/leaves/moves voice channels.
        /// </summary>
        public event AsyncEventHandler<VoiceStateUpdateEventArgs> VoiceStateUpdated
        {
            add => this._voiceStateUpdated.Register(value);
            remove => this._voiceStateUpdated.Unregister(value);
        }
        private AsyncEvent<VoiceStateUpdateEventArgs> _voiceStateUpdated;

        /// <summary>
        /// Fired when a guild's voice server is updated.
        /// </summary>
        public event AsyncEventHandler<VoiceServerUpdateEventArgs> VoiceServerUpdated
        {
            add => this._voiceServerUpdated.Register(value);
            remove => this._voiceServerUpdated.Unregister(value);
        }
        private AsyncEvent<VoiceServerUpdateEventArgs> _voiceServerUpdated;

        /// <summary>
        /// Fired in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<GuildMembersChunkEventArgs> GuildMembersChunked
        {
            add => this._guildMembersChunk.Register(value);
            remove => this._guildMembersChunk.Unregister(value);
        }
        private AsyncEvent<GuildMembersChunkEventArgs> _guildMembersChunk;

        /// <summary>
        /// Fired when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<UnknownEventArgs> UnknownEvent
        {
            add => this._unknownEvent.Register(value);
            remove => this._unknownEvent.Unregister(value);
        }
        private AsyncEvent<UnknownEventArgs> _unknownEvent;

        /// <summary>
        /// Fired when a reaction gets added to a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionAddEventArgs> MessageReactionAdded
        {
            add => this._messageReactionAdded.Register(value);
            remove => this._messageReactionAdded.Unregister(value);
        }
        private AsyncEvent<MessageReactionAddEventArgs> _messageReactionAdded;

        /// <summary>
        /// Fired when a reaction gets removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEventArgs> MessageReactionRemoved
        {
            add => this._messageReactionRemoved.Register(value);
            remove => this._messageReactionRemoved.Unregister(value);
        }
        private AsyncEvent<MessageReactionRemoveEventArgs> _messageReactionRemoved;

        /// <summary>
        /// Fired when all reactions get removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionsClearEventArgs> MessageReactionsCleared
        {
            add => this._messageReactionsCleared.Register(value);
            remove => this._messageReactionsCleared.Unregister(value);
        }
        private AsyncEvent<MessageReactionsClearEventArgs> _messageReactionsCleared;

        /// <summary>
        /// Fired when all reactions of a specific reaction are removed from a message.
        /// </summary>
        public event AsyncEventHandler<MessageReactionRemoveEmojiEventArgs> MessageReactionRemovedEmoji
        {
            add => this._messageReactionRemovedEmoji.Register(value);
            remove => this._messageReactionRemovedEmoji.Unregister(value);
        }
        private AsyncEvent<MessageReactionRemoveEmojiEventArgs> _messageReactionRemovedEmoji;

        /// <summary>
        /// Fired whenever webhooks update.
        /// </summary>
        public event AsyncEventHandler<WebhooksUpdateEventArgs> WebhooksUpdated
        {
            add => this._webhooksUpdated.Register(value);
            remove => this._webhooksUpdated.Unregister(value);
        }
        private AsyncEvent<WebhooksUpdateEventArgs> _webhooksUpdated;

        /// <summary>
        /// Fired on received heartbeat ACK.
        /// </summary>
        public event AsyncEventHandler<HeartbeatEventArgs> Heartbeated
        {
            add => this._heartbeated.Register(value);
            remove => this._heartbeated.Unregister(value);
        }
        private AsyncEvent<HeartbeatEventArgs> _heartbeated;

        internal void EventErrorHandler(string evname, Exception ex)
        {
            this.DebugLogger.LogMessage(LogLevel.Error, "DSharpPlus", $"An {ex.GetType()} occured in {evname}.", DateTime.Now);
            this._clientErrored.InvokeAsync(new ClientErrorEventArgs(null) { EventName = evname, Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();
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
        public IReadOnlyDictionary<int, DiscordClient> ShardClients 
            => new ReadOnlyDictionary<int, DiscordClient>(this.Shards);

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public DiscordUser CurrentUser 
            => this._currentUser;

        private DiscordUser _currentUser;

        /// <summary>
        /// Gets the current application.
        /// </summary>
        public DiscordApplication CurrentApplication 
            => this._currentApplication;

        private DiscordApplication _currentApplication;

        /// <summary>
        /// Gets the list of available voice regions. Note that this property will not contain VIP voice regions.
        /// </summary>
        public IReadOnlyDictionary<string, DiscordVoiceRegion> VoiceRegions 
            => this._voiceRegionsLazy?.Value;

        /// <summary>
        /// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
        /// </summary>
        private ConcurrentDictionary<string, DiscordVoiceRegion> InternalVoiceRegions { get; set; }
        private Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> _voiceRegionsLazy;

        /// <summary>
        /// Initializes new auto-sharding Discord client.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        public DiscordShardedClient(DiscordConfiguration config)
        {
            this._clientErrored = new AsyncEvent<ClientErrorEventArgs>(this.Goof, "CLIENT_ERRORED");
            this._socketErrored = new AsyncEvent<SocketErrorEventArgs>(this.Goof, "SOCKET_ERRORED");
            this._socketOpened = new AsyncEvent(this.EventErrorHandler, "SOCKET_OPENED");
            this._socketClosed = new AsyncEvent<SocketCloseEventArgs>(this.EventErrorHandler, "SOCKET_CLOSED");
            this._ready = new AsyncEvent<ReadyEventArgs>(this.EventErrorHandler, "READY");
            this._resumed = new AsyncEvent<ReadyEventArgs>(this.EventErrorHandler, "RESUMED");
            this._channelCreated = new AsyncEvent<ChannelCreateEventArgs>(this.EventErrorHandler, "CHANNEL_CREATED");
            this._dmChannelCreated = new AsyncEvent<DmChannelCreateEventArgs>(this.EventErrorHandler, "DM_CHANNEL_CREATED");
            this._channelUpdated = new AsyncEvent<ChannelUpdateEventArgs>(this.EventErrorHandler, "CHANNEL_UPDATED");
            this._channelDeleted = new AsyncEvent<ChannelDeleteEventArgs>(this.EventErrorHandler, "CHANNEL_DELETED");
            this._dmChannelDeleted = new AsyncEvent<DmChannelDeleteEventArgs>(this.EventErrorHandler, "DM_CHANNEL_DELETED");
            this._channelPinsUpdated = new AsyncEvent<ChannelPinsUpdateEventArgs>(this.EventErrorHandler, "CHANNEL_PINS_UPDATED");
            this._guildCreated = new AsyncEvent<GuildCreateEventArgs>(this.EventErrorHandler, "GUILD_CREATED");
            this._guildAvailable = new AsyncEvent<GuildCreateEventArgs>(this.EventErrorHandler, "GUILD_AVAILABLE");
            this._guildUpdated = new AsyncEvent<GuildUpdateEventArgs>(this.EventErrorHandler, "GUILD_UPDATED");
            this._guildDeleted = new AsyncEvent<GuildDeleteEventArgs>(this.EventErrorHandler, "GUILD_DELETED");
            this._guildUnavailable = new AsyncEvent<GuildDeleteEventArgs>(this.EventErrorHandler, "GUILD_UNAVAILABLE");
            this._guildDownloadCompleted = new AsyncEvent<GuildDownloadCompletedEventArgs>(this.EventErrorHandler, "GUILD_DOWNLOAD_COMPLETED");
            this._inviteCreated = new AsyncEvent<InviteCreateEventArgs>(this.EventErrorHandler, "INVITE_CREATED");
            this._inviteDeleted = new AsyncEvent<InviteDeleteEventArgs>(this.EventErrorHandler, "INVITE_DELETED");
            this._messageCreated = new AsyncEvent<MessageCreateEventArgs>(this.EventErrorHandler, "MESSAGE_CREATED");
            this._presenceUpdated = new AsyncEvent<PresenceUpdateEventArgs>(this.EventErrorHandler, "PRESENCE_UPDATED");
            this._guildBanAdded = new AsyncEvent<GuildBanAddEventArgs>(this.EventErrorHandler, "GUILD_BAN_ADDED");
            this._guildBanRemoved = new AsyncEvent<GuildBanRemoveEventArgs>(this.EventErrorHandler, "GUILD_BAN_REMOVED");
            this._guildEmojisUpdated = new AsyncEvent<GuildEmojisUpdateEventArgs>(this.EventErrorHandler, "GUILD_EMOJI_UPDATED");
            this._guildIntegrationsUpdated = new AsyncEvent<GuildIntegrationsUpdateEventArgs>(this.EventErrorHandler, "GUILD_INTEGRATIONS_UPDATED");
            this._guildMemberAdded = new AsyncEvent<GuildMemberAddEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_ADDED");
            this._guildMemberRemoved = new AsyncEvent<GuildMemberRemoveEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_REMOVED");
            this._guildMemberUpdated = new AsyncEvent<GuildMemberUpdateEventArgs>(this.EventErrorHandler, "GUILD_MEMBER_UPDATED");
            this._guildRoleCreated = new AsyncEvent<GuildRoleCreateEventArgs>(this.EventErrorHandler, "GUILD_ROLE_CREATED");
            this._guildRoleUpdated = new AsyncEvent<GuildRoleUpdateEventArgs>(this.EventErrorHandler, "GUILD_ROLE_UPDATED");
            this._guildRoleDeleted = new AsyncEvent<GuildRoleDeleteEventArgs>(this.EventErrorHandler, "GUILD_ROLE_DELETED");
            this._messageUpdated = new AsyncEvent<MessageUpdateEventArgs>(this.EventErrorHandler, "MESSAGE_UPDATED");
            this._messageDeleted = new AsyncEvent<MessageDeleteEventArgs>(this.EventErrorHandler, "MESSAGE_DELETED");
            this._messageBulkDeleted = new AsyncEvent<MessageBulkDeleteEventArgs>(this.EventErrorHandler, "MESSAGE_BULK_DELETED");
            this._typingStarted = new AsyncEvent<TypingStartEventArgs>(this.EventErrorHandler, "TYPING_STARTED");
            this._userSettingsUpdated = new AsyncEvent<UserSettingsUpdateEventArgs>(this.EventErrorHandler, "USER_SETTINGS_UPDATED");
            this._userUpdated = new AsyncEvent<UserUpdateEventArgs>(this.EventErrorHandler, "USER_UPDATED");
            this._voiceStateUpdated = new AsyncEvent<VoiceStateUpdateEventArgs>(this.EventErrorHandler, "VOICE_STATE_UPDATED");
            this._voiceServerUpdated = new AsyncEvent<VoiceServerUpdateEventArgs>(this.EventErrorHandler, "VOICE_SERVER_UPDATED");
            this._guildMembersChunk = new AsyncEvent<GuildMembersChunkEventArgs>(this.EventErrorHandler, "GUILD_MEMBERS_CHUNKED");
            this._unknownEvent = new AsyncEvent<UnknownEventArgs>(this.EventErrorHandler, "UNKNOWN_EVENT");
            this._messageReactionAdded = new AsyncEvent<MessageReactionAddEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_ADDED");
            this._messageReactionRemoved = new AsyncEvent<MessageReactionRemoveEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_REMOVED");
            this._messageReactionsCleared = new AsyncEvent<MessageReactionsClearEventArgs>(this.EventErrorHandler, "MESSAGE_REACTIONS_CLEARED");
            this._messageReactionRemovedEmoji = new AsyncEvent<MessageReactionRemoveEmojiEventArgs>(this.EventErrorHandler, "MESSAGE_REACTION_REMOVED_EMOJI");
            this._webhooksUpdated = new AsyncEvent<WebhooksUpdateEventArgs>(this.EventErrorHandler, "WEBHOOKS_UPDATED");
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

            var shardc = this.Config.ShardCount == 1 ? await this.GetShardCountAsync().ConfigureAwait(false) : this.Config.ShardCount;
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
            var shardc = await this.InitializeShardsAsync().ConfigureAwait(false);
            this.DebugLogger.LogMessage(LogLevel.Info, "Autoshard", $"Booting {shardc.ToString(CultureInfo.InvariantCulture)} shards", DateTime.Now);

            for (var i = 0; i < shardc; i++)
            {
                if (!this.Shards.TryGetValue(i, out var client))
                    throw new Exception("Could not initialize shards");

                if (this.CurrentUser != null)
                    client.CurrentUser = this.CurrentUser;

                if (this._currentApplication != null)
                    client.CurrentApplication = this.CurrentApplication;

                if (this.InternalVoiceRegions != null)
                {
                    client.InternalVoiceRegions = this.InternalVoiceRegions;
                    client._voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(client.InternalVoiceRegions));
                }

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
                client.GuildDownloadCompleted += this.Client_GuildDownloadCompleted;
                client.InviteCreated += this.Client_InviteCreated;
                client.InviteDeleted += this.Client_InviteDeleted;
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
                client.MessageReactionRemovedEmoji += this.Client_MessageReactionRemovedEmoji;
                client.WebhooksUpdated += this.Client_WebhooksUpdate;
                client.Heartbeated += this.Client_HeartBeated;
                client.DebugLogger.LogMessageReceived += this.DebugLogger_LogMessageReceived;
                
                await client.ConnectAsync().ConfigureAwait(false);
                this.DebugLogger.LogMessage(LogLevel.Info, "Autoshard", $"Booted shard {i.ToString(CultureInfo.InvariantCulture)}", DateTime.Now);

                if (this._currentUser == null)
                    this._currentUser = client.CurrentUser;

                if (this._currentApplication == null)
                    this._currentApplication = client.CurrentApplication;

                if (this.InternalVoiceRegions == null)
                {
                    this.InternalVoiceRegions = client.InternalVoiceRegions;
                    this._voiceRegionsLazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this.InternalVoiceRegions));
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
            foreach (var client in this.ShardClients.Values)
                tasks.Add(client.UpdateStatusAsync(activity, userStatus, idleSince));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task<int> GetShardCountAsync()
        {
            string url = $"{Utilities.GetApiBaseUri()}{Endpoints.GATEWAY}{Endpoints.BOT}";
            var headers = Utilities.GetBaseHeaders();

            var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
            http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(this.Config));
            var resp = await http.GetAsync(url).ConfigureAwait(false);
            http.Dispose();

            var jo = JObject.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
            if (jo["shards"] != null)
                return jo.Value<int>("shards");
            return 1;
        }

        #region Event Dispatchers
        private Task Client_ClientError(ClientErrorEventArgs e) 
            => this._clientErrored.InvokeAsync(e);

        private Task Client_SocketError(SocketErrorEventArgs e) 
            => this._socketErrored.InvokeAsync(e);

        private Task Client_SocketOpened() 
            => this._socketOpened.InvokeAsync();

        private Task Client_SocketClosed(SocketCloseEventArgs e) 
            => this._socketClosed.InvokeAsync(e);

        private Task Client_Ready(ReadyEventArgs e) 
            => this._ready.InvokeAsync(e);

        private Task Client_Resumed(ReadyEventArgs e) 
            => this._resumed.InvokeAsync(e);

        private Task Client_ChannelCreated(ChannelCreateEventArgs e) 
            => this._channelCreated.InvokeAsync(e);

        private Task Client_DMChannelCreated(DmChannelCreateEventArgs e) 
            => this._dmChannelCreated.InvokeAsync(e);

        private Task Client_ChannelUpdated(ChannelUpdateEventArgs e) 
            => this._channelUpdated.InvokeAsync(e);

        private Task Client_ChannelDeleted(ChannelDeleteEventArgs e) 
            => this._channelDeleted.InvokeAsync(e);

        private Task Client_DMChannelDeleted(DmChannelDeleteEventArgs e) 
            => this._dmChannelDeleted.InvokeAsync(e);

        private Task Client_ChannelPinsUpdated(ChannelPinsUpdateEventArgs e) 
            => this._channelPinsUpdated.InvokeAsync(e);

        private Task Client_GuildCreated(GuildCreateEventArgs e) 
            => this._guildCreated.InvokeAsync(e);

        private Task Client_GuildAvailable(GuildCreateEventArgs e) 
            => this._guildAvailable.InvokeAsync(e);

        private Task Client_GuildUpdated(GuildUpdateEventArgs e) 
            => this._guildUpdated.InvokeAsync(e);

        private Task Client_GuildDeleted(GuildDeleteEventArgs e) 
            => this._guildDeleted.InvokeAsync(e);

        private Task Client_GuildUnavailable(GuildDeleteEventArgs e) 
            => this._guildUnavailable.InvokeAsync(e);

        private Task Client_GuildDownloadCompleted(GuildDownloadCompletedEventArgs e)
            => this._guildDownloadCompleted.InvokeAsync(e);

        private Task Client_MessageCreated(MessageCreateEventArgs e) 
            => this._messageCreated.InvokeAsync(e);

        private Task Client_InviteCreated(InviteCreateEventArgs e)
            => this._inviteCreated.InvokeAsync(e);

        private Task Client_InviteDeleted(InviteDeleteEventArgs e)
            => this._inviteDeleted.InvokeAsync(e);

        private Task Client_PresenceUpdate(PresenceUpdateEventArgs e) 
            => this._presenceUpdated.InvokeAsync(e);

        private Task Client_GuildBanAdd(GuildBanAddEventArgs e) 
            => this._guildBanAdded.InvokeAsync(e);

        private Task Client_GuildBanRemove(GuildBanRemoveEventArgs e) 
            => this._guildBanRemoved.InvokeAsync(e);

        private Task Client_GuildEmojisUpdate(GuildEmojisUpdateEventArgs e) 
            => this._guildEmojisUpdated.InvokeAsync(e);

        private Task Client_GuildIntegrationsUpdate(GuildIntegrationsUpdateEventArgs e) 
            => this._guildIntegrationsUpdated.InvokeAsync(e);

        private Task Client_GuildMemberAdd(GuildMemberAddEventArgs e) 
            => this._guildMemberAdded.InvokeAsync(e);

        private Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e) 
            => this._guildMemberRemoved.InvokeAsync(e);

        private Task Client_GuildMemberUpdate(GuildMemberUpdateEventArgs e) 
            => this._guildMemberUpdated.InvokeAsync(e);

        private Task Client_GuildRoleCreate(GuildRoleCreateEventArgs e) 
            => this._guildRoleCreated.InvokeAsync(e);

        private Task Client_GuildRoleUpdate(GuildRoleUpdateEventArgs e) 
            => this._guildRoleUpdated.InvokeAsync(e);

        private Task Client_GuildRoleDelete(GuildRoleDeleteEventArgs e) 
            => this._guildRoleDeleted.InvokeAsync(e);

        private Task Client_MessageUpdate(MessageUpdateEventArgs e) 
            => this._messageUpdated.InvokeAsync(e);

        private Task Client_MessageDelete(MessageDeleteEventArgs e) 
            => this._messageDeleted.InvokeAsync(e);

        private Task Client_MessageBulkDelete(MessageBulkDeleteEventArgs e) 
            => this._messageBulkDeleted.InvokeAsync(e);

        private Task Client_TypingStart(TypingStartEventArgs e) 
            => this._typingStarted.InvokeAsync(e);

        private Task Client_UserSettingsUpdate(UserSettingsUpdateEventArgs e) 
            => this._userSettingsUpdated.InvokeAsync(e);

        private Task Client_UserUpdate(UserUpdateEventArgs e) 
            => this._userUpdated.InvokeAsync(e);

        private Task Client_VoiceStateUpdate(VoiceStateUpdateEventArgs e) 
            => this._voiceStateUpdated.InvokeAsync(e);

        private Task Client_VoiceServerUpdate(VoiceServerUpdateEventArgs e) 
            => this._voiceServerUpdated.InvokeAsync(e);

        private Task Client_GuildMembersChunk(GuildMembersChunkEventArgs e) 
            => this._guildMembersChunk.InvokeAsync(e);

        private Task Client_UnknownEvent(UnknownEventArgs e) 
            => this._unknownEvent.InvokeAsync(e);

        private Task Client_MessageReactionAdd(MessageReactionAddEventArgs e) 
            => this._messageReactionAdded.InvokeAsync(e);

        private Task Client_MessageReactionRemove(MessageReactionRemoveEventArgs e) 
            => this._messageReactionRemoved.InvokeAsync(e);

        private Task Client_MessageReactionRemoveAll(MessageReactionsClearEventArgs e) 
            => this._messageReactionsCleared.InvokeAsync(e);

        private Task Client_MessageReactionRemovedEmoji(MessageReactionRemoveEmojiEventArgs e)
            => this._messageReactionRemovedEmoji.InvokeAsync(e);

        private Task Client_WebhooksUpdate(WebhooksUpdateEventArgs e) 
            => this._webhooksUpdated.InvokeAsync(e);

        private Task Client_HeartBeated(HeartbeatEventArgs e) 
            => this._heartbeated.InvokeAsync(e);

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e) 
            => this.DebugLogger.LogMessage(e.Level, e.Application, e.Message, e.Timestamp);
        #endregion
    }
}
