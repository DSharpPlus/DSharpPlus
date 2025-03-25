using System.Threading.Tasks;
using DSharpPlus.Commands;

namespace DSharpPlus.Tools.Benchmarks.Commands;

public static class ThreeArgCommand
{
    [Command("three")]
    public static ValueTask ExecuteAsync(CommandContext context, int num1, int num2, int num3) => ValueTask.CompletedTask;
}
