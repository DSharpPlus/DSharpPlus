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
    public event AsyncEventHandler<DiscordClient, SocketEventArgs> SocketOpened
    {
        add => this.socketOpened.Register(value);
        remove => this.socketOpened.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SocketEventArgs> socketOpened;

    /// <summary>
    /// Fired whenever WebSocket connection is terminated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, SocketCloseEventArgs> SocketClosed
    {
        add => this.socketClosed.Register(value);
        remove => this.socketClosed.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SocketCloseEventArgs> socketClosed;

    /// <summary>
    /// Fired when this client has successfully completed its handshake with the websocket gateway.
    /// </summary>
    /// <remarks>
    /// <i><see cref="Guilds"/> will not be populated when this event is fired.</i><br/>
    /// See also: <see cref="GuildAvailable"/>, <see cref="GuildDownloadCompleted"/>
    /// </remarks>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, SessionReadyEventArgs> SessionCreated
    {
        add => this.ready.Register(value);
        remove => this.ready.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SessionReadyEventArgs> ready;

    /// <summary>
    /// Fired whenever a session is resumed.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, SessionReadyEventArgs> SessionResumed
    {
        add => this.resumed.Register(value);
        remove => this.resumed.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SessionReadyEventArgs> resumed;

    /// <summary>
    /// Fired on received heartbeat ACK.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, HeartbeatEventArgs> Heartbeated
    {
        add => this.heartbeated.Register(value);
        remove => this.heartbeated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, HeartbeatEventArgs> heartbeated;

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
    public event AsyncEventHandler<DiscordClient, ChannelCreateEventArgs> ChannelCreated
    {
        add => this.channelCreated.Register(value);
        remove => this.channelCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelCreateEventArgs> channelCreated;

    /// <summary>
    /// Fired when a channel is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelUpdateEventArgs> ChannelUpdated
    {
        add => this.channelUpdated.Register(value);
        remove => this.channelUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelUpdateEventArgs> channelUpdated;

    /// <summary>
    /// Fired when a channel is deleted
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelDeleteEventArgs> ChannelDeleted
    {
        add => this.channelDeleted.Register(value);
        remove => this.channelDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelDeleteEventArgs> channelDeleted;

    /// <summary>
    /// Fired when a dm channel is deleted
    /// For this Event you need the <see cref="DiscordIntents.DirectMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, DmChannelDeleteEventArgs> DmChannelDeleted
    {
        add => this.dmChannelDeleted.Register(value);
        remove => this.dmChannelDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, DmChannelDeleteEventArgs> dmChannelDeleted;

    /// <summary>
    /// Fired whenever a channel's pinned message list is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelPinsUpdateEventArgs> ChannelPinsUpdated
    {
        add => this.channelPinsUpdated.Register(value);
        remove => this.channelPinsUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs> channelPinsUpdated;

    #endregion

    #region Guild

    /// <summary>
    /// Fired when the user joins a new guild.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    /// <remarks>[alias="GuildJoined"][alias="JoinedGuild"]</remarks>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildCreateEventArgs> GuildCreated
    {
        add => this.guildCreated.Register(value);
        remove => this.guildCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildCreateEventArgs> guildCreated;

    /// <summary>
    /// Fired when a guild is becoming available.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildCreateEventArgs> GuildAvailable
    {
        add => this.guildAvailable.Register(value);
        remove => this.guildAvailable.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildCreateEventArgs> guildAvailable;

    /// <summary>
    /// Fired when a guild is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildUpdateEventArgs> GuildUpdated
    {
        add => this.guildUpdated.Register(value);
        remove => this.guildUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildUpdateEventArgs> guildUpdated;

    /// <summary>
    /// Fired when the user leaves or is removed from a guild.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildDeleteEventArgs> GuildDeleted
    {
        add => this.guildDeleted.Register(value);
        remove => this.guildDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildDeleteEventArgs> guildDeleted;

    /// <summary>
    /// Fired when a guild becomes unavailable.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildDeleteEventArgs> GuildUnavailable
    {
        add => this.guildUnavailable.Register(value);
        remove => this.guildUnavailable.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildDeleteEventArgs> guildUnavailable;

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
    public event AsyncEventHandler<DiscordClient, GuildEmojisUpdateEventArgs> GuildEmojisUpdated
    {
        add => this.guildEmojisUpdated.Register(value);
        remove => this.guildEmojisUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs> guildEmojisUpdated;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildStickersUpdateEventArgs> GuildStickersUpdated
    {
        add => this.guildStickersUpdated.Register(value);
        remove => this.guildStickersUpdated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, GuildStickersUpdateEventArgs> guildStickersUpdated;

    /// <summary>
    /// Fired when a guild integration is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
    {
        add => this.guildIntegrationsUpdated.Register(value);
        remove => this.guildIntegrationsUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs> guildIntegrationsUpdated;

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
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventCreateEventArgs> ScheduledGuildEventCreated
    {
        add => this.scheduledGuildEventCreated.Register(value);
        remove => this.scheduledGuildEventCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventCreateEventArgs> scheduledGuildEventCreated;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUpdateEventArgs> ScheduledGuildEventUpdated
    {
        add => this.scheduledGuildEventUpdated.Register(value);
        remove => this.scheduledGuildEventUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventUpdateEventArgs> scheduledGuildEventUpdated;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventDeleteEventArgs> ScheduledGuildEventDeleted
    {
        add => this.scheduledGuildEventDeleted.Register(value);
        remove => this.scheduledGuildEventDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventDeleteEventArgs> scheduledGuildEventDeleted;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventCompletedEventArgs> ScheduledGuildEventCompleted
    {
        add => this.scheduledGuildEventCompleted.Register(value);
        remove => this.scheduledGuildEventCompleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventCompletedEventArgs> scheduledGuildEventCompleted;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUserAddEventArgs> ScheduledGuildEventUserAdded
    {
        add => this.scheduledGuildEventUserAdded.Register(value);
        remove => this.scheduledGuildEventUserAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventUserAddEventArgs> scheduledGuildEventUserAdded;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUserRemoveEventArgs> ScheduledGuildEventUserRemoved
    {
        add => this.scheduledGuildEventUserRemoved.Register(value);
        remove => this.scheduledGuildEventUserRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventUserRemoveEventArgs> scheduledGuildEventUserRemoved;

    #endregion

    #region Guild Ban

    /// <summary>
    /// Fired when a guild ban gets added
    /// For this Event you need the <see cref="DiscordIntents.GuildModeration"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildBanAddEventArgs> GuildBanAdded
    {
        add => this.guildBanAdded.Register(value);
        remove => this.guildBanAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildBanAddEventArgs> guildBanAdded;

    /// <summary>
    /// Fired when a guild ban gets removed
    /// For this Event you need the <see cref="DiscordIntents.GuildModeration"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildBanRemoveEventArgs> GuildBanRemoved
    {
        add => this.guildBanRemoved.Register(value);
        remove => this.guildBanRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildBanRemoveEventArgs> guildBanRemoved;

    #endregion

    #region Guild Member

    /// <summary>
    /// Fired when a new user joins a guild.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMemberAddEventArgs> GuildMemberAdded
    {
        add => this.guildMemberAdded.Register(value);
        remove => this.guildMemberAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMemberAddEventArgs> guildMemberAdded;

    /// <summary>
    /// Fired when a user is removed from a guild (leave/kick/ban).
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMemberRemoveEventArgs> GuildMemberRemoved
    {
        add => this.guildMemberRemoved.Register(value);
        remove => this.guildMemberRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs> guildMemberRemoved;

    /// <summary>
    /// Fired when a guild member is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMemberUpdateEventArgs> GuildMemberUpdated
    {
        add => this.guildMemberUpdated.Register(value);
        remove => this.guildMemberUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs> guildMemberUpdated;

    /// <summary>
    /// Fired in response to Gateway Request Guild Members.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMembersChunkEventArgs> GuildMembersChunked
    {
        add => this.guildMembersChunked.Register(value);
        remove => this.guildMembersChunked.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMembersChunkEventArgs> guildMembersChunked;

    #endregion

    #region Guild Role

    /// <summary>
    /// Fired when a guild role is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildRoleCreateEventArgs> GuildRoleCreated
    {
        add => this.guildRoleCreated.Register(value);
        remove => this.guildRoleCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildRoleCreateEventArgs> guildRoleCreated;

    /// <summary>
    /// Fired when a guild role is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildRoleUpdateEventArgs> GuildRoleUpdated
    {
        add => this.guildRoleUpdated.Register(value);
        remove => this.guildRoleUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs> guildRoleUpdated;

    /// <summary>
    /// Fired when a guild role is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildRoleDeleteEventArgs> GuildRoleDeleted
    {
        add => this.guildRoleDeleted.Register(value);
        remove => this.guildRoleDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs> guildRoleDeleted;

    #endregion

    #region Invite

    /// <summary>
    /// Fired when an invite is created.
    /// For this Event you need the <see cref="DiscordIntents.GuildInvites"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, InviteCreateEventArgs> InviteCreated
    {
        add => this.inviteCreated.Register(value);
        remove => this.inviteCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, InviteCreateEventArgs> inviteCreated;

    /// <summary>
    /// Fired when an invite is deleted.
    /// For this Event you need the <see cref="DiscordIntents.GuildInvites"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, InviteDeleteEventArgs> InviteDeleted
    {
        add => this.inviteDeleted.Register(value);
        remove => this.inviteDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, InviteDeleteEventArgs> inviteDeleted;

    #endregion

    #region Message

    /// <summary>
    /// Fired when a message is created.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageCreateEventArgs> MessageCreated
    {
        add => this.messageCreated.Register(value);
        remove => this.messageCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageCreateEventArgs> messageCreated;

    /// <summary>
    /// Fired when a message is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageUpdateEventArgs> MessageUpdated
    {
        add => this.messageUpdated.Register(value);
        remove => this.messageUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageUpdateEventArgs> messageUpdated;

    /// <summary>
    /// Fired when a message is deleted.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageDeleteEventArgs> MessageDeleted
    {
        add => this.messageDeleted.Register(value);
        remove => this.messageDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageDeleteEventArgs> messageDeleted;

    /// <summary>
    /// Fired when multiple messages are deleted at once.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageBulkDeleteEventArgs> MessagesBulkDeleted
    {
        add => this.messagesBulkDeleted.Register(value);
        remove => this.messagesBulkDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs> messagesBulkDeleted;

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessagePollVoteEventArgs> MessagePollVoted
    {
        add => this.messagePollVoted.Register(value);
        remove => this.messagePollVoted.Unregister(value);
    }

    private AsyncEvent<DiscordClient, MessagePollVoteEventArgs> messagePollVoted;

    #endregion

    #region Message Reaction

    /// <summary>
    /// Fired when a reaction gets added to a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs> MessageReactionAdded
    {
        add => this.messageReactionAdded.Register(value);
        remove => this.messageReactionAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionAddEventArgs> messageReactionAdded;

    /// <summary>
    /// Fired when a reaction gets removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs> MessageReactionRemoved
    {
        add => this.messageReactionRemoved.Register(value);
        remove => this.messageReactionRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs> messageReactionRemoved;

    /// <summary>
    /// Fired when all reactions get removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionsClearEventArgs> MessageReactionsCleared
    {
        add => this.messageReactionsCleared.Register(value);
        remove => this.messageReactionsCleared.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionsClearEventArgs> messageReactionsCleared;

    /// <summary>
    /// Fired when all reactions of a specific reaction are removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionRemoveEmojiEventArgs> MessageReactionRemovedEmoji
    {
        add => this.messageReactionRemovedEmoji.Register(value);
        remove => this.messageReactionRemovedEmoji.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs> messageReactionRemovedEmoji;

    #endregion

    #region Presence/User Update

    /// <summary>
    /// Fired when a presence has been updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, PresenceUpdateEventArgs> PresenceUpdated
    {
        add => this.presenceUpdated.Register(value);
        remove => this.presenceUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, PresenceUpdateEventArgs> presenceUpdated;

    /// <summary>
    /// Fired when the current user updates their settings.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, UserSettingsUpdateEventArgs> UserSettingsUpdated
    {
        add => this.userSettingsUpdated.Register(value);
        remove => this.userSettingsUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs> userSettingsUpdated;

    /// <summary>
    /// Fired when properties about the current user change.
    /// </summary>
    /// <remarks>
    /// NB: This event only applies for changes to the <b>current user</b>, the client that is connected to Discord.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </remarks>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, UserUpdateEventArgs> UserUpdated
    {
        add => this.userUpdated.Register(value);
        remove => this.userUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, UserUpdateEventArgs> userUpdated;

    #endregion

    #region Voice

    /// <summary>
    /// Fired when someone joins/leaves/moves voice channels.
    /// For this Event you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, VoiceStateUpdateEventArgs> VoiceStateUpdated
    {
        add => this.voiceStateUpdated.Register(value);
        remove => this.voiceStateUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs> voiceStateUpdated;

    /// <summary>
    /// Fired when a guild's voice server is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, VoiceServerUpdateEventArgs> VoiceServerUpdated
    {
        add => this.voiceServerUpdated.Register(value);
        remove => this.voiceServerUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs> voiceServerUpdated;

    #endregion

    #region Thread

    /// <summary>
    /// Fired when a thread is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadCreateEventArgs> ThreadCreated
    {
        add => this.threadCreated.Register(value);
        remove => this.threadCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadCreateEventArgs> threadCreated;

    /// <summary>
    /// Fired when a thread is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadUpdateEventArgs> ThreadUpdated
    {
        add => this.threadUpdated.Register(value);
        remove => this.threadUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadUpdateEventArgs> threadUpdated;

    /// <summary>
    /// Fired when a thread is deleted.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadDeleteEventArgs> ThreadDeleted
    {
        add => this.threadDeleted.Register(value);
        remove => this.threadDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadDeleteEventArgs> threadDeleted;

    /// <summary>
    /// Fired when the current member gains access to a channel(s) that has threads.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadListSyncEventArgs> ThreadListSynced
    {
        add => this.threadListSynced.Register(value);
        remove => this.threadListSynced.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadListSyncEventArgs> threadListSynced;

    /// <summary>
    /// Fired when the thread member for the current user is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    /// <remarks>
    /// This event is primarily implemented for completeness and unlikely to be useful to bots.
    /// </remarks>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadMemberUpdateEventArgs> ThreadMemberUpdated
    {
        add => this.threadMemberUpdated.Register(value);
        remove => this.threadMemberUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadMemberUpdateEventArgs> threadMemberUpdated;

    /// <summary>
    /// Fired when the thread members are updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> or <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadMembersUpdateEventArgs> ThreadMembersUpdated
    {
        add => this.threadMembersUpdated.Register(value);
        remove => this.threadMembersUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadMembersUpdateEventArgs> threadMembersUpdated;

    #endregion

    #region Integration

    /// <summary>
    /// Fired when an integration is created.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, IntegrationCreateEventArgs> IntegrationCreated
    {
        add => this.integrationCreated.Register(value);
        remove => this.integrationCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, IntegrationCreateEventArgs> integrationCreated;

    /// <summary>
    /// Fired when an integration is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, IntegrationUpdateEventArgs> IntegrationUpdated
    {
        add => this.integrationUpdated.Register(value);
        remove => this.integrationUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, IntegrationUpdateEventArgs> integrationUpdated;

    /// <summary>
    /// Fired when an integration is deleted.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, IntegrationDeleteEventArgs> IntegrationDeleted
    {
        add => this.integrationDeleted.Register(value);
        remove => this.integrationDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, IntegrationDeleteEventArgs> integrationDeleted;

    #endregion

    #region Stage Instance

    /// <summary>
    /// Fired when a stage instance is created.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, StageInstanceCreateEventArgs> StageInstanceCreated
    {
        add => this.stageInstanceCreated.Register(value);
        remove => this.stageInstanceCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, StageInstanceCreateEventArgs> stageInstanceCreated;

    /// <summary>
    /// Fired when a stage instance is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, StageInstanceUpdateEventArgs> StageInstanceUpdated
    {
        add => this.stageInstanceUpdated.Register(value);
        remove => this.stageInstanceUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, StageInstanceUpdateEventArgs> stageInstanceUpdated;

    /// <summary>
    /// Fired when a stage instance is deleted.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, StageInstanceDeleteEventArgs> StageInstanceDeleted
    {
        add => this.stageInstanceDeleted.Register(value);
        remove => this.stageInstanceDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, StageInstanceDeleteEventArgs> stageInstanceDeleted;

    #endregion

    #region Misc

    /// <summary>
    /// Fired when an interaction is invoked.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, InteractionCreateEventArgs> InteractionCreated
    {
        add => this.interactionCreated.Register(value);
        remove => this.interactionCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, InteractionCreateEventArgs> interactionCreated;

    /// <summary>
    /// Fired when a component is invoked.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> ComponentInteractionCreated
    {
        add => this.componentInteractionCreated.Register(value);
        remove => this.componentInteractionCreated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ComponentInteractionCreateEventArgs> componentInteractionCreated;

    /// <summary>
    /// Fired when a modal is submitted. If a modal is closed, this event is not fired.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ModalSubmitEventArgs> ModalSubmitted
    {
        add => this.modalSubmitted.Register(value);
        remove => this.modalSubmitted.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ModalSubmitEventArgs> modalSubmitted;

    /// <summary>
    /// Fired when a user uses a context menu.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ContextMenuInteractionCreateEventArgs> ContextMenuInteractionCreated
    {
        add => this.contextMenuInteractionCreated.Register(value);
        remove => this.contextMenuInteractionCreated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ContextMenuInteractionCreateEventArgs> contextMenuInteractionCreated;

    /// <summary>
    /// Fired when a user starts typing in a channel.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, TypingStartEventArgs> TypingStarted
    {
        add => this.typingStarted.Register(value);
        remove => this.typingStarted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, TypingStartEventArgs> typingStarted;

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
    public event AsyncEventHandler<DiscordClient, WebhooksUpdateEventArgs> WebhooksUpdated
    {
        add => this.webhooksUpdated.Register(value);
        remove => this.webhooksUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, WebhooksUpdateEventArgs> webhooksUpdated;

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
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleCreateEventArgs> AutoModerationRuleCreated
    {
        add => this.autoModerationRuleCreated.Register(value);
        remove => this.autoModerationRuleCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleCreateEventArgs> autoModerationRuleCreated;

    /// <summary>
    /// Fired when an auto-moderation rule is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleUpdateEventArgs> AutoModerationRuleUpdated
    {
        add => this.autoModerationRuleUpdated.Register(value);
        remove => this.autoModerationRuleUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleUpdateEventArgs> autoModerationRuleUpdated;

    /// <summary>
    /// Fired when an auto-moderation rule is deleted.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleDeleteEventArgs> AutoModerationRuleDeleted
    {
        add => this.autoModerationRuleDeleted.Register(value);
        remove => this.autoModerationRuleDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleDeleteEventArgs> autoModerationRuleDeleted;

    /// <summary>
    /// Fired when an auto-moderation is executed.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleExecuteEventArgs> AutoModerationRuleExecuted
    {
        add => this.autoModerationRuleExecuted.Register(value);
        remove => this.autoModerationRuleExecuted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleExecuteEventArgs> autoModerationRuleExecuted;
    #endregion
}
