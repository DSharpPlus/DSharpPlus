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
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a discord stage instance.
    /// </summary>
    public sealed class DiscordStageInstance : SnowflakeObject
    {
        /// <summary>
        /// Gets the guild this stage instance is in.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild
            => this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null;

        /// <summary>
        /// Gets the id of the guild this stage instance is in.
        /// </summary>
        [JsonProperty("guild_id")]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets the channel this stage instance is in.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel
            => (this.Discord as DiscordClient)?.InternalGetCachedChannel(this.ChannelId) ?? null;

        /// <summary>
        /// Gets the id of the channel this stage instance is in.
        /// </summary>
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the topic of this stage instance.
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; internal set; }

        /// <summary>
        /// Gets the privacy level of this stage instance.
        /// </summary>
        [JsonProperty("privacy_level")]
        public PrivacyLevel PrivacyLevel { get; internal set; }

        /// <summary>
        /// Gets whether or not stage discovery is disabled.
        /// </summary>
        [JsonProperty("discoverable_disabled")]
        public bool DiscoverableDisabled { get; internal set; }

        /// <summary>
        /// Become speaker of current stage.
        /// </summary>
        public Task BecomeSpeaker()
            => this.Discord.ApiClient.StageInstanceBecomeSpeakerAsync(this.GuildId, this.Id);
        /// <summary>
        /// Send request to speak in current stage.
        /// </summary>
        public Task SendSpeakerRequest() => this.Discord.ApiClient.StageInstanceBecomeSpeakerAsync(this.GuildId, this.Id, DateTime.Now);
        /// <summary>
        /// Send invite to speak in current stage.
        /// </summary>
        /// <param name="member">Member to invite speak.</param>
        /// <returns></returns>
        public Task InviteToSpeak(DiscordMember member) => this.Discord.ApiClient.StageInstanceBecomeSpeakerAsync(this.GuildId, this.Id, null, false, member.Id);
    }
}
