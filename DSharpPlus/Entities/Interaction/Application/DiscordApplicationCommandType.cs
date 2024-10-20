namespace DSharpPlus.Entities;


/// <summary>
/// Represents the type of a <see cref="DiscordApplicationCommand"/>.
/// </summary>
public enum DiscordApplicationCommandType
{
    /// <summary>
    /// This command is registered as a slash-command, aka "Chat Input".
    /// </summary>
    SlashCommand = 1,

    /// <summary>
    /// This command is registered as a user context menu, and is applicable when interacting a user.
    /// </summary>
    UserContextMenu = 2,

    /// <summary>
    /// This command is registered as a message context menu, and is applicable when interacting with a message.
    /// </summary>
    MessageContextMenu = 3,

    /// <summary>
    /// This command serves as the primary entry point into the app's activity.
    /// </summary>
    ActivityEntryPoint = 4,
}
