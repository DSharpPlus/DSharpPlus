using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using Emzi0767.Utilities;

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

        private Paginator Paginator;

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
            this.Paginator = new Paginator(this.Client);
        }

        /// <summary>
        /// Makes a poll and returns poll results.
        /// </summary>
        /// <param name="m">Message to create poll on.</param>
        /// <param name="emojis">Emojis to use for this poll.</param>
        /// <param name="behaviour">What to do when the poll ends.</param>
        /// <param name="timeout">override timeout period.</param>
        /// <returns></returns>
        public async Task<ReadOnlyCollection<PollEmoji>> DoPollAsync(DiscordMessage m, DiscordEmoji[] emojis, PollBehaviour? behaviour = default, TimeSpan? timeout = null)
        {
            if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No reaction intents are enabled.");

            if (emojis.Count() < 1)
                throw new ArgumentException("You need to provide at least one emoji for a poll!");

            foreach(var em in emojis)
            {
                await m.CreateReactionAsync(em);
            }
            var res = await Poller.DoPollAsync(new PollRequest(m, timeout ?? this.Config.Timeout, emojis));

            var pollbehaviour = behaviour ?? this.Config.PollBehaviour;
            var thismember = await m.Channel.Guild.GetMemberAsync(Client.CurrentUser.Id);

            if (pollbehaviour == PollBehaviour.DeleteEmojis && m.Channel.PermissionsFor(thismember).HasPermission(Permissions.ManageMessages))
                await m.DeleteAllReactionsAsync();

            return new ReadOnlyCollection<PollEmoji>(res.ToList());
        }

        /// <summary>
        /// Waits for a specific message.
        /// </summary>
        /// <param name="predicate">Predicate to match.</param>
        /// <param name="timeoutoverride">override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<DiscordMessage>> WaitForMessageAsync(Func<DiscordMessage, bool> predicate, 
            TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasMessageIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No message intents are enabled.");

            var timeout = timeoutoverride ?? Config.Timeout;
            var returns = await this.MessageCreatedWaiter.WaitForMatch(new MatchRequest<MessageCreateEventArgs>(x => predicate(x.Message), timeout));

            return new InteractivityResult<DiscordMessage>(returns == null, returns?.Message);
        }

        /// <summary>
        /// Wait for a specific reaction.
        /// </summary>
        /// <param name="predicate">Predicate to match.</param>
        /// <param name="timeoutoverride">override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(Func<MessageReactionAddEventArgs, bool> predicate,
            TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No reaction intents are enabled.");

            var timeout = timeoutoverride ?? Config.Timeout;
            var returns = await this.MessageReactionAddWaiter.WaitForMatch(new MatchRequest<MessageReactionAddEventArgs>(x => predicate(x), timeout));

            return new InteractivityResult<MessageReactionAddEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Wait for a specific reaction.
        /// </summary>
        /// <param name="message">Message reaction was added to.</param>
        /// <param name="user">User that made the reaction.</param>
        /// <param name="timeoutoverride">override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(DiscordMessage message, DiscordUser user,
            TimeSpan? timeoutoverride = null)
            => await WaitForReactionAsync(x => x.User.Id == user.Id && x.Message.Id == message.Id, timeoutoverride);

        /// <summary>
        /// Waits for a specific reaction.
        /// </summary>
        /// <param name="predicate">Predicate to match.</param>
        /// <param name="message">Message reaction was added to.</param>
        /// <param name="user">User that made the reaction.</param>
        /// <param name="timeoutoverride">override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(Func<MessageReactionAddEventArgs, bool> predicate, 
            DiscordMessage message, DiscordUser user, TimeSpan? timeoutoverride = null)
            => await WaitForReactionAsync(x => predicate(x) && x.User.Id == user.Id && x.Message.Id == message.Id, timeoutoverride);

        /// <summary>
        /// Waits for a specific reaction.
        /// </summary>
        /// <param name="predicate">predicate to match.</param>
        /// <param name="user">User that made the reaction.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReactionAsync(Func<MessageReactionAddEventArgs, bool> predicate,
            DiscordUser user, TimeSpan? timeoutoverride = null)
            => await WaitForReactionAsync(x => predicate(x) && x.User.Id == user.Id, timeoutoverride);

        /// <summary>
        /// Waits for a user to start typing.
        /// </summary>
        /// <param name="user">User that starts typing.</param>
        /// <param name="channel">Channel the user is typing in.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(DiscordUser user, 
            DiscordChannel channel, TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No typing intents are enabled.");

            var timeout = timeoutoverride ?? Config.Timeout;
            var returns = await this.TypingStartWaiter.WaitForMatch(
                new MatchRequest<TypingStartEventArgs>(x => x.User.Id == user.Id && x.Channel.Id == channel.Id, timeout));

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Waits for a user to start typing.
        /// </summary>
        /// <param name="user">User that starts typing.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(DiscordUser user, TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No typing intents are enabled.");

            var timeout = timeoutoverride ?? Config.Timeout;
            var returns = await this.TypingStartWaiter.WaitForMatch(
                new MatchRequest<TypingStartEventArgs>(x => x.User.Id == user.Id, timeout));

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Waits for any user to start typing.
        /// </summary>
        /// <param name="channel">Channel to type in.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task<InteractivityResult<TypingStartEventArgs>> WaitForTypingAsync(DiscordChannel channel, TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasTypingIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No typing intents are enabled.");

            var timeout = timeoutoverride ?? Config.Timeout;
            var returns = await this.TypingStartWaiter.WaitForMatch(
                new MatchRequest<TypingStartEventArgs>(x => x.Channel.Id == channel.Id, timeout));

            return new InteractivityResult<TypingStartEventArgs>(returns == null, returns);
        }

        /// <summary>
        /// Collects reactions on a specific message.
        /// </summary>
        /// <param name="m">Message to colelct reactions on.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task<ReadOnlyCollection<Reaction>> CollectReactionsAsync(DiscordMessage m, TimeSpan? timeoutoverride = null)
        {
            if (!Utilities.HasReactionIntents(this.Client.Configuration.Intents))
                throw new InvalidOperationException("No reaction intents are enabled.");

            var timeout = timeoutoverride ?? Config.Timeout;
            var collection = await ReactionCollector.CollectAsync(new ReactionCollectRequest(m, timeout));
            return new ReadOnlyCollection<Reaction>(collection.ToList());
        }

        /// <summary>
        /// Waits for specific event args to be received. Make sure the appropriate <see cref="DiscordIntents"/> are registered, if needed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="timeoutoverride"></param>
        /// <returns></returns>
        public async Task<InteractivityResult<T>> WaitForEventArgsAsync<T>(Func<T, bool> predicate, TimeSpan? timeoutoverride = null) where T : AsyncEventArgs
        {
            var timeout = timeoutoverride ?? this.Config.Timeout;

            using (var waiter = new EventWaiter<T>(this.Client))
            {
                var res = await waiter.WaitForMatch(new MatchRequest<T>(predicate, timeout));
                return new InteractivityResult<T>(res == null, res);
            }
        }

        public async Task<ReadOnlyCollection<T>> CollectEventArgsAsync<T>(Func<T, bool> predicate, TimeSpan? timeoutoverride = null) where T : AsyncEventArgs
        {
            var timeout = timeoutoverride ?? this.Config.Timeout;

            using (var waiter = new EventWaiter<T>(this.Client))
            {
                var res = await waiter.CollectMatches(new CollectRequest<T>(predicate, timeout));
                return res;
            }
        }

        /// <summary>
        /// Sends a paginated message.
        /// </summary>
        /// <param name="c">Channel to send paginated message in.</param>
        /// <param name="u">User to give control.</param>
        /// <param name="pages">Pages.</param>
        /// <param name="emojis">Pagination emojis (emojis set to null get disabled).</param>
        /// <param name="behaviour">Pagination behaviour (when hitting max and min indices).</param>
        /// <param name="deletion">Deletion behaviour.</param>
        /// <param name="timeoutoverride">Override timeout period.</param>
        /// <returns></returns>
        public async Task SendPaginatedMessageAsync(DiscordChannel c, DiscordUser u, IEnumerable<Page> pages, PaginationEmojis emojis = null,
            PaginationBehaviour? behaviour = default, PaginationDeletion? deletion = default, TimeSpan? timeoutoverride = null)
        {
            var m = await c.SendMessageAsync(pages.First().Content, false, pages.First().Embed);
            var timeout = timeoutoverride ?? Config.Timeout;

            var bhv = behaviour ?? this.Config.PaginationBehaviour;
            var del = deletion ?? this.Config.PaginationDeletion;
            var ems = emojis ?? this.Config.PaginationEmojis;

            var prequest = new PaginationRequest(m, u, bhv, del, ems, timeout, pages.ToArray());

            await Paginator.DoPaginationAsync(prequest);
        }

        /// <summary>
        /// Waits for a custom pagination request to finish.
        /// This does NOT handle removing emojis after finishing for you.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task WaitForCustomPaginationAsync(IPaginationRequest request)
        {
            await Paginator.DoPaginationAsync(request);
        }

        /// <summary>
        /// Generates pages from a string, and puts them in message content.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="splittype">How to split input string.</param>
        /// <returns></returns>
        public Page[] GeneratePagesInContent(string input, SplitType splittype = SplitType.Character)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("You must provide a string that is not null or empty!");

            List<Page> result = new List<Page>();
            List<string> split;

            switch (splittype) {
                default:
                case SplitType.Character:
                    split = SplitString(input, 500).ToList();
                    break;
                case SplitType.Line:
                    var subsplit = input.Split('\n');

                    split = new List<string>();
                    string s = "";

                    for(int i = 0; i < subsplit.Length; i++)
                    {
                        s += subsplit[i];
                        if(i % 15 == 0)
                        {
                            split.Add(s);
                            s = "";
                        }
                    }
                    if (!split.Any(x => x == s))
                        split.Add(s);
                    break;
            }

            int page = 1;
            foreach (string s in split)
            {
                result.Add(new Page($"Page {page}:\n{s}", null));
                page++;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Generates pages from a string, and puts them in message embeds.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="splittype">How to split input string.</param>
        /// <param name="embedbase">Base embed for output embeds.</param>
        /// <returns></returns>
        public Page[] GeneratePagesInEmbed(string input, SplitType splittype = SplitType.Character, DiscordEmbedBuilder embedbase = null)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("You must provide a string that is not null or empty!");

            var embed = embedbase ?? new DiscordEmbedBuilder();

            List<Page> result = new List<Page>();
            List<string> split;

            switch (splittype)
            {
                default:
                case SplitType.Character:
                    split = SplitString(input, 500).ToList();
                    break;
                case SplitType.Line:
                    var subsplit = input.Split('\n');

                    split = new List<string>();
                    string s = "";

                    for (int i = 0; i < subsplit.Length; i++)
                    {
                        s += $"{subsplit[i]}\n";
                        if (i % 15 == 0 && i != 0)
                        {
                            split.Add(s);
                            s = "";
                        }
                    }
                    if (!split.Any(x => x == s))
                        split.Add(s);
                    break;
            }

            int page = 1;
            foreach (string s in split)
            {
                result.Add(new Page("", new DiscordEmbedBuilder(embed).WithDescription(s).WithFooter($"Page {page}/{split.Count}")));
                page++;
            }

            return result.ToArray();
        }

        private List<string> SplitString(string str, int chunkSize)
        {
            var res = new List<string>();
            var len = str.Length;
            var i = 0;

            while (i < len)
            {
                var size = Math.Min(len - i, chunkSize);
                res.Add(str.Substring(i, size));
                i += size;
            }

            return res;
        }
    }

    /// <summary>
    /// Way to split strings.
    /// </summary>
    public enum SplitType
    {
        /// <summary>
        /// Splits string per 500 characters.
        /// </summary>
        Character,
        /// <summary>
        /// Splits string per 15 lines.
        /// </summary>
        Line
    }
}
// the one true InteractivityNext