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
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordApplication
    {
        /// <summary>
        /// The id of the app.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The name of the app.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The icon hash of the app.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string? IconHash { get; internal set; }

        /// <summary>
        /// The description of the app.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; } = null!;

        /// <summary>
        /// An array of rpc origin urls, if rpc is enabled.
        /// </summary>
        [JsonProperty("rpc_origins", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string[]> RpcOrigins { get; internal set; }

        /// <summary>
        /// When false only app owner can join the app's bot to guilds.
        /// </summary>
        [JsonProperty("bot_public", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPublicBot { get; internal set; }

        /// <summary>
        /// When true the app's bot will only join upon completion of the full oauth2 code grant flow.
        /// </summary>
        [JsonProperty("bot_require_code_grant", NullValueHandling = NullValueHandling.Ignore)]
        public bool BotRequiresFullCodeGrant { get; internal set; }

        /// <summary>
        /// The url of the app's terms of service.
        /// </summary>
        [JsonProperty("terms_of_service_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> TermsOfServiceUrl { get; internal set; }

        /// <summary>
        /// The url of the app's privacy policy.
        /// </summary>
        [JsonProperty("privacy_policy_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> PrivacyPolicyUrl { get; internal set; }

        /// <summary>
        /// Partial user object containing info on the owner of the application.
        /// </summary>
        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUser> Owner { get; internal set; }

        /// <summary>
        /// deprecated: previously if this application was a game sold on Discord, this field would be the summary field for the store page of its primary SKU; now an empty string
        /// </summary>
        [JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("deprecated: previously if this application was a game sold on Discord, this field would be the summary field for the store page of its primary SKU; now an empty string")]
        public string Summary { get; set; } = null!;

        /// <summary>
        /// The hex encoded key for verification in interactions and the GameSDK's GetTicket.
        /// </summary>
        [JsonProperty("verify_key", NullValueHandling = NullValueHandling.Ignore)]
        public string VerifyKey { get; internal set; } = null!;

        /// <summary>
        /// If the application belongs to a team, this will be a list of the members of that team.
        /// </summary>
        [JsonProperty("team", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordTeam? Team { get; internal set; }

        /// <summary>
        /// If this application is a game sold on Discord, this field will be the guild to which it has been linked.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// If this application is a game sold on Discord, this field will be the id of the "Game SKU" that is created, if exists.
        /// </summary>
        [JsonProperty("primary_sku_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> PrimarySKUId { get; init; }

        /// <summary>
        /// If this application is a game sold on Discord, this field will be the URL slug that links to the store page.
        /// </summary>
        [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Slug { get; internal set; }

        /// <summary>
        /// The application's default rich presence invite cover image hash.
        /// </summary>
        [JsonProperty("cover_image", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> CoverImageHash { get; internal set; }

        /// <summary>
        /// The application's public <see cref="DiscordApplicationFlags">flags</see>.
        /// </summary>
        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordApplicationFlags> Flags { get; internal set; }
    }
}
