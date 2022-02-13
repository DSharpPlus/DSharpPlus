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
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace DSharpPlus.Test
{
    public class StickerTestCommands : BaseCommandModule
    {
        private static readonly HttpClient _client = new HttpClient();

        [Command]
        public async Task CreateStickerAsync(CommandContext ctx)
        {
            var mref = ctx.Message.ReferencedMessage;

            if (mref is null)
            {
                await ctx.RespondAsync("How to use this command: Reply to a message with an image that's exactly 320x320");
                return;
            }

            if (mref.Attachments.Count is 0 || !mref.Attachments[0].FileName.EndsWith("png"))
            {
                await ctx.RespondAsync("You must reply to a message with an image (e.g. my_sticker.png)");
                return;
            }

            var ms = new MemoryStream(await _client.GetByteArrayAsync(mref.Attachments[0].Url));

            try
            {
                await ctx.Guild.CreateStickerAsync("sticker!", "A sticker", "✔", ms, StickerFormat.PNG);
            }
            catch (BadRequestException e)
            {
                await ctx.RespondAsync(e.JsonMessage);
            }

        }

        [Command("send_sticker")]
        public async Task SendStickerAsync(CommandContext ctx)
        {
            if (ctx.Message.Stickers.Count() is 0)
            {
                await ctx.RespondAsync("Send a sticker!");
                return;
            }

            var str = ctx.Message.Stickers.First();

            if (!ctx.Guild.Stickers.TryGetValue(str.Id, out _))
            {
                await ctx.RespondAsync("Send a sticker from this guild!");
                return;
            }

            var builder = new DiscordMessageBuilder();
            builder.Sticker = str;

            await ctx.RespondAsync(builder);
        }
    }
}
