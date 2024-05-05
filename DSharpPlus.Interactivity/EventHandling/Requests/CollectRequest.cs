using System;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.AsyncEvents;

namespace DSharpPlus.Interactivity.EventHandling;

/// <summary>
/// CollectRequest is a class that serves as a representation of
/// EventArgs that are being collected within a specific time frame.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class CollectRequest<T> : IDisposable where T : AsyncEventArgs
{
    internal TaskCompletionSource<bool> tcs;
    internal CancellationTokenSource ct;
    internal Func<T, bool> predicate;
    internal TimeSpan timeout;
    internal ConcurrentHashSet<T> collected;

    /// <summary>
    /// Creates a new CollectRequest object.
    /// </summary>
    /// <param name="predicate">Predicate to match</param>
    /// <param name="timeout">Timeout time</param>
    public CollectRequest(Func<T, bool> predicate, TimeSpan timeout)
    {
        this.tcs = new TaskCompletionSource<bool>();
        this.ct = new CancellationTokenSource(timeout);
        this.predicate = predicate;
        this.ct.Token.Register(() => this.tcs.TrySetResult(true));
        this.timeout = timeout;
        this.collected = [];
    }

    /// <summary>
    /// Disposes this CollectRequest.
    /// </summary>
    public void Dispose()
    {
        this.ct.Dispose();
        this.tcs = null!;
        this.predicate = null!;

        if (this.collected != null)
        {
            this.collected.Clear();
            this.collected = null!;
        }
    }
}

/*
          ^  ^
( Quack! )> (ﾐචᆽචﾐ)

(somewhere on twitter I read amazon had a duck
that said meow so I had to add a cat that says quack)

*/
