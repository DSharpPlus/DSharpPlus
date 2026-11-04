#pragma warning disable IDE0040

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity.InteractiveMessages;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Interactivity;

partial class MessageExtensions
{
    /// <summary>
    /// Sends the specified interactive message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send this message to.</param>
    /// <param name="interactiveFactory">A delegate for generating interactive pages from their state.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="pageCount">The amount of pages supported for this message. May be zero if this message is not paginated.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordChannel channel,
        Func<InteractiveMessageState, DiscordWebhookBuilder> interactiveFactory,
        Func<DiscordUser, bool> condition,
        int pageCount,
        TimeSpan? timeoutOverride = null
    )
    {
        IInteractiveMessageHandler interactivity = channel.Discord.AsDiscordClient().ServiceProvider
            .GetRequiredService<IInteractiveMessageHandler>();

        return await interactivity.SendInteractiveMessageAsync
        (
            channel,
            interactiveFactory,
            condition,
            pageCount,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends the specified interactive message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send this message to.</param>
    /// <param name="interactiveFactory">A delegate for generating interactive pages from their state.</param>
    /// <param name="user">The user who may interact with this message.</param>
    /// <param name="pageCount">The amount of pages supported for this message. May be zero if this message is not paginated.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordChannel channel,
        Func<InteractiveMessageState, DiscordWebhookBuilder> interactiveFactory,
        DiscordUser user,
        int pageCount,
        TimeSpan? timeoutOverride = null
    )
    {
        return await SendInteractiveMessageAsync
        (
            channel,
            interactiveFactory,
            candidate => candidate.Id == user.Id,
            pageCount,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends the specified interactive message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send this message to.</param>
    /// <param name="builder">The scheme to generate interactive messages from.</param>
    /// <param name="pages">The pages to send along with this message.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordChannel channel,
        InteractiveMessageBuilder builder,
        IReadOnlyList<Page> pages,
        Func<DiscordUser, bool> condition,
        TimeSpan? timeoutOverride = null
    )
    {
        IServiceProvider services = channel.Discord.AsDiscordClient().ServiceProvider;

        return await SendInteractiveMessageAsync
        (
            channel,
            state => builder.Build(services, pages, state),
            condition,
            pages.Count,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends the specified interactive message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send this message to.</param>
    /// <param name="builder">The scheme to generate interactive messages from.</param>
    /// <param name="pages">The pages to send along with this message.</param>
    /// <param name="user">The user who may interact with this message.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordChannel channel,
        InteractiveMessageBuilder builder,
        IReadOnlyList<Page> pages,
        DiscordUser user,
        TimeSpan? timeoutOverride = null
    )
    {
        return await SendInteractiveMessageAsync
        (
            channel,
            builder,
            pages,
            candidate => candidate.Id == user.Id,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends the specified interactive message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send this message to.</param>
    /// <param name="builder">The scheme to generate interactive messages from.</param>
    /// <param name="text">The text to send with this message.</param>
    /// <param name="splitType">The scheme to use for splitting text into pages.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordChannel channel,
        InteractiveMessageBuilder builder,
        string text,
        Func<DiscordUser, bool> condition,
        PageSplitType splitType = PageSplitType.CharacterWise,
        TimeSpan? timeoutOverride = null
    )
    {
        IServiceProvider services = channel.Discord.AsDiscordClient().ServiceProvider;
        IReadOnlyList<Page> pages = PageSplitter.GeneratePages(text, splitType);

        return await SendInteractiveMessageAsync
        (
            channel,
            state => builder.Build(services, pages, state),
            condition,
            pages.Count,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends the specified interactive message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send this message to.</param>
    /// <param name="builder">The scheme to generate interactive messages from.</param>
    /// <param name="text">The text to send with this message.</param>
    /// <param name="splitType">The scheme to use for splitting text into pages.</param>
    /// <param name="user">The user who may interact with this message.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendInteractiveMessageAsync
    (
        this DiscordChannel channel,
        InteractiveMessageBuilder builder,
        string text,
        DiscordUser user,
        PageSplitType splitType = PageSplitType.CharacterWise,
        TimeSpan? timeoutOverride = null
    )
    {
        IReadOnlyList<Page> pages = PageSplitter.GeneratePages(text, splitType);

        return await SendInteractiveMessageAsync
        (
            channel,
            builder,
            pages,
            candidate => candidate.Id == user.Id,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends a paginated message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send this message to.</param>
    /// <param name="pages">The pages to send along with this message.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendPaginatedMessageAsync
    (
        this DiscordChannel channel,
        IReadOnlyList<Page> pages,
        Func<DiscordUser, bool> condition,
        TimeSpan? timeoutOverride = null
    )
    {
        IInteractiveMessageHandler interactivity = channel.Discord.AsDiscordClient().ServiceProvider
            .GetRequiredService<IInteractiveMessageHandler>();

        return await interactivity.SendPaginatedMessageAsync
        (
            channel,
            pages,
            condition,
            timeoutOverride
        );
    }

    /// <summary>
    /// Sends a paginated message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send this message to.</param>
    /// <param name="pages">The pages to send along with this message.</param>
    /// <param name="user">The user who may interact with this message.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendPaginatedMessageAsync
    (
        this DiscordChannel channel,
        IReadOnlyList<Page> pages,
        DiscordUser user,
        TimeSpan? timeoutOverride = null
    )
        => await SendPaginatedMessageAsync(channel, pages, candidate => candidate.Id == user.Id, timeoutOverride);

    /// <summary>
    /// Sends a paginated message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send this message to.</param>
    /// <param name="text">The text to send with this message.</param>
    /// <param name="splitType">The scheme to use for splitting text into pages.</param>
    /// <param name="user">The user who may interact with this message.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public static async Task<DiscordMessage> SendPaginatedMessageAsync
    (
        this DiscordChannel channel,
        string text,
        DiscordUser user,
        PageSplitType splitType = PageSplitType.CharacterWise,
        TimeSpan? timeoutOverride = null
    )
    {
        IReadOnlyList<Page> pages = PageSplitter.GeneratePages(text, splitType);

        return await SendPaginatedMessageAsync(channel, pages, candidate => candidate.Id == user.Id, timeoutOverride);
    }
}
