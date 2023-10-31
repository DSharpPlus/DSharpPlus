using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildRoleDeleted"/> event.
/// </summary>
public class GuildRoleDeleteEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild in which the role was deleted. This value is null if the guild was not in cache.
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }
    
    /// <summary>
    /// Gets the id of the guild in which the role was deleted.
    /// </summary>
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// Gets the role that was deleted. This value is null if the role was not in cache.
    /// </summary>
    public DiscordRole? Role { get; internal set; }

    /// <summary>
    /// Gets the id of the role that was deleted.
    /// </summary>
    public ulong RoleId { get; set; }

    internal GuildRoleDeleteEventArgs() : base() { }
}
