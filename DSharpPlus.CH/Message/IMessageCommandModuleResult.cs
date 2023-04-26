namespace DSharpPlus.CH.Message;

public interface IMessageCommandModuleResult
{
    public MessageCommandModuleResultType Type { get; set; }
    public string? Content { get; set; }
    public List<Entities.DiscordEmbed>? Embeds { get; set; }
}
