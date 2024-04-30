namespace DSharpPlus.Interactivity.EventHandling;

using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

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
    public TaskCompletionSource<ModalSubmitEventArgs> Tcs { get; private set; } = new();

    protected CancellationToken Cancellation { get; }

    /// <summary>
    /// The predicate/criteria that this match will be fulfilled under.
    /// </summary>
    protected Func<ModalSubmitEventArgs, bool> Predicate { get; }

    public ModalMatchRequest(string modal_id, Func<ModalSubmitEventArgs, bool> predicate, CancellationToken cancellation)
    {
        ModalId = modal_id;
        Predicate = predicate;
        Cancellation = cancellation;
        Cancellation.Register(() => Tcs.TrySetResult(null)); // "TrySetCancelled would probably be better but I digress" - Velvet // "TrySetCancelled throws an exception when you await the task, actually" - Velvet, 2022
    }

    /// <summary>
    /// Checks whether the <see cref="ModalSubmitEventArgs"/> matches the predicate criteria.
    /// </summary>
    /// <param name="args">The <see cref="ModalSubmitEventArgs"/> to check.</param>
    /// <returns>Whether the <see cref="ModalSubmitEventArgs"/> matches the predicate.</returns>
    public bool IsMatch(ModalSubmitEventArgs args)
        => Predicate(args);
}
