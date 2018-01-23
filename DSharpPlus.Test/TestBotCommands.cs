﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DSharpPlus.Test
{
    public class TestBotCommands
    {
        public static ConcurrentDictionary<ulong, string> PrefixSettings { get; } = new ConcurrentDictionary<ulong, string>();

        [Command("testcreateow")]
        public async Task TestCreateOwAsync(CommandContext ctx)
        {
            List<DiscordOverwriteBuilder> dowbs = new List<DiscordOverwriteBuilder>();
            dowbs.Add(new DiscordOverwriteBuilder()
                .Allow(Permissions.ManageChannels)
                .Deny(Permissions.ManageMessages)
                .ForId(ctx.Member.Id)
                .WithType(OverwriteType.Member));

            await ctx.Guild.CreateTextChannelAsync("memes", overwrites: dowbs);
            await ctx.RespondAsync("naam check your shitcode");
        }

        [Command("testmodify")]
        public async Task TestModifyAsync(CommandContext ctx, DiscordMember m)
        {
            await ctx.Channel.ModifyAsync(x =>
            {
                x.Name = "poopies_and_peepees";
                x.Topic = "Childish stuff";
                x.AuditLogReason = "Just because..";
            });

            await ctx.Guild.ModifyAsync(x =>
            {
                x.Name = "House of memes";
                x.AuditLogReason = "This is our name now.";
            });

            await m.ModifyAsync(x =>
            {
                x.Nickname = "Lord of the memes";
                x.AuditLogReason = "He owns u nao";
            });

            await ctx.RespondAsync($"You are now the lord of memes, {m.Mention}. Here in the house of memes. In the channel of poopies and peepees.");
        }

        [Command("setprefix"), Aliases("channelprefix"), Description("Sets custom command prefix for current channel. The bot will still respond to the default one."), RequireOwner]
        public async Task SetPrefixAsync(CommandContext ctx, [Description("The prefix to use for current channel.")] string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                if (PrefixSettings.TryRemove(ctx.Channel.Id, out _))
                    await ctx.RespondAsync("👍").ConfigureAwait(false);
                else
                    await ctx.RespondAsync("👎").ConfigureAwait(false);
            else
            {
                PrefixSettings.AddOrUpdate(ctx.Channel.Id, prefix, (k, vold) => prefix);
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

            [Command("nullable"), Description("Attempts to bind a nullable integer.")]
            public Task NullableAsync(CommandContext ctx, int? x = 4)
                => ctx.RespondAsync(x?.ToString("#,##0") ?? "<null>");

            //[Command("enum"), Description("Attempts to bind an enum value.")]
            //public Task EnumAsync(CommandContext ctx, TestEnum? te = null)
            //    => ctx.RespondAsync(te?.ToString() ?? "<null>");

            //public enum TestEnum
            //{
            //    String,
            //    Integer
            //}
        }

        [Group]
        public class ImplicitGroup
        {
            [Command]
            public Task ImplicitAsync(CommandContext ctx)
                => ctx.RespondAsync("Hello from trimmed name!");

            [Command]
            public Task Another(CommandContext ctx)
                => ctx.RespondAsync("Hello from untrimmed name!");
        }

        [Group]
        public class Prefixes
        {
            [Command, RequirePrefixes("<<", ShowInHelp = true)]
            public Task PrefixShown(CommandContext ctx)
                => ctx.RespondAsync("Hello from shown prefix.");

            [Command, RequirePrefixes("<<", ShowInHelp = false)]
            public Task PrefixHidden(CommandContext ctx)
                => ctx.RespondAsync("Hello from hidden prefix.");
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
