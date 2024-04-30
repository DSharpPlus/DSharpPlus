using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Executors;

/// <summary>
/// Defines an API surface for all command executors.
/// </summary>
public interface ICommandExecutor : IDisposable
{
    /// <summary>
    /// Executes a command from given context.
    /// </summary>
    /// <param name="ctx">Context to execute in.</param>
    /// <returns>Task encapsulating the async operation.</returns>
    Task ExecuteAsync(CommandContext ctx);
}
