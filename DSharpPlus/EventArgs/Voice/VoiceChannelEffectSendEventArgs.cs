using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.EventArgs;

public class VoiceChannelEffectSendEventArgs : DiscordEventArgs
{
    internal DiscordClient Client { get; set; }
    
    /// <summary>
    ///  D of the channel the effect was sent in
    /// </summary>
    [JsonProperty("channel_id")]
    public ulong ChannelId { get; init; }

    /// <summary>
    /// ID of the guild the effect was sent in
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong GuildId { get; init; }

    /// <summary>
    /// ID of the user who sent the effect
    /// </summary>
    [JsonProperty("user_id")]
    public ulong UserId { get; init; }

    /// <summary>
    /// The emoji sent, for emoji reaction and soundboard effects
    /// </summary>
    [JsonProperty("emoji")]
    public DiscordEmoji? Emoji { get; init; }

    /// <summary>
    /// The type of emoji animation, for emoji reaction and soundboard effects
    /// </summary>
    [JsonProperty("animation_type")]
    public DiscordVoiceChannelEffectAnimationType? AnimationType { get; init; }

    /// <summary>
    /// The ID of the emoji animation, for emoji reaction and soundboard effects
    /// </summary>
    [JsonProperty("animation_id")]
    public int? AnimationId { get; init; }

    /// <summary>
    /// The ID of the soundboard sound, for soundboard effects
    /// </summary>
    [JsonProperty("sound_id")]
    public ulong? SoundId { get; init; }

    /// <summary>
    /// The ID of the soundboard sound, for soundboard effects
    /// </summary>
    [JsonProperty("sound_volume")]
    public double? Volume { get; init; }

    internal VoiceChannelEffectSendEventArgs(DiscordClient client) => this.Client = client;
    
    /// <summary>
    /// Gets the channel where the effect was sent.
    /// </summary>
    public async ValueTask<DiscordChannel> GetChannelAsync()
    {
        if (this.Client is null)
        {
            throw new InvalidOperationException("Client is not set.");
        }

        return await this.Client.GetChannelAsync(this.ChannelId);
    }
    
    /// <summary>
    /// Gets the guild where the effect was sent.
    /// </summary>
    public async ValueTask<DiscordGuild> GetGuildAsync()
    {
        if (this.Client is null)
        {
            throw new InvalidOperationException("Client is not set.");
        }

        return await this.Client.GetGuildAsync(this.GuildId);
    }
    
    
}
