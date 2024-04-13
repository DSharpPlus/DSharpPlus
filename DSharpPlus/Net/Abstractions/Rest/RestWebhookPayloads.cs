using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.Entities.Channel;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

internal sealed class RestWebhookPayload
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name { get; set; }

    [JsonProperty("avatar", NullValueHandling = NullValueHandling.Include)]
    public string? AvatarBase64 { get; set; }

    [JsonProperty("channel_id")]
    public ulong ChannelId { get; set; }

    [JsonProperty]
    public bool AvatarSet { get; set; }

    public bool ShouldSerializeAvatarBase64()
        => this.AvatarSet;
}

internal sealed class RestWebhookExecutePayload
{
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    public string? Content { get; set; }

    [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
    public string? Username { get; set; }

    [JsonProperty("avatar_url", NullValueHandling = NullValueHandling.Ignore)]
    public string? AvatarUrl { get; set; }

    [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsTTS { get; set; }

    [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<DiscordEmbed>? Embeds { get; set; }

    [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<DiscordActionRowComponent>? Components { get; set; }

    [JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMentions? Mentions { get; set; }
    
    [JsonProperty("poll", NullValueHandling = NullValueHandling.Ignore)]
    public PollCreatePayload? Poll { get; set; }
}

internal sealed class RestWebhookMessageEditPayload
{
    [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
    public Optional<string> Content { get; set; }

    [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<DiscordEmbed>? Embeds { get; set; }

    [JsonProperty("allowed_mentions", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMentions? Mentions { get; set; }

    [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<DiscordActionRowComponent>? Components { get; set; }

    [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<DiscordAttachment>? Attachments { get; set; }
}
