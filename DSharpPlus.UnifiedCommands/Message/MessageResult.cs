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

    public DiscordMessageBuilder Builder { get; set; } = new();

    /// <summary>
    /// Implicit conversion for embeds.
    /// </summary>
    /// <param name="embed"></param>
    /// <returns></returns>
    public static implicit operator MessageResult(DiscordEmbed embed)
    {
        MessageResult msgCmdResult = new();
        msgCmdResult.Builder.AddEmbed(embed);

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
        msgCmdResult.Builder.AddEmbed(builder);

        return msgCmdResult;
    }

    /// <summary>
    /// Implicit conversion for content.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static implicit operator MessageResult(string str)
    {
        MessageResult msgCmdResult = new();
        msgCmdResult.Builder.WithContent(str);
        return msgCmdResult;
    }
}
