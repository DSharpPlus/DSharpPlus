using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a soundboard sound
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class DiscordSoundboardSound : SnowflakeObject
{
    internal DiscordSoundboardSound() { }

    // We overwrite the id property because of the different json key
    /// <summary>
    /// Gets the ID of this object.
    /// </summary>
    [JsonProperty("sound_id", NullValueHandling = NullValueHandling.Ignore)]
    public new ulong Id { get; internal set; }

    /// <summary>
    /// The name of the sound.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }

    /// <summary>
    /// The id of the guild the sound belongs to. This is null if the sound is a default sound.
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong? GuildId { get; internal set; }

    /// <summary>
    /// Volume of the sound between 1 and 0.
    /// </summary>
    [JsonProperty("volume")]
    public double Volume { get; internal set; }

    /// <summary>
    /// The id of the emoji associated with the sound.
    /// </summary>
    [JsonProperty("emoji_id")]
    public ulong? EmojiId { get; internal set; }
    
    /// <summary>
    /// The name of the emoji associated with the sound.
    /// </summary>
    [JsonProperty("emoji_name")]
    public string? EmojiName { get; internal set; }
    
    [JsonProperty("user")]
    internal TransportUser User {
        set => this.UserId = value.Id;
    }

    /// <summary>
    /// The id of the user who created the soundboard.
    /// This is null if the bot does not have the <see cref="DiscordPermission.CreateGuildExpressions"/>
    /// or <see cref="DiscordPermission.ManageGuildExpressions"/> permission or if the sound is a default one.
    /// </summary>
    public ulong? UserId { get; internal set; }

    /// <summary>
    /// Gets the guild this sound belongs to, or <c>null</c> if the sound is a default sound.
    /// </summary>
    /// <param name="skipCache">If set to true this method will skip all caches and always perform a rest api call</param>
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
    /// <param name="skipCache">If set to true this method will skip all caches and always perform a rest api call</param>
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

        if (!skipCache && this.GuildId is not null &&
            this.Discord.Guilds.TryGetValue(this.GuildId.Value, out DiscordGuild? guild))
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

    /// <summary>
    /// Modifies this soundboard sound.
    /// </summary>
    /// <param name="action">Action to perform on this soundboard sound</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuildExpressions"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the soundboard sound does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordSoundboardSound> ModifyAsync(Action<SoundboardSoundEditModel> action)
    {
        if (this.GuildId is null)
        {
            throw new InvalidOperationException("Cannot modify a default soundboard sound.");
        }

        SoundboardSoundEditModel mdl = new() {Name = this.Name, Volume = this.Volume};
        action(mdl);

        Optional<ulong>? emojiId = null;
        Optional<string>? emojiName = null;

        if (mdl.Emoji.HasValue)
        {
            if (mdl.Emoji.Value.Id == 0)
            {
                emojiName = mdl.Emoji.Value.Name;
                emojiId = new Optional<ulong>();
            }
            else
            {
                emojiId = mdl.Emoji.Value.Id;
                emojiName = new Optional<string>();
            }
        }

        return await this.Discord.ApiClient.ModifyGuildSoundboardSoundAsync
        (
            this.GuildId.Value,
            this.Id,
            mdl.Name,
            mdl.Volume,
            emojiId,
            emojiName
        );
    }
    
    /// <summary>
    /// Deletes this soundboard sound.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the sound is a default sound.</exception>
    public async ValueTask DeleteAsync()
    {
        if (this.GuildId is null)
        {
            throw new InvalidOperationException("Cannot delete a default soundboard sound.");
        }

        await this.Discord.ApiClient.DeleteGuildSoundboardSoundAsync(this.GuildId.Value, this.Id);
    }

    /// <inheritdoc />
    public override string ToString() => $"{this.Name} ({this.Id}) - Volume: {this.Volume}, Emoji: {this.EmojiName ?? this.EmojiId?.ToString() ?? "None"}";
}
