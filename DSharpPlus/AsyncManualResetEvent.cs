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
    public bool IsSet => this.taskCompletionSource != null && this.taskCompletionSource.Task.IsCompleted;

    private TaskCompletionSource<bool> taskCompletionSource;

    public AsyncManualResetEvent()
        : this(false)
    { }

    public AsyncManualResetEvent(bool initialState)
    {
        this.taskCompletionSource = new TaskCompletionSource<bool>();

        if (initialState)
        {
            this.taskCompletionSource.TrySetResult(true);
        }
    }

    public Task WaitAsync() => this.taskCompletionSource.Task;

    public Task SetAsync() => Task.Run(() => this.taskCompletionSource.TrySetResult(true));

    public void Reset()
    {
        while (true)
        {
            TaskCompletionSource<bool> completionSource = this.taskCompletionSource;

            if (!completionSource.Task.IsCompleted || Interlocked.CompareExchange(ref this.taskCompletionSource, new TaskCompletionSource<bool>(), completionSource) == completionSource)
            {
                return;
            }
        }
    }
}
