using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the guild permissions for a application command.
/// </summary>
public class DiscordGuildApplicationCommandPermissions : SnowflakeObject
{
    /// <summary>
    /// Gets the id of the application the command belongs to.
    /// </summary>
    [JsonProperty("application_id")]
    public ulong ApplicationId { get; internal set; }

    /// <summary>
    /// Gets the id of the guild.
    /// </summary>
    [JsonProperty("guild_id")]
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// Gets the guild.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild Guild
        => (this.Discord as DiscordClient).InternalGetCachedGuild(this.GuildId);

    /// <summary>
    /// Gets the permissions for the application command in the guild.
    /// </summary>
    [JsonProperty("permissions")]
    public IReadOnlyList<DiscordApplicationCommandPermission> Permissions { get; internal set; }

    internal DiscordGuildApplicationCommandPermissions() { }

    /// <summary>
    /// Represents the guild application command permissions for a application command.
    /// </summary>
    /// <param name="commandId">The id of the command.</param>
    /// <param name="permissions">The permissions for the application command.</param>
    public DiscordGuildApplicationCommandPermissions(ulong commandId, IEnumerable<DiscordApplicationCommandPermission> permissions)
    {
        this.Id = commandId;
        this.Permissions = permissions.ToList();
    }
}
