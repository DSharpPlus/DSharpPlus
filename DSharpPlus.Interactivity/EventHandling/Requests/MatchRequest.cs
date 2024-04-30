namespace DSharpPlus.Interactivity.EventHandling;

using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;

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
        _tcs = new TaskCompletionSource<T>();
        _ct = new CancellationTokenSource(timeout);
        _predicate = predicate;
        _ct.Token.Register(() => _tcs.TrySetResult(null));
        _timeout = timeout;
    }

    /// <summary>
    /// Disposes this MatchRequest.
    /// </summary>
    public void Dispose()
    {
        _ct?.Dispose();
        _tcs = null!;
        _predicate = null!;

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}
