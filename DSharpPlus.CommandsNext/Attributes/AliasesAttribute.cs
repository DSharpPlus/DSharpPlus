using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Adds aliases to this command or group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AliasesAttribute : Attribute
    {
        /// <summary>
        /// Gets this group's aliases.
        /// </summary>
        public IReadOnlyCollection<string> Aliases { get; private set; }

        /// <summary>
        /// Adds aliases to this command or group.
        /// </summary>
        /// <param name="aliases">Aliases to add to this command or group.</param>
        public AliasesAttribute(params string[] aliases)
        {
            this.Aliases = new ReadOnlyCollection<string>(aliases);
        }
    }
}
