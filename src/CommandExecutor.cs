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

namespace DSharpPlus.CommandAll
{
    public sealed class CommandExecutor
    {
        private readonly ConcurrentDictionary<Guid, Task> _tasks = new();

        /// <summary>
        /// Executes a command asynchronously.
        /// </summary>
        /// <param name="context">The context of the command.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation. The value will be a <see cref="Optional{T}"/> of <see cref="bool"/>. <see cref="Optional{T}.HasValue"/> will be <see langword="true"/> if the command was executed successfully, <see langword="false"/> if the command was not executed successfully, and <see langword="null"/> if the command threw an exception.</returns>
        public async Task ExecuteAsync(CommandContext context, bool block = false, CancellationToken cancellationToken = default)
        {
            Guid guid = Guid.NewGuid();
            Task task = Task.Run(() => WorkerAsync(context), cancellationToken);
            if (!block)
            {
                task = task.ContinueWith(_ => _tasks.TryRemove(guid, out Task? _));
            }

            _tasks.TryAdd(guid, task);
            if (block)
            {
                await _tasks[guid];
                _tasks.TryRemove(guid, out Task? _);
            }
        }

        private static async ValueTask WorkerAsync(CommandContext context)
        {
            try
            {
                object? value = context.Command.Method!.Invoke(context.Command.Target, context.Arguments.Values.Prepend(context).ToArray());
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
        }
    }
}
