using System.Collections.Generic;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.CommandsNext.Converters;

/// <summary>
/// Represents a base class for all default help formatters.
/// </summary>
public abstract class BaseHelpFormatter
{
    /// <summary>
    /// Gets the context in which this formatter is being invoked.
    /// </summary>
    protected CommandContext Context { get; }

    /// <summary>
    /// Gets the CommandsNext extension which constructed this help formatter.
    /// </summary>
    protected CommandsNextExtension CommandsNext => this.Context.CommandsNext;

    /// <summary>
    /// Creates a new help formatter for specified CommandsNext extension instance.
    /// </summary>
    /// <param name="ctx">Context in which this formatter is being invoked.</param>
    public BaseHelpFormatter(CommandContext ctx) => this.Context = ctx;

    /// <summary>
    /// Sets the command this help message will be for.
    /// </summary>
    /// <param name="command">Command for which the help message is being produced.</param>
    /// <returns>This help formatter.</returns>
    public abstract BaseHelpFormatter WithCommand(Command command);

    /// <summary>
    /// Sets the subcommands for this command, if applicable. This method will be called with filtered data.
    /// </summary>
    /// <param name="subcommands">Subcommands for this command group.</param>
    /// <returns>This help formatter.</returns>
    public abstract BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands);

    /// <summary>
    /// Constructs the help message.
    /// </summary>
    /// <returns>Data for the help message.</returns>
    public abstract CommandHelpMessage Build();
}
