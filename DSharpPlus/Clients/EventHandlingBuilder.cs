using System;
using System.Threading.Tasks;

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
    /// Registers all event handlers implemented on the provided type.
    /// </summary>
    public EventHandlingBuilder AddEventHandlers<T>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : IEventHandler
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register<T>());
        this.Services.Add(ServiceDescriptor.Describe(typeof(T), typeof(T), lifetime));
        return this;
    }

    /// <summary>
    /// Fired whenever the underlying websocket connection is established.
    /// </summary>
    public EventHandlingBuilder HandleSocketOpened(Func<DiscordClient, SocketOpenedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired whenever the underlying websocket connection is terminated.
    /// </summary>
    public EventHandlingBuilder HandleSocketClosed(Func<DiscordClient, SocketClosedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when this client has successfully completed its handshake with the websocket gateway.
    /// </summary>
    /// <remarks>
    /// <i><see cref="DiscordClient.Guilds"/> will not be populated when this event is fired.</i><br/>
    /// See also: <see cref="HandleGuildAvailable"/>, <see cref="HandleGuildDownloadCompleted"/>
    /// </remarks>
    public EventHandlingBuilder HandleSessionCreated(Func<DiscordClient, SessionCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired whenever a session is resumed.
    /// </summary>
    public EventHandlingBuilder HandleSessionResumed(Func<DiscordClient, SessionResumedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when too many consecutive heartbeats fail and the library considers the connection zombied.
    /// </summary>
    public EventHandlingBuilder HandleZombied(Func<DiscordClient, ZombiedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when the permissions for an application command are updated.
    /// </summary>
    public EventHandlingBuilder HandleApplicationCommandPermissionsUpdated
    (
        Func<DiscordClient, ApplicationCommandPermissionsUpdatedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a new channel is created. 
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleChannelCreated(Func<DiscordClient, ChannelCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a channel is updated. 
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleChannelUpdated(Func<DiscordClient, ChannelUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a channel is deleted. 
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleChannelDeleted(Func<DiscordClient, ChannelDeletedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a DM channel is deleted. 
    /// For this event to fire you need the <see cref="DiscordIntents.DirectMessages"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleDmChannelDeleted(Func<DiscordClient, DmChannelDeletedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired whenever a channels pinned message list is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleChannelPinsUpdated(Func<DiscordClient, ChannelPinsUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when the user joins a new guild.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildCreated(Func<DiscordClient, GuildCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild is becoming available.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildAvailable(Func<DiscordClient, GuildAvailableEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildUpdated(Func<DiscordClient, GuildUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when the user leaves or is removed from a guild.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildDeleted(Func<DiscordClient, GuildDeletedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild becomes unavailable.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildUnavailable(Func<DiscordClient, GuildUnavailableEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when all guilds finish streaming from Discord upon initial connection.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildDownloadCompleted
    (
        Func<DiscordClient, GuildDownloadCompletedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guilds emojis get updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildEmojisAndStickers"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildEmojisUpdated(Func<DiscordClient, GuildEmojisUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guilds stickers get updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildEmojisAndStickers"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildStickersUpdated(Func<DiscordClient, GuildStickersUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild integration is updated.
    /// </summary>
    public EventHandlingBuilder HandleGuildIntegrationsUpdated
    (
        Func<DiscordClient, GuildIntegrationsUpdatedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a audit log entry is created.
    /// </summary>
    public EventHandlingBuilder HandleGuildAuditLogCreated(Func<DiscordClient, GuildAuditLogCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register<GuildAuditLogCreatedEventArgs>(handler));
        return this;
    }

    /// <summary>
    /// Fired when a scheduled event is created.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventCreated
    (
        Func<DiscordClient, ScheduledGuildEventCreatedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.Register<ScheduledGuildEventCreatedEventArgs>(handler)
        );

        return this;
    }

    /// <summary>
    /// Fired when a scheduled event is updated.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventUpdated
    (
        Func<DiscordClient, ScheduledGuildEventUpdatedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a scheduled event is deleted.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventDeleted
    (
        Func<DiscordClient, ScheduledGuildEventDeletedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a scheduled event is completed.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventCompleted
    (
        Func<DiscordClient, ScheduledGuildEventCompletedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when an user is registered to a scheduled event.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventUserAdded
    (
        Func<DiscordClient, ScheduledGuildEventUserAddedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));

        return this;
    }

    /// <summary>
    /// Fired when an user removes themselves from a scheduled event.
    /// </summary>
    public EventHandlingBuilder HandleScheduledGuildEventUserRemoved
    (
        Func<DiscordClient, ScheduledGuildEventUserRemovedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));

        return this;
    }

    /// <summary>
    /// Fired when a guild ban gets added.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildModeration"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildBanAdded(Func<DiscordClient, GuildBanAddedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild ban gets removed.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildModeration"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildBanRemoved(Func<DiscordClient, GuildBanRemovedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a new user joins a guild.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMembers"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildMemberAdded(Func<DiscordClient, GuildMemberAddedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a user is removed from a guild, by leaving, a kick or a ban.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMembers"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildMemberRemoved(Func<DiscordClient, GuildMemberRemovedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild member is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMembers"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildMemberUpdated(Func<DiscordClient, GuildMemberUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired in response to requesting guild members over the gateway.
    /// </summary>
    public EventHandlingBuilder HandleGuildMembersChunked(Func<DiscordClient, GuildMembersChunkedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild role is created.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildRoleCreated(Func<DiscordClient, GuildRoleCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild role is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildRoleUpdated(Func<DiscordClient, GuildRoleUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild role is deleted.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleGuildRoleDeleted(Func<DiscordClient, GuildRoleDeletedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when an invite is created.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildInvites"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleInviteCreated(Func<DiscordClient, InviteCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when an invite is deleted.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildInvites"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleInviteDeleted(Func<DiscordClient, InviteDeletedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a message is created.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessages"/> or 
    /// <see cref="DiscordIntents.DirectMessages"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageCreated(Func<DiscordClient, MessageCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a message is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessages"/> or 
    /// <see cref="DiscordIntents.DirectMessages"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageUpdated(Func<DiscordClient, MessageUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a message is deleted.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessages"/> or 
    /// <see cref="DiscordIntents.DirectMessages"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageDeleted(Func<DiscordClient, MessageDeletedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when multiple messages are deleted at once.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessages"/> or 
    /// <see cref="DiscordIntents.DirectMessages"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessagesBulkDeleted(Func<DiscordClient, MessagesBulkDeletedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a vote was cast on a poll.
    /// </summary>
    public EventHandlingBuilder HandleMessagePollVoted(Func<DiscordClient, MessagePollVotedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a reaction gets added to a message.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageReactionAdded(Func<DiscordClient, MessageReactionAddedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a reaction gets removed from a message.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageReactionRemoved
    (
        Func<DiscordClient, MessageReactionRemovedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when all reactions get removed from a message.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageReactionsCleared
    (
        Func<DiscordClient, MessageReactionsClearedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when all reactions of a specific emoji are removed from a message.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleMessageReactionRemovedEmoji
    (
        Func<DiscordClient, MessageReactionRemovedEmojiEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a user presence has been updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildPresences"/> intent.
    /// </summary>
    public EventHandlingBuilder HandlePresenceUpdated(Func<DiscordClient, PresenceUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when the current user updates their settings.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildPresences"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleUserSettingsUpdated(Func<DiscordClient, UserSettingsUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when properties about the current user change.
    /// </summary>
    /// <remarks>
    /// Note that this event only applies for changes to the <b>current user</b>, the client that is connected to Discord.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildPresences"/> intent.
    /// </remarks>
    public EventHandlingBuilder HandleUserUpdated(Func<DiscordClient, UserUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when someone joins, leaves or moves voice channels.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleVoiceStateUpdated(Func<DiscordClient, VoiceStateUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a guild's voice server is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleVoiceServerUpdated(Func<DiscordClient, VoiceServerUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a thread is created.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleThreadCreated(Func<DiscordClient, ThreadCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a thread is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleThreadUpdated(Func<DiscordClient, ThreadUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a thread is deleted.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleThreadDeleted(Func<DiscordClient, ThreadDeletedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when the current member gains access to channels that contain threads.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleThreadListSynced(Func<DiscordClient, ThreadListSyncedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when the thread member for the current user is updated.
    /// For this event to fire you need the <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    /// <remarks>
    /// This event is primarily implemented for completeness and unlikely to be useful to bots.
    /// </remarks>
    public EventHandlingBuilder HandleThreadMemberUpdated(Func<DiscordClient, ThreadMemberUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when the thread members are updated.
    /// For this event to fire you need the <see cref="DiscordIntents.GuildMembers"/> or <see cref="DiscordIntents.Guilds"/> intent.
    /// </summary>
    public EventHandlingBuilder HandleThreadMembersUpdated(Func<DiscordClient, ThreadMembersUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when an integration is created.
    /// </summary>
    public EventHandlingBuilder HandleIntegrationCreated(Func<DiscordClient, IntegrationCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when an integration is updated.
    /// </summary>
    public EventHandlingBuilder HandleIntegrationUpdated(Func<DiscordClient, IntegrationUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when an integration is deleted.
    /// </summary>
    public EventHandlingBuilder HandleIntegrationDeleted(Func<DiscordClient, IntegrationDeletedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a stage instance is created.
    /// </summary>
    public EventHandlingBuilder HandleStageInstanceCreated(Func<DiscordClient, StageInstanceCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a stage instance is updated.
    /// </summary>
    public EventHandlingBuilder HandleStageInstanceUpdated(Func<DiscordClient, StageInstanceUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a stage instance is deleted.
    /// </summary>
    public EventHandlingBuilder HandleStageInstanceDeleted(Func<DiscordClient, StageInstanceDeletedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when any interaction is invoked.
    /// </summary>
    public EventHandlingBuilder HandleInteractionCreated(Func<DiscordClient, InteractionCreatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a component is interacted with.
    /// </summary>
    public EventHandlingBuilder HandleComponentInteractionCreated
    (
        Func<DiscordClient, ComponentInteractionCreatedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a modal is submitted. If a modal is closed, this event is not fired.
    /// </summary>
    public EventHandlingBuilder HandleModalSubmitted(Func<DiscordClient, ModalSubmittedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a user uses a context menu.
    /// </summary>
    public EventHandlingBuilder HandleContextMenuInteractionCreated
    (
        Func<DiscordClient, ContextMenuInteractionCreatedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a user starts typing in a channel.
    /// </summary>
    public EventHandlingBuilder HandleTypingStarted(Func<DiscordClient, TypingStartedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when an unknown event gets received.
    /// </summary>
    public EventHandlingBuilder HandleUnknownEvent(Func<DiscordClient, UnknownEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired whenever webhooks update.
    /// </summary>
    public EventHandlingBuilder HandleWebhooksUpdated(Func<DiscordClient, WebhooksUpdatedEventArgs, Task> handler)
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when a new auto-moderation rule is created.
    /// </summary>
    public EventHandlingBuilder HandleAutoModerationRuleCreated
    (
        Func<DiscordClient, AutoModerationRuleCreatedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when an auto-moderation rule is updated.
    /// </summary>
    public EventHandlingBuilder HandleAutoModerationRuleUpdated
    (
        Func<DiscordClient, AutoModerationRuleUpdatedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when an auto-moderation rule is deleted.
    /// </summary>
    public EventHandlingBuilder HandleAutoModerationRuleDeleted
    (
        Func<DiscordClient, AutoModerationRuleDeletedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }

    /// <summary>
    /// Fired when an auto-moderation is executed.
    /// </summary>
    public EventHandlingBuilder HandleAutoModerationRuleExecuted
    (
        Func<DiscordClient, AutoModerationRuleExecutedEventArgs, Task> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>(c => c.Register(handler));
        return this;
    }
    
    /// <summary>
    /// Fired when an entitlement was created.
    /// </summary>
    public EventHandlingBuilder HandleEntitlementCreated
    (
        AsyncEventHandler<DiscordClient, EntitlementCreatedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(EntitlementCreatedEventArgs), _ => []).Add(handler)
        );

        return this;
    }
    
    /// <summary>
    /// Fired when an entitlement was updated.
    /// </summary>
    public EventHandlingBuilder HandleEntitlementUpdated
    (
        AsyncEventHandler<DiscordClient, EntitlementUpdatedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(EntitlementUpdatedEventArgs), _ => []).Add(handler)
        );

        return this;
    }
    
    /// <summary>
    /// Fired when an entitlement was deleted.
    /// </summary>
    public EventHandlingBuilder HandleEntitlementDeleted
    (
        AsyncEventHandler<DiscordClient, EntitlementDeletedEventArgs> handler
    )
    {
        this.Services.Configure<EventHandlerCollection>
        (
            c => c.DelegateHandlers.GetOrAdd(typeof(EntitlementDeletedEventArgs), _ => []).Add(handler)
        );

        return this;
    }
}
