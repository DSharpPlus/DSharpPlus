using System.Threading.Tasks;
using DSharpPlus.Commands;

namespace DSharpPlus.Tools.Benchmarks.Commands;

public static class OneArgCommand
{
    [Command("one")]
    public static ValueTask ExecuteAsync(CommandContext context, int num) => ValueTask.CompletedTask;
}
