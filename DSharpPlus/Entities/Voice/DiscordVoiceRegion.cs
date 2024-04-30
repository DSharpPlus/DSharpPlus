namespace DSharpPlus.Entities;

using Newtonsoft.Json;

/// <summary>
/// Represents information about a Discord voice server region.
/// </summary>
public class DiscordVoiceRegion
{
    /// <summary>
    /// Gets the unique ID for the region.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string Id { get; internal set; }

    /// <summary>
    /// Gets the name of the region.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets an example server hostname for this region.
    /// </summary>
    [JsonProperty("sample_hostname", NullValueHandling = NullValueHandling.Ignore)]
    public string SampleHostname { get; internal set; }

    /// <summary>
    /// Gets an example server port for this region.
    /// </summary>
    [JsonProperty("sample_port", NullValueHandling = NullValueHandling.Ignore)]
    public int SamplePort { get; internal set; }

    /// <summary>
    /// Gets whether this is a VIP-only region.
    /// </summary>
    [JsonProperty("vip", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsVIP { get; internal set; }

    /// <summary>
    /// Gets whether this region is the most optimal for the current user.
    /// </summary>
    [JsonProperty("optimal", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsOptimal { get; internal set; }

    /// <summary>
    /// Gets whether this voice region is deprecated.
    /// </summary>
    [JsonProperty("deprecated", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsDeprecated { get; internal set; }

    /// <summary>
    /// Gets whether this is a custom voice region.
    /// </summary>
    [JsonProperty("custom", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsCustom { get; internal set; }

    /// <summary>
    /// Gets whether two <see cref="DiscordVoiceRegion"/>s are equal.
    /// </summary>
    /// <param name="region">The region to compare with.</param>
    /// <returns></returns>
    public bool Equals(DiscordVoiceRegion region)
        => this == region;

    public override bool Equals(object obj) => Equals(obj as DiscordVoiceRegion);

    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="DiscordVoiceRegion"/> objects are equal.
    /// </summary>
    /// <param name="left">First voice region to compare.</param>
    /// <param name="right">Second voice region to compare.</param>
    /// <returns>Whether the two voice regions are equal.</returns>
    public static bool operator ==(DiscordVoiceRegion left, DiscordVoiceRegion right)
    {
        object? o1 = left as object;
        object? o2 = right as object;

        return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || left.Id == right.Id);
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordVoiceRegion"/> objects are not equal.
    /// </summary>
    /// <param name="left">First voice region to compare.</param>
    /// <param name="right">Second voice region to compare.</param>
    /// <returns>Whether the two voice regions are not equal.</returns>
    public static bool operator !=(DiscordVoiceRegion left, DiscordVoiceRegion right)
        => !(left == right);

    internal DiscordVoiceRegion() { }
}
