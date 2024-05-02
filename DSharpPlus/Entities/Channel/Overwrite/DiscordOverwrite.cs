using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a permission overwrite for a channel.
/// </summary>
public class DiscordOverwrite : SnowflakeObject
{
    /// <summary>
    /// Gets the type of the overwrite. Either "role" or "member".
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordOverwriteType Type { get; internal set; }

    /// <summary>
    /// Gets the allowed permission set.
    /// </summary>
    [JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordPermissions Allowed { get; internal set; }

    /// <summary>
    /// Gets the denied permission set.
    /// </summary>
    [JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordPermissions Denied { get; internal set; }

    [JsonIgnore]
    internal ulong channelId;

    /// <summary>
    /// Deletes this channel overwrite.
    /// </summary>
    /// <param name="reason">Reason as to why this overwrite gets deleted.</param>
    /// <returns></returns>
    public async Task DeleteAsync(string? reason = null) => await Discord.ApiClient.DeleteChannelPermissionAsync(channelId, Id, reason);

    /// <summary>
    /// Updates this channel overwrite.
    /// </summary>
    /// <param name="allow">Permissions that are allowed.</param>
    /// <param name="deny">Permissions that are denied.</param>
    /// <param name="reason">Reason as to why you made this change.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermissions.ManageRoles"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the overwrite does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task UpdateAsync(DiscordPermissions? allow = null, DiscordPermissions? deny = null, string? reason = null)
        => await Discord.ApiClient.EditChannelPermissionsAsync(channelId, Id, allow ?? Allowed, deny ?? Denied, Type.ToString().ToLowerInvariant(), reason);

    /// <summary>
    /// Gets the DiscordMember that is affected by this overwrite.
    /// </summary>
    /// <returns>The DiscordMember that is affected by this overwrite</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermissions.AccessChannels"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the overwrite does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMember> GetMemberAsync() => Type != DiscordOverwriteType.Member
            ? throw new ArgumentException(nameof(Type), "This overwrite is for a role, not a member.")
            : await (await Discord.ApiClient.GetChannelAsync(channelId)).Guild.GetMemberAsync(Id);

    /// <summary>
    /// Gets the DiscordRole that is affected by this overwrite.
    /// </summary>
    /// <returns>The DiscordRole that is affected by this overwrite</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the role does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordRole> GetRoleAsync() => Type != DiscordOverwriteType.Role
            ? throw new ArgumentException(nameof(Type), "This overwrite is for a member, not a role.")
            : (await Discord.ApiClient.GetChannelAsync(channelId)).Guild.GetRole(Id);

    internal DiscordOverwrite() { }

    /// <summary>
    /// Checks whether given permissions are allowed, denied, or not set.
    /// </summary>
    /// <param name="permission">Permissions to check.</param>
    /// <returns>Whether given permissions are allowed, denied, or not set.</returns>
    public DiscordPermissionLevel CheckPermission(DiscordPermissions permission) => (Allowed & permission) != 0
            ? DiscordPermissionLevel.Allowed
            : (Denied & permission) != 0 ? DiscordPermissionLevel.Denied : DiscordPermissionLevel.Unset;
}
