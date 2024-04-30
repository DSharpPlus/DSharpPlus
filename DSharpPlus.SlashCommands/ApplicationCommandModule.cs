using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands;

/// <summary>
/// Represents a base class for slash command modules.
/// </summary>
public abstract class ApplicationCommandModule
{
    /// <summary>
    /// Called before the execution of a slash command in the module.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <returns> Whether or not to execute the slash command.</returns>
    public virtual Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        => Task.FromResult(true);

    /// <summary>
    /// Called after the execution of a slash command in the module.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <returns></returns>
    public virtual Task AfterSlashExecutionAsync(InteractionContext ctx)
        => Task.CompletedTask;

    /// <summary>
    /// Called before the execution of a context menu in the module.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <returns> Whether or not to execute the slash command. </returns>
    public virtual Task<bool> BeforeContextMenuExecutionAsync(ContextMenuContext ctx)
        => Task.FromResult(true);

    /// <summary>
    /// Called after the execution of a context menu in the module.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <returns></returns>
    public virtual Task AfterContextMenuExecutionAsync(ContextMenuContext ctx)
        => Task.CompletedTask;

}
