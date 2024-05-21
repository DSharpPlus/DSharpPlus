using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity.EventHandling;

/// <summary>
/// Represents a match request for a modal of the given Id and predicate.
/// </summary>
internal class ModalMatchRequest
{
    /// <summary>
    /// The custom Id of the modal.
    /// </summary>
    public string ModalId { get; }

    /// <summary>
    /// The completion source that represents the result of the match.
    /// </summary>
    public TaskCompletionSource<ModalSubmittedEventArgs> Tcs { get; private set; } = new();

    protected CancellationToken Cancellation { get; }

    /// <summary>
    /// The predicate/criteria that this match will be fulfilled under.
    /// </summary>
    protected Func<ModalSubmittedEventArgs, bool> Predicate { get; }

    public ModalMatchRequest(string modal_id, Func<ModalSubmittedEventArgs, bool> predicate, CancellationToken cancellation)
    {
        this.ModalId = modal_id;
        this.Predicate = predicate;
        this.Cancellation = cancellation;
        this.Cancellation.Register(() => this.Tcs.TrySetResult(null)); // "TrySetCancelled would probably be better but I digress" - Velvet // "TrySetCancelled throws an exception when you await the task, actually" - Velvet, 2022
    }

    /// <summary>
    /// Checks whether the <see cref="ModalSubmittedEventArgs"/> matches the predicate criteria.
    /// </summary>
    /// <param name="args">The <see cref="ModalSubmittedEventArgs"/> to check.</param>
    /// <returns>Whether the <see cref="ModalSubmittedEventArgs"/> matches the predicate.</returns>
    public bool IsMatch(ModalSubmittedEventArgs args)
        => this.Predicate(args);
}
