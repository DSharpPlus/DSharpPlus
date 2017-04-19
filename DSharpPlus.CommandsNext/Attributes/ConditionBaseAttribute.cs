using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Represents a base for all permission check attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class ConditionBaseAttribute : Attribute
    {
        /// <summary>
        /// Asynchronously checks whether this command can be executed within given context.
        /// </summary>
        /// <param name="ctx">Context to check execution ability for.</param>
        /// <returns></returns>
        public abstract Task<bool> CanExecute(CommandContext ctx);
    }
}
