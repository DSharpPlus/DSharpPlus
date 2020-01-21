using Newtonsoft.Json;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.InviteDeleted"/>
    /// </summary>
    public class InviteDeleteEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild that deleted the invite.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets the channel that the invite was for.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the invite that was deleted.
        /// </summary>
        public DiscordInvite Invite { get; internal set; }

        internal InviteDeleteEventArgs(DiscordClient client) : base(client) { }
    }
}
