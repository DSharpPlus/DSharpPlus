using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ConcurrentCollections;

using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling;

// nice documentation lmfao
/// <summary>
/// Eventwaiter is a class that serves as a layer between the InteractivityExtension
/// and the DiscordClient to listen to an event and check for matches to a predicate.
/// </summary>
internal class ReactionCollector : IDisposable
{
    private DiscordClient client;
    private AsyncEvent<DiscordClient, MessageReactionAddedEventArgs> reactionAddEvent;
    private AsyncEventHandler<DiscordClient, MessageReactionAddedEventArgs> reactionAddHandler;
    private AsyncEvent<DiscordClient, MessageReactionRemovedEventArgs> reactionRemoveEvent;
    private AsyncEventHandler<DiscordClient, MessageReactionRemovedEventArgs> reactionRemoveHandler;
    private AsyncEvent<DiscordClient, MessageReactionsClearedEventArgs> reactionClearEvent;
    private AsyncEventHandler<DiscordClient, MessageReactionsClearedEventArgs> reactionClearHandler;
    private ConcurrentHashSet<ReactionCollectRequest> requests;

    /// <summary>
    /// Creates a new Eventwaiter object.
    /// </summary>
    /// <param name="extension">Your DiscordClient</param>
    public ReactionCollector(InteractivityExtension extension)
    {
        this.requests = [];
        this.client = extension.Client;

        this.reactionAddEvent = (AsyncEvent<DiscordClient, MessageReactionAddedEventArgs>)extension.eventDistributor.GetOrAdd
        (
            typeof(MessageReactionAddedEventArgs),
            new AsyncEvent<DiscordClient, MessageReactionAddedEventArgs>(extension.errorHandler)
        );

        this.reactionRemoveEvent = (AsyncEvent<DiscordClient, MessageReactionRemovedEventArgs>)extension.eventDistributor.GetOrAdd
        (
            typeof(MessageReactionRemovedEventArgs),
            new AsyncEvent<DiscordClient, MessageReactionRemovedEventArgs>(extension.errorHandler)
        );

        this.reactionClearEvent = (AsyncEvent<DiscordClient, MessageReactionsClearedEventArgs>)extension.eventDistributor.GetOrAdd
        (
            typeof(MessageReactionsClearedEventArgs),
            new AsyncEvent<DiscordClient, MessageReactionsClearedEventArgs>(extension.errorHandler)
        );

        // Registering handlers
        this.reactionAddHandler = new AsyncEventHandler<DiscordClient, MessageReactionAddedEventArgs>(HandleReactionAdd);
        this.reactionAddEvent.Register(this.reactionAddHandler);

        this.reactionRemoveHandler = new AsyncEventHandler<DiscordClient, MessageReactionRemovedEventArgs>(HandleReactionRemove);
        this.reactionRemoveEvent.Register(this.reactionRemoveHandler);

        this.reactionClearHandler = new AsyncEventHandler<DiscordClient, MessageReactionsClearedEventArgs>(HandleReactionClear);
        this.reactionClearEvent.Register(this.reactionClearHandler);
    }

    public async Task<ReadOnlyCollection<Reaction>> CollectAsync(ReactionCollectRequest request)
    {
        this.requests.Add(request);
        ReadOnlyCollection<Reaction>? result;

        try
        {
            await request.tcs.Task;
        }
        catch (Exception ex)
        {
            this.client.Logger.LogError(InteractivityEvents.InteractivityCollectorError, ex, "Exception occurred while collecting reactions");
        }
        finally
        {
            result = new ReadOnlyCollection<Reaction>(new HashSet<Reaction>(request.collected).ToList());
            request.Dispose();
            this.requests.TryRemove(request);
        }
        return result;
    }

    private Task HandleReactionAdd(DiscordClient client, MessageReactionAddedEventArgs eventargs)
    {
        // foreach request add
        foreach (ReactionCollectRequest req in this.requests)
        {
            if (req.message.Id == eventargs.Message.Id)
            {
                if (req.collected.Any(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id != eventargs.User.Id)))
                {
                    Reaction reaction = req.collected.First(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id != eventargs.User.Id));
                    req.collected.TryRemove(reaction);
                    reaction.Users.Add(eventargs.User);
                    req.collected.Add(reaction);
                }
                else
                {
                    req.collected.Add(new Reaction()
                    {
                        Emoji = eventargs.Emoji,
                        Users = [eventargs.User]
                    });
                }
            }
        }
        return Task.CompletedTask;
    }

    private Task HandleReactionRemove(DiscordClient client, MessageReactionRemovedEventArgs eventargs)
    {
        // foreach request remove
        foreach (ReactionCollectRequest req in this.requests)
        {
            if (req.message.Id == eventargs.Message.Id)
            {
                if (req.collected.Any(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id == eventargs.User.Id)))
                {
                    Reaction reaction = req.collected.First(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id == eventargs.User.Id));
                    req.collected.TryRemove(reaction);
                    reaction.Users.TryRemove(eventargs.User);
                    if (reaction.Users.Count > 0)
                    {
                        req.collected.Add(reaction);
                    }
                }
            }
        }
        return Task.CompletedTask;
    }

    private Task HandleReactionClear(DiscordClient client, MessageReactionsClearedEventArgs eventargs)
    {
        // foreach request add
        foreach (ReactionCollectRequest req in this.requests)
        {
            if (req.message.Id == eventargs.Message.Id)
            {
                req.collected.Clear();
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes this EventWaiter
    /// </summary>
    public void Dispose()
    {
        this.client = null!;

        if (this.reactionAddHandler != null)
        {
            this.reactionAddEvent?.Unregister(this.reactionAddHandler);
        }

        if (this.reactionRemoveHandler != null)
        {
            this.reactionRemoveEvent?.Unregister(this.reactionRemoveHandler);
        }

        if (this.reactionClearHandler != null)
        {
            this.reactionClearEvent?.Unregister(this.reactionClearHandler);
        }

        this.reactionAddEvent = null!;
        this.reactionAddHandler = null!;
        this.reactionRemoveEvent = null!;
        this.reactionRemoveHandler = null!;
        this.reactionClearEvent = null!;
        this.reactionClearHandler = null!;

        this.requests?.Clear();
        this.requests = null!;

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}

public class ReactionCollectRequest : IDisposable
{
    internal TaskCompletionSource<Reaction> tcs;
    internal CancellationTokenSource ct;
    internal TimeSpan timeout;
    internal DiscordMessage message;
    internal ConcurrentHashSet<Reaction> collected;

    public ReactionCollectRequest(DiscordMessage msg, TimeSpan timeout)
    {
        this.message = msg;
        this.collected = [];
        this.timeout = timeout;
        this.tcs = new TaskCompletionSource<Reaction>();
        this.ct = new CancellationTokenSource(this.timeout);
        this.ct.Token.Register(() => this.tcs.TrySetResult(null));
    }

    public void Dispose()
    {
        this.ct.Dispose();
        this.tcs = null;
        this.message = null;
        this.collected?.Clear();
        this.collected = null;

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}

public class Reaction
{
    public DiscordEmoji Emoji { get; internal set; }
    public ConcurrentHashSet<DiscordUser> Users { get; internal set; }
    public int Total => this.Users.Count;
}
