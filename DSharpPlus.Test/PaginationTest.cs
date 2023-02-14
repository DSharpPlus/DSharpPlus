// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;

namespace DSharpPlus.Test
{
    public class PaginationTest : BaseCommandModule
    {
        private const string Lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce mattis turpis dapibus risus bibendum condimentum. Mauris ut dictum arcu. Nullam id ante nisl. Morbi lobortis nisi dignissim nisl pulvinar, eu suscipit augue pellentesque. Sed vehicula quam magna, id lobortis diam condimentum sed. Sed in mi felis. Cras sodales dui purus, sed gravida nulla venenatis ac. Aliquam venenatis pharetra tellus, eget pretium felis ultricies ac. Donec a tristique lacus, ac vulputate tellus. Praesent condimentum fringilla venenatis. Nam quis dui ut ante scelerisque scelerisque ac dictum sem. Donec non tristique ex. Quisque iaculis felis non tincidunt finibus. Praesent pellentesque sit amet tortor sit amet viverra. Aenean vestibulum est sit amet mauris faucibus sodales. Praesent tristique lacus at lorem consectetur, ut accumsan lorem tincidunt." +
                                      "Pellentesque ipsum magna, laoreet eu erat id, sagittis posuere nulla. Nulla suscipit dui luctus lorem consectetur, et hendrerit est cursus. Integer malesuada, mi et viverra sagittis, urna enim finibus eros, sed vestibulum ipsum leo ut massa. In lacinia risus in commodo pretium. Interdum et malesuada fames ac ante ipsum primis in faucibus. Donec eu pulvinar dolor. Nullam molestie, lectus at tincidunt laoreet, nunc magna accumsan nibh, ac faucibus erat enim vel lorem." +
                                      "" +
                                      "" +
                                      "";

        [Command("paginate")]
        public async Task PaginateAsync(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder().WithContent("** **").AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "a", "Paginate"));
            await ctx.RespondAsync(builder);

            ctx.Client.ComponentInteractionCreated += this.Handle;
        }

        private Task Handle(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            if (e.Id != "a")
                return Task.CompletedTask;
            var pages = sender.GetInteractivity().GeneratePagesInContent(Lorem);
            _ = sender.GetInteractivity().SendPaginatedResponseAsync(e.Interaction, true, e.User, pages);
            sender.ComponentInteractionCreated -= this.Handle;
            return Task.CompletedTask;
        }
    }
}
