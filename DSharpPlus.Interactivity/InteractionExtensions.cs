using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.InteractiveMessages;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Interactivity;

/// <summary>
/// Provides extension methods for interactive behaviour on interactions.
/// </summary>
public static class InteractionExtensions
{
    /// <summary>
    /// Sends a modal and returns it once it has been submitted.
    /// </summary>
    /// <param name="interaction">The interaction to respond with a modal to.</param>
    /// <param name="modalBuilder">The modal builder to respond with.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the submitted modal or an error that might have occurred.</returns>
    public static async Task<Result<ModalSubmittedEventArgs>> SendAndWaitForModalAsync
    (
        this DiscordInteraction interaction,
        DiscordModalBuilder modalBuilder,
        TimeSpan? timeoutOverride = null
    )
    {
        TimeSpan timeout = timeoutOverride ?? interaction.Discord.GetDefaultInteractivityTimeout();

        Ulid id = Ulid.NewUlid();
        string customId = id.ToString();

        modalBuilder.WithCustomId(customId);

        Task<Result<ModalSubmittedEventArgs>> modalTask = interaction.Discord.AsDiscordClient()
            .WaitForEventAsync<ModalSubmittedEventArgs>(x => x.Interaction.Data.CustomId == customId, timeout);

        await interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modalBuilder);

        return await modalTask;
    }

    /// <summary>
    /// Sends the specified interactive message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond with this message to.</param>
    /// <param name="interactiveFactory">A delegate for generating interactive pages from their state.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="pageCount">The amount of pages supported for this message. May be zero if this message is not paginated.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordInteraction interaction,
        Func<InteractiveMessageState, DiscordWebhookBuilder> interactiveFactory,
        Func<DiscordUser, bool> condition,
        int pageCount,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
    {
        IInteractiveMessageHandler interactivity = interaction.Discord.AsDiscordClient().ServiceProvider
            .GetRequiredService<IInteractiveMessageHandler>();

        return await interactivity.SendInteractiveMessageAsync
        (
            interaction,
            interactiveFactory,
            condition,
            pageCount,
            ephemeral,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends the specified interactive message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond with this message to.</param>
    /// <param name="interactiveFactory">A delegate for generating interactive pages from their state.</param>
    /// <param name="user">The user who may interact with this message.</param>
    /// <param name="pageCount">The amount of pages supported for this message. May be zero if this message is not paginated.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordInteraction interaction,
        Func<InteractiveMessageState, DiscordWebhookBuilder> interactiveFactory,
        DiscordUser user,
        int pageCount,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
    {
        return await SendInteractiveMessageAsync
        (
            interaction,
            interactiveFactory,
            candidate => candidate.Id == user.Id,
            pageCount,
            ephemeral,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends the specified interactive message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond with this message to.</param>
    /// <param name="builder">The scheme to generate interactive messages from.</param>
    /// <param name="pages">The pages to send along with this message.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordInteraction interaction,
        InteractiveMessageBuilder builder,
        IReadOnlyList<Page> pages,
        Func<DiscordUser, bool> condition,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
    {
        IServiceProvider services = interaction.Discord.AsDiscordClient().ServiceProvider;

        return await SendInteractiveMessageAsync
        (
            interaction,
            state => builder.Build(services, pages, state),
            condition,
            pages.Count,
            ephemeral,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends the specified interactive message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond with this message to.</param>
    /// <param name="builder">The scheme to generate interactive messages from.</param>
    /// <param name="pages">The pages to send along with this message.</param>
    /// <param name="user">The user who may interact with this message.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordInteraction interaction,
        InteractiveMessageBuilder builder,
        IReadOnlyList<Page> pages,
        DiscordUser user,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
    {
        return await SendInteractiveMessageAsync
        (
            interaction,
            builder,
            pages,
            candidate => candidate.Id == user.Id,
            ephemeral,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends the specified interactive message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond with this message to.</param>
    /// <param name="builder">The scheme to generate interactive messages from.</param>
    /// <param name="text">The text to send with this message.</param>
    /// <param name="splitType">The scheme to use for splitting text into pages.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordInteraction interaction,
        InteractiveMessageBuilder builder,
        string text,
        Func<DiscordUser, bool> condition,
        PageSplitType splitType = PageSplitType.CharacterWise,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
    {
        IServiceProvider services = interaction.Discord.AsDiscordClient().ServiceProvider;
        IReadOnlyList<Page> pages = PageSplitter.GeneratePages(text, splitType);

        return await SendInteractiveMessageAsync
        (
            interaction,
            state => builder.Build(services, pages, state),
            condition,
            pages.Count,
            ephemeral,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends the specified interactive message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond with this message to.</param>
    /// <param name="builder">The scheme to generate interactive messages from.</param>
    /// <param name="text">The text to send with this message.</param>
    /// <param name="splitType">The scheme to use for splitting text into pages.</param>
    /// <param name="user">The user who may interact with this message.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordInteraction interaction,
        InteractiveMessageBuilder builder,
        string text,
        DiscordUser user,
        PageSplitType splitType = PageSplitType.CharacterWise,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
    {
        IReadOnlyList<Page> pages = PageSplitter.GeneratePages(text, splitType);

        return await SendInteractiveMessageAsync
        (
            interaction,
            builder,
            pages,
            candidate => candidate.Id == user.Id,
            ephemeral,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends a paginated message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond with this message to.</param>
    /// <param name="pages">The pages to send along with this message.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendPaginatedMessageAsync
    (
        this DiscordInteraction interaction,
        IReadOnlyList<Page> pages,
        Func<DiscordUser, bool> condition,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
    {
        IInteractiveMessageHandler interactivity = interaction.Discord.AsDiscordClient().ServiceProvider
            .GetRequiredService<IInteractiveMessageHandler>();

        return await interactivity.SendPaginatedMessageAsync
        (
            interaction,
            pages,
            condition,
            ephemeral,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends a paginated message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond with this message to.</param>
    /// <param name="pages">The pages to send along with this message.</param>
    /// <param name="user">The user who may interact with this message.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendPaginatedMessageAsync
    (
        this DiscordInteraction interaction,
        IReadOnlyList<Page> pages,
        DiscordUser user,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
        => await SendPaginatedMessageAsync(interaction, pages, candidate => candidate.Id == user.Id, ephemeral, timeoutOverride);

    /// <summary>
    /// Sends a paginated message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond with this message to.</param>
    /// <param name="text">The text to send with this message.</param>
    /// <param name="splitType">The scheme to use for splitting text into pages.</param>
    /// <param name="user">The user who may interact with this message.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendPaginatedMessageAsync
    (
        this DiscordInteraction interaction,
        string text,
        DiscordUser user,
        PageSplitType splitType = PageSplitType.CharacterWise,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    )
    {
        IReadOnlyList<Page> pages = PageSplitter.GeneratePages(text, splitType);

        return await SendPaginatedMessageAsync(interaction, pages, candidate => candidate.Id == user.Id, ephemeral, timeoutOverride);
    }
}
