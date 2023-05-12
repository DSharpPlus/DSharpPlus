using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// The primary class used for MessageResult.
/// </summary>
public class MessageResult : IMessageResult
{
    /// <summary>
    /// The type of action that should be processed.
    /// </summary>
    public MessageResultType Type { get; set; }
    /// <summary>
    /// The possible content for a message.
    /// </summary>
    public string? Content { get; set; }
    /// <summary>
    /// The possible embeds for a message
    /// </summary>
    public List<DiscordEmbed>? Embeds { get; set; }

    /// <summary>
    /// Implicit conversion for embeds.
    /// </summary>
    /// <param name="embed"></param>
    /// <returns></returns>
    public static implicit operator MessageResult(DiscordEmbed embed)
    {
        MessageResult msgCmdResult = new();
        if (msgCmdResult.Embeds is null)
        {
            msgCmdResult.Embeds ??= new List<DiscordEmbed> { embed };
        }
        else
        {
            msgCmdResult.Embeds.Add(embed);
        }

        return msgCmdResult;
    }

    /// <summary>
    /// Implicit conversion for embed builder.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static implicit operator MessageResult(DiscordEmbedBuilder builder)
    {
        DiscordEmbed embed = builder.Build();
        MessageResult msgCmdResult = new();
        msgCmdResult.Embeds ??= new List<DiscordEmbed> { embed };

        return msgCmdResult;
    }

    /// <summary>
    /// Implicit conversion for content.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static implicit operator MessageResult(string str)
    {
        MessageResult msgCmdResult = new() { Content = str };
        return msgCmdResult;
    }
}
