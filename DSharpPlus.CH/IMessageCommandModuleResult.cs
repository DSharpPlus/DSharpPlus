namespace DSharpPlus.CH
{
    public interface IMessageCommandModuleResult
    {
        public MessageCommandModuleResultType Type { get; set; }
        public string? Content { get; set; }
        public List<DSharpPlus.Entities.DiscordEmbed>? Embeds { get; set; }
    }
}
