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
    public sealed record DiscordActivity
    {
        /// <summary>
        /// The activity's name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        ///The activity type.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordActivityType Type { get; init; }

        /// <summary>
        /// The stream url, is validated when type is <see cref="DiscordActivityType.Streaming"/>.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Url { get; init; }

        /// <summary>
        /// A timestamp of when the activity was added to the user's session.
        /// </summary>
        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Timestamps for start and/or end of the game.
        /// </summary>
        [JsonProperty("timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityTimestamps> Timestamps { get; init; }

        /// <summary>
        /// The application id for the game.
        /// </summary>
        [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> ApplicationId { get; init; }

        /// <summary>
        /// What the player is currently doing.
        /// </summary>
        [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Details { get; init; }

        /// <summary>
        /// The user's current party status.
        /// </summary>
        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> State { get; init; }

        /// <summary>
        /// The emoji used for a custom status.
        /// </summary>
        [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityEmoji?> Emoji { get; init; }

        /// <summary>
        /// The information for the current party of the player.
        /// </summary>
        [JsonProperty("party", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityParty> Party { get; init; }

        /// <summary>
        /// Images for the presence and their hover texts.
        /// </summary>
        [JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityAssets> Assets { get; init; }

        /// <summary>
        /// Secrets for Rich Presence joining and spectating.
        /// </summary>
        [JsonProperty("secrets", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivitySecrets> Secrets { get; init; }

        /// <summary>
        /// Whether or not the activity is an instanced game session.
        /// </summary>
        [JsonProperty("instance", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Instance { get; init; }

        /// <summary>
        /// <see cref="DiscordActivityFlags"/> <c>OR</c>'d together, describes what the payload includes.
        /// </summary>
        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityFlags> Flags { get; init; }

        /// <summary>
        /// The custom buttons shown in the Rich Presence (max 2).
        /// </summary>
        [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordActivityButton> Buttons { get; init; }
    }
}
