using Newtonsoft.Json;

namespace DSharpPlus.Entities;

using System.Threading.Tasks;
using Caching;

/// <summary>
/// Represents a Discord guild's widget settings.
/// </summary>
public class DiscordWidgetSettings
{
    /// <summary>
    /// DicordClient associated with this entity.
    /// </summary>
    public BaseDiscordClient Discord { get; internal set; }
    
    /// <summary>
    /// Guild associated with this entity.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild> Guild { get; internal set; }

    /// <summary>
    /// Gets the guild's widget channel id.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the guild's widget channel.
    /// </summary>
    public async ValueTask<DiscordChannel> GetChannelAsync()
    {
        DiscordChannel? cachedChannel = await this.Discord.Cache.TryGetChannelAsync(this.ChannelId);
        if (cachedChannel is not null)
        {
            return cachedChannel;
        }
        else
        {
            return await this.Discord.GetChannelAsync(this.ChannelId);
        }
    }
    
    /// <summary>
    /// Gets if the guild's widget is enabled.
    /// </summary>
    [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsEnabled { get; internal set; }
}
