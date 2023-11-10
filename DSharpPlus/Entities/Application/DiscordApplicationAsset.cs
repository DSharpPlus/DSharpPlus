using System;
using System.Globalization;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an asset for an OAuth2 application.
/// </summary>
public sealed class DiscordApplicationAsset : DiscordAsset, IEquatable<DiscordApplicationAsset>
{
    /// <summary>
    /// Gets the Discord client instance for this asset.
    /// </summary>
    internal BaseDiscordClient? Discord { get; set; }

    /// <summary>
    /// Gets the asset's name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; } = default!;

    /// <summary>
    /// Gets the asset's type.
    /// </summary>
    [JsonProperty("type")]
    public DiscordApplicationAssetType Type { get; internal set; }

    /// <summary>
    /// Gets the application this asset belongs to.
    /// </summary>
    public DiscordApplication Application { get; internal set; } = default!;

    /// <summary>
    /// Gets the Url of this asset.
    /// </summary>
    public override Uri Url
        => new($"https://cdn.discordapp.com/app-assets/{this.Application.Id.ToString(CultureInfo.InvariantCulture)}/{this.Id}.png");

    internal DiscordApplicationAsset() { }

    internal DiscordApplicationAsset(DiscordApplication app) => this.Discord = app.Discord;

    /// <summary>
    /// Checks whether this <see cref="DiscordApplicationAsset"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordApplicationAsset"/>.</returns>
    public override bool Equals(object? obj) => this.Equals(obj as DiscordApplicationAsset);

    /// <summary>
    /// Checks whether this <see cref="DiscordApplicationAsset"/> is equal to another <see cref="DiscordApplicationAsset"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordApplicationAsset"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordApplicationAsset"/> is equal to this <see cref="DiscordApplicationAsset"/>.</returns>
    public bool Equals(DiscordApplicationAsset? e) => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordApplication"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordApplication"/>.</returns>
    public override int GetHashCode() => this.Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="DiscordApplicationAsset"/> objects are equal.
    /// </summary>
    /// <param name="right">First application asset to compare.</param>
    /// <param name="left">Second application asset to compare.</param>
    /// <returns>Whether the two application assets not equal.</returns>
    public static bool operator ==(DiscordApplicationAsset right, DiscordApplicationAsset left)
    {
        return (right is not null || left is null)
            && (right is null || left is not null)
            && ((right is null && left is null)
                || right!.Id == left!.Id);
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordApplicationAsset"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First application asset to compare.</param>
    /// <param name="e2">Second application asset to compare.</param>
    /// <returns>Whether the two application assets are not equal.</returns>
    public static bool operator !=(DiscordApplicationAsset e1, DiscordApplicationAsset e2)
        => !(e1 == e2);
}
