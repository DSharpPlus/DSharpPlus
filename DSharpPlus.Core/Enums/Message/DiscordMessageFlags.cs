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
    public enum DiscordMessageFlags
    {
        /// <summary>
        /// This message has been published to subscribed channels (via Channel Following).
        /// </summary>
        Crossposted = 1 << 0,

        /// <summary>
        /// This message originated from a message in another channel (via Channel Following).
        /// </summary>
        IsCrosspost = 1 << 1,

        /// <summary>
        /// Do not include any embeds when serializing this message.
        /// </summary>
        SupressEmbeds = 1 << 2,

        /// <summary>
        /// The source message for this crosspost has been deleted (via Channel Following).
        /// </summary>
        SourceMessageDeleted = 1 << 3,

        /// <summary>
        /// This message came from the urgent message system.
        /// </summary>
        Urgent = 1 << 4,

        /// <summary>
        /// This message has an associated thread, with the same id as the message.
        /// </summary>
        HasThread = 1 << 5,

        /// <summary>
        /// This message is only visible to the user who invoked the Interaction.
        /// </summary>
        Ephemeral = 1 << 6,

        /// <summary>
        /// This message is an Interaction Response and the bot is "thinking".
        /// </summary>
        Loading = 1 << 7,

        /// <summary>
        /// This message failed to mention some roles and add their members to the thread.
        /// </summary>
        FailedToMentionSomeRolesInThread = 1 << 8,
    }
}
