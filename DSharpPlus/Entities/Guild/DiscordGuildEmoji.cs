namespace DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

public sealed class DiscordGuildEmoji : DiscordEmoji
{
    /// <summary>
    /// Gets the user that created this emoji.
    /// </summary>
    [JsonIgnore]
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// Gets the guild to which this emoji belongs.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild Guild { get; internal set; }

    internal DiscordGuildEmoji() { }

    /// <summary>
    /// Modifies this emoji.
    /// </summary>
    /// <param name="name">New name for this emoji.</param>
    /// <param name="roles">Roles for which this emoji will be available. This works only if your application is whitelisted as integration.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns>The modified emoji.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojis"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordGuildEmoji> ModifyAsync(string name, IEnumerable<DiscordRole> roles = null, string reason = null)
        => Guild.ModifyEmojiAsync(this, name, roles, reason);

    /// <summary>
    /// Deletes this emoji.
    /// </summary>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojis"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task DeleteAsync(string reason = null)
        => Guild.DeleteEmojiAsync(this, reason);
}
