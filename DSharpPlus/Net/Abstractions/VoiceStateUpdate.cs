using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

/// <summary>
/// Represents data for websocket voice state update payload.
/// </summary>
internal sealed class VoiceStateUpdate
{
    /// <summary>
    /// Gets or sets the guild for which the user is updating their voice state.
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong GuildId { get; set; }

    /// <summary>
    /// Gets or sets the channel user wants to connect to. Null if disconnecting.
    /// </summary>
    [JsonProperty("channel_id")]
    public ulong? ChannelId { get; set; }

    /// <summary>
    /// Gets or sets whether the client is muted.
    /// </summary>
    [JsonProperty("self_mute")]
    public bool Mute { get; set; }

    /// <summary>
    /// Gets or sets whether the client is deafened.
    /// </summary>
    [JsonProperty("self_deaf")]
    public bool Deafen { get; set; }
}
