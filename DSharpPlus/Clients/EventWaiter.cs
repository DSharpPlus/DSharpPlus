using System;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus.Clients;

/// <summary>
/// Provides a mechanism to wait for an instance of an event matching the specified condition.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record EventWaiter<T>
    where T : DiscordEventArgs
{
    /// <summary>
    /// The tracking identifier for this waiter.
    /// </summary>
    public required Ulid Id { get; init; }

    /// <summary>
    /// The condition an event needs to meet before it is considered to fulfil this event waiter.
    /// </summary>
    public required Func<T, bool> Condition { get; init; }

    /// <summary>
    /// An awaitable task completion source for this event waiter.
    /// </summary>
    /// <remarks>
    /// Do not manipulate this manually - it is exposed for implementers of <see cref="IEventDispatcher"/>. 
    /// </remarks>
    public required TaskCompletionSource<EventWaiterResult<T>> CompletionSource { get; init; }

    /// <summary>
    /// An awaitable task for this event waiter.
    /// </summary>
    public Task<EventWaiterResult<T>> Task => this.CompletionSource.Task;

    /// <summary>
    /// The timeout for this event waiter.
    /// </summary>
    public required TimeSpan Timeout { get; init; }
}