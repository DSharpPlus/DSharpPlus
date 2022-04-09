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
using System.Text.Json.Serialization;
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordIntegration
    {
        /// <summary>
        /// The integration id.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The integration name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; internal set; } = null!;

        /// <summary>
        /// The integration type (twitch, youtube, or discord).
        /// </summary>
        [JsonPropertyName("type")]
        public DiscordIntegrationType Type { get; init; }

        /// <summary>
        /// Is this integration enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; internal set; }

        /// <summary>
        /// Is this integration syncing.
        /// </summary>
        /// <remarks>
        /// Not provided for bot integrations.
        /// </remarks>
        [JsonPropertyName("syncing")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<bool> Syncing { get; internal set; }

        /// <summary>
        /// The id that this integration uses for "subscribers".
        /// </summary>
        /// <remarks>
        /// Not provided for bot integrations.
        /// </remarks>
        [JsonPropertyName("role_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<string> RoleId { get; internal set; }

        /// <summary>
        /// Whether emoticons should be synced for this integration (twitch only currently).
        /// </summary>
        /// <remarks>
        /// Not provided for bot integrations.
        /// </remarks>
        [JsonPropertyName("enable_emoticons")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<bool> EnableEmoticons { get; internal set; }

        /// <summary>
        /// The behavior of expiring subscribers.
        /// </summary>
        /// <remarks>
        /// Not provided for bot integrations.
        /// </remarks>
        [JsonPropertyName("expire_behavior")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordIntegrationExpireBehavior> ExpireBehavior { get; internal set; }

        /// <summary>
        /// The grace period (in days) before expiring subscribers.
        /// </summary>
        /// <remarks>
        /// Not provided for bot integrations.
        /// </remarks>
        [JsonPropertyName("expire_grace_period")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<int> ExpireGracePeriod { get; internal set; }

        /// <summary>
        /// The user for this integration.
        /// </summary>
        /// <remarks>
        /// Not provided for bot integrations.
        /// </remarks>
        [JsonPropertyName("user")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordUser> User { get; init; }

        /// <summary>
        /// The integration account information.
        /// </summary>
        [JsonPropertyName("account")]
        public DiscordIntegrationAccount Account { get; init; } = null!;

        /// <summary>
        /// When this integration was last synced.
        /// </summary>
        /// <remarks>
        /// Not provided for bot integrations.
        /// </remarks>
        [JsonPropertyName("synced_at")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DateTimeOffset> SyncedAt { get; internal set; }

        /// <summary>
        /// How many subscribers this integration has.
        /// </summary>
        /// <remarks>
        /// Not provided for bot integrations.
        /// </remarks>
        [JsonPropertyName("subscriber_count")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<int> SubscriberCount { get; internal set; }

        /// <summary>
        /// Has this integration been revoked.
        /// </summary>
        /// <remarks>
        /// Not provided for bot integrations.
        /// </remarks>
        [JsonPropertyName("revoked")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<bool> Revoked { get; internal set; }

        /// <summary>
        /// The bot/OAuth2 application for discord integrations.
        /// </summary>
        [JsonPropertyName("application")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordIntegrationApplication> Application { get; init; }

        /// <summary>
        /// Sent on gateway integration events such as INTEGRATION_CREATE or INTEGRATION_UPDATE.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Optional<DiscordSnowflake> GuildId { get; init; }
    }
}
