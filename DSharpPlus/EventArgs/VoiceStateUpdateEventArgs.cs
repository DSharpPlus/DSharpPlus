using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.VoiceStateUpdated"/> event.
    /// </summary>
    public class VoiceStateUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the user whose voice state was updated.
        /// </summary>
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the guild in which the update occured.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets the related voice channel.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the ID of voice session.
        /// </summary>
        internal string SessionId { get; set; }

        internal VoiceStateUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
