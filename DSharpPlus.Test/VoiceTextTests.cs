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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace DSharpPlus.Test
{
    public class VoiceTextTests : BaseCommandModule
    {
        [Command("testvoice")]
        public async Task TestVoice(CommandContext ctx)
        {
            if (ctx.Member.VoiceState is null)
            {
                await ctx.RespondAsync("You're not in a voice channel!");
                return;
            }

            var channel = ctx.Member.VoiceState.Channel;

            var s = new Stack<string>();

            try
            {
                await channel.SendMessageAsync("Testing Guild Voice Text (1/5).");
                this.ChannelPassed(s, "content");
            }
            catch { this.ChannelFailed(s, "content"); }


            try
            {
                await channel.SendMessageAsync(new DiscordEmbedBuilder().WithTitle("Testing Guild Voice Text (2/5)"));
                this.ChannelPassed(s, "embed");
            }
            catch { this.ChannelFailed(s, "embed"); }

            try
            {
                await channel.SendMessageAsync("Testing Guild Voice Text (3/5)", new DiscordEmbedBuilder().WithTitle("Testing Guild Voice Text (3/5)"));
                this.ChannelPassed(s, "embed, content");
            }
            catch { this.ChannelFailed(s, "embed, content"); }

            try
            {
                await channel.SendMessageAsync(new DiscordMessageBuilder().WithContent("Testing Guild Voice Text (4/5)").WithEmbed(new DiscordEmbedBuilder().WithTitle("Testing Guild Voice Text (4/5)")));
                this.ChannelPassed(s, "embed, content");
            }
            catch { this.ChannelFailed(s, "builder"); }

            try
            {
                await channel.SendMessageAsync(b => b.WithContent("Testing Guild Voice Text (5/5)").WithEmbed(new DiscordEmbedBuilder().WithTitle("Testing Guild Voice Text (5/5)")));
                this.ChannelPassed(s, "Action<DiscordMessageBuilder>");
            }
            catch { this.ChannelFailed(s, "Action<DiscordMessageBuilder>"); }

            var sb = new StringBuilder();
            while (s.TryPop(out var res))
                sb.AppendLine(res);

            await ctx.RespondAsync(sb.ToString());
        }

        private void ChannelPassed(Stack<string> stack, string test) => stack.Push($"<:check:777724297627172884> {nameof(DiscordChannel.SendMessageAsync)} ({test}) **Passed**");

        private void ChannelFailed(Stack<string> stack, string test) => stack.Push($"<:cross:777724316115796011> {nameof(DiscordChannel.SendMessageAsync)} ({test}) **Failed**");
    }
}
