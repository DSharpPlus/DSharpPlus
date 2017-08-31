using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.VoiceServerUpdated"/> event.
    /// </summary>
    public class VoiceServerUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild for which the update occured.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets the new voice endpoint.
        /// </summary>
        public string Endpoint { get; internal set; }

        /// <summary>
        /// Gets the voice connection token.
        /// </summary>
        internal string VoiceToken { get; set; }

        internal VoiceServerUpdateEventArgs(DiscordClient client) : base(client) { }
    }
}
