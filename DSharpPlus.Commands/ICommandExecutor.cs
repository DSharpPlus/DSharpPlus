using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Commands;

public interface ICommandExecutor
{

    /// <summary>
    /// Executes a command asynchronously.
    /// </summary>
    /// <param name="context">The context of the command.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    public ValueTask ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default);
}
