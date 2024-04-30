namespace DSharpPlus;
using System;
using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

public sealed partial class DiscordClient
{
    internal static TimeSpan EventExecutionLimit { get; } = TimeSpan.FromSeconds(1);

    // oh lord why did you have to pack into regions
    // this makes simple copy-paste ineffective
    // :notlikethis:

    #region WebSocket

    /// <summary>
    /// Fired whenever a WebSocket error occurs within the client.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, SocketErrorEventArgs> SocketErrored
    {
        add => _socketErrored.Register(value);
        remove => _socketErrored.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SocketErrorEventArgs> _socketErrored;

    /// <summary>
    /// Fired whenever WebSocket connection is established.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, SocketEventArgs> SocketOpened
    {
        add => _socketOpened.Register(value);
        remove => _socketOpened.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SocketEventArgs> _socketOpened;

    /// <summary>
    /// Fired whenever WebSocket connection is terminated.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, SocketCloseEventArgs> SocketClosed
    {
        add => _socketClosed.Register(value);
        remove => _socketClosed.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SocketCloseEventArgs> _socketClosed;

    /// <summary>
    /// Fired when this client has successfully completed its handshake with the websocket gateway.
    /// </summary>
    /// <remarks>
    /// <i><see cref="Guilds"/> will not be populated when this event is fired.</i><br/>
    /// See also: <see cref="GuildAvailable"/>, <see cref="GuildDownloadCompleted"/>
    /// </remarks>
    public event AsyncEventHandler<DiscordClient, SessionReadyEventArgs> SessionCreated
    {
        add => _ready.Register(value);
        remove => _ready.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SessionReadyEventArgs> _ready;

    /// <summary>
    /// Fired whenever a session is resumed.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, SessionReadyEventArgs> SessionResumed
    {
        add => _resumed.Register(value);
        remove => _resumed.Unregister(value);
    }
    private AsyncEvent<DiscordClient, SessionReadyEventArgs> _resumed;

    /// <summary>
    /// Fired on received heartbeat ACK.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, HeartbeatEventArgs> Heartbeated
    {
        add => _heartbeated.Register(value);
        remove => _heartbeated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, HeartbeatEventArgs> _heartbeated;

    /// <summary>
    /// Fired on heartbeat attempt cancellation due to too many failed heartbeats.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ZombiedEventArgs> Zombied
    {
        add => _zombied.Register(value);
        remove => _zombied.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ZombiedEventArgs> _zombied;

    #endregion


    #region Application
    public event AsyncEventHandler<DiscordClient, ApplicationCommandPermissionsUpdatedEventArgs> ApplicationCommandPermissionsUpdated
    {
        add => _applicationCommandPermissionsUpdated.Register(value);
        remove => _applicationCommandPermissionsUpdated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ApplicationCommandPermissionsUpdatedEventArgs> _applicationCommandPermissionsUpdated;

    #endregion

    #region Channel

    /// <summary>
    /// Fired when a new channel is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ChannelCreateEventArgs> ChannelCreated
    {
        add => _channelCreated.Register(value);
        remove => _channelCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelCreateEventArgs> _channelCreated;

    /// <summary>
    /// Fired when a channel is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ChannelUpdateEventArgs> ChannelUpdated
    {
        add => _channelUpdated.Register(value);
        remove => _channelUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelUpdateEventArgs> _channelUpdated;

    /// <summary>
    /// Fired when a channel is deleted
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ChannelDeleteEventArgs> ChannelDeleted
    {
        add => _channelDeleted.Register(value);
        remove => _channelDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelDeleteEventArgs> _channelDeleted;

    /// <summary>
    /// Fired when a dm channel is deleted
    /// For this Event you need the <see cref="DiscordIntents.DirectMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, DmChannelDeleteEventArgs> DmChannelDeleted
    {
        add => _dmChannelDeleted.Register(value);
        remove => _dmChannelDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, DmChannelDeleteEventArgs> _dmChannelDeleted;

    /// <summary>
    /// Fired whenever a channel's pinned message list is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ChannelPinsUpdateEventArgs> ChannelPinsUpdated
    {
        add => _channelPinsUpdated.Register(value);
        remove => _channelPinsUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs> _channelPinsUpdated;

    #endregion

    #region Guild

    /// <summary>
    /// Fired when the user joins a new guild.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    /// <remarks>[alias="GuildJoined"][alias="JoinedGuild"]</remarks>
    public event AsyncEventHandler<DiscordClient, GuildCreateEventArgs> GuildCreated
    {
        add => _guildCreated.Register(value);
        remove => _guildCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildCreateEventArgs> _guildCreated;

    /// <summary>
    /// Fired when a guild is becoming available.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildCreateEventArgs> GuildAvailable
    {
        add => _guildAvailable.Register(value);
        remove => _guildAvailable.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildCreateEventArgs> _guildAvailable;

    /// <summary>
    /// Fired when a guild is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildUpdateEventArgs> GuildUpdated
    {
        add => _guildUpdated.Register(value);
        remove => _guildUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildUpdateEventArgs> _guildUpdated;

    /// <summary>
    /// Fired when the user leaves or is removed from a guild.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildDeleteEventArgs> GuildDeleted
    {
        add => _guildDeleted.Register(value);
        remove => _guildDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildDeleteEventArgs> _guildDeleted;

    /// <summary>
    /// Fired when a guild becomes unavailable.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildDeleteEventArgs> GuildUnavailable
    {
        add => _guildUnavailable.Register(value);
        remove => _guildUnavailable.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildDeleteEventArgs> _guildUnavailable;

    /// <summary>
    /// Fired when all guilds finish streaming from Discord.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildDownloadCompletedEventArgs> GuildDownloadCompleted
    {
        add => _guildDownloadCompletedEv.Register(value);
        remove => _guildDownloadCompletedEv.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs> _guildDownloadCompletedEv;

    /// <summary>
    /// Fired when a guilds emojis get updated
    /// For this Event you need the <see cref="DiscordIntents.GuildEmojisAndStickers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildEmojisUpdateEventArgs> GuildEmojisUpdated
    {
        add => _guildEmojisUpdated.Register(value);
        remove => _guildEmojisUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs> _guildEmojisUpdated;


    public event AsyncEventHandler<DiscordClient, GuildStickersUpdateEventArgs> GuildStickersUpdated
    {
        add => _guildStickersUpdated.Register(value);
        remove => _guildStickersUpdated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, GuildStickersUpdateEventArgs> _guildStickersUpdated;

    /// <summary>
    /// Fired when a guild integration is updated.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
    {
        add => _guildIntegrationsUpdated.Register(value);
        remove => _guildIntegrationsUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs> _guildIntegrationsUpdated;

    /// <summary>
    /// Fired when a audit log entry is created.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildAuditLogCreatedEventArgs> GuildAuditLogCreated
    {
        add => _guildAuditLogCreated.Register(value);
        remove => _guildAuditLogCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildAuditLogCreatedEventArgs> _guildAuditLogCreated;

    #endregion

    #region Scheduled Guild Events

    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventCreateEventArgs> ScheduledGuildEventCreated
    {
        add => _scheduledGuildEventCreated.Register(value);
        remove => _scheduledGuildEventCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventCreateEventArgs> _scheduledGuildEventCreated;

    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUpdateEventArgs> ScheduledGuildEventUpdated
    {
        add => _scheduledGuildEventUpdated.Register(value);
        remove => _scheduledGuildEventUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventUpdateEventArgs> _scheduledGuildEventUpdated;

    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventDeleteEventArgs> ScheduledGuildEventDeleted
    {
        add => _scheduledGuildEventDeleted.Register(value);
        remove => _scheduledGuildEventDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventDeleteEventArgs> _scheduledGuildEventDeleted;

    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventCompletedEventArgs> ScheduledGuildEventCompleted
    {
        add => _scheduledGuildEventCompleted.Register(value);
        remove => _scheduledGuildEventCompleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventCompletedEventArgs> _scheduledGuildEventCompleted;

    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUserAddEventArgs> ScheduledGuildEventUserAdded
    {
        add => _scheduledGuildEventUserAdded.Register(value);
        remove => _scheduledGuildEventUserAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventUserAddEventArgs> _scheduledGuildEventUserAdded;

    public event AsyncEventHandler<DiscordClient, ScheduledGuildEventUserRemoveEventArgs> ScheduledGuildEventUserRemoved
    {
        add => _scheduledGuildEventUserRemoved.Register(value);
        remove => _scheduledGuildEventUserRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ScheduledGuildEventUserRemoveEventArgs> _scheduledGuildEventUserRemoved;

    #endregion

    #region Guild Ban

    /// <summary>
    /// Fired when a guild ban gets added
    /// For this Event you need the <see cref="DiscordIntents.GuildModeration"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildBanAddEventArgs> GuildBanAdded
    {
        add => _guildBanAdded.Register(value);
        remove => _guildBanAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildBanAddEventArgs> _guildBanAdded;

    /// <summary>
    /// Fired when a guild ban gets removed
    /// For this Event you need the <see cref="DiscordIntents.GuildModeration"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildBanRemoveEventArgs> GuildBanRemoved
    {
        add => _guildBanRemoved.Register(value);
        remove => _guildBanRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildBanRemoveEventArgs> _guildBanRemoved;

    #endregion

    #region Guild Member

    /// <summary>
    /// Fired when a new user joins a guild.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildMemberAddEventArgs> GuildMemberAdded
    {
        add => _guildMemberAdded.Register(value);
        remove => _guildMemberAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMemberAddEventArgs> _guildMemberAdded;

    /// <summary>
    /// Fired when a user is removed from a guild (leave/kick/ban).
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildMemberRemoveEventArgs> GuildMemberRemoved
    {
        add => _guildMemberRemoved.Register(value);
        remove => _guildMemberRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs> _guildMemberRemoved;

    /// <summary>
    /// Fired when a guild member is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildMemberUpdateEventArgs> GuildMemberUpdated
    {
        add => _guildMemberUpdated.Register(value);
        remove => _guildMemberUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs> _guildMemberUpdated;

    /// <summary>
    /// Fired in response to Gateway Request Guild Members.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildMembersChunkEventArgs> GuildMembersChunked
    {
        add => _guildMembersChunked.Register(value);
        remove => _guildMembersChunked.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildMembersChunkEventArgs> _guildMembersChunked;

    #endregion

    #region Guild Role

    /// <summary>
    /// Fired when a guild role is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildRoleCreateEventArgs> GuildRoleCreated
    {
        add => _guildRoleCreated.Register(value);
        remove => _guildRoleCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildRoleCreateEventArgs> _guildRoleCreated;

    /// <summary>
    /// Fired when a guild role is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildRoleUpdateEventArgs> GuildRoleUpdated
    {
        add => _guildRoleUpdated.Register(value);
        remove => _guildRoleUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs> _guildRoleUpdated;

    /// <summary>
    /// Fired when a guild role is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, GuildRoleDeleteEventArgs> GuildRoleDeleted
    {
        add => _guildRoleDeleted.Register(value);
        remove => _guildRoleDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs> _guildRoleDeleted;

    #endregion

    #region Invite

    /// <summary>
    /// Fired when an invite is created.
    /// For this Event you need the <see cref="DiscordIntents.GuildInvites"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, InviteCreateEventArgs> InviteCreated
    {
        add => _inviteCreated.Register(value);
        remove => _inviteCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, InviteCreateEventArgs> _inviteCreated;

    /// <summary>
    /// Fired when an invite is deleted.
    /// For this Event you need the <see cref="DiscordIntents.GuildInvites"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, InviteDeleteEventArgs> InviteDeleted
    {
        add => _inviteDeleted.Register(value);
        remove => _inviteDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, InviteDeleteEventArgs> _inviteDeleted;

    #endregion

    #region Message

    /// <summary>
    /// Fired when a message is created.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, MessageCreateEventArgs> MessageCreated
    {
        add => _messageCreated.Register(value);
        remove => _messageCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageCreateEventArgs> _messageCreated;

    /// <summary>
    /// Fired when a message is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, MessageUpdateEventArgs> MessageUpdated
    {
        add => _messageUpdated.Register(value);
        remove => _messageUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageUpdateEventArgs> _messageUpdated;

    /// <summary>
    /// Fired when a message is deleted.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, MessageDeleteEventArgs> MessageDeleted
    {
        add => _messageDeleted.Register(value);
        remove => _messageDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageDeleteEventArgs> _messageDeleted;

    /// <summary>
    /// Fired when multiple messages are deleted at once.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, MessageBulkDeleteEventArgs> MessagesBulkDeleted
    {
        add => _messagesBulkDeleted.Register(value);
        remove => _messagesBulkDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs> _messagesBulkDeleted;

    public event AsyncEventHandler<DiscordClient, MessagePollVoteEventArgs> MessagePollVoted
    {
        add => _messagePollVoted.Register(value);
        remove => _messagePollVoted.Unregister(value);
    }

    private AsyncEvent<DiscordClient, MessagePollVoteEventArgs> _messagePollVoted;

    #endregion

    #region Message Reaction

    /// <summary>
    /// Fired when a reaction gets added to a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs> MessageReactionAdded
    {
        add => _messageReactionAdded.Register(value);
        remove => _messageReactionAdded.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionAddEventArgs> _messageReactionAdded;

    /// <summary>
    /// Fired when a reaction gets removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs> MessageReactionRemoved
    {
        add => _messageReactionRemoved.Register(value);
        remove => _messageReactionRemoved.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs> _messageReactionRemoved;

    /// <summary>
    /// Fired when all reactions get removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, MessageReactionsClearEventArgs> MessageReactionsCleared
    {
        add => _messageReactionsCleared.Register(value);
        remove => _messageReactionsCleared.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionsClearEventArgs> _messageReactionsCleared;

    /// <summary>
    /// Fired when all reactions of a specific reaction are removed from a message.
    /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, MessageReactionRemoveEmojiEventArgs> MessageReactionRemovedEmoji
    {
        add => _messageReactionRemovedEmoji.Register(value);
        remove => _messageReactionRemovedEmoji.Unregister(value);
    }
    private AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs> _messageReactionRemovedEmoji;

    #endregion

    #region Presence/User Update

    /// <summary>
    /// Fired when a presence has been updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, PresenceUpdateEventArgs> PresenceUpdated
    {
        add => _presenceUpdated.Register(value);
        remove => _presenceUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, PresenceUpdateEventArgs> _presenceUpdated;


    /// <summary>
    /// Fired when the current user updates their settings.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, UserSettingsUpdateEventArgs> UserSettingsUpdated
    {
        add => _userSettingsUpdated.Register(value);
        remove => _userSettingsUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs> _userSettingsUpdated;

    /// <summary>
    /// Fired when properties about the current user change.
    /// </summary>
    /// <remarks>
    /// NB: This event only applies for changes to the <b>current user</b>, the client that is connected to Discord.
    /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </remarks>
    public event AsyncEventHandler<DiscordClient, UserUpdateEventArgs> UserUpdated
    {
        add => _userUpdated.Register(value);
        remove => _userUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, UserUpdateEventArgs> _userUpdated;

    #endregion

    #region Voice

    /// <summary>
    /// Fired when someone joins/leaves/moves voice channels.
    /// For this Event you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, VoiceStateUpdateEventArgs> VoiceStateUpdated
    {
        add => _voiceStateUpdated.Register(value);
        remove => _voiceStateUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs> _voiceStateUpdated;

    /// <summary>
    /// Fired when a guild's voice server is updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, VoiceServerUpdateEventArgs> VoiceServerUpdated
    {
        add => _voiceServerUpdated.Register(value);
        remove => _voiceServerUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs> _voiceServerUpdated;

    #endregion

    #region Thread

    /// <summary>
    /// Fired when a thread is created.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ThreadCreateEventArgs> ThreadCreated
    {
        add => _threadCreated.Register(value);
        remove => _threadCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadCreateEventArgs> _threadCreated;

    /// <summary>
    /// Fired when a thread is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ThreadUpdateEventArgs> ThreadUpdated
    {
        add => _threadUpdated.Register(value);
        remove => _threadUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadUpdateEventArgs> _threadUpdated;

    /// <summary>
    /// Fired when a thread is deleted.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ThreadDeleteEventArgs> ThreadDeleted
    {
        add => _threadDeleted.Register(value);
        remove => _threadDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadDeleteEventArgs> _threadDeleted;

    /// <summary>
    /// Fired when the current member gains access to a channel(s) that has threads.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ThreadListSyncEventArgs> ThreadListSynced
    {
        add => _threadListSynced.Register(value);
        remove => _threadListSynced.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadListSyncEventArgs> _threadListSynced;

    /// <summary>
    /// Fired when the thread member for the current user is updated.
    /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    /// <remarks>
    /// This event is mostly documented for completeness, and it not fired every time
    /// DM channels in which no prior messages were received or sent.
    /// </remarks>
    public event AsyncEventHandler<DiscordClient, ThreadMemberUpdateEventArgs> ThreadMemberUpdated
    {
        add => _threadMemberUpdated.Register(value);
        remove => _threadMemberUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadMemberUpdateEventArgs> _threadMemberUpdated;

    /// <summary>
    /// Fired when the thread members are updated.
    /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> or <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ThreadMembersUpdateEventArgs> ThreadMembersUpdated
    {
        add => _threadMembersUpdated.Register(value);
        remove => _threadMembersUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ThreadMembersUpdateEventArgs> _threadMembersUpdated;

    #endregion


    #region Integration

    /// <summary>
    /// Fired when an integration is created.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, IntegrationCreateEventArgs> IntegrationCreated
    {
        add => _integrationCreated.Register(value);
        remove => _integrationCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, IntegrationCreateEventArgs> _integrationCreated;

    /// <summary>
    /// Fired when an integration is updated.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, IntegrationUpdateEventArgs> IntegrationUpdated
    {
        add => _integrationUpdated.Register(value);
        remove => _integrationUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, IntegrationUpdateEventArgs> _integrationUpdated;

    /// <summary>
    /// Fired when an integration is deleted.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, IntegrationDeleteEventArgs> IntegrationDeleted
    {
        add => _integrationDeleted.Register(value);
        remove => _integrationDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, IntegrationDeleteEventArgs> _integrationDeleted;

    #endregion

    #region Stage Instance

    /// <summary>
    /// Fired when a stage instance is created.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, StageInstanceCreateEventArgs> StageInstanceCreated
    {
        add => _stageInstanceCreated.Register(value);
        remove => _stageInstanceCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, StageInstanceCreateEventArgs> _stageInstanceCreated;

    /// <summary>
    /// Fired when a stage instance is updated.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, StageInstanceUpdateEventArgs> StageInstanceUpdated
    {
        add => _stageInstanceUpdated.Register(value);
        remove => _stageInstanceUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, StageInstanceUpdateEventArgs> _stageInstanceUpdated;

    /// <summary>
    /// Fired when a stage instance is deleted.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, StageInstanceDeleteEventArgs> StageInstanceDeleted
    {
        add => _stageInstanceDeleted.Register(value);
        remove => _stageInstanceDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, StageInstanceDeleteEventArgs> _stageInstanceDeleted;

    #endregion

    #region Misc

    /// <summary>
    /// Fired when an interaction is invoked.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, InteractionCreateEventArgs> InteractionCreated
    {
        add => _interactionCreated.Register(value);
        remove => _interactionCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, InteractionCreateEventArgs> _interactionCreated;

    /// <summary>
    /// Fired when a component is invoked.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> ComponentInteractionCreated
    {
        add => _componentInteractionCreated.Register(value);
        remove => _componentInteractionCreated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ComponentInteractionCreateEventArgs> _componentInteractionCreated;

    /// <summary>
    /// Fired when a modal is submitted. If a modal is closed, this event is not fired.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ModalSubmitEventArgs> ModalSubmitted
    {
        add => _modalSubmitted.Register(value);
        remove => _modalSubmitted.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ModalSubmitEventArgs> _modalSubmitted;

    /// <summary>
    /// Fired when a user uses a context menu.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ContextMenuInteractionCreateEventArgs> ContextMenuInteractionCreated
    {
        add => _contextMenuInteractionCreated.Register(value);
        remove => _contextMenuInteractionCreated.Unregister(value);
    }

    private AsyncEvent<DiscordClient, ContextMenuInteractionCreateEventArgs> _contextMenuInteractionCreated;

    /// <summary>
    /// Fired when a user starts typing in a channel.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, TypingStartEventArgs> TypingStarted
    {
        add => _typingStarted.Register(value);
        remove => _typingStarted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, TypingStartEventArgs> _typingStarted;

    /// <summary>
    /// Fired when an unknown event gets received.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, UnknownEventArgs> UnknownEvent
    {
        add => _unknownEvent.Register(value);
        remove => _unknownEvent.Unregister(value);
    }
    private AsyncEvent<DiscordClient, UnknownEventArgs> _unknownEvent;

    /// <summary>
    /// Fired whenever webhooks update.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, WebhooksUpdateEventArgs> WebhooksUpdated
    {
        add => _webhooksUpdated.Register(value);
        remove => _webhooksUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, WebhooksUpdateEventArgs> _webhooksUpdated;

    /// <summary>
    /// Fired whenever an error occurs within an event handler.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, ClientErrorEventArgs> ClientErrored
    {
        add => _clientErrored.Register(value);
        remove => _clientErrored.Unregister(value);
    }
    private AsyncEvent<DiscordClient, ClientErrorEventArgs> _clientErrored;

    #endregion

    #region Error Handling

    internal void EventErrorHandler<TSender, TArgs>(AsyncEvent<TSender, TArgs> asyncEvent, Exception ex, AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs
    {
        Logger.LogError(LoggerEvents.EventHandlerException, ex, "Event handler exception for event {Event} thrown from {Method} (defined in {DeclaryingType})", asyncEvent.Name, handler.Method, handler.Method.DeclaringType);
        _clientErrored.InvokeAsync(this, new ClientErrorEventArgs { EventName = asyncEvent.Name, Exception = ex }).GetAwaiter().GetResult();
    }

    private void Goof<TSender, TArgs>(AsyncEvent<TSender, TArgs> asyncEvent, Exception ex, AsyncEventHandler<TSender, TArgs> handler, TSender sender, TArgs eventArgs)
        where TArgs : AsyncEventArgs => Logger.LogCritical(LoggerEvents.EventHandlerException, ex, "Exception event handler {Method} (defined in {DeclaringType}) threw an exception", handler.Method, handler.Method.DeclaringType);

    #endregion

    #region AutoModeration

    /// <summary>
    /// Fired when a new auto-moderation rule is created.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleCreateEventArgs> AutoModerationRuleCreated
    {
        add => _autoModerationRuleCreated.Register(value);
        remove => _autoModerationRuleCreated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleCreateEventArgs> _autoModerationRuleCreated;

    /// <summary>
    /// Fired when an auto-moderation rule is updated.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleUpdateEventArgs> AutoModerationRuleUpdated
    {
        add => _autoModerationRuleUpdated.Register(value);
        remove => _autoModerationRuleUpdated.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleUpdateEventArgs> _autoModerationRuleUpdated;

    /// <summary>
    /// Fired when an auto-moderation rule is deleted.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleDeleteEventArgs> AutoModerationRuleDeleted
    {
        add => _autoModerationRuleDeleted.Register(value);
        remove => _autoModerationRuleDeleted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleDeleteEventArgs> _autoModerationRuleDeleted;

    /// <summary>
    /// Fired when an auto-moderation is executed.
    /// </summary>
    public event AsyncEventHandler<DiscordClient, AutoModerationRuleExecuteEventArgs> AutoModerationRuleExecuted
    {
        add => _autoModerationRuleExecuted.Register(value);
        remove => _autoModerationRuleExecuted.Unregister(value);
    }
    private AsyncEvent<DiscordClient, AutoModerationRuleExecuteEventArgs> _autoModerationRuleExecuted;
    #endregion
}
