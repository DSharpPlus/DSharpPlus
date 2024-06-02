using System;
using System.Collections.Concurrent;
using System.Threading;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity.EventHandling;

/// <summary>
/// Represents a component event that is being waited for.
/// </summary>
internal sealed class ComponentCollectRequest : ComponentMatchRequest
{
    public ConcurrentBag<ComponentInteractionCreatedEventArgs> Collected { get; private set; }

    public ComponentCollectRequest(DiscordMessage message, Func<ComponentInteractionCreatedEventArgs, bool> predicate, CancellationToken cancellation) :
        base(message, predicate, cancellation)
    { }
}
