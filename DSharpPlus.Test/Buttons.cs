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
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;


namespace DSharpPlus.Test
{
    public class Buttons : BaseCommandModule
    {
        [Command("send_buttons")]
        public async Task SendbuttonsAsync(CommandContext ctx)
        {
            var lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer tellus lectus, tristique at turpis nec, ultrices vestibulum mi. Praesent aliquam et neque sit amet suscipit. Nullam nec pulvinar leo. Integer pharetra mauris ac imperdiet vestibulum. Maecenas eu tellus at nisi bibendum pharetra a nec ligula. Curabitur euismod est sem, non tempus felis varius eu. Duis molestie quis ante sed elementum. Suspendisse in diam bibendum, cursus metus vel, imperdiet dolor. Etiam rutrum, justo sed vehicula ultrices, massa augue mollis odio, sed placerat diam orci in erat. Sed pulvinar felis eget lacus imperdiet fringilla. Nunc blandit, orci quis varius varius, nibh diam scelerisque dolor, a cursus ex dui non diam. Etiam a erat eros. Nulla porttitor venenatis ligula, ac tincidunt mauris auctor quis. Vivamus ut gravida urna, eu finibus enim. Quisque vitae vestibulum metus. Vestibulum non leo ut odio pharetra mattis." +
                         "Donec volutpat condimentum velit. Aenean tincidunt massa eu malesuada aliquet. Suspendisse potenti. Nulla porttitor vel sem et pretium. Vestibulum tempus tortor lectus, aliquet tempus risus condimentum vel. Quisque ultricies dapibus lacus, nec mollis enim efficitur in. Aliquam laoreet ultricies lorem, quis hendrerit elit pellentesque at. Cras sit amet dignissim velit. Suspendisse vulputate aliquam faucibus. Morbi dictum magna gravida diam vehicula convallis. Sed egestas nulla lectus, id cursus libero pharetra id. Aliquam fermentum gravida dictum. Curabitur aliquam diam tortor.";

            var interactivity = ctx.Client.GetInteractivity();
            var embedPages = interactivity.GeneratePagesInEmbed(lipsum);
            var pages = interactivity.GeneratePagesInContent(lipsum, SplitType.Character);

            await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, embedPages, token: new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token);
        }

    }
}
