using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message;

public class MessageCommandResult : IMessageCommandResult
{
    public MessageCommandResultType Type { get; set; }
    public string? Content { get; set; }
    public List<DiscordEmbed>? Embeds { get; set; }

    public static implicit operator MessageCommandResult(DiscordEmbed embed)
    {
        MessageCommandResult msgCmdResult = new();
        msgCmdResult.Embeds ??= new List<DiscordEmbed> { embed };

        return msgCmdResult;
    }

    public static implicit operator MessageCommandResult(DiscordEmbedBuilder builder)
    {
        DiscordEmbed embed = builder.Build();
        MessageCommandResult msgCmdResult = new();
        msgCmdResult.Embeds ??= new List<DiscordEmbed> { embed };

        return msgCmdResult;
    }

    public static implicit operator MessageCommandResult(string str)
    {
        MessageCommandResult msgCmdResult = new() { Content = str };
        return msgCmdResult;
    }
}
