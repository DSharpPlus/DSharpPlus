using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a channel in a welcome screen
/// </summary>
public class DiscordGuildWelcomeScreenChannel
{
    /// <summary>
    /// Gets the id of the channel.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the description shown for the channel.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; internal set; }

    /// <summary>
    /// Gets the emoji id if the emoji is custom, when applicable.
    /// </summary>
    [JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? EmojiId { get; internal set; }

    /// <summary>
    /// Gets the name of the emoji if custom or the unicode character if standard, when applicable.
    /// </summary>
    [JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Ignore)]
    public string EmojiName { get; internal set; }

    public DiscordGuildWelcomeScreenChannel(ulong channelId, string description, DiscordEmoji emoji = null)
    {
        this.ChannelId = channelId;
        this.Description = description;
        if (emoji != null)
        {
            if (emoji.Id == 0)
            {
                this.EmojiName = emoji.Name;
            }
            else
            {
                this.EmojiId = emoji.Id;
            }
        }
    }
}
