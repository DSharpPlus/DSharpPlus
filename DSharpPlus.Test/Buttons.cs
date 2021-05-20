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
                new DiscordButtonComponent(1, label: "Send poggies", customId: "emoji", emoji: new DiscordComponentEmoji {Id = 833475075474063421}),
                new DiscordButtonComponent(2, label: "Send poggies (but grey)", customId: "pog"),
                new DiscordButtonComponent(3, label: "Just ack (great for role menus!)", customId: "ack")
            );
            builder.WithContent("Buttons!");
            await ctx.RespondAsync(builder);
        }

        [Command]
        public async Task WaitForButton(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder().WithComponentRow(
                new DiscordButtonComponent(1, customId: "P_", emoji: new DiscordComponentEmoji {Id = 833475075474063421}),
                new DiscordButtonComponent(3, customId: "S_", emoji: new DiscordComponentEmoji {Id = 803713580385435700}),
                new DiscordButtonComponent(4, customId: "L_", emoji: new DiscordComponentEmoji {Id = 797806751617384459})
            );
            builder.WithContent("You have 30 seconds to pick an option!!");
            var msg = await ctx.RespondAsync(builder);

            var interactivity = ctx.Client.GetInteractivity();

            var result = await interactivity.WaitForButtonAsync(msg, "S_", TimeSpan.FromSeconds(30));

            if (result.TimedOut)
                await ctx.RespondAsync("Timed out!");
            else
            {
                await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = "Poggies", IsEphemeral = true
                });
            }
        }

        [Command]
        public async Task WaitForAnyButton(CommandContext ctx)
        {

        }
    }
}
