using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;

namespace DSharpPlus.Test
{
    public class TestBotNextCommands
    {
        [Command("hello"), Aliases("hi", "say_hello", "say_hi"), Description("Says hello to given user.")]
        public async Task SayHello(CommandContext ctx, [Description("Name to say hi to.")] string name)
        {
            await ctx.RespondAsync($"Hello, {name}!");
        }

        [Command("pingme"), Aliases("mentionme"), Description("Mentions the executor.")]
        public async Task PingMe(CommandContext ctx)
        {
            await ctx.RespondAsync($"{ctx.User.Mention}");
        }

        [Command("ping"), Aliases("mention"), Description("Mentions specified user.")]
        public async Task Ping(CommandContext ctx, [Description("Member to mention.")] DiscordMember member)
        {
            await ctx.RespondAsync($"{member.Mention}");
        }

        [Command("echo"), Description("Echoes supplied numbers.")]
        public async Task Echo(CommandContext ctx, [Description("Numbers to echo back.")] params int[] numbers)
        {
            await ctx.RespondAsync(string.Concat("Supplied numbers: ", string.Join(", ", numbers)));
        }

        [Command("unixtime"), Description("Converts a unix timestamp to printable date time."), Hidden]
        public async Task PrintUnixTimestamp(CommandContext ctx, long timestamp = 1492712905)
        {
            var dto = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(timestamp);
            await ctx.RespondAsync($"{dto.ToString("yyyy-MM-dd HH:mm:ss zzz")}");
        }

        [Command("dice"), Aliases("roll"), Description("Rolls dice.")]
        public async Task Roll(CommandContext ctx, [Description("Number of sides the dice have.")] int sides, [Description("Number of times to roll.")] int rolls)
        {
            await ctx.Channel.TriggerTypingAsync();

            if (sides > 1 && sides < 256 && rolls > 0)
            {
                var drs = new byte[rolls];
                using (var rng = RandomNumberGenerator.Create())
                    rng.GetBytes(drs);

                for (var i = 0; i < rolls; i++)
                    drs[i] = (byte)((drs[i] % sides) + 1);

                var ans = $"{ctx.Member.Mention} Rolled {drs.Sum(xb => xb)}: {string.Join(", ", drs)}";
                await ctx.RespondAsync(ans);
            }
            else
                await ctx.RespondAsync("ill fukken bash ur head in i swer on me mum");
        }

        [Group("interactive"), Aliases("int", "interact", "interactivity"), Description("Interactivity commands."), SimpleCanTest]
        public class InteractivityTest
        {
            [Command("collect"), Aliases("reactions"), Description("Collects reactions over given period of time.")]
            public async Task CollectReactions(CommandContext ctx, [Description("How long to collect reactions for")] TimeSpan timeout)
            {
                var m = ctx.Client.GetInteractivityModule();

                var msg = await ctx.RespondAsync($"m88 react spam here (timeout {timeout})");
                var r = await m.CollectReactionsAsync(msg, timeout);

                var embed = new DiscordEmbed
                {
                    Color = 0x7F00FF,
                    Description = string.Join("\n", r.Select(xkvp => $"{xkvp.Key}: {xkvp.Value}"))
                };
                await msg.RespondAsync("", embed: embed);
            }
        }

        [Group("sub"), Aliases("submodule"), CanExecute, Description("Copypasta things."), Hidden]
        public class SubGroup
        {
            [Command("navyseal"), Aliases("navy_seal", "copypasta"), Description("Prints a modified Navy Seal copypasta."), SimpleCanTest]
            public async Task NavySeal(CommandContext ctx)
            {
                await ctx.RespondAsync("What the fuck did you just fucking say about me, you little bitch? I'll have you know I graduated top of my class on GitHub, and I've been involved in numerous pull requests for DSharpPlus, and I have over 30 confirmed commits. I am trained in C# programming and I'm the top coder in the Discord API. You are nothing to me but just another whitey. I will rewrite you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet? Think again, fucker. As we speak I am contacting my secret network of DAPI mods across the USA and your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing you call your lib. You're fucking dead, kid. I can code anywhere, anytime, and I can commit in over seven hundred ways, and that's just with my laptop. Not only am I extensively trained in using Visual Studio, but I have access to the entire toolchain of the .NET Framework and I will use it to its full extent to rewrite your miserable lib off the face of the continent, you little shit. If only you could have known what unholy retribution your little \"clever\" comment was about to bring down upon you, maybe you would have held your fucking tongue. But you couldn't, you didn't, and now you're paying the price, you goddamn idiot. I will commit fury all over you and you will drown in it. You're fucking dead, kiddo.");
            }

            [Command("pengu1n"), Aliases("penguin"), Description("Katy t3h PeNgU1N oF d00m copypasta."), Hidden]
            public async Task PenguinOfDoom(CommandContext ctx)
            {
                await ctx.RespondAsync("hi every1 im new!!!!!!! holds up spork my name is katy but u can call me t3h PeNgU1N oF d00m!!!!!!!! lol…as u can see im very random!!!! thats why i came here, 2 meet random ppl like me _… im 13 years old (im mature 4 my age tho!!) i like 2 watch invader zim w/ my girlfreind (im bi if u dont like it deal w/it) its our favorite tv show!!! bcuz its SOOOO random!!!! shes random 2 of course but i want 2 meet more random ppl =) like they say the more the merrier!!!! lol…neways i hope 2 make alot of freinds here so give me lots of commentses!!!!\nDOOOOOMMMM!!!!!!!!!!!!!!!! <--- me bein random again _^ hehe…toodles!!!!!\n\nlove and waffles,\n\nt3h PeNgU1N oF d00m");
            }

            public async Task ModuleCommand(CommandContext ctx)
            {
                await ctx.RespondAsync("I'm a proud ethnic Kekistani. For centuries my people bled under Normie oppression. But no more. We have suffered enough under your Social Media Tyranny. It is time to strike back. I hereby declare a meme jihad on all Normies. Normies, GET OUT! RRRÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆ﻿");
            }
        }

        [Group("actualtesting"), Aliases("testing"), Description("This is for actually testing the bot.")]
        public class ActualTesting
        {
            [Command("test_me_senpai"), Description("S-senpai...")]
            public async Task TestMeSenpai(CommandContext ctx, [Description("...please be gentle.")] params string[] filename)
            {
                var a = typeof(ActualTesting).GetTypeInfo().Assembly;
                var l = a.Location;
                var p1 = Path.GetDirectoryName(l);
                var p2 = string.Join(" ", filename);

                var p = Path.Combine(p1, p2);
                using (var fs = File.OpenRead(p))
                    await ctx.RespondAsync("", fs, Path.GetFileName(p));
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class SimpleCanTestAttribute : ConditionBaseAttribute
    {
        private static readonly ulong[] specs = new[] { 181875147148361728u, 127408598010560513u };

        public override Task<bool> CanExecute(CommandContext ctx)
        {
            if (specs.Contains(ctx.User.Id))
                return Task.FromResult(true);
            return Task.FromResult(false);
        }
    }
}
