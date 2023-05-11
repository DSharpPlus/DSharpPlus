using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message;

public class MessageResult : IMessageResult
{
    public MessageResultType Type { get; set; }
    public string? Content { get; set; }
    public List<DiscordEmbed>? Embeds { get; set; }

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

    public static implicit operator MessageResult(DiscordEmbedBuilder builder)
    {
        DiscordEmbed embed = builder.Build();
        MessageResult msgCmdResult = new();
        msgCmdResult.Embeds ??= new List<DiscordEmbed> { embed };

        return msgCmdResult;
    }

    public static implicit operator MessageResult(string str)
    {
        MessageResult msgCmdResult = new() { Content = str };
        return msgCmdResult;
    }
}
