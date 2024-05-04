using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;

namespace DSharpPlus.Interactivity.EventHandling;

/// <summary>
/// MatchRequest is a class that serves as a representation of a
/// match that is being waited for.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class MatchRequest<T> : IDisposable where T : AsyncEventArgs
{
    internal TaskCompletionSource<T> tcs;
    internal CancellationTokenSource ct;
    internal Func<T, bool> predicate;
    internal TimeSpan timeout;

    /// <summary>
    /// Creates a new MatchRequest object.
    /// </summary>
    /// <param name="predicate">Predicate to match</param>
    /// <param name="timeout">Timeout time</param>
    public MatchRequest(Func<T, bool> predicate, TimeSpan timeout)
    {
        this.tcs = new TaskCompletionSource<T>();
        this.ct = new CancellationTokenSource(timeout);
        this.predicate = predicate;
        this.ct.Token.Register(() => this.tcs.TrySetResult(null));
        this.timeout = timeout;
    }

    /// <summary>
    /// Disposes this MatchRequest.
    /// </summary>
    public void Dispose()
    {
        this.ct?.Dispose();
        this.tcs = null!;
        this.predicate = null!;

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}
