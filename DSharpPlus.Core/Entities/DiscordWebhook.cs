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
    /// Used to represent a webhook.
    /// </summary>
    public sealed record DiscordWebhook
    {
        /// <summary>
        /// The id of the webhook.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The <see cref="DiscordWebhookType"/> of the webhook.
        /// </summary>
        [JsonPropertyName("type")]
        public DiscordWebhookType Type { get; init; }

        /// <summary>
        /// The guild id this webhook is for, if any.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<DiscordSnowflake?> GuildId { get; init; }

        /// <summary>
        /// The channel id this webhook is for, if any.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake? ChannelId { get; init; }

        /// <summary>
        /// The user this webhook was created by (not returned when getting a webhook with its token).
        /// </summary>
        [JsonPropertyName("user")]
        public Optional<DiscordUser> User { get; init; }

        /// <summary>
        /// The default name of the webhook.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; internal set; }

        /// <summary>
        /// The default user avatar hash of the webhook.
        /// </summary>
        [JsonPropertyName("avatar")]
        public string? Avatar { get; internal set; }

        /// <summary>
        /// The secure token of the webhook (returned for Incoming Webhooks).
        /// </summary>
        [JsonPropertyName("token")]
        public Optional<string> Token { get; internal set; }

        /// <summary>
        /// The bot/OAuth2 application that created this webhook.
        /// </summary>
        [JsonPropertyName("application_id")]
        public DiscordSnowflake? ApplicationId { get; init; }

        /// <summary>
        /// The guild of the channel that this webhook is following (returned for Channel Follower Webhooks).
        /// </summary>
        [JsonPropertyName("source_guild")]
        public Optional<DiscordGuild> SourceGuild { get; init; }

        /// <summary>
        /// The channel that this webhook is following (returned for Channel Follower Webhooks).
        /// </summary>
        [JsonPropertyName("source_channel")]
        public Optional<DiscordChannel> SourceChannel { get; init; }

        /// <summary>
        /// The url used for executing the webhook (returned by the webhooks OAuth2 flow).
        /// </summary>
        [JsonPropertyName("url")]
        public Optional<string> Url { get; init; }
    }
}
