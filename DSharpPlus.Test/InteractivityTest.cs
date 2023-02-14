// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023

 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace DSharpPlus.Test
{
    public class InteractivityTest : BaseCommandModule
    {
        [Command("wait")]
        [Description("Interactivity test.")]
        public async Task WaitForCompsAsync(CommandContext ctx)
        {
            await ctx.RespondAsync(":warning: This is a test method, and will test interactivity methods. Please be ready to click. :warning:");

            var one = await ctx.RespondAsync(m => m.WithContent("**Button**: 1/6")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "button-one", "Push me")));
            var buttonRes = await one.WaitForButtonAsync();

            if (!buttonRes.TimedOut)
            {
                await buttonRes.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                await one.ModifyAsync("✅ WaitForButtonAsync() passed");
            }
            else
            {
                await one.ModifyAsync("❎ WaitForButtonAsync() failed");
            }

            var two = await ctx.RespondAsync(m => m.WithContent("**Button**: 2/6")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "button-two", "Push me")));

            buttonRes = await two.WaitForButtonAsync(ctx.User);

            if (!buttonRes.TimedOut)
            {
                await buttonRes.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                await two.ModifyAsync("✅ WaitForButtonAsync(DiscordUser) passed");
            }
            else
            {
                await two.ModifyAsync("❎ WaitForButtonAsync(DiscordUser) failed");
            }

            var three = await ctx.RespondAsync(m => m.WithContent("**Button**: 3/6")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "button-three", "Push me")));

            buttonRes = await three.WaitForButtonAsync(b => b.User == ctx.User, null);

            if (!buttonRes.TimedOut)
            {
                await buttonRes.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                await three.ModifyAsync("✅ WaitForButtonAsync(Func<ComponentInteractionCreateEventArgs, bool>) passed");
            }
            else
            {
                await three.ModifyAsync("❎ WaitForButtonAsync(Func<ComponentInteractionCreateEventArgs, bool>) failed");
            }


            var four = await ctx.RespondAsync(m => m.WithContent("**Button**: 4/6")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "button-four", "Push me")));

            buttonRes = await four.WaitForButtonAsync("button-four");

            if (!buttonRes.TimedOut)
            {
                await buttonRes.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                await four.ModifyAsync("✅ WaitForButtonAsync(string) passed");
            }
            else
            {
                await four.ModifyAsync("❎ WaitForButtonAsync(string) failed");
            }


            var five = await ctx.RespondAsync(m => m.WithContent("**Button**: 5/6")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "button-five", "Push me")));

            buttonRes = await ctx.Client.GetInteractivity().WaitForButtonAsync(five, new[] { new DiscordButtonComponent(ButtonStyle.Primary, "button-five", "Push me") }, null);

            if (!buttonRes.TimedOut)
            {
                await buttonRes.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                await five.ModifyAsync("✅ WaitForButtonAsync(IEnumerable<DiscordButtonComponent>) passed");
            }
            else
            {
                await five.ModifyAsync("❎ WaitForButtonAsync(IEnumerable<DiscordButtonComponent>) failed");
            }


            var six = await ctx.RespondAsync(m => m.WithContent("**Button**: 6/6")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "button-six", "Push me")));
            var seven = await  ctx.RespondAsync(m => m.WithContent("**Button**: 6/6")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "button-six", "Push me too")));

            buttonRes = await six.WaitForButtonAsync();
            var buttonRes2 = await seven.WaitForButtonAsync();

            if (!buttonRes.TimedOut && !buttonRes2.TimedOut)
            {
                await buttonRes.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                await buttonRes2.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                await buttonRes2.Result.Interaction.DeleteOriginalResponseAsync();

                await six.ModifyAsync("✅ WaitForButtonAsync(Func<ComponentInteractionCreateEventArgs, bool>) passed");
            }
            else
            {
                await six.ModifyAsync("❎ WaitForButtonAsync(Func<ComponentInteractionCreateEventArgs, bool>) failed");
                await seven.DeleteAsync();
            }
        }
    }
}
