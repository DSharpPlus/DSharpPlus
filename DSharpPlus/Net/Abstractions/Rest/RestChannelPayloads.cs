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
using System.Collections.Generic;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    internal sealed class RestChannelCreatePayload
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public ChannelType Type { get; set; }

        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? Parent { get; set; }

        [JsonProperty("topic")]
        public Optional<string> Topic { get; set; }

        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bitrate { get; set; }

        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? UserLimit { get; set; }

        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordRestOverwrite> PermissionOverwrites { get; set; }

        [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Nsfw { get; set; }

        [JsonProperty("rate_limit_per_user")]
        public Optional<int?> PerUserRateLimit { get; set; }

        [JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
        public VideoQualityMode? QualityMode { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int? Position { get; set; }
    }

    internal sealed class RestChannelModifyPayload
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("type")]
        public Optional<ChannelType> Type { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int? Position { get; set; }

        [JsonProperty("topic")]
        public Optional<string> Topic { get; set; }

        [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Nsfw { get; set; }

        [JsonProperty("parent_id")]
        public Optional<ulong?> Parent { get; set; }

        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bitrate { get; set; }

        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? UserLimit { get; set; }

        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordRestOverwrite> PermissionOverwrites { get; set; }

        [JsonProperty("rate_limit_per_user")]
        public Optional<int?> PerUserRateLimit { get; set; }

        [JsonProperty("rtc_region")]
        public Optional<string> RtcRegion { get; set; }

        [JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
        public VideoQualityMode? QualityMode { get; set; }
    }

    internal sealed class RestThreadChannelModifyPayload
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("type")]
        public Optional<ChannelType> Type { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int? Position { get; set; }

        [JsonProperty("topic")]
        public Optional<string> Topic { get; set; }

        [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Nsfw { get; set; }

        [JsonProperty("parent_id")]
        public Optional<ulong?> Parent { get; set; }

        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bitrate { get; set; }

        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? UserLimit { get; set; }

        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordRestOverwrite> PermissionOverwrites { get; set; }

        [JsonProperty("rate_limit_per_user")]
        public Optional<int?> PerUserRateLimit { get; set; }

        [JsonProperty("rtc_region")]
        public Optional<string> RtcRegion { get; set; }

        [JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
        public VideoQualityMode? QualityMode { get; set; }

        [JsonProperty("archived", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsArchived { get; set; }

        [JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
        public AutoArchiveDuration? ArchiveDuration { get; set; }

        [JsonProperty("locked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Locked { get; set; }
    }

    internal class RestChannelMessageEditPayload
    {
        [JsonProperty("content", NullValueHandling = NullValueHandling.Include)]
        public string Content { get; set; }

        [JsonIgnore]
        public bool HasContent { get; set; }

        [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordEmbed> Embeds { get; set; }

        [JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMentions Mentions { get; set; }

        [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordActionRowComponent> Components { get; set; }

        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public MessageFlags? Flags { get; set; }

        [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordAttachment> Attachments { get; set; }

        [JsonIgnore]
        public bool HasEmbed { get; set; }

        public bool ShouldSerializeContent()
            => this.HasContent;

        public bool ShouldSerializeEmbed()
            => this.HasEmbed;
    }

    internal sealed class RestChannelMessageCreatePayload : RestChannelMessageEditPayload
    {
        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTTS { get; set; }

        [JsonProperty("sticker_ids", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ulong> StickersIds { get; set; } // Discord sends an array, but you can only have one* sticker on a message //

        [JsonProperty("message_reference", NullValueHandling = NullValueHandling.Ignore)]
        public InternalDiscordMessageReference? MessageReference { get; set; }

    }

    internal sealed class RestChannelMessageCreateMultipartPayload
    {
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }

        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTTS { get; set; }

        [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordEmbed> Embeds { get; set; }

        [JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMentions Mentions { get; set; }

        [JsonProperty("message_reference", NullValueHandling = NullValueHandling.Ignore)]
        public InternalDiscordMessageReference? MessageReference { get; set; }
    }

    internal sealed class RestChannelMessageBulkDeletePayload
    {
        [JsonProperty("messages", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ulong> Messages { get; set; }
    }

    internal sealed class RestChannelMessageSuppressEmbedsPayload
    {
        [JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Suppress { get; set; }
    }

    internal sealed class RestChannelInviteCreatePayload
    {
        [JsonProperty("max_age", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxAge { get; set; }

        [JsonProperty("max_uses", NullValueHandling = NullValueHandling.Ignore)]
        public int MaxUses { get; set; }

        [JsonProperty("temporary", NullValueHandling = NullValueHandling.Ignore)]
        public bool Temporary { get; set; }

        [JsonProperty("unique", NullValueHandling = NullValueHandling.Ignore)]
        public bool Unique { get; set; }

        [JsonProperty("target_type", NullValueHandling = NullValueHandling.Ignore)]
        public InviteTargetType? TargetType { get; set; }

        [JsonProperty("target_user_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? TargetUserId { get; set; }

        [JsonProperty("target_application_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? TargetApplicationId { get; set; }
    }

    internal sealed class RestChannelPermissionEditPayload
    {
        [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Allow { get; set; }

        [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Deny { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }

    internal sealed class RestChannelGroupDmRecipientAddPayload : IOAuth2Payload
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
        public string Nickname { get; set; }
    }

    internal sealed class AcknowledgePayload
    {
        [JsonProperty("token", NullValueHandling = NullValueHandling.Include)]
        public string Token { get; set; }
    }

    internal sealed class RestCreateStageInstancePayload
    {
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("privacy_level", NullValueHandling = NullValueHandling.Ignore)]
        public PrivacyLevel? PrivacyLevel { get; set; }
    }

    internal sealed class RestModifyStageInstancePayload
    {
        [JsonProperty("topic")]
        public Optional<string> Topic { get; set; }

        [JsonProperty("privacy_level")]
        public Optional<PrivacyLevel> PrivacyLevel { get; set; }
    }

    internal sealed class RestBecomeStageSpeakerInstancePayload
    {
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; set; }
        [JsonProperty("request_to_speak_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? RequestToSpeakTimestamp { get; set; }
        [JsonProperty("suppress")]
        public bool Suppress { get; set; }
    }
}
