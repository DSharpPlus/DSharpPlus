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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity.EventHandling
{
    public class PaginationButtons
    {
        public DiscordButtonComponent SkipLeft { internal get; set; }
        public DiscordButtonComponent Left { internal get; set; }
        public DiscordButtonComponent Stop { internal get; set; }
        public DiscordButtonComponent Right { internal get; set; }
        public DiscordButtonComponent SkipRight { internal get; set; }

        internal DiscordButtonComponent[] ButtonArray => new[] // This isn't great but I can't figure out how to pass these elements by ref :(
        {                                                      // And yes, it should be by ref to begin with, but in testing it refuses to update.
            this.SkipLeft,                                     // So I have no idea what that's about, and this array is "cheap-enough" and infrequent
            this.Left,                                         // enough to the point that it *should* be fine.
            this.Stop,
            this.Right,
            this.SkipRight
        };

        public PaginationButtons()
        {
            this.SkipLeft = new(ButtonStyle.Secondary, "leftskip", null, false, new(DiscordEmoji.FromUnicode("⏮")));
            this.Left = new(ButtonStyle.Secondary, "left", null, false, new(DiscordEmoji.FromUnicode("◀")));
            this.Stop = new(ButtonStyle.Secondary, "stop", null, false, new(DiscordEmoji.FromUnicode("⏹")));
            this.Right = new(ButtonStyle.Secondary, "right", null, false, new(DiscordEmoji.FromUnicode("▶")));
            this.SkipRight = new(ButtonStyle.Secondary, "rightskip", null, false, new(DiscordEmoji.FromUnicode("⏭")));
        }

        public PaginationButtons(PaginationButtons other)
        {
            this.Stop = new(other.Stop);
            this.Left = new(other.Left);
            this.Right = new(other.Right);
            this.SkipLeft = new(other.SkipLeft);
            this.SkipRight = new(other.SkipRight);
        }
    }
}
