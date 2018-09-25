using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity
{
    public static partial class InteractivityExtensionMethods
    {
        /// <summary>
        /// Waits for a message to be received
        /// </summary>
        /// <param name="chn">Channel to listen to</param>
        /// <param name="user">User to check for</param>
        /// <param name="contentpredicate">Predicate for message content</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
        public static async Task<MessageContext> WaitForMessageAsync(this DiscordChannel chn, DiscordUser user, Func<string, bool> contentpredicate,
            TimeSpan? timeoutoverride = null)
        {
            if (chn.Discord.GetType() != typeof(DiscordClient))
                throw new InvalidOperationException("Your client is not a default DiscordClient!");

            var client = (DiscordClient)chn.Discord;

            if (client.GetInteractivity() == null)
                throw new NullReferenceException("Your interactivity module has not been initialized for this client!");

            var interactivity = client.GetInteractivity();

            return await interactivity.WaitForMessageAsync(x => x.ChannelId == chn.Id && x.Author.Id == user.Id && contentpredicate(x.Content),
                timeoutoverride);
        }

        /// <summary>
        /// Waits for a reaction
        /// </summary>
        /// <param name="mes">Message to wait on</param>
        /// <param name="user">User reaction should belong to</param>
        /// <param name="emoji">Emoji to listen for</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
        public static async Task<ReactionContext> WaitForReactionAsync(this DiscordMessage mes, DiscordUser user, DiscordEmoji emoji = null,
            TimeSpan? timeoutoverride = null)
        {
            if (mes.Discord.GetType() != typeof(DiscordClient))
                throw new InvalidOperationException("Your client is not a default DiscordClient!");

            var client = (DiscordClient)mes.Discord;

            if (client.GetInteractivity() == null)
                throw new NullReferenceException("Your interactivity module has not been initialized for this client!");

            var interactivity = client.GetInteractivity();

            if (emoji != null)
                return await interactivity.WaitForMessageReactionAsync(x => x == emoji, mes, user, timeoutoverride);
            else
                return await interactivity.WaitForMessageReactionAsync(mes, user, timeoutoverride);
        }

        /// <summary>
        /// Waits for any reaction
        /// </summary>
        /// <param name="mes">Message to listen on</param>
        /// <param name="emoji">Emoji to listen for</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
        public static async Task<ReactionContext> WaitForAnyReactionAsync(this DiscordMessage mes, DiscordEmoji emoji = null,
            TimeSpan? timeoutoverride = null)
        {
            if (mes.Discord.GetType() != typeof(DiscordClient))
                throw new InvalidOperationException("Your client is not a default DiscordClient!");

            var client = (DiscordClient)mes.Discord;

            if (client.GetInteractivity() == null)
                throw new NullReferenceException("Your interactivity module has not been initialized for this client!");

            var interactivity = client.GetInteractivity();

            if (emoji != null)
                return await interactivity.WaitForMessageReactionAsync(x => x == emoji, mes, timeoutoverride: timeoutoverride);
            else
                return await interactivity.WaitForMessageReactionAsync(mes, timeoutoverride: timeoutoverride);
        }

        /// <summary>
        /// Makes a poll
        /// </summary>
        /// <param name="mes">Message to make a poll on</param>
        /// <param name="emojis">Emojis to poll</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
        public static async Task<ReactionCollectionContext> MakePoll(this DiscordMessage mes, IEnumerable<DiscordEmoji> emojis,
            TimeSpan? timeoutoverride = null)
        {
            if (mes.Discord.GetType() != typeof(DiscordClient))
                throw new InvalidOperationException("Your client is not a default DiscordClient!");

            var client = (DiscordClient)mes.Discord;

            if (client.GetInteractivity() == null)
                throw new NullReferenceException("Your interactivity module has not been initialized for this client!");

            var interactivity = client.GetInteractivity();

            return await interactivity.CreatePollAsync(mes, emojis, timeoutoverride);
        }

        /// <summary>
        /// Collects all reactions
        /// </summary>
        /// <param name="mes">Message reactions are collected from</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <returns></returns>
        public static async Task<ReactionCollectionContext> CollectReactions(this DiscordMessage mes, TimeSpan? timeoutoverride = null)
        {
            if (mes.Discord.GetType() != typeof(DiscordClient))
                throw new InvalidOperationException("Your client is not a default DiscordClient!");

            var client = (DiscordClient)mes.Discord;

            if (client.GetInteractivity() == null)
                throw new NullReferenceException("Your interactivity module has not been initialized for this client!");

            var interactivity = client.GetInteractivity();

            return await interactivity.CollectReactionsAsync(mes, timeoutoverride);
        }

        /// <summary>
        /// Sends paginated message
        /// </summary>
        /// <param name="chn">Channel to send to</param>
        /// <param name="user">User that is allowed to interact with this paginated message</param>
        /// <param name="pages">Message pages</param>
        /// <param name="emojis">Pagination emojis</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <param name="timeoutbehaviouroverride">Timeout behaviour override</param>
        /// <returns></returns>
        public static async Task SendPaginatedMessage(this DiscordChannel chn, DiscordUser user, IEnumerable<Page> pages,
            PaginationEmojis emojis = null, TimeSpan? timeoutoverride = null, TimeoutBehaviour? timeoutbehaviouroverride = null)
        {
            if (chn.Discord.GetType() != typeof(DiscordClient))
                throw new InvalidOperationException("Your client is not a default DiscordClient!");

            var client = (DiscordClient)chn.Discord;

            if (client.GetInteractivity() == null)
                throw new NullReferenceException("Your interactivity module has not been initialized for this client!");

            var interactivity = client.GetInteractivity();

            await interactivity.SendPaginatedMessage(chn, user, pages, timeoutoverride, timeoutbehaviouroverride, emojis);
        }

        /// <summary>
        /// Responds with a paginated message
        /// </summary>
        /// <param name="mes">Message that is responded to</param>
        /// <param name="user">User that is allowed to interact with this paginated message</param>
        /// <param name="pages">Message pages</param>
        /// <param name="emojis">Pagination emojis</param>
        /// <param name="timeoutoverride">Timeout override</param>
        /// <param name="timeoutbehaviouroverride">Timeout behaviour override</param>
        /// <returns></returns>
        public static async Task RespondPaginated(this DiscordMessage mes, DiscordUser user, IEnumerable<Page> pages,
            PaginationEmojis emojis = null, TimeSpan? timeoutoverride = null, TimeoutBehaviour? timeoutbehaviouroverride = null)
        {
            if (mes.Discord.GetType() != typeof(DiscordClient))
                throw new InvalidOperationException("Your client is not a default DiscordClient!");

            var client = (DiscordClient)mes.Discord;

            if (client.GetInteractivity() == null)
                throw new NullReferenceException("Your interactivity module has not been initialized for this client!");

            var interactivity = client.GetInteractivity();

            await interactivity.SendPaginatedMessage(mes.Channel, user, pages, timeoutoverride, timeoutbehaviouroverride, emojis);
        }
    }
}
