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
using DSharpPlus.Interactivity.Extensions;


namespace DSharpPlus.Test
{
    public class Buttons : BaseCommandModule
    {
        [Command]
        public async Task SendButton(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder().WithComponentRow(
                new DiscordButtonComponent(ButtonStyle.Primary, label: "Send poggies", customId: "emoji", emoji: new DiscordComponentEmoji {Id = 833475075474063421}),
                new DiscordButtonComponent(ButtonStyle.Secondary, label: "Send poggies (but grey)", customId: "pog"),
                new DiscordButtonComponent(ButtonStyle.Success, label: "Just ack (great for role menus!)", customId: "ack")
            );
            builder.WithContent("Buttons!");
            await ctx.RespondAsync(builder);
        }

        [Command]
        public async Task WaitForButton(CommandContext ctx)
        {
            var p = new DiscordButtonComponent(ButtonStyle.Primary, "P_", emoji: new DiscordComponentEmoji {Id = 833475075474063421});
            var s = new DiscordButtonComponent(ButtonStyle.Success, "S_", emoji: new DiscordComponentEmoji {Id = 803713580385435700});
            var l = new DiscordButtonComponent(ButtonStyle.Danger, "L_", emoji: new DiscordComponentEmoji {Id = 797806751617384459});
            var builder = new DiscordMessageBuilder().WithComponentRow(p, s, l);

            builder.WithContent("You have 30 seconds to pick the right option!");

            var msg = await builder.SendAsync(ctx.Channel);

            var interactivity = ctx.Client.GetInteractivity();

            var result = await interactivity.WaitForButtonAsync(msg, "S_", TimeSpan.FromSeconds(30));

            if (result.TimedOut)
                await ctx.RespondAsync("Timed out!");
            else
                await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = "Poggies!",
                    IsEphemeral = true
                });
        }

        [Command]
        public async Task WaitForAnyButton(CommandContext ctx)
        {

        }

        private static void RemoveComponent(DiscordMessageBuilder builder, DiscordComponent comp)
        {
            foreach (var ar in builder.Components)
                ar.Components.Remove(comp);
        }
    }
}
