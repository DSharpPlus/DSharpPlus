// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace DSharpPlus.Test;

public class SelectTests : BaseCommandModule
{
    [Command("send_select")]
    public async Task SendSelectAsync(CommandContext ctx)
        => await ctx.RespondAsync("This is missing an implementation!");

    [Command("Select_Interactive_Test_1")]
    [Description("A test for select menus. This waits for one input.")]
    public async Task Select_Interactive_Test_1_Async(CommandContext ctx)
    {
        var input = ctx.Client.GetInteractivity();
        var builder = new DiscordMessageBuilder();
        builder.WithContent("This is a test! StringSelect is valid for 30 seconds.");

        var opts = new[]
        {
            new DiscordSelectComponentOption("Label 1", "the first option", emoji: new DiscordComponentEmoji("⬜")),
            new DiscordSelectComponentOption("Label 2", "the second option", emoji: new DiscordComponentEmoji("🟦")),
            new DiscordSelectComponentOption("Label 3", "the third option", emoji: new DiscordComponentEmoji("⬛")),
        };

        var select = new DiscordSelectComponent("yert", "Dropdowns!", opts, true, 0, 1);

        var btn1 = new DiscordButtonComponent(ButtonStyle.Primary, "no1", "Button 1!", true);
        var btn2 = new DiscordButtonComponent(ButtonStyle.Secondary, "no2", "Button 2!", true);
        var btn3 = new DiscordButtonComponent(ButtonStyle.Success, "no3", "Button 3!", true);

        builder.AddComponents(btn1, btn2, btn3);
        builder.AddComponents(select);

        var msg = await builder.SendAsync(ctx.Channel);
        var res = await input.WaitForSelectAsync(msg, "yert", TimeSpan.FromSeconds(30));

        if (res.TimedOut)
            await ctx.RespondAsync("Sorry but it timed out!");
        else
            await ctx.RespondAsync($"You selected {string.Join(", ", res.Result.Values)}");
    }

    [Command("Select_Interactive_Test_2")]
    [Description("A test for select menus. This waits for two inputs.")]
    public async Task Select_Interactive_Test_2_Async(CommandContext ctx)
    {
        var input = ctx.Client.GetInteractivity();
        var builder = new DiscordMessageBuilder();
        builder.WithContent("This is a test! StringSelect is valid for 30 seconds.");

        var opts = new[]
        {
            new DiscordSelectComponentOption("Label 1", "the first option", emoji: new DiscordComponentEmoji("⬜")),
            new DiscordSelectComponentOption("Label 2", "the second option", emoji: new DiscordComponentEmoji("🟦")),
            new DiscordSelectComponentOption("Label 3", "the third option", emoji: new DiscordComponentEmoji("⬛")),
        };

        var select = new DiscordSelectComponent("yert", "Dropdowns!", opts, false);

        var btn1 = new DiscordButtonComponent(ButtonStyle.Primary, "no1", "Button 1!", true);
        var btn2 = new DiscordButtonComponent(ButtonStyle.Secondary, "no2", "Button 2!", true);
        var btn3 = new DiscordButtonComponent(ButtonStyle.Success, "no3", "Button 3!", true);

        builder.AddComponents(btn1, btn2, btn3);
        builder.AddComponents(select);

        var msg = await builder.SendAsync(ctx.Channel);
    wait:
        var res = await input.WaitForSelectAsync(msg, "yert", TimeSpan.FromSeconds(30));

        if (res.TimedOut)
            await ctx.RespondAsync("Sorry but it timed out!");
        else if (res.Result.Values.Length != 2)
            goto wait; // I'm lazy. A while(true) or while (res?.Result.Values.Length != 2 ?? false) would be better. This duplicates messages.
                       // Velvet, why would you do this? You're scaring the children! - Lunar
        else
            await ctx.RespondAsync($"You selected {string.Join(", ", res.Result.Values)}");
    }
}
