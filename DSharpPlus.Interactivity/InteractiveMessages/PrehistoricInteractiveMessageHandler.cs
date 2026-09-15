using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

using Microsoft.Extensions.Options;

namespace DSharpPlus.Interactivity.InteractiveMessages;

/// <summary>
/// Provides support for v1 messages in interactivity.
/// </summary>
public sealed class PrehistoricInteractiveMessageHandler : IInteractiveMessageHandler
{
    private readonly IInteractiveMessageHandler underlying;
    private readonly InteractivityOptions options;
    private readonly IInteractiveComponentResolver componentResolver;

    public PrehistoricInteractiveMessageHandler
    (
        IInteractiveMessageHandler underlying,
        IOptions<InteractivityOptions> options,
        IInteractiveComponentResolver componentResolver
    )
    {
        this.underlying = underlying;
        this.options = options.Value;
        this.componentResolver = componentResolver;
    }

    /// <inheritdoc/>
    public async Task<DiscordMessage> SendPaginatedMessageAsync(DiscordChannel channel, IReadOnlyList<Page> pages, Func<DiscordUser, bool> condition, TimeSpan? timeoutOverride = null)
    {
        DiscordMessage message = await SendInteractiveMessageAsync
        (
            channel, 
            state =>
            {
                DiscordWebhookBuilder builder = new();

                builder.AddEmbed(new DiscordEmbedBuilder().WithDescription($"{pages[state.Page].Content}\n\n-# Page {state.Page + 1}/{pages.Count + 1}"));

                if (!this.options.UseReactionsForPagination)
                {
                    builder.AddActionRowComponent(this.componentResolver.CreatePaginationButtonRow(pages[state.Page], state.IsDisabled));
                }

                return builder;
            },
            condition,
            pages.Count,
            timeoutOverride
        );

        if (this.options.UseReactionsForPagination)
        {
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏮️"));
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("◀️"));
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏹️"));
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("▶️"));
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏭️"));
        }

        return message;
    }

    /// <inheritdoc/>
    public async Task<DiscordMessage> SendPaginatedMessageAsync
    (
        DiscordInteraction interaction,
        IReadOnlyList<Page> pages,
        Func<DiscordUser, bool> condition,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
    {
        DiscordMessage message = await SendInteractiveMessageAsync
        (
            interaction, 
            state =>
            {
                DiscordWebhookBuilder builder = new();

                builder.AddEmbed(new DiscordEmbedBuilder().WithDescription($"{pages[state.Page].Content}\n\n-# Page {state.Page + 1}/{pages.Count + 1}"));

                if (!this.options.UseReactionsForPagination)
                {
                    builder.AddActionRowComponent(this.componentResolver.CreatePaginationButtonRow(pages[state.Page], state.IsDisabled));
                }

                return builder;
            },
            condition,
            pages.Count,
            ephemeral,
            timeoutOverride
        );

        if (this.options.UseReactionsForPagination)
        {
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏮️"));
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("◀️"));
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏹️"));
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("▶️"));
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("⏭️"));
        }

        return message;
    }

    // these all just delegate to the underlying real handler, they're not interesting

    /// <inheritdoc/>
    public async Task HandlePaginationInputAsync(DiscordMessage message, DiscordUser user, PaginationInputType input)
        => await this.underlying.HandlePaginationInputAsync(message, user, input);

    /// <inheritdoc/>
    public async Task HandlePaginationInputAsync(ComponentInteractionCreatedEventArgs interaction, PaginationInputType input)
        => await this.underlying.HandlePaginationInputAsync(interaction, input);

    /// <inheritdoc/>
    public async Task HandleSelectPageAsync(ComponentInteractionCreatedEventArgs interaction)
        => await this.underlying.HandleSelectPageAsync(interaction);

    /// <inheritdoc/>
    public async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        DiscordChannel channel, 
        Func<InteractiveMessageState, DiscordWebhookBuilder> interactivity, 
        Func<DiscordUser, bool> condition, 
        int pageCount,
        TimeSpan? timeoutOverride = null
    )
        => await this.underlying.SendInteractiveMessageAsync(channel, interactivity, condition, pageCount, timeoutOverride);

    /// <inheritdoc/>
    public async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        DiscordInteraction interaction, 
        Func<InteractiveMessageState, DiscordWebhookBuilder> interactivity, 
        Func<DiscordUser, bool> condition,
        int pageCount, 
        bool ephemeral = false, 
        TimeSpan? timeoutOverride = null
    ) 
        => await this.underlying.SendInteractiveMessageAsync(interaction, interactivity, condition, pageCount, ephemeral, timeoutOverride);
}
