namespace DSharpPlus.Entities;

/// <summary>
/// Represents the state of an interaction regarding responding.
/// </summary>
public enum DiscordDiscordInteractionResponseState
{
    /// <summary>
    /// The interaction has not been acknowledged; a response is required.
    /// </summary>
    Unacknowledged = 0,
    
    /// <summary>
    /// The interaction was deferred; a followup or edit is required.
    /// </summary>
    Deferred = 1,
    
    /// <summary>
    /// The interaction was replied to; no further action is required.
    /// </summary>
    Replied = 2,
}
