using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return await interactivity.WaitForMessageAsync(x => x.ChannelId == c.Id && predicate(x));
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
    }
}
