using DSharpPlus.CommandsNext.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public IReadOnlyCollection<Command> Children { get; internal set; }

        internal CommandGroup() : base() { }

        /// <summary>
        /// Executes this command or its subcommand with specified context.
        /// </summary>
        /// <param name="ctx">Context to execute the command in.</param>
        /// <returns>Command's execution results.</returns>
        public override async Task<CommandResult> ExecuteAsync(CommandContext ctx)
        {
            //var cn = ctx.RawArguments.FirstOrDefault();
            var cn = CommandsNextUtilities.ExtractNextArgument(ctx.RawArgumentString, out var x);

            if (x != null)
            {
                var xi = 0;
                for (; xi < x.Length; xi++)
                    if (!char.IsWhiteSpace(x[xi]))
                        break;
                if (xi > 0)
                    x = x.Substring(xi);
            }

            if (cn != null)
            {
                var hascommands = false;
                if (ctx.Config.CaseSensitive)
                    hascommands = this.Children.Any(xc => xc.Name == cn || (xc.Aliases != null && xc.Aliases.Contains(cn)));
                else
                    hascommands = this.Children.Any(xc => xc.Name.ToLower() == cn.ToLower() || (xc.Aliases != null && xc.Aliases.Select(xs => xs.ToLower()).Contains(cn.ToLower())));

                if (hascommands)
                {
                    // pass the execution on
                    var cmd = this.Children.First(xc => xc.Name == cn || (xc.Aliases != null && xc.Aliases.Contains(cn)));

                    var xctx = new CommandContext
                    {
                        Client = ctx.Client,
                        Message = ctx.Message,
                        Command = cmd,
                        Config = ctx.Config,
                        RawArgumentString = x,
                        CommandsNext = ctx.CommandsNext,
                        Dependencies = ctx.Dependencies
                    };

                    if (cmd.ExecutionChecks != null && cmd.ExecutionChecks.Any())
                        foreach (var ec in cmd.ExecutionChecks)
                            if (!(await ec.CanExecute(xctx)))
                                return new CommandResult
                                {
                                    IsSuccessful = false,
                                    Exception = new ChecksFailedException("One or more execution pre-checks failed.", cmd, xctx),
                                    Context = xctx
                                };
                    
                    return await cmd.ExecuteAsync(xctx);
                }
            }

            if (this.Callable == null)
                return new CommandResult
                {
                    IsSuccessful = false,
                    Exception = new NotSupportedException("No matching subcommands were found, and this group is not executable."),
                    Context = ctx
                };

            return await base.ExecuteAsync(ctx);
        }
    }
}
