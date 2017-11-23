using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DSharpPlus.Test
{
    public class TestBotCommands
    {
        public static ConcurrentDictionary<ulong, string> Prefixes { get; } = new ConcurrentDictionary<ulong, string>();

        [Command("setprefix"), Aliases("channelprefix"), Description("Sets custom command prefix for current channel. The bot will still respond to the default one."), RequireOwner]
        public async Task SetPrefixAsync(CommandContext ctx, [Description("The prefix to use for current channel.")] string prefix = null)
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

        [Command("sudo"), Description("Run a command as another user."), Hidden, RequireOwner]
        public async Task SudoAsync(CommandContext ctx, DiscordUser user, [RemainingText] string content)
        {
            await ctx.Client.GetCommandsNext().SudoAsync(user, ctx.Channel, content).ConfigureAwait(false);
        }

        [Group("bind"), Description("Various argument binder testing commands.")]
        public class Binding
        {
            [Command("user"), Description("Attempts to get a user.")]
            public Task UserAsync(CommandContext ctx, DiscordUser usr = null)
                => ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(usr?.Mention ?? "<null>"));

            [Command("member"), Description("Attempts to get a member.")]
            public Task MemberAsync(CommandContext ctx, DiscordMember mbr = null)
                => ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(mbr?.Mention ?? "<null>"));

            [Command("role"), Description("Attempts to get a role.")]
            public Task RoleAsync(CommandContext ctx, DiscordRole rol = null)
                => ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(rol?.Mention ?? "<null>"));

            [Command("channel"), Description("Attempts to get a channel.")]
            public Task ChannelAsync(CommandContext ctx, DiscordChannel chn = null)
                => ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(chn?.Mention ?? "<null>"));

            [Command("guild"), Description("Attempts to get a guild.")]
            public Task GuildAsync(CommandContext ctx, DiscordGuild gld = null)
                => ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(gld?.Name ?? "<null>"));

            [Command("emote"), Description("Attempts to get an emoji.")]
            public Task EmoteAsync(CommandContext ctx, DiscordEmoji emt = null)
                => ctx.RespondAsync(embed: new DiscordEmbedBuilder().WithDescription(emt?.ToString() ?? "<null>"));

            [Command("string"), Description("Attempts to bind a string.")]
            public Task StringAsync(CommandContext ctx, string s = null)
                => ctx.RespondAsync(s ?? "<null>");

            [Command("bool"), Description("Attempts to bind a boolean.")]
            public Task BoolAsync(CommandContext ctx, bool b)
                => ctx.RespondAsync($"{b}");
        }
        
        // this is a mention of _moonPtr#8058 (276460831187664897)
        // I don't hate you, in fact I appreciate you breaking this stuff
        // but revenge is revenge
        // nothing personnel kid 😎
        [Group("<@!276460831187664897>"), Aliases("<@276460831187664897>"), Description("That's what you get for breaking my christian lib.")]
        public class Moon
        {
            [Command("test1")]
            public Task WhatTheHeck(CommandContext ctx)
                => ctx.RespondAsync("wewlad 0");

            [Command("test2")]
            public Task StopBreakingMyStuff(CommandContext ctx)
                => ctx.RespondAsync("wewlad 1");
        }
    }

    public class TestBotService
    {
        public int CommandCounter => this._cmd_counter;
        private volatile int _cmd_counter;

        public TestBotService()
        {
            this._cmd_counter = 0;
        }

        public void InrementUseCount()
        {
            Interlocked.Increment(ref this._cmd_counter);
        }
    }
}
