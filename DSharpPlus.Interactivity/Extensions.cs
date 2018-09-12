using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.Extensions
{
    public static class ExtensionMethods
    {
        public static InteractivityExtension UseInteractivity(this DiscordClient c, InteractivityConfiguration cfg)
        {
            if (c.GetExtension<InteractivityExtension>() != null)
                throw new Exception("Interactivity module is already enabled for this client!");

            var m = new InteractivityExtension(cfg);
            c.AddExtension(m);
            return m;
        }

        public static async Task<IReadOnlyDictionary<int, InteractivityExtension>> UseInteractivityAsync(this DiscordShardedClient c, InteractivityConfiguration cfg)
        {
            var modules = new Dictionary<int, InteractivityExtension>();
            await c.InitializeShardsAsync().ConfigureAwait(false);

            foreach (var shard in c.ShardClients.Select(xkvp => xkvp.Value))
            {
                var m = shard.GetExtension<InteractivityExtension>();
                if (m == null)
                    m = shard.UseInteractivity(cfg);

                modules[shard.ShardId] = m;
            }

            return new ReadOnlyDictionary<int, InteractivityExtension>(modules);
        }

        public static InteractivityExtension GetInteractivity(this DiscordClient c)
        {
            return c.GetExtension<InteractivityExtension>();
        }

        public static IReadOnlyDictionary<int, InteractivityExtension> GetInteractivity(this DiscordShardedClient c)
        {
            var modules = new Dictionary<int, InteractivityExtension>();

            c.InitializeShardsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            foreach (var shard in c.ShardClients.Select(xkvp => xkvp.Value))
                modules.Add(shard.ShardId, shard.GetExtension<InteractivityExtension>());

            return new ReadOnlyDictionary<int, InteractivityExtension>(modules);
        }

        public static IEnumerable<string> Split(this string str, int chunkSize)
        {
            var len = str.Length;
            var i = 0;

            while (i < len)
            {
                var size = Math.Min(len - i, chunkSize);
                yield return str.Substring(i, size);
                i += size;
            }
        }

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
