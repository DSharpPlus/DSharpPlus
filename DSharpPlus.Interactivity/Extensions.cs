using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Interactivity;
using DSharpPlus.EventArgs;
using System.Collections.ObjectModel;

namespace DSharpPlus.Interactivity
{
    public static class Extensions
    {
        /// <summary>
        /// Returns the next message sent in this channel that matches the predicate.
        /// </summary>
        /// <param name="c">Channel message was sent in.</param>
        /// <param name="predicate">Predicate to match message to.</param>
        /// <param name="timeoutoverride">Timeout override.</param>
        /// <returns></returns>
        public static async Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel c, Func<DiscordMessage, bool> predicate,
            TimeSpan? timeoutoverride = null)
        {
            if (!(c.Discord is DiscordClient))
                throw new InvalidOperationException("Your client is not a DiscordClient!");

            var discord = (DiscordClient)c.Discord;
            var interactivity = discord.GetInteractivity();

            if (interactivity == null)
                throw new InvalidOperationException("Interactivity was not set up!");

            var timeout = timeoutoverride ?? interactivity.Config.Timeout;
            return await interactivity.WaitForMessageAsync(x => x.ChannelId == c.Id && predicate(x), timeout);
        }

        /// <summary>
        /// Gets the next message sent in this channel.
        /// </summary>
        /// <param name="c">Channel message was sent in.</param>
        /// <param name="timeoutoverride">Timeout override.</param>
        /// <returns></returns>
        public static async Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel c, TimeSpan? timeoutoverride = null)
            => await c.GetNextMessageAsync(x => true, timeoutoverride);

        /// <summary>
        /// Gets the next message sent in this channel by a specific user.
        /// </summary>
        /// <param name="c">Channel message was sent in.</param>
        /// <param name="m">Member message was sent by.</param>
        /// <param name="timeoutoverride">Timeout override.</param>
        /// <returns></returns>
        public static async Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel c, DiscordMember m, TimeSpan? timeoutoverride = null)
            => await c.GetNextMessageAsync(x => x.Author.Id == m.Id, timeoutoverride);

        /// <summary>
        /// Gets the next message with the same channel and user.
        /// </summary>
        /// <param name="m">Message to follow up.</param>
        /// <param name="timeoutoverride">Timeout override.</param>
        /// <returns></returns>
        public static async Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordMessage m, TimeSpan? timeoutoverride = null)
            => await m.Channel.GetNextMessageAsync(x => x.Author.Id == m.Author.Id && m.ChannelId == x.ChannelId, timeoutoverride);

        /// <summary>
        /// Gets the next message with the same channel and user, matching a predicate.
        /// </summary>
        /// <param name="m">Message to follow up.</param>
        /// <param name="predicate">Predicate to match.</param>
        /// <param name="timeoutoverride">Timeout override.</param>
        /// <returns></returns>
        public static async Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordMessage m, Func<DiscordMessage, bool> predicate, 
            TimeSpan? timeoutoverride = null)
            => await m.Channel.GetNextMessageAsync(x => x.Author.Id == m.Author.Id && m.ChannelId == x.ChannelId && predicate(x), timeoutoverride);

        /// <summary>
        /// Does a poll on a message
        /// </summary>
        /// <param name="m">Message to do a poll on.</param>
        /// <param name="emojis">Emojis to poll.</param>
        /// <param name="behaviour">Poll behaviour.</param>
        /// <param name="timeout">Override timeout period.</param>
        /// <returns></returns>
        public static async Task<ReadOnlyCollection<PollEmoji>> DoPollAsync(this DiscordMessage m, DiscordEmoji[] emojis, PollBehaviour behaviour,
            TimeSpan? timeout = null)
            => await ((DiscordClient)m.Discord).GetInteractivity().DoPollAsync(m, emojis, behaviour, timeout);

        /// <summary>
        /// waits for a reaction on a message.
        /// </summary>
        /// <param name="m">Message to wait on.</param>
        /// <param name="user">User to send a reaction.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public static async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(this DiscordMessage m, DiscordUser user,
            TimeSpan? timeoutoverride = null)
            => await ((DiscordClient)m.Discord).GetInteractivity().WaitForReactionAsync(m, user, timeoutoverride);

        /// <summary>
        /// Waits for a reaction on a message.
        /// </summary>
        /// <param name="m">Message to wait on.</param>
        /// <param name="user">User to send a reaction.</param>
        /// <param name="emoji">Emoji to wait for.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public static async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(this DiscordMessage m, DiscordUser user,
            DiscordEmoji emoji, TimeSpan? timeoutoverride = null)
            => await ((DiscordClient)m.Discord).GetInteractivity().WaitForReactionAsync(x => x.Emoji == emoji, m, user, timeoutoverride);

        /// <summary>
        /// Waits for a user to start typing
        /// </summary>
        /// <param name="c">Channel user is typing in.</param>
        /// <param name="user">User to start typing.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public static async Task WaitForUserTypingAsync(this DiscordChannel c, DiscordUser user, TimeSpan? timeoutoverride = null)
            => await ((DiscordClient)c.Discord).GetInteractivity().WaitForUserTypingAsync(user, c, timeoutoverride);

        /// <summary>
        /// Collects reactions on this messages
        /// </summary>
        /// <param name="m">Message to collect reactions from</param>
        /// <param name="timeoutoverride">Override timeout period</param>
        /// <returns></returns>
        public static async Task CollectReactionsAsync(this DiscordMessage m, TimeSpan? timeoutoverride = null)
            => await ((DiscordClient)m.Discord).GetInteractivity().CollectReactionsAsync(m, timeoutoverride);

        /// <summary>
        /// Sends a paginated message
        /// </summary>
        /// <param name="c">Channel to send message in.</param>
        /// <param name="user">User to control pagination.</param>
        /// <param name="pages">Pages to send.</param>
        /// <param name="emojis">Pagination emojis (emojis set to null will be disabled)</param>
        /// <param name="behaviour">Pagination behaviour.</param>
        /// <param name="deletion">Deletion behaviour.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public static async Task SendPaginatedMessageAsync(this DiscordChannel c, DiscordUser user, Page[] pages, PaginationEmojis emojis,
            PaginationBehaviour? behaviour = default, PaginationDeletion? deletion = default,
            TimeSpan? timeoutoverride = null)
            => await ((DiscordClient)c.Discord).GetInteractivity().SendPaginatedMessageAsync(c, user, pages, emojis, behaviour, deletion, timeoutoverride);
    }
}

// note to future self: do your god damn homework