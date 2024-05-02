
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace DSharpPlus.Tests.Commands.Cases.Commands;
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
