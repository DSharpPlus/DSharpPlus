namespace DSharpPlus.CommandsNext;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Exceptions;

/// <summary>
/// Represents a command group.
/// </summary>
public class CommandGroup : Command
{
    /// <summary>
    /// Gets all the commands that belong to this module.
    /// </summary>
    public IReadOnlyList<Command> Children { get; internal set; } = Array.Empty<Command>();

    /// <summary>
    /// Gets whether this command is executable without subcommands.
    /// </summary>
    public bool IsExecutableWithoutSubcommands => Overloads.Count > 0;

    internal CommandGroup() : base() { }

    /// <summary>
    /// Executes this command or its subcommand with specified context.
    /// </summary>
    /// <param name="ctx">Context to execute the command in.</param>
    /// <returns>Command's execution results.</returns>
    public override async Task<CommandResult> ExecuteAsync(CommandContext ctx)
    {
        int findpos = 0;
        string? cn = CommandsNextUtilities.ExtractNextArgument(ctx.RawArgumentString, ref findpos, ctx.Config.QuotationMarks);

        if (cn != null)
        {
            (StringComparison comparison, StringComparer comparer) = ctx.Config.CaseSensitive
                ? (StringComparison.InvariantCulture, StringComparer.InvariantCulture)
                : (StringComparison.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);

            Command? cmd = Children.FirstOrDefault(xc => xc.Name.Equals(cn, comparison) || xc.Aliases.Contains(cn, comparer));

            if (cmd is not null)
            {
                // pass the execution on
                CommandContext xctx = new CommandContext
                {
                    Client = ctx.Client,
                    Message = ctx.Message,
                    Command = cmd,
                    Config = ctx.Config,
                    RawArgumentString = ctx.RawArgumentString[findpos..],
                    Prefix = ctx.Prefix,
                    CommandsNext = ctx.CommandsNext,
                    Services = ctx.Services
                };

                IEnumerable<Attributes.CheckBaseAttribute> fchecks = await cmd.RunChecksAsync(xctx, false);
                return !fchecks.Any()
                    ? await cmd.ExecuteAsync(xctx)
                    : new CommandResult
                    {
                        IsSuccessful = false,
                        Exception = new ChecksFailedException(cmd, xctx, fchecks),
                        Context = xctx
                    };
            }
        }

        return IsExecutableWithoutSubcommands
            ? await base.ExecuteAsync(ctx)
            : new CommandResult
            {
                IsSuccessful = false,
                Exception = new InvalidOperationException("No matching subcommands were found, and this group is not executable."),
                Context = ctx
            };
    }
}
