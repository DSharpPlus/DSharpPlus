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


namespace DSharpPlus
{
    /// <summary>
    /// Represents flags for a discord application.
    /// </summary>
    public enum ApplicationFlags
    {
        /// <summary>
        /// Indicates that the application is approved for the <see cref="DiscordIntents.GuildPresences"/> intent.
        /// </summary>
        GatewayPresence = 1 << 12,

        /// <summary>
        /// Indicates that the application is awaiting approval for the <see cref="DiscordIntents.GuildPresences"/> intent.
        /// </summary>
        GatewayPresenceLimited = 1 << 13,

        /// <summary>
        /// Indicates that the application is approved for the <see cref="DiscordIntents.GuildMembers"/> intent.
        /// </summary>
        GatewayGuildMembers = 1 << 14,

        /// <summary>
        /// Indicates that the application is awaiting approval for the <see cref="DiscordIntents.GuildMembers"/> intent.
        /// </summary>
        GatewayGuildMembersLimited = 1 << 15,

        /// <summary>
        /// Indicates that the application is awaiting verification.
        /// </summary>
        VerificationPendingGuildLimit = 1 << 16,

        /// <summary>
        /// Indicates that the application is a voice channel application.
        /// </summary>
        Embedded = 1 << 17,
        
        /// <summary>
        /// The application can track message content.
        /// </summary>
        GatewayMessageContent = 1 << 18,

        /// <summary>
        /// The application can track message content (limited).
        /// </summary>
        GatewayMessageContentLimited = 1 << 19
    }
}
