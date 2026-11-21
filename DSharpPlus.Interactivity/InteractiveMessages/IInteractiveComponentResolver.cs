using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

namespace DSharpPlus.Interactivity.InteractiveMessages;

/// <summary>
/// Represents a mechanism to create components for interactivity.
/// </summary>
public interface IInteractiveComponentResolver
{
    /// <summary>
    /// Creates a pagination display of the specified page.
    /// </summary>
    public DiscordTextDisplayComponent CreatePaginationDisplay(Page page);

    /// <summary>
    /// Creates a row of pagination buttons for the specified page.
    /// </summary>
    public DiscordActionRowComponent CreatePaginationButtonRow(Page page, bool isDisabled);

    /// <summary>
    /// Creates a page select dropdown given the pages provided.
    /// </summary>
    /// <param name="pages">The pages taking part in this interactivity operation.</param>
    /// <param name="pageIndex">The index of the currently selected page.</param>
    public DiscordActionRowComponent CreatePageSelectDropdown(IReadOnlyList<Page> pages, int pageIndex);
}
