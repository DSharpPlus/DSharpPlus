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

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordTeam
    {
        /// <summary>
        /// A hash of the image of the team's icon.
        /// </summary>
        [JsonPropertyName("icon")]
        public string? Icon { get; internal set; }

        /// <summary>
        /// The unique id of the team.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The members of the team.
        /// </summary>
        [JsonPropertyName("members")]
        public DiscordTeamMember[] Members { get; internal set; } = null!;

        /// <summary>
        /// The name of the team.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; internal set; } = null!;

        /// <summary>
        /// The user id of the current team owner.
        /// </summary>
        [JsonPropertyName("owner_user_id")]
        public DiscordSnowflake OwnerUserId { get; internal set; } = null!;

        public static implicit operator ulong(DiscordTeam team) => team.Id;
        public static implicit operator DiscordSnowflake(DiscordTeam team) => team.Id;
    }
}
