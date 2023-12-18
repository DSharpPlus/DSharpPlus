using System;
using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// The base class for a pre-execution check for a slash command.
/// </summary>
public abstract class SlashCheckBaseAttribute : Attribute
{
    /// <summary>
    /// Checks whether this command can be executed within the current context.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <returns>Whether the checks passed.</returns>
    public abstract Task<bool> ExecuteChecksAsync(InteractionContext ctx);
}
