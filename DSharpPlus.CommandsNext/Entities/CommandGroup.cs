// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023

 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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

                    var fchecks = await cmd.RunChecksAsync(xctx, false).ConfigureAwait(false);
                    return !fchecks.Any()
                        ? await cmd.ExecuteAsync(xctx).ConfigureAwait(false)
                        : new CommandResult
                        {
                            IsSuccessful = false,
                            Exception = new ChecksFailedException(cmd, xctx, fchecks),
                            Context = xctx
                        };
                }
            }

            return this.IsExecutableWithoutSubcommands
                ? await base.ExecuteAsync(ctx).ConfigureAwait(false)
                : new CommandResult
                {
                    IsSuccessful = false,
                    Exception = new InvalidOperationException("No matching subcommands were found, and this group is not executable."),
                    Context = ctx
                };
        }
    }
}
