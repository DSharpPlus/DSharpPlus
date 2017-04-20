using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity
{
    #region Extension stuff
    public static class InteractivityExtension
    {
        internal static InteractivityModule _interactivity_module;

        public static InteractivityModule UseInteractivity(this DiscordClient c)
        {
            if (_interactivity_module != null)
                throw new Exception("Interactivity module is already set!");

            _interactivity_module = new InteractivityModule();
            _interactivity_module.Setup(c);
            return _interactivity_module;
        }

        public static InteractivityModule GetInteractivityModule(this DiscordClient c)
        {
            if (_interactivity_module == null)
                throw new Exception("Interactivity module is not set!");

            return _interactivity_module;
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

        public async Task<DiscordMessage> WaitForReactionAsync(Func<DiscordEmoji, bool> predicate, ulong user_id, TimeSpan timeout)
        {
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


        public async Task<DiscordEmoji> WaitForMessageReactionAsync(Func<DiscordEmoji, bool> predicate, ulong message_id, TimeSpan timeout)
        {
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

        public async Task<DiscordEmoji> WaitForMessageReactionAsync(ulong message_id, TimeSpan timeout)
        {
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
        public async Task<DiscordUser> WaitForTypingUserAsync(ulong channel_id, TimeSpan timeout)
        {
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

        public async Task<DiscordChannel> WaitForTypingChannelAsync(ulong user_id, TimeSpan timeout)
        {
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
