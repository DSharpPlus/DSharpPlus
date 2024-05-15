using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using DSharpPlus.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DSharpPlus;

/// <summary>
/// A Discord API wrapper.
/// </summary>
public sealed partial class DiscordClient : BaseDiscordClient
{
    #region Internal Fields/Properties

    internal bool isShard = false;
    internal IMessageCacheProvider? MessageCache { get; }

    private List<BaseExtension> extensions = [];
    private StatusUpdate status = null;

    private ManualResetEventSlim ConnectionLock { get; } = new ManualResetEventSlim(true);

    #endregion

    #region Public Fields/Properties
    /// <summary>
    /// Gets the gateway protocol version.
    /// </summary>
    public int GatewayVersion { get; internal set; }

    /// <summary>
    /// Gets the gateway session information for this client.
    /// </summary>
    public GatewayInfo GatewayInfo { get; internal set; }

    /// <summary>
    /// Gets the gateway URL.
    /// </summary>
    public Uri GatewayUri { get; internal set; }

    /// <summary>
    /// Gets the total number of shards the bot is connected to.
    /// </summary>
    public int ShardCount => this.GatewayInfo != null
        ? this.GatewayInfo.ShardCount
        : this.Configuration.ShardCount;

    /// <summary>
    /// Gets the currently connected shard ID.
    /// </summary>
    public int ShardId
        => this.Configuration.ShardId;

    /// <summary>
    /// Gets the intents configured for this client.
    /// </summary>
    public DiscordIntents Intents
        => this.Configuration.Intents;

    /// <summary>
    /// Gets whether this client is connected to the gateway.
    /// </summary>
    public bool IsConnected
        => this.webSocketClient is not null && this.webSocketClient.IsConnected;

    /// <summary>
    /// Gets a dictionary of DM channels that have been cached by this client. The dictionary's key is the channel
    /// ID.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordDmChannel> PrivateChannels { get; }
    internal ConcurrentDictionary<ulong, DiscordDmChannel> privateChannels = new();

