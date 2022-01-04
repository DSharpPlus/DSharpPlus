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
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Fired when a new scheduled guild event is created.
    /// </summary>
    public class ScheduledGuildEventCreateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// The guild this event is scheduled for.
        /// </summary>
        public DiscordGuild Guild => this.Event.Guild;

        /// <summary>
        /// The channel this event is scheduled for, if applicable.
        /// </summary>
        public DiscordChannel Channel => this.Event.Channel;

        /// <summary>
        /// The user that created the event.
        /// </summary>
        public DiscordUser Creator => this.Event.Creator;

        /// <summary>
        /// The event that was created.
        /// </summary>
        public DiscordScheduledGuildEvent Event { get; internal set; }

        internal ScheduledGuildEventCreateEventArgs() : base() { }
    }
}
