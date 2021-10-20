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

namespace DSharpPlus
{
    public static class SystemChannelFlagsExtension
    {
        /// <summary>
        /// Calculates whether these system channel flags contain a specific flag.
        /// </summary>
        /// <param name="baseFlags">The existing flags.</param>
        /// <param name="flag">The flag to search for.</param>
        /// <returns></returns>
        public static bool HasSystemChannelFlag(this SystemChannelFlags baseFlags, SystemChannelFlags flag) => (baseFlags & flag) == flag;
    }

    /// <summary>
    /// Represents settings for a guild's system channel.
    /// </summary>
    [Flags]
    public enum SystemChannelFlags
    {
        /// <summary>
        /// Member join messages are disabled.
        /// </summary>
        SuppressJoinNotifications = 1 << 0,

        /// <summary>
        /// Server boost messages are disabled.
        /// </summary>
        SuppressPremiumSubscriptions = 1 << 1,

        /// <summary>
        /// Server setup tips are disabled.
        /// </summary>
        SuppressGuildReminderNotifications = 1 << 2,

        /// <summary>
        /// Server join messages supress wave CTA button.
        /// </summary>
        SuppressJoinNotificationReplies = 1 << 3
    }
}
