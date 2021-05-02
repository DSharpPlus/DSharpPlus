using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.GuildBanRemoved"/> event.
    /// </summary>
    public class GuildBanRemoveEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the member that just got unbanned.
        /// </summary>
        public DiscordMember Member { get; internal set; }

        /// <summary>
        /// Gets the guild this member was unbanned in.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        internal GuildBanRemoveEventArgs() : base() { }
    }
}
