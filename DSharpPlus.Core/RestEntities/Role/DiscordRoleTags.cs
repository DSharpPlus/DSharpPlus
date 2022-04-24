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


using Newtonsoft.Json;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// A <see cref="DiscordRole"/>'s metadata.
    /// </summary>
    public sealed record DiscordRoleTags
    {
        /// <summary>
        /// The id of the bot this role belongs to.
        /// </summary>
        [JsonProperty("bot_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> BotId { get; init; }

        /// <summary>
        /// The id of the integration this role belongs to.
        /// </summary>
        [JsonProperty("integration_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> IntegrationId { get; init; }

        /// <summary>
        /// Whether this is the guild's premium subscriber role.
        /// </summary>
        /// <remarks>
        /// Null when it is the guild's premium subscriber role, otherwise <see cref="Optional{T}.Empty"/>. You should use <see cref="Optional{T}.HasValue"/> to check if this is the guild's premium subscriber role.
        /// </remarks>
        [JsonProperty("premium_subscriber", NullValueHandling = NullValueHandling.Ignore)]
        internal Optional<bool> PremiumSubscriber { get; init; }
    }
}
