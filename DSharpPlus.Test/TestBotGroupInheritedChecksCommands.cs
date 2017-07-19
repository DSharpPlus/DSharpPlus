using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DSharpPlus.Test
{
    [RequireOwner]
    public class TestBotGroupInheritedChecksCommands // damn you raelyn
    {
        [Command("shitpost")]
        public Task Shitpost(CommandContext ctx) =>
            ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":poop:").ToString());
    }
}
