using DSharpPlus.Entities;
namespace DSharpPlus.Interactivity.EventHandling;

public class PaginationButtons
{
    public DiscordButtonComponent SkipLeft { internal get; set; }
    public DiscordButtonComponent Left { internal get; set; }
    public DiscordButtonComponent Stop { internal get; set; }
    public DiscordButtonComponent Right { internal get; set; }
    public DiscordButtonComponent SkipRight { internal get; set; }

    internal DiscordButtonComponent[] ButtonArray => new[] // This isn't great but I can't figure out how to pass these elements by ref :(
    {                                                      // And yes, it should be by ref to begin with, but in testing it refuses to update.
        this.SkipLeft,                                     // So I have no idea what that's about, and this array is "cheap-enough" and infrequent
        this.Left,                                         // enough to the point that it *should* be fine.
        this.Stop,
        this.Right,
        this.SkipRight
    };

    public PaginationButtons()
    {
        this.SkipLeft = new(ButtonStyle.Secondary, "leftskip", null, false, new(DiscordEmoji.FromUnicode("⏮")));
        this.Left = new(ButtonStyle.Secondary, "left", null, false, new(DiscordEmoji.FromUnicode("◀")));
        this.Stop = new(ButtonStyle.Secondary, "stop", null, false, new(DiscordEmoji.FromUnicode("⏹")));
        this.Right = new(ButtonStyle.Secondary, "right", null, false, new(DiscordEmoji.FromUnicode("▶")));
        this.SkipRight = new(ButtonStyle.Secondary, "rightskip", null, false, new(DiscordEmoji.FromUnicode("⏭")));
    }

    public PaginationButtons(PaginationButtons other)
    {
        this.Stop = new(other.Stop);
        this.Left = new(other.Left);
        this.Right = new(other.Right);
        this.SkipLeft = new(other.SkipLeft);
        this.SkipRight = new(other.SkipRight);
    }
}
