﻿using System;

namespace DSharpPlus
{
    public static class SystemChannelFlagsExtension
    {
        /// <summary>
        /// Calculates whether these system channel flags contain a specific flag.
        /// </summary>
        /// <param name="baseFlags">The existing flags.</param>
        /// <param name="flag">The flag to search for.</param>
        /// <returns></returns>
        public static bool HasSystemChannelFlag(this SystemChannelFlags baseFlags, SystemChannelFlags flag) => (baseFlags & flag) == flag;
    }

    /// <summary>
    /// Represents settings for a guild's system channel.
    /// </summary>
    [Flags]
    public enum SystemChannelFlags
    {
        /// <summary>
        /// Member join messages are disabled.
        /// </summary>
        SuppressJoinNotifications = 1 << 0,

        /// <summary>
        /// Server boost messages are disabled.
        /// </summary>
        SuppressPremiumSubscriptions = 1 << 1
    }
}
