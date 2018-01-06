using System;
using System.Collections.Generic;

namespace DSharpPlus.CommandsNext
{
    public sealed class CommandOverload
    {
        /// <summary>
        /// Gets this command's arguments.
        /// </summary>
        public IReadOnlyList<CommandArgument> Arguments { get; internal set; }

        /// <summary>
        /// Gets this command's callable.
        /// </summary>
        internal Delegate Callable { get; set; }
    }
}
