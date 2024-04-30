namespace DSharpPlus.CommandsNext.Executors;
using System;
using System.Threading.Tasks;

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
