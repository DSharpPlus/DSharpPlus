using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;

namespace DSharpPlus.Interactivity.Extensions
{
    /// <summary>
    /// Interactivity extension methods for <see cref="DiscordChannel"/>.
    /// </summary>
    public static class ChannelExtensions
    {
        /// <summary>
        /// Waits for the next message sent in this channel that satisfies the predicate.
        /// </summary>
        /// <param name="channel">The channel to monitor.</param>
        /// <param name="predicate">A predicate that should return <see langword="true"/> if a message matches.</param>
        /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel channel, Func<DiscordMessage, bool> predicate, TimeSpan? timeoutOverride = null) 
            => GetInteractivity(channel).WaitForMessageAsync(msg => msg.ChannelId == channel.Id && predicate(msg), timeoutOverride);

        /// <summary>
        /// Waits for the next message sent in this channel.
        /// </summary>
        /// <param name="channel">The channel to monitor.</param>
        /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel channel, TimeSpan? timeoutOverride = null)
            => channel.GetNextMessageAsync(msg => true, timeoutOverride);

        /// <summary>
        /// Waits for the next message sent in this channel from a specific user.
        /// </summary>
        /// <param name="channel">The channel to monitor.</param>
        /// <param name="user">The target user.</param>
        /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel channel, DiscordUser user, TimeSpan? timeoutOverride = null)
            => channel.GetNextMessageAsync(msg => msg.Author.Id == user.Id, timeoutOverride);

        /// <summary>
        /// Waits for a specific user to start typing in this channel.
        /// </summary>
        /// <param name="channel">The target channel.</param>
        /// <param name="user">The target user.</param>
        /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
        /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(this DiscordChannel channel, DiscordUser user, TimeSpan? timeoutOverride = null) 
            => GetInteractivity(channel).WaitForUserTypingAsync(user, channel, timeoutOverride);

        /// <summary>
        /// Sends a new paginated message.
        /// </summary>
        /// <param name="channel">Target channel.</param>
        /// <param name="user">The user that'll be able to control the pages.</param>
        /// <param name="pages">A collection of <see cref="Page"/> to display.</param>
        /// <param name="emojis"></param>
        /// <param name="behaviour"></param>
        /// <param name="deletion"></param>
        /// <param name="timeoutoverride"></param>
        /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
        public static Task SendPaginatedMessageAsync(this DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationEmojis emojis = null, PaginationBehaviour? behaviour = default, PaginationDeletion? deletion = default, TimeSpan? timeoutoverride = null) 
            => GetInteractivity(channel).SendPaginatedMessageAsync(channel, user, pages, emojis, behaviour, deletion, timeoutoverride);

        /// <summary>
        /// Retrieves an interactivity instance from a channel instance.
        /// </summary>
        private static InteractivityExtension GetInteractivity(DiscordChannel channel)
        {
            var client = (DiscordClient)channel.Discord;
            var interactivity = client.GetInteractivity();

            if (interactivity == null) throw new InvalidOperationException($"Interactivity is not enabled for this {(client._isShard ? "shard" : "client")}.");

            return interactivity;
        }
    }
}
