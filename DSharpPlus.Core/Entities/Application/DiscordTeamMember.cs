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

using System.Text.Json.Serialization;
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordTeamMember
    {
        /// <summary>
        /// The user's <see cref="DiscordMembershipState">membership state</see> on the team.
        /// </summary>
        [JsonPropertyName("membership_state")]
        public DiscordMembershipState MembershipState { get; internal set; }

        /// <summary>
        /// Will always be <c>["*"]</c>.
        /// </summary>
        [JsonPropertyName("permissions")]
        public string[] Permissions { get; init; } = null!;

        /// <summary>
        /// The id of the parent team of which they are a member.
        /// </summary>
        [JsonPropertyName("team_id")]
        public DiscordSnowflake TeamId { get; init; } = null!;

        /// <summary>
        /// The avatar, discriminator, id, and username of the user.
        /// </summary>
        [JsonPropertyName("user")]
        public DiscordUser User { get; init; } = null!;

        public static implicit operator ulong(DiscordTeamMember teamMember) => teamMember.User.Id;
        public static implicit operator DiscordSnowflake(DiscordTeamMember teamMember) => teamMember.User.Id;
    }
}
