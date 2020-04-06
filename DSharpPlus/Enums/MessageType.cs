namespace DSharpPlus
{
    /// <summary>
    /// Indicates the type of the message.
    /// </summary>
    public enum MessageType : int
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
        /// USER pinned a message to this channel.
        /// </summary>
        ChannelPinnedMessage = 6,

        /// <summary>
        /// Message when a guild member joins. Most frequently seen in newer, smaller guilds.
        /// </summary>
        GuildMemberJoin = 7,

        /// <summary>
        /// Message when a member nitro boosts the guild.
        /// </summary>
        UserPremiumGuildSubscription = 8,

        /// <summary>
        /// Message when the guild reaches tier one of nitro boosts.
        /// </summary>
        TierOneUserPremiumGuildSubscription = 9,

        /// <summary>
        /// Message when the guild reaches tier two of nitro boosts.
        /// </summary>
        TierTwoUserPremiumGuildSubscription = 10,

        /// <summary>
        /// Message when the guild reaches tier three of nitro boosts.
        /// </summary>
        TierThreeUserPremiumGuildSubscription = 11,

        /// <summary>
        /// Message when a user follows a news channel.
        /// </summary>
        ChannelFollowAdd = 12
    }
}
