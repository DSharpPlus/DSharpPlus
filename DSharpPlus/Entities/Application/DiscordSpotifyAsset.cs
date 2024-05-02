
using System;

namespace DSharpPlus.Entities;
public sealed class DiscordSpotifyAsset : DiscordAsset
{
    /// <summary>
    /// Gets the URL of this asset.
    /// </summary>
    public override Uri Url
        => _url;

    private readonly Uri _url;

    public DiscordSpotifyAsset(string pId)
    {
        Id = pId;
        string[] ids = Id.Split(':');
        string id = ids[1];

        _url = new Uri($"https://i.scdn.co/image/{id}");
    }
}
