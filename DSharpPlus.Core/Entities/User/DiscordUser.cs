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
    /// <summary>
    /// Implements a <see href="https://discord.com/developers/docs/resources/user#user-object-user-structure">Discord User</see>.
    /// </summary>
    public sealed record DiscordUser
    {
        /// <summary>
        /// The user's id, used to identify the user across all of Discord.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The user's username, not unique across the platform.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; init; } = null!;

        /// <summary>
        /// The user's 4-digit discord-tag.
        /// </summary>
        [JsonPropertyName("discriminator")]
        public string Discriminator { get; init; } = null!;

        /// <summary>
        /// The user's avatar hash.
        /// </summary>
        [JsonPropertyName("avatar")]
        public string? Avatar { get; internal set; }

        /// <summary>
        /// Whether the user belongs to an OAuth2 application.
        /// </summary>
        [JsonPropertyName("bot")]
        public Optional<bool> Bot { get; init; }

        /// <summary>
        /// Whether the user is an Official Discord System user (part of the urgent message system).
        /// </summary>
        [JsonPropertyName("system")]
        public Optional<bool> System { get; init; }

        /// <summary>
        /// Whether the user has two factor enabled on their account.
        /// </summary>
        [JsonPropertyName("mfa_enabled")]
        public Optional<bool> MFAEnabled { get; internal set; }

        /// <summary>
        /// The user's banner hash.
        /// </summary>
        [JsonPropertyName("banner")]
        public Optional<string?> Banner { get; internal set; }

        /// <summary>
        /// The user's banner color.
        /// </summary>
        [JsonPropertyName("accent_color")]
        public Optional<int?> AccentColor { get; internal set; }

        /// <summary>
        /// The user's chosen language option.
        /// </summary>
        [JsonPropertyName("locale")]
        public Optional<string> Locale { get; internal set; }

        /// <summary>
        /// Whether the email on this account has been verified. Requires the email oauth2 scope.
        /// </summary>
        [JsonPropertyName("verified")]
        public Optional<bool> Verified { get; internal set; }

        /// <summary>
        /// The user's email. Requires the email oauth2 scope.
        /// </summary>
        [JsonPropertyName("email")]
        public Optional<string?> Email { get; internal set; }

        /// <summary>
        /// The user flags on a user's account.
        /// </summary>
        [JsonPropertyName("flags")]
        public Optional<DiscordUserFlags> Flags { get; internal set; }


        /// <summary>
        /// The type of Nitro subscription on a user's account.
        /// </summary>
        [JsonPropertyName("premium_type")]
        public Optional<DiscordPremiumType> PremiumType { get; internal set; }

        /// <summary>
        /// The public flags on a user's account.
        /// </summary>
        [JsonPropertyName("public_flags")]
        public Optional<DiscordUserFlags> PublicFlags { get; internal set; }

        public static implicit operator ulong(DiscordUser user) => user.Id;
        public static implicit operator DiscordSnowflake(DiscordUser user) => user.Id;
    }
}
