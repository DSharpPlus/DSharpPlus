namespace DSharpPlus.Interactivity.EventHandling;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

/// <summary>
/// Eventwaiter is a class that serves as a layer between the InteractivityExtension
/// and the DiscordClient to listen to an event and check for matches to a predicate.
/// </summary>
internal class ReactionCollector : IDisposable
{
    private DiscordClient _client;
    private AsyncEvent<DiscordClient, MessageReactionAddEventArgs> _reactionAddEvent;
    private AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs> _reactionAddHandler;
    private AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs> _reactionRemoveEvent;
    private AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs> _reactionRemoveHandler;
    private AsyncEvent<DiscordClient, MessageReactionsClearEventArgs> _reactionClearEvent;
    private AsyncEventHandler<DiscordClient, MessageReactionsClearEventArgs> _reactionClearHandler;
    private ConcurrentHashSet<ReactionCollectRequest> _requests;

    /// <summary>
    /// Creates a new Eventwaiter object.
    /// </summary>
    /// <param name="client">Your DiscordClient</param>
    public ReactionCollector(DiscordClient client)
    {
        _client = client;
        TypeInfo tinfo = _client.GetType().GetTypeInfo();

        _requests = [];

        // Grabbing all three events from client
        FieldInfo handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<DiscordClient, MessageReactionAddEventArgs>));

        _reactionAddEvent = (AsyncEvent<DiscordClient, MessageReactionAddEventArgs>)handler.GetValue(_client);
        _reactionAddHandler = new AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs>(HandleReactionAdd);
        _reactionAddEvent.Register(_reactionAddHandler);

        handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs>));

        _reactionRemoveEvent = (AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs>)handler.GetValue(_client);
        _reactionRemoveHandler = new AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs>(HandleReactionRemove);
        _reactionRemoveEvent.Register(_reactionRemoveHandler);

        handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<DiscordClient, MessageReactionsClearEventArgs>));

        _reactionClearEvent = (AsyncEvent<DiscordClient, MessageReactionsClearEventArgs>)handler.GetValue(_client);
        _reactionClearHandler = new AsyncEventHandler<DiscordClient, MessageReactionsClearEventArgs>(HandleReactionClear);
        _reactionClearEvent.Register(_reactionClearHandler);
    }

    public async Task<ReadOnlyCollection<Reaction>> CollectAsync(ReactionCollectRequest request)
    {
        _requests.Add(request);
        ReadOnlyCollection<Reaction>? result = null;

        try
        {
            await request._tcs.Task;
        }
        catch (Exception ex)
        {
            _client.Logger.LogError(InteractivityEvents.InteractivityCollectorError, ex, "Exception occurred while collecting reactions");
        }
        finally
        {
            result = new ReadOnlyCollection<Reaction>(new HashSet<Reaction>(request._collected).ToList());
            request.Dispose();
            _requests.TryRemove(request);
        }
        return result;
    }

    private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventargs)
    {
        // foreach request add
        foreach (ReactionCollectRequest req in _requests)
        {
            if (req._message.Id == eventargs.Message.Id)
            {
                if (req._collected.Any(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id != eventargs.User.Id)))
                {
                    Reaction reaction = req._collected.First(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id != eventargs.User.Id));
                    req._collected.TryRemove(reaction);
                    reaction.Users.Add(eventargs.User);
                    req._collected.Add(reaction);
                }
                else
                {
                    req._collected.Add(new Reaction()
                    {
                        Emoji = eventargs.Emoji,
                        Users = [eventargs.User]
                    });
                }
            }
        }
        return Task.CompletedTask;
    }

    private Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs eventargs)
    {
        // foreach request remove
        foreach (ReactionCollectRequest req in _requests)
        {
            if (req._message.Id == eventargs.Message.Id)
            {
                if (req._collected.Any(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id == eventargs.User.Id)))
                {
                    Reaction reaction = req._collected.First(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id == eventargs.User.Id));
                    req._collected.TryRemove(reaction);
                    reaction.Users.TryRemove(eventargs.User);
                    if (reaction.Users.Count > 0)
                    {
                        req._collected.Add(reaction);
                    }
                }
            }
        }
        return Task.CompletedTask;
    }

    private Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventargs)
    {
        // foreach request add
        foreach (ReactionCollectRequest req in _requests)
        {
            if (req._message.Id == eventargs.Message.Id)
            {
                req._collected.Clear();
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes this EventWaiter
    /// </summary>
    public void Dispose()
    {
        _client = null!;

        if (_reactionAddHandler != null)
        {
            _reactionAddEvent?.Unregister(_reactionAddHandler);
        }

        if (_reactionRemoveHandler != null)
        {
            _reactionRemoveEvent?.Unregister(_reactionRemoveHandler);
        }

        if (_reactionClearHandler != null)
        {
            _reactionClearEvent?.Unregister(_reactionClearHandler);
        }

        _reactionAddEvent = null!;
        _reactionAddHandler = null!;
        _reactionRemoveEvent = null!;
        _reactionRemoveHandler = null!;
        _reactionClearEvent = null!;
        _reactionClearHandler = null!;

        _requests?.Clear();
        _requests = null!;

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}

public class ReactionCollectRequest : IDisposable
{
    internal TaskCompletionSource<Reaction> _tcs;
    internal CancellationTokenSource _ct;
    internal TimeSpan _timeout;
    internal DiscordMessage _message;
    internal ConcurrentHashSet<Reaction> _collected;

    public ReactionCollectRequest(DiscordMessage msg, TimeSpan timeout)
    {
        _message = msg;
        _collected = [];
        _timeout = timeout;
        _tcs = new TaskCompletionSource<Reaction>();
        _ct = new CancellationTokenSource(_timeout);
        _ct.Token.Register(() => _tcs.TrySetResult(null));
    }

    public void Dispose()
    {
        _ct.Dispose();
        _tcs = null;
        _message = null;
        _collected?.Clear();
        _collected = null;

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}

public class Reaction
{
    public DiscordEmoji Emoji { get; internal set; }
    public ConcurrentHashSet<DiscordUser> Users { get; internal set; }
    public int Total => Users.Count;
}
