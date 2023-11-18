namespace DSharpPlus.Commands;

using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Commands.Trees;

public interface ICommandExecutor
{

    /// <summary>
    /// Executes a command asynchronously.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="awaitCommandExecution">Whether to not return until the command has finished executing.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    public ValueTask ExecuteAsync
    (
        CommandContext context, 
        bool awaitCommandExecution = false, 
        CancellationToken cancellationToken = default
    );
}
