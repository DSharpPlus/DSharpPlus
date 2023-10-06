using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.CommandAll
{
    public sealed class CommandExecutor
    {
        private readonly ILogger<CommandExecutor> _logger;
        private readonly CancellationToken _cancellationToken;
        private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(1));
        private readonly ConcurrentQueue<CommandContext> _queue = new();
        private readonly Task[] _workers;

        public CommandExecutor(ILogger<CommandExecutor> logger, CancellationToken cancellationToken = default) : this((int)double.Round(Environment.ProcessorCount * 0.75, MidpointRounding.ToZero), logger, cancellationToken) { }

        public CommandExecutor(int parallelism, ILogger<CommandExecutor> logger, CancellationToken cancellationToken = default)
        {
            _logger = logger ?? NullLogger<CommandExecutor>.Instance;
            _cancellationToken = cancellationToken;

            if (parallelism < 1)
            {
                parallelism = 1;
                _logger.LogWarning("Parallelism was set to less than 1. Defaulting to 1.");
            }

            _workers = new Task[parallelism];
            for (int i = 0; i < parallelism; i++)
            {
                _workers[i] = Task.Run(WorkerAsync);
            }
        }

        /// <summary>
        /// Executes a command asynchronously.
        /// </summary>
        /// <param name="context">The context of the command.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation. The value will be a <see cref="Optional{T}"/> of <see cref="bool"/>. <see cref="Optional{T}.HasValue"/> will be <see langword="true"/> if the command was executed successfully, <see langword="false"/> if the command was not executed successfully, and <see langword="null"/> if the command threw an exception.</returns>
        public void ExecuteAsync(CommandContext context) => _queue.Enqueue(context);

        private async ValueTask WorkerAsync()
        {
            while (await _timer.WaitForNextTickAsync(_cancellationToken))
            {
                if (!_queue.TryDequeue(out CommandContext? context))
                {
                    // Wait another millisecond for the next command.
                    continue;
                }

                try
                {
                    object? value = context.Command.Delegate.Method.Invoke(context.Command.Delegate.Target, context.Arguments.Values.Prepend(context).ToArray());
                    if (value is Task task)
                    {
                        await task;
                    }
                    else if (value is ValueTask valueTask)
                    {
                        await valueTask;
                    }

                    await context.Extension._commandExecuted.InvokeAsync(context.Extension, new CommandExecutedEventArgs()
                    {
                        Context = context,
                        Value = value
                    });
                }
                catch (Exception error)
                {
                    if (error is TargetInvocationException targetInvocationError)
                    {
                        error = ExceptionDispatchInfo.Capture(targetInvocationError.InnerException!).SourceException;
                    }

                    await context.Extension._commandExecuted.InvokeAsync(context.Extension, new CommandErroredEventArgs()
                    {
                        Context = context,
                        Exception = error
                    });
                }
            }
        }
    }
}
