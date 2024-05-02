
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
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Invocation;
using DSharpPlus.Commands.Trees;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Commands;
public class DefaultCommandExecutor : ICommandExecutor
{
    /// <summary>
    /// This dictionary contains all of the command wrappers intended to be used for bypassing the overhead of reflection and Task/ValueTask handling.
    /// </summary>
    protected readonly ConcurrentDictionary<Ulid, Func<object?, object?[], ValueTask>> _commandWrappers = new();

    /// <summary>
    /// This method will ensure that the command is executable, execute all context checks, and then execute the command, and invoke the appropriate events.
    /// </summary>
    /// <remarks>
    /// If any exceptions caused by the command were to occur, they will be delegated to the <see cref="CommandsExtension.CommandErrored"/> event.
    /// </remarks>
    /// <param name="context">The context of the command being executed.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the command execution.</param>
    public virtual async ValueTask ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        // Do some safety checks
        if (!IsCommandExecutable(context, out string? errorMessage))
        {
            await InvokeCommandErroredEventAsync(context.Extension, new CommandErroredEventArgs()
            {
                Context = context,
                Exception = new CommandNotExecutableException(context.Command, errorMessage),
                CommandObject = null
            });

            return;
        }

        // Execute all context checks and return any that failed.
        IReadOnlyList<ContextCheckFailedData> failedChecks = await ExecuteContextChecksAsync(context);
        if (failedChecks.Count > 0)
        {
            await InvokeCommandErroredEventAsync(context.Extension, new CommandErroredEventArgs()
            {
                Context = context,
                Exception = new ChecksFailedException(failedChecks, context.Command),
                CommandObject = null
            });

            return;
        }

        IReadOnlyList<ParameterCheckFailedData> failedParameterChecks = await ExecuteParameterChecksAsync(context);

        if (failedParameterChecks.Count > 0)
        {
            await InvokeCommandErroredEventAsync(context.Extension, new CommandErroredEventArgs()
            {
                Context = context,
                Exception = new ParameterChecksFailedException(failedParameterChecks, context.Command),
                CommandObject = null
            });

            return;
        }

        // Execute the command
        (object? commandObject, Exception? error) = await ExecuteCoreAsync(context);

        // If the command threw an exception, invoke the CommandErrored event.
        if (error is not null)
        {
            await InvokeCommandErroredEventAsync(context.Extension, new CommandErroredEventArgs()
            {
                Context = context,
                Exception = error,
                CommandObject = commandObject
            });
        }
        // Otherwise, invoke the CommandExecuted event.
        else
        {
            await InvokeCommandExecutedEventAsync(context.Extension, new CommandExecutedEventArgs()
            {
                Context = context,
                CommandObject = commandObject
            });
        }

