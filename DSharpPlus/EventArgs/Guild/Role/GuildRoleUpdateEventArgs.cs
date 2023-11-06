using DSharpPlus.Caching;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildRoleUpdated"/> event.
/// </summary>
public class GuildRoleUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild in which the update occurred.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild> Guild { get; internal set; }
    
    /// <summary>
    /// Gets the id of the guild in which the update occurred.
    /// </summary>
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// Gets the post-update role.
    /// </summary>
    public DiscordRole RoleAfter { get; internal set; }

    /// <summary>
    /// Gets the pre-update role. This value is null if the role was created.
    /// </summary>
    public DiscordRole? RoleBefore { get; internal set; }

    internal GuildRoleUpdateEventArgs() : base() { }
}
