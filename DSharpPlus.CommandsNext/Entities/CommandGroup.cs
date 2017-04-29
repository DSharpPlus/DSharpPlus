using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        
        internal override async Task Execute(CommandContext ctx)
        {
            var cn = ctx.RawArguments.FirstOrDefault();
            if (cn != null && this.Children.Any(xc => xc.Name == cn || (xc.Aliases != null && xc.Aliases.Contains(cn))))
            {
                // pass the execution on
                var cmd = this.Children.First(xc => xc.Name == cn || (xc.Aliases != null && xc.Aliases.Contains(cn)));

                var xctx = new CommandContext
                {
                    Client = ctx.Client,
                    Message = ctx.Message,
                    RawArguments = new ReadOnlyCollection<string>(ctx.RawArguments.Skip(1).ToList()),
                    Command = cmd
                };
                
                if (cmd.ExecutionChecks != null && cmd.ExecutionChecks.Any())
                    foreach (var ec in cmd.ExecutionChecks)
                        if (!(await ec.CanExecute(xctx)))
                            throw new UnauthorizedAccessException("One or more execution pre-checks failed.");

                await cmd.Execute(xctx);
                return;
            }

            if (this.Callable == null)
                throw new NotSupportedException("No matching subcommands were found, and this group is not executable.");

            var args = CommandsNextUtilities.BindArguments(ctx);

            var ret = (Task)this.Callable.DynamicInvoke(args);
            await ret;
        }
    }
}
