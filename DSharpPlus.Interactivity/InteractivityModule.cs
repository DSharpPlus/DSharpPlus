using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity
{
    #region Extension stuff
    public static class InteractivityExtension
    {
        public static InteractivityModule UseInteractivity(this DiscordClient c)
        {
            if (c.GetModule<InteractivityModule>() != null)
                throw new Exception("Interactivity module is already enabled for this client!");

            var m = new InteractivityModule();
            c.AddModule(m);
            return m;
        }

        public static IReadOnlyDictionary<int, InteractivityModule> UseInteractivity(this DiscordShardedClient c)
        {
            var modules = new Dictionary<int, InteractivityModule>();

            c.InitializeShardsAsync().GetAwaiter().GetResult();

            foreach (var shard in c.ShardClients.Select(xkvp => xkvp.Value))
            {
                var m = shard.GetModule<InteractivityModule>();
                if (m == null)
                    m = shard.UseInteractivity();

                modules.Add(shard.ShardId, m);
            }

            return new ReadOnlyDictionary<int, InteractivityModule>(modules);
        }

        public static InteractivityModule GetInteractivityModule(this DiscordClient c)
        {
            return c.GetModule<InteractivityModule>();
        }

        public static IReadOnlyDictionary<int, InteractivityModule> GetInteractivityModule(this DiscordShardedClient c)
        {
            var modules = new Dictionary<int, InteractivityModule>();

            c.InitializeShardsAsync().GetAwaiter().GetResult();

            foreach (var shard in c.ShardClients.Select(xkvp => xkvp.Value))
                modules.Add(shard.ShardId, shard.GetModule<InteractivityModule>());

            return new ReadOnlyDictionary<int, InteractivityModule>(modules);
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
    }
    #endregion

    public class InteractivityModule : BaseModule
    {
        /// <summary>
        /// DO NOT USE THIS MANUALLY.
        /// </summary>
        /// <param name="client">DO NOT USE THIS MANUALLY.</param>
        /// <exception cref="InvalidOperationException"/>
        protected internal override void Setup(DiscordClient client)
        {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = client;
        }

        #region Message
        public async Task<DiscordMessage> WaitForMessageAsync(Func<DiscordMessage, bool> predicate, TimeSpan timeout)
        {
            var tsc = new TaskCompletionSource<DiscordMessage>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<MessageCreateEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (predicate(e.Message))
                {
                    tsc.TrySetResult(e.Message);
                    return;
                }
            };

            this.Client.MessageCreated += handler;

            DiscordMessage result = await tsc.Task;

            this.Client.MessageCreated -= handler;
            return result;
        }
        #endregion

        #region Reaction
        public async Task<DiscordMessage> WaitForReactionAsync(Func<DiscordEmoji, bool> predicate, TimeSpan timeout)
        {
            var tsc = new TaskCompletionSource<DiscordMessage>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));
            AsyncEventHandler<MessageReactionAddEventArgs> handler = async e =>
            {
                await Task.Yield();
                if (predicate(e.Emoji))
                {
                    tsc.TrySetResult(e.Message);
                    return;
                }
            };

            this.Client.MessageReactionAdded += handler;

            DiscordMessage result = await tsc.Task;

            this.Client.MessageReactionAdded -= handler;
            return result;
        }

        public async Task<DiscordMessage> WaitForReactionAsync(Func<DiscordEmoji, bool> predicate, DiscordUser user, TimeSpan timeout)
        {
            var user_id = user.Id;
            var tsc = new TaskCompletionSource<DiscordMessage>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<MessageReactionAddEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (predicate(e.Emoji))
                {
                    if (e.User.Id == user_id)
                    {
                        tsc.TrySetResult(e.Message);
                        return;
                    }
                }
            };

            this.Client.MessageReactionAdded += handler;

            DiscordMessage result = await tsc.Task;

            this.Client.MessageReactionAdded -= handler;
            return result;
        }

        public async Task<DiscordEmoji> WaitForMessageReactionAsync(Func<DiscordEmoji, bool> predicate, DiscordMessage msg, TimeSpan timeout, ulong user_id = 0)
        {
            var message_id = msg.Id;
            var tsc = new TaskCompletionSource<DiscordEmoji>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<MessageReactionAddEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (predicate(e.Emoji))
                {
                    if (e.Message.Id == message_id)
                    {
                        if (user_id == 0 || e.User.Id == user_id)
                        {
                            tsc.TrySetResult(e.Emoji);
                            return;
                        }
                    }
                }
            };

            this.Client.MessageReactionAdded += handler;

            DiscordEmoji result = await tsc.Task;

            this.Client.MessageReactionAdded -= handler;
            return result;
        }

        public async Task<DiscordEmoji> WaitForMessageReactionAsync(DiscordMessage msg, TimeSpan timeout, ulong user_id = 0)
        {
            var message_id = msg.Id;
            var tsc = new TaskCompletionSource<DiscordEmoji>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<MessageReactionAddEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (e.Message.Id == message_id)
                {
                    if (user_id == 0 || e.User.Id == user_id)
                    {
                        tsc.TrySetResult(e.Emoji);
                        return;
                    }
                }
            };

            this.Client.MessageReactionAdded += handler;

            DiscordEmoji result = await tsc.Task;

            this.Client.MessageReactionAdded -= handler;
            return result;
        }

        public async Task<ConcurrentDictionary<DiscordEmoji, int>> CollectReactionsAsync(DiscordMessage m, TimeSpan timeout)
        {
            ConcurrentDictionary<DiscordEmoji, int> Reactions = new ConcurrentDictionary<DiscordEmoji, int>();
            var tsc = new TaskCompletionSource<ConcurrentDictionary<DiscordEmoji, int>>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(Reactions));
            AsyncEventHandler<MessageReactionAddEventArgs> handler1 = async (e) =>
            {
                await Task.Yield();
                if (e.Message.Id == m.Id)
                {
                    if (Reactions.ContainsKey(e.Emoji))
                        Reactions[e.Emoji]++;
                    else
                        Reactions.TryAdd(e.Emoji, 1);
                }
            };

            this.Client.MessageReactionAdded += handler1;

            AsyncEventHandler<MessageReactionRemoveEventArgs> handler2 = async (e) =>
            {
                await Task.Yield();
                if (e.Message.Id == m.Id)
                {
                    if (Reactions.ContainsKey(e.Emoji))
                    {
                        Reactions[e.Emoji]--;
                        if (Reactions[e.Emoji] == 0)
                            Reactions.TryRemove(e.Emoji, out int something);
                    }
                }
            };

            this.Client.MessageReactionRemoved += handler2;

            AsyncEventHandler<MessageReactionsClearEventArgs> handler3 = async (e) =>
            {
                await Task.Yield();
                if (e.Message.Id == m.Id)
                {
                    Reactions = new ConcurrentDictionary<DiscordEmoji, int>();
                }
            };

            this.Client.MessageReactionsCleared += handler3;

            var result = await tsc.Task;

            this.Client.MessageReactionAdded -= handler1;
            this.Client.MessageReactionRemoved -= handler2;
            this.Client.MessageReactionsCleared -= handler3;

            return result;
        }
        #endregion

        #region Typing
        public async Task<DiscordUser> WaitForTypingUserAsync(DiscordChannel channel, TimeSpan timeout)
        {
            var channel_id = channel.Id;
            var tsc = new TaskCompletionSource<DiscordUser>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<TypingStartEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (e.Channel.Id == channel_id)
                {
                    tsc.TrySetResult(e.User);
                    return;
                }
            };

            this.Client.TypingStarted += handler;

            DiscordUser result = await tsc.Task;

            this.Client.TypingStarted -= handler;
            return result;
        }

        public async Task<DiscordChannel> WaitForTypingChannelAsync(DiscordUser user, TimeSpan timeout)
        {
            var user_id = user.Id;
            var tsc = new TaskCompletionSource<DiscordChannel>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<TypingStartEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (e.User.Id == user_id)
                {
                    tsc.TrySetResult(e.Channel);
                    return;
                }
            };

            this.Client.TypingStarted += handler;

            DiscordChannel result = await tsc.Task;

            this.Client.TypingStarted -= handler;
            return result;
        }
        #endregion

        #region Pagination
        public async Task SendPaginatedMessage(DiscordChannel channel, DiscordUser user, IEnumerable<Page> message_pages, TimeSpan timeout, TimeoutBehaviour timeout_behaviour)
        {
            List<Page> pages = message_pages.ToList();

            if (pages.Count() == 0)
                throw new ArgumentException("You need to provide at least 1 page!");

            var tsc = new TaskCompletionSource<string>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            DiscordMessage m = await this.Client.SendMessageAsync(channel, string.IsNullOrEmpty(pages.First().Content) ? "" : pages.First().Content, embed: pages.First().Embed);
            PaginatedMessage pm = new PaginatedMessage()
            {
                CurrentIndex = 0,
                Pages = pages,
                Timeout = timeout
            };

            await this.GeneratePaginationReactions(m);

            AsyncEventHandler<MessageReactionsClearEventArgs> _reaction_removed_all = async e =>
            {
                await this.GeneratePaginationReactions(m);
            };
            this.Client.MessageReactionsCleared += _reaction_removed_all;

            AsyncEventHandler<MessageReactionAddEventArgs> _reaction_added = async e =>
            {
                if (e.Message.Id == m.Id && e.User.Id != this.Client.CurrentUser.Id && e.User.Id == user.Id)
                {
                    await this.DoPagination(e.Emoji, m, pm, ct);
                }
            };
            this.Client.MessageReactionAdded += _reaction_added;

            AsyncEventHandler<MessageReactionRemoveEventArgs> _reaction_removed = async e =>
            {
                if (e.Message.Id == m.Id && e.User.Id != this.Client.CurrentUser.Id && e.User.Id == user.Id)
                    await this.DoPagination(e.Emoji, m, pm, ct);
            };
            this.Client.MessageReactionRemoved += _reaction_removed;

            await tsc.Task;

            switch (timeout_behaviour)
            {
                case TimeoutBehaviour.Default:
                case TimeoutBehaviour.Ignore:
                    await m.DeleteAllReactionsAsync();
                    break;
                case TimeoutBehaviour.Delete:
                    await m.DeleteAllReactionsAsync();
                    await m.DeleteAsync();
                    break;
            }

            this.Client.MessageReactionsCleared -= _reaction_removed_all;
            this.Client.MessageReactionAdded -= _reaction_added;
            this.Client.MessageReactionRemoved -= _reaction_removed;
        }

        public IEnumerable<Page> GeneratePagesInEmbeds(string input)
        {
            List<Page> result = new List<Page>();
            List<string> split = input.Split(2000).ToList();
            foreach (string s in split)
            {
                result.Add(new Page()
                {
                    Embed = new DiscordEmbed()
                    {
                        Description = s
                    }
                });
            }
            return result;
        }

        public IEnumerable<Page> GeneratePagesInStrings(string input)
        {
            List<Page> result = new List<Page>();
            List<string> split = input.Split(2000).ToList();
            foreach (string s in split)
            {
                result.Add(new Page()
                {
                    Content = s
                });
            }
            return result;
        }

        public async Task GeneratePaginationReactions(DiscordMessage m)
        {
            await m.CreateReactionAsync(DiscordEmoji.FromUnicode(this.Client, "⏮"));
            await m.CreateReactionAsync(DiscordEmoji.FromUnicode(this.Client, "◀"));
            await m.CreateReactionAsync(DiscordEmoji.FromUnicode(this.Client, "⏹"));
            await m.CreateReactionAsync(DiscordEmoji.FromUnicode(this.Client, "▶"));
            await m.CreateReactionAsync(DiscordEmoji.FromUnicode(this.Client, "⏭"));
        }

        public async Task DoPagination(DiscordEmoji emoji, DiscordMessage m, PaginatedMessage pm, CancellationTokenSource ct)
        {
            #region The "good" shit
            switch (emoji.Name)
            {
                case "⏮":
                    pm.CurrentIndex = 0;
                    break;

                case "◀":
                    if (pm.CurrentIndex != 0)
                        pm.CurrentIndex--;
                    break;

                case "⏹":
                    ct.Cancel();
                    return;

                case "▶":
                    if (pm.CurrentIndex != pm.Pages.Count() - 1)
                        pm.CurrentIndex++;
                    break;

                case "⏭":
                    pm.CurrentIndex = pm.Pages.Count() - 1;
                    break;

                default:
                    return;
            }

            await m.ModifyAsync((string.IsNullOrEmpty(pm.Pages.ToArray()[pm.CurrentIndex].Content)) ? "" : pm.Pages.ToArray()[pm.CurrentIndex].Content,
                embed: pm.Pages.ToArray()[pm.CurrentIndex].Embed ?? null);
            #endregion
        }
        #endregion
    }

    public enum TimeoutBehaviour
    {
        Default, // ignore
        Ignore,
        Delete
    }

    public class PaginatedMessage
    {
        public IEnumerable<Page> Pages { get; internal set; }
        public int CurrentIndex { get; internal set; }
        public TimeSpan Timeout { get; internal set; }
    }

    public class Page
    {
        public string Content { get; set; }
        public DiscordEmbed Embed { get; set; }
    }

}
// send nudes

// wait don't im not 18 yet

// I mean I don't mind..