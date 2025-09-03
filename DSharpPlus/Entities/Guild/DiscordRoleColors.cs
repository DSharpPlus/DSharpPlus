using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the colors of a role with the new role color system.
/// </summary>
public class DiscordRoleColors
{
    /// <summary>
    /// Gets the primary color of this role.
    /// </summary>
    [JsonIgnore]
    public DiscordColor PrimaryColor
        => new(this.primaryColor);

    [JsonProperty("primary_color", NullValueHandling = NullValueHandling.Ignore)]
    internal int primaryColor;
   
    /// <summary>
    /// Gets the secondary color of this role.
    /// </summary>
    [JsonIgnore]
    public DiscordColor SecondaryColor
        => new(this.secondaryColor);

    [JsonProperty("secondary_color", NullValueHandling = NullValueHandling.Ignore)]
    internal int secondaryColor;
   
    /// <summary>
    /// Gets the tertiary color of this role. This is only set when the role has the holographic style.
    /// And the values will be predefined by Discord.
    /// </summary>
    [JsonIgnore]
    public DiscordColor TertiaryColor
        => new(this.tertiaryColor);

    [JsonProperty("tertiary_color", NullValueHandling = NullValueHandling.Ignore)]
    internal int tertiaryColor;
}
