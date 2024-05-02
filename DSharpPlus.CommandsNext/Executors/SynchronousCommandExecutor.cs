
using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Executors;
/// <summary>
/// Executes commands by awaiting them.
/// </summary>
public sealed class SynchronousCommandExecutor : ICommandExecutor
{
    Task ICommandExecutor.ExecuteAsync(CommandContext ctx)
        => ctx.CommandsNext.ExecuteCommandAsync(ctx);

    void IDisposable.Dispose()
    { }
}
