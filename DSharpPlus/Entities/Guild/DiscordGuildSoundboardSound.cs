using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Entities;

public class DiscordGuildSoundboardSound : SnowflakeObject
{
    internal DiscordGuildSoundboardSound() { }

    internal DiscordGuildSoundboardSound(TransportSoundboardSound transportSoundboardSound, BaseDiscordClient client)
    {
        this.Id = transportSoundboardSound.Id;
        this.Name = transportSoundboardSound.Name;
        this.GuildId = transportSoundboardSound.GuildId;
        this.UserId = transportSoundboardSound.User?.Id;
        this.Volume = transportSoundboardSound.Volume;
        this.Discord = client;

        if (transportSoundboardSound.EmojiId is not null &&
            DiscordEmoji.TryFromGuildEmote(client, transportSoundboardSound.EmojiId.Value, out DiscordEmoji emoji))
        {
            this.Emoji = emoji;
        }
        else if (!string.IsNullOrWhiteSpace(transportSoundboardSound.EmojiName) &&
                 DiscordEmoji.TryFromName(client, transportSoundboardSound.EmojiName, false, out emoji))
        {
            this.Emoji = emoji;
        }
    }

    /// <summary>
    /// The name of the sound
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// The id of the guild the sound belongs 
    /// </summary>
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// Volume of the sound between 1 and 0
    /// </summary>
    public double Volume { get; internal set; }

    /// <summary>
    /// The emoji associated with the sound
    /// </summary>
    public DiscordEmoji? Emoji { get; internal set; }

    /// <summary>
    /// The id of the user who created the soundboard
    /// </summary>
    public ulong? UserId { get; internal set; }

    /// <summary>
    /// Get the guild this sound belongs to
    /// </summary>
    /// <param name="skipCache">If the guild should be fetched and the cache skiped </param>
    public async ValueTask<DiscordGuild?> GetGuildAsync(bool skipCache = false)
    {
        if (!skipCache && this.Discord.Guilds.TryGetValue(this.GuildId, out DiscordGuild? guild))
        {
            return guild;
        }

        return await this.Discord.ApiClient.GetGuildAsync(this.GuildId, null);
    }

    /// <summary>
    /// Retrieves the user who created this sound
    /// </summary>
    /// <param name="skipCache">If the user should be fetched and cache skiped</param>
    /// <returns>
    /// Returns the user who created the sound if the user is known. If the user is in cache it can be a DiscordMember
    /// </returns>
    public async ValueTask<DiscordUser?> GetUserAsync(bool skipCache = false)
    {
        if (this.UserId is null)
        {
            return null;
        }

        if (!skipCache && this.Discord.Guilds.TryGetValue(this.GuildId, out DiscordGuild? guild))
        {
            if (guild.members.TryGetValue(this.UserId.Value, out DiscordMember? member))
            {
                return member;
            }
        }

        if (!skipCache && this.Discord.UserCache.TryGetValue(this.GuildId, out DiscordUser? user))
        {
            return user;
        }

        return await this.Discord.ApiClient.GetUserAsync(this.UserId.Value);
    }
}
