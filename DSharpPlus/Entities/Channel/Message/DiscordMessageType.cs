namespace DSharpPlus.Entities;

/// <summary>
/// Represents the type of a message.
/// </summary>
public enum DiscordMessageType : int
{
    /// <summary>
    /// Indicates a regular message.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Message indicating a recipient was added to a group direct message.
    /// </summary>
    RecipientAdd = 1,

    /// <summary>
    /// Message indicating a recipient was removed from a group direct message.
    /// </summary>
    RecipientRemove = 2,

    /// <summary>
    /// Message indicating a call.
    /// </summary>
    Call = 3,

    /// <summary>
    /// Message indicating a group direct message channel rename.
    /// </summary>
    ChannelNameChange = 4,

    /// <summary>
    /// Message indicating a group direct message channel icon change.
    /// </summary>
    ChannelIconChange = 5,

    /// <summary>
    /// Message indicating a user pinned a message to a channel.
    /// </summary>
    ChannelPinnedMessage = 6,

    /// <summary>
    /// Message indicating a guild member joined. Most frequently seen in newer, smaller guilds.
    /// </summary>
    GuildMemberJoin = 7,

    /// <summary>
    /// Message indicating a member nitro boosted a guild.
    /// </summary>
    UserPremiumGuildSubscription = 8,

    /// <summary>
    /// Message indicating a guild reached tier one of nitro boosts.
    /// </summary>
    TierOneUserPremiumGuildSubscription = 9,

    /// <summary>
    /// Message indicating a guild reached tier two of nitro boosts.
    /// </summary>
    TierTwoUserPremiumGuildSubscription = 10,

    /// <summary>
    /// Message indicating a guild reached tier three of nitro boosts.
    /// </summary>
    TierThreeUserPremiumGuildSubscription = 11,

    /// <summary>
    /// Message indicating a user followed a news channel.
    /// </summary>
    ChannelFollowAdd = 12,

    /// <summary>
    /// Message indicating a guild was removed from guild discovery.
    /// </summary>
    GuildDiscoveryDisqualified = 14,

    /// <summary>
    /// Message indicating a guild was re-added to guild discovery.
    /// </summary>
    GuildDiscoveryRequalified = 15,

    /// <summary>
    /// Message indicating that a guild has failed to meet guild discovery requirements for a week.
    /// </summary>
    GuildDiscoveryGracePeriodInitialWarning = 16,

    /// <summary>
    /// Message indicating that a guild has failed to meet guild discovery requirements for 3 weeks.
    /// </summary>
    GuildDiscoveryGracePeriodFinalWarning = 17,

    /// <summary>
    /// 
    /// </summary>
    ThreadCreated = 18,


    /// <summary>
    /// Message indicating a user replied to another user.
    /// </summary>
    Reply = 19,

    /// <summary>
    /// Message indicating an application command was invoked.
    /// </summary>
    ApplicationCommand = 20,

    /// <summary>
    /// 
    /// </summary>
    ThreadStarterMessage = 21,

    /// <summary>
    /// Message reminding you to invite people to help you build the server.
    /// </summary>
    GuildInviteReminder = 22,

    /// <summary>
    /// Message indicating a context menu was executed.
    /// </summary>
    ContextMenuCommand = 23,

    /// <summary>
    /// Message indicating an auto-moderation alert.
    /// </summary>
    AutoModerationAlert = 24,

    RoleSubscriptionPurchase = 25,
    InteractionPremiumUpsell = 26,
    StageStart = 27,
    StageEnd = 28,
    StageSpeaker = 29,
    StageTopic = 31,
    GuildApplicationPremiumSubscription = 32,
}
