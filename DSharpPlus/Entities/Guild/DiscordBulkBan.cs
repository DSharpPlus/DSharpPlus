using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Result of a bulk ban. Contains the ids of users that were successfully banned and the ids of users that failed to be banned.
/// </summary>
public class DiscordBulkBan
{
    /// <summary>
    /// Ids of users that were successfully banned.
    /// </summary>
    [JsonProperty("banned_users")]
    public IEnumerable<ulong> BannedUserIds { get; internal set; }
    
    /// <summary>
    /// Ids of users that failed to be banned (Already banned or not possible to ban).
    /// </summary>
    [JsonProperty("failed_users")]
    public IEnumerable<ulong> FailedUserIds { get; internal set; }
    
    /// <summary>
    /// Users that were successfully banned.
    /// </summary>
    public IEnumerable<DiscordUser> BannedUsers { get; internal set; }
    
    /// <summary>
    /// Users that failed to be banned (Already banned or not possible to ban).
    /// </summary>
    public IEnumerable<DiscordUser> FailedUsers { get; internal set; }
}
