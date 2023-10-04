
namespace DSharpPlus
{
    /// <summary>
    /// Represents flags for a discord application.
    /// </summary>
    public enum ApplicationFlags
    {   
        /// <summary>
        /// Indicates if an application uses the Auto Moderation API.
        /// </summary>
        ApplicationAutoModerationRuleCreateBadge = 1 << 6,

        /// <summary>
        /// Indicates that the application is approved for the <see cref="DiscordIntents.GuildPresences"/> intent.
        /// </summary>
        GatewayPresence = 1 << 12,

        /// <summary>
        /// Indicates that the application is awaiting approval for the <see cref="DiscordIntents.GuildPresences"/> intent.
        /// </summary>
        GatewayPresenceLimited = 1 << 13,

        /// <summary>
        /// Indicates that the application is approved for the <see cref="DiscordIntents.GuildMembers"/> intent.
        /// </summary>
        GatewayGuildMembers = 1 << 14,

        /// <summary>
        /// Indicates that the application is awaiting approval for the <see cref="DiscordIntents.GuildMembers"/> intent.
        /// </summary>
        GatewayGuildMembersLimited = 1 << 15,

        /// <summary>
        /// Indicates that the application is awaiting verification.
        /// </summary>
        VerificationPendingGuildLimit = 1 << 16,

        /// <summary>
        /// Indicates that the application is a voice channel application.
        /// </summary>
        Embedded = 1 << 17,

        /// <summary>
        /// The application can track message content.
        /// </summary>
        GatewayMessageContent = 1 << 18,

        /// <summary>
        /// The application can track message content (limited).
        /// </summary>
        GatewayMessageContentLimited = 1 << 19,

        /// <summary>
        /// Indicates if an application has registered global application commands.
        /// </summary>
        ApplicationCommandBadge = 1 << 23,
    }
}
