﻿namespace DSharpPlus
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
        /// Message indiciating a user pinned a message to a channel.
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
        /// Message indicating a guild was readded to guild discovery.
        /// </summary>
        GuildDiscoveryRequalified = 15,

        /// <summary>
        /// Message indicating a user replied to another user.
        /// </summary>
        Reply = 19,

        /// <summary>
        /// Message indicating an application command was invoked.
        /// </summary>
        ApplicationCommand = 20
    }
}
