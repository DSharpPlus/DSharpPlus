using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;

namespace DSharpPlus.Interactivity.Extensions;

/// <summary>
/// Interactivity extension methods for <see cref="DiscordMessage"/>.
/// </summary>
public static class MessageExtensions
{
    /// <summary>
    /// Waits for the next message that has the same author and channel as this message.
    /// </summary>
    /// <param name="message">Original message.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    public static Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordMessage message, TimeSpan? timeoutOverride = null)
        => message.Channel.GetNextMessageAsync(message.Author, timeoutOverride);

    /// <summary>
    /// Waits for the next message with the same author and channel as this message, which also satisfies a predicate.
    /// </summary>
    /// <param name="message">Original message.</param>
    /// <param name="predicate">A predicate that should return <see langword="true"/> if a message matches.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    public static Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordMessage message, Func<DiscordMessage, bool> predicate, TimeSpan? timeoutOverride = null)
        => message.Channel.GetNextMessageAsync(msg => msg.Author.Id == message.Author.Id && message.ChannelId == msg.ChannelId && predicate(msg), timeoutOverride);

    /// <summary>
    /// Waits for any button to be pressed on the specified message.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this DiscordMessage message)
        => GetInteractivity(message).WaitForButtonAsync(message);

    /// <summary>
    /// Waits for any button to be pressed on the specified message.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this DiscordMessage message, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).WaitForButtonAsync(message, timeoutOverride);

    /// <summary>
    /// Waits for any button to be pressed on the specified message.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this DiscordMessage message, CancellationToken token)
        => GetInteractivity(message).WaitForButtonAsync(message, token);

    /// <summary>
    /// Waits for a button with the specified Id to be pressed on the specified message.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="id">The Id of the button to wait for.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this DiscordMessage message, string id, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).WaitForButtonAsync(message, id, timeoutOverride);

    /// <summary>
    /// Waits for a button with the specified Id to be pressed on the specified message.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="id">The Id of the button to wait for.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this DiscordMessage message, string id, CancellationToken token)
        => GetInteractivity(message).WaitForButtonAsync(message, id, token);

    /// <summary>
    /// Waits for any button to be pressed on the specified message by the specified user.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="user">The user to wait for button input from.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this DiscordMessage message, DiscordUser user, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).WaitForButtonAsync(message, user, timeoutOverride);

    /// <summary>
    /// Waits for any button to be pressed on the specified message by the specified user.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="user">The user to wait for button input from.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this DiscordMessage message, DiscordUser user, CancellationToken token)
        => GetInteractivity(message).WaitForButtonAsync(message, user, token);

    /// <summary>
    /// Waits for any button to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="predicate">The predicate to filter interactions by.</param>
    /// <param name="timeoutOverride">Override the timeout specified in <see cref="InteractivityConfiguration"/></param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).WaitForButtonAsync(message, predicate, timeoutOverride);

    /// <summary>
    /// Waits for any button to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="predicate">The predicate to filter interactions by.</param>
    /// <param name="token">A token to cancel interactivity with at any time. Pass <see cref="CancellationToken.None"/> to wait indefinitely.</param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButtonAsync(this DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, CancellationToken token)
        => GetInteractivity(message).WaitForButtonAsync(message, predicate, token);

    /// <summary>
    /// Waits for any dropdown to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait for.</param>
    /// <param name="predicate">A filter predicate.</param>
    /// <param name="timeoutOverride">Override the timeout period specified in <see cref="InteractivityConfiguration"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the message doesn't contain any dropdowns</exception>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(this DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).WaitForSelectAsync(message, predicate, timeoutOverride);


    /// <summary>
    /// Waits for any dropdown to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait for.</param>
    /// <param name="predicate">A filter predicate.</param>
    /// <param name="token">A token that can be used to cancel interactivity. Pass <see cref="CancellationToken.None"/> to wait indefinitely.</param>
    /// <exception cref="ArgumentException">Thrown when the message doesn't contain any dropdowns</exception>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(this DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, CancellationToken token)
        => GetInteractivity(message).WaitForSelectAsync(message, predicate, token);

    /// <summary>
    /// Waits for a dropdown to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="id">The Id of the dropdown to wait for.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(this DiscordMessage message, string id, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).WaitForSelectAsync(message, id, timeoutOverride);

    /// <summary>
    /// Waits for a dropdown to be interacted with.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="id">The Id of the dropdown to wait for.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(this DiscordMessage message, string id, CancellationToken token)
        => GetInteractivity(message).WaitForSelectAsync(message, id, token);

    /// <summary>
    /// Waits for a dropdown to be interacted with by the specified user.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="user">The user to wait for.</param>
    /// <param name="id">The Id of the dropdown to wait for.</param>
    /// <param name="timeoutOverride"></param>
    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(this DiscordMessage message, DiscordUser user, string id, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).WaitForSelectAsync(message, user, id, timeoutOverride);

    /// <summary>
    /// Waits for a dropdown to be interacted with by the specified user.
    /// </summary>
    /// <param name="message">The message to wait on.</param>
    /// <param name="user">The user to wait for.</param>
    /// <param name="id">The Id of the dropdown to wait for.</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>

    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelectAsync(this DiscordMessage message, DiscordUser user, string id, CancellationToken token)
        => GetInteractivity(message).WaitForSelectAsync(message, user, id, token);


    /// <summary>
    /// Waits for a reaction on this message from a specific user.
    /// </summary>
    /// <param name="message">Target message.</param>
    /// <param name="user">The target user.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the message.</exception>
    public static Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(this DiscordMessage message, DiscordUser user, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).WaitForReactionAsync(message, user, timeoutOverride);

    /// <summary>
    /// Waits for a specific reaction on this message from the specified user.
    /// </summary>
    /// <param name="message">Target message.</param>
    /// <param name="user">The target user.</param>
    /// <param name="emoji">The target emoji.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the message.</exception>
    public static Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(this DiscordMessage message, DiscordUser user, DiscordEmoji emoji, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).WaitForReactionAsync(e => e.Emoji == emoji, message, user, timeoutOverride);

    /// <summary>
    /// Collects all reactions on this message within the timeout duration.
    /// </summary>
    /// <param name="message">The message to collect reactions from.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the message.</exception>
    public static Task<ReadOnlyCollection<Reaction>> CollectReactionsAsync(this DiscordMessage message, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).CollectReactionsAsync(message, timeoutOverride);


    /// <summary>
    /// Begins a poll using this message.
    /// </summary>
    /// <param name="message">Target message.</param>
    /// <param name="emojis">Options for this poll.</param>
    /// <param name="behaviorOverride">Overrides the action set in <see cref="InteractivityConfiguration.PaginationBehaviour"/></param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the message.</exception>
    public static Task<ReadOnlyCollection<PollEmoji>> DoPollAsync(this DiscordMessage message, IEnumerable<DiscordEmoji> emojis, PollBehaviour? behaviorOverride = null, TimeSpan? timeoutOverride = null)
        => GetInteractivity(message).DoPollAsync(message, emojis, behaviorOverride, timeoutOverride);

    /// <summary>
    /// Retrieves an interactivity instance from a message instance.
    /// </summary>
    internal static InteractivityExtension GetInteractivity(DiscordMessage message)
    {
        DiscordClient client = (DiscordClient)message.Discord;
        InteractivityExtension interactivity = client.GetInteractivity();

        return interactivity ?? throw new InvalidOperationException($"Interactivity is not enabled for this {(client._isShard ? "shard" : "client")}.");
    }
}
