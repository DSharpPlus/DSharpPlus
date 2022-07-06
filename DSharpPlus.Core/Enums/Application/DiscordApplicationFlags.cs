namespace DSharpPlus.Core.Enums
{
    public enum DiscordApplicationFlags
    {
        /// <summary>
        /// Intent required for bots in <b>100 or more servers</b> to receive <see cref="GatewayEntities.Payloads.DiscordUpdatePresencePayload"/> events.
        /// </summary>
        GatewayPresence = 1 << 12,

        /// <summary>
        /// Intent required for bots in under 100 servers to receive <see cref="GatewayEntities.Payloads.DiscordUpdatePresencePayload"/> events, found in Bot Settings.
        /// </summary>
        GatewayPresenceLimited = 1 << 13,

        /// <summary>
        /// Intent required for bots in <b>100 or more servers</b> to receive member-related events like <c>guild_member_add</c>. See list of member-related events under <c>GUILD_MEMBERS</c>.
        /// </summary>
        GatewayGuildMembers = 1 << 14,

        /// <summary>
        /// Intent required for bots in under 100 servers to receive member-related events like <c>guild_member_add</c>, found in Bot Settings. See list of member-related events under <c>GUILD_MEMBERS</c>.
        /// </summary>
        GatewayGuildMembersLimited = 1 << 15,

        /// <summary>
        /// Indicates unusual growth of an app that prevents verification.
        /// </summary>
        VerificationPendingGuildLimit = 1 << 16,

        /// <summary>
        /// Indicates if an app is embedded within the Discord client (currently unavailable publicly).
        /// </summary>
        Embedded = 1 << 17,

        /// <summary>
        /// Intent required for bots in <b>100 or more servers</b> to receive <c>message content</c>.
        /// </summary>
        GatewayMessageContent = 1 << 18,

        /// <summary>
        /// Intent required for bots in under 100 servers to receive <c>message content</c>, found in Bot Settings.
        /// </summary>
        GatewayMessageContentLimited = 1 << 19
    }
}
