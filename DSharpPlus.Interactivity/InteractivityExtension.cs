using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
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
        internal InteractivityConfiguration Config { get; }

        private EventWaiter<MessageCreateEventArgs> MessageCreatedWaiter;

        private EventWaiter<MessageReactionAddEventArgs> MessageReactionAddWaiter;

        private EventWaiter<TypingStartEventArgs> TypingStartWaiter;

        private ReactionCollector ReactionCollector;

        private Poller Poller;

        internal InteractivityExtension(InteractivityConfiguration cfg)
        {
            this.Config = new InteractivityConfiguration(cfg);
        }

        protected internal override void Setup(DiscordClient client)
        {
            this.Client = client;
            this.MessageCreatedWaiter = new EventWaiter<MessageCreateEventArgs>(this.Client);
            this.MessageReactionAddWaiter = new EventWaiter<MessageReactionAddEventArgs>(this.Client);
            this.TypingStartWaiter = new EventWaiter<TypingStartEventArgs>(this.Client);
            this.Poller = new Poller(this.Client);
            this.ReactionCollector = new ReactionCollector(this.Client);
        }

        public async Task<ReadOnlySet<PollEmoji>> WaitPollAsync(DiscordMessage m, DiscordEmoji[] emojis, PollBehaviour behaviour = PollBehaviour.Default, TimeSpan? timeout = null)
        {
            if (emojis.Count() < 1)
                throw new ArgumentException("You need to provide at least one emoji for a poll!");

            foreach(var em in emojis)
            {
                await m.CreateReactionAsync(em);
            }
            var res = await Poller.DoPollAsync(new PollRequest(m, timeout ?? this.Config.Timeout, emojis));

            var pollbehaviour = behaviour == PollBehaviour.Default ? this.Config.PollBehaviour : behaviour;
            var thismember = await m.Channel.Guild.GetMemberAsync(Client.CurrentUser.Id);

            if (pollbehaviour == PollBehaviour.DeleteEmojis && m.Channel.PermissionsFor(thismember).HasPermission(Permissions.ManageMessages))
                await m.DeleteAllReactionsAsync();

            return res;
        }

        public async Task<InteractivityResult<DiscordMessage>> WaitForMessageAsync(Func<DiscordMessage, bool> predicate, 
            TimeSpan? timeoutoverride = null)
        {
            var timeout = timeoutoverride ?? Config.Timeout;
            var returns = await this.MessageCreatedWaiter.WaitForMatch(new MatchRequest<MessageCreateEventArgs>(x => predicate(x.Message), timeout));

            return new InteractivityResult<DiscordMessage>(returns == null, returns?.Message);
        }

        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(Func<MessageReactionAddEventArgs, bool> predicate,
            TimeSpan? timeoutoverride = null)
        {
            var timeout = timeoutoverride ?? Config.Timeout;
            var returns = await this.MessageReactionAddWaiter.WaitForMatch(new MatchRequest<MessageReactionAddEventArgs>(x => predicate(x), timeout));

            return new InteractivityResult<MessageReactionAddEventArgs>(returns == null, returns);
        }

        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(DiscordUser user, 
            DiscordChannel channel, TimeSpan? timeoutoverride = null)
        {
            var timeout = timeoutoverride ?? Config.Timeout;
            var returns = await this.TypingStartWaiter.WaitForMatch(
                new MatchRequest<TypingStartEventArgs>(x => x.User.Id == user.Id && x.Channel.Id == channel.Id, timeout));

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForTypingAsync(DiscordChannel channel, TimeSpan? timeoutoverride = null)
        {
            var timeout = timeoutoverride ?? Config.Timeout;
            var returns = await this.TypingStartWaiter.WaitForMatch(
                new MatchRequest<TypingStartEventArgs>(x => x.Channel.Id == channel.Id, timeout));

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        public async Task<ReadOnlySet<Reaction>> CollectReactionsAsync(DiscordMessage m, PollBehaviour behaviour = PollBehaviour.Default, TimeSpan? timeoutoverride = null)
        {
            var timeout = timeoutoverride ?? Config.Timeout;
            var collection = await ReactionCollector.CollectAsync(new ReactionCollectRequest(m, timeout));
            return collection;
        }

        // TODO
        public async Task DoPaginationAsync()
        {
            await Task.Yield(); // warning be gone!!1
        }
    }
}
// the one true InteractivityNext