using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.VoiceNext.EventArgs
{
    /// <summary>
    /// Arguments for <see cref="VoiceNextConnection.UserJoined"/>.
    /// </summary>
    public sealed class VoiceUserJoinEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the user that left voice chat.
        /// </summary>
        public DiscordUser User { get; internal set; }

        internal VoiceUserJoinEventArgs(DiscordClient discord) : base(discord) { }
    }
}
