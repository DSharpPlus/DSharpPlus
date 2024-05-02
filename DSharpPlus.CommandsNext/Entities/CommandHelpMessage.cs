using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Entities;

/// <summary>
/// Represents a formatted help message.
/// </summary>
public readonly struct CommandHelpMessage
{
    /// <summary>
    /// Gets the contents of the help message.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    /// Gets the embed attached to the help message.
    /// </summary>
    public DiscordEmbed? Embed { get; }

    /// <summary>
    /// Creates a new instance of a help message.
    /// </summary>
    /// <param name="content">Contents of the message.</param>
    /// <param name="embed">Embed to attach to the message.</param>
    public CommandHelpMessage(string? content = null, DiscordEmbed? embed = null)
    {
        Content = content;
        Embed = embed;
    }
}
