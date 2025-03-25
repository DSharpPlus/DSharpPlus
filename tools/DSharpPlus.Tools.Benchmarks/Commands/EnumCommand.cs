using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;

namespace DSharpPlus.Tools.Benchmarks.Commands;

public static class EnumCommand
{
    [Command("enum")]
    public static ValueTask ExecuteAsync(CommandContext context, DayOfWeek day) => context.RespondAsync(day.ToString());
}
