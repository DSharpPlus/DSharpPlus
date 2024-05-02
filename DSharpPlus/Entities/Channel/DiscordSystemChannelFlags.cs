using System;

namespace DSharpPlus.Entities;

public static class SystemChannelFlagsExtension
{
    /// <summary>
    /// Calculates whether these system channel flags contain a specific flag.
    /// </summary>
    /// <param name="baseFlags">The existing flags.</param>
    /// <param name="flag">The flag to search for.</param>
    /// <returns></returns>
    public static bool HasSystemChannelFlag(this DiscordSystemChannelFlags baseFlags, DiscordSystemChannelFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
/// Represents settings for a guild's system channel.
/// </summary>
[Flags]
public enum DiscordSystemChannelFlags
{
    /// <summary>
    /// Member join messages are disabled.
    /// </summary>
    SuppressJoinNotifications = 1 << 0,

    /// <summary>
    /// Server boost messages are disabled.
    /// </summary>
    SuppressPremiumSubscriptions = 1 << 1,

    /// <summary>
    /// Server setup tips are disabled.
    /// </summary>
    SuppressGuildReminderNotifications = 1 << 2,

    /// <summary>
    /// Server join messages suppress the wave sticker button.
    /// </summary>
    SuppressJoinNotificationReplies = 1 << 3
}
