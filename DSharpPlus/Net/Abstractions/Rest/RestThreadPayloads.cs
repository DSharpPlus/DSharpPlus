namespace DSharpPlus.Net.Abstractions;

using System.Collections.Generic;
using DSharpPlus.Entities;

using Newtonsoft.Json;

internal sealed class RestThreadCreatePayload
{
    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordAutoArchiveDuration ArchiveAfter { get; set; }

    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordChannelType? Type { get; set; }
}

internal sealed class RestForumPostCreatePayload
{
    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordAutoArchiveDuration? ArchiveAfter { get; set; }

    [JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Include)]
    public int? RateLimitPerUser { get; set; }

    [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
    public required RestChannelMessageCreatePayload Message { get; set; }

    [JsonProperty("applied_tags", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<ulong>? AppliedTags { get; set; }
}
