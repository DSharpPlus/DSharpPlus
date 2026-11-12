using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

using Microsoft.Extensions.Options;

namespace DSharpPlus.Interactivity.InteractiveMessages;

/// <summary>
/// The default implementation of <see cref="IInteractiveComponentResolver"/>. 
/// </summary>
public sealed class DefaultInteractiveComponentResolver : IInteractiveComponentResolver
{
    private readonly InteractivityOptions options;

    public DefaultInteractiveComponentResolver(IOptions<InteractivityOptions> options) 
        => this.options = options.Value;

    /// <inheritdoc/>
    public DiscordActionRowComponent CreatePaginationButtonRow(Page page, bool isDisabled)
    {
        DiscordButtonComponent[] buttons = [..this.options.PaginationButtons.GetButtonRow()];

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].CustomId == "pagination-page-display")
            {
                buttons[i] = new(buttons[i].Style, "pagination-page-display", $"{page.Index + 1}/{page.TotalPages + 1}", buttons[i].Disabled, buttons[i].Emoji);
            }

            if (isDisabled)
            {
                buttons[i].Disable();
            }
        }

        return new(buttons);
    }

    /// <inheritdoc/>
    public DiscordTextDisplayComponent CreatePaginationDisplay(Page page)
        => new(page.Content);

    // this code is adapted from 
    // https://github.com/OoLunar/DocBot/blob/b37a73b19977a1ea4fdec7d892b2671c18608151/src/Interactivity/Moments/Pagination/PaginationDefaultComponentCreator.cs
    //
    // SPDX-License-Identifier: MIT
    // Copyright holder: Lunar Starstrum, https://github.com/OoLunar
    /// <inheritdoc/>
    public DiscordActionRowComponent CreatePageSelectDropdown(IReadOnlyList<Page> pages, int pageIndex)
    {
        if (pageIndex == -1)
        {
            return new([new DiscordSelectComponent(
                customId: "pagination-page-select",
                placeholder: "Select a page (Disabled)",
                options: pages.Take(1).Select((page, index) => new DiscordSelectComponentOption(
                    label: page.Name ?? $"Page {index + 1}",
                    value: index.ToString(CultureInfo.InvariantCulture),
                    description: page.Content!,
                    isDefault: false)
                ),
                disabled: true
            )]);
        }

        int startingIndex = int.Max(pageIndex - 12, 0);
        int maxIndex = startingIndex + CalculatePageCount(startingIndex, pages.Count);

        List<DiscordSelectComponentOption> options = [];
        for (int i = startingIndex; i < maxIndex; i++)
        {
            Page page = pages[i];
            options.Add(new DiscordSelectComponentOption(
                label: page.Name ?? $"Page {i + 1}/{pages.Count}",
                value: i.ToString(CultureInfo.InvariantCulture),
                description: page.Content,
                isDefault: pageIndex == i)
            );
        }

        if ((maxIndex - startingIndex) < 25)
        {
            int section = pageIndex / 23;
            if (section > 0)
            {
                options.Insert(0, CreatePreviousSectionOption(pageIndex, pages));
            }

            if (maxIndex < pages.Count)
            {
                options.Add(CreateNextSectionOption(pageIndex, pages));
            }
        }

        return new([new DiscordSelectComponent(
            customId: "pagination-page-select",
            placeholder: $"Select a page ({pageIndex + 1}/{pages.Count})",
            options: options
        )]);
    }

    private static int CalculatePageCount(int startingIndex, int totalPageCount)
    {
        if (startingIndex == 0)
        {
            // Return 24 so that the "Next Section" button is added.
            return totalPageCount > 25 ? 24 : int.Min(24, totalPageCount);
        }

        // Return 23 so that the "Previous Section" and "Next Section" buttons are added.
        return totalPageCount - startingIndex > 24 ? 23 : totalPageCount - startingIndex;
    }

    private static DiscordSelectComponentOption CreatePreviousSectionOption(int currentPageIndex, IReadOnlyList<Page> pages)
    {
        int section = currentPageIndex / 23;
        int previousSectionIndex = int.Max(0, ((section - 1) * 23) - 12);
        int previousSectionMaxIndex = previousSectionIndex + CalculatePageCount(previousSectionIndex, pages.Count);

        return new DiscordSelectComponentOption(
            label: "Previous Section",
            value: ((section - 1) * 23).ToString(CultureInfo.InvariantCulture),
            description: $"Go to the previous section (pages {previousSectionIndex + 1}-{previousSectionMaxIndex})",
            isDefault: false,
            emoji: new DiscordComponentEmoji("⬅️")
        );
    }

    private static DiscordSelectComponentOption CreateNextSectionOption(int currentPageIndex, IReadOnlyList<Page> pages)
    {
        int section = currentPageIndex / 23;
        int nextSectionIndex = int.Max(1, ((section + 1) * 23) - 12);
        int nextSectionMaxIndex = nextSectionIndex + CalculatePageCount(nextSectionIndex, pages.Count);

        return new DiscordSelectComponentOption(
            label: "Next Section",
            value: ((section + 1) * 23).ToString(CultureInfo.InvariantCulture),
            description: $"Go to the next section (pages {nextSectionIndex + 1}-{nextSectionMaxIndex})",
            isDefault: false,
            emoji: new DiscordComponentEmoji("➡️")
        );
    }
}
