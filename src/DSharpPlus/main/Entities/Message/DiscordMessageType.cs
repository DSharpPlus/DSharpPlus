namespace DSharpPlus.Entities;

public enum DiscordMessageType
{
    Default = 0,
    RecipientAdd = 1,
    RecipientRemove = 2,
    Call = 3,
    ChannelNameChange = 4,
    ChannelIconChange = 5,
    ChannelPinnedMessage = 6,
    GuildMemberJoin = 7,
    UserPremiumGuildSubscription = 8,
    UserPremiumGuildSubscriptionTier1 = 9,
    UserPremiumGuildSubscriptionTier2 = 10,
    UserPremiumGuildSubscriptionTier3 = 11,
    ChannelFollowAdd = 12,
    GuildDiscoveryDisqualified = 14,
    GuildDiscoveryRequalified = 15,
    GuildDiscoveryGracePeriodInitialWarning = 16,
    GuildDiscoveryGracePeriodFinalWarning = 17,
    ThreadCreated = 18,

    /// <remarks>
    /// Only in API v8. In v6, they are <see cref="Default"/>
    /// </remarks>
    Reply = 19,

    /// <remarks>
    /// Only in API v8. In v6, they are <see cref="Default"/>
    /// </remarks>
    ChatInputCommand = 20,

    /// <remarks>
    /// Only in API v9.
    /// </remarks>
    ThreadStarterMessage = 21,
    GuildInviteReminder = 22,
    ContextMenuCommand = 23,
    AutoModerationAction = 24
}
