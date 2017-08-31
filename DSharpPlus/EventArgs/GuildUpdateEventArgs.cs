using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.GuildUpdated"/> event.
    /// </summary>
    public class GuildUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild that was updated.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        internal GuildUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
