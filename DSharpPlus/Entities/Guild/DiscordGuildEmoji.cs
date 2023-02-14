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

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    public sealed class DiscordGuildEmoji : DiscordEmoji
    {
        /// <summary>
        /// Gets the user that created this emoji.
        /// </summary>
        [JsonIgnore]
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the guild to which this emoji belongs.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild { get; internal set; }

        internal DiscordGuildEmoji() { }

        /// <summary>
        /// Modifies this emoji.
        /// </summary>
        /// <param name="name">New name for this emoji.</param>
        /// <param name="roles">Roles for which this emoji will be available. This works only if your application is whitelisted as integration.</param>
        /// <param name="reason">Reason for audit log.</param>
        /// <returns>The modified emoji.</returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojis"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildEmoji> ModifyAsync(string name, IEnumerable<DiscordRole> roles = null, string reason = null)
            => this.Guild.ModifyEmojiAsync(this, name, roles, reason);

        /// <summary>
        /// Deletes this emoji.
        /// </summary>
        /// <param name="reason">Reason for audit log.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojis"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task DeleteAsync(string reason = null)
            => this.Guild.DeleteEmojiAsync(this, reason);
    }
}