    /// <summary>
    /// Gets a dictionary of guilds that this client is in. The dictionary's key is the guild ID. Note that the
    /// guild objects in this dictionary will not be filled in if the specific guilds aren't available (the
    /// <see cref="GuildAvailable"/> or <see cref="GuildDownloadCompleted"/> events haven't been fired yet)
    /// </summary>
    public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }
    internal ConcurrentDictionary<ulong, DiscordGuild> guilds = new();

    /// <summary>
    /// Gets the WS latency for this client.
    /// </summary>
    public int Ping
        => Volatile.Read(ref this.ping);

    private int ping;

    /// <summary>
    /// Gets the collection of presences held by this client.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordPresence> Presences
        => this.presencesLazy.Value;

    internal Dictionary<ulong, DiscordPresence> presences = [];
    private Lazy<IReadOnlyDictionary<ulong, DiscordPresence>> presencesLazy;
    #endregion

    #region Constructor/Internal Setup

    /// <summary>
    /// Initializes a new instance of DiscordClient.
    /// </summary>
    /// <param name="config">Specifies configuration parameters.</param>
    public DiscordClient(DiscordConfiguration config)
        : base(config)
    {
        DiscordIntents intents = this.Configuration.Intents;
        if (intents.HasIntent(DiscordIntents.GuildMessages) || intents.HasIntent(DiscordIntents.DirectMessages))
        {
            this.MessageCache = this.Configuration.MessageCacheProvider
                ?? (this.Configuration.MessageCacheSize > 0 ? new MessageCache(this.Configuration.MessageCacheSize) : null);
        }

        InternalSetup();

        this.Guilds = new ReadOnlyConcurrentDictionary<ulong, DiscordGuild>(this.guilds);
        this.PrivateChannels = new ReadOnlyConcurrentDictionary<ulong, DiscordDmChannel>(this.privateChannels);
    }

    /// <summary>
    /// This constructor is used when constructing a sharded client to use a shared rest client.
    /// </summary>
    /// <param name="config">Specifies configuration parameters.</param>
    /// <param name="restClient">Restclient which will be used for the underlying ApiClients</param>
    internal DiscordClient(DiscordConfiguration config, RestClient restClient)
        : base(config, restClient)
    {
        DiscordIntents intents = this.Configuration.Intents;
        if (intents.HasIntent(DiscordIntents.GuildMessages) || intents.HasIntent(DiscordIntents.DirectMessages))
        {
            this.MessageCache = this.Configuration.MessageCacheProvider
                                ?? (this.Configuration.MessageCacheSize > 0 ? new MessageCache(this.Configuration.MessageCacheSize) : null);
        }

        InternalSetup();

        this.Guilds = new ReadOnlyConcurrentDictionary<ulong, DiscordGuild>(this.guilds);
        this.PrivateChannels = new ReadOnlyConcurrentDictionary<ulong, DiscordDmChannel>(this.privateChannels);
    }

    internal void InternalSetup()
    {
        this.clientErrored = new AsyncEvent<DiscordClient, ClientErrorEventArgs>("CLIENT_ERRORED", Goof);
        this.socketErrored = new AsyncEvent<DiscordClient, SocketErrorEventArgs>("SOCKET_ERRORED", Goof);
        this.socketOpened = new AsyncEvent<DiscordClient, SocketEventArgs>("SOCKET_OPENED", EventErrorHandler);
        this.socketClosed = new AsyncEvent<DiscordClient, SocketCloseEventArgs>("SOCKET_CLOSED", EventErrorHandler);
        this.ready = new AsyncEvent<DiscordClient, SessionReadyEventArgs>("READY", EventErrorHandler);
        this.resumed = new AsyncEvent<DiscordClient, SessionReadyEventArgs>("RESUMED", EventErrorHandler);
        this.channelCreated = new AsyncEvent<DiscordClient, ChannelCreateEventArgs>("CHANNEL_CREATED", EventErrorHandler);
        this.channelUpdated = new AsyncEvent<DiscordClient, ChannelUpdateEventArgs>("CHANNEL_UPDATED", EventErrorHandler);
        this.channelDeleted = new AsyncEvent<DiscordClient, ChannelDeleteEventArgs>("CHANNEL_DELETED", EventErrorHandler);
        this.dmChannelDeleted = new AsyncEvent<DiscordClient, DmChannelDeleteEventArgs>("DM_CHANNEL_DELETED", EventErrorHandler);
        this.channelPinsUpdated = new AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs>("CHANNEL_PINS_UPDATED", EventErrorHandler);
        this.guildCreated = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_CREATED", EventErrorHandler);
        this.guildAvailable = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_AVAILABLE", EventErrorHandler);
        this.guildUpdated = new AsyncEvent<DiscordClient, GuildUpdateEventArgs>("GUILD_UPDATED", EventErrorHandler);
        this.guildDeleted = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_DELETED", EventErrorHandler);
        this.guildUnavailable = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_UNAVAILABLE", EventErrorHandler);
        this.guildDownloadCompletedEv = new AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs>("GUILD_DOWNLOAD_COMPLETED", EventErrorHandler);
        this.inviteCreated = new AsyncEvent<DiscordClient, InviteCreateEventArgs>("INVITE_CREATED", EventErrorHandler);
        this.inviteDeleted = new AsyncEvent<DiscordClient, InviteDeleteEventArgs>("INVITE_DELETED", EventErrorHandler);
        this.messageCreated = new AsyncEvent<DiscordClient, MessageCreateEventArgs>("MESSAGE_CREATED", EventErrorHandler);
        this.presenceUpdated = new AsyncEvent<DiscordClient, PresenceUpdateEventArgs>("PRESENCE_UPDATED", EventErrorHandler);
        this.scheduledGuildEventCreated = new AsyncEvent<DiscordClient, ScheduledGuildEventCreateEventArgs>("SCHEDULED_GUILD_EVENT_CREATED", EventErrorHandler);
        this.scheduledGuildEventDeleted = new AsyncEvent<DiscordClient, ScheduledGuildEventDeleteEventArgs>("SCHEDULED_GUILD_EVENT_DELETED", EventErrorHandler);
        this.scheduledGuildEventUpdated = new AsyncEvent<DiscordClient, ScheduledGuildEventUpdateEventArgs>("SCHEDULED_GUILD_EVENT_UPDATED", EventErrorHandler);
        this.scheduledGuildEventCompleted = new AsyncEvent<DiscordClient, ScheduledGuildEventCompletedEventArgs>("SCHEDULED_GUILD_EVENT_COMPLETED", EventErrorHandler);
        this.scheduledGuildEventUserAdded = new AsyncEvent<DiscordClient, ScheduledGuildEventUserAddEventArgs>("SCHEDULED_GUILD_EVENT_USER_ADDED", EventErrorHandler);
        this.scheduledGuildEventUserRemoved = new AsyncEvent<DiscordClient, ScheduledGuildEventUserRemoveEventArgs>("SCHEDULED_GUILD_EVENT_USER_REMOVED", EventErrorHandler);
        this.guildBanAdded = new AsyncEvent<DiscordClient, GuildBanAddEventArgs>("GUILD_BAN_ADD", EventErrorHandler);
        this.guildBanRemoved = new AsyncEvent<DiscordClient, GuildBanRemoveEventArgs>("GUILD_BAN_REMOVED", EventErrorHandler);
        this.guildEmojisUpdated = new AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs>("GUILD_EMOJI_UPDATED", EventErrorHandler);
        this.guildStickersUpdated = new AsyncEvent<DiscordClient, GuildStickersUpdateEventArgs>("GUILD_STICKER_UPDATED", EventErrorHandler);
        this.guildIntegrationsUpdated = new AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs>("GUILD_INTEGRATIONS_UPDATED", EventErrorHandler);
        this.guildMemberAdded = new AsyncEvent<DiscordClient, GuildMemberAddEventArgs>("GUILD_MEMBER_ADD", EventErrorHandler);
        this.guildMemberRemoved = new AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs>("GUILD_MEMBER_REMOVED", EventErrorHandler);
        this.guildMemberUpdated = new AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs>("GUILD_MEMBER_UPDATED", EventErrorHandler);
        this.guildRoleCreated = new AsyncEvent<DiscordClient, GuildRoleCreateEventArgs>("GUILD_ROLE_CREATED", EventErrorHandler);
        this.guildRoleUpdated = new AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs>("GUILD_ROLE_UPDATED", EventErrorHandler);
        this.guildRoleDeleted = new AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs>("GUILD_ROLE_DELETED", EventErrorHandler);
        this.guildAuditLogCreated = new AsyncEvent<DiscordClient, GuildAuditLogCreatedEventArgs>("GUILD_AUDIT_LOG_CREATED", EventErrorHandler);
        this.messageUpdated = new AsyncEvent<DiscordClient, MessageUpdateEventArgs>("MESSAGE_UPDATED", EventErrorHandler);
        this.messageDeleted = new AsyncEvent<DiscordClient, MessageDeleteEventArgs>("MESSAGE_DELETED", EventErrorHandler);
        this.messagesBulkDeleted = new AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs>("MESSAGE_BULK_DELETED", EventErrorHandler);
        this.messagePollVoted = new AsyncEvent<DiscordClient, MessagePollVoteEventArgs>("MESSAGE_POLL_VOTED", EventErrorHandler);
        this.interactionCreated = new AsyncEvent<DiscordClient, InteractionCreateEventArgs>("INTERACTION_CREATED", EventErrorHandler);
        this.componentInteractionCreated = new AsyncEvent<DiscordClient, ComponentInteractionCreateEventArgs>("COMPONENT_INTERACTED", EventErrorHandler);
        this.modalSubmitted = new AsyncEvent<DiscordClient, ModalSubmitEventArgs>("MODAL_SUBMITTED", EventErrorHandler);
        this.contextMenuInteractionCreated = new AsyncEvent<DiscordClient, ContextMenuInteractionCreateEventArgs>("CONTEXT_MENU_INTERACTED", EventErrorHandler);
        this.typingStarted = new AsyncEvent<DiscordClient, TypingStartEventArgs>("TYPING_STARTED", EventErrorHandler);
        this.userSettingsUpdated = new AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs>("USER_SETTINGS_UPDATED", EventErrorHandler);
        this.userUpdated = new AsyncEvent<DiscordClient, UserUpdateEventArgs>("USER_UPDATED", EventErrorHandler);
        this.voiceStateUpdated = new AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs>("VOICE_STATE_UPDATED", EventErrorHandler);
        this.voiceServerUpdated = new AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs>("VOICE_SERVER_UPDATED", EventErrorHandler);
        this.guildMembersChunked = new AsyncEvent<DiscordClient, GuildMembersChunkEventArgs>("GUILD_MEMBERS_CHUNKED", EventErrorHandler);
        this.unknownEvent = new AsyncEvent<DiscordClient, UnknownEventArgs>("UNKNOWN_EVENT", EventErrorHandler);
        this.messageReactionAdded = new AsyncEvent<DiscordClient, MessageReactionAddEventArgs>("MESSAGE_REACTION_ADDED", EventErrorHandler);
        this.messageReactionRemoved = new AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs>("MESSAGE_REACTION_REMOVED", EventErrorHandler);
        this.messageReactionsCleared = new AsyncEvent<DiscordClient, MessageReactionsClearEventArgs>("MESSAGE_REACTIONS_CLEARED", EventErrorHandler);
        this.messageReactionRemovedEmoji = new AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs>("MESSAGE_REACTION_REMOVED_EMOJI", EventErrorHandler);
        this.webhooksUpdated = new AsyncEvent<DiscordClient, WebhooksUpdateEventArgs>("WEBHOOKS_UPDATED", EventErrorHandler);
        this.heartbeated = new AsyncEvent<DiscordClient, HeartbeatEventArgs>("HEARTBEATED", EventErrorHandler);
        this.zombied = new AsyncEvent<DiscordClient, ZombiedEventArgs>("ZOMBIED", EventErrorHandler);
        this.applicationCommandPermissionsUpdated = new AsyncEvent<DiscordClient, ApplicationCommandPermissionsUpdatedEventArgs>("APPLICATION_COMMAND_PERMISSIONS_UPDATED", EventErrorHandler);
        this.integrationCreated = new AsyncEvent<DiscordClient, IntegrationCreateEventArgs>("INTEGRATION_CREATED", EventErrorHandler);
        this.integrationUpdated = new AsyncEvent<DiscordClient, IntegrationUpdateEventArgs>("INTEGRATION_UPDATED", EventErrorHandler);
        this.integrationDeleted = new AsyncEvent<DiscordClient, IntegrationDeleteEventArgs>("INTEGRATION_DELETED", EventErrorHandler);
        this.stageInstanceCreated = new AsyncEvent<DiscordClient, StageInstanceCreateEventArgs>("STAGE_INSTANCE_CREATED", EventErrorHandler);
        this.stageInstanceUpdated = new AsyncEvent<DiscordClient, StageInstanceUpdateEventArgs>("STAGE_INSTANCE_UPDATED", EventErrorHandler);
        this.stageInstanceDeleted = new AsyncEvent<DiscordClient, StageInstanceDeleteEventArgs>("STAGE_INSTANCE_DELETED", EventErrorHandler);
        this.autoModerationRuleCreated = new AsyncEvent<DiscordClient, AutoModerationRuleCreateEventArgs>("AUTO_MODERATION_RULE_CREATE", EventErrorHandler);
        this.autoModerationRuleUpdated = new AsyncEvent<DiscordClient, AutoModerationRuleUpdateEventArgs>("AUTO_MODERATION_RULE_UPDATE", EventErrorHandler);
        this.autoModerationRuleDeleted = new AsyncEvent<DiscordClient, AutoModerationRuleDeleteEventArgs>("AUTO_MODERATION_RULE_DELETE", EventErrorHandler);
        this.autoModerationRuleExecuted = new AsyncEvent<DiscordClient, AutoModerationRuleExecuteEventArgs>("AUTO_MODERATION_ACTION_EXECUTION", EventErrorHandler);
        #region Threads
        this.threadCreated = new AsyncEvent<DiscordClient, ThreadCreateEventArgs>("THREAD_CREATED", EventErrorHandler);
        this.threadUpdated = new AsyncEvent<DiscordClient, ThreadUpdateEventArgs>("THREAD_UPDATED", EventErrorHandler);
        this.threadDeleted = new AsyncEvent<DiscordClient, ThreadDeleteEventArgs>("THREAD_DELETED", EventErrorHandler);
        this.threadListSynced = new AsyncEvent<DiscordClient, ThreadListSyncEventArgs>("THREAD_LIST_SYNCED", EventErrorHandler);
        this.threadMemberUpdated = new AsyncEvent<DiscordClient, ThreadMemberUpdateEventArgs>("THREAD_MEMBER_UPDATED", EventErrorHandler);
        this.threadMembersUpdated = new AsyncEvent<DiscordClient, ThreadMembersUpdateEventArgs>("THREAD_MEMBERS_UPDATED", EventErrorHandler);
        #endregion

        this.guilds.Clear();

        this.presencesLazy = new Lazy<IReadOnlyDictionary<ulong, DiscordPresence>>(() => new ReadOnlyDictionary<ulong, DiscordPresence>(this.presences));
    }

    #endregion

    #region Client Extension Methods

    /// <summary>
    /// Registers an extension with this client.
    /// </summary>
    /// <param name="ext">Extension to register.</param>
    /// <returns></returns>
    public void AddExtension(BaseExtension ext)
    {
        ext.Setup(this);
        this.extensions.Add(ext);
    }

    /// <summary>
    /// Retrieves a previously-registered extension from this client.
    /// </summary>
    /// <typeparam name="T">Type of extension to retrieve.</typeparam>
    /// <returns>The requested extension.</returns>
    public T GetExtension<T>() where T : BaseExtension
        => this.extensions.FirstOrDefault(x => x.GetType() == typeof(T)) as T;

    #endregion

    #region Public Connection Methods

    /// <summary>
    /// Connects to the gateway
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when an invalid token was provided.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ConnectAsync(DiscordActivity activity = null, DiscordUserStatus? status = null, DateTimeOffset? idlesince = null)
    {
        // Check if connection lock is already set, and set it if it isn't
        if (!this.ConnectionLock.Wait(0))
        {
            throw new InvalidOperationException("This client is already connected.");
        }

        this.ConnectionLock.Set();

        int w = 7500;
        int i = 5;
        bool s = false;
        Exception cex = null;

        if (activity == null && status == null && idlesince == null)
        {
            this.status = null;
        }
        else
        {
            long? since_unix = idlesince != null ? Utilities.GetUnixTime(idlesince.Value) : null;
            this.status = new StatusUpdate()
            {
                Activity = new TransportActivity(activity),
                Status = status ?? DiscordUserStatus.Online,
                IdleSince = since_unix,
                IsAFK = idlesince != null,
                activity = activity
            };
        }

        if (!this.isShard)
        {
            if (this.Configuration.TokenType != TokenType.Bot)
            {
                this.Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.");
            }

            this.Logger.LogInformation(LoggerEvents.Startup, "DSharpPlus, version {Version}", this.VersionString);
        }

        while (i-- > 0 || this.Configuration.ReconnectIndefinitely)
        {
            try
            {
                await InternalConnectAsync();
                s = true;
                break;
            }
            catch (UnauthorizedException e)
            {
                FailConnection(this.ConnectionLock);
                throw new Exception("Authentication failed. Check your token and try again.", e);
            }
            catch (PlatformNotSupportedException)
            {
                FailConnection(this.ConnectionLock);
                throw;
            }
            catch (NotImplementedException)
            {
                FailConnection(this.ConnectionLock);
                throw;
            }
            catch (Exception ex)
            {
                FailConnection(null);

                cex = ex;
                if (i <= 0 && !this.Configuration.ReconnectIndefinitely)
                {
                    break;
                }

                this.Logger.LogError(LoggerEvents.ConnectionFailure, ex, "Connection attempt failed, retrying in {Seconds}s", w / 1000);
                await Task.Delay(w);

                if (i > 0)
                {
                    w *= 2;
                }
            }
        }

        if (!s && cex != null)
        {
            this.ConnectionLock.Set();
            throw new Exception("Could not connect to Discord.", cex);
        }

        // non-closure, hence args
        static void FailConnection(ManualResetEventSlim cl)
        {
            // unlock this (if applicable) so we can let others attempt to connect
            cl?.Set();
        }
    }

    public Task ReconnectAsync(bool startNewSession = false)
        => InternalReconnectAsync(startNewSession, code: startNewSession ? 1000 : 4002);

    /// <summary>
    /// Disconnects from the gateway
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectAsync()
    {
        this.Configuration.AutoReconnect = false;
        if (this.webSocketClient != null)
        {
            await this.webSocketClient.DisconnectAsync();
        }
    }

    #endregion

    #region Public REST Methods

    /// <summary>
    /// Gets a sticker.
    /// </summary>
    /// <param name="stickerId">The ID of the sticker.</param>
    /// <returns>The specified sticker</returns>
    public async Task<DiscordMessageSticker> GetStickerAsync(ulong stickerId)
        => await this.ApiClient.GetStickerAsync(stickerId);

    /// <summary>
    /// Gets a collection of sticker packs that may be used by nitro users.
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordMessageStickerPack>> GetStickerPacksAsync()
        => await this.ApiClient.GetStickerPacksAsync();

    /// <summary>
    /// Gets a user
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="updateCache">Whether to always make a REST request and update cache. Passing true will update the user, updating stale properties such as <see cref="DiscordUser.BannerHash"/>.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordUser> GetUserAsync(ulong userId, bool updateCache = false)
    {
        if (!updateCache && TryGetCachedUserInternal(userId, out DiscordUser? usr))
        {
            return usr;
        }

        usr = await this.ApiClient.GetUserAsync(userId);

        // See BaseDiscordClient.UpdateUser for why this is done like this.
        this.UserCache.AddOrUpdate(userId, usr, (_, _) => usr);

        return usr;
    }

    /// <summary>
    /// Gets a channel
    /// </summary>
    /// <param name="id">The ID of the channel to get.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordChannel> GetChannelAsync(ulong id)
        => InternalGetCachedThread(id) ?? InternalGetCachedChannel(id) ?? await this.ApiClient.GetChannelAsync(id);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="content">Message content to send.</param>
    /// <returns>The Discord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content)
        => await this.ApiClient.CreateMessageAsync(channel.Id, content, embeds: null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <returns>The Discord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, DiscordEmbed embed)
        => await this.ApiClient.CreateMessageAsync(channel.Id, null, embed != null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="content">Message content to send.</param>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <returns>The Discord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermissions.SendMessages"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content, DiscordEmbed embed)
        => await this.ApiClient.CreateMessageAsync(channel.Id, content, embed != null ? new[] { embed } : null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false, suppressNotifications: false);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="builder">The Discord Message builder.</param>
    /// <returns>The Discord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermissions.SendMessages"/> permission if TTS is false and <see cref="DiscordPermissions.SendTtsMessages"/> if TTS is true.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, DiscordMessageBuilder builder)
        => await this.ApiClient.CreateMessageAsync(channel.Id, builder);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channel">Channel to send to.</param>
    /// <param name="action">The Discord Message builder.</param>
    /// <returns>The Discord Message that was sent.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermissions.SendMessages"/> permission if TTS is false and <see cref="DiscordPermissions.SendTtsMessages"/> if TTS is true.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, Action<DiscordMessageBuilder> action)
    {
        DiscordMessageBuilder builder = new();
        action(builder);

        return await this.ApiClient.CreateMessageAsync(channel.Id, builder);
    }

    /// <summary>
    /// Creates a guild. This requires the bot to be in less than 10 guilds total.
    /// </summary>
    /// <param name="name">Name of the guild.</param>
    /// <param name="region">Voice region of the guild.</param>
    /// <param name="icon">Stream containing the icon for the guild.</param>
    /// <param name="verificationLevel">Verification level for the guild.</param>
    /// <param name="defaultMessageNotifications">Default message notification settings for the guild.</param>
    /// <param name="systemChannelFlags">System channel flags fopr the guild.</param>
    /// <returns>The created guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuild> CreateGuildAsync
    (
        string name,
        string? region = null,
        Optional<Stream> icon = default,
        DiscordVerificationLevel? verificationLevel = null,
        DiscordDefaultMessageNotifications? defaultMessageNotifications = null,
        DiscordSystemChannelFlags? systemChannelFlags = null
    )
    {
        Optional<string?> iconb64 = Optional.FromNoValue<string?>();

        if (icon.HasValue && icon.Value != null)
        {
            using ImageTool imgtool = new(icon.Value);
            iconb64 = imgtool.GetBase64();
        }
        else if (icon.HasValue)
        {
            iconb64 = null;
        }

        return await this.ApiClient.CreateGuildAsync(name, region, iconb64, verificationLevel, defaultMessageNotifications, systemChannelFlags);
    }

    /// <summary>
    /// Creates a guild from a template. This requires the bot to be in less than 10 guilds total.
    /// </summary>
    /// <param name="code">The template code.</param>
    /// <param name="name">Name of the guild.</param>
    /// <param name="icon">Stream containing the icon for the guild.</param>
    /// <returns>The created guild.</returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuild> CreateGuildFromTemplateAsync(string code, string name, Optional<Stream> icon = default)
    {
        Optional<string?> iconb64 = Optional.FromNoValue<string?>();

        if (icon.HasValue && icon.Value != null)
        {
            using ImageTool imgtool = new(icon.Value);
            iconb64 = imgtool.GetBase64();
        }
        else if (icon.HasValue)
        {
            iconb64 = null;
        }

        return await this.ApiClient.CreateGuildFromTemplateAsync(code, name, iconb64);
    }

    /// <summary>
    /// Gets a guild.
    /// <para>Setting <paramref name="withCounts"/> to true will make a REST request.</para>
    /// </summary>
    /// <param name="id">The guild ID to search for.</param>
    /// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
    /// <returns>The requested Guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuild> GetGuildAsync(ulong id, bool? withCounts = null)
    {
        if (this.guilds.TryGetValue(id, out DiscordGuild? guild) && (!withCounts.HasValue || !withCounts.Value))
        {
            return guild;
        }

        guild = await this.ApiClient.GetGuildAsync(id, withCounts);
        IReadOnlyList<DiscordChannel> channels = await this.ApiClient.GetGuildChannelsAsync(guild.Id);
        foreach (DiscordChannel channel in channels)
        {
            guild.channels[channel.Id] = channel;
        }

        return guild;
    }

    /// <summary>
    /// Gets a guild preview
    /// </summary>
    /// <param name="id">The guild ID.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong id)
        => await this.ApiClient.GetGuildPreviewAsync(id);

    /// <summary>
    /// Gets an invite.
    /// </summary>
    /// <param name="code">The invite code.</param>
    /// <param name="withCounts">Whether to include presence and total member counts in the returned invite.</param>
    /// <param name="withExpiration">Whether to include the expiration date in the returned invite.</param>
    /// <returns>The requested Invite.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the invite does not exists.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordInvite> GetInviteByCodeAsync(string code, bool? withCounts = null, bool? withExpiration = null)
        => await this.ApiClient.GetInviteAsync(code, withCounts, withExpiration);

    /// <summary>
    /// Gets a list of connections
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordConnection>> GetConnectionsAsync()
        => await this.ApiClient.GetUsersConnectionsAsync();

    /// <summary>
    /// Gets a webhook
    /// </summary>
    /// <param name="id">The ID of webhook to get.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordWebhook> GetWebhookAsync(ulong id)
        => await this.ApiClient.GetWebhookAsync(id);

    /// <summary>
    /// Gets a webhook
    /// </summary>
    /// <param name="id">The ID of webhook to get.</param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong id, string token)
        => await this.ApiClient.GetWebhookWithTokenAsync(id, token);

    /// <summary>
    /// Updates current user's activity and status.
    /// </summary>
    /// <param name="activity">Activity to set.</param>
    /// <param name="userStatus">Status of the user.</param>
    /// <param name="idleSince">Since when is the client performing the specified activity.</param>
    /// <returns></returns>
    public Task UpdateStatusAsync(DiscordActivity activity = null, DiscordUserStatus? userStatus = null, DateTimeOffset? idleSince = null)
        => InternalUpdateStatusAsync(activity, userStatus, idleSince);

    /// <summary>
    /// Edits current user.
    /// </summary>
    /// <param name="username">New username.</param>
    /// <param name="avatar">New avatar.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordUser> UpdateCurrentUserAsync(string username = null, Optional<Stream> avatar = default)
    {
        Optional<string> av64 = Optional.FromNoValue<string>();
        if (avatar.HasValue && avatar.Value != null)
        {
            using ImageTool imgtool = new(avatar.Value);
            av64 = imgtool.GetBase64();
        }
        else if (avatar.HasValue)
        {
            av64 = null;
        }

        TransportUser usr = await this.ApiClient.ModifyCurrentUserAsync(username, av64);

        this.CurrentUser.Username = usr.Username;
        this.CurrentUser.Discriminator = usr.Discriminator;
        this.CurrentUser.AvatarHash = usr.AvatarHash;
        return this.CurrentUser;
    }

    /// <summary>
    /// Gets a guild template by the code.
    /// </summary>
    /// <param name="code">The code of the template.</param>
    /// <returns>The guild template for the code.</returns>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildTemplate> GetTemplateAsync(string code)
        => await this.ApiClient.GetTemplateAsync(code);

    /// <summary>
    /// Gets all the global application commands for this application.
    /// </summary>
    /// <returns>A list of global application commands.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync() =>
        await this.ApiClient.GetGlobalApplicationCommandsAsync(this.CurrentApplication.Id);

    /// <summary>
    /// Overwrites the existing global application commands. New commands are automatically created and missing commands are automatically deleted.
    /// </summary>
    /// <param name="commands">The list of commands to overwrite with.</param>
    /// <returns>The list of global commands.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(IEnumerable<DiscordApplicationCommand> commands) =>
        await this.ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(this.CurrentApplication.Id, commands);

    /// <summary>
    /// Creates or overwrites a global application command.
    /// </summary>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command.</returns>
    public async Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(DiscordApplicationCommand command) =>
        await this.ApiClient.CreateGlobalApplicationCommandAsync(this.CurrentApplication.Id, command);

    /// <summary>
    /// Gets a global application command by its id.
    /// </summary>
    /// <param name="commandId">The ID of the command to get.</param>
    /// <returns>The command with the ID.</returns>
    public async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong commandId) =>
        await this.ApiClient.GetGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

    /// <summary>
    /// Gets a global application command by its name.
    /// </summary>
    /// <param name="commandName">The name of the command to get.</param>
    /// <returns>The command with the name.</returns>
    public async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(string commandName)
    {
        foreach (DiscordApplicationCommand command in await this.ApiClient.GetGlobalApplicationCommandsAsync(this.CurrentApplication.Id))
        {
            if (command.Name == commandName)
            {
                return command;
            }
        }

        return null;
    }

    /// <summary>
    /// Edits a global application command.
    /// </summary>
    /// <param name="commandId">The ID of the command to edit.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The edited command.</returns>
    public async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong commandId, Action<ApplicationCommandEditModel> action)
    {
        ApplicationCommandEditModel mdl = new();
        action(mdl);
        ulong applicationId = this.CurrentApplication?.Id ?? (await GetCurrentApplicationAsync()).Id;
        return await this.ApiClient.EditGlobalApplicationCommandAsync(applicationId, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.DefaultPermission, mdl.NSFW, default, default, mdl.AllowDMUsage, mdl.DefaultMemberPermissions);
    }

    /// <summary>
    /// Deletes a global application command.
    /// </summary>
    /// <param name="commandId">The ID of the command to delete.</param>
    public async Task DeleteGlobalApplicationCommandAsync(ulong commandId) =>
        await this.ApiClient.DeleteGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

    /// <summary>
    /// Gets all the application commands for a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to get application commands for.</param>
    /// <returns>A list of application commands in the guild.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong guildId) =>
        await this.ApiClient.GetGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId);

    /// <summary>
    /// Overwrites the existing application commands in a guild. New commands are automatically created and missing commands are automatically deleted.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="commands">The list of commands to overwrite with.</param>
    /// <returns>The list of guild commands.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong guildId, IEnumerable<DiscordApplicationCommand> commands) =>
        await this.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId, commands);

    /// <summary>
    /// Creates or overwrites a guild application command.
    /// </summary>
    /// <param name="guildId">The ID of the guild to create the application command in.</param>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command.</returns>
    public async Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong guildId, DiscordApplicationCommand command) =>
        await this.ApiClient.CreateGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, command);

    /// <summary>
    /// Gets a application command in a guild by its ID.
    /// </summary>
    /// <param name="guildId">The ID of the guild the application command is in.</param>
    /// <param name="commandId">The ID of the command to get.</param>
    /// <returns>The command with the ID.</returns>
    public async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
         await this.ApiClient.GetGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

    /// <summary>
    /// Edits a application command in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild the application command is in.</param>
    /// <param name="commandId">The ID of the command to edit.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The edited command.</returns>
    public async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong guildId, ulong commandId, Action<ApplicationCommandEditModel> action)
    {
        ApplicationCommandEditModel mdl = new();
        action(mdl);
        ulong applicationId = this.CurrentApplication?.Id ?? (await GetCurrentApplicationAsync()).Id;
        return await this.ApiClient.EditGuildApplicationCommandAsync(applicationId, guildId, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.DefaultPermission, mdl.NSFW, default, default, mdl.AllowDMUsage, mdl.DefaultMemberPermissions);
    }

    /// <summary>
    /// Deletes a application command in a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to delete the application command in.</param>
    /// <param name="commandId">The ID of the command.</param>
    public async Task DeleteGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
        await this.ApiClient.DeleteGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

    /// <summary>
    /// Returns a list of guilds before a certain guild. This will execute one API request per 200 guilds.
    /// <param name="limit">The amount of guilds to fetch.</param>
    /// <param name="before">The ID of the guild before which we fetch the guilds</param>
    /// <param name="withCount">Whether to include approximate member and presence counts in the returned guilds.</param>
    /// <param name="cancellationToken">Cancels the enumeration before doing the next api request</param>
    /// </summary>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public IAsyncEnumerable<DiscordGuild> GetGuildsBeforeAsync(ulong before, int limit = 200, bool? withCount = null, CancellationToken cancellationToken = default)
        => GetGuildsInternalAsync(limit, before, withCount: withCount, cancellationToken: cancellationToken);

    /// <summary>
    /// Returns a list of guilds after a certain guild. This will execute one API request per 200 guilds.
    /// <param name="limit">The amount of guilds to fetch.</param>
    /// <param name="after">The ID of the guild after which we fetch the guilds.</param>
    /// <param name="withCount">Whether to include approximate member and presence counts in the returned guilds.</param>
    /// <param name="cancellationToken">Cancels the enumeration before doing the next api request</param>
    /// </summary>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public IAsyncEnumerable<DiscordGuild> GetGuildsAfterAsync(ulong after, int limit = 200, bool? withCount = null, CancellationToken cancellationToken = default)
        => GetGuildsInternalAsync(limit, after: after, withCount: withCount, cancellationToken: cancellationToken);

    /// <summary>
    /// Returns a list of guilds the bot is in. This will execute one API request per 200 guilds.
    /// <param name="limit">The amount of guilds to fetch.</param>
    /// <param name="withCount">Whether to include approximate member and presence counts in the returned guilds.</param>
    /// <param name="cancellationToken">Cancels the enumeration before doing the next api request</param>
    /// </summary>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public IAsyncEnumerable<DiscordGuild> GetGuildsAsync(int limit = 200, bool? withCount = null, CancellationToken cancellationToken = default) =>
        GetGuildsInternalAsync(limit, withCount: withCount, cancellationToken: cancellationToken);

    private async IAsyncEnumerable<DiscordGuild> GetGuildsInternalAsync
    (
        int limit = 200,
        ulong? before = null,
        ulong? after = null,
        bool? withCount = null,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default
    )
    {
        if (limit < 0)
        {
            throw new ArgumentException("Cannot get a negative number of guilds.");
        }

        if (limit == 0)
        {
            yield break;
        }

        List<DiscordGuild> guilds = new(limit);
        int remaining = limit;
        ulong? last = null;
        bool isbefore = before != null;

        int lastCount;
        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            int fetchSize = remaining > 200 ? 200 : remaining;
            IReadOnlyList<DiscordGuild> fetchedGuilds = await this.ApiClient.GetGuildsAsync(fetchSize, isbefore ? last ?? before : null, !isbefore ? last ?? after : null, withCount);

            lastCount = fetchedGuilds.Count;
            remaining -= lastCount;

            //We sort the returned guilds by ID so that they are in order in case Discord switches the order AGAIN.
            DiscordGuild[] sortedGuildsArray = [.. fetchedGuilds];
            Array.Sort(sortedGuildsArray, (x, y) => x.Id.CompareTo(y.Id));

            if (!isbefore)
            {
                foreach (DiscordGuild guild in sortedGuildsArray)
                {
                    yield return guild;
                }
                last = sortedGuildsArray.LastOrDefault()?.Id;
            }
            else
            {
                for (int i = sortedGuildsArray.Length - 1; i >= 0; i--)
                {
                    yield return sortedGuildsArray[i];
                }
                last = sortedGuildsArray.FirstOrDefault()?.Id;
            }
        }
        while (remaining > 0 && lastCount is > 0 and 100);
    }

    #endregion

    #region Internal Caching Methods

    internal DiscordThreadChannel? InternalGetCachedThread(ulong threadId)
    {
        foreach (DiscordGuild guild in this.Guilds.Values)
        {
            if (guild.Threads.TryGetValue(threadId, out DiscordThreadChannel? foundThread))
            {
                return foundThread;
            }
        }

        return null;
    }

    internal DiscordChannel InternalGetCachedChannel(ulong channelId)
    {
        if (this.privateChannels?.TryGetValue(channelId, out DiscordDmChannel? foundDmChannel) == true)
        {
            return foundDmChannel;
        }

        foreach (DiscordGuild guild in this.Guilds.Values)
        {
            if (guild.Channels.TryGetValue(channelId, out DiscordChannel? foundChannel))
            {
                return foundChannel;
            }
        }

        return null;
    }

    internal DiscordGuild InternalGetCachedGuild(ulong? guildId)
    {
        if (this.guilds != null && guildId.HasValue)
        {
            if (this.guilds.TryGetValue(guildId.Value, out DiscordGuild? guild))
            {
                return guild;
            }
        }

        return null;
    }

    private void UpdateMessage(DiscordMessage message, TransportUser author, DiscordGuild guild, TransportMember member)
    {
        if (author != null)
        {
            DiscordUser usr = new(author) { Discord = this };

            if (member != null)
            {
                member.User = author;
            }

            message.Author = UpdateUser(usr, guild?.Id, guild, member);
        }

        DiscordChannel? channel = InternalGetCachedChannel(message.ChannelId) ?? InternalGetCachedThread(message.ChannelId);

        if (channel != null)
        {
            return;
        }

        channel = !message.guildId.HasValue
            ? new DiscordDmChannel
            {
                Id = message.ChannelId,
                Discord = this,
                Type = DiscordChannelType.Private,
                Recipients = [message.Author]
            }
            : new DiscordChannel
            {
                Id = message.ChannelId,
                GuildId = guild.Id,
                Discord = this
            };
        
        UpdateChannelCache(channel);

        message.Channel = channel;
    }

    private DiscordUser UpdateUser(DiscordUser usr, ulong? guildId, DiscordGuild guild, TransportMember mbr)
    {
        if (mbr != null)
        {
            if (mbr.User != null)
            {
                usr = new DiscordUser(mbr.User) { Discord = this };

                UpdateUserCache(usr);

                usr = new DiscordMember(mbr) { Discord = this, guild_id = guildId.Value };
            }

            DiscordIntents intents = this.Configuration.Intents;

            DiscordMember member = default;

            if (!intents.HasAllPrivilegedIntents() || guild.IsLarge) // we have the necessary privileged intents, no need to worry about caching here unless guild is large.
            {
                if (guild?.members.TryGetValue(usr.Id, out member) == false)
                {
                    if (intents.HasIntent(DiscordIntents.GuildMembers) || this.Configuration.AlwaysCacheMembers) // member can be updated by events, so cache it
                    {
                        guild.members.TryAdd(usr.Id, (DiscordMember)usr);
                    }
                }
                else if (intents.HasIntent(DiscordIntents.GuildPresences) || this.Configuration.AlwaysCacheMembers) // we can attempt to update it if it's already in cache.
                {
                    if (!intents.HasIntent(DiscordIntents.GuildMembers)) // no need to update if we already have the member events
                    {
                        _ = guild?.members.TryUpdate(usr.Id, (DiscordMember)usr, member);
                    }
                }
            }
        }
        else if (usr.Username != null) // check if not a skeleton user
        {
            UpdateUserCache(usr);
        }

        return usr;
    }

    private void UpdateCachedGuild(DiscordGuild newGuild, JArray rawMembers)
    {
        if (this.disposed)
        {
            return;
        }

        if (!this.guilds.TryGetValue(newGuild.Id, out DiscordGuild guild))
        {
            guild = newGuild;
            this.guilds[newGuild.Id] = guild;
        }

        if (newGuild.channels != null && !newGuild.channels.IsEmpty)
        {
            foreach (DiscordChannel channel in newGuild.channels.Values)
            {
                if (guild.channels.TryGetValue(channel.Id, out _))
                {
                    continue;
                }

                foreach (DiscordOverwrite overwrite in channel.permissionOverwrites)
                {
                    overwrite.Discord = this;
                    overwrite.channelId = channel.Id;
                }

                guild.channels[channel.Id] = channel;
            }
        }
        if (newGuild.threads != null && !newGuild.threads.IsEmpty)
        {
            foreach (DiscordThreadChannel thread in newGuild.threads.Values)
            {
                if (guild.threads.TryGetValue(thread.Id, out _))
                {
                    continue;
                }

                guild.threads[thread.Id] = thread;
            }
        }

        foreach (DiscordEmoji newEmoji in newGuild.emojis.Values)
        {
            _ = guild.emojis.GetOrAdd(newEmoji.Id, _ => newEmoji);
        }

        foreach (DiscordMessageSticker newSticker in newGuild.stickers.Values)
        {
            _ = guild.stickers.GetOrAdd(newSticker.Id, _ => newSticker);
        }

        if (rawMembers != null)
        {
            guild.members.Clear();

            foreach (JToken xj in rawMembers)
            {
                TransportMember xtm = xj.ToDiscordObject<TransportMember>();

                DiscordUser xu = new(xtm.User) { Discord = this };
                UpdateUserCache(xu);

                guild.members[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, guild_id = guild.Id };
            }
        }

        foreach (DiscordRole role in newGuild.roles.Values)
        {
            if (guild.roles.TryGetValue(role.Id, out _))
            {
                continue;
            }

            role.guild_id = guild.Id;
            guild.roles[role.Id] = role;
        }

        if (newGuild.stageInstances != null)
        {
            foreach (DiscordStageInstance newStageInstance in newGuild.stageInstances.Values)
            {
                _ = guild.stageInstances.GetOrAdd(newStageInstance.Id, _ => newStageInstance);
            }
        }

        guild.Name = newGuild.Name;
        guild.AfkChannelId = newGuild.AfkChannelId;
        guild.AfkTimeout = newGuild.AfkTimeout;
        guild.DefaultMessageNotifications = newGuild.DefaultMessageNotifications;
        guild.Features = newGuild.Features;
        guild.IconHash = newGuild.IconHash;
        guild.MfaLevel = newGuild.MfaLevel;
        guild.OwnerId = newGuild.OwnerId;
        guild.voiceRegionId = newGuild.voiceRegionId;
        guild.SplashHash = newGuild.SplashHash;
        guild.VerificationLevel = newGuild.VerificationLevel;
        guild.WidgetEnabled = newGuild.WidgetEnabled;
        guild.WidgetChannelId = newGuild.WidgetChannelId;
        guild.ExplicitContentFilter = newGuild.ExplicitContentFilter;
        guild.PremiumTier = newGuild.PremiumTier;
        guild.PremiumSubscriptionCount = newGuild.PremiumSubscriptionCount;
        guild.Banner = newGuild.Banner;
        guild.Description = newGuild.Description;
        guild.VanityUrlCode = newGuild.VanityUrlCode;
        guild.Banner = newGuild.Banner;
        guild.SystemChannelId = newGuild.SystemChannelId;
        guild.SystemChannelFlags = newGuild.SystemChannelFlags;
        guild.DiscoverySplashHash = newGuild.DiscoverySplashHash;
        guild.MaxMembers = newGuild.MaxMembers;
        guild.MaxPresences = newGuild.MaxPresences;
        guild.ApproximateMemberCount = newGuild.ApproximateMemberCount;
        guild.ApproximatePresenceCount = newGuild.ApproximatePresenceCount;
        guild.MaxVideoChannelUsers = newGuild.MaxVideoChannelUsers;
        guild.PreferredLocale = newGuild.PreferredLocale;
        guild.RulesChannelId = newGuild.RulesChannelId;
        guild.PublicUpdatesChannelId = newGuild.PublicUpdatesChannelId;
        guild.PremiumProgressBarEnabled = newGuild.PremiumProgressBarEnabled;

        // fields not sent for update:
        // - guild.Channels
        // - voice states
        // - guild.JoinedAt = new_guild.JoinedAt;
        // - guild.Large = new_guild.Large;
        // - guild.MemberCount = Math.Max(new_guild.MemberCount, guild.members.Count);
        // - guild.Unavailable = new_guild.Unavailable;
    }

    private void PopulateMessageReactionsAndCache(DiscordMessage message, TransportUser author, TransportMember member)
    {
        DiscordGuild guild = message.Channel?.Guild ?? InternalGetCachedGuild(message.guildId);
        UpdateMessage(message, author, guild, member);
        message.reactions ??= [];

        foreach (DiscordReaction xr in message.reactions)
        {
            xr.Emoji.Discord = this;
        }

        if (this.Configuration.MessageCacheSize > 0 && message.Channel != null)
        {
            this.MessageCache?.Add(message);
        }
    }

    // Ensures the channel is cached:
    // - DM -> _privateChannels dict on DiscordClient
    // - Thread -> DiscordGuild#_threads
    // - _ -> DiscordGuild#_channels
    private void UpdateChannelCache(DiscordChannel? channel)
    {
        if (channel is null)
        {
            return;
        }

        switch (channel)
        {
            case DiscordDmChannel dmChannel:
                this.privateChannels.TryAdd(channel.Id, dmChannel);
                break;
            case DiscordThreadChannel threadChannel:
                if (this.guilds.TryGetValue(channel.GuildId!.Value, out DiscordGuild? guild))
                {
                    guild.threads.TryAdd(channel.Id, threadChannel);
                }
                break;
            default:
                if (this.guilds.TryGetValue(channel.GuildId!.Value, out guild))
                {
                    guild.channels.TryAdd(channel.Id, channel);
                }
                break;
        }
    }

    #endregion

    #region Disposal

    private bool disposed;

    /// <summary>
    /// Disposes your DiscordClient.
    /// </summary>
    public override void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        DisconnectAsync().GetAwaiter().GetResult();
        this.ApiClient?.rest?.Dispose();
        this.CurrentUser = null!;

        List<BaseExtension> extensions = this.extensions; // prevent extensions being modified during dispose
        this.extensions = null!;

        foreach (BaseExtension extension in extensions)
        {
            if (extension is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        try
        {
            this.cancelTokenSource?.Cancel();
            this.cancelTokenSource?.Dispose();
        }
        catch { }

        this.guilds = null!;
        this.privateChannels = null!;
    }

    #endregion
}
