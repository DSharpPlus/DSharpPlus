namespace DSharpPlus.CH.Message;

public interface IMessageResult
{
    public MessageResultType Type { get; set; }
    public string? Content { get; set; }
    public List<Entities.DiscordEmbed>? Embeds { get; set; }
}
