using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

internal class TransportSoundboardSound
{
    [JsonProperty("sound_id")]
    public ulong Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("guild_id")]
    public ulong GuildId { get; set; }

    [JsonProperty("volume")]
    public double Volume { get; set; }

    [JsonProperty("emoji_id")]
    public ulong? EmojiId { get; set; }

    [JsonProperty("emoji_name")]
    public string? EmojiName { get; set; }

    [JsonProperty("user")]
    public TransportUser? User { get; set; }
}
