using System.Threading.Tasks;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Represents a base class for slash command modules
    /// </summary>
    public abstract class SlashCommandModule
    {
        /// <summary>
        /// Called before the execution of command in the module.
        /// </summary>
        /// <param name="ctx">The context.</param>
        public virtual Task BeforeExecutionAsync(InteractionContext ctx)
            => Task.CompletedTask;

        /// <summary>
        /// Called after the execution of a command in the module.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <returns></returns>
        public virtual Task AfterExecutionAsync(InteractionContext ctx)
            => Task.CompletedTask;
    }
}