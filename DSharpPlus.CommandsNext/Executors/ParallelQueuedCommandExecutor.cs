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

    private readonly CancellationTokenSource cts;
    private readonly CancellationToken ct;
    private readonly Channel<CommandContext> queue;
    private readonly ChannelWriter<CommandContext> queueWriter;
    private readonly ChannelReader<CommandContext> queueReader;
    private readonly Task[] tasks;

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
        this.Parallelism = parallelism;

        this.cts = new();
        this.ct = this.cts.Token;
        this.queue = Channel.CreateUnbounded<CommandContext>();
        this.queueReader = this.queue.Reader;
        this.queueWriter = this.queue.Writer;

        this.tasks = new Task[parallelism];
        for (int i = 0; i < parallelism; i++)
        {
            this.tasks[i] = Task.Run(this.ExecuteAsync);
        }
    }

    /// <summary>
    /// Disposes of the resources used by this executor.
    /// </summary>
    public void Dispose()
    {
        this.queueWriter.Complete();
        this.cts.Cancel();
        this.cts.Dispose();
    }

    async Task ICommandExecutor.ExecuteAsync(CommandContext ctx)
        => await this.queueWriter.WriteAsync(ctx, this.ct);

    private async Task ExecuteAsync()
    {
        while (!this.ct.IsCancellationRequested)
        {
            CommandContext? ctx = await this.queueReader.ReadAsync(this.ct);
            if (ctx is null)
            {
                continue;
            }

            await ctx.CommandsNext.ExecuteCommandAsync(ctx);
        }
    }
}
