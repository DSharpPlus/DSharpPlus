using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity;

/// <summary>
/// Provides interactivity methods for interacting with messages.
/// </summary>
public static partial class MessageInteractivity
{
    // methods for waiting on the next message matching a certain condition

    /// <summary>
    /// Waits for the next message in the channel that matches a certain condition.
    /// </summary>
    /// <param name="channel">The channel to wait for a message in.</param>
    /// <param name="condition">The condition the message has to match to count.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned message or an error that might have occurred.</returns>
    public static async Task<Result<DiscordMessage>> WaitForMessageAsync
    (
        this DiscordChannel channel, 
        Func<MessageCreatedEventArgs, bool> condition, 
        TimeSpan? timeoutOverride = null
    )
    {
        TimeSpan timeout = timeoutOverride ?? channel.Discord.GetDefaultInteractivityTimeout();

        Result<MessageCreatedEventArgs> result = await channel.Discord.AsDiscordClient()
            .WaitForEventAsync<MessageCreatedEventArgs>(args => args.Channel.Id == channel.Id && condition(args), timeout);

        return result.Map(messageCreated => messageCreated.Message);
    }

    /// <summary>
    /// Waits for the next message in the channel sent by the specified user.
    /// </summary>
    /// <param name="channel">The channel to wait for a message in.</param>
    /// <param name="author">The user whose message we are waiting for.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned message or an error that might have occurred.</returns>
    public static async Task<Result<DiscordMessage>> WaitForMessageAsync(this DiscordChannel channel, DiscordUser author, TimeSpan? timeoutOverride = null)
        => await WaitForMessageAsync(channel, args => args.Author.Id == author.Id, timeoutOverride);


    /// <summary>
    /// Waits for the next message in the channel.
    /// </summary>
    /// <param name="channel">The channel to wait for a message in.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned message or an error that might have occurred.</returns>
    public static async Task<Result<DiscordMessage>> WaitForMessageAsync(this DiscordChannel channel, TimeSpan? timeoutOverride = null)
        => await WaitForMessageAsync(channel, _ => true, timeoutOverride);

    /// <summary>
    /// Waits for the next reply to the given message that matches the provided condition.
    /// </summary>
    /// <param name="message">The message to wait for a reply to.</param>
    /// <param name="condition">The condition the reply has to match to count.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned message or an error that might have occurred.</returns>
    public static async Task<Result<DiscordMessage>> WaitForReplyAsync
    (
        this DiscordMessage message,
        Func<MessageCreatedEventArgs, bool> condition,
        TimeSpan? timeoutOverride = null
    )
    {
        return await WaitForMessageAsync
        (
            message.Channel,
            args => args.Message.ReferencedMessage?.Id == message.Id && args.Message.Reference?.Type == DiscordMessageReferenceType.Default && condition(args),
            timeoutOverride
        );
    }

    /// <summary>
    /// Waits for the next reply to the given message by a specified author.
    /// </summary>
    /// <param name="message">The message to wait for a reply to.</param>
    /// <param name="author">The user whose message we are waiting for.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned message or an error that might have occurred.</returns>
    public static async Task<Result<DiscordMessage>> WaitForReplyAsync(this DiscordMessage message, DiscordUser author, TimeSpan? timeoutOverride = null)
        => await WaitForReplyAsync(message, args => args.Author.Id == author.Id, timeoutOverride);

    /// <summary>
    /// Waits for the next reply to the given message.
    /// </summary>
    /// <param name="message">The message to wait for a reply to.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned message or an error that might have occurred.</returns>
    public static async Task<Result<DiscordMessage>> WaitForReplyAsync(this DiscordMessage message, TimeSpan? timeoutOverride = null)
        => await WaitForReplyAsync(message, _ => true, timeoutOverride);

    /// <summary>
    /// Waits for a reaction to the given message that matches the specified condition.
    /// </summary>
    /// <param name="message">The message to wait for a reaction to.</param>
    /// <param name="condition">The condition a reaction needs to match.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned reaction or an error that might have occurred.</returns>
    public static async Task<Result<Reaction>> WaitForReactionAsync
    (
        this DiscordMessage message,
        Func<MessageReactionAddedEventArgs, bool> condition,
        TimeSpan? timeoutOverride = null
    )
    {
        TimeSpan timeout = timeoutOverride ?? message.Discord.GetDefaultInteractivityTimeout();

        Result<MessageReactionAddedEventArgs> result = await message.Discord.AsDiscordClient()
            .WaitForEventAsync<MessageReactionAddedEventArgs>(args => args.Message.Id == message.Id && condition(args), timeout);

        return result.Map<Reaction>(messageCreated => new(messageCreated.Emoji, [messageCreated.User]));
    }

