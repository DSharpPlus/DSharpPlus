using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.ContextChecks;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.CommandAll.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

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
            List<ContextCheckAttribute> checks = new(context.Command.Attributes.OfType<ContextCheckAttribute>());
            Command? parent = context.Command.Parent;
            while (parent is not null)
            {
                checks.AddRange(parent.Attributes.OfType<ContextCheckAttribute>());
                parent = parent.Parent;
            }

            if (checks.Count != 0)
            {
                ContextCheckAttribute check = null!;
                try
                {
                    // Reverse foreach so we execute the top-most command's checks first.
                    for (int i = checks.Count - 1; i >= 0; i--)
                    {
                        check = checks[i];
                        if (!await check.ExecuteCheckAsync(context))
                        {
                            await context.Extension._commandErrored.InvokeAsync(context.Extension, new CommandErroredEventArgs()
                            {
                                Context = context,
                                Exception = new CheckFailedException(check),
                                CommandObject = null
                            });

                            return;
                        }
                    }
                }
                catch (Exception error)
                {
                    if (error is TargetInvocationException targetInvocationError && targetInvocationError.InnerException is not null)
                    {
                        error = ExceptionDispatchInfo.Capture(targetInvocationError.InnerException).SourceException;
                    }

                    await context.Extension._commandErrored.InvokeAsync(context.Extension, new CommandErroredEventArgs()
                    {
                        Context = context,
                        Exception = new CheckFailedException(check, error),
                        CommandObject = null
                    });

                    return;
                }
            }

            object? commandObject = null;
            try
            {
                commandObject = context.Command.Target is not null
                    ? context.Command.Target
                    : ActivatorUtilities.CreateInstance(context.ServiceProvider, context.Command.Method!.DeclaringType!);

                object? returnValue = context.Command.Method!.Invoke(commandObject, context.Arguments.Values.Prepend(context).ToArray());
                if (returnValue is Task task)
                {
                    await task;
                }
                else if (returnValue is ValueTask valueTask)
                {
                    await valueTask;
                }

                await context.Extension._commandExecuted.InvokeAsync(context.Extension, new CommandExecutedEventArgs()
                {
                    Context = context,
                    ReturnValue = returnValue,
                    CommandObject = commandObject
                });
            }
            catch (Exception error)
            {
                if (error is TargetInvocationException targetInvocationError && targetInvocationError.InnerException is not null)
                {
                    error = ExceptionDispatchInfo.Capture(targetInvocationError.InnerException).SourceException;
                }

                await context.Extension._commandErrored.InvokeAsync(context.Extension, new CommandErroredEventArgs()
                {
                    Context = context,
                    Exception = error,
                    CommandObject = commandObject
                });
            }

            await context.ServiceScope.DisposeAsync();
        }
    }
}
