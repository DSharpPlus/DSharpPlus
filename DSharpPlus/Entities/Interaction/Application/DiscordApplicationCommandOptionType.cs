namespace DSharpPlus.Entities;


/// <summary>
/// Represents the type of parameter when invoking an interaction.
/// </summary>
public enum DiscordApplicationCommandOptionType
{
    /// <summary>
    /// Whether this parameter is another subcommand.
    /// </summary>
    SubCommand = 1,

    /// <summary>
    /// Whether this parameter is apart of a subcommand group.
    /// </summary>
    SubCommandGroup,

    /// <summary>
    /// Whether this parameter is a string.
    /// </summary>
    String,

    /// <summary>
    /// Whether this parameter is an integer.
    /// </summary>
    Integer,

    /// <summary>
    /// Whether this parameter is a boolean.
    /// </summary>
    Boolean,

    /// <summary>
    /// Whether this parameter is a Discord user.
    /// </summary>
    User,

    /// <summary>
    /// Whether this parameter is a Discord channel.
    /// </summary>
    Channel,

    /// <summary>
    /// Whether this parameter is a Discord role.
    /// </summary>
    Role,

    /// <summary>
    /// Whether this parameter is a mentionable (role or user).
    /// </summary>
    Mentionable,

    /// <summary>
    /// Whether this parameter is a double.
    /// </summary>
    Number,

    /// <summary>
    /// Whether this parameter is a Discord attachment.
    /// </summary>
    Attachment
}
