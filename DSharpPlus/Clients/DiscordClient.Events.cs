using System;

using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;

namespace DSharpPlus;

public sealed partial class DiscordClient
{
    // oh lord why did you have to pack into regions
    // this makes simple copy-paste ineffective
    // :notlikethis:
    //
    // i gotchu, they're removed now <3

    private const string ObsoletionMessage = "Events on DiscordClient are deprecated and will be removed within the v5 development cycle. " +
        "Please use the ConfigureEventHandlers methods on your preferred construction method instead.";

    /// <summary>
    /// Fired whenever a WebSocket error occurs within the client.
    /// </summary>
    [Obsolete("This event is superseded by implementing/shimming IClientErrorHandler", true, DiagnosticId = "DSP0003")]
    public event AsyncEventHandler<DiscordClient, SocketErrorEventArgs> SocketErrored
    {
        add => throw new NotSupportedException();
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired whenever WebSocket connection is established.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, SocketOpenedEventArgs> SocketOpened
    {
        add => this.events[typeof(SocketOpenedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired whenever WebSocket connection is terminated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, SocketClosedEventArgs> SocketClosed
    {
        add => this.events[typeof(SocketClosedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

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
        add => this.events[typeof(SessionCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired whenever a session is resumed.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, SessionResumedEventArgs> SessionResumed
    {
        add => this.events[typeof(SessionResumedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired on received heartbeat ACK.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, HeartbeatedEventArgs> Heartbeated
    {
        add => this.events[typeof(HeartbeatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired on heartbeat attempt cancellation due to too many failed heartbeats.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ZombiedEventArgs> Zombied
    {
        add => this.events[typeof(ZombiedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ApplicationCommandPermissionsUpdatedEventArgs> ApplicationCommandPermissionsUpdated
    {
        add => this.events[typeof(ApplicationCommandPermissionsUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a new channel is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelCreatedEventArgs> ChannelCreated
    {
        add => this.events[typeof(ChannelCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a channel is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelUpdatedEventArgs> ChannelUpdated
    {
        add => this.events[typeof(ChannelUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a channel is deleted
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelDeletedEventArgs> ChannelDeleted
    {
        add => this.events[typeof(ChannelDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a dm channel is deleted
    /// For this Event you need the <see cref="DiscordIntents.DirectMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, DmChannelDeletedEventArgs> DmChannelDeleted
    {
        add => this.events[typeof(DmChannelDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired whenever a channel's pinned message list is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ChannelPinsUpdatedEventArgs> ChannelPinsUpdated
    {
        add => this.events[typeof(ChannelPinsUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when the user joins a new guild.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    /// <remarks>[alias="GuildJoined"][alias="JoinedGuild"]</remarks>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildCreatedEventArgs> GuildCreated
    {
        add => this.events[typeof(GuildCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild is becoming available.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildAvailableEventArgs> GuildAvailable
    {
        add => this.events[typeof(GuildAvailableEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildUpdatedEventArgs> GuildUpdated
    {
        add => this.events[typeof(GuildUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when the user leaves or is removed from a guild.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildDeletedEventArgs> GuildDeleted
    {
        add => this.events[typeof(GuildDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild becomes unavailable.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildUnavailableEventArgs> GuildUnavailable
    {
        add => this.events[typeof(GuildUnavailableEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when all guilds finish streaming from Discord.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildDownloadCompletedEventArgs> GuildDownloadCompleted
    {
        add => this.events[typeof(GuildDownloadCompletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guilds emojis get updated
    /// For this Event you need the <see cref="DiscordIntents.GuildEmojisAndStickers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildEmojisUpdatedEventArgs> GuildEmojisUpdated
    {
        add => this.events[typeof(GuildEmojisUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildStickersUpdatedEventArgs> GuildStickersUpdated
    {
        add => this.events[typeof(GuildStickersUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild integration is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildIntegrationsUpdatedEventArgs> GuildIntegrationsUpdated
    {
        add => this.events[typeof(GuildIntegrationsUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a audit log entry is created.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildAuditLogCreatedEventArgs> GuildAuditLogCreated
    {
        add => this.events[typeof(GuildAuditLogCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventCreatedEventArgs> ScheduledGuildEventCreated
    {
        add => this.events[typeof(ScheduledGuildEventCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUpdatedEventArgs> ScheduledGuildEventUpdated
    {
        add => this.events[typeof(ScheduledGuildEventUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventDeletedEventArgs> ScheduledGuildEventDeleted
    {
        add => this.events[typeof(ScheduledGuildEventDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventCompletedEventArgs> ScheduledGuildEventCompleted
    {
        add => this.events[typeof(ScheduledGuildEventCompletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUserAddedEventArgs> ScheduledGuildEventUserAdded
    {
        add => this.events[typeof(ScheduledGuildEventUserAddedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUserRemovedEventArgs> ScheduledGuildEventUserRemoved
    {
        add => this.events[typeof(ScheduledGuildEventUserRemovedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild ban gets added
    /// For this Event you need the <see cref="DiscordIntents.GuildModeration"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildBanAddedEventArgs> GuildBanAdded
    {
        add => this.events[typeof(GuildBanAddedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild ban gets removed
    /// For this Event you need the <see cref="DiscordIntents.GuildModeration"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildBanRemovedEventArgs> GuildBanRemoved
    {
        add => this.events[typeof(GuildBanRemovedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a new user joins a guild.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMemberAddedEventArgs> GuildMemberAdded
    {
        add => this.events[typeof(GuildMemberAddedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a user is removed from a guild (leave/kick/ban).
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMemberRemovedEventArgs> GuildMemberRemoved
    {
        add => this.events[typeof(GuildMemberRemovedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild member is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMemberUpdatedEventArgs> GuildMemberUpdated
    {
        add => this.events[typeof(GuildMemberUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired in response to Gateway Request Guild Members.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildMembersChunkedEventArgs> GuildMembersChunked
    {
        add => this.events[typeof(GuildMembersChunkedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild role is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildRoleCreatedEventArgs> GuildRoleCreated
    {
        add => this.events[typeof(GuildRoleCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild role is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildRoleUpdatedEventArgs> GuildRoleUpdated
    {
        add => this.events[typeof(GuildRoleUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild role is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, GuildRoleDeletedEventArgs> GuildRoleDeleted
    {
        add => this.events[typeof(GuildRoleDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when an invite is created.
    /// For this Event you need the <see cref="DiscordIntents.GuildInvites"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, InviteCreatedEventArgs> InviteCreated
    {
        add => this.events[typeof(InviteCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when an invite is deleted.
    /// For this Event you need the <see cref="DiscordIntents.GuildInvites"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, InviteDeletedEventArgs> InviteDeleted
    {
        add => this.events[typeof(InviteDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a message is created.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageCreatedEventArgs> MessageCreated
    {
        add => this.events[typeof(MessageCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a message is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageUpdatedEventArgs> MessageUpdated
    {
        add => this.events[typeof(MessageUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a message is deleted.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageDeletedEventArgs> MessageDeleted
    {
        add => this.events[typeof(MessageDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when multiple messages are deleted at once.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessagesBulkDeletedEventArgs> MessagesBulkDeleted
    {
        add => this.events[typeof(MessagesBulkDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessagePollVotedEventArgs> MessagePollVoted
    {
        add => this.events[typeof(MessagePollVotedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a reaction gets added to a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionAddedEventArgs> MessageReactionAdded
    {
        add => this.events[typeof(MessageReactionAddedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a reaction gets removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionRemovedEventArgs> MessageReactionRemoved
    {
        add => this.events[typeof(MessageReactionRemovedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when all reactions get removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionsClearedEventArgs> MessageReactionsCleared
    {
        add => this.events[typeof(MessageReactionsClearedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when all reactions of a specific reaction are removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, MessageReactionRemovedEmojiEventArgs> MessageReactionRemovedEmoji
    {
        add => this.events[typeof(MessageReactionRemovedEmojiEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a presence has been updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, PresenceUpdatedEventArgs> PresenceUpdated
    {
        add => this.events[typeof(PresenceUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when the current user updates their settings.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, UserSettingsUpdatedEventArgs> UserSettingsUpdated
    {
        add => this.events[typeof(UserSettingsUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

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
        add => this.events[typeof(UserUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when someone joins/leaves/moves voice channels.
    /// For this Event you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, VoiceStateUpdatedEventArgs> VoiceStateUpdated
    {
        add => this.events[typeof(VoiceStateUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a guild's voice server is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, VoiceServerUpdatedEventArgs> VoiceServerUpdated
    {
        add => this.events[typeof(VoiceServerUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a thread is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadCreatedEventArgs> ThreadCreated
    {
        add => this.events[typeof(ThreadCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a thread is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadUpdatedEventArgs> ThreadUpdated
    {
        add => this.events[typeof(ThreadUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a thread is deleted.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadDeletedEventArgs> ThreadDeleted
    {
        add => this.events[typeof(ThreadDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when the current member gains access to a channel(s) that has threads.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadListSyncedEventArgs> ThreadListSynced
    {
        add => this.events[typeof(ThreadListSyncedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

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
        add => this.events[typeof(ThreadMemberUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when the thread members are updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> or <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ThreadMembersUpdatedEventArgs> ThreadMembersUpdated
    {
        add => this.events[typeof(ThreadMembersUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when an integration is created.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, IntegrationCreatedEventArgs> IntegrationCreated
    {
        add => this.events[typeof(IntegrationCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when an integration is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, IntegrationUpdatedEventArgs> IntegrationUpdated
    {
        add => this.events[typeof(IntegrationUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when an integration is deleted.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, IntegrationDeletedEventArgs> IntegrationDeleted
    {
        add => this.events[typeof(IntegrationDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a stage instance is created.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, StageInstanceCreatedEventArgs> StageInstanceCreated
    {
        add => this.events[typeof(StageInstanceCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a stage instance is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, StageInstanceUpdatedEventArgs> StageInstanceUpdated
    {
        add => this.events[typeof(StageInstanceUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a stage instance is deleted.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, StageInstanceDeletedEventArgs> StageInstanceDeleted
    {
        add => this.events[typeof(StageInstanceDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when an interaction is invoked.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, InteractionCreatedEventArgs> InteractionCreated
    {
        add => this.events[typeof(InteractionCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a component is invoked.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ComponentInteractionCreatedEventArgs> ComponentInteractionCreated
    {
        add => this.events[typeof(ComponentInteractionCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a modal is submitted. If a modal is closed, this event is not fired.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ModalSubmittedEventArgs> ModalSubmitted
    {
        add => this.events[typeof(ModalSubmittedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a user uses a context menu.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, ContextMenuInteractionCreatedEventArgs> ContextMenuInteractionCreated
    {
        add => this.events[typeof(ContextMenuInteractionCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a user starts typing in a channel.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, TypingStartedEventArgs> TypingStarted
    {
        add => this.events[typeof(TypingStartedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when an unknown event gets received.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, UnknownEventArgs> UnknownEvent
    {
        add => this.events[typeof(UnknownEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired whenever webhooks update.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, WebhooksUpdatedEventArgs> WebhooksUpdated
    {
        add => this.events[typeof(WebhooksUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired whenever an error occurs within an event handler.
    /// </summary>
    [Obsolete("This event is superseded by implementing/shimming IClientErrorHandler", DiagnosticId = "DSP0003")]
    public event AsyncEventHandler<DiscordClient, ClientErrorEventArgs> ClientErrored
    {
        add => throw new NotSupportedException();
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when a new auto-moderation rule is created.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleCreatedEventArgs> AutoModerationRuleCreated
    {
        add => this.events[typeof(AutoModerationRuleCreatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when an auto-moderation rule is updated.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleUpdatedEventArgs> AutoModerationRuleUpdated
    {
        add => this.events[typeof(AutoModerationRuleUpdatedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when an auto-moderation rule is deleted.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleDeletedEventArgs> AutoModerationRuleDeleted
    {
        add => this.events[typeof(AutoModerationRuleDeletedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// Fired when an auto-moderation is executed.
    /// </summary>
    [Obsolete(ObsoletionMessage, DiagnosticId = "DSP0001")]
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleExecutedEventArgs> AutoModerationRuleExecuted
    {
        add => this.events[typeof(AutoModerationRuleExecutedEventArgs)].Register(value);
        remove => throw new NotSupportedException();
    }
}
