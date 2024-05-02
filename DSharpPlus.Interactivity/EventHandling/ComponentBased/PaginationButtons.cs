
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity.EventHandling;
public class PaginationButtons
{
    public DiscordButtonComponent SkipLeft { internal get; set; }
    public DiscordButtonComponent Left { internal get; set; }
    public DiscordButtonComponent Stop { internal get; set; }
    public DiscordButtonComponent Right { internal get; set; }
    public DiscordButtonComponent SkipRight { internal get; set; }

    internal DiscordButtonComponent[] ButtonArray =>
    // This isn't great but I can't figure out how to pass these elements by ref :(
    [                                                      // And yes, it should be by ref to begin with, but in testing it refuses to update.
        SkipLeft,                                     // So I have no idea what that's about, and this array is "cheap-enough" and infrequent
        Left,                                         // enough to the point that it *should* be fine.
        Stop,
        Right,
        SkipRight
    ];

    public PaginationButtons()
    {
        SkipLeft = new(DiscordButtonStyle.Secondary, "leftskip", null, false, new(DiscordEmoji.FromUnicode("⏮")));
        Left = new(DiscordButtonStyle.Secondary, "left", null, false, new(DiscordEmoji.FromUnicode("◀")));
        Stop = new(DiscordButtonStyle.Secondary, "stop", null, false, new(DiscordEmoji.FromUnicode("⏹")));
        Right = new(DiscordButtonStyle.Secondary, "right", null, false, new(DiscordEmoji.FromUnicode("▶")));
        SkipRight = new(DiscordButtonStyle.Secondary, "rightskip", null, false, new(DiscordEmoji.FromUnicode("⏭")));
    }

    public PaginationButtons(PaginationButtons other)
    {
        Stop = new(other.Stop);
        Left = new(other.Left);
        Right = new(other.Right);
        SkipLeft = new(other.SkipLeft);
        SkipRight = new(other.SkipRight);
    }
}
