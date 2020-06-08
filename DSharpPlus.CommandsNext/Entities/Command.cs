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
        public string Name { get; internal set; }

        /// <summary>
        /// Gets this command's qualified name (i.e. one that includes all module names).
        /// </summary>
        public string QualifiedName
            => this.Parent != null ? string.Concat(this.Parent.QualifiedName, " ", this.Name) : this.Name;

        /// <summary>
        /// Gets this command's aliases.
        /// </summary>
        public IReadOnlyList<string> Aliases { get; internal set; }

        /// <summary>
        /// Gets this command's parent module, if any.
        /// </summary>
        public CommandGroup Parent { get; internal set; }

        /// <summary>
        /// Gets this command's description.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets whether this command is hidden.
        /// </summary>
        public bool IsHidden { get; internal set; }

        /// <summary>
        /// Gets a collection of pre-execution checks for this command.
        /// </summary>
        public IReadOnlyList<CheckBaseAttribute> ExecutionChecks { get; internal set; }

        /// <summary>
        /// Gets a collection of this command's overloads.
        /// </summary>
        public IReadOnlyList<CommandOverload> Overloads { get; internal set; }

        /// <summary>
        /// Gets the module in which this command is defined.
        /// </summary>
        public ICommandModule Module { get; internal set; }

        /// <summary>
        /// Gets the custom attributes defined on this command.
        /// </summary>
        public IReadOnlyList<Attribute> CustomAttributes { get; internal set; }

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
                    var args = await CommandsNextUtilities.BindArguments(ctx, ctx.Config.IgnoreExtraArguments).ConfigureAwait(false);

                    if (!args.IsSuccessful)
                        continue;

                    ctx.RawArguments = args.Raw;
                    
                    var mdl = ovl.InvocationTarget ?? this.Module?.GetInstance(ctx.Services);
                    if (mdl is BaseCommandModule bcmBefore)
                        await bcmBefore.BeforeExecutionAsync(ctx).ConfigureAwait(false);

                    args.Converted[0] = mdl;
                    var ret = (Task)ovl.Callable.DynamicInvoke(args.Converted);
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
        public async Task<IEnumerable<CheckBaseAttribute>> RunChecksAsync(CommandContext ctx, bool help)
        {
            var fchecks = new List<CheckBaseAttribute>();
            if (this.ExecutionChecks != null && this.ExecutionChecks.Any())
                foreach (var ec in this.ExecutionChecks)
                    if (!(await ec.ExecuteCheckAsync(ctx, help).ConfigureAwait(false)))
                        fchecks.Add(ec);

            return fchecks;
        }

        /// <summary>
        /// Checks whether this command is equal to another one.
        /// </summary>
        /// <param name="cmd1">Command to compare to.</param>
        /// <param name="cmd2">Command to compare.</param>
        /// <returns>Whether the two commands are equal.</returns>
        public static bool operator ==(Command cmd1, Command cmd2)
        {
            var o1 = cmd1 as object;
            var o2 = cmd2 as object;

            if (o1 == null && o2 != null)
                return false;
            else if (o1 != null && o2 == null)
                return false;
            else if (o1 == null && o2 == null)
                return true;

            return cmd1.QualifiedName == cmd2.QualifiedName;
        }

        /// <summary>
        /// Checks whether this command is not equal to another one.
        /// </summary>
        /// <param name="cmd1">Command to compare to.</param>
        /// <param name="cmd2">Command to compare.</param>
        /// <returns>Whether the two commands are not equal.</returns>
        public static bool operator !=(Command cmd1, Command cmd2)
            => !(cmd1 == cmd2);

        /// <summary>
        /// Checks whether this command equals another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether this command is equal to another object.</returns>
        public override bool Equals(object obj)
        {
            var o1 = obj as object;
            var o2 = this as object;

            if (o1 == null && o2 != null)
                return false;
            else if (o1 != null && o2 == null)
                return false;
            else if (o1 == null && o2 == null)
                return true;

            var cmd = obj as Command;
            if ((object)cmd == null)
                return false;

            return cmd.QualifiedName == this.QualifiedName;
        }

        /// <summary>
        /// Gets this command's hash code.
        /// </summary>
        /// <returns>This command's hash code.</returns>
        public override int GetHashCode()
        {
            return this.QualifiedName.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of this command.
        /// </summary>
        /// <returns>String representation of this command.</returns>
        public override string ToString()
        {
            if (this is CommandGroup g)
                return $"Command Group: {this.QualifiedName}, {g.Children.Count} top-level children";
            return $"Command: {this.QualifiedName}";
        }
    }
}
