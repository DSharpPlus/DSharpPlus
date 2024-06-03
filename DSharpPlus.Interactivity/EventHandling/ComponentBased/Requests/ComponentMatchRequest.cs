using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity.EventHandling;

/// <summary>
/// Represents a match that is being waited for.
/// </summary>
internal class ComponentMatchRequest
{
    /// <summary>
    /// The id to wait on. This should be uniquely formatted to avoid collisions.
    /// </summary>
    public DiscordMessage Message { get; private set; }

    /// <summary>
    /// The completion source that represents the result of the match.
    /// </summary>
    public TaskCompletionSource<ComponentInteractionCreatedEventArgs> Tcs { get; private set; } = new();

    protected readonly CancellationToken cancellation;
    protected readonly Func<ComponentInteractionCreatedEventArgs, bool> predicate;

    public ComponentMatchRequest(DiscordMessage message, Func<ComponentInteractionCreatedEventArgs, bool> predicate, CancellationToken cancellation)
    {
        this.Message = message;
        this.predicate = predicate;
        this.cancellation = cancellation;
        this.cancellation.Register(() => this.Tcs.TrySetResult(null)); // TrySetCancelled would probably be better but I digress ~Velvet //
    }

    public bool IsMatch(ComponentInteractionCreatedEventArgs args) => this.predicate(args);
}
