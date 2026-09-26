#pragma warning disable IDE0045, IDE0046

using System;
using System.Collections.Generic;
using System.Linq;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity.InteractiveMessages.Components;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Interactivity.InteractiveMessages;

/// <summary>
/// Represents a message builder with special capabilities for adding interactive components.
/// </summary>
public sealed class InteractiveMessageBuilder : BaseDiscordMessageBuilder<InteractiveMessageBuilder>
{
    /// <summary>
    /// Adds a text display for paginated text.
    /// </summary>
    /// <returns>The current builder for chaining.</returns>
    public InteractiveMessageBuilder AddPaginatedTextDisplay()
    {
        AddArbitraryComponent(new PaginatedTextDisplayComponent());
        return this;
    }

    /// <summary>
    /// Adds a row of pagination buttons as specified in <see cref="InteractivityOptions.PaginationButtons"/>. 
    /// </summary>
    /// <returns>The current builder for chaining.</returns>
    public InteractiveMessageBuilder AddPaginationButtons()
    {
        AddArbitraryComponent(new PaginationButtonRowComponent());
        return this;
    }

    /// <summary>
    /// Adds a page selector dropdown.
    /// </summary>
    /// <returns>The current builder for chaining.</returns>
    public InteractiveMessageBuilder AddPageSelector()
    {
        AddArbitraryComponent(new PaginationPageSelectComponent());
        return this;
    }

    internal DiscordWebhookBuilder Build(IServiceProvider services, IReadOnlyList<Page> pages, InteractiveMessageState state)
    {
        int pageIndex = state.Page;
        DiscordWebhookBuilder messageBuilder = new(this);
        IInteractiveComponentResolver componentResolver = services.GetRequiredService<IInteractiveComponentResolver>();
        
        for (int i = 0; i < messageBuilder.components.Count; i++)
        {
            if (messageBuilder.components[i] is DiscordSectionComponent section)
            {
                messageBuilder.components[i] = new DiscordSectionComponent([..section.Components.Select(ResolveSpecialInteractivityComponent)], section.Accessory);
            }
            else if (messageBuilder.components[i] is DiscordContainerComponent container)
            {
                messageBuilder.components[i] = new DiscordContainerComponent([..container.Components.Select(ResolveSpecialInteractivityComponent)], container.IsSpoilered, container.Color);
            }
            else
            {
                messageBuilder.components[i] = ResolveSpecialInteractivityComponent(messageBuilder.components[i]);
            }
        }

        return messageBuilder;

        DiscordComponent ResolveSpecialInteractivityComponent(DiscordComponent component)
        {
            if (component is PaginatedTextDisplayComponent)
            {
                return componentResolver.CreatePaginationDisplay(pages[pageIndex]);
            }

            if (component is PaginationButtonRowComponent)
            {
                return componentResolver.CreatePaginationButtonRow(pages[pageIndex], state.IsDisabled);
            }

            if (component is PaginationPageSelectComponent)
            {
                return componentResolver.CreatePageSelectDropdown(pages, state.IsDisabled ? -1 : pageIndex);
            }

            return component;
        }
    }
}
