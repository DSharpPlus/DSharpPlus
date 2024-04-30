namespace DSharpPlus.Interactivity.EventHandling;

using System;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.AsyncEvents;

/// <summary>
/// CollectRequest is a class that serves as a representation of
/// EventArgs that are being collected within a specific time frame.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class CollectRequest<T> : IDisposable where T : AsyncEventArgs
{
    internal TaskCompletionSource<bool> _tcs;
    internal CancellationTokenSource _ct;
    internal Func<T, bool> _predicate;
    internal TimeSpan _timeout;
    internal ConcurrentHashSet<T> _collected;

    /// <summary>
    /// Creates a new CollectRequest object.
    /// </summary>
    /// <param name="predicate">Predicate to match</param>
    /// <param name="timeout">Timeout time</param>
    public CollectRequest(Func<T, bool> predicate, TimeSpan timeout)
    {
        _tcs = new TaskCompletionSource<bool>();
        _ct = new CancellationTokenSource(timeout);
        _predicate = predicate;
        _ct.Token.Register(() => _tcs.TrySetResult(true));
        _timeout = timeout;
        _collected = new ConcurrentHashSet<T>();
    }

    /// <summary>
    /// Disposes this CollectRequest.
    /// </summary>
    public void Dispose()
    {
        _ct.Dispose();
        _tcs = null!;
        _predicate = null!;

        if (_collected != null)
        {
            _collected.Clear();
            _collected = null!;
        }
    }
}


/*
          ^  ^
( Quack! )> (ﾐචᆽචﾐ)


(somewhere on twitter I read amazon had a duck
that said meow so I had to add a cat that says quack)

*/
