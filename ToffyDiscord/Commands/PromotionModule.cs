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

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace ToffyDiscord.Commands;

public class PromotionModule : BaseCommandModule
{
    public class Promotion
    {

        private string _text;
        public string Text
        {
            get => this._text;
            set
            {
                this._text = value;
                this.IsEnabled = true;
            }
        }

        public bool IsEnabled { get; set; }


        public Promotion(string text)
        {
            this.Text = text;
            this.IsEnabled = false;
        }


    }

    public static Dictionary<ulong, Promotion> Promotions;


    public PromotionModule()
    {
        Promotions = new Dictionary<ulong, Promotion>();
    }

    [Command]
    public async Task PromoteAsync(CommandContext ctx, [RemainingText] string? text = null)
    {
        var serverId = ctx.Guild.Id;

        if (!string.IsNullOrEmpty(text))
        {
            if (Promotions.ContainsKey(serverId))
            {
                Promotions[serverId].Text = text;
                await ctx.PromotionResponseAsync("Ви підключили промо");
            }
            else
            {
                Promotions.Add(serverId, new Promotion(text));
                Promotions[serverId].IsEnabled = true;
                await ctx.PromotionResponseAsync("Ви підключили промо");
            }
        }
        else
        {
            Promotions[serverId].IsEnabled = false;
            await ctx.PromotionResponseAsync("Ви відключили промо");
        }
    }
}
