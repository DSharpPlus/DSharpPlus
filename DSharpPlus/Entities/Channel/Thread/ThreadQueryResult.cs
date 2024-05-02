
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;
public class ThreadQueryResult
{
    /// <summary>
    /// Gets whether additional calls will yield more threads.
    /// </summary>
    [JsonProperty("has_more", NullValueHandling = NullValueHandling.Ignore)]
    public bool HasMore { get; internal set; }

    /// <summary>
    /// Gets the list of threads returned by the query. Generally ordered by <seealso cref="DiscordThreadChannelMetadata.ArchiveTimestamp"/> in descending order.
    /// </summary>
    [JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordThreadChannel> Threads { get; internal set; }

    [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
    internal IReadOnlyList<DiscordThreadChannelMember> Members { get; set; }

}
