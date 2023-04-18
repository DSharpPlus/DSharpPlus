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

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord permission overwrite builder.
    /// </summary>
    public sealed class DiscordOverwriteBuilder
    {
        /// <summary>
        /// Gets or sets the allowed permissions for this overwrite.
        /// </summary>
        public Permissions Allowed { get; set; }

        /// <summary>
        /// Gets or sets the denied permissions for this overwrite.
        /// </summary>
        public Permissions Denied { get; set; }

        /// <summary>
        /// Gets the type of this overwrite's target.
        /// </summary>
        public OverwriteType Type { get; private set; }

        /// <summary>
        /// Gets the target for this overwrite.
        /// </summary>
        public SnowflakeObject Target { get; private set; }

        /// <summary>
        /// Creates a new Discord permission overwrite builder for a member. This class can be used to construct permission overwrites for guild channels, used when creating channels.
        /// </summary>
        public DiscordOverwriteBuilder(DiscordMember member)
        {
            this.Target = member;
            this.Type = OverwriteType.Member;
        }
        /// <summary>
        /// Creates a new Discord permission overwrite builder for a role. This class can be used to construct permission overwrites for guild channels, used when creating channels.
        /// </summary>
        public DiscordOverwriteBuilder(DiscordRole role)
        {
            this.Target = role;
            this.Type = OverwriteType.Role;
        }

        /// <summary>
        /// Creates a new Discord permission overwrite builder. This class can be used to construct permission overwrites for guild channels, used when creating channels.
        /// </summary>
        [Obsolete("The parameterless constructor will be removed in 4.4. Use the parameterized constructors instead", false)]
        public DiscordOverwriteBuilder()
        {

        }

        /// <summary>
        /// Allows a permission for this overwrite.
        /// </summary>
        /// <param name="permission">Permission or permission set to allow for this overwrite.</param>
        /// <returns>This builder.</returns>
        public DiscordOverwriteBuilder Allow(Permissions permission)
        {
            this.Allowed |= permission;
            return this;
        }

        /// <summary>
        /// Denies a permission for this overwrite.
        /// </summary>
        /// <param name="permission">Permission or permission set to deny for this overwrite.</param>
        /// <returns>This builder.</returns>
        public DiscordOverwriteBuilder Deny(Permissions permission)
        {
            this.Denied |= permission;
            return this;
        }

        /// <summary>
        /// Sets the member to which this overwrite applies.
        /// </summary>
        /// <param name="member">Member to which apply this overwrite's permissions.</param>
        /// <returns>This builder.</returns>
        public DiscordOverwriteBuilder For(DiscordMember member)
        {
            this.Target = member;
            this.Type = OverwriteType.Member;
            return this;
        }

        /// <summary>
        /// Sets the role to which this overwrite applies.
        /// </summary>
        /// <param name="role">Role to which apply this overwrite's permissions.</param>
        /// <returns>This builder.</returns>
        public DiscordOverwriteBuilder For(DiscordRole role)
        {
            this.Target = role;
            this.Type = OverwriteType.Role;
            return this;
        }

        /// <summary>
        /// Populates this builder with data from another overwrite object.
        /// </summary>
        /// <param name="other">Overwrite from which data will be used.</param>
        /// <returns>This builder.</returns>
        public async Task<DiscordOverwriteBuilder> FromAsync(DiscordOverwrite other)
        {
            this.Allowed = other.Allowed;
            this.Denied = other.Denied;
            this.Type = other.Type;
            this.Target = this.Type == OverwriteType.Member ? await other.GetMemberAsync() as SnowflakeObject : await other.GetRoleAsync() as SnowflakeObject;

            return this;
        }

        /// <summary>
        /// Builds this DiscordOverwrite.
        /// </summary>
        /// <returns>Use this object for creation of new overwrites.</returns>
        internal DiscordRestOverwrite Build()
        {
            return new DiscordRestOverwrite()
            {
                Allow = this.Allowed,
                Deny = this.Denied,
                Id = this.Target.Id,
                Type = this.Type,
            };
        }
    }

    internal struct DiscordRestOverwrite
    {
        [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
        internal Permissions Allow { get; set; }

        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
        internal Permissions Deny { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        internal OverwriteType Type { get; set; }
    }
}
