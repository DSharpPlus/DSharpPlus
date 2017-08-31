using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;

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

    public class InteractivityModule : IModule
    {
        #region fields n stuff
        public DiscordClient Client { get { return this._client; } }
        private DiscordClient _client;
        #endregion

        public void Setup(DiscordClient client)
        {
            this._client = client;
        }

        #region Message
        public async Task<MessageContext> WaitForMessageAsync(Func<DiscordMessage, bool> predicate, TimeSpan timeout)
        {
            var tsc = new TaskCompletionSource<MessageContext>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<MessageCreateEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (predicate(e.Message))
                {
                    var mc = new MessageContext()
                    {
                        Interactivity = this,
                        Message = e.Message
                    };
                    tsc.TrySetResult(mc);
                    return;
                }
            };

            _client.MessageCreated += handler;

            var result = await tsc.Task;

            _client.MessageCreated -= handler;
            return result;
        }
        #endregion

        #region Reaction
        public async Task<ReactionContext> WaitForReactionAsync(Func<DiscordEmoji, bool> predicate, TimeSpan timeout)
        {
            var tsc = new TaskCompletionSource<ReactionContext>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));
            AsyncEventHandler<MessageReactionAddEventArgs> handler = async e =>
            {
                await Task.Yield();
                if (predicate(e.Emoji))
                {
                    var rc = new ReactionContext()
                    {
                        Channel = e.Channel,
                        Emoji = e.Emoji,
                        Message = e.Message,
                        User = e.User,
                        Interactivity = this
                    };
                    tsc.TrySetResult(rc);
                    return;
                }
            };

            _client.MessageReactionAdd += handler;

            var result = await tsc.Task;

            _client.MessageReactionAdd -= handler;
            return result;
        }

        public async Task<ReactionContext> WaitForReactionAsync(Func<DiscordEmoji, bool> predicate, DiscordUser user, TimeSpan timeout)
        {
            var user_id = user.Id;
            var tsc = new TaskCompletionSource<ReactionContext>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<MessageReactionAddEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (predicate(e.Emoji))
                {
                    if (e.User.Id == user_id)
                    {
                        var rc = new ReactionContext()
                        {
                            Channel = e.Channel,
                            Emoji = e.Emoji,
                            Message = e.Message,
                            User = e.User,
                            Interactivity = this
                        };
                        tsc.TrySetResult(rc);
                        return;
                    }
                }
            };

            _client.MessageReactionAdd += handler;

            var result = await tsc.Task;

            _client.MessageReactionAdd -= handler;
            return result;
        }

        public async Task<ReactionContext> WaitForMessageReactionAsync(Func<DiscordEmoji, bool> predicate, DiscordMessage msg, TimeSpan timeout, ulong user_id = 0)
        {
            var message_id = msg.Id;
            var tsc = new TaskCompletionSource<ReactionContext>();
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
                            var rc = new ReactionContext()
                            {
                                Channel = e.Channel,
                                Emoji = e.Emoji,
                                Message = e.Message,
                                User = e.User,
                                Interactivity = this
                            };
                            tsc.TrySetResult(rc);
                            return;
                        }
                    }
                }
            };

            _client.MessageReactionAdd += handler;

            var result = await tsc.Task;

            _client.MessageReactionAdd -= handler;
            return result;
        }

        public async Task<ReactionContext> WaitForMessageReactionAsync(DiscordMessage msg, TimeSpan timeout, ulong user_id = 0)
        {
            var message_id = msg.Id;
            var tsc = new TaskCompletionSource<ReactionContext>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<MessageReactionAddEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (e.Message.Id == message_id)
                {
                    if (user_id == 0 || e.User.Id == user_id)
                    {
                        var rc = new ReactionContext()
                        {
                            Channel = e.Channel,
                            Emoji = e.Emoji,
                            Message = e.Message,
                            User = e.User,
                            Interactivity = this
                        };
                        tsc.TrySetResult(rc);
                        return;
                    }
                }
            };

            _client.MessageReactionAdd += handler;

            var result = await tsc.Task;

            _client.MessageReactionAdd -= handler;
            return result;
        }

        public async Task<ReactionCollectionContext> CollectReactionsAsync(DiscordMessage m, TimeSpan timeout)
        {
            var rcc = new ReactionCollectionContext();
            var tsc = new TaskCompletionSource<ReactionCollectionContext>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(rcc));
            AsyncEventHandler<MessageReactionAddEventArgs> handler1 = async (e) =>
            {
                await Task.Yield();
                if (e.Message.Id == m.Id)
                {
                    rcc.AddReaction(e.Emoji);
                }
            };

            _client.MessageReactionAdd += handler1;

            AsyncEventHandler<MessageReactionRemoveEventArgs> handler2 = async (e) =>
            {
                await Task.Yield();
                if (e.Message.Id == m.Id)
                {
                    rcc.RemoveReaction(e.Emoji);
                }
            };

            _client.MessageReactionRemove += handler2;

            AsyncEventHandler<MessageReactionRemoveAllEventArgs> handler3 = async (e) =>
            {
                await Task.Yield();
                if (e.Message.Id == m.Id)
                {
                    rcc.ClearReactions();
                }
            };

            _client.MessageReactionRemoveAll += handler3;

            var result = await tsc.Task;

            _client.MessageReactionAdd -= handler1;
            _client.MessageReactionRemove -= handler2;
            _client.MessageReactionRemoveAll -= handler3;

            return result;
        }
        #endregion

        #region Typing
        // I don't really know anymore why I added this..
        public async Task<TypingContext> WaitForTypingUserAsync(DiscordChannel channel, TimeSpan timeout)
        {
            var channel_id = channel.Id;
            var tsc = new TaskCompletionSource<TypingContext>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<TypingStartEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (e.Channel.Id == channel_id)
                {
                    var tc = new TypingContext()
                    {
                        Channel = e.Channel,
                        Interactivity = this,
                        StartedAt = e.StartedAt,
                        User = e.User
                    };
                    tsc.TrySetResult(tc);
                    return;
                }
            };

            _client.TypingStart += handler;

            var result = await tsc.Task;

            _client.TypingStart -= handler;
            return result;
        }

        public async Task<TypingContext> WaitForTypingChannelAsync(DiscordUser user, TimeSpan timeout)
        {
            var user_id = user.Id;
            var tsc = new TaskCompletionSource<TypingContext>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            AsyncEventHandler<TypingStartEventArgs> handler = async (e) =>
            {
                await Task.Yield();
                if (e.User.Id == user_id)
                {
                    var tc = new TypingContext()
                    {
                        Channel = e.Channel,
                        Interactivity = this,
                        StartedAt = e.StartedAt,
                        User = e.User
                    };
                    tsc.TrySetResult(tc);
                    return;
                }
            };

            _client.TypingStart += handler;

            var result = await tsc.Task;

            _client.TypingStart -= handler;
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

            DiscordMessage m = await _client.SendMessageAsync(channel, string.IsNullOrEmpty(pages.First().Content) ? "" : pages.First().Content, embed: pages.First().Embed);
            PaginatedMessage pm = new PaginatedMessage()
            {
                CurrentIndex = 0,
                Pages = pages,
                Timeout = timeout
            };

            await this.GeneratePaginationReactions(m);

            AsyncEventHandler<MessageReactionRemoveAllEventArgs> _reaction_removed_all = async e =>
            {
                await this.GeneratePaginationReactions(m);
            };
            _client.MessageReactionRemoveAll += _reaction_removed_all;

            AsyncEventHandler<MessageReactionAddEventArgs> _reaction_added = async e =>
            {
                if (e.Message.Id == m.Id && e.User.Id != _client.CurrentUser.Id && e.User.Id == user.Id)
                {
                    await this.DoPagination(e.Emoji, m, pm, ct);
                }
            };
            _client.MessageReactionAdd += _reaction_added;

            AsyncEventHandler<MessageReactionRemoveEventArgs> _reaction_removed = async e =>
            {
                if (e.Message.Id == m.Id && e.User.Id != _client.CurrentUser.Id && e.User.Id == user.Id)
                    await this.DoPagination(e.Emoji, m, pm, ct);
            };
            _client.MessageReactionRemove += _reaction_removed;

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

            _client.MessageReactionRemoveAll -= _reaction_removed_all;
            _client.MessageReactionAdd -= _reaction_added;
            _client.MessageReactionRemove -= _reaction_removed;
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

            await m.EditAsync((string.IsNullOrEmpty(pm.Pages.ToArray()[pm.CurrentIndex].Content)) ? "" : pm.Pages.ToArray()[pm.CurrentIndex].Content,
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