using System;
using System.Collections.Generic;
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
        public IReadOnlyCollection<string> Aliases { get; internal set; }

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
        /// Gets a collection of execution pre-checks for this command.
        /// </summary>
        public IReadOnlyCollection<ConditionBaseAttribute> ExecutionChecks { get; internal set; }

        /// <summary>
        /// Gets this command's callable.
        /// </summary>
        internal Delegate Callable { get; set; }

        internal virtual async Task Execute(CommandContext ctx)
        {
            var args = CommandsNextUtilities.BindArguments(ctx);
            
            var ret = (Task)this.Callable.DynamicInvoke(args);
            await ret;
        }
    }
}
