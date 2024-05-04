using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity;

public class PaginationEmojis
{
    public DiscordEmoji SkipLeft;
    public DiscordEmoji SkipRight;
    public DiscordEmoji Left;
    public DiscordEmoji Right;
    public DiscordEmoji Stop;

    public PaginationEmojis()
    {
        this.Left = DiscordEmoji.FromUnicode("◀");
        this.Right = DiscordEmoji.FromUnicode("▶");
        this.SkipLeft = DiscordEmoji.FromUnicode("⏮");
        this.SkipRight = DiscordEmoji.FromUnicode("⏭");
        this.Stop = DiscordEmoji.FromUnicode("⏹");
    }
}

public class Page
{
    public string Content { get; set; }
    public DiscordEmbed Embed { get; set; }

    public Page(string content = "", DiscordEmbedBuilder embed = null)
    {
        this.Content = content;
        this.Embed = embed?.Build();
    }
}
