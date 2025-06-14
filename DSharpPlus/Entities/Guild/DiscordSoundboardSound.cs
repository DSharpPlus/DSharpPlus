using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Entities;

public class DiscordSoundboardSound : SnowflakeObject
{
    internal DiscordSoundboardSound() { }

    internal DiscordSoundboardSound(TransportSoundboardSound transportSoundboardSound, BaseDiscordClient client)
    {
        this.Id = transportSoundboardSound.Id;
        this.Name = transportSoundboardSound.Name;
        this.GuildId = transportSoundboardSound.GuildId;
        this.UserId = transportSoundboardSound.User?.Id;
        this.Volume = transportSoundboardSound.Volume;
        this.Discord = client;

        if (transportSoundboardSound.EmojiId is not null &&
            DiscordEmoji.TryFromGuildEmote(client, transportSoundboardSound.EmojiId.Value, out DiscordEmoji emoji, this.GuildId))
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
    /// The id of the guild the sound belongs to. This is null if the sound is a default sound.
    /// </summary>
    public ulong? GuildId { get; internal set; }

    /// <summary>
    /// Volume of the sound between 1 and 0
    /// </summary>
    public double Volume { get; internal set; }

    /// <summary>
    /// The emoji associated with the sound
    /// </summary>
    public DiscordEmoji? Emoji { get; internal set; }

    /// <summary>
    /// The id of the user who created the soundboard.
    /// This is null if the bot does not have the <see cref="DiscordPermission.CreateGuildExpressions"/>
    /// or <see cref="DiscordPermission.ManageGuildExpressions"/> permission or if the sound is a default one.
    /// </summary>
    public ulong? UserId { get; internal set; }

    /// <summary>
    /// Gets the guild this sound belongs to, or <c>null</c> if the sound is a default sound.
    /// </summary>
    /// <param name="skipCache">
    /// If set to <c>true</c>, this method will bypass any cached data and fetch the guild directly from the Discord API.
    /// If set to <c>false</c>, the method will attempt to retrieve the guild from the local cache first, and only query the API if it is not found in the cache.
    /// </param>
    public async ValueTask<DiscordGuild?> GetGuildAsync(bool skipCache = false)
    {
        if (this.GuildId is null)
        {
            return null;
        }
        
        if (!skipCache && this.Discord.Guilds.TryGetValue(this.GuildId.Value, out DiscordGuild? guild))
        {
            return guild;
        }

        return await this.Discord.ApiClient.GetGuildAsync(this.GuildId.Value, null);
    }

    /// <summary>
    /// Retrieves the user who created this sound.
    /// </summary>
    /// <param name="skipCache">
    /// If set to <c>true</c>, this method will bypass any cached data and fetch the user directly from the Discord API.
    /// If set to <c>false</c>, the method will attempt to retrieve the user from the local cache first, and only query the API if the user is not found in the cache.
    /// </param>
    /// <returns>
    /// The user who created the sound, if known. If the user is in cache, this may be a <see cref="DiscordMember"/>.
    /// Returns <c>null</c> if the bot does not have the <see cref="DiscordPermission.CreateGuildExpressions"/> or <see cref="DiscordPermission.ManageGuildExpressions"/> permission, or if the sound is a default one.
    /// </returns>
    public async ValueTask<DiscordUser?> GetUserAsync(bool skipCache = false)
    {
        if (this.UserId is null)
        {
            return null;
        }

        if (!skipCache && this.GuildId is not null && this.Discord.Guilds.TryGetValue(this.GuildId.Value, out DiscordGuild? guild))
        {
            if (guild.members.TryGetValue(this.UserId.Value, out DiscordMember? member))
            {
                return member;
            }
        }

        if (!skipCache && this.Discord.UserCache.TryGetValue(this.UserId.Value, out DiscordUser? user))
        {
            return user;
        }

        return await this.Discord.ApiClient.GetUserAsync(this.UserId.Value);
    }
}
