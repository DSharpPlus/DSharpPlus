using System.Collections.Generic;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

internal class TransportListGuildSoundboardSounds
{
    [JsonProperty("items")]
    public IReadOnlyList<TransportSoundboardSound> Items { get; set; }
}
