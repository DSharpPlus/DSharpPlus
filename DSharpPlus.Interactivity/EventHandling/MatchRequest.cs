using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.EventHandling
{
    public class MatchRequest<T> : IDisposable where T : AsyncEventArgs
    {
        internal TaskCompletionSource<T> _tcs;
        internal CancellationTokenSource _ct;
        internal Func<T, bool> _predicate;
        internal TimeSpan? _timeout;

        public MatchRequest(Func<T, bool> predicate, TimeSpan timeout)
        {
            _tcs = new TaskCompletionSource<T>();
            _ct = new CancellationTokenSource(timeout);
            _predicate = predicate;
            _ct.Token.Register(() => _tcs.TrySetResult(null));
            _timeout = timeout;
        }

        ~MatchRequest(){
            this.Dispose();
        }

        public void Dispose()
        {
            this._ct.Dispose();
            this._tcs = null;
            this._predicate = null;
        }
    }
}
