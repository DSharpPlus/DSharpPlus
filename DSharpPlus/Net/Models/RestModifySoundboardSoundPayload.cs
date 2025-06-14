using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net;

internal sealed record RestModifySoundboardSoundPayload
{
    [JsonProperty("name")]
    public required string Name { get; init; }

    [JsonProperty("volume")]
    public Optional<double>? Volume { get; init; }

    [JsonProperty("emoji_id")]
    public Optional<ulong>? EmojiId { get; init; }

    [JsonProperty("emoji_name")]
    public Optional<string>? EmojiName { get; init; }
}
