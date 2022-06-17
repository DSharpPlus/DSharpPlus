namespace DSharpPlus.Core.Enums
{
    public enum DiscordAutoModerationActionType
    {
        /// <summary>
        /// Blocks the content of a message according to the rule.
        /// </summary>
        BlockMessage = 1,

        /// <summary>
        /// Logs user content to a specified channel.
        /// </summary>
        SendAlertMessage = 2,

        /// <summary>
        /// Timeout user for a specified duration
        /// </summary>
        /// <remarks>
        /// This can only be setup for KEYWORD rules. <see cref="DiscordPermissions.ModerateMembers"/> permission is required to use the TIMEOUT action type.
        /// The duration is in seconds and the maximum is 2419200 seconds (4 weeks).
        /// </remarks>
        Timeout = 3
    }
}
