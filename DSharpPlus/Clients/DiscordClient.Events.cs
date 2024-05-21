using System;
using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DSharpPlus;

public sealed partial class DiscordClient
{
    // oh lord why did you have to pack into regions
    // this makes simple copy-paste ineffective
    // :notlikethis:

    private const string ObsoletionMessage = "Events on DiscordClient are deprecated and will be removed within the v5 development cycle. " +
        "Please use the ConfigureEventHandlers methods on your preferred construction method instead.";

    #region WebSocket

    /// <summary>
    /// Fired whenever a WebSocket error occurs within the client.
    /// </summary>
    [Obsolete("This event is superseded by implementing/shimming IClientErrorHandler", true, DiagnosticId = "DSP0003")]
    public event AsyncEventHandler<DiscordClient, SocketErrorEventArgs> SocketErrored
    {
        add => this.socketErrored.Register(value);
        remove => this.socketErrored.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SocketErrorEventArgs> socketErrored;

    /// <summary>
    /// Fired whenever WebSocket connection is established.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, SocketOpenedEventArgs> SocketOpened
    {
        add => this.socketOpened.Register(value);
        remove => this.socketOpened.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SocketOpenedEventArgs> socketOpened;

    /// <summary>
    /// Fired whenever WebSocket connection is terminated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, SocketClosedEventArgs> SocketClosed
    {
        add => this.socketClosed.Register(value);
        remove => this.socketClosed.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SocketClosedEventArgs> socketClosed;

    /// <summary>
    /// Fired when this client has successfully completed its handshake with the websocket gateway.
    /// </summary>
    /// <remarks>
    /// <i><see cref="Guilds"/> will not be populated when this event is fired.</i><br/>
    /// See also: <see cref="GuildAvailable"/>, <see cref="GuildDownloadCompleted"/>
    /// </remarks>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, SessionCreatedEventArgs> SessionCreated
    {
        add => this.ready.Register(value);
        remove => this.ready.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SessionCreatedEventArgs> ready;

    /// <summary>
    /// Fired whenever a session is resumed.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, SessionCreatedEventArgs> SessionResumed
    {
        add => this.resumed.Register(value);
        remove => this.resumed.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SessionCreatedEventArgs> resumed;

    /// <summary>
    /// Fired on received heartbeat ACK.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, HeartbeatedEventArgs> Heartbeated
    {
        add => this.heartbeated.Register(value);
        remove => this.heartbeated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, HeartbeatedEventArgs> heartbeated;

    /// <summary>
    /// Fired on heartbeat attempt cancellation due to too many failed heartbeats.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ZombiedEventArgs> Zombied
    {
        add => this.zombied.Register(value);
        remove => this.zombied.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ZombiedEventArgs> zombied;

    #endregion

    #region Application
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ApplicationCommandPermissionsUpdatedEventArgs> ApplicationCommandPermissionsUpdated
    {
        add => this.applicationCommandPermissionsUpdated.Register(value);
        remove => this.applicationCommandPermissionsUpdated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ApplicationCommandPermissionsUpdatedEventArgs> applicationCommandPermissionsUpdated;

    #endregion

    #region Channel

    /// <summary>
    /// Fired when a new channel is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelCreatedEventArgs> ChannelCreated
    {
        add => this.channelCreated.Register(value);
        remove => this.channelCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelCreatedEventArgs> channelCreated;

    /// <summary>
    /// Fired when a channel is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelUpdatedEventArgs> ChannelUpdated
    {
        add => this.channelUpdated.Register(value);
        remove => this.channelUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelUpdatedEventArgs> channelUpdated;

    /// <summary>
    /// Fired when a channel is deleted
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelDeletedEventArgs> ChannelDeleted
    {
        add => this.channelDeleted.Register(value);
        remove => this.channelDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelDeletedEventArgs> channelDeleted;

    /// <summary>
    /// Fired when a dm channel is deleted
    /// For this Event you need the <see cref="DiscordIntents.DirectMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, DmChannelDeletedEventArgs> DmChannelDeleted
    {
        add => this.dmChannelDeleted.Register(value);
        remove => this.dmChannelDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, DmChannelDeletedEventArgs> dmChannelDeleted;

    /// <summary>
    /// Fired whenever a channel's pinned message list is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelPinsUpdatedEventArgs> ChannelPinsUpdated
    {
        add => this.channelPinsUpdated.Register(value);
        remove => this.channelPinsUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelPinsUpdatedEventArgs> channelPinsUpdated;

    #endregion

    #region Guild

    /// <summary>
    /// Fired when the user joins a new guild.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    /// <remarks>[alias="GuildJoined"][alias="JoinedGuild"]</remarks>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildCreatedEventArgs> GuildCreated
    {
        add => this.guildCreated.Register(value);
        remove => this.guildCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildCreatedEventArgs> guildCreated;

    /// <summary>
    /// Fired when a guild is becoming available.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildAvailableEventArgs> GuildAvailable
    {
        add => this.guildAvailable.Register(value);
        remove => this.guildAvailable.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildAvailableEventArgs> guildAvailable;

    /// <summary>
    /// Fired when a guild is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildUpdatedEventArgs> GuildUpdated
    {
        add => this.guildUpdated.Register(value);
        remove => this.guildUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildUpdatedEventArgs> guildUpdated;

    /// <summary>
    /// Fired when the user leaves or is removed from a guild.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildDeletedEventArgs> GuildDeleted
    {
        add => this.guildDeleted.Register(value);
        remove => this.guildDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildDeletedEventArgs> guildDeleted;

    /// <summary>
    /// Fired when a guild becomes unavailable.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildUnavailableEventArgs> GuildUnavailable
    {
        add => this.guildUnavailable.Register(value);
        remove => this.guildUnavailable.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildUnavailableEventArgs> guildUnavailable;

    /// <summary>
    /// Fired when all guilds finish streaming from Discord.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildDownloadCompletedEventArgs> GuildDownloadCompleted
    {
        add => this.guildDownloadCompletedEv.Register(value);
        remove => this.guildDownloadCompletedEv.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs> guildDownloadCompletedEv;

    /// <summary>
    /// Fired when a guilds emojis get updated
    /// For this Event you need the <see cref="DiscordIntents.GuildEmojisAndStickers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildEmojisUpdatedEventArgs> GuildEmojisUpdated
    {
        add => this.guildEmojisUpdated.Register(value);
        remove => this.guildEmojisUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildEmojisUpdatedEventArgs> guildEmojisUpdated;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildStickersUpdatedEventArgs> GuildStickersUpdated
    {
        add => this.guildStickersUpdated.Register(value);
        remove => this.guildStickersUpdated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, GuildStickersUpdatedEventArgs> guildStickersUpdated;

    /// <summary>
    /// Fired when a guild integration is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildIntegrationsUpdatedEventArgs> GuildIntegrationsUpdated
    {
        add => this.guildIntegrationsUpdated.Register(value);
        remove => this.guildIntegrationsUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildIntegrationsUpdatedEventArgs> guildIntegrationsUpdated;

    /// <summary>
    /// Fired when a audit log entry is created.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildAuditLogCreatedEventArgs> GuildAuditLogCreated
    {
        add => this.guildAuditLogCreated.Register(value);
        remove => this.guildAuditLogCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildAuditLogCreatedEventArgs> guildAuditLogCreated;

    #endregion

    #region Scheduled Guild Events

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventCreatedEventArgs> ScheduledGuildEventCreated
    {
        add => this.scheduledGuildEventCreated.Register(value);
        remove => this.scheduledGuildEventCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventCreatedEventArgs> scheduledGuildEventCreated;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUpdatedEventArgs> ScheduledGuildEventUpdated
    {
        add => this.scheduledGuildEventUpdated.Register(value);
        remove => this.scheduledGuildEventUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventUpdatedEventArgs> scheduledGuildEventUpdated;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventDeletedEventArgs> ScheduledGuildEventDeleted
    {
        add => this.scheduledGuildEventDeleted.Register(value);
        remove => this.scheduledGuildEventDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventDeletedEventArgs> scheduledGuildEventDeleted;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventCompletedEventArgs> ScheduledGuildEventCompleted
    {
        add => this.scheduledGuildEventCompleted.Register(value);
        remove => this.scheduledGuildEventCompleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventCompletedEventArgs> scheduledGuildEventCompleted;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUserAddedEventArgs> ScheduledGuildEventUserAdded
    {
        add => this.scheduledGuildEventUserAdded.Register(value);
        remove => this.scheduledGuildEventUserAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventUserAddedEventArgs> scheduledGuildEventUserAdded;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUserRemovedEventArgs> ScheduledGuildEventUserRemoved
    {
        add => this.scheduledGuildEventUserRemoved.Register(value);
        remove => this.scheduledGuildEventUserRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventUserRemovedEventArgs> scheduledGuildEventUserRemoved;

    #endregion

    #region Guild Ban

    /// <summary>
    /// Fired when a guild ban gets added
    /// For this Event you need the <see cref="DiscordIntents.GuildModeration"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildBanAddedEventArgs> GuildBanAdded
    {
        add => this.guildBanAdded.Register(value);
        remove => this.guildBanAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildBanAddedEventArgs> guildBanAdded;

    /// <summary>
    /// Fired when a guild ban gets removed
    /// For this Event you need the <see cref="DiscordIntents.GuildModeration"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildBanRemovedEventArgs> GuildBanRemoved
    {
        add => this.guildBanRemoved.Register(value);
        remove => this.guildBanRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildBanRemovedEventArgs> guildBanRemoved;

    #endregion

    #region Guild Member

    /// <summary>
    /// Fired when a new user joins a guild.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMemberAddedEventArgs> GuildMemberAdded
    {
        add => this.guildMemberAdded.Register(value);
        remove => this.guildMemberAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMemberAddedEventArgs> guildMemberAdded;

    /// <summary>
    /// Fired when a user is removed from a guild (leave/kick/ban).
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMemberRemovedEventArgs> GuildMemberRemoved
    {
        add => this.guildMemberRemoved.Register(value);
        remove => this.guildMemberRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMemberRemovedEventArgs> guildMemberRemoved;

    /// <summary>
    /// Fired when a guild member is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMemberUpdatedEventArgs> GuildMemberUpdated
    {
        add => this.guildMemberUpdated.Register(value);
        remove => this.guildMemberUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMemberUpdatedEventArgs> guildMemberUpdated;

    /// <summary>
    /// Fired in response to Gateway Request Guild Members.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMembersChunkedEventArgs> GuildMembersChunked
    {
        add => this.guildMembersChunked.Register(value);
        remove => this.guildMembersChunked.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMembersChunkedEventArgs> guildMembersChunked;

    #endregion

    #region Guild Role

    /// <summary>
    /// Fired when a guild role is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildRoleCreatedEventArgs> GuildRoleCreated
    {
        add => this.guildRoleCreated.Register(value);
        remove => this.guildRoleCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildRoleCreatedEventArgs> guildRoleCreated;

    /// <summary>
    /// Fired when a guild role is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildRoleUpdatedEventArgs> GuildRoleUpdated
    {
        add => this.guildRoleUpdated.Register(value);
        remove => this.guildRoleUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildRoleUpdatedEventArgs> guildRoleUpdated;

    /// <summary>
    /// Fired when a guild role is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildRoleDeletedEventArgs> GuildRoleDeleted
    {
        add => this.guildRoleDeleted.Register(value);
        remove => this.guildRoleDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildRoleDeletedEventArgs> guildRoleDeleted;

    #endregion

    #region Invite

    /// <summary>
    /// Fired when an invite is created.
    /// For this Event you need the <see cref="DiscordIntents.GuildInvites"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, InviteCreatedEventArgs> InviteCreated
    {
        add => this.inviteCreated.Register(value);
        remove => this.inviteCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, InviteCreatedEventArgs> inviteCreated;

    /// <summary>
    /// Fired when an invite is deleted.
    /// For this Event you need the <see cref="DiscordIntents.GuildInvites"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, InviteDeletedEventArgs> InviteDeleted
    {
        add => this.inviteDeleted.Register(value);
        remove => this.inviteDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, InviteDeletedEventArgs> inviteDeleted;

    #endregion

    #region Message

    /// <summary>
    /// Fired when a message is created.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageCreatedEventArgs> MessageCreated
    {
        add => this.messageCreated.Register(value);
        remove => this.messageCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageCreatedEventArgs> messageCreated;

    /// <summary>
    /// Fired when a message is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageUpdatedEventArgs> MessageUpdated
    {
        add => this.messageUpdated.Register(value);
        remove => this.messageUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageUpdatedEventArgs> messageUpdated;

    /// <summary>
    /// Fired when a message is deleted.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageDeletedEventArgs> MessageDeleted
    {
        add => this.messageDeleted.Register(value);
        remove => this.messageDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageDeletedEventArgs> messageDeleted;

    /// <summary>
    /// Fired when multiple messages are deleted at once.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessagesBulkDeletedEventArgs> MessagesBulkDeleted
    {
        add => this.messagesBulkDeleted.Register(value);
        remove => this.messagesBulkDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessagesBulkDeletedEventArgs> messagesBulkDeleted;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessagePollVotedEventArgs> MessagePollVoted
    {
        add => this.messagePollVoted.Register(value);
        remove => this.messagePollVoted.Unregister(value);
    }

    private AsyncEvent<DiscordClient, MessagePollVotedEventArgs> messagePollVoted;

    #endregion

    #region Message Reaction

    /// <summary>
    /// Fired when a reaction gets added to a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionAddedEventArgs> MessageReactionAdded
    {
        add => this.messageReactionAdded.Register(value);
        remove => this.messageReactionAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionAddedEventArgs> messageReactionAdded;

    /// <summary>
    /// Fired when a reaction gets removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionRemovedEventArgs> MessageReactionRemoved
    {
        add => this.messageReactionRemoved.Register(value);
        remove => this.messageReactionRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionRemovedEventArgs> messageReactionRemoved;

    /// <summary>
    /// Fired when all reactions get removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionsClearedEventArgs> MessageReactionsCleared
    {
        add => this.messageReactionsCleared.Register(value);
        remove => this.messageReactionsCleared.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionsClearedEventArgs> messageReactionsCleared;

    /// <summary>
    /// Fired when all reactions of a specific reaction are removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionRemovedEmojiEventArgs> MessageReactionRemovedEmoji
    {
        add => this.messageReactionRemovedEmoji.Register(value);
        remove => this.messageReactionRemovedEmoji.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionRemovedEmojiEventArgs> messageReactionRemovedEmoji;

    #endregion

    #region Presence/User Update

    /// <summary>
    /// Fired when a presence has been updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, PresenceUpdatedEventArgs> PresenceUpdated
    {
        add => this.presenceUpdated.Register(value);
        remove => this.presenceUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, PresenceUpdatedEventArgs> presenceUpdated;

    /// <summary>
    /// Fired when the current user updates their settings.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, UserSettingsUpdatedEventArgs> UserSettingsUpdated
    {
        add => this.userSettingsUpdated.Register(value);
        remove => this.userSettingsUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, UserSettingsUpdatedEventArgs> userSettingsUpdated;

    /// <summary>
    /// Fired when properties about the current user change.
    /// </summary>
    /// <remarks>
    /// NB: This event only applies for changes to the <b>current user</b>, the client that is connected to Discord.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </remarks>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, UserUpdatedEventArgs> UserUpdated
    {
        add => this.userUpdated.Register(value);
        remove => this.userUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, UserUpdatedEventArgs> userUpdated;

    #endregion

    #region Voice

    /// <summary>
    /// Fired when someone joins/leaves/moves voice channels.
    /// For this Event you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, VoiceStateUpdatedEventArgs> VoiceStateUpdated
    {
        add => this.voiceStateUpdated.Register(value);
        remove => this.voiceStateUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, VoiceStateUpdatedEventArgs> voiceStateUpdated;

    /// <summary>
    /// Fired when a guild's voice server is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, VoiceServerUpdatedEventArgs> VoiceServerUpdated
    {
        add => this.voiceServerUpdated.Register(value);
        remove => this.voiceServerUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, VoiceServerUpdatedEventArgs> voiceServerUpdated;

    #endregion

    #region Thread

    /// <summary>
    /// Fired when a thread is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadCreatedEventArgs> ThreadCreated
    {
        add => this.threadCreated.Register(value);
        remove => this.threadCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadCreatedEventArgs> threadCreated;

    /// <summary>
    /// Fired when a thread is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadUpdatedEventArgs> ThreadUpdated
    {
        add => this.threadUpdated.Register(value);
        remove => this.threadUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadUpdatedEventArgs> threadUpdated;

    /// <summary>
    /// Fired when a thread is deleted.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadDeletedEventArgs> ThreadDeleted
    {
        add => this.threadDeleted.Register(value);
        remove => this.threadDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadDeletedEventArgs> threadDeleted;

    /// <summary>
    /// Fired when the current member gains access to a channel(s) that has threads.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadListSyncedEventArgs> ThreadListSynced
    {
        add => this.threadListSynced.Register(value);
        remove => this.threadListSynced.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadListSyncedEventArgs> threadListSynced;

    /// <summary>
    /// Fired when the thread member for the current user is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    /// <remarks>
    /// This event is primarily implemented for completeness and unlikely to be useful to bots.
    /// </remarks>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadMemberUpdatedEventArgs> ThreadMemberUpdated
    {
        add => this.threadMemberUpdated.Register(value);
        remove => this.threadMemberUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadMemberUpdatedEventArgs> threadMemberUpdated;

    /// <summary>
    /// Fired when the thread members are updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> or <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadMembersUpdatedEventArgs> ThreadMembersUpdated
    {
        add => this.threadMembersUpdated.Register(value);
        remove => this.threadMembersUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadMembersUpdatedEventArgs> threadMembersUpdated;

    #endregion

    #region Integration

    /// <summary>
    /// Fired when an integration is created.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, IntegrationCreatedEventArgs> IntegrationCreated
    {
        add => this.integrationCreated.Register(value);
        remove => this.integrationCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, IntegrationCreatedEventArgs> integrationCreated;

    /// <summary>
    /// Fired when an integration is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, IntegrationUpdatedEventArgs> IntegrationUpdated
    {
        add => this.integrationUpdated.Register(value);
        remove => this.integrationUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, IntegrationUpdatedEventArgs> integrationUpdated;

    /// <summary>
    /// Fired when an integration is deleted.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, IntegrationDeletedEventArgs> IntegrationDeleted
    {
        add => this.integrationDeleted.Register(value);
        remove => this.integrationDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, IntegrationDeletedEventArgs> integrationDeleted;

    #endregion

    #region Stage Instance

    /// <summary>
    /// Fired when a stage instance is created.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, StageInstanceCreatedEventArgs> StageInstanceCreated
    {
        add => this.stageInstanceCreated.Register(value);
        remove => this.stageInstanceCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, StageInstanceCreatedEventArgs> stageInstanceCreated;

    /// <summary>
    /// Fired when a stage instance is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, StageInstanceUpdatedEventArgs> StageInstanceUpdated
    {
        add => this.stageInstanceUpdated.Register(value);
        remove => this.stageInstanceUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, StageInstanceUpdatedEventArgs> stageInstanceUpdated;

    /// <summary>
    /// Fired when a stage instance is deleted.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, StageInstanceDeletedEventArgs> StageInstanceDeleted
    {
        add => this.stageInstanceDeleted.Register(value);
        remove => this.stageInstanceDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, StageInstanceDeletedEventArgs> stageInstanceDeleted;

    #endregion

    #region Misc

    /// <summary>
    /// Fired when an interaction is invoked.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, InteractionCreatedEventArgs> InteractionCreated
    {
        add => this.interactionCreated.Register(value);
        remove => this.interactionCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, InteractionCreatedEventArgs> interactionCreated;

    /// <summary>
    /// Fired when a component is invoked.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ComponentInteractionCreatedEventArgs> ComponentInteractionCreated
    {
        add => this.componentInteractionCreated.Register(value);
        remove => this.componentInteractionCreated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ComponentInteractionCreatedEventArgs> componentInteractionCreated;

    /// <summary>
    /// Fired when a modal is submitted. If a modal is closed, this event is not fired.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ModalSubmittedEventArgs> ModalSubmitted
    {
        add => this.modalSubmitted.Register(value);
        remove => this.modalSubmitted.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ModalSubmittedEventArgs> modalSubmitted;

    /// <summary>
    /// Fired when a user uses a context menu.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ContextMenuInteractionCreatedEventArgs> ContextMenuInteractionCreated
    {
        add => this.contextMenuInteractionCreated.Register(value);
        remove => this.contextMenuInteractionCreated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ContextMenuInteractionCreatedEventArgs> contextMenuInteractionCreated;

    /// <summary>
    /// Fired when a user starts typing in a channel.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, TypingStartedEventArgs> TypingStarted
    {
        add => this.typingStarted.Register(value);
        remove => this.typingStarted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, TypingStartedEventArgs> typingStarted;

    /// <summary>
    /// Fired when an unknown event gets received.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, UnknownEventArgs> UnknownEvent
    {
        add => this.unknownEvent.Register(value);
        remove => this.unknownEvent.Unregister(value);
    }
    private AsyncEvent<DiscordClient, UnknownEventArgs> unknownEvent;

    /// <summary>
    /// Fired whenever webhooks update.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, WebhooksUpdatedEventArgs> WebhooksUpdated
    {
        add => this.webhooksUpdated.Register(value);
        remove => this.webhooksUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, WebhooksUpdatedEventArgs> webhooksUpdated;

    /// <summary>
    /// Fired whenever an error occurs within an event handler.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ClientErrorEventArgs> ClientErrored
    {
        add => this.clientErrored.Register(value);
        remove => this.clientErrored.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ClientErrorEventArgs> clientErrored;

    #endregion

    #region Error Handling

    internal void EventErrorHandler<TSender, TArgs>(AsyncEvent<TSender, TArgs> asyncEvent, Exception ex, AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs
    {
        this.Logger.LogError(LoggerEvents.EventHandlerException, ex, "Event handler exception for event {Event} thrown from {Method} (defined in {DeclaryingType})", asyncEvent.Name, handler.Method, handler.Method.DeclaringType);
        this.clientErrored.InvokeAsync(this, new ClientErrorEventArgs { EventName = asyncEvent.Name, Exception = ex }).GetAwaiter().GetResult();
    }

    private void Goof<TSender, TArgs>(AsyncEvent<TSender, TArgs> asyncEvent, Exception ex, AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs => this.Logger.LogCritical(LoggerEvents.EventHandlerException, ex, "Exception event handler {Method} (defined in {DeclaringType}) threw an exception", handler.Method, handler.Method.DeclaringType);

    #endregion

    #region AutoModeration

    /// <summary>
    /// Fired when a new auto-moderation rule is created.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleCreatedEventArgs> AutoModerationRuleCreated
    {
        add => this.autoModerationRuleCreated.Register(value);
        remove => this.autoModerationRuleCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleCreatedEventArgs> autoModerationRuleCreated;

    /// <summary>
    /// Fired when an auto-moderation rule is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleUpdatedEventArgs> AutoModerationRuleUpdated
    {
        add => this.autoModerationRuleUpdated.Register(value);
        remove => this.autoModerationRuleUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleUpdatedEventArgs> autoModerationRuleUpdated;

    /// <summary>
    /// Fired when an auto-moderation rule is deleted.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleDeletedEventArgs> AutoModerationRuleDeleted
    {
        add => this.autoModerationRuleDeleted.Register(value);
        remove => this.autoModerationRuleDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleDeletedEventArgs> autoModerationRuleDeleted;

    /// <summary>
    /// Fired when an auto-moderation is executed.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleExecutedEventArgs> AutoModerationRuleExecuted
    {
        add => this.autoModerationRuleExecuted.Register(value);
        remove => this.autoModerationRuleExecuted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleExecutedEventArgs> autoModerationRuleExecuted;
    #endregion
}
