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

namespace DSharpPlus.Core.Enums
{
    public enum DiscordActivityType
    {
        /// <summary>
        /// Format: Playing {name}
        /// Example: "Playing Rocket League"
        /// </summary>
        Game = 0,

        /// <summary>
        /// Format: Streaming {details}
        /// Example: "Streaming Rocket League"
        /// </summary>
        /// <remarks>
        /// The streaming type currently only supports Twitch and YouTube. Only <c>https://twitch.tv/</c> and <c>https://youtube.com/</c> urls will work.
        /// </remarks>
        Streaming = 1,

        /// <summary>
        /// Format: Listening to {name}
        /// Example: "Listening to Spotify"
        /// </summary>
        Listening = 2,

        /// <summary>
        /// Format: Watching {name}
        /// Example: "Watching YouTube Together"
        /// </summary>
        Watching = 3,

        /// <summary>
        /// Format: {emoji} {name}
        /// Example: ":smiley: I am cool"
        /// </summary>
        Custom = 4,

        /// <summary>
        /// Format: Competing in {name}
        /// Example: "Competing in Arena World Champions"
        /// </summary>
        Competing = 5
    }
}
