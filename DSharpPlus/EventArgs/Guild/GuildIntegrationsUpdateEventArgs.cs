using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.GuildIntegrationsUpdated"/> event.
    /// </summary>
    public class GuildIntegrationsUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild that had its integrations updated.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        internal GuildIntegrationsUpdateEventArgs() : base() { }
    }
}
