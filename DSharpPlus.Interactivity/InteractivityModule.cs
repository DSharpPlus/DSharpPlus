using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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

        public static InteractivityModule GetInteractivityModule(this DiscordClient c)
        {
            var m = c.GetModule<InteractivityModule>();
            if (m == null)
                throw new Exception("Interactivity module is not enabled for this client!");

            return m;
        }

        public static IEnumerable<string> Split(this string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
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
            // hook events 'n shit
        }

        #region Message
        public async Task<DiscordMessage> WaitForMessageAsync(Func<DiscordMessage, bool> predicate, TimeSpan timeout)
        {
            var tsc = new TaskCompletionSource<DiscordMessage>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            _client.MessageCreated += async (e) =>
            {
                await Task.Yield();
                if (predicate(e.Message))
                {
                    tsc.TrySetResult(e.Message);
                    return;
                }
            };

            DiscordMessage result = await tsc.Task;
            return result;
        }
        #endregion

        #region Reaction
        public async Task<DiscordMessage> WaitForReactionAsync(Func<DiscordEmoji, bool> predicate, TimeSpan timeout)
        {
            var tsc = new TaskCompletionSource<DiscordMessage>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            _client.MessageReactionAdd += async (e) =>
            {
                await Task.Yield();
                if (predicate(e.Emoji))
                {
                    tsc.TrySetResult(e.Message);
                    return;
                }
            };

            DiscordMessage result = await tsc.Task;
            return result;
        }

        public async Task<DiscordMessage> WaitForReactionAsync(Func<DiscordEmoji, bool> predicate, DiscordUser user, TimeSpan timeout)
        {
            var user_id = user.Id;
            var tsc = new TaskCompletionSource<DiscordMessage>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            _client.MessageReactionAdd += async (e) =>
            {
                await Task.Yield();
                if (predicate(e.Emoji))
                {
                    if (e.UserID == user_id)
                    {
                        tsc.TrySetResult(e.Message);
                        return;
                    }
                }
            };

            DiscordMessage result = await tsc.Task;
            return result;
        }


        public async Task<DiscordEmoji> WaitForMessageReactionAsync(Func<DiscordEmoji, bool> predicate, DiscordMessage msg, TimeSpan timeout)
        {
            var message_id = msg.Id;
            var tsc = new TaskCompletionSource<DiscordEmoji>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            _client.MessageReactionAdd += async (e) =>
            {
                await Task.Yield();
                if (predicate(e.Emoji))
                {
                    if (e.MessageID == message_id)
                    {
                        tsc.TrySetResult(e.Emoji);
                        return;
                    }
                }
            };

            DiscordEmoji result = await tsc.Task;
            return result;
        }

        public async Task<DiscordEmoji> WaitForMessageReactionAsync(DiscordMessage msg, TimeSpan timeout)
        {
            var message_id = msg.Id;
            var tsc = new TaskCompletionSource<DiscordEmoji>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            _client.MessageReactionAdd += async (e) =>
            {
                await Task.Yield();
                if (e.MessageID == message_id)
                {
                    tsc.TrySetResult(e.Emoji);
                    return;
                }
            };

            DiscordEmoji result = await tsc.Task;
            return result;
        }

        public async Task<ConcurrentDictionary<string, int>> CollectReactionsAsync(DiscordMessage m, TimeSpan timeout)
        {
            ConcurrentDictionary<string, int> Reactions = new ConcurrentDictionary<string, int>();
            var tsc = new TaskCompletionSource<ConcurrentDictionary<string, int>>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(Reactions));
            _client.MessageReactionAdd += async (e) =>
            {
                await Task.Yield();
                if (e.MessageID == m.Id)
                {
                    if (Reactions.ContainsKey(e.Emoji.ToString()))
                        Reactions[e.Emoji.ToString()]++;
                    else
                        Reactions.TryAdd(e.Emoji.ToString(), 1);
                }
            };

            _client.MessageReactionRemove += async (e) =>
            {
                await Task.Yield();
                if (e.MessageID == m.Id)
                {
                    if (Reactions.ContainsKey(e.Emoji.ToString()))
                    {
                        Reactions[e.Emoji.ToString()]--;
                        if (Reactions[e.Emoji.ToString()] == 0)
                            Reactions.TryRemove(e.Emoji.ToString(), out int something);
                    }
                }
            };

            _client.MessageReactionRemoveAll += async (e) =>
            {
                await Task.Yield();
                if (e.MessageID == m.Id)
                {
                    Reactions = new ConcurrentDictionary<string, int>();
                }
            };

            return await tsc.Task;
        }
        #endregion

        #region Typing
        public async Task<DiscordUser> WaitForTypingUserAsync(DiscordChannel channel, TimeSpan timeout)
        {
            var channel_id = channel.Id;
            var tsc = new TaskCompletionSource<DiscordUser>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            _client.TypingStart += async (e) =>
            {
                await Task.Yield();
                if (e.ChannelID == channel_id)
                {
                    tsc.TrySetResult(e.User);
                    return;
                }
            };

            DiscordUser result = await tsc.Task;
            return result;
        }

        public async Task<DiscordChannel> WaitForTypingChannelAsync(DiscordUser user, TimeSpan timeout)
        {
            var user_id = user.Id;
            var tsc = new TaskCompletionSource<DiscordChannel>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            _client.TypingStart += async (e) =>
            {
                await Task.Yield();
                if (e.UserID == user_id)
                {
                    tsc.TrySetResult(e.Channel);
                    return;
                }
            };

            DiscordChannel result = await tsc.Task;
            return result;
        }
        #endregion

        // ⏮ ◀ ⏹ (🔢) ▶ ⏭
        public async Task SendPaginatedMessage(DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, TimeSpan timeout, TimeoutBehaviour timeout_behaviour)
        {
            if (pages.Count() == 0)
                throw new ArgumentException("You need to provide at least 1 page!");

            var tsc = new TaskCompletionSource<string>();
            var ct = new CancellationTokenSource(timeout);
            ct.Token.Register(() => tsc.TrySetResult(null));

            DiscordMessage m = await _client.SendMessageAsync(channel.Id, string.IsNullOrEmpty(pages.First().Content) ? "" : pages.First().Content, embed: pages.First().Embed);

            PaginatedMessage pm = new PaginatedMessage()
            {
                CurrentIndex = 0,
                Pages = pages,
                Timeout = timeout
            };

            #region Ugh, adding reactions
            await m.CreateReactionAsync("⏮");
            await Task.Delay(500);
            await m.CreateReactionAsync("◀");
            await Task.Delay(500);
            await m.CreateReactionAsync("⏹");
            //await Task.Delay(500);
            //await m.CreateReaction("🔢");
            await Task.Delay(500);
            await m.CreateReactionAsync("▶");
            await Task.Delay(500);
            await m.CreateReactionAsync("⏭");
            #endregion

            _client.MessageReactionRemoveAll += async e =>
            {
                #region Ugh, adding reactions back
                await m.CreateReactionAsync("⏮");
                await Task.Delay(500);
                await m.CreateReactionAsync("◀");
                await Task.Delay(500);
                await m.CreateReactionAsync("⏹");
                //await Task.Delay(500);
                //await m.CreateReaction("🔢");
                await Task.Delay(500);
                await m.CreateReactionAsync("▶");
                await Task.Delay(500);
                await m.CreateReactionAsync("⏭");
                #endregion
            };

            _client.MessageReactionAdd += async e =>
            {
                if (e.MessageID == m.Id)
                {
                    if (e.UserID != _client.Me.Id)
                    {
                        if (e.Emoji.Id == 0)
                            await m.DeleteReactionAsync(e.Emoji.Name, e.UserID);
                        else
                            await m.DeleteReactionAsync(e.Emoji.Name + ":" + e.Emoji.Id, e.UserID);

                        if (e.UserID == user.Id)
                        {
                            #region The "good" shit
                            switch (e.Emoji.Name)
                            {
                                default:
                                    break;
                                case "⏮":
                                    pm.CurrentIndex = 0;
                                    break;
                                case "◀":
                                    if (pm.CurrentIndex != 0)
                                        pm.CurrentIndex--;
                                    break;
                                case "⏹":
                                    ct.Cancel();
                                    break;
                                /*
                            case "🔢":
                                var m1 = await e.Channel.SendMessage("Enter page number..");
                                var m2 = await WaitForMessageAsync(x => x.ChannelID == channel_id && x.Author.ID == user_id, TimeSpan.FromSeconds(10));
                                int i = int.Parse(m2.Content);
                                if (i < pm.Pages.Count() - 1)
                                    pm.CurrentIndex = i;
                                break;
                                */
                                case "▶":
                                    if (pm.CurrentIndex != pm.Pages.Count() - 1)
                                        pm.CurrentIndex++;
                                    break;
                                case "⏭":
                                    pm.CurrentIndex = pm.Pages.Count() - 1;
                                    break;
                            }

                            await m.EditAsync((string.IsNullOrEmpty(pm.Pages.ToArray()[pm.CurrentIndex].Content)) ? "" : pm.Pages.ToArray()[pm.CurrentIndex].Content,
                                embed: pm.Pages.ToArray()[pm.CurrentIndex].Embed ?? new DiscordEmbed());
                            #endregion
                        }
                    }
                }
            };

            await tsc.Task;
            switch (timeout_behaviour)
            {
                case TimeoutBehaviour.Default:
                case TimeoutBehaviour.Ignore:
                    break;
                case TimeoutBehaviour.Delete:
                    await m.DeleteAsync();
                    break;
            }
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