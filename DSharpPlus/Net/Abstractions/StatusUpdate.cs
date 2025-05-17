using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

/// <summary>
/// Represents data for websocket status update payload.
/// </summary>
internal sealed class StatusUpdate
{
    /// <summary>
    /// Gets or sets the unix millisecond timestamp of when the user went idle.
    /// </summary>
    [JsonProperty("since", NullValueHandling = NullValueHandling.Include)]
    public long? IdleSince { get; set; }

    /// <summary>
    /// Gets or sets whether the user is AFK.
    /// </summary>
    [JsonProperty("afk")]
    public bool IsAFK { get; set; }

    /// <summary>
    /// Gets or sets the status of the user.
    /// </summary>
    [JsonIgnore]
    public DiscordUserStatus Status { get; set; } = DiscordUserStatus.Online;

    [JsonProperty("status")]
    internal string StatusString => this.Status switch
    {
        DiscordUserStatus.Online => "online",
        DiscordUserStatus.Idle => "idle",
        DiscordUserStatus.DoNotDisturb => "dnd",
        DiscordUserStatus.Invisible or DiscordUserStatus.Offline => "invisible",
        _ => "online",
    };

    /// <summary>
    /// Gets or sets the game the user is playing.
    /// </summary>
    [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
    public TransportActivity Activity { get; set; }

    internal DiscordActivity activity;
}
