using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DSharpPlus.Test
{
    public class TestBotNextCommands
    {
        [Command("hello"), Aliases("hi", "say_hello", "say_hi"), Description("Says hello to given user.")]
        public async Task SayHello(CommandContext ctx, string name)
        {
            await ctx.RespondAsync($"Hello, {name}!");
        }

        [Command("pingme"), Aliases("mentionme"), Description("Mentions the executor.")]
        public async Task PingMe(CommandContext ctx)
        {
            await ctx.RespondAsync($"{ctx.User.Mention}");
        }

        [Command("ping"), Aliases("mention"), Description("Mentions specified user.")]
        public async Task Ping(CommandContext ctx, DiscordMember member)
        {
            await ctx.RespondAsync($"{member.User.Mention}");
        }
    }
}
