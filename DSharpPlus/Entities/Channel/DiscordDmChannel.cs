using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <inheritdoc />
/// <summary>
/// Represents a direct message channel.
/// </summary>
public class DiscordDmChannel : DiscordChannel
{
    /// <summary>
    /// Gets the recipients of this direct message.
    /// </summary>
    [JsonProperty("recipients", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordUser> Recipients { get; internal set; }

    /// <summary>
    /// Gets the hash of this channel's icon.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
    public string IconHash { get; internal set; }

    /// <summary>
    /// Gets the ID of this direct message's creator.
    /// </summary>
    [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong OwnerId { get; internal set; }

    /// <summary>
    /// Gets the application ID of the direct message's creator if it a bot.
    /// </summary>
    [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong ApplicationId { get; internal set; }

    /// <summary>
    /// Gets the URL of this channel's icon.
    /// </summary>
    [JsonIgnore]
    public string IconUrl
        => !string.IsNullOrWhiteSpace(this.IconHash) ? $"https://cdn.discordapp.com/channel-icons/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png" : null;

    /// <summary>
    /// Only use for Group DMs! Whitelisted bots only. Requires user's oauth2 access token
    /// </summary>
    /// <param name="user_id">The ID of the user to add.</param>
    /// <param name="accesstoken">The OAuth2 access token.</param>
    /// <param name="nickname">The nickname to give to the user.</param>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task AddDmRecipientAsync(ulong user_id, string accesstoken, string nickname)
        => await this.Discord.ApiClient.AddGroupDmRecipientAsync(this.Id, user_id, accesstoken, nickname);

    /// <summary>
    /// Only use for Group DMs!
    /// </summary>
    /// <param name="user_id">The ID of the User to remove.</param>
    /// <param name="accesstoken">The OAuth2 access token.</param>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task RemoveDmRecipientAsync(ulong user_id, string accesstoken)
        => await this.Discord.ApiClient.RemoveGroupDmRecipientAsync(this.Id, user_id);
}
