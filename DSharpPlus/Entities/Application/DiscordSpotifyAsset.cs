using System;

namespace DSharpPlus.Entities;

public sealed class DiscordSpotifyAsset : DiscordAsset
{
    /// <summary>
    /// Gets the URL of this asset.
    /// </summary>
    public override Uri Url
        => this.url;

    private readonly Uri url;

    public DiscordSpotifyAsset(string pId)
    {
        this.Id = pId;
        string[] ids = this.Id.Split(':');
        string id = ids[1];

        this.url = new Uri($"https://i.scdn.co/image/{id}");
    }
}
