using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a user's primary guild, which is the guild that the user has chosen to display as their "guild tag" on Discord.
/// </summary>
public class DiscordUserPrimaryGuild
{
    internal DiscordUserPrimaryGuild() { }
    
    /// <summary>
    /// The id of the user's primary guild
    /// </summary>
    [JsonProperty("identity_guild_id")]
    public ulong? IdentityGuildId { get; init; }

    /// <summary>
    /// Whether the user is displaying the primary guild's server tag. This can be <c>null</c> if the system clears the identity, e.g. because the server no longer supports tags.
    /// </summary>
    [JsonProperty("identity_enabled")]
    public bool? IdentityEnabled { get; init; }

    /// <summary>
    /// The text of the user's server tag. Limited to 4 characters
    /// </summary>
    [JsonProperty("tag")]
    public string? Tag { get; init; }

    /// <summary>
    /// The server tag badge hash
    /// </summary>
    [JsonProperty("badge")]
    public string? BadgeHash { get; init; }

    /// <summary>
    /// The URL of the user's server tag badge, if available. This will be <c>null</c> if the user does not have a server tag badge or if the <see cref="BadgeHash"/> is empty.
    /// </summary>
    [JsonIgnore]
    public string? BadgeUrl => string.IsNullOrWhiteSpace(this.BadgeHash)
        ? null
        : $"https://cdn.discordapp.com/guild-tag-badges/{this.IdentityGuildId}/{this.BadgeHash}.png";
}
