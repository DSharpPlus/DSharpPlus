namespace DSharpPlus.Entities;

public enum DiscordApplicationCommandOptionType
{
    SubCommand = 1,
    SubCommandGroup = 2,
    String = 3,

    /// <remarks>
    /// Any integer between -2^53 and 2^53.
    /// </remarks>
    Integer = 4,
    Boolean = 5,
    User = 6,

    /// <remarks>
    /// Includes all channel types + categories
    /// </remarks>
    Channel = 7,
    Role = 8,

    /// <remarks>
    /// Includes users and roles.
    /// </remarks>
    Mentionable = 9,

    /// <remarks>
    /// Any double between -2^53 and 2^53.
    /// </remarks>
    Number = 10,
    Attachment = 11
}