        // Dispose of the service scope if it was created.
        context.ServiceScope.Dispose();
    }

    /// <summary>
    /// Ensures the command is executable before attempting to execute it.
    /// </summary>
    /// <remarks>
    /// This does NOT execute any context checks. This only checks if the command is executable based on the number of arguments provided.
    /// </remarks>
    /// <param name="context">The context of the command being executed.</param>
    /// <param name="errorMessage">Any error message that occurred during the check.</param>
    /// <returns>Whether the command can be executed.</returns>
    protected virtual bool IsCommandExecutable(CommandContext context, [NotNullWhen(false)] out string? errorMessage)
    {
        if (context.Command.Method is null)
        {
            errorMessage = "Unable to execute a command that has no method. Is this command a group command?";
            return false;
        }
        else if (context.Command.Target is null && context.Command.Method.DeclaringType is null)
        {
            errorMessage = "Unable to execute a delegate that has no target or declaring type. Is this command a group command?";
            return false;
        }
        else if (context.Arguments.Count != context.Command.Parameters.Count)
        {
            errorMessage = "The number of arguments provided does not match the number of parameters the command expects.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Executes any context checks tied
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual async ValueTask<IReadOnlyList<ContextCheckFailedData>> ExecuteContextChecksAsync(CommandContext context)
    {
        // Execute all checks and return any that failed.
        List<ContextCheckFailedData> failedChecks = [];

        // Reuse the same instance of UnconditionalCheckAttribute for all unconditional checks.
        UnconditionalCheckAttribute unconditionalCheck = new();

        // First, execute all unconditional checks
        foreach (ContextCheckMapEntry entry in context.Extension.Checks)
        {
            // Users must implement the check that requests the UnconditionalCheckAttribute from IContextCheck<UnconditionalCheckAttribute>
            if (entry.AttributeType != typeof(UnconditionalCheckAttribute))
            {
                continue;
            }

            try
            {
                // Create the check instance
                object check = ActivatorUtilities.CreateInstance(context.ServiceProvider, entry.CheckType);

                // Execute it
                string? result = await entry.ExecuteCheckAsync(check, unconditionalCheck, context);

                // It failed, add it to the list and continue with the others
                if (result is not null)
                {
                    failedChecks.Add(new()
                    {
                        ContextCheckAttribute = unconditionalCheck,
                        ErrorMessage = result
                    });
                }
            }
            catch (Exception error)
            {
                failedChecks.Add(new()
                {
                    ContextCheckAttribute = unconditionalCheck,
                    ErrorMessage = error.Message,
                    Exception = error
                });
            }
        }

        // Add all of the checks attached to the delegate first.
        List<ContextCheckAttribute> checks = new(context.Command.Attributes.OfType<ContextCheckAttribute>());

        // Add the parent's checks last so we can execute the checks in order.
        Command? parent = context.Command.Parent;
        while (parent is not null)
        {
            checks.AddRange(parent.Attributes.OfType<ContextCheckAttribute>());
            parent = parent.Parent;
        }

        // If there are no checks, we can skip this step.
        if (checks.Count == 0)
        {
            return [];
        }

        // Reverse foreach so we execute the top-most command's checks first.
        for (int i = checks.Count - 1; i >= 0; i--)
        {
            // Search for any checks that match the current check's type, as there can be multiple checks for the same attribute.
            foreach (ContextCheckMapEntry entry in context.Extension.Checks)
            {
                ContextCheckAttribute checkAttribute = checks[i];

                // Skip checks that don't match the current check's type.
                if (entry.AttributeType != checkAttribute.GetType())
                {
                    continue;
                }

                try
                {
                    // Create the check instance
                    object check = ActivatorUtilities.CreateInstance(context.ServiceProvider, entry.CheckType);

                    // Execute it
                    string? result = await entry.ExecuteCheckAsync(check, checkAttribute, context);

                    // It failed, add it to the list and continue with the others
                    if (result is not null)
                    {
                        failedChecks.Add(new()
                        {
                            ContextCheckAttribute = checkAttribute,
                            ErrorMessage = result
                        });

                        continue;
                    }
                }
                // try/catch blocks are free until they catch
                catch (Exception error)
                {
                    failedChecks.Add(new()
                    {
                        ContextCheckAttribute = checkAttribute,
                        ErrorMessage = error.Message,
                        Exception = error
                    });
                }
            }
        }

        return failedChecks;
    }

    public virtual async ValueTask<IReadOnlyList<ParameterCheckFailedData>> ExecuteParameterChecksAsync(CommandContext context)
    {
        List<ParameterCheckFailedData> failedChecks = [];

        // iterate over all parameters and their attributes.
        foreach (CommandParameter parameter in context.Command.Parameters)
        {
            foreach (ParameterCheckAttribute checkAttribute in parameter.Attributes.OfType<ParameterCheckAttribute>())
            {
                ParameterCheckInfo info = new(parameter, context.Arguments[parameter]);

                // execute each check, skipping over non-matching ones
                foreach (ParameterCheckMapEntry entry in context.Extension.ParameterChecks)
                {
                    if (entry.AttributeType != checkAttribute.GetType())
                    {
                        continue;
                    }

                    try
                    {
                        // create the check instance
                        object check = ActivatorUtilities.CreateInstance(context.ServiceProvider, entry.CheckType);

                        // execute the check
                        string? result = await entry.ExecuteCheckAsync(check, checkAttribute, info, context);

                        // it failed, add it to the list and continue with the others
                        if (result is not null)
                        {
                            failedChecks.Add(new()
                            {
                                ParameterCheckAttribute = checkAttribute,
                                ErrorMessage = result
                            });

                            continue;
                        }
                    }
                    // if an error occurred, add it to the list and continue, making sure to set the error message.
                    catch (Exception error)
                    {
                        failedChecks.Add(new()
                        {
                            ParameterCheckAttribute = checkAttribute,
                            ErrorMessage = error.Message,
                            Exception = error
                        });
                    }
                }
            }
        }

        return failedChecks;
    }

    /// <summary>
    /// This method will execute the command provided without any safety checks, context checks or event invocation.
    /// </summary>
    /// <param name="context">The context of the command being executed.</param>
    /// <returns>A tuple containing the command object and any error that occurred during execution. The command object may be null when the delegate is static and is from a static class.</returns>
    public virtual async ValueTask<(object? CommandObject, Exception? Error)> ExecuteCoreAsync(CommandContext context)
    {
        // Keep the command object in scope so it can be accessed after the command has been executed.
        object? commandObject = null;

        try
        {
            // If the class isn't static, we need to create an instance of it.
            if (!context.Command.Method!.DeclaringType!.IsAbstract || !context.Command.Method.DeclaringType.IsSealed)
            {
                // The delegate's object was provided, so we can use that.
                commandObject = context.Command.Target ?? ActivatorUtilities.CreateInstance(context.ServiceProvider, context.Command.Method.DeclaringType);
            }

            // Grab the method that wraps Task/ValueTask execution.
            if (!_commandWrappers.TryGetValue(context.Command.Id, out Func<object?, object?[], ValueTask>? wrapper))
            {
                wrapper = CommandEmitUtil.GetCommandInvocationFunc(context.Command.Method, context.Command.Target);
                _commandWrappers[context.Command.Id] = wrapper;
            }

            // Execute the command and return the result.
            await wrapper(commandObject, [context, .. context.Arguments.Values]);
            return (commandObject, null);
        }
        catch (Exception error)
        {
            // The command threw. Unwrap the stack trace as much as we can to provide helpful information to the developer.
            if (error is TargetInvocationException targetInvocationError && targetInvocationError.InnerException is not null)
            {
                error = ExceptionDispatchInfo.Capture(targetInvocationError.InnerException).SourceException;
            }

            return (commandObject, error);
        }
    }

    /// <summary>
    /// Invokes the <see cref="CommandsExtension.CommandErrored"/> event, which isn't normally exposed to the public API.
    /// </summary>
    /// <param name="extension">The extension/shard that the event is being invoked on.</param>
    /// <param name="eventArgs">The event arguments to pass to the event.</param>
    protected virtual async ValueTask InvokeCommandErroredEventAsync(CommandsExtension extension, CommandErroredEventArgs eventArgs)
        => await extension._commandErrored.InvokeAsync(extension, eventArgs);

    /// <summary>
    /// Invokes the <see cref="CommandsExtension.CommandExecuted"/> event, which isn't normally exposed to the public API.
    /// </summary>
    /// <param name="extension">The extension/shard that the event is being invoked on.</param>
    /// <param name="eventArgs">The event arguments to pass to the event.</param>
    protected virtual async ValueTask InvokeCommandExecutedEventAsync(CommandsExtension extension, CommandExecutedEventArgs eventArgs)
        => await extension._commandExecuted.InvokeAsync(extension, eventArgs);
}
