namespace DSharpPlus.Entities;

public enum DiscordApplicationCommandType
{
    /// <summary>
    /// Slash commands; a text-based command that shows up when a user types <c>/</c>.
    /// </summary>
    ChatInput = 1,

    /// <summary>
    /// A UI-based command that shows up when you right click or tap on a user.
    /// </summary>
    User = 2,

    /// <summary>
    /// A UI-based command that shows up when you right click or tap on a message.
    /// </summary>
    Message = 3
}
