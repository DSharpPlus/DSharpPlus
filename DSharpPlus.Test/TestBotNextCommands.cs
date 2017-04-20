﻿using System.Threading.Tasks;
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

        [Group("sub"), Aliases("submodule"), CanExecute, Description("Copypasta things.")]
        public class SubGroup
        {
            [Command("navyseal"), Aliases("navy_seal", "copypasta"), Description("Prints a modified Navy Seal copypasta.")]
            public async Task NavySeal(CommandContext ctx)
            {
                await ctx.RespondAsync("What the fuck did you just fucking say about me, you little bitch? I'll have you know I graduated top of my class on GitHub, and I've been involved in numerous pull requests for DSharpPlus, and I have over 30 confirmed commits. I am trained in C# programming and I'm the top coder in the Discord API. You are nothing to me but just another whitey. I will rewrite you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet? Think again, fucker. As we speak I am contacting my secret network of DAPI mods across the USA and your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing you call your lib. You're fucking dead, kid. I can code anywhere, anytime, and I can commit in over seven hundred ways, and that's just with my laptop. Not only am I extensively trained in using Visual Studio, but I have access to the entire toolchain of the .NET Framework and I will use it to its full extent to rewrite your miserable lib off the face of the continent, you little shit. If only you could have known what unholy retribution your little \"clever\" comment was about to bring down upon you, maybe you would have held your fucking tongue. But you couldn't, you didn't, and now you're paying the price, you goddamn idiot. I will commit fury all over you and you will drown in it. You're fucking dead, kiddo.");
            }

            public async Task ModuleCommand(CommandContext ctx)
            {
                await ctx.RespondAsync("I'm a proud ethnic Kekistani. For centuries my people bled under Normie oppression. But no more. We have suffered enough under your Social Media Tyranny. It is time to strike back. I hereby declare a meme jihad on all Normies. Normies, GET OUT! RRRÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆÆ﻿");
            }
        }
    }
}
