using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    internal sealed class RestChannelCreatePayload
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonIgnore]
        public ChannelType? Type { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string TypeString => this.Type != null ? (this.Type.Value == ChannelType.Text ? "text" : "voice") : null;

        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bitrate { get; set; }

        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? UserLimit { get; set; }

        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DiscordOverwrite> PermissionOverwrites { get; set; }
    }

    internal sealed class RestChannelModifyPayload
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int? Position { get; set; }

        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public string Topic { get; set; }

        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bitrate { get; set; }

        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? UserLimit { get; set; }
    }

    internal class RestChannelMessageEditPayload
    {
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }
        
        [JsonProperty("embed", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbed Embed { get; set; }
    }

    internal sealed class RestChannelMessageCreatePayload : RestChannelMessageEditPayload
    {
        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTTS { get; set; }
    }

    internal sealed class RestChannelMessageCreateMultipartPayload
    {
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }

        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTTS { get; set; }

        [JsonProperty("embed", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmbed Embed { get; set; }
    }

    internal sealed class RestChannelMessageBulkDeletePayload
    {
        [JsonProperty("messages", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ulong> Messages { get; set; }
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
}
