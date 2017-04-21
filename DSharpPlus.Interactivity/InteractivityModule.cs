using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

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
            var ct = new CancellationTokenSource((int)timeout.TotalMilliseconds);
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
            var ct = new CancellationTokenSource((int)timeout.TotalMilliseconds);
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
            var user_id = user.ID;
            var tsc = new TaskCompletionSource<DiscordMessage>();
            var ct = new CancellationTokenSource((int)timeout.TotalMilliseconds);
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
            var message_id = msg.ID;
            var tsc = new TaskCompletionSource<DiscordEmoji>();
            var ct = new CancellationTokenSource((int)timeout.TotalMilliseconds);
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
            var message_id = msg.ID;
            var tsc = new TaskCompletionSource<DiscordEmoji>();
            var ct = new CancellationTokenSource((int)timeout.TotalMilliseconds);
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
            var ct = new CancellationTokenSource((int)timeout.TotalMilliseconds);
            ct.Token.Register(() => tsc.TrySetResult(Reactions));
            _client.MessageReactionAdd += async (e) =>
            {
                await Task.Yield();
                if (e.MessageID == m.ID)
                {
                    if (Reactions.ContainsKey(e.Emoji.Name))
                        Reactions[e.Emoji.Name]++;
                    else
                        Reactions.TryAdd(e.Emoji.Name, 1);
                }
            };

            _client.MessageReactionRemove += async (e) =>
            {
                await Task.Yield();
                if (e.MessageID == m.ID)
                {
                    if (Reactions.ContainsKey(e.Emoji.Name))
                    {
                        Reactions[e.Emoji.Name]--;
                        if (Reactions[e.Emoji.Name] == 0)
                            Reactions.TryRemove(e.Emoji.Name, out int something);
                    }
                }
            };

            _client.MessageReactionRemoveAll += async (e) =>
            {
                await Task.Yield();
                if (e.MessageID == m.ID)
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
            var channel_id = channel.ID;
            var tsc = new TaskCompletionSource<DiscordUser>();
            var ct = new CancellationTokenSource((int)timeout.TotalMilliseconds);
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
            var user_id = user.ID;
            var tsc = new TaskCompletionSource<DiscordChannel>();
            var ct = new CancellationTokenSource((int)timeout.TotalMilliseconds);
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
    }
}
