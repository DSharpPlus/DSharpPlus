using Newtonsoft.Json;

namespace DSharpPlus.Entities.Invite;
/// <summary>
/// Represents a role which the user will get when joining the invited guild.
/// </summary>
public class DiscordInviteRole : SnowflakeObject
{
    /// <summary>
    /// Gets the name of this role.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }
    /// <summary>
    /// Gets the position of this role in the role hierarchy.
    /// </summary>
    [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
    public int Position { get; internal set; }
    /// <summary>
    /// Gets the colors of this role
    /// </summary>
    public DiscordRoleColors Colors { get; internal set; }
    /// <summary>
    /// The url for this role's icon, if set.
    /// </summary>
    public string? IconUrl => this.IconHash != null ? $"https://cdn.discordapp.com/role-icons/{this.Id}/{this.IconHash}.png" : null;

    /// <summary>
    /// The hash of this role's icon, if any.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
    public string? IconHash { get; internal set; }
    /// <summary>
    /// The emoji associated with this role's icon, if set.
    /// </summary>
    public DiscordEmoji Emoji => this.emoji != null ? DiscordEmoji.FromUnicode(this.emoji) : null;

    [JsonProperty("unicode_emoji", NullValueHandling = NullValueHandling.Ignore)]
    internal string? emoji;
    /// <summary>
    /// Gets the permissions set for this role.
    /// </summary>
    [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordPermissions Permissions { get; internal set; }

    internal DiscordInviteRole() { }
}
