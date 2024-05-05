using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus;

// source: https://blogs.msdn.microsoft.com/pfxteam/2012/02/11/building-async-coordination-primitives-part-1-asyncmanualresetevent/
/// <summary>
/// Implements an async version of a <see cref="ManualResetEvent"/>
/// This class does currently not support Timeouts or the use of CancellationTokens
/// </summary>
internal class AsyncManualResetEvent
{
    public bool IsSet => this.tsc != null && this.tsc.Task.IsCompleted;

    private TaskCompletionSource<bool> tsc;

    public AsyncManualResetEvent()
        : this(false)
    { }

    public AsyncManualResetEvent(bool initialState)
    {
        this.tsc = new TaskCompletionSource<bool>();

        if (initialState)
        {
            this.tsc.TrySetResult(true);
        }
    }

    public Task WaitAsync() => this.tsc.Task;

    public Task SetAsync() => Task.Run(() => this.tsc.TrySetResult(true));

    public void Reset()
    {
        while (true)
        {
            TaskCompletionSource<bool> tsc = this.tsc;

            if (!tsc.Task.IsCompleted || Interlocked.CompareExchange(ref this.tsc, new TaskCompletionSource<bool>(), tsc) == tsc)
            {
                return;
            }
        }
    }
}
