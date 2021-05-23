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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a guild application command permission.
    /// </summary>
    public sealed class DiscordGuildApplicationCommandPermission : SnowflakeObject, IEquatable<DiscordGuildApplicationCommandPermission>
    {
        /// <summary>
        /// Gets the unique ID of this command's application.
        /// </summary>
        [JsonProperty("application_id")]
        public ulong ApplicationId { get; internal set; }

        /// <summary>
        /// Gets the guild id this permission applies to.
        /// </summary>
        [JsonProperty("guild_id")]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets the permission array.
        /// </summary>
        [JsonProperty("permissions")]
        public DiscordApplicationCommandPermission Permissions { get; internal set; }

        /// <summary>
        /// Checks whether this <see cref="DiscordGuildApplicationCommandPermission"/> object is equal to another object.
        /// </summary>
        /// <param name="other">The command to compare to.</param>
        /// <returns>Whether the command is equal to this <see cref="DiscordGuildApplicationCommandPermission"/>.</returns>
        public bool Equals(DiscordGuildApplicationCommandPermission other)
            => this.Id == other.Id;

        /// <summary>
        /// Determines if two <see cref="DiscordGuildApplicationCommandPermission"/> objects are equal. 
        /// </summary>
        /// <param name="e1">The first command object.</param>
        /// <param name="e2">The second command object.</param>
        /// <returns>Whether the two <see cref="DiscordGuildApplicationCommandPermission"/> objects are equal.</returns>
        public static bool operator ==(DiscordGuildApplicationCommandPermission e1, DiscordGuildApplicationCommandPermission e2)
            => e1.Equals(e2);

        /// <summary>
        /// Determines if two <see cref="DiscordGuildApplicationCommandPermission"/> objects are not equal. 
        /// </summary>
        /// <param name="e1">The first command object.</param>
        /// <param name="e2">The second command object.</param>
        /// <returns>Whether the two <see cref="DiscordGuildApplicationCommandPermission"/> objects are not equal.</returns>
        public static bool operator !=(DiscordGuildApplicationCommandPermission e1, DiscordGuildApplicationCommandPermission e2)
            => !(e1 == e2);

        /// <summary>
        /// Determines if a <see cref="object"/> is equal to the current <see cref="DiscordApplicationCommand"/>.
        /// </summary>
        /// <param name="other">The object to compare to.</param>
        /// <returns>Whether the two <see cref="DiscordGuildApplicationCommandPermission"/> objects are not equal.</returns>
        public override bool Equals(object other)
        {
            return other is DiscordGuildApplicationCommandPermission dac ? this.Equals(dac) : false;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordGuildApplicationCommandPermission"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordGuildApplicationCommandPermission"/>.</returns>
        public override int GetHashCode()
            => this.Id.GetHashCode();
    }
}
