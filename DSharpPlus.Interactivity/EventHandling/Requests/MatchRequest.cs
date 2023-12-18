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
    internal TaskCompletionSource<T> _tcs;
    internal CancellationTokenSource _ct;
    internal Func<T, bool> _predicate;
    internal TimeSpan _timeout;

    /// <summary>
    /// Creates a new MatchRequest object.
    /// </summary>
    /// <param name="predicate">Predicate to match</param>
    /// <param name="timeout">Timeout time</param>
    public MatchRequest(Func<T, bool> predicate, TimeSpan timeout)
    {
        this._tcs = new TaskCompletionSource<T>();
        this._ct = new CancellationTokenSource(timeout);
        this._predicate = predicate;
        this._ct.Token.Register(() => this._tcs.TrySetResult(null));
        this._timeout = timeout;
    }

    /// <summary>
    /// Disposes this MatchRequest.
    /// </summary>
    public void Dispose()
    {
        this._ct?.Dispose();
        this._tcs = null!;
        this._predicate = null!;

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}
