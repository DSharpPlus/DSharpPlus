using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
        public IReadOnlyList<string> Aliases { get; private set; }

        /// <summary>
        /// Adds aliases to this command or group.
        /// </summary>
        /// <param name="aliases">Aliases to add to this command or group.</param>
        public AliasesAttribute(params string[] aliases)
        {
#if !NETSTANDARD1_1
            if (aliases.Any(xa => xa.Any(xc => char.IsWhiteSpace(xc))))
#else
            if (aliases.Any(xa => xa.ToCharArray().Any(xc => char.IsWhiteSpace(xc))))
#endif
                throw new ArgumentException("Aliases cannot contain whitespace characters.", nameof(aliases));

            this.Aliases = new ReadOnlyCollection<string>(aliases);
        }
    }
}
