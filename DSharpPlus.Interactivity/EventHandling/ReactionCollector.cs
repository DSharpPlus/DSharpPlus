using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.EventHandling
{
    /// <summary>
    /// Eventwaiter is a class that serves as a layer between the InteractivityExtension
    /// and the DiscordClient to listen to an event and check for matches to a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReactionCollector : IDisposable
    {
        DiscordClient _client;

        AsyncEvent<MessageReactionAddEventArgs> _reactionAddEvent;
        AsyncEventHandler<MessageReactionAddEventArgs> _reactionAddHandler;

        AsyncEvent<MessageReactionRemoveEventArgs> _reactionRemoveEvent;
        AsyncEventHandler<MessageReactionRemoveEventArgs> _reactionRemoveHandler;

        AsyncEvent<MessageReactionsClearEventArgs> _reactionClearEvent;
        AsyncEventHandler<MessageReactionsClearEventArgs> _reactionClearHandler;

        ConcurrentHashSet<ReactionCollectRequest> _requests;

        /// <summary>
        /// Creates a new Eventwaiter object.
        /// </summary>
        /// <param name="client">Your DiscordClient</param>
        public ReactionCollector(DiscordClient client)
        {
            this._client = client;
            var tinfo = _client.GetType().GetTypeInfo();

            _requests = new ConcurrentHashSet<ReactionCollectRequest>();

            // Grabbing all three events from client
            var handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<MessageReactionAddEventArgs>));

            this._reactionAddEvent = (AsyncEvent<MessageReactionAddEventArgs>)handler.GetValue(_client);
            this._reactionAddHandler = new AsyncEventHandler<MessageReactionAddEventArgs>(HandleReactionAdd);
            this._reactionAddEvent.Register(_reactionAddHandler);

            handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<MessageReactionRemoveEventArgs>));

            this._reactionRemoveEvent = (AsyncEvent<MessageReactionRemoveEventArgs>)handler.GetValue(_client);
            this._reactionRemoveHandler = new AsyncEventHandler<MessageReactionRemoveEventArgs>(HandleReactionRemove);
            this._reactionRemoveEvent.Register(_reactionRemoveHandler);

            handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<MessageReactionsClearEventArgs>));

            this._reactionClearEvent = (AsyncEvent<MessageReactionsClearEventArgs>)handler.GetValue(_client);
            this._reactionClearHandler = new AsyncEventHandler<MessageReactionsClearEventArgs>(HandleReactionClear);
            this._reactionClearEvent.Register(_reactionClearHandler);
        }

        public async Task<ReadOnlySet<Reaction>> CollectAsync(ReactionCollectRequest request)
        {
            this._requests.Add(request);
            var result = (ReadOnlySet<Reaction>)null;

            try
            {
                await request._tcs.Task;
            }
            catch (Exception ex)
            {
                this._client.DebugLogger.LogMessage(LogLevel.Error, "Interactivity",
                    $"Something went wrong collecting reactions with exception {ex.GetType().Name}.", DateTime.Now);
            }
            finally
            {
                result = new ReadOnlySet<Reaction>(new HashSet<Reaction>(request._collected));
                request.Dispose();
                this._requests.TryRemove(request);
            }
            return result;
        }

        async Task HandleReactionAdd(MessageReactionAddEventArgs eventargs)
        {
            await Task.Yield();
            // foreach request add
            foreach(var req in _requests)
            {
                if (req.message.Id == eventargs.Message.Id)
                {
                    if (req._collected.Any(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id == eventargs.User.Id)))
                    {
                        var reaction = req._collected.First(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id == eventargs.User.Id));
                        req._collected.TryRemove(reaction);
                        reaction.Users.Add(eventargs.User);
                        req._collected.Add(reaction);
                    }
                    else
                    {
                        req._collected.Add(new Reaction()
                        {
                            Emoji = eventargs.Emoji,
                            Users = new ConcurrentHashSet<DiscordUser>() { eventargs.User }
                        });
                    }
                }
            }
        }

        async Task HandleReactionRemove(MessageReactionRemoveEventArgs eventargs)
        {
            await Task.Yield();
            // foreach request remove
            foreach (var req in _requests)
            {
                if (req.message.Id == eventargs.Message.Id)
                {
                    if(req._collected.Any(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id == eventargs.User.Id)))
                    {
                        var reaction = req._collected.First(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id == eventargs.User.Id));
                        req._collected.TryRemove(reaction);
                        reaction.Users.TryRemove(eventargs.User);
                        if (reaction.Users.Count > 0)
                            req._collected.Add(reaction);
                    }
                }
            }
        }

        async Task HandleReactionClear(MessageReactionsClearEventArgs eventargs)
        {
            await Task.Yield();
            // foreach request add
            foreach (var req in _requests)
            {
                if (req.message.Id == eventargs.Message.Id)
                {
                    req._collected.Clear();
                }
            }
        }

        ~ReactionCollector()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this EventWaiter
        /// </summary>
        public void Dispose()
        {
            this._client = null;

            this._reactionAddEvent.Unregister(this._reactionAddHandler);
            this._reactionRemoveEvent.Unregister(this._reactionRemoveHandler);
            this._reactionClearEvent.Unregister(this._reactionClearHandler);

            this._reactionAddEvent = null;
            this._reactionAddHandler = null;
            this._reactionRemoveEvent = null;
            this._reactionRemoveHandler = null;
            this._reactionClearEvent = null;
            this._reactionClearHandler = null;

            _requests.Clear();
            _requests = null;
        }
    }

    public class ReactionCollectRequest : IDisposable
    {
        internal TaskCompletionSource<Reaction> _tcs;
        internal CancellationTokenSource _ct;
        internal TimeSpan _timeout;
        internal DiscordMessage message;
        internal ConcurrentHashSet<Reaction> _collected;

        public ReactionCollectRequest(DiscordMessage msg, TimeSpan timeout)
        {
            message = msg;
            _collected = new ConcurrentHashSet<Reaction>();
            _timeout = timeout;
            _tcs = new TaskCompletionSource<Reaction>();
            _ct = new CancellationTokenSource(_timeout);
            _ct.Token.Register(() => _tcs.TrySetResult(null));
        }

        ~ReactionCollectRequest()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this._ct.Dispose();
            this._tcs = null;
            this.message = null;
            this._collected.Clear();
            this._collected = null;
        }
    }

    public class Reaction
    {
        public DiscordEmoji Emoji { get; internal set; }
        public ConcurrentHashSet<DiscordUser> Users { get; internal set; }
        public int Total { get { return Users.Count; } }
    }
}
