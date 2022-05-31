namespace DSharpPlus.Core.Enums
{
    public enum DiscordGuildMFALevel
    {
        /// <summary>
        /// The guild has no MFA/2FA requirement for moderation actions.
        /// </summary>
        None = 0,

        /// <summary>
        /// The guild has a 2FA requirement for moderation actions.
        /// </summary>
        Elevated = 1
    }
}
