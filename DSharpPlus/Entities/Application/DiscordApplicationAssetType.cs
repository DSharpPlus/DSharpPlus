namespace DSharpPlus.Entities;


/// <summary>
/// Determines the type of the asset attached to the application.
/// </summary>
public enum DiscordApplicationAssetType
{
    /// <summary>
    /// Unknown type. This indicates something went terribly wrong.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// This asset can be used as small image for rich presences.
    /// </summary>
    SmallImage = 1,

    /// <summary>
    /// This asset can be used as large image for rich presences.
    /// </summary>
    LargeImage = 2
}
