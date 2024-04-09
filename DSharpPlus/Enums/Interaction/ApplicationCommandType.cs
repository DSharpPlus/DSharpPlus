namespace DSharpPlus.Entities;

/// <summary>
/// Represents the type of an <see cref="Entities.DiscordApplicationCommand"/>.
/// </summary>
public enum ApplicationCommandType
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
    /// Inbound only: An auto-complete option is being interacted with.
    /// </summary>
    AutoCompleteRequest = 4,
}
