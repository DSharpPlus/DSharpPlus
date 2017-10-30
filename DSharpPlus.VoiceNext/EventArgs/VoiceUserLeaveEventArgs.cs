using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.VoiceNext.EventArgs
{
    /// <summary>
    /// Arguments for <see cref="VoiceNextConnection.UserLeft"/>.
    /// </summary>
    public sealed class VoiceUserLeaveEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the user that left voice chat.
        /// </summary>
        public DiscordUser User { get; internal set; }

        internal VoiceUserLeaveEventArgs(DiscordClient discord) : base(discord) { }
    }
}
