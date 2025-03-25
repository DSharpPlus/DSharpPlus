using System.Threading.Tasks;
using DSharpPlus.Commands;

namespace DSharpPlus.Tools.Benchmarks.Commands;

public static class TwoArgCommand
{
    [Command("two")]
    public static ValueTask ExecuteAsync(CommandContext context, int num1, int num2) => ValueTask.CompletedTask;
}
