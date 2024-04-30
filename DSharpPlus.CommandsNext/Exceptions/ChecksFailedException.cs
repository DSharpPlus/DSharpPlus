namespace DSharpPlus.CommandsNext.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DSharpPlus.CommandsNext.Attributes;

/// <summary>
/// Indicates that one or more checks for given command have failed.
/// </summary>
public class ChecksFailedException : Exception
{
    /// <summary>
    /// Gets the command that was executed.
    /// </summary>
    public Command Command { get; }

    /// <summary>
    /// Gets the context in which given command was executed.
    /// </summary>
    public CommandContext Context { get; }

    /// <summary>
    /// Gets the checks that failed.
    /// </summary>
    public IReadOnlyList<CheckBaseAttribute> FailedChecks { get; }

    /// <summary>
    /// Creates a new <see cref="ChecksFailedException"/>.
    /// </summary>
    /// <param name="command">Command that failed to execute.</param>
    /// <param name="ctx">Context in which the command was executed.</param>
    /// <param name="failedChecks">A collection of checks that failed.</param>
    public ChecksFailedException(Command command, CommandContext ctx, IEnumerable<CheckBaseAttribute> failedChecks)
        : base("One or more pre-execution checks failed.")
    {
        Command = command;
        Context = ctx;
        FailedChecks = new ReadOnlyCollection<CheckBaseAttribute>(new List<CheckBaseAttribute>(failedChecks));
    }
}
