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

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string IconBase64 { get; set; }

        [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
        public VerificationLevel? VerificationLevel { get; set; }

        [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
        public DefaultMessageNotifications? DefaultMessageNotifications { get; set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordRole> Roles { get; set; }

        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<RestChannelCreatePayload> Channels { get; set; }
    }

    internal sealed class RestGuildModifyPayload : RestGuildCreatePayload
    {
        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? OwnerId { get; set; }

        [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
        public string SplashBase64 { get; set; }

        [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? AfkChannelId { get; set; }

        [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
        public int? AfkTimeout { get; set; }

        [JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
        public MfaLevel? MfaLevel { get; set; }

        [JsonProperty("explicit_content_filter", NullValueHandling = NullValueHandling.Ignore)]
        public ExplicitContentFilter? ExplicitContentFilter { get; set; }

        // we no want that here
        [JsonIgnore]
        public new IEnumerable<DiscordRole> Roles { get; set; }

        // we no want that here
        [JsonIgnore]
        public new IEnumerable<RestChannelCreatePayload> Channels { get; set; }
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
        [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
        public string Nickname { get; set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ulong> RoleIds { get; set; }

        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Mute { get; set; }

        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Deafen { get; set; }

        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? VoiceChannelId { get; set; }
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
        [JsonProperty("pruned")]
        public int Pruned { get; set; }
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
}
