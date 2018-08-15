using System;
using System.Collections.Generic;

namespace DSharpPlus.CommandsNext
{
    /// <summary>
    /// Represents a specific overload of a command.
    /// </summary>
    public sealed class CommandOverload
    {
        /// <summary>
        /// Gets this command overload's arguments.
        /// </summary>
        public IReadOnlyList<CommandArgument> Arguments { get; internal set; }

        /// <summary>
        /// Gets this command overload's priority.
        /// </summary>
        public int Priority { get; internal set; }

        /// <summary>
        /// Gets this command overload's delegate.
        /// </summary>
        internal Delegate Callable { get; set; }
        
        internal object InvocationTarget { get; set; }

        internal CommandOverload() { }
    }
}
