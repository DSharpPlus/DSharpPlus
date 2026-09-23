using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

internal sealed class RestInviteTargetUserBulkUpdatePayload
{
    [JsonProperty("user_ids")]
    public IEnumerable<ulong> UserIds { get; set; }
}
