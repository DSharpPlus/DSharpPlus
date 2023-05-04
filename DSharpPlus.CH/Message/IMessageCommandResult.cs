namespace DSharpPlus.CH.Message;

public interface IMessageCommandResult
{
    public MessageCommandResultType Type { get; set; }
    public string? Content { get; set; }
    public List<Entities.DiscordEmbed>? Embeds { get; set; }
}
