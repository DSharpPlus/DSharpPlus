using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus.Clients;

/// <summary>
/// Provides a mechanism to collect instances of a specified event matching a certain condition over a specified amount of time
/// </summary>
public sealed record EventCollector<T>
    where T : DiscordEventArgs
{
    /// <summary>
    /// The tracking identifier for this collector.
    /// </summary>
    public required Ulid Id { get; init; }

    /// <summary>
    /// The condition an event needs to meet before it is considered to fulfil this event collector.
    /// </summary>
    public required Func<T, bool> Condition { get; init; }

    /// <summary>
    /// An awaitable task completion source for this event collector.
    /// </summary>
    internal TaskCompletionSource<IReadOnlyList<T>> CompletionSource { get; } = new();

    /// <summary>
    /// The events already collected by this object.
    /// </summary>
    internal ConcurrentBag<T> CollectedEvents { get; } = [];

    /// <summary>
    /// An awaitable task for this event collector.
    /// </summary>
    public Task<IReadOnlyList<T>> Task => this.CompletionSource.Task;

    /// <summary>
    /// The timeout for this event collector.
    /// </summary>
    public required TimeSpan Timeout { get; init; }
}
