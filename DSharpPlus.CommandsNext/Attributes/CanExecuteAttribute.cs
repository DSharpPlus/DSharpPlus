using System;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Defines that this group can be executed without subcommands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CanExecuteAttribute : Attribute
    {
        /// <summary>
        /// Defines that this group can be executed without subcommands.
        /// </summary>
        public CanExecuteAttribute()
        { }
    }
}
