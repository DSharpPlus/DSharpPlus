namespace DSharpPlus.Entities;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;

/// <summary>
/// Represents a discord role, to which users can be assigned.
/// </summary>
public class DiscordRole : SnowflakeObject, IEquatable<DiscordRole>
{
    /// <summary>
    /// Gets the name of this role.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the color of this role.
    /// </summary>
    [JsonIgnore]
    public DiscordColor Color
        => new(_color);

    [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
    internal int _color;

    /// <summary>
    /// Gets whether this role is hoisted.
    /// </summary>
    [JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsHoisted { get; internal set; }

    /// <summary>
    /// The url for this role's icon, if set.
    /// </summary>
    public string IconUrl => IconHash != null ? $"https://cdn.discordapp.com/role-icons/{Id}/{IconHash}.png" : null;

    /// <summary>
    /// The hash of this role's icon, if any.
    /// </summary>
    [JsonProperty("icon")]
    public string IconHash { get; internal set; }

    /// <summary>
    /// The emoji associated with this role's icon, if set.
    /// </summary>
    public DiscordEmoji Emoji => _emoji != null ? DiscordEmoji.FromUnicode(_emoji) : null;

    [JsonProperty("unicode_emoji")]
    internal string _emoji;

    /// <summary>
    /// Gets the position of this role in the role hierarchy.
    /// </summary>
    [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
    public int Position { get; internal set; }

    /// <summary>
    /// Gets the permissions set for this role.
    /// </summary>
    [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordPermissions Permissions { get; internal set; }

    /// <summary>
    /// Gets whether this role is managed by an integration.
    /// </summary>
    [JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsManaged { get; internal set; }

    /// <summary>
    /// Gets whether this role is mentionable.
    /// </summary>
    [JsonProperty("mentionable", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsMentionable { get; internal set; }

    /// <summary>
    /// Gets the tags this role has.
    /// </summary>
    [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordRoleTags Tags { get; internal set; }

    [JsonIgnore]
    internal ulong _guild_id = 0;

    /// <summary>
    /// Gets a mention string for this role. If the role is mentionable, this string will mention all the users that belong to this role.
    /// </summary>
    public string Mention
        => Formatter.Mention(this);

    #region Methods
    /// <summary>
    /// Modifies this role's position.
    /// </summary>
    /// <param name="position">New position</param>
    /// <param name="reason">Reason why we moved it</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the role does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ModifyPositionAsync(int position, string reason = null)
    {
        DiscordRole[] roles = Discord.Guilds[_guild_id].Roles.Values.OrderByDescending(xr => xr.Position).ToArray();
        RestGuildRoleReorderPayload[] pmds = new RestGuildRoleReorderPayload[roles.Length];
        for (int i = 0; i < roles.Length; i++)
        {
            pmds[i] = new RestGuildRoleReorderPayload
            {
                RoleId = roles[i].Id,
                Position = roles[i].Id == Id ? position : roles[i].Position <= position ? roles[i].Position - 1 : roles[i].Position
            };
        }

        await Discord.ApiClient.ModifyGuildRolePositionsAsync(_guild_id, pmds, reason);
    }

    /// <summary>
    /// Updates this role.
    /// </summary>
    /// <param name="name">New role name</param>
    /// <param name="permissions">New role permissions</param>
    /// <param name="color">New role color</param>
    /// <param name="hoist">New role hoist</param>
    /// <param name="mentionable">Whether this role is mentionable</param>
    /// <param name="reason">Reason why we made this change</param>
    /// <param name="icon">The icon to add to this role</param>
    /// <param name="emoji">The emoji to add to this role. Must be unicode.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the role does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ModifyAsync(string name = null, DiscordPermissions? permissions = null, DiscordColor? color = null, bool? hoist = null, bool? mentionable = null, string reason = null, Stream icon = null, DiscordEmoji emoji = null)
        => await Discord.ApiClient.ModifyGuildRoleAsync(_guild_id, Id, name, permissions, color?.Value, hoist, mentionable, icon, emoji?.ToString(), reason);

    /// <exception cref = "Exceptions.UnauthorizedException" > Thrown when the client does not have the<see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the role does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task ModifyAsync(Action<RoleEditModel> action)
    {
        RoleEditModel mdl = new RoleEditModel();
        action(mdl);

        return ModifyAsync(mdl.Name, mdl.Permissions, mdl.Color, mdl.Hoist, mdl.Mentionable, mdl.AuditLogReason, mdl.Icon, mdl.Emoji);
    }

    /// <summary>
    /// Deletes this role.
    /// </summary>
    /// <param name="reason">Reason as to why this role has been deleted.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the role does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteAsync(string? reason = null)
        => await Discord.ApiClient.DeleteRoleAsync(_guild_id, Id, reason);
    #endregion

    internal DiscordRole() { }

    /// <summary>
    /// Checks whether this role has specific permissions.
    /// </summary>
    /// <param name="permission">Permissions to check for.</param>
    /// <returns>Whether the permissions are allowed or not.</returns>
    public DiscordPermissionLevel CheckPermission(DiscordPermissions permission)
        => (Permissions & permission) != 0 ? DiscordPermissionLevel.Allowed : DiscordPermissionLevel.Unset;

    /// <summary>
    /// Returns a string representation of this role.
    /// </summary>
    /// <returns>String representation of this role.</returns>
    public override string ToString() => $"Role {Id}; {Name}";

    /// <summary>
    /// Checks whether this <see cref="DiscordRole"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordRole"/>.</returns>
    public override bool Equals(object obj) => Equals(obj as DiscordRole);

    /// <summary>
    /// Checks whether this <see cref="DiscordRole"/> is equal to another <see cref="DiscordRole"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordRole"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordRole"/> is equal to this <see cref="DiscordRole"/>.</returns>
    public bool Equals(DiscordRole e)
        => e switch
        {
            null => false,
            _ => ReferenceEquals(this, e) || Id == e.Id
        };

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordRole"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordRole"/>.</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="DiscordRole"/> objects are equal.
    /// </summary>
    /// <param name="e1">First role to compare.</param>
    /// <param name="e2">Second role to compare.</param>
    /// <returns>Whether the two roles are equal.</returns>
    public static bool operator ==(DiscordRole e1, DiscordRole e2)
        => (e1 is null) == (e2 is null)
        && ((e1 is null && e2 is null) || e1.Id == e2.Id);

    /// <summary>
    /// Gets whether the two <see cref="DiscordRole"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First role to compare.</param>
    /// <param name="e2">Second role to compare.</param>
    /// <returns>Whether the two roles are not equal.</returns>
    public static bool operator !=(DiscordRole e1, DiscordRole e2)
        => !(e1 == e2);
}
