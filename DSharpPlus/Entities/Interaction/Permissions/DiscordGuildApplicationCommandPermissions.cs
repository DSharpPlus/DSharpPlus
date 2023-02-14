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

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the guild permissions for a application command.
    /// </summary>
    public class DiscordGuildApplicationCommandPermissions : SnowflakeObject
    {
        /// <summary>
        /// Gets the id of the application the command belongs to.
        /// </summary>
        [JsonProperty("application_id")]
        public ulong ApplicationId { get; internal set; }

        /// <summary>
        /// Gets the id of the guild.
        /// </summary>
        [JsonProperty("guild_id")]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets the guild.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild
            => (this.Discord as DiscordClient).InternalGetCachedGuild(this.GuildId);

        /// <summary>
        /// Gets the permissions for the application command in the guild.
        /// </summary>
        [JsonProperty("permissions")]
        public IReadOnlyList<DiscordApplicationCommandPermission> Permissions { get; internal set; }

        internal DiscordGuildApplicationCommandPermissions() { }

        /// <summary>
        /// Represents the guild application command permissions for a application command.
        /// </summary>
        /// <param name="commandId">The id of the command.</param>
        /// <param name="permissions">The permissions for the application command.</param>
        public DiscordGuildApplicationCommandPermissions(ulong commandId, IEnumerable<DiscordApplicationCommandPermission> permissions)
        {
            this.Id = commandId;
            this.Permissions = permissions.ToList();
        }
    }
}
