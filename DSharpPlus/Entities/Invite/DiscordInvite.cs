
using System;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;
/// <summary>
/// Represents a Discord invite.
/// </summary>
public class DiscordInvite
{
    internal BaseDiscordClient Discord { get; set; }

    /// <summary>
    /// Gets the invite's code.
    /// </summary>
    [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
    public string Code { get; internal set; }

    /// <summary>
    /// Gets the guild this invite is for.
    /// </summary>
    [JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordInviteGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the channel this invite is for.
    /// </summary>
    [JsonProperty("channel", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordInviteChannel Channel { get; internal set; }

    /// <summary>
    /// Gets the partial user that is currently livestreaming.
    /// </summary>
    [JsonProperty("target_user", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUser TargetUser { get; internal set; }

    /// <summary>
    /// Gets the partial embedded application to open for a voice channel.
    /// </summary>
    [JsonProperty("target_application", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordApplication TargetApplication { get; internal set; }
    /// <summary>
    /// Gets the target application for this invite.
    /// </summary>
    [JsonProperty("target_type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordInviteTargetType? TargetType { get; internal set; }

    /// <summary>
    /// Gets the approximate guild online member count for the invite.
    /// </summary>
    [JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
    public int? ApproximatePresenceCount { get; internal set; }

    /// <summary>
    /// Gets the approximate guild total member count for the invite.
    /// </summary>
    [JsonProperty("approximate_member_count")]
    public int? ApproximateMemberCount { get; internal set; }

    /// <summary>
    /// Gets the user who created the invite.
    /// </summary>
    [JsonProperty("inviter", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUser Inviter { get; internal set; }

    /// <summary>
    /// Gets the number of times this invite has been used.
    /// </summary>
    [JsonProperty("uses", NullValueHandling = NullValueHandling.Ignore)]
    public int Uses { get; internal set; }

    /// <summary>
    /// Gets the max number of times this invite can be used.
    /// </summary>
    [JsonProperty("max_uses", NullValueHandling = NullValueHandling.Ignore)]
    public int MaxUses { get; internal set; }

    /// <summary>
    /// Gets duration in seconds after which the invite expires.
    /// </summary>
    [JsonProperty("max_age", NullValueHandling = NullValueHandling.Ignore)]
    public int MaxAge { get; internal set; }

    /// <summary>
    /// Gets whether this invite only grants temporary membership.
    /// </summary>
    [JsonProperty("temporary", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsTemporary { get; internal set; }

    /// <summary>
    /// Gets the date and time this invite was created.
    /// </summary>
    [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset CreatedAt { get; internal set; }

    /// <summary>
    /// Gets whether this invite is revoked.
    /// </summary>
    [JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsRevoked { get; internal set; }

    /// <summary>
    /// Gets the expiration date of this invite.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset? ExpiresAt
        => !string.IsNullOrWhiteSpace(ExpiresAtRaw) && DateTimeOffset.TryParse(ExpiresAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dto) ? dto : null;

    [JsonProperty("expires_at", NullValueHandling = NullValueHandling.Ignore)]
    internal string ExpiresAtRaw { get; set; }

    /// <summary>
    /// Gets stage instance data for this invite if it is for a stage instance channel.
    /// </summary>
    [JsonProperty("stage_instance")]
    public DiscordStageInvite StageInstance { get; internal set; }

    internal DiscordInvite() { }

    /// <summary>
    /// Deletes the invite.
    /// </summary>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission or the <see cref="Permissions.ManageGuild"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the emoji does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordInvite> DeleteAsync(string reason = null)
        => await Discord.ApiClient.DeleteInviteAsync(Code, reason);

    /*
     * Disabled due to API restrictions.
     *
     * /// <summary>
     * /// Accepts an invite. Not available to bot accounts. Requires "guilds.join" scope or user token. Please note that accepting these via the API will get your account unverified.
     * /// </summary>
     * /// <returns></returns>
     * [Obsolete("Using this method will get your account unverified.")]
     * public Task<DiscordInvite> AcceptAsync()
     *     => this.Discord._rest_client.InternalAcceptInvite(Code);
     */

    /// <summary>
    /// Converts this invite into an invite link.
    /// </summary>
    /// <returns>A discord.gg invite link.</returns>
    public override string ToString() => $"https://discord.gg/{Code}";
}
