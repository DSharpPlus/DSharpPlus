using System;
using System.Threading;
using System.Threading.Tasks;
using Emzi0767.Utilities;

namespace DSharpPlus.Interactivity.EventHandling
{
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
            this._ct.Token.Register(() => _tcs.TrySetResult(null));
            this._timeout = timeout;
        }

        ~MatchRequest()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this MatchRequest.
        /// </summary>
        public void Dispose()
        {
            this._ct.Dispose();
            this._tcs = null;
            this._predicate = null;
        }
    }
}
