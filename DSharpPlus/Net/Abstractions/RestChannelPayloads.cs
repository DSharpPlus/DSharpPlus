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
    }

    internal sealed class RestChannelModifyPayload
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

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

        [JsonProperty("rate_limit_per_user")]
        public Optional<int?> PerUserRateLimit { get; set; }
    }

    internal class RestChannelMessageEditPayload
    {
        [JsonProperty("content", NullValueHandling = NullValueHandling.Include)]
        public string Content { get; set; }

        [JsonIgnore]
        public bool HasContent { get; set; }
        
        [JsonProperty("embed", NullValueHandling = NullValueHandling.Include)]
        public DiscordEmbed Embed { get; set; }


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
        [JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMentions Mentions { get; set; }
    }

    internal sealed class RestChannelMessageCreateMultipartPayload
    {
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }

        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTTS { get; set; }

        [JsonProperty("embed", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbed Embed { get; set; }

        [JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordMentions Mentions { get; set; }
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
}
