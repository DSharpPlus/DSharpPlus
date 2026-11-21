using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.InteractiveMessages.Pagination;

namespace DSharpPlus.Interactivity.InteractiveMessages;

/// <summary>
/// Specifies a mechanism to handle interactive messages.
/// </summary>
public interface IInteractiveMessageHandler
{
    /// <summary>
    /// Sends the specified interactive message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send to.</param>
    /// <param name="interactivity">A delegate for generating interactive pages from their state.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="pageCount">The amount of pages supported for this message. May be zero if this message is not paginated.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public Task<DiscordMessage> SendInteractiveMessageAsync
    (
        DiscordChannel channel,
        Func<InteractiveMessageState, DiscordWebhookBuilder> interactivity,
        Func<DiscordUser, bool> condition,
        int pageCount,
        TimeSpan? timeoutOverride = null
    );

    // this exists because v1 messages need to take a different code path here, and we need to support that legacy behaviour per
    // https://canary.discord.com/channels/379378609942560770/379386730538860554/1521239470706266193
    // O tempora, o mores!

    /// <summary>
    /// Sends a paginated message to the specified channel.
    /// </summary>
    /// <param name="channel">The channel to send to.</param>
    /// <param name="pages">The pages to send to the specified channel.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public Task<DiscordMessage> SendPaginatedMessageAsync(DiscordChannel channel, IReadOnlyList<Page> pages, Func<DiscordUser, bool> condition, TimeSpan? timeoutOverride = null);

    /// <summary>
    /// Sends the specified interactive message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond to with this message.</param>
    /// <param name="interactivity">A delegate for generating interactive pages from their state.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="pageCount">The amount of pages supported for this message. May be zero if this message is not paginated.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public Task<DiscordMessage> SendInteractiveMessageAsync
    (
        DiscordInteraction interaction,
        Func<InteractiveMessageState, DiscordWebhookBuilder> interactivity,
        Func<DiscordUser, bool> condition,
        int pageCount,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    );

    /// <summary>
    /// Sends a paginated message in response to the specified interaction.
    /// </summary>
    /// <param name="interaction">The interaction to respond to with this message.</param>
    /// <param name="pages">The pages to send to the specified channel.</param>
    /// <param name="condition">A condition users must match before their inputs are considered to be valid.</param>
    /// <param name="ephemeral">Indicates whether this response should be ephemeral.</param>
    /// <param name="timeoutOverride">An override for the default interactivity timeout.</param>
    public Task<DiscordMessage> SendPaginatedMessageAsync
    (
        DiscordInteraction interaction,
        IReadOnlyList<Page> pages,
        Func<DiscordUser, bool> condition,
        bool ephemeral = false,
        TimeSpan? timeoutOverride = null
    );

    /// <summary>
    /// Ingests the specified pagination input. This is a legacy code path to support reaction-based interactivity.
    /// </summary>
    /// <param name="message">The message a pagination input has been made to.</param>
    /// <param name="user">The user who made the input.</param>
    /// <param name="input">The pagination input.</param>
    public Task HandlePaginationInputAsync(DiscordMessage message, DiscordUser user, PaginationInputType input);

    /// <summary>
    /// Ingests the specified pagination input.
    /// </summary>
    /// <remarks>
    /// Implementers should respond to interactions where the message ID matches a currently valid interactive message. Implementers should
    /// assume that inputs to invalid messages are not meant for this feature.
    /// </remarks>
    /// <param name="interaction">The input interaction.</param>
    /// <param name="input">The pagination input.</param>
    public Task HandlePaginationInputAsync(ComponentInteractionCreatedEventArgs interaction, PaginationInputType input);

    /// <summary>
    /// Selects the specified page on the specified emssage.
    /// </summary>
    /// <remarks>
    /// Implementers should respond to interactions where the message ID matches a currently valid interactive message. Implementers should
    /// assume that inputs to invalid messages are not meant for this feature.
    /// </remarks>
    /// <param name="interaction">The input interaction.</param>
    public Task HandleSelectPageAsync(ComponentInteractionCreatedEventArgs interaction);
}
