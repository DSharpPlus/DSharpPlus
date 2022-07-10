// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Represents a command.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Gets this command's name.
        /// </summary>
        public string Name { get; internal set; } = string.Empty;

        /// <summary>
        /// Gets this command's qualified name (i.e. one that includes all module names).
        /// </summary>
        public string QualifiedName => this.Parent is not null ? string.Concat(this.Parent.QualifiedName, " ", this.Name) : this.Name;

        /// <summary>
        /// Gets this command's aliases.
        /// </summary>
        public IReadOnlyList<string> Aliases { get; internal set; } = Array.Empty<string>();

        /// <summary>
        /// Gets this command's parent module, if any.
        /// </summary>
        public CommandGroup? Parent { get; internal set; }

        /// <summary>
        /// Gets this command's description.
        /// </summary>
        public string? Description { get; internal set; }

        /// <summary>
        /// Gets whether this command is hidden.
        /// </summary>
        public bool IsHidden { get; internal set; }

        /// <summary>
        /// Gets a collection of pre-execution checks for this command.
        /// </summary>
        public IReadOnlyList<CheckBaseAttribute> ExecutionChecks { get; internal set; } = Array.Empty<CheckBaseAttribute>();

        /// <summary>
        /// Gets a collection of this command's overloads.
        /// </summary>
        public IReadOnlyList<CommandOverload> Overloads { get; internal set; } = Array.Empty<CommandOverload>();

        /// <summary>
        /// Gets the module in which this command is defined.
        /// </summary>
        public ICommandModule? Module { get; internal set; }

        /// <summary>
        /// Gets the custom attributes defined on this command.
        /// </summary>
        public IReadOnlyList<Attribute> CustomAttributes { get; internal set; } = Array.Empty<Attribute>();

        internal Command() { }

        /// <summary>
        /// Executes this command with specified context.
        /// </summary>
        /// <param name="ctx">Context to execute the command in.</param>
        /// <returns>Command's execution results.</returns>
        public virtual async Task<CommandResult> ExecuteAsync(CommandContext ctx)
        {
            CommandResult res = default;
            try
            {
                var executed = false;
                foreach (var ovl in this.Overloads.OrderByDescending(x => x.Priority))
                {
                    ctx.Overload = ovl;
                    var args = await CommandsNextUtilities.BindArgumentsAsync(ctx, ctx.Config.IgnoreExtraArguments).ConfigureAwait(false);

                    if (!args.IsSuccessful)
                        continue;

                    ctx.RawArguments = args.Raw;

                    var mdl = ovl._invocationTarget ?? this.Module?.GetInstance(ctx.Services);
                    if (mdl is BaseCommandModule bcmBefore)
                        await bcmBefore.BeforeExecutionAsync(ctx).ConfigureAwait(false);

                    args.Converted[0] = mdl;
                    var ret = (Task)ovl._callable.DynamicInvoke(args.Converted);
                    await ret.ConfigureAwait(false);
                    executed = true;
                    res = new CommandResult
                    {
                        IsSuccessful = true,
                        Context = ctx
                    };

                    if (mdl is BaseCommandModule bcmAfter)
                        await bcmAfter.AfterExecutionAsync(ctx).ConfigureAwait(false);
                    break;
                }

                if (!executed)
                    throw new ArgumentException("Could not find a suitable overload for the command.");
            }
            catch (Exception ex)
            {
                res = new CommandResult
                {
                    IsSuccessful = false,
                    Exception = ex,
                    Context = ctx
                };
            }

            return res;
        }

        /// <summary>
        /// Runs pre-execution checks for this command and returns any that fail for given context.
        /// </summary>
        /// <param name="ctx">Context in which the command is executed.</param>
        /// <param name="help">Whether this check is being executed from help or not. This can be used to probe whether command can be run without setting off certain fail conditions (such as cooldowns).</param>
        /// <returns>Pre-execution checks that fail for given context.</returns>
        public async Task<List<CheckBaseAttribute>> RunChecksAsync(CommandContext ctx, bool help)
        {
            var fchecks = new List<CheckBaseAttribute>();
            if (this.ExecutionChecks.Any())
                foreach (var ec in this.ExecutionChecks)
                    if (!await ec.ExecuteCheckAsync(ctx, help).ConfigureAwait(false))
                        fchecks.Add(ec);

            return fchecks;
        }

        /// <summary>
        /// Checks whether this command is equal to another one.
        /// </summary>
        /// <param name="cmd1">Command to compare to.</param>
        /// <param name="cmd2">Command to compare.</param>
        /// <returns>Whether the two commands are equal.</returns>
        public static bool operator ==(Command? cmd1, Command? cmd2)
        {
            var o1 = cmd1 as object;
            var o2 = cmd2 as object;
            return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || cmd1!.QualifiedName == cmd2!.QualifiedName);
        }

        /// <summary>
        /// Checks whether this command is not equal to another one.
        /// </summary>
        /// <param name="cmd1">Command to compare to.</param>
        /// <param name="cmd2">Command to compare.</param>
        /// <returns>Whether the two commands are not equal.</returns>
        public static bool operator !=(Command? cmd1, Command? cmd2) => !(cmd1 == cmd2);

        /// <summary>
        /// Checks whether this command equals another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether this command is equal to another object.</returns>
        public override bool Equals(object? obj)
        {
            var o2 = this as object;
            return (obj != null || o2 == null) && (obj == null || o2 != null) && ((obj == null && o2 == null) || (obj is Command cmd && cmd.QualifiedName == this.QualifiedName));
        }

        /// <summary>
        /// Gets this command's hash code.
        /// </summary>
        /// <returns>This command's hash code.</returns>
        public override int GetHashCode() => this.QualifiedName.GetHashCode();

        /// <summary>
        /// Returns a string representation of this command.
        /// </summary>
        /// <returns>String representation of this command.</returns>
        public override string ToString()
        {
            return this is CommandGroup g
                ? $"Command Group: {this.QualifiedName}, {g.Children.Count} top-level children"
                : $"Command: {this.QualifiedName}";
        }
    }
}