    /// <summary>
    /// Waits for a reaction to the given message from the specified user and/or with the specified emoji..
    /// </summary>
    /// <param name="message">The message to wait for a reaction to.</param>
    /// <param name="user">The user to wait for a reaction from. This is not mutually exclusive with <paramref name="emoji"/>.</param>
    /// <param name="emoji">The emoji to wait for a reaction of. This is not mutually exclusive with <paramref name="user"/>.</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing either the returned reaction or an error that might have occurred.</returns>
    public static async Task<Result<Reaction>> WaitForReactionAsync
    (
        this DiscordMessage message,
        DiscordUser? user = null,
        DiscordEmoji? emoji = null,
        TimeSpan? timeoutOverride = null
    )
    {
        return await WaitForReactionAsync
        (
            message,
            args => (user is null || args.User.Id == user.Id) && (emoji is null || args.Emoji == emoji),
            timeoutOverride
        );
    }

    /// <summary>
    /// Collects reactions from the specified message.
    /// </summary>
    /// <param name="message">The message to collect reactions on.</param>
    /// <param name="condition">The condition a reaction needs to match to be collected..</param>
    /// <param name="timeoutOverride">An optional override for the default interactivity timeout set in <see cref="InteractivityOptions"/>.</param>
    /// <returns>A result containing the reactions submitted, or a <see cref="MessageDeletedError"/> if the message was deleted.</returns>
    public static async Task<Result<IReadOnlyList<Reaction>>> CollectReactionsAsync
    (
        this DiscordMessage message,
        Func<MessageReactionAddedEventArgs, bool> condition,
        TimeSpan? timeoutOverride = null
    )
    {
        TimeSpan timeout = timeoutOverride ?? message.Discord.GetDefaultInteractivityTimeout();

        Task<IReadOnlyList<DiscordEventArgs>> reactionEventsTask = message.Discord.AsDiscordClient()
            .CollectEventsAsync<DiscordEventArgs>
            (
                args =>
                {
                    return args switch
                    {
                        MessageReactionAddedEventArgs add => add.Message.Id == message.Id && condition(add),
                        MessageReactionRemovedEventArgs remove => remove.Message.Id == message.Id,
                        MessageReactionRemovedEmojiEventArgs emojisCleared => emojisCleared.Message.Id == message.Id,
                        MessageReactionsClearedEventArgs clear => clear.Message.Id == message.Id,
                        _ => false
                    };
                }, 
                timeout
            );

        Task<Result<MessageDeletedEventArgs>> messageDeletedTask = message.Discord.AsDiscordClient()
            .WaitForEventAsync<MessageDeletedEventArgs>(args => args.Message.Id == message.Id, timeout);

        await Task.WhenAny(reactionEventsTask, messageDeletedTask);

        // if the message was deleted or reactions cleared, we have nothing to return

        if (messageDeletedTask.IsCompleted && !messageDeletedTask.Result.IsTimedOut())
        {
            return new MessageDeletedError("The target message was deleted while collecting reactions.");
        }

        // we have reactions. theoretically, there might be a race condition in cache eviction here, so let's await again
        IReadOnlyList<DiscordEventArgs> events = await reactionEventsTask;

        Dictionary<DiscordEmoji, HashSet<DiscordUser>> reactions = [];
        HashSet<DiscordUser> list;

        foreach (DiscordEventArgs @event in events)
        {
            switch (@event)
            {
                case MessageReactionAddedEventArgs added:

                    list = reactions.GetValueOrDefault(added.Emoji) ?? [];

                    list.Add(added.User);
                    reactions[added.Emoji] = list;

                    break;

                case MessageReactionRemovedEventArgs removed:

                    list = reactions.GetValueOrDefault(removed.Emoji) ?? [];

                    _ = list.Remove(removed.User);
                    reactions[removed.Emoji] = list;

                    break;

                case MessageReactionRemovedEmojiEventArgs emojiCleared:

                    _ = reactions.Remove(emojiCleared.Emoji);
                    break;

                case MessageReactionsClearedEventArgs:

                    reactions.Clear();
                    break;
            }
        }

        return new([..reactions.Where(x => x.Value.Count > 0).Select(x => new Reaction(x.Key, [..x.Value]))]);
    }

    /// <inheritdoc cref="CollectReactionsAsync(DiscordMessage, Func{MessageReactionAddedEventArgs, bool}, TimeSpan?)"/>
    public static async Task<Result<IReadOnlyList<Reaction>>> CollectReactionsAsync(this DiscordMessage message, TimeSpan? timeoutOverride = null)
        => await CollectReactionsAsync(message, _ => true, timeoutOverride);
}
