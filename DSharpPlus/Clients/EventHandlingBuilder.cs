using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus;

/// <summary>
/// Provides an API for configuring delegate-based event handlers for your application.
/// </summary>
public sealed class EventHandlingBuilder
{
    /// <summary>
    /// The underlying service collection for this application.
    /// </summary>
    public IServiceCollection Services { get; }

    internal EventHandlingBuilder(IServiceCollection services)
        => this.Services = services;

    /// <summary>
    /// Fired whenever the underlying websocket connection is established.
    /// </summary>
    public EventHandlingBuilder HandleSocketOpened(AsyncEventHandler<DiscordClient, SocketOpenedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(SocketOpenedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired whenever the underlying websocket connection is terminated.
    /// </summary>
    public EventHandlingBuilder HandleSocketClosed(AsyncEventHandler<DiscordClient, SocketClosedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(SocketClosedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when this client has successfully completed its handshake with the websocket gateway.
    /// </summary>
    /// <remarks>
    /// <i><see cref="DiscordClient.Guilds"/> will not be populated when this event is fired.</i><br/>
    /// See also: <see cref="HandleGuildAvailable"/>, <see cref="HandleGuildDownloadCompleted"/>
    /// </remarks>
    public EventHandlingBuilder HandleSessionCreated(AsyncEventHandler<DiscordClient, SessionCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(SessionCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired whenever a session is resumed.
    /// </summary>
    public EventHandlingBuilder HandleSessionResumed(AsyncEventHandler<DiscordClient, SessionResumedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(SessionResumedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired whenever we receive a heartbeat from Discord.
    /// </summary>
    public EventHandlingBuilder HandleHeartbeated(AsyncEventHandler<DiscordClient, HeartbeatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(HeartbeatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when too many consecutive heartbeats fail and the library considers the connection zombied.
    /// </summary>
    public EventHandlingBuilder HandleZombied(AsyncEventHandler<DiscordClient, ZombiedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ZombiedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when the permissions for an application command are updated.
    /// </summary>
    public EventHandlingBuilder HandleApplicationCommandPermissionsUpdated
    (
        AsyncEventHandler<DiscordClient, ApplicationCommandPermissionsUpdatedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(ApplicationCommandPermissionsUpdatedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a new channel is created. 
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleChannelCreated(AsyncEventHandler<DiscordClient, ChannelCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ChannelCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a channel is updated. 
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleChannelUpdated(AsyncEventHandler<DiscordClient, ChannelUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ChannelUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a channel is deleted. 
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleChannelDeleted(AsyncEventHandler<DiscordClient, ChannelDeletedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ChannelDeletedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a DM channel is deleted. 
    /// For this event to fire you need the <see cref="DiscordIntents.DirectMessages"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleDmChannelDeleted(AsyncEventHandler<DiscordClient, DmChannelDeletedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(DmChannelDeletedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired whenever a channels pinned message list is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleChannelPinsUpdated(AsyncEventHandler<DiscordClient, ChannelPinsUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ChannelPinsUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when the user joins a new guild.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildCreated(AsyncEventHandler<DiscordClient, GuildCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild is becoming available.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildAvailable(AsyncEventHandler<DiscordClient, GuildAvailableEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildAvailableEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildUpdated(AsyncEventHandler<DiscordClient, GuildUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when the user leaves or is removed from a guild.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildDeleted(AsyncEventHandler<DiscordClient, GuildDeletedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildDeletedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild becomes unavailable.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildUnavailable(AsyncEventHandler<DiscordClient, GuildUnavailableEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildUnavailableEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when all guilds finish streaming from Discord upon initial connection.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildDownloadCompleted
    (
        AsyncEventHandler<DiscordClient, GuildDownloadCompletedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(GuildDownloadCompletedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a guilds emojis get updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildEmojisAndStickers"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildEmojisUpdated(AsyncEventHandler<DiscordClient, GuildEmojisUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildEmojisUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guilds stickers get updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildEmojisAndStickers"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildStickersUpdated(AsyncEventHandler<DiscordClient, GuildStickersUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildStickersUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild integration is updated.
    /// </summary>
    public EventHandlingBuilder HandleGuildIntegrationsUpdated
    (
        AsyncEventHandler<DiscordClient, GuildIntegrationsUpdatedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(GuildIntegrationsUpdatedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a audit log entry is created.
    /// </summary>
    public EventHandlingBuilder HandleGuildAuditLogCreated(AsyncEventHandler<DiscordClient, GuildAuditLogCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildAuditLogCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a scheduled event is created.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventCreated
    (
        AsyncEventHandler<DiscordClient, ScheduledGuildEventCreatedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(ScheduledGuildEventCreatedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a scheduled event is updated.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventUpdated
    (
        AsyncEventHandler<DiscordClient, ScheduledGuildEventUpdatedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(ScheduledGuildEventUpdatedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a scheduled event is deleted.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventDeleted
    (
        AsyncEventHandler<DiscordClient, ScheduledGuildEventDeletedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(ScheduledGuildEventDeletedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a scheduled event is completed.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventCompleted
    (
        AsyncEventHandler<DiscordClient, ScheduledGuildEventCompletedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(ScheduledGuildEventCompletedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when an user is registered to a scheduled event.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventUserAdded
    (
        AsyncEventHandler<DiscordClient, ScheduledGuildEventUserAddedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(ScheduledGuildEventUserAddedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when an user removes themselves from a scheduled event.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventUserRemoved
    (
        AsyncEventHandler<DiscordClient, ScheduledGuildEventUserRemovedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(ScheduledGuildEventUserRemovedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a guild ban gets added.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildModeration"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildBanAdded(AsyncEventHandler<DiscordClient, GuildBanAddedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildBanAddedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild ban gets removed.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildModeration"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildBanRemoved(AsyncEventHandler<DiscordClient, GuildBanRemovedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildBanRemovedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a new user joins a guild.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMembers"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildMemberAdded(AsyncEventHandler<DiscordClient, GuildMemberAddedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildMemberAddedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a user is removed from a guild, by leaving, a kick or a ban.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMembers"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildMemberRemoved(AsyncEventHandler<DiscordClient, GuildMemberRemovedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildMemberRemovedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild member is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMembers"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildMemberUpdated(AsyncEventHandler<DiscordClient, GuildMemberUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildMemberUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired in response to requesting guild members over the gateway.
    /// </summary>
    public EventHandlingBuilder HandleGuildMembersChunked(AsyncEventHandler<DiscordClient, GuildMembersChunkedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildMembersChunkedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild role is created.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildRoleCreated(AsyncEventHandler<DiscordClient, GuildRoleCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildRoleCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild role is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildRoleUpdated(AsyncEventHandler<DiscordClient, GuildRoleUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildRoleUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild role is deleted.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildRoleDeleted(AsyncEventHandler<DiscordClient, GuildRoleDeletedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(GuildRoleDeletedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when an invite is created.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildInvites"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleInviteCreated(AsyncEventHandler<DiscordClient, InviteCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(InviteCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when an invite is deleted.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildInvites"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleInviteDeleted(AsyncEventHandler<DiscordClient, InviteDeletedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(InviteDeletedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a message is created.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessages"/> or 
    /// <see cref="DiscordIntents.DirectMessages"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageCreated(AsyncEventHandler<DiscordClient, MessageCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(MessageCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a message is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessages"/> or 
    /// <see cref="DiscordIntents.DirectMessages"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageUpdated(AsyncEventHandler<DiscordClient, MessageUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(MessageUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a message is deleted.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessages"/> or 
    /// <see cref="DiscordIntents.DirectMessages"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageDeleted(AsyncEventHandler<DiscordClient, MessageDeletedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(MessageDeletedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when multiple messages are deleted at once.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessages"/> or 
    /// <see cref="DiscordIntents.DirectMessages"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessagesBulkDeleted(AsyncEventHandler<DiscordClient, MessagesBulkDeletedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(MessagesBulkDeletedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a vote was cast on a poll.
    /// </summary>
    public EventHandlingBuilder HandleMessagePollVoted(AsyncEventHandler<DiscordClient, MessagePollVotedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(MessagePollVotedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a reaction gets added to a message.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageReactionAdded(AsyncEventHandler<DiscordClient, MessageReactionAddedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(MessageReactionAddedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a reaction gets removed from a message.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageReactionRemoved
    (
        AsyncEventHandler<DiscordClient, MessageReactionRemovedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(MessageReactionRemovedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when all reactions get removed from a message.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageReactionsCleared
    (
        AsyncEventHandler<DiscordClient, MessageReactionsClearedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(MessageReactionsClearedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when all reactions of a specific emoji are removed from a message.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageReactionRemovedEmoji
    (
        AsyncEventHandler<DiscordClient, MessageReactionRemovedEmojiEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(MessageReactionRemovedEmojiEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a user presence has been updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildPresences"/> intent.
    /// </summary>
    public EventHandlingBuilder HandlePresenceUpdated(AsyncEventHandler<DiscordClient, PresenceUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(PresenceUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when the current user updates their settings.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildPresences"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleUserSettingsUpdated(AsyncEventHandler<DiscordClient, UserSettingsUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(UserSettingsUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when properties about the current user change.
    /// </summary>
    /// <remarks>
    /// Note that this event only applies for changes to the <b>current user</b>, the client that is connected to Discord.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildPresences"/> intent.
    /// </remarks>
    public EventHandlingBuilder HandleUserUpdated(AsyncEventHandler<DiscordClient, UserUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(UserUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when someone joins, leaves or moves voice channels.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleVoiceStateUpdated(AsyncEventHandler<DiscordClient, VoiceStateUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(VoiceStateUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild's voice server is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleVoiceServerUpdated(AsyncEventHandler<DiscordClient, VoiceServerUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(VoiceServerUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a thread is created.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleThreadCreated(AsyncEventHandler<DiscordClient, ThreadCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ThreadCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a thread is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleThreadUpdated(AsyncEventHandler<DiscordClient, ThreadUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ThreadUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a thread is deleted.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleThreadDeleted(AsyncEventHandler<DiscordClient, ThreadDeletedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ThreadDeletedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when the current member gains access to channels that contain threads.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleThreadListSynced(AsyncEventHandler<DiscordClient, ThreadListSyncedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ThreadListSyncedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when the thread member for the current user is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    /// <remarks>
    /// This event is primarily implemented for completeness and unlikely to be useful to bots.
    /// </remarks>
    public EventHandlingBuilder HandleThreadMemberUpdated(AsyncEventHandler<DiscordClient, ThreadMemberUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ThreadMemberUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when the thread members are updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMembers"/> or <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleThreadMembersUpdated(AsyncEventHandler<DiscordClient, ThreadMembersUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ThreadMembersUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when an integration is created.
    /// </summary>
    public EventHandlingBuilder HandleIntegrationCreated(AsyncEventHandler<DiscordClient, IntegrationCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(IntegrationCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when an integration is updated.
    /// </summary>
    public EventHandlingBuilder HandleIntegrationUpdated(AsyncEventHandler<DiscordClient, IntegrationUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(IntegrationUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when an integration is deleted.
    /// </summary>
    public EventHandlingBuilder HandleIntegrationDeleted(AsyncEventHandler<DiscordClient, IntegrationDeletedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(IntegrationDeletedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a stage instance is created.
    /// </summary>
    public EventHandlingBuilder HandleStageInstanceCreated(AsyncEventHandler<DiscordClient, StageInstanceCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(StageInstanceCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a stage instance is updated.
    /// </summary>
    public EventHandlingBuilder HandleStageInstanceUpdated(AsyncEventHandler<DiscordClient, StageInstanceUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(StageInstanceUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a stage instance is deleted.
    /// </summary>
    public EventHandlingBuilder HandleStageInstanceDeleted(AsyncEventHandler<DiscordClient, StageInstanceDeletedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(StageInstanceDeletedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when any interaction is invoked.
    /// </summary>
    public EventHandlingBuilder HandleInteractionCreated(AsyncEventHandler<DiscordClient, InteractionCreatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(InteractionCreatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a component is interacted with.
    /// </summary>
    public EventHandlingBuilder HandleComponentInteractionCreated
    (
        AsyncEventHandler<DiscordClient, ComponentInteractionCreatedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(ComponentInteractionCreatedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a modal is submitted. If a modal is closed, this event is not fired.
    /// </summary>
    public EventHandlingBuilder HandleModalSubmitted(AsyncEventHandler<DiscordClient, ModalSubmittedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(ModalSubmittedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a user uses a context menu.
    /// </summary>
    public EventHandlingBuilder HandleContextMenuInteractionCreated
    (
        AsyncEventHandler<DiscordClient, ContextMenuInteractionCreatedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(ContextMenuInteractionCreatedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a user starts typing in a channel.
    /// </summary>
    public EventHandlingBuilder HandleTypingStarted(AsyncEventHandler<DiscordClient, TypingStartedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(TypingStartedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when an unknown event gets received.
    /// </summary>
    public EventHandlingBuilder HandleUnknownEvent(AsyncEventHandler<DiscordClient, UnknownEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(UnknownEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired whenever webhooks update.
    /// </summary>
    public EventHandlingBuilder HandleWebhooksUpdated(AsyncEventHandler<DiscordClient, WebhooksUpdatedEventArgs> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.DelegateHandlers.GetOrAdd(typeof(WebhooksUpdatedEventArgs), _ => []).Add(handler));
        return this;
    }

    /// <summary>
    /// Fired when a new auto-moderation rule is created.
    /// </summary>
    public EventHandlingBuilder HandleAutoModerationRuleCreated
    (
        AsyncEventHandler<DiscordClient, AutoModerationRuleCreatedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(AutoModerationRuleCreatedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when an auto-moderation rule is updated.
    /// </summary>
    public EventHandlingBuilder HandleAutoModerationRuleUpdated
    (
        AsyncEventHandler<DiscordClient, AutoModerationRuleUpdatedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(AutoModerationRuleUpdatedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when an auto-moderation rule is deleted.
    /// </summary>
    public EventHandlingBuilder HandleAutoModerationRuleDeleted
    (
        AsyncEventHandler<DiscordClient, AutoModerationRuleDeletedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(AutoModerationRuleDeletedEventArgs), _ => []).Add(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when an auto-moderation is executed.
    /// </summary>
    public EventHandlingBuilder HandleAutoModerationRuleExecuted
    (
        AsyncEventHandler<DiscordClient, AutoModerationRuleExecutedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(AutoModerationRuleExecutedEventArgs), _ => []).Add(handler)
        );

        return this;
    }
}
