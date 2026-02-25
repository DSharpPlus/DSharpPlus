using Newtonsoft.Json;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents metadata for <see cref="RatelimitedEventArgs"/> if it was a Request Guild Member operation that got ratelimited.
/// </summary>
public sealed class RequestGuildMembersRatelimitMetadata : RatelimitMetadata
{
    /// <summary>
    /// The ID of the guild the bot attempted to get members for.
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong GuildId { get; private set; }

    /// <summary>
    /// The nonce of the original request, to identify which request got ratelimited.
    /// </summary>
    [JsonProperty("nonce")]
    public string? Nonce {  get; private set; }
}
