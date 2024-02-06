namespace DSharpPlus.Commands;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Invocation;
using DSharpPlus.Commands.Trees;
using Microsoft.Extensions.DependencyInjection;

public sealed class CommandExecutor : ICommandExecutor
{
    private readonly ConcurrentDictionary<Ulid, Func<object?, object?[], ValueTask>> commandWrappers = new();

    /// <inheritdoc/>
    [SuppressMessage("Quality", "CA2012", Justification = "The worker does not pool instances and has its own error handling.")]
    public async ValueTask ExecuteAsync
    (
        CommandContext context,
        bool awaitCommandExecution = false,
        CancellationToken cancellationToken = default
    )
    {
        if (!awaitCommandExecution)
        {
            _ = WorkerAsync(context);
        }
        else
        {
            await WorkerAsync(context);
        }
    }

    private async ValueTask WorkerAsync(CommandContext context)
    {
        if (context.Command.Method is null)
        {
            await context.Extension._commandErrored.InvokeAsync(context.Extension, new CommandErroredEventArgs()
            {
                Context = context,
                Exception = new CommandNotExecutableException(context.Command, "Unable to execute a command that has no method. Is this command a group command?"),
                CommandObject = null
            });

            return;
        }
        else if (context.Arguments.Count != context.Command.Parameters.Count)
        {
            await context.Extension._commandErrored.InvokeAsync(context.Extension, new CommandErroredEventArgs()
            {
                Context = context,
                Exception = new ArgumentException("The number of arguments provided does not match the number of parameters the command expects."),
                CommandObject = null
            });

            return;
        }

        List<ContextCheckAttribute> checks = new(context.Command.Attributes.OfType<ContextCheckAttribute>());
        Command? parent = context.Command.Parent;
        while (parent is not null)
        {
            checks.AddRange(parent.Attributes.OfType<ContextCheckAttribute>());
            parent = parent.Parent;
        }

        if (checks.Count != 0)
        {
            List<string> failedChecks = [];

            // first, execute all unconditional checks
            foreach (ContextCheckMapEntry entry in context.Extension.Checks.Where(x => x.AttributeType == typeof(UnconditionalCheckAttribute)))
            {
                try
                {
                    object check = ActivatorUtilities.CreateInstance(context.Extension.ServiceProvider, entry.CheckType);
                    string? result = await entry.ExecuteCheckAsync(check, new UnconditionalCheckAttribute(), context);

                    if (result is not null)
                    {
                        failedChecks.Add(result);
                        continue;
                    }
                }
                catch (Exception error)
                {
                    failedChecks.Add($"{error}: {error.Message}\n{error.StackTrace}");
                }
            }

            // Reverse foreach so we execute the top-most command's checks first.
            for (int i = checks.Count - 1; i >= 0; i--)
            {
                foreach (ContextCheckMapEntry entry in context.Extension.Checks.Where(x => x.AttributeType == checks[i].GetType()))
                {
                    try
                    {
                        object check = ActivatorUtilities.CreateInstance(context.Extension.ServiceProvider, entry.CheckType);
                        string? result = await entry.ExecuteCheckAsync(check, checks[i], context);

                        if (result is not null)
                        {
                            failedChecks.Add(result);
                            continue;
                        }
                    }
                    // try/catch blocks are free until they catch
                    catch (Exception error)
                    {
                        failedChecks.Add($"{error}: {error.Message}\n{error.StackTrace}");
                    }
                }
            }

            if (failedChecks.Count > 0)
            {
                await context.Extension._commandErrored.InvokeAsync(context.Extension, new CommandErroredEventArgs()
                {
                    Context = context,
                    Exception = new ChecksFailedException(failedChecks.Distinct().ToList(), context.Command.Name),
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
                : ActivatorUtilities.CreateInstance(context.ServiceProvider, context.Command.Method.DeclaringType!);

            if (!this.commandWrappers.TryGetValue(context.Command.Id, out Func<object?, object?[], ValueTask>? wrapper))
            {
                wrapper = CommandEmitUtil.GetCommandInvocationFunc(context.Command.Method, context.Command.Target);
                this.commandWrappers[context.Command.Id] = wrapper;
            }

            await wrapper(commandObject, [context, .. context.Arguments.Values]);

            await context.Extension._commandExecuted.InvokeAsync(context.Extension, new CommandExecutedEventArgs()
            {
                Context = context,
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
