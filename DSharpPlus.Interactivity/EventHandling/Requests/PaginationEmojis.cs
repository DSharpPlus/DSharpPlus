namespace DSharpPlus.Interactivity;

using DSharpPlus.Entities;

public class PaginationEmojis
{
    public DiscordEmoji SkipLeft;
    public DiscordEmoji SkipRight;
    public DiscordEmoji Left;
    public DiscordEmoji Right;
    public DiscordEmoji Stop;

    public PaginationEmojis()
    {
        Left = DiscordEmoji.FromUnicode("◀");
        Right = DiscordEmoji.FromUnicode("▶");
        SkipLeft = DiscordEmoji.FromUnicode("⏮");
        SkipRight = DiscordEmoji.FromUnicode("⏭");
        Stop = DiscordEmoji.FromUnicode("⏹");
    }
}

public class Page
{
    public string Content { get; set; }
    public DiscordEmbed Embed { get; set; }

    public Page(string content = "", DiscordEmbedBuilder embed = null)
    {
        Content = content;
        Embed = embed?.Build();
    }
}
