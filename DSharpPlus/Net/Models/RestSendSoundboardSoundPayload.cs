using Newtonsoft.Json;

namespace DSharpPlus.Net;

internal sealed record RestSendSoundboardSoundPayload
{
    /// <summary>
    /// The ID of the soundboard sound to play.
    /// </summary>
    [JsonProperty("sound_id")]
    public required ulong SoundId { get; init; }

    /// <summary>
    /// The ID of the guild where the soundboard sound is located.
    /// </summary>
    [JsonProperty("source_guild_id")]
    public required ulong? SourceGuildId { get; init; }
}
