using DSharpPlus.Entities;

namespace DSharpPlus.CH.Message;

public class MessageCommandModuleResult : IMessageCommandModuleResult
{
    public MessageCommandModuleResultType Type { get; set; }
    public string? Content { get; set; }
    public List<DiscordEmbed>? Embeds { get; set; }

    public static implicit operator MessageCommandModuleResult(DiscordEmbed embed)
    {
        MessageCommandModuleResult msgCmdResult = new();
        if (msgCmdResult.Embeds is null)
        {
            msgCmdResult.Embeds = new List<DiscordEmbed> { embed };
        }

        return msgCmdResult;
    }

    public static implicit operator MessageCommandModuleResult(DiscordEmbedBuilder builder)
    {
        DiscordEmbed embed = builder.Build();
        MessageCommandModuleResult msgCmdResult = new();
        if (msgCmdResult.Embeds is null)
        {
            msgCmdResult.Embeds = new List<DiscordEmbed> { embed };
        }

        return msgCmdResult;
    }

    public static implicit operator MessageCommandModuleResult(string str)
    {
        MessageCommandModuleResult msgCmdResult = new() { Content = str };
        return msgCmdResult;
    }
}
