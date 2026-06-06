using System;

namespace DSharpPlus;

/// <summary>
/// Represents configuration for <see cref="DiscordClient"/>.
/// </summary>
public sealed class DiscordConfiguration
{

    /// <summary>
    /// Sets whether the client should attempt to cache members if exclusively using unprivileged intents.
    /// <para>
    ///     This will only take effect if there are no <see cref="DiscordIntents.GuildMembers"/> or <see cref="DiscordIntents.GuildPresences"/>
    ///     intents specified. Otherwise, this will always be overwritten to true.
    /// </para>
    /// <para>Defaults to true.</para>
    /// </summary>
    public bool AlwaysCacheMembers { internal get; set; } = true;

    /// <summary>
    /// Sets the default absolute expiration time for cached messages.
    /// </summary>
    public TimeSpan AbsoluteMessageCacheExpiration { internal get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    /// Sets the default sliding expiration time for cached messages. This is refreshed every time the message is
    /// accessed.
    /// </summary>
    public TimeSpan SlidingMessageCacheExpiration { internal get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Whether to log unknown events or not. Defaults to true.
    /// </summary>
    public bool LogUnknownEvents { internal get; set; } = true;

    /// <summary>
    /// Whether to log unknown auditlog types and change keys or not. Defaults to true.
    /// </summary>
    public bool LogUnknownAuditlogs { internal get; set; } = true;
}
