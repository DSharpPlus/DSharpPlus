﻿using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.GuildRoleDeleted"/> event.
    /// </summary>
    public class GuildRoleDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild in which the role was deleted.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets the role that was deleted.
        /// </summary>
        public DiscordRole Role { get; internal set; }

        internal GuildRoleDeleteEventArgs() : base() { }
    }
}
