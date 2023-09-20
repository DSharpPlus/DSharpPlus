using System.Collections.Generic;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    internal sealed class RestThreadCreatePayload
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
        public AutoArchiveDuration ArchiveAfter { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public ChannelType? Type { get; set; }
    }

    internal sealed class RestForumPostCreatePayload
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("auto_archive_duration", NullValueHandling = NullValueHandling.Ignore)]
        public AutoArchiveDuration? ArchiveAfter { get; set; }

        [JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Include)]
        public int? RateLimitPerUser { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public RestChannelMessageCreatePayload Message { get; set; }

        [JsonProperty("applied_tags", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ulong> AppliedTags { get; set; }
    }

}
