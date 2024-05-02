
using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Attributes;
/// <summary>
/// Represents a base for all command pre-execution check attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class CheckBaseAttribute : Attribute
{
    /// <summary>
    /// Asynchronously checks whether this command can be executed within given context.
    /// </summary>
    /// <param name="ctx">Context to check execution ability for.</param>
    /// <param name="help">Whether this check is being executed from help or not. This can be used to probe whether command can be run without setting off certain fail conditions (such as cooldowns).</param>
    /// <returns>Whether the command can be executed in given context.</returns>
    public abstract Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help);
}
