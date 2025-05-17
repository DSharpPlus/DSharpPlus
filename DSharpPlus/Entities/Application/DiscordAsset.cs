using System;

namespace DSharpPlus.Entities;

public abstract class DiscordAsset
{
    /// <summary>
    /// Gets the ID of this asset.
    /// </summary>
    public virtual string Id { get; set; } = default!;

    /// <summary>
    /// Gets the URL of this asset.
    /// </summary>
    public abstract Uri Url { get; }
}
