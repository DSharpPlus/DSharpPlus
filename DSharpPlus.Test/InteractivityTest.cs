// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using System;
using System.Threading;
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
        public async Task WaitForCompsAsync(CommandContext ctx)
        {
            var comps = new DiscordMessageBuilder()
                .WithContent("** **")
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "button", "Push me"))
                .AddComponents(new DiscordSelectComponent("select", "Context menus coming soon", new DiscordSelectComponentOption[]
                {
                    new DiscordSelectComponentOption("1", "one", "The first"),
                    new DiscordSelectComponentOption("2", "two", "The second"),
                }));

            var msg = await comps.SendAsync(ctx.Channel);

            var itv = ctx.Client.GetInteractivity();
            var cts = new CancellationTokenSource();

        var one = itv.WaitForButtonAsync(msg, "button", cts.Token);
        var two = itv.WaitForSelectAsync(msg, "select", cts.Token);

        var task = await await Task.WhenAny(one, two);
        await task.Result?.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (task.TimedOut)
                await ctx.RespondAsync("Both timed out, sorry!");
            else
                await ctx.RespondAsync($"You picked the {task.Result.Id}");
        }
    }
}
