using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Exceptions;

namespace DSharpPlus.CommandsNext
{
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
        public bool IsExecutableWithoutSubcommands => this.Overloads.Count > 0;

        internal CommandGroup() : base() { }

        /// <summary>
        /// Executes this command or its subcommand with specified context.
        /// </summary>
        /// <param name="ctx">Context to execute the command in.</param>
        /// <returns>Command's execution results.</returns>
        public override async Task<CommandResult> ExecuteAsync(CommandContext ctx)
        {
            var findpos = 0;
            var cn = CommandsNextUtilities.ExtractNextArgument(ctx.RawArgumentString, ref findpos, ctx.Config.QuotationMarks);

            if (cn != null)
            {
                var (comparison, comparer) = ctx.Config.CaseSensitive
                    ? (StringComparison.InvariantCulture, StringComparer.InvariantCulture)
                    : (StringComparison.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);

                var cmd = this.Children.FirstOrDefault(xc => xc.Name.Equals(cn, comparison) || xc.Aliases.Contains(cn, comparer));

                if (cmd is not null)
                {
                    // pass the execution on
                    var xctx = new CommandContext
                    {
                        Client = ctx.Client,
                        Message = ctx.Message,
                        Command = cmd,
                        Config = ctx.Config,
                        RawArgumentString = ctx.RawArgumentString.Substring(findpos),
                        Prefix = ctx.Prefix,
                        CommandsNext = ctx.CommandsNext,
                        Services = ctx.Services
                    };

                    var fchecks = await cmd.RunChecksAsync(xctx, false);
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

            return this.IsExecutableWithoutSubcommands
                ? await base.ExecuteAsync(ctx)
                : new CommandResult
                {
                    IsSuccessful = false,
                    Exception = new InvalidOperationException("No matching subcommands were found, and this group is not executable."),
                    Context = ctx
                };
        }
    }
}
