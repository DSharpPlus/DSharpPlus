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
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;


namespace DSharpPlus.Test
{
    public class Buttons : BaseCommandModule
    {
        [Command]
        public async Task Sendbuttons(CommandContext ctx)
        {
            var p = new DiscordButtonComponent(ButtonStyle.Primary, "P_", "Blurple", emoji: new DiscordComponentEmoji( 833475075474063421));
            var c = new DiscordButtonComponent(ButtonStyle.Secondary, "C_", "Grey", emoji: new DiscordComponentEmoji(833475015114358854));
            var b = new DiscordButtonComponent(ButtonStyle.Success, "B_", "Green", emoji: new DiscordComponentEmoji(831306677449785394));
            var y = new DiscordButtonComponent(ButtonStyle.Danger, "Y_", "Red", emoji: new DiscordComponentEmoji(833886629792972860));
            var z = new DiscordLinkButtonComponent("https://velvetthepanda.dev", "Link", false, new DiscordComponentEmoji(826108356656758794));

            var d1 = new DiscordButtonComponent(ButtonStyle.Primary, "disabled", "and", true);
            var d2 = new DiscordButtonComponent(ButtonStyle.Secondary, "disabled2", "these", true);
            var d3 = new DiscordButtonComponent(ButtonStyle.Success, "disabled3", "are", true);
            var d4 = new DiscordButtonComponent(ButtonStyle.Danger, "disabled4", "disabled~!", true);


            var builder = new DiscordMessageBuilder();

            builder
                .WithContent("Buttons!")
                .AddComponents(p)
                .AddComponents(c, b)
                .AddComponents(y, z)
                .AddComponents(d1, d2, d3, d4);

            await builder.SendAsync(ctx.Channel);

        }

    }
}
