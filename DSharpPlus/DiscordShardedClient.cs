#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;
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
        /// Fired whenever an error occurs within an event handler.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ClientErrorEventArgs> ClientErrored
        {
            add => this._clientErrored.Register(value);
            remove => this._clientErrored.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ClientErrorEventArgs> _clientErrored;

        /// <summary>
        /// Fired whenever a WebSocket error occurs within the client.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, SocketErrorEventArgs> SocketErrored
        {
            add => this._socketErrored.Register(value);
            remove => this._socketErrored.Unregister(value);
        }
        private AsyncEvent<DiscordClient, SocketErrorEventArgs> _socketErrored;

        /// <summary>
        /// Fired whenever WebSocket connection is established.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, SocketEventArgs> SocketOpened
        {
            add => this._socketOpened.Register(value);
            remove => this._socketOpened.Unregister(value);
        }
        private AsyncEvent<DiscordClient, SocketEventArgs> _socketOpened;

        /// <summary>
        /// Fired whenever WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, SocketCloseEventArgs> SocketClosed
        {
            add => this._socketClosed.Register(value);
            remove => this._socketClosed.Unregister(value);
        }
        private AsyncEvent<DiscordClient, SocketCloseEventArgs> _socketClosed;

        /// <summary>
        /// Fired when the client enters ready state.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ReadyEventArgs> Ready
        {
            add => this._ready.Register(value);
            remove => this._ready.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ReadyEventArgs> _ready;

        /// <summary>
        /// Fired whenever a session is resumed.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ReadyEventArgs> Resumed
        {
            add => this._resumed.Register(value);
            remove => this._resumed.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ReadyEventArgs> _resumed;

        /// <summary>
        /// Fired when a new channel is created.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ChannelCreateEventArgs> ChannelCreated
        {
            add => this._channelCreated.Register(value);
            remove => this._channelCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ChannelCreateEventArgs> _channelCreated;

        /// <summary>
        /// Fired when a new direct message channel is created.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, DmChannelCreateEventArgs> DmChannelCreated
        {
            add => this._dmChannelCreated.Register(value);
            remove => this._dmChannelCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, DmChannelCreateEventArgs> _dmChannelCreated;

        /// <summary>
        /// Fired when a channel is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ChannelUpdateEventArgs> ChannelUpdated
        {
            add => this._channelUpdated.Register(value);
            remove => this._channelUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ChannelUpdateEventArgs> _channelUpdated;

        /// <summary>
        /// Fired when a channel is deleted
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ChannelDeleteEventArgs> ChannelDeleted
        {
            add => this._channelDeleted.Register(value);
            remove => this._channelDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ChannelDeleteEventArgs> _channelDeleted;

        /// <summary>
        /// Fired when a dm channel is deleted
        /// </summary>
        public event AsyncEventHandler<DiscordClient, DmChannelDeleteEventArgs> DmChannelDeleted
        {
            add => this._dmChannelDeleted.Register(value);
            remove => this._dmChannelDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, DmChannelDeleteEventArgs> _dmChannelDeleted;

        /// <summary>
        /// Fired whenever a channel's pinned message list is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ChannelPinsUpdateEventArgs> ChannelPinsUpdated
        {
            add => this._channelPinsUpdated.Register(value);
            remove => this._channelPinsUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs> _channelPinsUpdated;

        /// <summary>
        /// Fired when the user joins a new guild.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildCreateEventArgs> GuildCreated
        {
            add => this._guildCreated.Register(value);
            remove => this._guildCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildCreateEventArgs> _guildCreated;

        /// <summary>
        /// Fired when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildCreateEventArgs> GuildAvailable
        {
            add => this._guildAvailable.Register(value);
            remove => this._guildAvailable.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildCreateEventArgs> _guildAvailable;

        /// <summary>
        /// Fired when a guild is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildUpdateEventArgs> GuildUpdated
        {
            add => this._guildUpdated.Register(value);
            remove => this._guildUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildUpdateEventArgs> _guildUpdated;

        /// <summary>
        /// Fired when the user leaves or is removed from a guild.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildDeleteEventArgs> GuildDeleted
        {
            add => this._guildDeleted.Register(value);
            remove => this._guildDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildDeleteEventArgs> _guildDeleted;

        /// <summary>
        /// Fired when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildDeleteEventArgs> GuildUnavailable
        {
            add => this._guildUnavailable.Register(value);
            remove => this._guildUnavailable.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildDeleteEventArgs> _guildUnavailable;

        /// <summary>
        /// Fired when all guilds finish streaming from Discord.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildDownloadCompletedEventArgs> GuildDownloadCompleted
        {
            add => this._guildDownloadCompleted.Register(value);
            remove => this._guildDownloadCompleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs> _guildDownloadCompleted;

        /// <summary>
        /// Fired when an invite is created.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, InviteCreateEventArgs> InviteCreated
        {
            add => this._inviteCreated.Register(value);
            remove => this._inviteCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, InviteCreateEventArgs> _inviteCreated;

        /// <summary>
        /// Fired when an invite is deleted.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, InviteDeleteEventArgs> InviteDeleted
        {
            add => this._inviteDeleted.Register(value);
            remove => this._inviteDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, InviteDeleteEventArgs> _inviteDeleted;

        /// <summary>
        /// Fired when a message is created.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageCreateEventArgs> MessageCreated
        {
            add => this._messageCreated.Register(value);
            remove => this._messageCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageCreateEventArgs> _messageCreated;

        /// <summary>
        /// Fired when a presence has been updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, PresenceUpdateEventArgs> PresenceUpdated
        {
            add => this._presenceUpdated.Register(value);
            remove => this._presenceUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, PresenceUpdateEventArgs> _presenceUpdated;

        /// <summary>
        /// Fired when a guild ban gets added
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildBanAddEventArgs> GuildBanAdded
        {
            add => this._guildBanAdded.Register(value);
            remove => this._guildBanAdded.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildBanAddEventArgs> _guildBanAdded;

        /// <summary>
        /// Fired when a guild ban gets removed
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildBanRemoveEventArgs> GuildBanRemoved
        {
            add => this._guildBanRemoved.Register(value);
            remove => this._guildBanRemoved.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildBanRemoveEventArgs> _guildBanRemoved;

        /// <summary>
        /// Fired when a guilds emojis get updated
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildEmojisUpdateEventArgs> GuildEmojisUpdated
        {
            add => this._guildEmojisUpdated.Register(value);
            remove => this._guildEmojisUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs> _guildEmojisUpdated;

        /// <summary>
        /// Fired when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
        {
            add => this._guildIntegrationsUpdated.Register(value);
            remove => this._guildIntegrationsUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs> _guildIntegrationsUpdated;

        /// <summary>
        /// Fired when a new user joins a guild.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildMemberAddEventArgs> GuildMemberAdded
        {
            add => this._guildMemberAdded.Register(value);
            remove => this._guildMemberAdded.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildMemberAddEventArgs> _guildMemberAdded;

        /// <summary>
        /// Fired when a user is removed from a guild (leave/kick/ban).
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildMemberRemoveEventArgs> GuildMemberRemoved
        {
            add => this._guildMemberRemoved.Register(value);
            remove => this._guildMemberRemoved.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs> _guildMemberRemoved;

        /// <summary>
        /// Fired when a guild member is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildMemberUpdateEventArgs> GuildMemberUpdated
        {
            add => this._guildMemberUpdated.Register(value);
            remove => this._guildMemberUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs> _guildMemberUpdated;

        /// <summary>
        /// Fired when a guild role is created.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildRoleCreateEventArgs> GuildRoleCreated
        {
            add => this._guildRoleCreated.Register(value);
            remove => this._guildRoleCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildRoleCreateEventArgs> _guildRoleCreated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildRoleUpdateEventArgs> GuildRoleUpdated
        {
            add => this._guildRoleUpdated.Register(value);
            remove => this._guildRoleUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs> _guildRoleUpdated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildRoleDeleteEventArgs> GuildRoleDeleted
        {
            add => this._guildRoleDeleted.Register(value);
            remove => this._guildRoleDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs> _guildRoleDeleted;

        /// <summary>
        /// Fired when a message is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageUpdateEventArgs> MessageUpdated
        {
            add => this._messageUpdated.Register(value);
            remove => this._messageUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageUpdateEventArgs> _messageUpdated;

        /// <summary>
        /// Fired when a message is deleted.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageDeleteEventArgs> MessageDeleted
        {
            add => this._messageDeleted.Register(value);
            remove => this._messageDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageDeleteEventArgs> _messageDeleted;

        /// <summary>
        /// Fired when multiple messages are deleted at once.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageBulkDeleteEventArgs> MessagesBulkDeleted
        {
            add => this._messageBulkDeleted.Register(value);
            remove => this._messageBulkDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs> _messageBulkDeleted;

        /// <summary>
        /// Fired when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, TypingStartEventArgs> TypingStarted
        {
            add => this._typingStarted.Register(value);
            remove => this._typingStarted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, TypingStartEventArgs> _typingStarted;

        /// <summary>
        /// Fired when the current user updates their settings.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, UserSettingsUpdateEventArgs> UserSettingsUpdated
        {
            add => this._userSettingsUpdated.Register(value);
            remove => this._userSettingsUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs> _userSettingsUpdated;

        /// <summary>
        /// Fired when properties about the user change.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, UserUpdateEventArgs> UserUpdated
        {
            add => this._userUpdated.Register(value);
            remove => this._userUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, UserUpdateEventArgs> _userUpdated;

        /// <summary>
        /// Fired when someone joins/leaves/moves voice channels.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, VoiceStateUpdateEventArgs> VoiceStateUpdated
        {
            add => this._voiceStateUpdated.Register(value);
            remove => this._voiceStateUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs> _voiceStateUpdated;

        /// <summary>
        /// Fired when a guild's voice server is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, VoiceServerUpdateEventArgs> VoiceServerUpdated
        {
            add => this._voiceServerUpdated.Register(value);
            remove => this._voiceServerUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs> _voiceServerUpdated;

        /// <summary>
        /// Fired in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildMembersChunkEventArgs> GuildMembersChunked
        {
            add => this._guildMembersChunk.Register(value);
            remove => this._guildMembersChunk.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildMembersChunkEventArgs> _guildMembersChunk;

        /// <summary>
        /// Fired when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, UnknownEventArgs> UnknownEvent
        {
            add => this._unknownEvent.Register(value);
            remove => this._unknownEvent.Unregister(value);
        }
        private AsyncEvent<DiscordClient, UnknownEventArgs> _unknownEvent;

        /// <summary>
        /// Fired when a reaction gets added to a message.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs> MessageReactionAdded
        {
            add => this._messageReactionAdded.Register(value);
            remove => this._messageReactionAdded.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageReactionAddEventArgs> _messageReactionAdded;

        /// <summary>
        /// Fired when a reaction gets removed from a message.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs> MessageReactionRemoved
        {
            add => this._messageReactionRemoved.Register(value);
            remove => this._messageReactionRemoved.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs> _messageReactionRemoved;

        /// <summary>
        /// Fired when all reactions get removed from a message.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageReactionsClearEventArgs> MessageReactionsCleared
        {
            add => this._messageReactionsCleared.Register(value);
            remove => this._messageReactionsCleared.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageReactionsClearEventArgs> _messageReactionsCleared;

        /// <summary>
        /// Fired when all reactions of a specific reaction are removed from a message.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageReactionRemoveEmojiEventArgs> MessageReactionRemovedEmoji
        {
            add => this._messageReactionRemovedEmoji.Register(value);
            remove => this._messageReactionRemovedEmoji.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs> _messageReactionRemovedEmoji;

        /// <summary>
        /// Fired whenever webhooks update.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, WebhooksUpdateEventArgs> WebhooksUpdated
        {
            add => this._webhooksUpdated.Register(value);
            remove => this._webhooksUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, WebhooksUpdateEventArgs> _webhooksUpdated;

        /// <summary>
        /// Fired on received heartbeat ACK.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, HeartbeatEventArgs> Heartbeated
        {
            add => this._heartbeated.Register(value);
            remove => this._heartbeated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, HeartbeatEventArgs> _heartbeated;

        internal void EventErrorHandler<TArgs>(AsyncEvent<DiscordClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<DiscordClient, TArgs> handler, DiscordClient sender, TArgs eventArgs)
            where TArgs : AsyncEventArgs
        {
            this.Logger.LogError(LoggerEvents.EventHandlerException, ex, "Event handler exception for event {0} thrown from {1} (defined in {2})", asyncEvent.Name, handler.Method, handler.Method.DeclaringType);
            this._clientErrored.InvokeAsync(sender, new ClientErrorEventArgs(sender) { EventName = asyncEvent.Name, Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void Goof<TArgs>(AsyncEvent<DiscordClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<DiscordClient, TArgs> handler, DiscordClient sender, TArgs eventArgs)
            where TArgs : AsyncEventArgs
        {
            this.Logger.LogCritical(LoggerEvents.EventHandlerException, ex, "Exception event handler {0} (defined in {1}) threw an exception", handler.Method, handler.Method.DeclaringType);
        }
        #endregion

        private DiscordConfiguration Configuration { get; }
        private ConcurrentDictionary<int, DiscordClient> Shards { get; }

        /// <summary>
        /// Gets the logger for this client.
        /// </summary>
        public ILogger<BaseDiscordClient> Logger { get; }

        /// <summary>
        /// Gets all client shards.
        /// </summary>
        public IReadOnlyDictionary<int, DiscordClient> ShardClients 
            => new ReadOnlyDictionary<int, DiscordClient>(this.Shards);

        /// <summary>
        /// Gets the gateway info for the client's session.
        /// </summary>
        public GatewayInfo GatewayInfo { get; private set; }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public DiscordUser CurrentUser { get; private set; }

        /// <summary>
        /// Gets the current application.
        /// </summary>
        public DiscordApplication CurrentApplication { get; private set; }

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
        private bool isStarted = false;

        /// <summary>
        /// Initializes new auto-sharding Discord client.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        public DiscordShardedClient(DiscordConfiguration config)
        {
            this._clientErrored = new AsyncEvent<DiscordClient, ClientErrorEventArgs>("CLIENT_ERRORED", DiscordClient.EventExecutionLimit, this.Goof);
            this._socketErrored = new AsyncEvent<DiscordClient, SocketErrorEventArgs>("SOCKET_ERRORED", DiscordClient.EventExecutionLimit, this.Goof);
            this._socketOpened = new AsyncEvent<DiscordClient, SocketEventArgs>("SOCKET_OPENED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._socketClosed = new AsyncEvent<DiscordClient, SocketCloseEventArgs>("SOCKET_CLOSED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._ready = new AsyncEvent<DiscordClient, ReadyEventArgs>("READY", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._resumed = new AsyncEvent<DiscordClient, ReadyEventArgs>("RESUMED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._channelCreated = new AsyncEvent<DiscordClient, ChannelCreateEventArgs>("CHANNEL_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._dmChannelCreated = new AsyncEvent<DiscordClient, DmChannelCreateEventArgs>("DM_CHANNEL_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._channelUpdated = new AsyncEvent<DiscordClient, ChannelUpdateEventArgs>("CHANNEL_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._channelDeleted = new AsyncEvent<DiscordClient, ChannelDeleteEventArgs>("CHANNEL_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._dmChannelDeleted = new AsyncEvent<DiscordClient, DmChannelDeleteEventArgs>("DM_CHANNEL_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._channelPinsUpdated = new AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs>("CHANNEL_PINS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildCreated = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildAvailable = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_AVAILABLE", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildUpdated = new AsyncEvent<DiscordClient, GuildUpdateEventArgs>("GUILD_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildDeleted = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildUnavailable = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_UNAVAILABLE", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildDownloadCompleted = new AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs>("GUILD_DOWNLOAD_COMPLETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._inviteCreated = new AsyncEvent<DiscordClient, InviteCreateEventArgs>("INVITE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._inviteDeleted = new AsyncEvent<DiscordClient, InviteDeleteEventArgs>("INVITE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageCreated = new AsyncEvent<DiscordClient, MessageCreateEventArgs>("MESSAGE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._presenceUpdated = new AsyncEvent<DiscordClient, PresenceUpdateEventArgs>("PRESENCE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildBanAdded = new AsyncEvent<DiscordClient, GuildBanAddEventArgs>("GUILD_BAN_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildBanRemoved = new AsyncEvent<DiscordClient, GuildBanRemoveEventArgs>("GUILD_BAN_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildEmojisUpdated = new AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs>("GUILD_EMOJI_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildIntegrationsUpdated = new AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs>("GUILD_INTEGRATIONS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildMemberAdded = new AsyncEvent<DiscordClient, GuildMemberAddEventArgs>("GUILD_MEMBER_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildMemberRemoved = new AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs>("GUILD_MEMBER_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildMemberUpdated = new AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs>("GUILD_MEMBER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildRoleCreated = new AsyncEvent<DiscordClient, GuildRoleCreateEventArgs>("GUILD_ROLE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildRoleUpdated = new AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs>("GUILD_ROLE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildRoleDeleted = new AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs>("GUILD_ROLE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageUpdated = new AsyncEvent<DiscordClient, MessageUpdateEventArgs>("MESSAGE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageDeleted = new AsyncEvent<DiscordClient, MessageDeleteEventArgs>("MESSAGE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageBulkDeleted = new AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs>("MESSAGE_BULK_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._typingStarted = new AsyncEvent<DiscordClient, TypingStartEventArgs>("TYPING_STARTED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._userSettingsUpdated = new AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs>("USER_SETTINGS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._userUpdated = new AsyncEvent<DiscordClient, UserUpdateEventArgs>("USER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._voiceStateUpdated = new AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs>("VOICE_STATE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._voiceServerUpdated = new AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs>("VOICE_SERVER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildMembersChunk = new AsyncEvent<DiscordClient, GuildMembersChunkEventArgs>("GUILD_MEMBERS_CHUNKED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._unknownEvent = new AsyncEvent<DiscordClient, UnknownEventArgs>("UNKNOWN_EVENT", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionAdded = new AsyncEvent<DiscordClient, MessageReactionAddEventArgs>("MESSAGE_REACTION_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionRemoved = new AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs>("MESSAGE_REACTION_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionsCleared = new AsyncEvent<DiscordClient, MessageReactionsClearEventArgs>("MESSAGE_REACTIONS_CLEARED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionRemovedEmoji = new AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs>("MESSAGE_REACTION_REMOVED_EMOJI", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._webhooksUpdated = new AsyncEvent<DiscordClient, WebhooksUpdateEventArgs>("WEBHOOKS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._heartbeated = new AsyncEvent<DiscordClient, HeartbeatEventArgs>("HEARTBEATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);

            this.Configuration = config;
            this.Shards = new ConcurrentDictionary<int, DiscordClient>();

            if (this.Configuration.LoggerFactory == null)
            {
                this.Configuration.LoggerFactory = new DefaultLoggerFactory();
                this.Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this.Configuration.MinimumLogLevel, this.Configuration.LogTimestampFormat));
            }
            this.Logger = this.Configuration.LoggerFactory.CreateLogger<BaseDiscordClient>();
        }

        internal async Task<int> InitializeShardsAsync()
        {
            if (this.Shards.Count != 0)
                return this.Shards.Count;

            this.GatewayInfo = await this.GetGatewayInfoAsync().ConfigureAwait(false);
            var shardc = this.Configuration.ShardCount == 1 ? this.GatewayInfo.ShardCount : this.Configuration.ShardCount;
            var lf = new ShardedLoggerFactory(this.Logger);
            for (var i = 0; i < shardc; i++)
            {
                var cfg = new DiscordConfiguration(this.Configuration)
                {
                    ShardId = i,
                    ShardCount = shardc,
                    LoggerFactory = lf
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
            if (this.isStarted)
                throw new InvalidOperationException("This client has already been started.");

            if (this.Configuration.TokenType != TokenType.Bot)
                this.Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.");
            this.Logger.LogInformation(LoggerEvents.Startup, "DSharpPlus, version {0}", this._versionString.Value);

            var shardc = await this.InitializeShardsAsync().ConfigureAwait(false);
            var connectTasks = new List<Task>();
            this.Logger.LogInformation(LoggerEvents.ShardStartup, "Booting {0} shards", shardc);

            for (var i = 0; i < shardc; i++)
            {
                //This should never happen, but in case it does...
                if (this.GatewayInfo.SessionBucket.MaxConcurrency < 1)
                    this.GatewayInfo.SessionBucket.MaxConcurrency = 1;

                if (this.GatewayInfo.SessionBucket.MaxConcurrency == 1)
                    await this.ConnectShardAsync(i).ConfigureAwait(false);
                else
                {
                    //Concurrent login.
                    connectTasks.Add(this.ConnectShardAsync(i));

                    if (connectTasks.Count == this.GatewayInfo.SessionBucket.MaxConcurrency)
                    {
                        await Task.WhenAll(connectTasks).ConfigureAwait(false);
                        connectTasks.Clear();
                    }
                }
            }

            this.isStarted = true;
        }

        /// <summary>
        /// Disconnects and disposes of all shards.
        /// </summary>
        /// <returns></returns>
        public Task StopAsync()
        {
            if (!this.isStarted)
                throw new InvalidOperationException("This client has not been started.");

            this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disposing {0} shards.", this.Shards.Count);
            this.isStarted = false;
            this.GatewayInfo = null;
            this.CurrentUser = null;
            this.CurrentApplication = null;
            this._voiceRegionsLazy = null;

            for (int i = 0; i < this.Shards.Count; i++)
            {
                if (this.Shards.TryGetValue(i, out var client))
                {
                    client.ClientErrored -= this.Client_ClientError;
                    client.SocketErrored -= this.Client_SocketError;
                    client.SocketOpened -= this.Client_SocketOpened;
                    client.SocketClosed -= this.Client_SocketClosed;
                    client.Ready -= this.Client_Ready;
                    client.Resumed -= this.Client_Resumed;
                    client.ChannelCreated -= this.Client_ChannelCreated;
                    client.DmChannelCreated -= this.Client_DMChannelCreated;
                    client.ChannelUpdated -= this.Client_ChannelUpdated;
                    client.ChannelDeleted -= this.Client_ChannelDeleted;
                    client.DmChannelDeleted -= this.Client_DMChannelDeleted;
                    client.ChannelPinsUpdated -= this.Client_ChannelPinsUpdated;
                    client.GuildCreated -= this.Client_GuildCreated;
                    client.GuildAvailable -= this.Client_GuildAvailable;
                    client.GuildUpdated -= this.Client_GuildUpdated;
                    client.GuildDeleted -= this.Client_GuildDeleted;
                    client.GuildUnavailable -= this.Client_GuildUnavailable;
                    client.GuildDownloadCompleted -= this.Client_GuildDownloadCompleted;
                    client.InviteCreated -= this.Client_InviteCreated;
                    client.InviteDeleted -= this.Client_InviteDeleted;
                    client.MessageCreated -= this.Client_MessageCreated;
                    client.PresenceUpdated -= this.Client_PresenceUpdate;
                    client.GuildBanAdded -= this.Client_GuildBanAdd;
                    client.GuildBanRemoved -= this.Client_GuildBanRemove;
                    client.GuildEmojisUpdated -= this.Client_GuildEmojisUpdate;
                    client.GuildIntegrationsUpdated -= this.Client_GuildIntegrationsUpdate;
                    client.GuildMemberAdded -= this.Client_GuildMemberAdd;
                    client.GuildMemberRemoved -= this.Client_GuildMemberRemove;
                    client.GuildMemberUpdated -= this.Client_GuildMemberUpdate;
                    client.GuildRoleCreated -= this.Client_GuildRoleCreate;
                    client.GuildRoleUpdated -= this.Client_GuildRoleUpdate;
                    client.GuildRoleDeleted -= this.Client_GuildRoleDelete;
                    client.MessageUpdated -= this.Client_MessageUpdate;
                    client.MessageDeleted -= this.Client_MessageDelete;
                    client.MessagesBulkDeleted -= this.Client_MessageBulkDelete;
                    client.TypingStarted -= this.Client_TypingStart;
                    client.UserSettingsUpdated -= this.Client_UserSettingsUpdate;
                    client.UserUpdated -= this.Client_UserUpdate;
                    client.VoiceStateUpdated -= this.Client_VoiceStateUpdate;
                    client.VoiceServerUpdated -= this.Client_VoiceServerUpdate;
                    client.GuildMembersChunked -= this.Client_GuildMembersChunk;
                    client.UnknownEvent -= this.Client_UnknownEvent;
                    client.MessageReactionAdded -= this.Client_MessageReactionAdd;
                    client.MessageReactionRemoved -= this.Client_MessageReactionRemove;
                    client.MessageReactionsCleared -= this.Client_MessageReactionRemoveAll;
                    client.MessageReactionRemovedEmoji -= this.Client_MessageReactionRemovedEmoji;
                    client.WebhooksUpdated -= this.Client_WebhooksUpdate;
                    client.Heartbeated -= this.Client_HeartBeated;

                    client.Dispose();
                    this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disconnected shard {0}.", i);
                }
            }

            this.Shards.Clear();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets a shard from a guild id.
        /// <para>This method uses the <see cref="Utilities.GetShardId(ulong, int)"/> method and will not iterate through the shard guild caches.</para>
        /// </summary>
        /// <param name="guildId">The guild id for the shard.</param>
        /// <returns>The found shard.</returns>
        public DiscordClient GetShard(ulong guildId)
        { 
            var index = Utilities.GetShardId(guildId, this.ShardClients.Count);
            return this.ShardClients[index];
        }

        /// <summary>
        /// Gets a shard from a guild.
        /// <para>This method uses the <see cref="Utilities.GetShardId(ulong, int)"/> method and will not iterate through the shard guild caches.</para>
        /// </summary>
        /// <param name="guild">The guild for the shard.</param>
        /// <returns>The found shard.</returns>
        public DiscordClient GetShard(DiscordGuild guild)
            => this.GetShard(guild.Id);

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

        private async Task ConnectShardAsync(int i)
        {
            if (!this.Shards.TryGetValue(i, out var client))
                throw new Exception($"Could not initialize shard {i}.");

            if (this.GatewayInfo != null)
            {
                client.GatewayInfo = this.GatewayInfo;
                client._gatewayUri = new Uri(client.GatewayInfo.Url);
            }

            if (this.CurrentUser != null)
                client.CurrentUser = this.CurrentUser;

            if (this.CurrentApplication != null)
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

            client._isShard = true;
            await client.ConnectAsync().ConfigureAwait(false);
            this.Logger.LogInformation(LoggerEvents.ShardStartup, "Booted shard {0}", i);

            if (this.CurrentUser == null)
                this.CurrentUser = client.CurrentUser;

            if (this.CurrentApplication == null)
                this.CurrentApplication = client.CurrentApplication;

            if (this.InternalVoiceRegions == null)
            {
                this.InternalVoiceRegions = client.InternalVoiceRegions;
                this._voiceRegionsLazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this.InternalVoiceRegions));
            }
        }

        private async Task<GatewayInfo> GetGatewayInfoAsync()
        {
            string url = $"{Utilities.GetApiBaseUri()}{Endpoints.GATEWAY}{Endpoints.BOT}";

            var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
            http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(this.Configuration));
            this.Logger.LogDebug(LoggerEvents.RestError, "Initial request for rate limit bucket [GET::::/gateway/bot] [0/0] 1/1/0001 12:00:00 AM +00:00. Allowing.");
            var resp = await http.GetAsync(url).ConfigureAwait(false);

            var timer = new Stopwatch();
            timer.Start();

            http.Dispose();
            var jo = JObject.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
            var info = jo.ToObject<GatewayInfo>();

            //There is a delay from parsing here.
            timer.Stop();
            info.SessionBucket.resetAfter -= (int)timer.ElapsedMilliseconds; 

            info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.resetAfter);

            return jo.ToObject<GatewayInfo>();
        }

        private readonly Lazy<string> _versionString = new Lazy<string>(() =>
        {
            var a = typeof(DiscordShardedClient).GetTypeInfo().Assembly;

            var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (iv != null)
                return iv.InformationalVersion;

            var v = a.GetName().Version;
            var vs = v.ToString(3);

            if (v.Revision > 0)
                vs = $"{vs}, CI build {v.Revision}";

            return vs;
        });

        #region Event Dispatchers
        private Task Client_ClientError(DiscordClient client, ClientErrorEventArgs e) 
            => this._clientErrored.InvokeAsync(client, e);

        private Task Client_SocketError(DiscordClient client, SocketErrorEventArgs e) 
            => this._socketErrored.InvokeAsync(client, e);

        private Task Client_SocketOpened(DiscordClient client, SocketEventArgs e) 
            => this._socketOpened.InvokeAsync(client, e);

        private Task Client_SocketClosed(DiscordClient client, SocketCloseEventArgs e) 
            => this._socketClosed.InvokeAsync(client, e);

        private Task Client_Ready(DiscordClient client, ReadyEventArgs e) 
            => this._ready.InvokeAsync(client, e);

        private Task Client_Resumed(DiscordClient client, ReadyEventArgs e) 
            => this._resumed.InvokeAsync(client, e);

        private Task Client_ChannelCreated(DiscordClient client, ChannelCreateEventArgs e) 
            => this._channelCreated.InvokeAsync(client, e);

        private Task Client_DMChannelCreated(DiscordClient client, DmChannelCreateEventArgs e) 
            => this._dmChannelCreated.InvokeAsync(client, e);

        private Task Client_ChannelUpdated(DiscordClient client, ChannelUpdateEventArgs e) 
            => this._channelUpdated.InvokeAsync(client, e);

        private Task Client_ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e) 
            => this._channelDeleted.InvokeAsync(client, e);

        private Task Client_DMChannelDeleted(DiscordClient client, DmChannelDeleteEventArgs e) 
            => this._dmChannelDeleted.InvokeAsync(client, e);

        private Task Client_ChannelPinsUpdated(DiscordClient client, ChannelPinsUpdateEventArgs e) 
            => this._channelPinsUpdated.InvokeAsync(client, e);

        private Task Client_GuildCreated(DiscordClient client, GuildCreateEventArgs e) 
            => this._guildCreated.InvokeAsync(client, e);

        private Task Client_GuildAvailable(DiscordClient client, GuildCreateEventArgs e) 
            => this._guildAvailable.InvokeAsync(client, e);

        private Task Client_GuildUpdated(DiscordClient client, GuildUpdateEventArgs e) 
            => this._guildUpdated.InvokeAsync(client, e);

        private Task Client_GuildDeleted(DiscordClient client, GuildDeleteEventArgs e) 
            => this._guildDeleted.InvokeAsync(client, e);

        private Task Client_GuildUnavailable(DiscordClient client, GuildDeleteEventArgs e) 
            => this._guildUnavailable.InvokeAsync(client, e);

        private Task Client_GuildDownloadCompleted(DiscordClient client, GuildDownloadCompletedEventArgs e)
            => this._guildDownloadCompleted.InvokeAsync(client, e);

        private Task Client_MessageCreated(DiscordClient client, MessageCreateEventArgs e) 
            => this._messageCreated.InvokeAsync(client, e);

        private Task Client_InviteCreated(DiscordClient client, InviteCreateEventArgs e)
            => this._inviteCreated.InvokeAsync(client, e);

        private Task Client_InviteDeleted(DiscordClient client, InviteDeleteEventArgs e)
            => this._inviteDeleted.InvokeAsync(client, e);

        private Task Client_PresenceUpdate(DiscordClient client, PresenceUpdateEventArgs e) 
            => this._presenceUpdated.InvokeAsync(client, e);

        private Task Client_GuildBanAdd(DiscordClient client, GuildBanAddEventArgs e) 
            => this._guildBanAdded.InvokeAsync(client, e);

        private Task Client_GuildBanRemove(DiscordClient client, GuildBanRemoveEventArgs e) 
            => this._guildBanRemoved.InvokeAsync(client, e);

        private Task Client_GuildEmojisUpdate(DiscordClient client, GuildEmojisUpdateEventArgs e) 
            => this._guildEmojisUpdated.InvokeAsync(client, e);

        private Task Client_GuildIntegrationsUpdate(DiscordClient client, GuildIntegrationsUpdateEventArgs e) 
            => this._guildIntegrationsUpdated.InvokeAsync(client, e);

        private Task Client_GuildMemberAdd(DiscordClient client, GuildMemberAddEventArgs e) 
            => this._guildMemberAdded.InvokeAsync(client, e);

        private Task Client_GuildMemberRemove(DiscordClient client, GuildMemberRemoveEventArgs e) 
            => this._guildMemberRemoved.InvokeAsync(client, e);

        private Task Client_GuildMemberUpdate(DiscordClient client, GuildMemberUpdateEventArgs e) 
            => this._guildMemberUpdated.InvokeAsync(client, e);

        private Task Client_GuildRoleCreate(DiscordClient client, GuildRoleCreateEventArgs e) 
            => this._guildRoleCreated.InvokeAsync(client, e);

        private Task Client_GuildRoleUpdate(DiscordClient client, GuildRoleUpdateEventArgs e) 
            => this._guildRoleUpdated.InvokeAsync(client, e);

        private Task Client_GuildRoleDelete(DiscordClient client, GuildRoleDeleteEventArgs e) 
            => this._guildRoleDeleted.InvokeAsync(client, e);

        private Task Client_MessageUpdate(DiscordClient client, MessageUpdateEventArgs e) 
            => this._messageUpdated.InvokeAsync(client, e);

        private Task Client_MessageDelete(DiscordClient client, MessageDeleteEventArgs e) 
            => this._messageDeleted.InvokeAsync(client, e);

        private Task Client_MessageBulkDelete(DiscordClient client, MessageBulkDeleteEventArgs e) 
            => this._messageBulkDeleted.InvokeAsync(client, e);

        private Task Client_TypingStart(DiscordClient client, TypingStartEventArgs e) 
            => this._typingStarted.InvokeAsync(client, e);

        private Task Client_UserSettingsUpdate(DiscordClient client, UserSettingsUpdateEventArgs e) 
            => this._userSettingsUpdated.InvokeAsync(client, e);

        private Task Client_UserUpdate(DiscordClient client, UserUpdateEventArgs e) 
            => this._userUpdated.InvokeAsync(client, e);

        private Task Client_VoiceStateUpdate(DiscordClient client, VoiceStateUpdateEventArgs e) 
            => this._voiceStateUpdated.InvokeAsync(client, e);

        private Task Client_VoiceServerUpdate(DiscordClient client, VoiceServerUpdateEventArgs e) 
            => this._voiceServerUpdated.InvokeAsync(client, e);

        private Task Client_GuildMembersChunk(DiscordClient client, GuildMembersChunkEventArgs e) 
            => this._guildMembersChunk.InvokeAsync(client, e);

        private Task Client_UnknownEvent(DiscordClient client, UnknownEventArgs e) 
            => this._unknownEvent.InvokeAsync(client, e);

        private Task Client_MessageReactionAdd(DiscordClient client, MessageReactionAddEventArgs e) 
            => this._messageReactionAdded.InvokeAsync(client, e);

        private Task Client_MessageReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs e) 
            => this._messageReactionRemoved.InvokeAsync(client, e);

        private Task Client_MessageReactionRemoveAll(DiscordClient client, MessageReactionsClearEventArgs e) 
            => this._messageReactionsCleared.InvokeAsync(client, e);

        private Task Client_MessageReactionRemovedEmoji(DiscordClient client, MessageReactionRemoveEmojiEventArgs e)
            => this._messageReactionRemovedEmoji.InvokeAsync(client, e);

        private Task Client_WebhooksUpdate(DiscordClient client, WebhooksUpdateEventArgs e) 
            => this._webhooksUpdated.InvokeAsync(client, e);

        private Task Client_HeartBeated(DiscordClient client, HeartbeatEventArgs e) 
            => this._heartbeated.InvokeAsync(client, e);
        #endregion
    }
}
