using System;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext.Executors
{
    /// <summary>
    /// Executes commands by awaiting them.
    /// </summary>
    public sealed class SynchronousCommandExecutor : ICommandExecutor
    {
        async Task ICommandExecutor.ExecuteAsync(CommandContext ctx)
            => await ctx.CommandsNext.ExecuteCommandAsync(ctx);

        void IDisposable.Dispose()
        { }
    }
}
