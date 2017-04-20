using System;

namespace DSharpPlus.CommandsNext
{
    public sealed class CommandArgument
    {
        /// <summary>
        /// Gets this argument's name.
        /// </summary>
        public string Name { get; internal set; }
        
        /// <summary>
        /// Gets this argument's type.
        /// </summary>
        public Type Type { get; internal set; }

        /// <summary>
        /// Gets whether this argument is optional.
        /// </summary>
        public bool IsOptional { get; internal set; }

        /// <summary>
        /// Gets whether this argument has a default value.
        /// </summary>
        public object DefaultValue { get; internal set; }

        /// <summary>
        /// Gets this argument's description.
        /// </summary>
        public string Description { get; internal set; }
    }
}
