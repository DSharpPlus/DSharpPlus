using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity.InteractiveMessages.Pagination;

/// <summary>
/// Represents the button row to use with pagination by default.
/// </summary>
public sealed class PaginationButtons
{
    /// <summary>
    /// Represents the button used to skip all the way left.
    /// </summary>
    public DiscordButtonComponent SkipLeft { private get; set; }

    /// <summary>
    /// Represents the button used to go one page to the left.
    /// </summary>
    public DiscordButtonComponent Left { private get; set; }

    /// <summary>
    /// Represents the button used to display the current page and total pages.
    /// </summary>
    public DiscordButtonComponent PageDisplay { private get; set; }

    /// <summary>
    /// Represents the button used to advance one page to the right
    /// </summary>
    public DiscordButtonComponent Right { private get; set; }

    /// <summary>
    /// Represents the button used to skip all the way right.
    /// </summary>
    public DiscordButtonComponent SkipRight { private get; set; }

    /// <summary>
    /// Represents the row of buttons to use with pagination, overwriting the other specified properties.
    /// </summary>
    /// <remarks>
    /// A row may consist of up to five buttons using recognized custom IDs. Inserting custom buttons not handled by interactivity is permissible, but
    /// they must differ in their custom ID. Not all recognized interactivity buttons must (or, indeed, can) be present. Recognized IDs are: <br/>
    /// - pagination-skip-left <br/>
    /// - pagination-left <br/>
    /// - pagination-right <br/>
    /// - pagination-skip-right <br/>
    /// - pagination-stop <br/>
    /// - pagination-page-display (this will automatically be populated appropriately)<br/>
    /// </remarks>
    public IReadOnlyList<DiscordButtonComponent>? ButtonRow { private get; set; }

    internal IReadOnlyList<DiscordButtonComponent> GetButtonRow()
        => this.ButtonRow ?? [this.SkipLeft, this.Left, this.PageDisplay, this.Right, this.SkipRight];

    public PaginationButtons()
    {
        this.SkipLeft = new(DiscordButtonStyle.Secondary, "pagination-skip-left", null, false, new(DiscordEmoji.FromUnicode("⏮")));
        this.Left = new(DiscordButtonStyle.Secondary, "pagination-left", null, false, new(DiscordEmoji.FromUnicode("◀")));
        this.PageDisplay = new(DiscordButtonStyle.Secondary, "pagination-page-display", null, true);
        this.Right = new(DiscordButtonStyle.Secondary, "pagination-right", null, false, new(DiscordEmoji.FromUnicode("▶")));
        this.SkipRight = new(DiscordButtonStyle.Secondary, "pagination-skip-right", null, false, new(DiscordEmoji.FromUnicode("⏭")));
    }
}
