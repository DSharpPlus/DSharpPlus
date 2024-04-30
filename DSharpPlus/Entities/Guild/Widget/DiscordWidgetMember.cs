namespace DSharpPlus.Entities;

using Newtonsoft.Json;

/// <summary>
/// Represents a member within a Discord guild's widget.
/// </summary>
public class DiscordWidgetMember
{
    /// <summary>
    /// Gets the member's identifier within the widget.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong Id { get; internal set; }

    /// <summary>
    /// Gets the member's username.
    /// </summary>
    [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
    public string Username { get; internal set; }

    /// <summary>
    /// Gets the member's discriminator.
    /// </summary>
    [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
    public string Discriminator { get; internal set; }

    /// <summary>
    /// Gets the member's avatar.
    /// </summary>
    [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
    public string Avatar { get; internal set; }

    /// <summary>
    /// Gets the member's online status.
    /// </summary>
    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
    public string Status { get; internal set; }

    /// <summary>
    /// Gets the member's avatar url.
    /// </summary>
    [JsonProperty("avatar_url", NullValueHandling = NullValueHandling.Ignore)]
    public string AvatarUrl { get; internal set; }
}
