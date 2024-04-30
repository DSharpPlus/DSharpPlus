namespace DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

internal sealed class FollowedChannelAddPayload
{
    [JsonProperty("webhook_channel_id")]
    public ulong WebhookChannelId { get; set; }
}
