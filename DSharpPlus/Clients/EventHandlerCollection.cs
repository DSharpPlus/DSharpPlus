using System;
using System.Collections.Concurrent;

using NonBlocking;

namespace DSharpPlus;

/// <summary>
/// Contains an in-construction, mutable, list of event handlers filtered by event name.
/// </summary>
public sealed class EventHandlerCollection
{
    /// <summary>
    /// The delegate-based event handlers configured for this application.
    /// </summary>
    public NonBlockingDictionary<Type, ConcurrentBag<Delegate>> DelegateHandlers { get; } = [];
}
