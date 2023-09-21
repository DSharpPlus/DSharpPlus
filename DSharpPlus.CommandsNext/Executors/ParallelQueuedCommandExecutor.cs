using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Executors
{
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
            this.Parallelism = parallelism;

            this._cts = new();
            this._ct = this._cts.Token;
            this._queue = Channel.CreateUnbounded<CommandContext>();
            this._queueReader = this._queue.Reader;
            this._queueWriter = this._queue.Writer;

            this._tasks = new Task[parallelism];
            for (var i = 0; i < parallelism; i++)
                this._tasks[i] = Task.Run(this.ExecuteAsync);
        }

        /// <summary>
        /// Disposes of the resources used by this executor.
        /// </summary>
        public void Dispose()
        {
            this._queueWriter.Complete();
            this._cts.Cancel();
            this._cts.Dispose();
        }

        async Task ICommandExecutor.ExecuteAsync(CommandContext ctx)
            => await this._queueWriter.WriteAsync(ctx, this._ct);

        private async Task ExecuteAsync()
        {
            while (!this._ct.IsCancellationRequested)
            {
                var ctx = await this._queueReader.ReadAsync(this._ct);
                if (ctx is null)
                    continue;

                await ctx.CommandsNext.ExecuteCommandAsync(ctx);
            }
        }
    }
}
