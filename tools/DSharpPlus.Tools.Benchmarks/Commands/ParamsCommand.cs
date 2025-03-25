using System.Threading.Tasks;
using DSharpPlus.Commands;

namespace DSharpPlus.Tools.Benchmarks.Commands;

public static class ParamsCommand
{
    [Command("params")]
    public static ValueTask ExecuteAsync(CommandContext context, params int[] number) => ValueTask.CompletedTask;
}
