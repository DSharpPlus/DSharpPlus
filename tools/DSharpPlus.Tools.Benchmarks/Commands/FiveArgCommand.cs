using System.Threading.Tasks;
using DSharpPlus.Commands;

namespace DSharpPlus.Tools.Benchmarks.Commands;

public static class FiveArgCommand
{
    [Command("five")]
    public static ValueTask ExecuteAsync(CommandContext context, int num1, int num2, int num3, int num4, int num5) => ValueTask.CompletedTask;
}
