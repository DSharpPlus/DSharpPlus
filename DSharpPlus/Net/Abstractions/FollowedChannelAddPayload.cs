
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;
internal sealed class FollowedChannelAddPayload
{
    [JsonProperty("webhook_channel_id")]
    public ulong WebhookChannelId { get; set; }
}
