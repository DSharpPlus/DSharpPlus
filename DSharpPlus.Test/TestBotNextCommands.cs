using System.Collections.Concurrent;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DSharpPlus.Test
{
    public class TestBotNextCommands
    {
        public static ConcurrentDictionary<ulong, string> Prefixes { get; } = new ConcurrentDictionary<ulong, string>();

        [Command("setprefix"), Aliases("channelprefix"), Description("Sets custom command prefix for current channel. The bot will still respond to the default one."), RequireOwner]
        public async Task SetPrefix(CommandContext ctx, [Description("The prefix to use for current channel.")] string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                if (Prefixes.TryRemove(ctx.Channel.Id, out _))
                    await ctx.RespondAsync("👍").ConfigureAwait(false);
                else
                    await ctx.RespondAsync("👎").ConfigureAwait(false);
            else
            {
                Prefixes.AddOrUpdate(ctx.Channel.Id, prefix, (k, vold) => prefix);
                await ctx.RespondAsync("👍").ConfigureAwait(false);
            }
        }

        [Command("sudo"), Description("Run a command as another user."), RequireOwner]
        public async Task Sudo(CommandContext ctx, DiscordUser user, [RemainingText] string content)
        {
            await ctx.Client.GetCommandsNext().SudoAsync(user, ctx.Channel, content).ConfigureAwait(false);
        }

        // this is a mention of _moonPtr#8058 (276460831187664897)
        // I don't hate you, in fact I appreciate you breaking this stuff
        // but revenge is revenge
        // nothing personnel kid 😎
        [Group("<@!276460831187664897>"), Aliases("<@276460831187664897>"), Description("That's what you get for breaking my lib.")]
        public class Moon
        {
            [Command("test1")]
            public Task WhatTheHeck(CommandContext ctx)
                => ctx.RespondAsync("wewlad 0");

            [Command("test2")]
            public Task StopBreakingMyStuff(CommandContext ctx)
                => ctx.RespondAsync("wewlad 1");
        }

        // I am GLaDOS
        //[Command("test")]
        //public Task TestAsync(CommandContext ctx)
        //    => ctx.RespondAsync("It's been a loooooong time...");
    }
}
