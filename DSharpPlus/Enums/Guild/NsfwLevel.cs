namespace DSharpPlus;

/// <summary>
/// Represents a server's content level.
/// </summary>
public enum NsfwLevel
{
    //I'm going off a hunch; default = safe(?) who knows. //
    /// <summary>
    /// Indicates a server's nsfw level is the default.
    /// </summary>
    Default = 0,
    /// <summary>
    /// Indicates a server's content contains explicit material.
    /// </summary>
    Explicit = 1,
    /// <summary>
    /// Indicates a server's content is safe for work (SFW).
    /// </summary>
    Safe = 2,
    /// <summary>
    /// Indicates a server's content is age-gated.
    /// </summary>
    AgeRestricted = 3
}
