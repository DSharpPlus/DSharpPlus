
using Newtonsoft.Json;

namespace DSharpPlus.Entities;
/// <summary>
/// Represents media for a poll. It is the backplane for poll options.
/// </summary>
public sealed class DiscordPollMedia
{
    /// <summary>
    /// Gets the text for the field. 
    /// </summary>
    /// <remarks>
    /// For questions, the maximum length of this is 300 characters. <br/>
    /// For answers, the maximum is 55. This is subject to change from Discord, however. <br/> <br/>
    /// Despite nullability, this field should always be non-null. This is also subject to change from Discord.
    /// </remarks>
    [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
    public string? Text { get; internal set; }

    /// <summary>
    /// Gets the emoji for the field, if any.
    /// </summary>
    [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordComponentEmoji? Emoji { get; internal set; }

    internal DiscordPollMedia() { }
}
