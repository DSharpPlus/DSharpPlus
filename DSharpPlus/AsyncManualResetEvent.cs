namespace DSharpPlus;

using System.Threading;
using System.Threading.Tasks;

// source: https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-1-asyncmanualresetevent/
/// <summary>
/// Implements an async version of a <see cref="ManualResetEvent"/>
/// This class does currently not support Timeouts or the use of CancellationTokens
/// </summary>
internal class AsyncManualResetEvent
{
    public bool IsSet => _tsc != null && _tsc.Task.IsCompleted;

    private TaskCompletionSource<bool> _tsc;

    public AsyncManualResetEvent()
        : this(false)
    { }

    public AsyncManualResetEvent(bool initialState)
    {
        _tsc = new TaskCompletionSource<bool>();

        if (initialState)
        {
            _tsc.TrySetResult(true);
        }
    }

    public Task WaitAsync() => _tsc.Task;

    public Task SetAsync() => Task.Run(() => _tsc.TrySetResult(true));

    public void Reset()
    {
        while (true)
        {
            TaskCompletionSource<bool> tsc = _tsc;

            if (!tsc.Task.IsCompleted || Interlocked.CompareExchange(ref _tsc, new TaskCompletionSource<bool>(), tsc) == tsc)
            {
                return;
            }
        }
    }
}
