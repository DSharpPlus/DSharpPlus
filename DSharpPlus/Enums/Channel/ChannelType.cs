namespace DSharpPlus
{
    /// <summary>
    /// Represents a channel's type.
    /// </summary>
    public enum ChannelType : int
    {
        /// <summary>
        /// Indicates that this is a text channel.
        /// </summary>
        Text = 0,

        /// <summary>
        /// Indicates that this is a private channel.
        /// </summary>
        Private = 1,

        /// <summary>
        /// Indicates that this is a voice channel.
        /// </summary>
        Voice = 2,

        /// <summary>
        /// Indicates that this is a group direct message channel.
        /// </summary>
        Group = 3,

        /// <summary>
        /// Indicates that this is a channel category.
        /// </summary>
        Category = 4,

        /// <summary>
        /// Indicates that this is a news channel.
        /// </summary>
        News = 5,

        /// <summary>
        /// Indicates that this is a store channel.
        /// </summary>
        Store = 6,

        /// <summary>
        /// Indicates unknown channel type.
        /// </summary>
        Unknown = int.MaxValue
    }
}
