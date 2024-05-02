using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Executors;

/// <summary>
/// Executes commands using <see cref="Task.Run(Func{Task})"/>.
/// </summary>
public sealed class AsynchronousCommandExecutor : ICommandExecutor
{
    Task ICommandExecutor.ExecuteAsync(CommandContext ctx)
    {
        _ = ctx.CommandsNext.ExecuteCommandAsync(ctx);
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    { }
}
