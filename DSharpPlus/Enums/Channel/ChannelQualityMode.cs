namespace DSharpPlus
{
    /// <summary>
    /// Represents the camera quality of a voice channel.
    /// </summary>
    public enum ChannelQualityMode : int
    {
        /// <summary>
        /// Indicates default video type, or channel is not a voice channel.
        /// </summary>
        DefaultValue = 0,

        /// <summary>
        /// Indicates that the camera quality is automatically chosen, or there is no value set.
        /// </summary>
        Auto = 1,

        /// <summary>
        /// Indicates that the camera quality is 720p.
        /// </summary>
        Full = 2,
    }
}