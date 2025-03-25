using System.Threading.Tasks;
using DSharpPlus.Commands;

namespace DSharpPlus.Tools.Benchmarks.Commands;

public static class NoArgCommand
{
    [Command("none")]
    public static ValueTask ExecuteAsync(CommandContext context) => ValueTask.CompletedTask;
}
