// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
    internal interface IReasonAction
    {
        string Reason { get; set; }

        //[JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        //public string Reason { get; set; }
    }

    internal class RestGuildCreatePayload
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public string RegionId { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
        public Optional<string> IconBase64 { get; set; }

        [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
        public VerificationLevel? VerificationLevel { get; set; }

        [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
        public DefaultMessageNotifications? DefaultMessageNotifications { get; set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordRole> Roles { get; set; }

        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<RestChannelCreatePayload> Channels { get; set; }
    }

    internal sealed class RestGuildCreateFromTemplatePayload
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
        public Optional<string> IconBase64 { get; set; }
    }

    internal sealed class RestGuildModifyPayload
    {
        [JsonProperty("name")]
        public Optional<string> Name { get; set; }

        [JsonProperty("region")]
        public Optional<string> RegionId { get; set; }

        [JsonProperty("icon")]
        public Optional<string> IconBase64 { get; set; }

        [JsonProperty("verification_level")]
        public Optional<VerificationLevel> VerificationLevel { get; set; }

        [JsonProperty("default_message_notifications")]
        public Optional<DefaultMessageNotifications> DefaultMessageNotifications { get; set; }

        [JsonProperty("owner_id")]
        public Optional<ulong> OwnerId { get; set; }

        [JsonProperty("splash")]
        public Optional<string> SplashBase64 { get; set; }

        [JsonProperty("afk_channel_id")]
        public Optional<ulong?> AfkChannelId { get; set; }

        [JsonProperty("afk_timeout")]
        public Optional<int> AfkTimeout { get; set; }

        [JsonProperty("mfa_level")]
        public Optional<MfaLevel> MfaLevel { get; set; }

        [JsonProperty("explicit_content_filter")]
        public Optional<ExplicitContentFilter> ExplicitContentFilter { get; set; }

        [JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Include)]
        public Optional<ulong?> SystemChannelId { get; set; }
    }

    internal sealed class RestGuildMemberAddPayload : IOAuth2Payload
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
        public string Nickname { get; set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordRole> Roles { get; set; }

        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Mute { get; set; }

        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Deaf { get; set; }
    }

    internal sealed class RestGuildChannelReorderPayload
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelId { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int Position { get; set; }

        [JsonProperty("lock_permissions", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LockPermissions { get; set; }

        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ParentId { get; set; }
    }

    internal sealed class RestGuildRoleReorderPayload
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong RoleId { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int Position { get; set; }
    }

    internal sealed class RestGuildMemberModifyPayload
    {
        [JsonProperty("nick")]
        public Optional<string> Nickname { get; set; }

        [JsonProperty("roles")]
        public Optional<IEnumerable<ulong>> RoleIds { get; set; }

        [JsonProperty("mute")]
        public Optional<bool> Mute { get; set; }

        [JsonProperty("deaf")]
        public Optional<bool> Deafen { get; set; }

        [JsonProperty("channel_id")]
        public Optional<ulong?> VoiceChannelId { get; set; }
    }

    internal sealed class RestGuildRolePayload
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions? Permissions { get; set; }

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public int? Color { get; set; }

        [JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Hoist { get; set; }

        [JsonProperty("mentionable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Mentionable { get; set; }
    }

    internal sealed class RestGuildPruneResultPayload
    {
        [JsonProperty("pruned", NullValueHandling = NullValueHandling.Ignore)]
        public int? Pruned { get; set; }
    }

    internal sealed class RestGuildIntegrationAttachPayload
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }
    }

    internal sealed class RestGuildIntegrationModifyPayload
    {
        [JsonProperty("expire_behavior", NullValueHandling = NullValueHandling.Ignore)]
        public int? ExpireBehavior { get; set; }

        [JsonProperty("expire_grace_period", NullValueHandling = NullValueHandling.Ignore)]
        public int? ExpireGracePeriod { get; set; }

        [JsonProperty("enable_emoticons", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EnableEmoticons { get; set; }
    }

    internal class RestGuildEmojiModifyPayload
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public ulong[] Roles { get; set; }
    }

    internal class RestGuildEmojiCreatePayload : RestGuildEmojiModifyPayload
    {
        [JsonProperty("image")]
        public string ImageB64 { get; set; }
    }

    internal class RestGuildWidgetSettingsPayload
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }

        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ChannelId { get; set; }
    }

    internal class RestGuildTemplateCreateOrModifyPayload
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
        public string Name { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
        public string Description { get; set; }
    }

    internal class RestGuildMembershipScreeningFormModifyPayload
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Enabled { get; set; }

        [JsonProperty("form_fields", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordGuildMembershipScreeningField[]> Fields { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Description { get; set; }
    }

    internal class RestGuildWelcomeScreenModifyPayload
    {
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Enabled { get; set; }

        [JsonProperty("welcome_channels", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IEnumerable<DiscordGuildWelcomeScreenChannel>> WelcomeChannels { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Description { get; set; }
    }

    internal class RestGuildUpdateCurrentUserVoiceStatePayload
    {
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; set; }

        [JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Suppress { get; set; }

        [JsonProperty("request_to_speak_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? RequestToSpeakTimestamp { get; set; }
    }

    internal class RestGuildUpdateUserVoiceStatePayload
    {
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; set; }

        [JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Suppress { get; set; }
    }
}
