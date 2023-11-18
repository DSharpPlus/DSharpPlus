namespace DSharpPlus.Commands;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Trees;
using Microsoft.Extensions.DependencyInjection;

public sealed class CommandExecutor : ICommandExecutor
{
    private readonly ConcurrentDictionary<Ulid, Task> _tasks = new();

    /// <inheritdoc/>
    public async ValueTask ExecuteAsync
    (
        CommandContext context, 
        bool awaitCommandExecution = false, 
        CancellationToken cancellationToken = default
    )
    {
        Ulid id = Ulid.NewUlid();
        Task task = Task.Run(() => WorkerAsync(context), cancellationToken);
        this._tasks.TryAdd(id, task);
        if (!awaitCommandExecution)
        {
            task = task.ContinueWith(_ => this._tasks.TryRemove(id, out Task? _));
        }
        else
        {
            await this._tasks[id];
            this._tasks.TryRemove(id, out Task? _);
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

        context.ServiceScope.Dispose();
    }
}
