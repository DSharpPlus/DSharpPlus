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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DSharpPlus.Test
{
    public class Role_IconTest : BaseCommandModule
    {
        [Command]
        public async Task Fetch(CommandContext ctx, DiscordRole role)
        {
            await ctx.RespondAsync(
                $"Role: {role.Mention}\n" +
                $"Role has icon: {role.IconHash != null}\n" +
                $"Associated emoji: {role.Emoji}\n" +
                $"Role icon: {role.IconUrl ?? "Not applicable"}");
        }

        [Command]
        public async Task Create(CommandContext ctx, string name, DiscordColor color = default)
        {
            if (!ctx.Message.Attachments.Any())
            {
                await ctx.RespondAsync("Provide an image please");
                return;
            }

            new WebClient().DownloadFile(ctx.Message.Attachments.First().Url, "./icon.png");

            var stream = File.OpenRead("./icon.png");
            var role = await ctx.Guild.CreateRoleAsync(name, Permissions.None, color, icon: stream, emoji: DiscordEmoji.FromUnicode("👀"));

            await ctx.RespondAsync($"Created role! {role.Mention}");

            await ctx.Member.GrantRoleAsync(role);
        }

        [Command]
        public async Task Edit(CommandContext ctx, DiscordRole role)
        {
            if (!ctx.Message.Attachments.Any())
            {
                await ctx.RespondAsync("Provide an image please");
                return;
            }

            new WebClient().DownloadFile(ctx.Message.Attachments.First().Url, "./icon.png");

            var stream = File.OpenRead("./icon.png");
            await role.ModifyAsync(icon: stream);

            await ctx.RespondAsync($"Edited role! {role.Mention}");

        }

        [Command]
        public async Task Edit(CommandContext ctx, DiscordRole role, DiscordEmoji emoji)
        {
            await role.ModifyAsync(emoji: emoji);

            await ctx.RespondAsync($"Edited role! {role.Mention}");

        }
    }
}
