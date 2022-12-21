namespace DSharpPlus.Core.Enums
{
    public enum DiscordGuildExplicitContentFilterLevel
    {
        /// <summary>
        /// Media content will not be scanned.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Media content sent by members without roles will be scanned.
        /// </summary>
        MembersWithoutRoles = 1,

        /// <summary>
        /// Media content sent by all members will be scanned.
        /// </summary>
        AllMembers = 2
    }
}
