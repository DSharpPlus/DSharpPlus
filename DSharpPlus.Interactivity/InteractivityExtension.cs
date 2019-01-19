using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.EventHandling;

namespace DSharpPlus.Interactivity
{
    #region Extensions
    public static partial class InteractivityExtensionMethods
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
    }
    #endregion

    /// <summary>
    /// Extension class for DSharpPlus.Interactivity
    /// </summary>
    public class InteractivityExtension : BaseExtension
    {
        private InteractivityConfiguration Config { get; }

        private EventWaiter<MessageCreateEventArgs> MessageCreatedWaiter;

        private Poller Poller;

        internal InteractivityExtension(InteractivityConfiguration cfg)
        {
            this.Config = new InteractivityConfiguration(cfg);
        }

        protected internal override void Setup(DiscordClient client)
        {
            this.Client = client;
            this.MessageCreatedWaiter = new EventWaiter<MessageCreateEventArgs>(this.Client);
            this.Poller = new Poller(this.Client);
        }

        // These methods have placeholder functionality / placeholder names. This is a big WIP.
        public async Task<DiscordMessage> GetMessage(Func<DiscordMessage, bool> predicate)
        {
            var result = await this.MessageCreatedWaiter.WaitForMatch(new MatchRequest<MessageCreateEventArgs>(x => predicate(x.Message), Config.Timeout));
            return result?.Message;
        }

        public async Task<int> CollectMessages(Func<DiscordMessage, bool> predicate, TimeSpan timespan)
        {
            var collected = await this.MessageCreatedWaiter.CollectMatches(new CollectRequest<MessageCreateEventArgs>(x => predicate(x.Message), timespan));
            return collected.Count;
        }

        public async Task<ReadOnlySet<PollEmoji>> WaitPoll(DiscordMessage m, params DiscordEmoji[] emojis)
        {
            foreach(var em in emojis)
            {
                await m.CreateReactionAsync(em);
            }
            var res = await Poller.DoPoll(new PollRequest(m, this.Config.Timeout, emojis));
            return res;
        }
    }
}
// InteractivityNext :^)