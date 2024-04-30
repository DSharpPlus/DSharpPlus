namespace DSharpPlus.Tests.Commands.Cases.Commands;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.TextCommands;

public class TestSingleLevelSubCommands
{
    [Command("tag")]
    public class TagCommand
    {
        [Command("add")]
        public static ValueTask AddAsync(TextCommandContext context, string name, [RemainingText] string content) => default;

        [Command("get")]
        public static ValueTask GetAsync(CommandContext context, string name) => default;
    }

    [Command("empty")]
    public class EmptyCommand { }
}
