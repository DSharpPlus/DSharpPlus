using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Executors;

/// <summary>
/// A command executor which uses a bounded pool of executors to execute commands. This can limit the impact of
/// commands on system resources, such as CPU usage.
/// </summary>
public sealed class ParallelQueuedCommandExecutor : ICommandExecutor
{
    /// <summary>
    /// Gets the degree of parallelism of this executor.
    /// </summary>
    public int Parallelism { get; }

    private readonly CancellationTokenSource _cts;
    private readonly CancellationToken _ct;
    private readonly Channel<CommandContext> _queue;
    private readonly ChannelWriter<CommandContext> _queueWriter;
    private readonly ChannelReader<CommandContext> _queueReader;
    private readonly Task[] _tasks;

    /// <summary>
    /// Creates a new executor, which uses up to 75% of system CPU resources.
    /// </summary>
    public ParallelQueuedCommandExecutor()
        : this((Environment.ProcessorCount + 1) * 3 / 4)
    { }

    /// <summary>
    /// Creates a new executor with specified degree of parallelism.
    /// </summary>
    /// <param name="parallelism">The number of workers to use. It is recommended this number does not exceed 150% of the physical CPU count.</param>
    public ParallelQueuedCommandExecutor(int parallelism)
    {
        Parallelism = parallelism;

        _cts = new();
        _ct = _cts.Token;
        _queue = Channel.CreateUnbounded<CommandContext>();
        _queueReader = _queue.Reader;
        _queueWriter = _queue.Writer;

        _tasks = new Task[parallelism];
        for (int i = 0; i < parallelism; i++)
        {
            _tasks[i] = Task.Run(ExecuteAsync);
        }
    }

    /// <summary>
    /// Disposes of the resources used by this executor.
    /// </summary>
    public void Dispose()
    {
        _queueWriter.Complete();
        _cts.Cancel();
        _cts.Dispose();
    }

    async Task ICommandExecutor.ExecuteAsync(CommandContext ctx)
        => await _queueWriter.WriteAsync(ctx, _ct);

    private async Task ExecuteAsync()
    {
        while (!_ct.IsCancellationRequested)
        {
            CommandContext? ctx = await _queueReader.ReadAsync(_ct);
            if (ctx is null)
            {
                continue;
            }

            await ctx.CommandsNext.ExecuteCommandAsync(ctx);
        }
    }
}
