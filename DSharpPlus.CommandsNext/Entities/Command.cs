using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;

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
        public string QualifiedName => this.Parent != null ? string.Concat(this.Parent.QualifiedName, " ", this.Name) : this.Name;

        /// <summary>
        /// Gets this command's alises.
        /// </summary>
        public IReadOnlyList<string> Aliases { get; internal set; }

        /// <summary>
        /// Gets this command's arguments.
        /// </summary>
        public IReadOnlyList<CommandArgument> Arguments { get; internal set; }

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
        /// Gets this command's callable.
        /// </summary>
        internal Delegate Callable { get; set; }

        internal Command() { }
        
        /// <summary>
        /// Executes this command with specified context.
        /// </summary>
        /// <param name="ctx">Context to execute the command in.</param>
        /// <returns>Command's execution results.</returns>
        public virtual async Task<CommandResult> ExecuteAsync(CommandContext ctx)
        {
            try
            {
                var args = CommandsNextUtilities.BindArguments(ctx, ctx.Config.IgnoreExtraArguments);
                var ret = (Task)this.Callable.DynamicInvoke(args);
                await ret;
            }
            catch (Exception ex)
            {
                return new CommandResult
                {
                    IsSuccessful = false,
                    Exception = ex,
                    Context = ctx
                };
            }

            return new CommandResult
            {
                IsSuccessful = true,
                Context = ctx
            };
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
                    if (!(await ec.CanExecute(ctx, help)))
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
        public static bool operator !=(Command cmd1, Command cmd2) =>
            !(cmd1 == cmd2);

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
