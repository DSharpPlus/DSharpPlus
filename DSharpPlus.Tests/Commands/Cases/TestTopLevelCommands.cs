namespace DSharpPlus.Tests.Commands.Cases;

using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;

public class TestTopLevelCommands
{
    [Command("oops")]
    public static ValueTask OopsAsync() => default;

    [Command("ping")]
    public static ValueTask PingAsync(CommandContext context) => default;

    [Command("echo")]
    public static ValueTask EchoAsync(CommandContext context, [RemainingText] string message) => default;

    [Command("user_info")]
    public static ValueTask UserInfoAsync(CommandContext context, DiscordUser? user = null) => default;
}
