namespace DSharpPlus;

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
    /// Indicates that this is a thread within a news channel.
    /// </summary>
    NewsThread = 10,

    /// <summary>
    /// Indicates that this is a public thread within a channel.
    /// </summary>
    PublicThread = 11,

    /// <summary>
    /// Indicates that this is a private thread within a channel.
    /// </summary>
    PrivateThread = 12,

    /// <summary>
    /// Indicates that this is a stage channel.
    /// </summary>
    Stage = 13,

    /// <summary>
    /// Indicates that this is a directory channel.
    /// </summary>
    Directory = 14,

    /// <summary>
    /// Indicates that this is a forum channel.
    /// </summary>
    GuildForum = 15,
    
    /// <summary>
    /// Indicates that this can only contain threads. Similar to GUILD_FORUM channels.
    /// </summary>
    GuildMedia = 16,

    /// <summary>
    /// Indicates unknown channel type.
    /// </summary>
    Unknown = int.MaxValue
}
