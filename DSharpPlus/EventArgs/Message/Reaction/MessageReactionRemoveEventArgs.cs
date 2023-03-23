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

using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.MessageReactionRemoved"/> event.
    /// </summary>
    public class MessageReactionRemoveEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the message for which the update occurred.
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// Gets the channel to which this message belongs.
        /// </summary>
        /// <remarks>
        /// This will be <c>null</c> for an uncached channel, which will usually happen for when this event triggers on
        /// DM channels in which no prior messages were received or sent.
        /// </remarks>
        public DiscordChannel Channel
            => this.Message.Channel;

        /// <summary>
        /// Gets the users whose reaction was removed.
        /// </summary>
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the guild in which the reaction was deleted.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets the emoji used for this reaction.
        /// </summary>
        public DiscordEmoji Emoji { get; internal set; }

        internal MessageReactionRemoveEventArgs() : base() { }
    }
}
