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
    public TaskCompletionSource<ComponentInteractionCreateEventArgs> Tcs { get; private set; } = new();

    protected readonly CancellationToken _cancellation;
    protected readonly Func<ComponentInteractionCreateEventArgs, bool> _predicate;

    public ComponentMatchRequest(DiscordMessage message, Func<ComponentInteractionCreateEventArgs, bool> predicate, CancellationToken cancellation)
    {
        Message = message;
        _predicate = predicate;
        _cancellation = cancellation;
        _cancellation.Register(() => Tcs.TrySetResult(null)); // TrySetCancelled would probably be better but I digress ~Velvet //
    }

    public bool IsMatch(ComponentInteractionCreateEventArgs args) => _predicate(args);
}
