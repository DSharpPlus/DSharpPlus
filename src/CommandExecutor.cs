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
        private readonly ConcurrentDictionary<Guid, Task> _tasks = new();

        public CommandExecutor(ILogger<CommandExecutor> logger, CancellationToken cancellationToken = default)
        {
            _logger = logger ?? NullLogger<CommandExecutor>.Instance;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Executes a command asynchronously.
        /// </summary>
        /// <param name="context">The context of the command.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation. The value will be a <see cref="Optional{T}"/> of <see cref="bool"/>. <see cref="Optional{T}.HasValue"/> will be <see langword="true"/> if the command was executed successfully, <see langword="false"/> if the command was not executed successfully, and <see langword="null"/> if the command threw an exception.</returns>
        public void Execute(CommandContext context)
        {
            Guid guid = Guid.NewGuid();
            _tasks.TryAdd(guid, Task.Run(() => WorkerAsync(context, guid)));
        }

        private async ValueTask WorkerAsync(CommandContext context, Guid id)
        {
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

                await context.Extension._commandErrored.InvokeAsync(context.Extension, new CommandErroredEventArgs()
                {
                    Context = context,
                    Exception = error
                });
            }
            finally
            {
                _tasks.TryRemove(id, out Task? _);
            }
        }
    }
}
