using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a guild member's state within a voice channel context,
/// including their user information, roles, and voice-related settings.
/// </summary>
public class VoiceStateMember
{
    /// <summary>
    /// Gets or sets the user associated with this voice state member.
    /// </summary>
    [JsonPropertyName("user")]
    public VoiceStateUser User { get; set; }

    /// <summary>
    /// Gets or sets the list of role IDs assigned to the member.
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; }

    /// <summary>
    /// Gets or sets the ISO8601 timestamp indicating when the member
    /// started their Nitro subscription, if applicable.
    /// </summary>
    [JsonPropertyName("premium_since")]
    public string PremiumSince { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member's account
    /// is pending membership screening completion.
    /// </summary>
    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    /// <summary>
    /// Gets or sets the nickname of the member in the guild, if set.
    /// </summary>
    [JsonPropertyName("nick")]
    public string Nick { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member is muted
    /// server-wide.
    /// </summary>
    [JsonPropertyName("mute")]
    public bool Mute { get; set; }

    /// <summary>
    /// Gets or sets the ISO8601 timestamp of when the member joined the guild.
    /// </summary>
    [JsonPropertyName("joined_at")]
    public string JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets the member's flags as an integer bitset.
    /// </summary>
    [JsonPropertyName("flags")]
    public int Flags { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member is deafened
    /// server-wide.
    /// </summary>
    [JsonPropertyName("deaf")]
    public bool Deaf { get; set; }

    /// <summary>
    /// Gets or sets the ISO8601 timestamp until which the member's
    /// communication is disabled (e.g., timeout).
    /// </summary>
    [JsonPropertyName("communication_disabled_until")]
    public string CommunicationDisabledUntil { get; set; }

    /// <summary>
    /// Gets or sets the member's banner image URL, if set.
    /// </summary>
    [JsonPropertyName("banner")]
    public string Banner { get; set; }

    /// <summary>
    /// Gets or sets the member's avatar image URL, which may be distinct
    /// from their global Discord avatar.
    /// </summary>
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
}
