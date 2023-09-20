namespace DSharpPlus.Entities
{
    /// <summary>
    /// Declares the type of a <see cref="DiscordScheduledGuildEvent"/>.
    /// </summary>
    public enum ScheduledGuildEventType
    {
        /// <summary>
        /// The event will be hosted in a stage channel.
        /// </summary>
        StageInstance = 1,
        /// <summary>
        /// The event will be hosted in a voice channel.
        /// </summary>
        VoiceChannel = 2,

        /// <summary>
        /// The event will be hosted in a custom location.
        /// </summary>
        External = 3
    }
}
